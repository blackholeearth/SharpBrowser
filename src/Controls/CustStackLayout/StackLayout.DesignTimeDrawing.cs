// StackLayout.DesignTimeDrawing.cs
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design; // Required for services
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D; // Required for drawing
using System.Linq;
using System.Windows.Forms;

// Ensure this namespace matches your StackLayout.cs and StackLayout.Extender.cs
namespace SharpBrowser.Controls
{
    // The other part of StackLayout - focused ONLY on Design-Time behavior
    public partial class StackLayout
    {
        #region Design-Time Constants & Fields

        // --- Add a constant for routing ---
        private const int DesignTimeLineRoutingMargin = 10; //15 <<< How far (pixels) lines route away from controls

        // --- Configuration ---
        private const int DesignTimeConnectorSize = 10; // Size of the connector circles/icons
        private const int DesignTimeConnectorOffset = 0;  // Pixels OUTSIDE the control edge (0 = touching)
        private const float DesignTimeLineWidth = 2.5f;  // Width for connection lines and borders
        // --- Derived ---
        private const int DesignTimeHalfConnectorSize = DesignTimeConnectorSize / 2;

        // --- Cached Services (Attempt to get via Site) ---
        private ISelectionService _selectionServiceDT = null;
        private IComponentChangeService _componentChangeServiceDT = null; // Separate reference for design-time use
        private IDesignerHost _designerHostDT = null;

        // --- Design-Time Interaction State ---
        private enum DesignDragMode { None, Connecting, Breaking }
        private DesignDragMode _currentDragModeDT = DesignDragMode.None;
        private Control _sourceControlDT = null; // Control drag initiated FROM (connecting or breaking source)
        private PointType _startConnectorTypeDT = PointType.None; // Connector type drag physically started on
        private Point _dragStartPointScreenDT = Point.Empty;
        private Point _dragCurrentPointScreenDT = Point.Empty;
        private Control _breakLinkTargetControlDT = null; // Control whose TARGET arrow was dragged
        private PointType _breakLinkTargetConnectorDT = PointType.None; // The specific TARGET arrow type dragged
        private bool _servicesCheckedDT = false; // Flag to check services only once per instance

        private Color TargetConnectorConnectedColor = Color.Red;
        private Color SourceConnectorConnectedColor = Color.Blue;

        // --- Local Enum ---
        private enum PointType { None, Top, Bottom, Left, Right }

        #endregion

        #region Service Acquisition & Site Override (Design-Time)

        // Attempts to get necessary services if in DesignMode and Site is available.
        private void EnsureServicesDT()
        {
            // Only check once per Site assignment unless forced by setting _servicesCheckedDT = false
            if (!_servicesCheckedDT && this.DesignMode && this.Site != null)
            {
                LayoutLogger.Log($"StackLayoutDT [{this.Name}]: Ensuring Services...");
                _selectionServiceDT = this.Site.GetService(typeof(ISelectionService)) as ISelectionService;
                _componentChangeServiceDT = this.Site.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                _designerHostDT = this.Site.GetService(typeof(IDesignerHost)) as IDesignerHost;
                _servicesCheckedDT = true; // Mark as checked for this Site instance

                LayoutLogger.Log($"StackLayoutDT [{this.Name}]: - Selection Service: {_selectionServiceDT != null}");
                LayoutLogger.Log($"StackLayoutDT [{this.Name}]: - Change Service:    {_componentChangeServiceDT != null}");
                LayoutLogger.Log($"StackLayoutDT [{this.Name}]: - Designer Host:     {_designerHostDT != null}");

                // Note: Subscription to IComponentChangeService should happen here
                // IF the _componentChangeService field in the MAIN partial class isn't used/needed at runtime.
                // If it IS needed at runtime, keep the subscription logic paired with that field.
                // For simplicity here, we assume the MAIN partial class handles the subscription
                // via its own _componentChangeService field when the Site is set there.
                // This _componentChangeServiceDT is just for use within design-time methods.
            }
            else if (!this.DesignMode)
            {
                // Clear cached services if not in design mode anymore
                _selectionServiceDT = null;
                _componentChangeServiceDT = null;
                _designerHostDT = null;
                _servicesCheckedDT = false;
            }
        }

        // --- Site Property Override ---
        // Handles service acquisition attempts when the site changes during design time.
        // Also manages ComponentChangeService subscription for the MAIN partial class instance.
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override ISite Site
        {
            get => base.Site;
            set
            {
                // LayoutLogger.Log($"StackLayoutDT [{this.Name}]: Site Property Set. Current Site: {base.Site?.Name}, New Site: {value?.Name}"); // Can be noisy

                // --- Unsubscribe from OLD ComponentChangeService (using MAIN partial class field) ---
                if (base.Site != null)
                {
                    // Use the field from the MAIN partial class (_componentChangeService)
                    var oldComponentSvc = base.Site.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                    if (oldComponentSvc != null && _componentChangeService == oldComponentSvc) // Ensure it's the one we subscribed to
                    {
                        try { _componentChangeService.ComponentChanged -= OnComponentChanged; } catch { } // OnComponentChanged lives in main partial
                        LayoutLogger.Log($"StackLayoutDT [{this.Name}]: Site - Unsubscribed OnComponentChanged from old service.");
                    }
                }

                // --- Set the new site (calls base property) ---
                base.Site = value;

                // --- Clear Design-Time Service Cache & Flag ---
                _selectionServiceDT = null;
                _componentChangeServiceDT = null; // Clear the DT reference
                _designerHostDT = null;
                _servicesCheckedDT = false; // Force re-check on next EnsureServicesDT call

                // --- Subscribe to NEW ComponentChangeService (using MAIN partial class field) ---
                _componentChangeService = null; // Clear main reference first
                if (value != null)
                {
                    _componentChangeService = (IComponentChangeService)value.GetService(typeof(IComponentChangeService));
                    if (_componentChangeService != null)
                    {
                        _componentChangeService.ComponentChanged += OnComponentChanged; // Method in main partial
                        LayoutLogger.Log($"StackLayoutDT [{this.Name}]: Site - Subscribed OnComponentChanged to new service.");
                    }
                    else { LayoutLogger.Log($"StackLayoutDT [{this.Name}]: Site WARNING - Could not get ComponentChangeService from new Site."); }
                }
            }
        }

        #endregion

        #region OnPaint Override (Design-Time)

        protected override void OnPaint(PaintEventArgs e)
        {
            // Always call base first to draw the panel itself
            base.OnPaint(e);

            // Execute design-time drawing logic ONLY if in DesignMode
            if (this.DesignMode)
            {
                // LayoutLogger.Log($"StackLayoutDT [{this.Name}]: OnPaint executing..."); // Very noisy
                try
                {
                    EnsureServicesDT(); // Attempt to get services if needed
                    PaintDesignTimeVisuals(e.Graphics); // Call the drawing method below
                }
                catch (Exception ex)
                {
                    LayoutLogger.Log($"StackLayoutDT [{this.Name}]: ERROR during PaintDesignTimeVisuals: {ex.Message}\n{ex.StackTrace}");
                    // Optional: Draw an error indicator on the control surface
                    try { e.Graphics.DrawString("DT Paint Error", Font, Brushes.Red, 3, 3); } catch { }
                }
            }
        }

        #endregion

        #region Design-Time Drawing Logic & Helpers

        // Main method called by OnPaint in DesignMode
        private void PaintDesignTimeVisuals__old(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            //// --- Drawing Colors ---
            Color ConnectorBorderColor = Color.Blue;
            Color ConnectorFillColor = Color.White;

            //Color TargetConnectorBorderColor = Color.Gray; 
            //Color TargetConnectorFillColor = Color.LightGray;
            // Potential targets during drag
            Color TargetConnectorBorderColor = Color.DarkGray;
            Color TargetConnectorFillColor = Color.FromArgb(220, 220, 220);

            Color ConnectionLineColor = Color.DarkSlateBlue;

            //Color SourceConnectorConnectedColor = Color.Green;
            //Color TargetConnectorConnectedColor = Color.Red;



            // --- Get Selection ---
            Control selectedChild = null;
            if (_selectionServiceDT != null)
            {
                if (_selectionServiceDT.PrimarySelection is Control sc && sc.Parent == this)
                {
                    selectedChild = sc;
                }
            }
            // else { LayoutLogger.Log("PaintDesignTimeVisuals: Selection Service Unavailable for drawing."); }

            // --- 1. Draw Connectors on Selected Child ---
            if (selectedChild != null)
            {
                bool isBreakingFromSelected = (_currentDragModeDT == DesignDragMode.Breaking && _sourceControlDT == selectedChild);
                if (!isBreakingFromSelected)
                {
                    DrawConnectionPointsForControl(g, selectedChild, ConnectorBorderColor, ConnectorFillColor, DesignTimeLineWidth, DesignTimeConnectorOffset);
                }
            }

            // --- 2. Draw Existing Connections (Lines, Dots, Arrows) ---
            DrawExistingConnections(g, ConnectionLineColor, DesignTimeLineWidth, SourceConnectorConnectedColor, TargetConnectorConnectedColor, ConnectorBorderColor, ConnectorFillColor, DesignTimeConnectorOffset);

            // --- 3. Draw Drag Feedback ---
            if (_currentDragModeDT == DesignDragMode.Connecting && _sourceControlDT != null)
            {
                // Draw potential target points (grayed out circles)
                foreach (Control child in this.Controls.OfType<Control>())
                {
                    if (child != _sourceControlDT && child.Visible)
                    {
                        DrawConnectionPointsForControl(g, child, TargetConnectorBorderColor, TargetConnectorFillColor, DesignTimeLineWidth, DesignTimeConnectorOffset, forceStandard: true);
                    }
                }
                // Draw drag line
                DrawDragLine(g, _sourceControlDT, _startConnectorTypeDT, _dragCurrentPointScreenDT, Color.Blue, DesignTimeLineWidth, DesignTimeConnectorOffset);
            }
            else if (_currentDragModeDT == DesignDragMode.Breaking && _breakLinkTargetControlDT != null)
            {
                // Draw breaking drag line (originates from the arrow being dragged)
                DrawDragLine(g, _breakLinkTargetControlDT, _breakLinkTargetConnectorDT, _dragCurrentPointScreenDT, Color.Red, DesignTimeLineWidth, DesignTimeConnectorOffset);
            }
        }
        // In StackLayout.DesignTimeDrawing.cs

        #region Design-Time Drawing Logic & Helpers

        // Main method called by OnPaint in DesignMode
        private void PaintDesignTimeVisuals(Graphics g)
        {
            // Ensure highest quality rendering for clear visuals
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            // --- Define Colors ---
            // Standard connectors (when not selected or connected)
            Color stdConnectorBorder = Color.DimGray;
            Color stdConnectorFill = Color.FromArgb(240, 240, 240); // Light gray

            // Potential targets shown during a connecting drag
            Color targetConnectorBorder = Color.DarkGray;
            Color targetConnectorFill = Color.FromArgb(220, 220, 220); // Slightly darker gray

            // Connection line and specific connected icons
            Color ConnectionLineColor = Color.DarkSlateBlue;
            //Color SourceConnectorConnectedColor = Color.Blue;   // <<< Your Source Color
            //Color TargetConnectorConnectedColor = Color.Red;     // <<< Your Target Color

            // --- Get Selection Info (if service available) ---
            Control selectedChild = null;
            if (_selectionServiceDT != null)
            {
                if (_selectionServiceDT.PrimarySelection is Control sc && sc.Parent == this)
                {
                    selectedChild = sc;
                    // LayoutLogger.Log($"PaintDesignTimeVisuals: Selected child is '{selectedChild.Name}'."); // Noisy
                }
            }
            // else { LayoutLogger.Log("PaintDesignTimeVisuals: Selection Service Unavailable for drawing."); }


            // --- 1. Draw Connectors on Currently Selected Child (if any) ---
            //    - Draw standard gray connectors unless they are involved in an existing connection.
            //    - Skip if currently breaking a connection *from* this selected control.
            if (selectedChild != null)
            {
                bool isBreakingFromSelected = (_currentDragModeDT == DesignDragMode.Breaking && _sourceControlDT == selectedChild);
                if (!isBreakingFromSelected)
                {
                    // LayoutLogger.Log($"PaintDesignTimeVisuals: Drawing connectors for selected '{selectedChild.Name}'.");
                    DrawConnectionPointsForControl(g, selectedChild,
                                                   stdConnectorBorder, stdConnectorFill, // Base colors
                                                   DesignTimeLineWidth, DesignTimeConnectorOffset);
                }
                // else { LayoutLogger.Log($"PaintDesignTimeVisuals: Skipping connectors for selected '{selectedChild.Name}' (Breaking drag from it)."); }
            }

            // --- 2. Draw Existing Connections (Orthogonal Lines & Specific Icons) ---
            //    - This method handles finding connections, calculating paths, drawing lines,
            //      and drawing the specific Blue Dot and Red Arrow icons on the connected points.
            // LayoutLogger.Log("PaintDesignTimeVisuals: Calling DrawExistingConnections.");
            DrawExistingConnections(g, ConnectionLineColor, DesignTimeLineWidth,
                                    SourceConnectorConnectedColor, TargetConnectorConnectedColor,
                                    stdConnectorBorder, stdConnectorFill, // Pass standard colors for the base circle of icons
                                    DesignTimeConnectorOffset);


            // --- 3. Draw Drag Feedback (if a drag is in progress) ---
            if (_currentDragModeDT != DesignDragMode.None)
            {
                // LayoutLogger.Log($"PaintDesignTimeVisuals: Drawing Drag Feedback for mode {_currentDragModeDT}.");

                if (_currentDragModeDT == DesignDragMode.Connecting && _sourceControlDT != null)
                {
                    // Draw potential target points (grayed out standard circles) on other visible controls
                    foreach (Control child in this.Controls.OfType<Control>())
                    {
                        if (child != _sourceControlDT && child.Visible)
                        {
                            DrawConnectionPointsForControl(g, child,
                                                           targetConnectorBorder, targetConnectorFill, // Use target colors
                                                           DesignTimeLineWidth, DesignTimeConnectorOffset,
                                                           forceStandard: true); // Force standard circle drawing
                        }
                    }
                    // Draw the direct dashed drag line (Blue)
                    DrawDragLine(g, _sourceControlDT, _startConnectorTypeDT, _dragCurrentPointScreenDT,
                                 Color.Blue, DesignTimeLineWidth, DesignTimeConnectorOffset);
                }
                else if (_currentDragModeDT == DesignDragMode.Breaking && _breakLinkTargetControlDT != null)
                {
                    // Draw the direct dashed drag line (Red) starting from the arrow being dragged
                    DrawDragLine(g, _breakLinkTargetControlDT, _breakLinkTargetConnectorDT, _dragCurrentPointScreenDT,
                                 Color.Red, DesignTimeLineWidth, DesignTimeConnectorOffset);
                }
            }
        } // End PaintDesignTimeVisuals

        #endregion


        // --- Drawing Helper: Connectors for a single control ---
        private void DrawConnectionPointsForControl(Graphics g, Control c, Color borderColor, Color fillColor, float lineWidth, int outwardOffset, bool forceStandard = false)
        {
            if (c == null || !c.Visible) return;

            var bounds = GetControlBoundsInPanel(c);
            var connectorRects = GetConnectorRects(bounds, outwardOffset);

            // Get Connection State only if needed (not forced standard)
            bool isSource = false; PointType sourcePoint = PointType.None;
            bool isTarget = false; PointType targetPoint = PointType.None;
            Control sourceForTargetArrow = null; // Not directly needed for drawing, but GetConnectionState finds it

            if (!forceStandard)
                GetConnectionState(c, out isSource, out sourcePoint, out isTarget, out targetPoint, out sourceForTargetArrow);

            using (var borderPen = new Pen(borderColor, lineWidth))
            using (var fillBrush = new SolidBrush(fillColor))
            {
                foreach (var kvp in connectorRects)
                {
                    PointType currentPointType = kvp.Key;
                    Rectangle rect = kvp.Value;
                    if (rect.IsEmpty) continue;

                    bool drawnSpecial = false;
                    if (!forceStandard)
                    {
                        if (isSource && currentPointType == sourcePoint)
                        {
                            DrawSourceConnectedIcon(g, rect, borderColor, SourceConnectorConnectedColor, lineWidth);
                            drawnSpecial = true;
                        }
                        else if (isTarget && currentPointType == targetPoint)
                        {
                            DrawTargetConnectedIcon(g, rect, borderColor, TargetConnectorConnectedColor, lineWidth, currentPointType);
                            drawnSpecial = true;
                        }
                    }

                    if (!drawnSpecial)
                    {
                        g.FillEllipse(fillBrush, rect);
                        g.DrawEllipse(borderPen, rect);
                    }
                }
            }
        }

        private void DrawSourceConnectedIcon(Graphics g, Rectangle rect, Color borderColor, object sourceConnectorConnectedColor, float lineWidth)
        {
            throw new NotImplementedException();
        }

        // --- Drawing Helper: Dot for connected source ---
        private void DrawSourceConnectedIcon(Graphics g, Rectangle rect, Color borderColor, Color dotColor, float lineWidth)
        {
            if (rect.IsEmpty) return;
            using (var borderPen = new Pen(borderColor, lineWidth))
            using (var dotBrush = new SolidBrush(dotColor))
            {
                g.FillEllipse(dotBrush, rect); // Fill colored circle
                g.DrawEllipse(borderPen, rect); // Draw border
            }
        }

        // --- Drawing Helper: Arrow for connected target ---
        private void DrawTargetConnectedIcon_old(Graphics g, Rectangle rect, Color borderColor, Color arrowColor, float lineWidth, PointType direction)
        {
            if (rect.IsEmpty) return;
            using (var borderPen = new Pen(borderColor, lineWidth))
            using (var arrowPen = new Pen(arrowColor, lineWidth))
            {
                g.FillEllipse(Brushes.White, rect); // White background
                g.DrawEllipse(borderPen, rect); // Border

                Point center = GetCenter(rect);
                float arrowSize = DesignTimeConnectorSize * 0.6f;
                int halfSize = DesignTimeHalfConnectorSize;
                Point p1, p2, p3; // Arrow points
                // Calculate arrow points based on direction
                switch (direction)
                {
                    case PointType.Top: p1 = new Point(center.X, center.Y + halfSize / 2); p2 = new Point(p1.X - (int)(arrowSize / 2), p1.Y - (int)arrowSize); p3 = new Point(p1.X + (int)(arrowSize / 2), p1.Y - (int)arrowSize); break;
                    case PointType.Bottom: p1 = new Point(center.X, center.Y - halfSize / 2); p2 = new Point(p1.X - (int)(arrowSize / 2), p1.Y + (int)arrowSize); p3 = new Point(p1.X + (int)(arrowSize / 2), p1.Y + (int)arrowSize); break;
                    case PointType.Left: p1 = new Point(center.X + halfSize / 2, center.Y); p2 = new Point(p1.X - (int)arrowSize, p1.Y - (int)(arrowSize / 2)); p3 = new Point(p1.X - (int)arrowSize, p1.Y + (int)(arrowSize / 2)); break;
                    case PointType.Right: default: p1 = new Point(center.X - halfSize / 2, center.Y); p2 = new Point(p1.X + (int)arrowSize, p1.Y - (int)(arrowSize / 2)); p3 = new Point(p1.X + (int)arrowSize, p1.Y + (int)(arrowSize / 2)); break;
                }
                g.DrawLine(arrowPen, p1, p2);
                g.DrawLine(arrowPen, p1, p3);
            }
        }
        
        // MODIFIED: Increased arrow size significantly (approx 2x visual area)
        private void DrawTargetConnectedIcon_old1(Graphics g, Rectangle rect, Color borderColor, Color arrowColor, float lineWidth, PointType direction)
        {
            if (rect.IsEmpty) return;

            using (var borderPen = new Pen(borderColor, lineWidth)) // Pen for the circle border
            using (var arrowPen = new Pen(arrowColor, lineWidth * 1.2f)) // Slightly thicker pen for a bolder arrow
            {
                arrowPen.StartCap = LineCap.Triangle; // Use Triangle cap for a sharper look
                arrowPen.EndCap = LineCap.Triangle;   // (Or keep LineCap.Round if preferred)

                // Draw the base circle (white fill, specified border)
                g.FillEllipse(Brushes.White, rect); // White background inside circle
                g.DrawEllipse(borderPen, rect);     // Circle border

                Point center = GetCenter(rect);

                // --- Arrow Size Calculation (Increased) ---
                // Make the arrow almost fill the connector rectangle
                float arrowHeadSize = DesignTimeConnectorSize * 0.85f; // Much larger fraction of the connector size
                float arrowHeadAngle = 30f; // Angle of the arrowhead tips (degrees)

                // Calculate angle in radians for Math functions
                double radAngle = arrowHeadAngle * Math.PI / 180.0;
                float halfWidth = (float)(arrowHeadSize * Math.Sin(radAngle / 2.0)); // Half the width at the base of the arrowhead
                float bodyLength = arrowHeadSize * 0.8f; // Length of the main shaft relative to head size

                Point pTip, pBaseLeft, pBaseRight; // Arrow points (tip and base corners)

                // Calculate points based on direction, placing the tip near the edge
                switch (direction)
                {
                    case PointType.Top: // Arrow pointing UP
                        pTip = new Point(center.X, rect.Top + (int)(rect.Height * 0.1)); // Tip near top edge
                        pBaseLeft = new Point(pTip.X - (int)halfWidth, pTip.Y + (int)arrowHeadSize);
                        pBaseRight = new Point(pTip.X + (int)halfWidth, pTip.Y + (int)arrowHeadSize);
                        // Draw body (optional, might look cluttered)
                        // g.DrawLine(arrowPen, pTip, new Point(center.X, pBaseLeft.Y));
                        break;
                    case PointType.Bottom: // Arrow pointing DOWN
                        pTip = new Point(center.X, rect.Bottom - (int)(rect.Height * 0.1)); // Tip near bottom edge
                        pBaseLeft = new Point(pTip.X - (int)halfWidth, pTip.Y - (int)arrowHeadSize);
                        pBaseRight = new Point(pTip.X + (int)halfWidth, pTip.Y - (int)arrowHeadSize);
                        // g.DrawLine(arrowPen, pTip, new Point(center.X, pBaseLeft.Y));
                        break;
                    case PointType.Left: // Arrow pointing LEFT
                        pTip = new Point(rect.Left + (int)(rect.Width * 0.1), center.Y); // Tip near left edge
                        pBaseLeft = new Point(pTip.X + (int)arrowHeadSize, pTip.Y - (int)halfWidth);
                        pBaseRight = new Point(pTip.X + (int)arrowHeadSize, pTip.Y + (int)halfWidth);
                        // g.DrawLine(arrowPen, pTip, new Point(pBaseLeft.X, center.Y));
                        break;
                    case PointType.Right: // Arrow pointing RIGHT
                    default:
                        pTip = new Point(rect.Right - (int)(rect.Width * 0.1), center.Y); // Tip near right edge
                        pBaseLeft = new Point(pTip.X - (int)arrowHeadSize, pTip.Y - (int)halfWidth);
                        pBaseRight = new Point(pTip.X - (int)arrowHeadSize, pTip.Y + (int)halfWidth);
                        // g.DrawLine(arrowPen, pTip, new Point(pBaseLeft.X, center.Y));
                        break;
                }

                // Draw the filled arrowhead polygon for a solid look
                Point[] arrowHeadPoints = { pTip, pBaseLeft, pBaseRight };
                g.FillPolygon(new SolidBrush(arrowColor), arrowHeadPoints);

                // Optionally draw the outline if FillPolygon isn't crisp enough
                // g.DrawPolygon(arrowPen, arrowHeadPoints);
            }
        }

        // --- Drawing Helper: Arrow for connected target ---
        // MODIFIED: Draws ONLY the arrow using Lines, ensuring correct shape. No surrounding circle.
        private void DrawTargetConnectedIcon(Graphics g, Rectangle rect, Color borderColor, Color arrowColor, float lineWidth, PointType direction)
        {
            if (rect.IsEmpty) return; // Add safety check back

            // Use a slightly thicker pen for the arrow for visibility
            using (var arrowPen = new Pen(arrowColor, lineWidth * 1.2f))
            {
                arrowPen.StartCap = LineCap.Round; // Use Round caps for smoother line ends
                arrowPen.EndCap = LineCap.Round;

                // <<< REMOVED: Drawing the base circle >>>

                Point center = GetCenter(rect); // Center of the area where the connector icon *would* be

                // --- Arrow Shape Definition ---
                int arrowLength = (int)(DesignTimeConnectorSize * 0.70f); // Length from base to tip
                int arrowHalfWidth = (int)(DesignTimeConnectorSize * 0.30f); // Half the width of the arrow base

                Point pTip, pWingLeft, pWingRight;

                // Calculate points based on direction, pointing *towards* the center/control
                switch (direction)
                {
                    case PointType.Top: // Arrow pointing DOWN (towards the control from the Top connector)
                        pTip = new Point(center.X, center.Y + arrowLength / 2);
                        pWingLeft = new Point(center.X - arrowHalfWidth, center.Y - arrowLength / 2);
                        pWingRight = new Point(center.X + arrowHalfWidth, center.Y - arrowLength / 2);
                        break;
                    case PointType.Bottom: // Arrow pointing UP (towards the control from the Bottom connector)
                        pTip = new Point(center.X, center.Y - arrowLength / 2);
                        pWingLeft = new Point(center.X - arrowHalfWidth, center.Y + arrowLength / 2);
                        pWingRight = new Point(center.X + arrowHalfWidth, center.Y + arrowLength / 2);
                        break;
                    case PointType.Left: // Arrow pointing RIGHT (towards the control from the Left connector)
                        pTip = new Point(center.X + arrowLength / 2, center.Y);
                        pWingLeft = new Point(center.X - arrowLength / 2, center.Y - arrowHalfWidth);
                        pWingRight = new Point(center.X - arrowLength / 2, center.Y + arrowHalfWidth);
                        break;
                    case PointType.Right: // Arrow pointing LEFT (towards the control from the Right connector)
                    default:
                        pTip = new Point(center.X - arrowLength / 2, center.Y);
                        pWingLeft = new Point(center.X + arrowLength / 2, center.Y - arrowHalfWidth);
                        pWingRight = new Point(center.X + arrowLength / 2, center.Y + arrowHalfWidth);
                        break;
                }

                // Draw the two lines forming the arrowhead
                g.DrawLine(arrowPen, pWingLeft, pTip);
                g.DrawLine(arrowPen, pWingRight, pTip);

            } // using arrowPen
        }
        // --- Drawing Helper: Existing connection lines ---
        private void DrawExistingConnections__old(Graphics g, Color lineColor, float lineWidth, Color sourceDotColor, Color targetArrowColor, Color defaultBorder, Color defaultFill, int outwardOffset)
        {
            // USE LayoutLogger consistently
            // LayoutLogger.Log("DrawExistingConnections executing..."); // Noisy

            if (_designerHostDT == null || _designerHostDT.Container == null)
            {
                // LayoutLogger.Log(" - Skipping DrawExistingConnections: DesignerHost or Container is null.");
                return;
            }

            using (Pen linePen = new Pen(lineColor, lineWidth) { StartCap = LineCap.RoundAnchor, EndCap = LineCap.RoundAnchor })
            {
                foreach (Control source in this.Controls.OfType<Control>().Where(c => c.Visible))
                {
                    var sourceProps = this.GetPropertiesOrDefault(source); // Extender method from other partial
                    if (sourceProps.IsFloating && !string.IsNullOrEmpty(sourceProps.FloatTargetName))
                    {
                        Control target = null;
                        try { target = _designerHostDT.Container.Components[sourceProps.FloatTargetName] as Control; } catch { } // Ignore lookup errors

                        if (target != null && target.Parent == this && target.Visible)
                        {
                            var sourceBounds = GetControlBoundsInPanel(source);
                            var targetBounds = GetControlBoundsInPanel(target);
                            PointType targetPointType = MapAlignmentToConnector(sourceProps.FloatAlignment);
                            PointType sourcePointType = GetClosestConnectionPointType(sourceBounds, targetBounds.Location);
                            var sourceRects = GetConnectorRects(sourceBounds, outwardOffset);
                            var targetRects = GetConnectorRects(targetBounds, outwardOffset);

                            if (sourceRects.TryGetValue(sourcePointType, out Rectangle sourceConnRect) &&
                                targetRects.TryGetValue(targetPointType, out Rectangle targetConnRect) &&
                                !sourceConnRect.IsEmpty && !targetConnRect.IsEmpty)
                            {
                                Point startPt = GetCenter(sourceConnRect);
                                Point endPt = GetCenter(targetConnRect);
                                try
                                {
                                    g.DrawLine(linePen, startPt, endPt);
                                    // LayoutLogger.Log($"       - >>> DrawExistingConnections: g.DrawLine CALLED From={startPt} To={endPt}<<<");

                                    // Re-draw icons on top to ensure visibility over the line
                                    DrawSourceConnectedIcon(g, sourceConnRect, defaultBorder, sourceDotColor, lineWidth);
                                    DrawTargetConnectedIcon(g, targetConnRect, defaultBorder, targetArrowColor, lineWidth, targetPointType);
                                }
                                catch (Exception drawEx) { LayoutLogger.Log($"       - XXX ERROR during DrawExistingConnections g.DrawLine: {drawEx.Message} XXX"); }
                            }
                            // else { LayoutLogger.Log($"     - XXX FAILED DrawExistingConnections: Cannot get valid connector Rects for line drawing between {source.Name} and {target.Name}."); }
                        }
                        // else { LayoutLogger.Log($"     - XXX DrawExistingConnections: Target '{sourceProps.FloatTargetName}' not found/visible/sibling for source '{source.Name}'."); }
                    }
                }
            }
        }

        // --- MODIFIED: DrawExistingConnections ---
        // Now calculates orthogonal path and uses DrawLines
        private void DrawExistingConnections(Graphics g, Color lineColor, float lineWidth, Color sourceDotColor, Color targetArrowColor, Color defaultBorder, Color defaultFill, int outwardOffset)
        {
            LayoutLogger.Log("DrawExistingConnections executing...");

            if (_designerHostDT == null || _designerHostDT.Container == null)
            {
                LayoutLogger.Log(" - Skipping DrawExistingConnections: DesignerHost or Container is null.");
                return;
            }

            int connectionsFound = 0;
            int linesDrawn = 0;

            using (Pen linePen = new Pen(lineColor, lineWidth) { StartCap = LineCap.RoundAnchor, EndCap = LineCap.RoundAnchor })
            {
                foreach (Control source in this.Controls.OfType<Control>().Where(c => c.Visible))
                {
                    var sourceProps = this.GetPropertiesOrDefault(source);
                    if (sourceProps.IsFloating && !string.IsNullOrEmpty(sourceProps.FloatTargetName))
                    {
                        connectionsFound++;
                        Control target = null;
                        try { target = _designerHostDT.Container.Components[sourceProps.FloatTargetName] as Control; } catch { }

                        if (target != null && target.Parent == this && target.Visible)
                        {
                            var sourceBounds = GetControlBoundsInPanel(source);
                            var targetBounds = GetControlBoundsInPanel(target);
                            PointType targetPointType = MapAlignmentToConnector(sourceProps.FloatAlignment);
                            PointType sourcePointType = GetClosestConnectionPointType(sourceBounds, targetBounds.Location);
                            var sourceRects = GetConnectorRects(sourceBounds, outwardOffset);
                            var targetRects = GetConnectorRects(targetBounds, outwardOffset);

                            if (sourceRects.TryGetValue(sourcePointType, out Rectangle sourceConnRect) &&
                                targetRects.TryGetValue(targetPointType, out Rectangle targetConnRect) &&
                                !sourceConnRect.IsEmpty && !targetConnRect.IsEmpty)
                            {
                                Point startPt = GetCenter(sourceConnRect);
                                Point endPt = GetCenter(targetConnRect);

                                // --- Calculate Orthogonal Path ---
                                List<Point> path = CalculateOrthogonalPath(startPt, endPt, sourcePointType, targetPointType, DesignTimeLineRoutingMargin);

                                if (path != null && path.Count >= 2)
                                {
                                    try
                                    {
                                        g.DrawLines(linePen, path.ToArray()); // <<< Use DrawLines
                                        linesDrawn++;
                                        LayoutLogger.Log($"       - >>> DrawExistingConnections: g.DrawLines CALLED with {path.Count} points.");

                                        // Re-draw icons on top to ensure visibility
                                        DrawSourceConnectedIcon(g, sourceConnRect, defaultBorder, sourceDotColor, lineWidth);
                                        DrawTargetConnectedIcon(g, targetConnRect, defaultBorder, targetArrowColor, lineWidth, targetPointType);
                                    }
                                    catch (Exception drawEx) { LayoutLogger.Log($"       - XXX ERROR during g.DrawLines: {drawEx.Message} XXX"); }
                                }
                                else { LayoutLogger.Log($"       - XXX FAILED DrawExistingConnections: Could not calculate path between {source.Name} and {target.Name}."); }
                            }
                            else { LayoutLogger.Log($"     - XXX FAILED DrawExistingConnections: Cannot get valid connector Rects for line drawing between {source.Name} and {target.Name}."); }
                        }
                        // else { LayoutLogger.Log($"     - XXX DrawExistingConnections: Target '{sourceProps.FloatTargetName}' not found/visible/sibling for source '{source.Name}'."); } // Noisy
                    }
                }
                // LayoutLogger.Log($" - Finished DrawExistingConnections. Lines Drawn: {linesDrawn}"); // Noisy
            }
        }
        private List<Point> CalculateOrthogonalPath(Point startPt, Point endPt, PointType startType, PointType endType, int margin)
        {
            if (startPt == endPt) return new List<Point> { startPt, endPt }; // Avoid zero-length

            var path = new List<Point>();
            path.Add(startPt); // Start at the source connector center

            Point p1 = startPt; // First intermediate point (moves outward)
            Point p2 = endPt;   // Second intermediate point (moves outward from target)

            // Calculate p1 based on startType
            switch (startType)
            {
                case PointType.Top: p1.Y -= margin; break;
                case PointType.Bottom: p1.Y += margin; break;
                case PointType.Left: p1.X -= margin; break;
                case PointType.Right: p1.X += margin; break;
            }

            // Calculate p2 based on endType
            switch (endType)
            {
                case PointType.Top: p2.Y -= margin; break;
                case PointType.Bottom: p2.Y += margin; break;
                case PointType.Left: p2.X -= margin; break;
                case PointType.Right: p2.X += margin; break;
            }

            path.Add(p1); // Add the outward point from source

            // Determine routing based on directions
            bool startIsVertical = (startType == PointType.Top || startType == PointType.Bottom);
            bool endIsVertical = (endType == PointType.Top || endType == PointType.Bottom);

            if (startIsVertical && !endIsVertical) // e.g., Bottom -> Left/Right
            {
                path.Add(new Point(p2.X, p1.Y)); // Corner point
            }
            else if (!startIsVertical && endIsVertical) // e.g., Right -> Top/Bottom
            {
                path.Add(new Point(p1.X, p2.Y)); // Corner point
            }
            else if (startIsVertical && endIsVertical) // e.g., Bottom -> Top
            {
                // Need two corners for vertical alignment
                int midX = (p1.X + p2.X) / 2;
                path.Add(new Point(midX, p1.Y));
                path.Add(new Point(midX, p2.Y));
            }
            else // Both horizontal, e.g., Right -> Left
            {
                // Need two corners for horizontal alignment
                int midY = (p1.Y + p2.Y) / 2;
                path.Add(new Point(p1.X, midY));
                path.Add(new Point(p2.X, midY));
            }

            path.Add(p2); // Add the outward point from target
            path.Add(endPt); // End at the target connector center

            // Optional: Optimize path - remove duplicate consecutive points
            // path = path.Distinct().ToList(); // Simple but might remove needed points if segments overlap
            var optimizedPath = new List<Point>();
            if (path.Count > 0)
            {
                optimizedPath.Add(path[0]);
                for (int i = 1; i < path.Count; i++)
                {
                    if (path[i] != path[i - 1]) // Add only if different from previous
                    {
                        optimizedPath.Add(path[i]);
                    }
                }
            }

            // LayoutLogger.Log($"      - Calculated Path: {string.Join(", ", optimizedPath.Select(p => p.ToString()))}");
            return optimizedPath;
        }


        // --- Drawing Helper: Dashed line during drag ---
        private void DrawDragLine(Graphics g, Control startControl, PointType startPointType, Point currentScreenPoint, Color lineColor, float lineWidth, int outwardOffset)
        {
            if (startControl == null || startPointType == PointType.None || startControl.IsDisposed) return;

            var startBounds = GetControlBoundsInPanel(startControl);
            var startRects = GetConnectorRects(startBounds, outwardOffset);

            if (startRects.TryGetValue(startPointType, out Rectangle startConnRect) && !startConnRect.IsEmpty)
            {
                Point startPt = GetCenter(startConnRect); // Panel coords
                Point endPt = this.PointToClient(currentScreenPoint); // Convert screen to panel coords

                using (Pen tempPen = new Pen(lineColor, lineWidth) { DashStyle = DashStyle.Dash })
                {
                    g.DrawLine(tempPen, startPt, endPt);
                }
            }
        }

        #endregion

        #region Geometry, Mapping & State Helpers (Design-Time)

        // --- Get Connection State ---
        // Determines if 'control' is a source (dot) or target (arrow) at any point
        private void GetConnectionState(Control control, out bool isSourceConnected, out PointType sourcePoint, out bool isTargetConnected, out PointType targetPoint, out Control sourceControlForTarget)
        {
            isSourceConnected = false; sourcePoint = PointType.None;
            isTargetConnected = false; targetPoint = PointType.None;
            sourceControlForTarget = null;
            if (control == null || _designerHostDT == null || _designerHostDT.Container == null) return;

            // Check if 'control' is a SOURCE
            var props = this.GetPropertiesOrDefault(control);
            if (props.IsFloating && !string.IsNullOrEmpty(props.FloatTargetName))
            {
                Control targetControl = _designerHostDT.Container.Components[props.FloatTargetName] as Control;
                if (targetControl != null && targetControl.Parent == this && targetControl.Visible)
                {
                    var controlBounds = GetControlBoundsInPanel(control);
                    var targetBounds = GetControlBoundsInPanel(targetControl);
                    sourcePoint = GetClosestConnectionPointType(controlBounds, targetBounds.Location);
                    isSourceConnected = true;
                }
            }

            // Check if 'control' is a TARGET (only if not already determined as source for simplicity)
            // A control could theoretically be both source and target, but we simplify icon display.
            if (!isSourceConnected && !string.IsNullOrEmpty(control.Name))
            {
                foreach (Control potentialSource in this.Controls.OfType<Control>())
                {
                    if (potentialSource == control || !potentialSource.Visible) continue;
                    var sourceProps = this.GetPropertiesOrDefault(potentialSource);
                    if (sourceProps.IsFloating && sourceProps.FloatTargetName == control.Name)
                    {
                        targetPoint = MapAlignmentToConnector(sourceProps.FloatAlignment);
                        isTargetConnected = true;
                        sourceControlForTarget = potentialSource;
                        break; // Found the connection targeting this control
                    }
                }
            }
        }

        // --- Basic Geometry ---
        private Rectangle GetControlBoundsInPanel(Control c) => c?.Bounds ?? Rectangle.Empty;

        // --- Connector Rectangle Calculation ---
        private Dictionary<PointType, Rectangle> GetConnectorRects(Rectangle controlBounds, int outwardOffset)
        {
            var dict = new Dictionary<PointType, Rectangle>();
            if (controlBounds.IsEmpty) return dict;

            int CurrentConnectorSize = DesignTimeConnectorSize;
            int CurrentHalfConnectorSize = DesignTimeHalfConnectorSize;

            int midX = controlBounds.Left + controlBounds.Width / 2;
            int midY = controlBounds.Top + controlBounds.Height / 2;

            // Calculate Top-Left position applying the offset logic
            int topY = controlBounds.Top - CurrentConnectorSize - outwardOffset;
            int bottomY = controlBounds.Bottom + outwardOffset;
            int leftX = controlBounds.Left - CurrentConnectorSize - outwardOffset;
            int rightX = controlBounds.Right + outwardOffset;

            // Calculate center for alignment
            int centerX_TB = midX - CurrentHalfConnectorSize;
            int centerY_LR = midY - CurrentHalfConnectorSize;

            dict[PointType.Top] = new Rectangle(centerX_TB, topY, CurrentConnectorSize, CurrentConnectorSize);
            dict[PointType.Bottom] = new Rectangle(centerX_TB, bottomY, CurrentConnectorSize, CurrentConnectorSize);
            dict[PointType.Left] = new Rectangle(leftX, centerY_LR, CurrentConnectorSize, CurrentConnectorSize);
            dict[PointType.Right] = new Rectangle(rightX, centerY_LR, CurrentConnectorSize, CurrentConnectorSize);

            return dict;
        }

        // --- Center Point ---
        private Point GetCenter(Rectangle rect) => rect.IsEmpty ? Point.Empty : new Point(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2);

        // --- Enum Mappings ---
        private FloatAlignment MapConnectorToAlignment(PointType targetPointType)
        {
            switch (targetPointType) { case PointType.Top: return FloatAlignment.ToTopOf; case PointType.Bottom: return FloatAlignment.ToBottomOf; case PointType.Left: return FloatAlignment.ToLeftOf; case PointType.Right: return FloatAlignment.ToRightOf; default: return FloatAlignment.TopLeft; }
        }
        private PointType MapAlignmentToConnector(FloatAlignment alignment)
        {
            switch (alignment) { case FloatAlignment.ToTopOf: return PointType.Top; case FloatAlignment.ToBottomOf: return PointType.Bottom; case FloatAlignment.ToLeftOf: return PointType.Left; case FloatAlignment.ToRightOf: return PointType.Right; default: return PointType.None; }
        }

        // --- Closest Point Calculation ---
        private PointType GetClosestConnectionPointType(Rectangle sourceBounds, Point targetPoint) // targetPoint in same coords as sourceBounds
        {
            var rects = GetConnectorRects(sourceBounds, DesignTimeConnectorOffset); // Use offset
            PointType closest = PointType.None; double minDistSq = double.MaxValue;
            foreach (var kvp in rects)
            {
                if (kvp.Value.IsEmpty) continue;
                Point center = GetCenter(kvp.Value);
                double dx = center.X - targetPoint.X; double dy = center.Y - targetPoint.Y; double distSq = dx * dx + dy * dy;
                if (distSq < minDistSq) { minDistSq = distSq; closest = kvp.Key; }
            }
            return closest;
        }

        #endregion

        #region Design-Time Mouse Event Overrides

        protected override void OnMouseDown(MouseEventArgs e)
        {
            bool designTimeHandled = false;
            // Only handle left clicks in design mode
            if (this.DesignMode && e.Button == MouseButtons.Left)
            {
                EnsureServicesDT(); // Try to get services if needed
                Point panelPoint = e.Location;
                Point screenPoint = this.PointToScreen(panelPoint);

                ResetDragStateDT(); // Prepare for a potential new drag

                // --- Check for Starting a BREAK ---
                if (HitTestTargetArrow(panelPoint, out Control sourceControlForBreak, out Control targetControlHit, out PointType targetPointHit))
                {
                    LayoutLogger.Log($"StackLayoutDT [{this.Name}]: MouseDown - Hit BREAK arrow on '{targetControlHit.Name}' (Source='{sourceControlForBreak.Name}', Point={targetPointHit})");
                    _currentDragModeDT = DesignDragMode.Breaking;
                    _sourceControlDT = sourceControlForBreak; // The source whose properties will change
                    _breakLinkTargetControlDT = targetControlHit; // The control where the arrow was
                    _breakLinkTargetConnectorDT = targetPointHit; // The specific arrow
                    _startConnectorTypeDT = targetPointHit; // Drag starts physically from the arrow
                    _dragStartPointScreenDT = screenPoint;
                    _dragCurrentPointScreenDT = screenPoint;
                    this.Capture = true; // Capture mouse on the StackLayout panel
                    this.Invalidate(true); // Trigger repaint to show drag feedback
                    designTimeHandled = true;
                }
                // --- Check for Starting a CONNECT ---
                else if (_selectionServiceDT != null && _selectionServiceDT.PrimarySelection is Control selectedControl && selectedControl.Parent == this)
                {
                    // Check if clicking on a connector of the SELECTED control
                    if (HitTestSourceConnector(panelPoint, selectedControl, out PointType sourcePointHit))
                    {
                        LayoutLogger.Log($"StackLayoutDT [{this.Name}]: MouseDown - Hit CONNECT point on '{selectedControl.Name}' (Point={sourcePointHit})");
                        _currentDragModeDT = DesignDragMode.Connecting;
                        _sourceControlDT = selectedControl;
                        _startConnectorTypeDT = sourcePointHit;
                        _dragStartPointScreenDT = screenPoint;
                        _dragCurrentPointScreenDT = screenPoint;
                        this.Capture = true; // Capture mouse on the StackLayout panel
                        this.Invalidate(true); // Trigger repaint
                        designTimeHandled = true;
                    }
                }

                if (!designTimeHandled) { LayoutLogger.Log($"StackLayoutDT [{this.Name}]: MouseDown - No design-time connector hit."); }
            }

            // If we didn't handle it for design-time dragging, call base for standard selection/move
            if (!designTimeHandled)
            {
                base.OnMouseDown(e);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            bool designTimeHandled = false;
            // Only process if in design mode AND a custom drag is active
            if (this.DesignMode && _currentDragModeDT != DesignDragMode.None)
            {
                // Ensure we still have mouse capture (could be lost)
                if (this.Capture)
                {
                    _dragCurrentPointScreenDT = this.PointToScreen(e.Location);
                    this.Invalidate(true); // Redraw drag line etc.
                    designTimeHandled = true;
                }
                else
                {
                    // Lost capture unexpectedly during drag - cancel the operation
                    LayoutLogger.Log($"StackLayoutDT [{this.Name}]: MouseMove - Capture lost during drag. Resetting state.");
                    ResetDragStateDT(); // Reset internal state
                    this.Invalidate(true); // Redraw without drag visuals
                    designTimeHandled = true; // We handled the loss of capture
                }
            }

            // If not handled by our drag logic, call base for standard designer behavior
            if (!designTimeHandled)
            {
                base.OnMouseMove(e);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            bool designTimeHandled = false;
            // Only process if in design mode AND a custom drag was active
            if (this.DesignMode && _currentDragModeDT != DesignDragMode.None)
            {
                // LayoutLogger.Log($"StackLayoutDT [{this.Name}]: MouseUp - Ending drag. Mode={_currentDragModeDT}, Button={e.Button}");
                if (this.Capture) this.Capture = false; // Always release capture on mouse up

                if (e.Button == MouseButtons.Left) // Only process the primary button release
                {
                    Point panelPoint = e.Location;
                    // Find which connector (if any) the mouse was released over
                    HitTestConnector(panelPoint, out Control droppedOnControl, out PointType droppedOnPoint);

                    if (_currentDragModeDT == DesignDragMode.Connecting)
                    {
                        // Check if dropped on a valid target connector
                        if (droppedOnControl != null && droppedOnControl != _sourceControlDT && droppedOnPoint != PointType.None)
                        {
                            LayoutLogger.Log($"StackLayoutDT [{this.Name}]: MouseUp - CONNECT drop on '{droppedOnControl.Name}', Point={droppedOnPoint}. Applying...");
                            ApplyConnectionDT(_sourceControlDT, droppedOnControl, droppedOnPoint); // Attempt property changes
                        }
                        else { LayoutLogger.Log($"StackLayoutDT [{this.Name}]: MouseUp - CONNECT drop missed a valid target."); }
                    }
                    else if (_currentDragModeDT == DesignDragMode.Breaking)
                    {
                        // Check if dropped OFF any connector
                        if (droppedOnControl == null)
                        {
                            LayoutLogger.Log($"StackLayoutDT [{this.Name}]: MouseUp - BREAK drop missed connectors. Breaking connection...");
                            BreakConnectionDT(_sourceControlDT); // Attempt property changes
                        }
                        else { LayoutLogger.Log($"StackLayoutDT [{this.Name}]: MouseUp - BREAK drop hit connector on '{droppedOnControl.Name}'. No change made."); }
                    }
                }
                else { LayoutLogger.Log($"StackLayoutDT [{this.Name}]: MouseUp - Ignored (button was not Left). Drag state reset."); }

                // Reset state and redraw regardless of success/failure after processing
                ResetDragStateDT();
                this.Invalidate(true);
                designTimeHandled = true;
            }

            // If not handled by our drag logic, call base
            if (!designTimeHandled)
            {
                base.OnMouseUp(e);
            }
        }

        #endregion

        #region Design-Time Hit Testing Helpers

        // Checks if point is on *any* connector of *any* visible child
        private bool HitTestConnector(Point panelPoint, out Control hitControl, out PointType hitPointType)
        {
            hitControl = null;
            hitPointType = PointType.None;
            // Iterate in reverse Z-order (topmost first) for correct hit testing if controls overlap
            foreach (Control child in this.Controls.OfType<Control>().Reverse().Where(c => c.Visible))
            {
                var bounds = GetControlBoundsInPanel(child);
                var rects = GetConnectorRects(bounds, DesignTimeConnectorOffset); // Use offset
                foreach (var kvp in rects)
                {
                    if (kvp.Value.Contains(panelPoint))
                    {
                        hitControl = child;
                        hitPointType = kvp.Key;
                        return true; // Found the topmost hit
                    }
                }
            }
            return false;
        }

        // Checks specifically if point is on a TARGET arrow icon
        private bool HitTestTargetArrow(Point panelPoint, out Control sourceControl, out Control targetControl, out PointType targetPoint)
        {
            sourceControl = null; targetControl = null; targetPoint = PointType.None;
            if (_designerHostDT == null || _designerHostDT.Container == null) return false;

            // Find which control (if any) has a connector at this point that IS a target arrow
            foreach (Control potentialTarget in this.Controls.OfType<Control>().Reverse().Where(c => c.Visible && !string.IsNullOrEmpty(c.Name)))
            {
                // Is potentialTarget targeted by anyone?
                foreach (Control potentialSource in this.Controls.OfType<Control>().Where(ps => ps != potentialTarget && ps.Visible))
                {
                    var sourceProps = this.GetPropertiesOrDefault(potentialSource);
                    if (sourceProps.IsFloating && sourceProps.FloatTargetName == potentialTarget.Name)
                    {
                        // Found connection: potentialSource -> potentialTarget
                        PointType pointOnTarget = MapAlignmentToConnector(sourceProps.FloatAlignment);
                        var targetBounds = GetControlBoundsInPanel(potentialTarget);
                        var targetRects = GetConnectorRects(targetBounds, DesignTimeConnectorOffset); // Use offset
                        if (targetRects.TryGetValue(pointOnTarget, out Rectangle arrowRect) && arrowRect.Contains(panelPoint))
                        {
                            // Hit the arrow!
                            sourceControl = potentialSource;
                            targetControl = potentialTarget;
                            targetPoint = pointOnTarget;
                            return true;
                        }
                        // Important: Only check the *first* connection targeting this control for hit-test simplicity
                        goto nextPotentialTarget; // Optimization: Move to the next potential target control
                    }
                }
            nextPotentialTarget:; // Label for the goto jump
            }
            return false; // No arrow hit
        }

        // Checks if point is on a standard source connector of the *currently selected* control
        private bool HitTestSourceConnector(Point panelPoint, Control selectedControl, out PointType sourcePoint)
        {
            sourcePoint = PointType.None;
            if (selectedControl == null || !selectedControl.Visible) return false;

            var bounds = GetControlBoundsInPanel(selectedControl);
            var rects = GetConnectorRects(bounds, DesignTimeConnectorOffset); // Use offset

            foreach (var kvp in rects)
            {
                if (kvp.Value.Contains(panelPoint))
                {
                    // Verify this point isn't ALSO a target arrow for some OTHER control
                    GetConnectionState(selectedControl, out _, out _, out bool isTargetAtThisPoint, out PointType targetPointType, out _);
                    if (isTargetAtThisPoint && kvp.Key == targetPointType)
                    {
                        // Clicked on an arrow icon, even if selected. Let HitTestTargetArrow handle it.
                        return false;
                    }

                    // It's a standard or source-dot connector on the selected control
                    sourcePoint = kvp.Key;
                    return true;
                }
            }
            return false; // No hit on selected control's source connectors
        }

        #endregion

        #region Design-Time State & Property Change Logic

        // Resets the internal state variables used for tracking drags
        private void ResetDragStateDT()
        {
            _currentDragModeDT = DesignDragMode.None;
            _sourceControlDT = null;
            _startConnectorTypeDT = PointType.None;
            _dragStartPointScreenDT = Point.Empty;
            _dragCurrentPointScreenDT = Point.Empty;
            _breakLinkTargetControlDT = null;
            _breakLinkTargetConnectorDT = PointType.None;
            // LayoutLogger.Log("StackLayoutDT [{this.Name}]: ResetDragStateDT called."); // Can be noisy
        }

        // Applies property changes for creating a connection
        private void ApplyConnectionDT(Control source, Control target, PointType targetPointType)
        {
            EnsureServicesDT(); // Ensure we've tried to get services
            // Check if critical services needed for Undo/Redo are available
            if (_componentChangeServiceDT == null || _designerHostDT == null)
            {
                LayoutLogger.Log($"StackLayoutDT [{this.Name}]: ERROR - ApplyConnectionDT Cannot proceed: ComponentChangeService or DesignerHost unavailable. Changes will not be saved or undoable.");
                // Optionally: Show a message to the user via IUIService if available?
                // DisplayError("Cannot complete connection: Design services unavailable.");
                this.Invalidate(true); // Still redraw to remove drag line
                return; // Abort the property change part
            }
            // Check other prerequisites
            if (source == null || target == null || string.IsNullOrEmpty(target.Name))
            {
                LayoutLogger.Log($"StackLayoutDT [{this.Name}]: ERROR: ApplyConnectionDT prerequisites failed (source, target, or target name invalid).");
                return;
            }


            FloatAlignment alignment = MapConnectorToAlignment(targetPointType);
            LayoutLogger.Log($"StackLayoutDT [{this.Name}]: Applying Connection: Source='{source.Name}', Target='{target.Name}', Alignment={alignment}");

            PropertyDescriptor isFloatingProp = TypeDescriptor.GetProperties(source)["lay_IsFloating"];
            PropertyDescriptor targetNameProp = TypeDescriptor.GetProperties(source)["lay_FloatTargetName"];
            PropertyDescriptor alignmentProp = TypeDescriptor.GetProperties(source)["lay_FloatAlignment"];

            if (isFloatingProp == null || targetNameProp == null || alignmentProp == null)
            {
                LayoutLogger.Log($"StackLayoutDT [{this.Name}]: ERROR: ApplyConnectionDT - Required PropertyDescriptor not found.");
                return;
            }

            DesignerTransaction transaction = null;
            try
            {
                transaction = _designerHostDT.CreateTransaction($"Connect {source.Name} to {target.Name}");
                LayoutLogger.Log($"StackLayoutDT [{this.Name}]: Started transaction: {transaction.Description}");

                // Get OLD values for ComponentChanged event
                bool oldIsFloating = (bool)GetCurrentValueDT(source, "lay_IsFloating", false);
                string oldTargetName = (string)GetCurrentValueDT(source, "lay_FloatTargetName", "");
                FloatAlignment oldAlignment = (FloatAlignment)GetCurrentValueDT(source, "lay_FloatAlignment", FloatAlignment.TopLeft);

                // Announce upcoming changes
                _componentChangeServiceDT.OnComponentChanging(source, isFloatingProp);
                _componentChangeServiceDT.OnComponentChanging(source, targetNameProp);
                _componentChangeServiceDT.OnComponentChanging(source, alignmentProp);
                // LayoutLogger.Log($"StackLayoutDT [{this.Name}]: Announced ComponentChanging.");

                // Set NEW values
                isFloatingProp.SetValue(source, true);
                targetNameProp.SetValue(source, target.Name);
                alignmentProp.SetValue(source, alignment);
                // LayoutLogger.Log($"StackLayoutDT [{this.Name}]: Set properties: IsFloating=True, Target='{target.Name}', Alignment={alignment}");

                // Announce changes completed
                _componentChangeServiceDT.OnComponentChanged(source, isFloatingProp, oldIsFloating, true);
                _componentChangeServiceDT.OnComponentChanged(source, targetNameProp, oldTargetName, target.Name);
                _componentChangeServiceDT.OnComponentChanged(source, alignmentProp, oldAlignment, alignment);
                // LayoutLogger.Log($"StackLayoutDT [{this.Name}]: Announced ComponentChanged.");

                transaction?.Commit();
                LayoutLogger.Log($"StackLayoutDT [{this.Name}]: Connection transaction committed.");

                // Trigger layout on this control instance AFTER properties are committed
                this.PerformLayout();

            }
            catch (Exception ex)
            {
                LayoutLogger.Log($"StackLayoutDT [{this.Name}]: ERROR Applying Connection Transaction: {ex.Message}");
                transaction?.Cancel();
                LayoutLogger.Log($"StackLayoutDT [{this.Name}]: Connection transaction cancelled.");
            }
            finally
            {
                // Ensure redraw happens even if transaction failed, to clear drag visuals
                this.Invalidate(true);
            }
        }

        // Applies property changes for breaking a connection
        private void BreakConnectionDT(Control source)
        {
            EnsureServicesDT();
            // Check if critical services needed for Undo/Redo are available
            if (_componentChangeServiceDT == null || _designerHostDT == null)
            {
                LayoutLogger.Log($"StackLayoutDT [{this.Name}]: ERROR - BreakConnectionDT Cannot proceed: ComponentChangeService or DesignerHost unavailable. Changes will not be saved or undoable.");
                this.Invalidate(true);
                return; // Abort
            }
            if (source == null) { LayoutLogger.Log($"StackLayoutDT [{this.Name}]: ERROR: BreakConnectionDT source is null."); return; }

            LayoutLogger.Log($"StackLayoutDT [{this.Name}]: Breaking Connection for: Source='{source.Name}'");

            PropertyDescriptor isFloatingProp = TypeDescriptor.GetProperties(source)["lay_IsFloating"];
            PropertyDescriptor targetNameProp = TypeDescriptor.GetProperties(source)["lay_FloatTargetName"];
            PropertyDescriptor alignmentProp = TypeDescriptor.GetProperties(source)["lay_FloatAlignment"];

            if (isFloatingProp == null || targetNameProp == null || alignmentProp == null)
            {
                LayoutLogger.Log($"StackLayoutDT [{this.Name}]: ERROR: BreakConnectionDT - Required PropertyDescriptor not found.");
                return;
            }

            // Get OLD values to see if change is needed and for ComponentChanged event
            bool currentIsFloating = (bool)GetCurrentValueDT(source, "lay_IsFloating", false);
            string currentTargetName = (string)GetCurrentValueDT(source, "lay_FloatTargetName", "");
            if (!currentIsFloating && string.IsNullOrEmpty(currentTargetName))
            {
                LayoutLogger.Log($"StackLayoutDT [{this.Name}]: Connection already broken for '{source.Name}'. No change needed.");
                return; // Already in the desired state
            }
            FloatAlignment currentAlignment = (FloatAlignment)GetCurrentValueDT(source, "lay_FloatAlignment", FloatAlignment.TopLeft);

            DesignerTransaction transaction = null;
            try
            {
                transaction = _designerHostDT.CreateTransaction($"Disconnect {source.Name}");
                LayoutLogger.Log($"StackLayoutDT [{this.Name}]: Started transaction: {transaction.Description}");

                _componentChangeServiceDT.OnComponentChanging(source, isFloatingProp);
                _componentChangeServiceDT.OnComponentChanging(source, targetNameProp);
                _componentChangeServiceDT.OnComponentChanging(source, alignmentProp);

                isFloatingProp.SetValue(source, false);
                targetNameProp.SetValue(source, "");
                alignmentProp.SetValue(source, FloatAlignment.TopLeft); // Set to default

                _componentChangeServiceDT.OnComponentChanged(source, isFloatingProp, currentIsFloating, false);
                _componentChangeServiceDT.OnComponentChanged(source, targetNameProp, currentTargetName, "");
                _componentChangeServiceDT.OnComponentChanged(source, alignmentProp, currentAlignment, FloatAlignment.TopLeft);

                transaction?.Commit();
                LayoutLogger.Log($"StackLayoutDT [{this.Name}]: Break connection transaction committed.");
                this.PerformLayout();

            }
            catch (Exception ex)
            {
                LayoutLogger.Log($"StackLayoutDT [{this.Name}]: ERROR Breaking Connection Transaction: {ex.Message}");
                transaction?.Cancel();
                LayoutLogger.Log($"StackLayoutDT [{this.Name}]: Break connection transaction cancelled.");
            }
            finally
            {
                this.Invalidate(true);
            }
        }

        // Helper to get property value safely
        private object GetCurrentValueDT(Control ctrl, string propName, object defaultValue)
        {
            try
            {
                if (ctrl == null) return defaultValue;
                PropertyDescriptor prop = TypeDescriptor.GetProperties(ctrl)[propName];
                return prop?.GetValue(ctrl) ?? defaultValue;
            }
            catch (Exception ex)
            {
                // LayoutLogger.Log($"StackLayoutDT [{this.Name}]: GetCurrentValueDT WARN for '{propName}' on '{ctrl?.Name}': {ex.Message}"); // Can be noisy
                return defaultValue;
            }
        }

        #endregion

    } // End partial class StackLayout

   


} // End namespace