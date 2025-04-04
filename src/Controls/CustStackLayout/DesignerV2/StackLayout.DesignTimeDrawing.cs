// StackLayout.DesignTimeDrawing.cs
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design; // Required for services
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D; // Required for drawing
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using SharpBrowser.Controls.DesignTime; // <<< Namespace for IDesignTimeDrawer, Drawers, HwndDrawingHelper

// Ensure this namespace matches your StackLayout.cs and StackLayout.Extender.cs
namespace SharpBrowser.Controls
{
    // The other part of StackLayout - focused ONLY on Design-Time behavior
    public partial class StackLayout
    {
        #region Design-Time Constants & Fields

        // --- Configuration ---
        private const int DesignTimeLineRoutingMargin = 15; // Pixels lines route away from controls
        private const int DesignTimeConnectorSize = 10;     // Size of the connector icons
        private const int DesignTimeConnectorOffset = 0;    // Pixels OUTSIDE the control edge (0 = touching)
        private const float DesignTimeLineWidth = 2.5f;     // Width for connection lines and borders
        // --- Derived ---
        private const int DesignTimeHalfConnectorSize = DesignTimeConnectorSize / 2;

        // --- Cached Services (Attempt to get via Site) ---
        private ISelectionService _selectionServiceDT = null;
        private IComponentChangeService _componentChangeServiceDT = null; // Separate reference for design-time use
        private IDesignerHost _designerHostDT = null;
        private ISelectionService _selectionServiceDT_ForEvents = null; // Store the service we subscribed to for SelectionChanged

        // --- HWND Drawing Helper ---
        private HwndDrawingHelper _hwndDrawingHelper = null; // Instance of the helper for HWND drawing mode

        // --- Design-Time Interaction State ---
        private enum DesignDragMode { None, Connecting, Breaking }
        private DesignDragMode _currentDragModeDT = DesignDragMode.None;
        private Control _sourceControlDT = null;        // Control drag initiated FROM
        private PointType _startConnectorTypeDT = PointType.None; // Connector type drag physically started on
        private Point _dragStartPointScreenDT = Point.Empty;
        private Point _dragCurrentPointScreenDT = Point.Empty;
        private Control _breakLinkTargetControlDT = null; // Control whose TARGET arrow was dragged
        private PointType _breakLinkTargetConnectorDT = PointType.None; // The specific TARGET arrow type dragged
        private bool _servicesCheckedDT = false; // Flag to check services only once per instance

        // --- Colors ---
        private Color TargetConnectorConnectedColor = Color.Red;
        private Color SourceConnectorConnectedColor = Color.Blue;

        // --- Local Enum ---
        private enum PointType { None, Top, Bottom, Left, Right }

        #endregion

        #region Service Acquisition & Site Override (Design-Time)

        // Attempts to get necessary services if in DesignMode and Site is available.
        private void EnsureServicesDT()
        {
            // Only check once per Site assignment unless forced
            if (!_servicesCheckedDT && this.DesignMode && this.Site != null)
            {
                LayoutLogger.Log($"StackLayoutDT [{this.Name}]: Ensuring Services...");
                _selectionServiceDT = this.Site.GetService(typeof(ISelectionService)) as ISelectionService;
                _componentChangeServiceDT = this.Site.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                _designerHostDT = this.Site.GetService(typeof(IDesignerHost)) as IDesignerHost;
                _servicesCheckedDT = true; // Mark as checked

                LayoutLogger.Log($"StackLayoutDT [{this.Name}]: - Selection Service: {_selectionServiceDT != null}");
                LayoutLogger.Log($"StackLayoutDT [{this.Name}]: - Change Service:    {_componentChangeServiceDT != null}");
                LayoutLogger.Log($"StackLayoutDT [{this.Name}]: - Designer Host:     {_designerHostDT != null}");
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
        // Handles service acquisition and event subscriptions/unsubscriptions.
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override ISite Site
        {
            get => base.Site;
            set
            {
                // LayoutLogger.Log($"StackLayoutDT [{this.Name}]: Site Property Set. Current Site: {base.Site?.Name}, New Site: {value?.Name}");

                // --- Unsubscribe from OLD Services (if they exist) ---
                if (base.Site != null)
                {
                    // ComponentChangeService (using MAIN partial class field _componentChangeService)
                    var oldComponentSvc = base.Site.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                    if (oldComponentSvc != null && _componentChangeService == oldComponentSvc)
                    {
                        try { _componentChangeService.ComponentChanged -= OnComponentChanged; } catch { } // OnComponentChanged lives in main partial
                        LayoutLogger.Log($"StackLayoutDT [{this.Name}]: Site - Unsubscribed OnComponentChanged from old ComponentChangeService.");
                    }

                    // SelectionService (using DESIGN-TIME field _selectionServiceDT_ForEvents)
                    if (_selectionServiceDT_ForEvents != null) // Use the stored instance for unsubscribing
                    {
                        try { _selectionServiceDT_ForEvents.SelectionChanged -= SelectionService_SelectionChanged; } catch { }
                        LayoutLogger.Log($"StackLayoutDT [{this.Name}]: Site - Unsubscribed SelectionChanged from old SelectionService.");
                        _selectionServiceDT_ForEvents = null; // Clear the stored instance
                    }
                }

                // --- Set the new site (calls base property) ---
                base.Site = value;

                // --- Clear Design-Time Service Cache & Flag ---
                _selectionServiceDT = null; // Clear the general DT reference
                _componentChangeServiceDT = null;
                _designerHostDT = null;
                _servicesCheckedDT = false; // Force re-check on next EnsureServicesDT call

                // --- Subscribe to NEW Services (if site is not null) ---
                _componentChangeService = null; // Clear main reference first
                _selectionServiceDT_ForEvents = null; // Clear event subscription reference

                if (value != null)
                {
                    // ComponentChangeService (using MAIN partial class field _componentChangeService)
                    _componentChangeService = (IComponentChangeService)value.GetService(typeof(IComponentChangeService));
                    if (_componentChangeService != null)
                    {
                        _componentChangeService.ComponentChanged += OnComponentChanged; // Method in main partial
                        LayoutLogger.Log($"StackLayoutDT [{this.Name}]: Site - Subscribed OnComponentChanged to new ComponentChangeService.");
                    }
                    else { LayoutLogger.Log($"StackLayoutDT [{this.Name}]: Site WARNING - Could not get ComponentChangeService from new Site."); }

                    // SelectionService (for SelectionChanged event)
                    var newSelectionSvc = value.GetService(typeof(ISelectionService)) as ISelectionService;
                    if (newSelectionSvc != null)
                    {
                        _selectionServiceDT_ForEvents = newSelectionSvc; // Store the service instance
                        _selectionServiceDT_ForEvents.SelectionChanged += SelectionService_SelectionChanged;
                        LayoutLogger.Log($"StackLayoutDT [{this.Name}]: Site - Subscribed SelectionChanged to new SelectionService.");
                    }
                    else { LayoutLogger.Log($"StackLayoutDT [{this.Name}]: Site WARNING - Could not get SelectionService from new Site."); }
                }
            }
        }

        #endregion

        #region OnPaint Override (Design-Time) - Uses Drawing Method Selector




        // --- Keep the flag to prevent task flood ---
        private static int _screenDrawTaskRunning = 0; // Renamed for clarity

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (this.DesignMode && lay__DesignTimeDrawingMethod == DesignTimeDrawingMethod.HwndExperimental)
            {
                // --- Launch SCREEN Drawing Task (No HWND needed) ---
                // Try to start a new task ONLY if one isn't already marked as running
                if (Interlocked.CompareExchange(ref _screenDrawTaskRunning, 1, 0) == 0)
                {
                    // We successfully set the flag from 0 to 1, launch the task
                    Task.Run(() => DrawTestPatternOnScreen_UserTiming()); // Call screen draw method
                    // LayoutLogger.Log($"StackLayoutDT [{this.Name}]: Launched SCREEN drawing task."); // Can be noisy
                }
                // else: Task already running, skip.
            }
            else if (this.DesignMode && lay__DesignTimeDrawingMethod == DesignTimeDrawingMethod.Direct)
            {
                // --- Execute standard Direct Drawing ---
                IDesignTimeDrawer drawer = new DirectDrawer(e.Graphics);
                try { EnsureServicesDT(); PaintDesignTimeVisuals(drawer); } catch (Exception ex) { /* Log */ try { e.Graphics.DrawString("DT Paint Error", Font, Brushes.Red, 3, 3); } catch { } }
            }
        }


        // --- NEW Background Task Method for Screen Drawing ---
        private void DrawTestPatternOnScreen_UserTiming()
        {
            LayoutLogger.Log($"DrawTestPatternOnScreen_UserTiming: Starting screen drawing task.");
            IntPtr screenDC = IntPtr.Zero; // Device Context handle
            Graphics screenGraphics = null; // Graphics object
            try
            {
                // --- User's Timing Hack ---
                Thread.Sleep(500); // Wait for potential repaints

                // 1. Get the Screen Device Context
                screenDC = NativeMethods.GetDC(IntPtr.Zero); // IntPtr.Zero means the screen
                if (screenDC == IntPtr.Zero)
                {
                    LayoutLogger.Log("DrawTestPatternOnScreen_UserTiming: Failed to get Screen DC.");
                    return; // Exit if failed
                }
                LayoutLogger.Log($"DrawTestPatternOnScreen_UserTiming: Obtained Screen DC: {screenDC}.");

                // 2. Create Graphics object from the Screen DC
                screenGraphics = Graphics.FromHdc(screenDC);
                if (screenGraphics == null)
                {
                    LayoutLogger.Log("DrawTestPatternOnScreen_UserTiming: Failed to create Graphics from Screen DC.");
                    return; // Exit if failed
                }

                // Apply quality settings if desired
                // screenGraphics.SmoothingMode = SmoothingMode.AntiAlias;

                // 3. Perform Drawing using SCREEN coordinates
                LayoutLogger.Log($"DrawTestPatternOnScreen_UserTiming: Drawing pattern on screen...");
                for (int i = 0; i < 1920; i = i + 50) // Your diagonal pattern loop
                {
                    try
                    {
                        screenGraphics.FillRectangle(Brushes.Magenta, i, i, 30, 30); // Use Magenta for distinction
                    }
                    catch (Exception drawEx)
                    {
                        // Log drawing specific errors if they occur mid-loop
                        LayoutLogger.Log($"DrawTestPatternOnScreen_UserTiming: Error during FillRectangle at ({i},{i}): {drawEx.Message}");
                        break; // Exit loop on drawing error
                    }
                }
                LayoutLogger.Log($"DrawTestPatternOnScreen_UserTiming: Finished drawing pattern.");

            }
            catch (Exception ex)
            {
                LayoutLogger.Log($"DrawTestPatternOnScreen_UserTiming: Error: {ex.Message}");
            }
            finally
            {
                // --- IMPORTANT: Clean up ---
                // 4. Dispose the Graphics object
                screenGraphics?.Dispose();

                // 5. Release the Screen Device Context
                if (screenDC != IntPtr.Zero)
                {
                    NativeMethods.ReleaseDC(IntPtr.Zero, screenDC);
                    // LayoutLogger.Log($"DrawTestPatternOnScreen_UserTiming: Released Screen DC: {screenDC}."); // Can be noisy
                }

                // 6. Reset the task running flag
                Interlocked.Exchange(ref _screenDrawTaskRunning, 0);
                LayoutLogger.Log($"DrawTestPatternOnScreen_UserTiming: Task finished. Flag reset.");
            }
        }



        protected void OnPaint_old(PaintEventArgs e)
        {
            // 1. Call base to draw the panel background etc.
            base.OnPaint(e);

            // 2. Execute custom design-time drawing logic ONLY if in DesignMode
            if (this.DesignMode)
            {
                IDesignTimeDrawer drawer = null;
                // Check the property defined in StackLayout.cs
                bool useHwnd = (lay__DesignTimeDrawingMethod == DesignTimeDrawingMethod.HwndExperimental);
                bool canDraw = false;

                // 3. Select and prepare the drawer
                if (useHwnd)
                {
                    if (_hwndDrawingHelper == null) 
                        _hwndDrawingHelper = new HwndDrawingHelper(this);

                    if (_hwndDrawingHelper.BeginDraw()) // Attempt to get Graphics for HWND
                    {
                        drawer = new HwndDrawer(_hwndDrawingHelper);
                        canDraw = true;
                        // LayoutLogger.Log($"StackLayoutDT [{this.Name}]: OnPaint using HWND drawer."); // Noisy
                    }
                    else
                    {
                        LayoutLogger.Log($"StackLayoutDT [{this.Name}]: OnPaint - Could not BeginDraw() for HWND. Adorners skipped.");
                        // Optionally draw error on e.Graphics as fallback feedback   <<- Nice!!. Extremely helpful. for me..
                        try { e.Graphics.DrawString("HWND Draw Failed", Font, Brushes.Red, 3, 3); } catch { }
                    }
                }
                else // Use Direct drawing
                {
                    // Dispose any existing HWND helper if switching away
                    _hwndDrawingHelper?.Dispose();
                    _hwndDrawingHelper = null;

                    drawer = new DirectDrawer(e.Graphics); // Use standard Graphics object
                    canDraw = true;
                    // LayoutLogger.Log($"StackLayoutDT [{this.Name}]: OnPaint using Direct drawer."); // Noisy
                }


                // 4. Execute drawing logic if a drawer was successfully created
                if (canDraw && drawer != null)
                {
                    try
                    {
                        EnsureServicesDT(); // Get designer services if needed
                        PaintDesignTimeVisuals(drawer); // Pass the chosen drawer interface
                    }
                    catch (Exception ex)
                    {
                        LayoutLogger.Log($"StackLayoutDT [{this.Name}]: ERROR during PaintDesignTimeVisuals: {ex.Message}\n{ex.StackTrace}");
                        // Optionally draw error using the DirectDrawer if possible, or log only
                        if (!useHwnd)
                        {
                             try { e.Graphics.DrawString("DT Paint Error", Font, Brushes.Red, 3, 3); } catch { }
                        }
                    }
                    finally
                    {
                        // 5. Clean up HWND drawing if it was used
                        if (useHwnd)
                        {
                            _hwndDrawingHelper?.EndDraw(); // Release HWND Graphics object
                        }
                        // DirectDrawer using e.Graphics doesn't need explicit cleanup here
                    }
                }
            }
            else
            {
                // If not in design mode, ensure HWND helper is disposed
                _hwndDrawingHelper?.Dispose();
                _hwndDrawingHelper = null;
            }
        }

        #endregion

        #region Design-Time Drawing Logic & Helpers (Uses IDesignTimeDrawer)

        // Main painting logic now accepts the drawer interface
        private void PaintDesignTimeVisuals(IDesignTimeDrawer drawer)
        {
            // All drawing settings and logic remain the same,
            // EXCEPT calls to g.Draw... are replaced with drawer.Draw...

            // --- Define Colors ---
            Color stdConnectorBorder = Color.DimGray;
            Color stdConnectorFill = Color.FromArgb(240, 240, 240); // Light gray
            Color targetConnectorBorder = Color.DarkGray;
            Color targetConnectorFill = Color.FromArgb(220, 220, 220); // Slightly darker gray
            Color ConnectionLineColor = Color.DarkSlateBlue;
            // SourceConnectorConnectedColor and TargetConnectorConnectedColor defined as fields

            // --- Get Selection Info ---
            Control selectedChild = null;
            if (_selectionServiceDT != null && _selectionServiceDT.PrimarySelection is Control sc && sc.Parent == this)
            {
                selectedChild = sc;
            }

            // --- 1. Draw Connectors on Currently Selected Child (if any) ---
            if (selectedChild != null)
            {
                bool isBreakingFromSelected = (_currentDragModeDT == DesignDragMode.Breaking && _sourceControlDT == selectedChild);
                if (!isBreakingFromSelected)
                {
                    DrawConnectionPointsForControl(drawer, selectedChild, // Pass drawer
                                                   stdConnectorBorder, stdConnectorFill,
                                                   DesignTimeLineWidth, DesignTimeConnectorOffset);
                }
            }

            // --- 2. Draw Existing Connections (Orthogonal Lines & Specific Icons) ---
            DrawExistingConnections(drawer, ConnectionLineColor, DesignTimeLineWidth, // Pass drawer
                                    SourceConnectorConnectedColor, TargetConnectorConnectedColor,
                                    stdConnectorBorder, stdConnectorFill, // Pass standard colors
                                    DesignTimeConnectorOffset);


            // --- 3. Draw Drag Feedback (if a drag is in progress) ---
            if (_currentDragModeDT != DesignDragMode.None)
            {
                if (_currentDragModeDT == DesignDragMode.Connecting && _sourceControlDT != null)
                {
                    // Draw potential target points on other controls
                    foreach (Control child in this.Controls.OfType<Control>())
                    {
                        if (child != _sourceControlDT && child.Visible)
                        {
                            DrawConnectionPointsForControl(drawer, child, // Pass drawer
                                                           targetConnectorBorder, targetConnectorFill,
                                                           DesignTimeLineWidth, DesignTimeConnectorOffset,
                                                           forceStandard: true); // Force standard circle
                        }
                    }
                    // Draw the direct dashed drag line (Blue)
                    DrawDragLine(drawer, _sourceControlDT, _startConnectorTypeDT, _dragCurrentPointScreenDT, // Pass drawer
                                 Color.Blue, DesignTimeLineWidth, DesignTimeConnectorOffset);
                }
                else if (_currentDragModeDT == DesignDragMode.Breaking && _breakLinkTargetControlDT != null)
                {
                    // Draw the direct dashed drag line (Red)
                    DrawDragLine(drawer, _breakLinkTargetControlDT, _breakLinkTargetConnectorDT, _dragCurrentPointScreenDT, // Pass drawer
                                 Color.Red, DesignTimeLineWidth, DesignTimeConnectorOffset);
                }
            }
        }

        // --- Drawing Helper: Connectors for a single control using IDesignTimeDrawer ---
        private void DrawConnectionPointsForControl(IDesignTimeDrawer drawer, Control c, Color borderColor, Color fillColor, float lineWidth, int outwardOffset, bool forceStandard = false)
        {
            if (c == null || !c.Visible) return;

            var bounds = GetControlBoundsInPanel(c); // Still use panel-relative bounds for calculation
            var connectorRects = GetConnectorRects(bounds, outwardOffset); // Calculate rects relative to panel

            bool isSource = false; PointType sourcePoint = PointType.None;
            bool isTarget = false; PointType targetPoint = PointType.None;
            Control sourceForTargetArrow = null;

            if (!forceStandard)
                GetConnectionState(c, out isSource, out sourcePoint, out isTarget, out targetPoint, out sourceForTargetArrow);

            // Use Pens and Brushes locally - IMPORTANT for GDI object disposal
            using (var borderPen = new Pen(borderColor, lineWidth))
            using (var fillBrush = new SolidBrush(fillColor))
            using (var sourceDotBrush = new SolidBrush(SourceConnectorConnectedColor)) // Use field color
            // Create arrow pen inside target check for correct context
            {
                foreach (var kvp in connectorRects)
                {
                    PointType currentPointType = kvp.Key;
                    Rectangle rect = kvp.Value; // This rect is relative to the StackLayout panel
                    if (rect.IsEmpty) continue;

                    bool drawnSpecial = false;
                    if (!forceStandard)
                    {
                        if (isSource && currentPointType == sourcePoint)
                        {
                            // Draw Source Dot via Drawer
                            drawer.FillEllipse(sourceDotBrush, rect); // Use drawer
                            drawer.DrawEllipse(borderPen, rect);      // Use drawer
                            drawnSpecial = true;
                        }
                        else if (isTarget && currentPointType == targetPoint)
                        {
                            // Draw Target Arrow via Drawer (logic inside this call)
                            // Create specific arrow pen here
                            using (var targetArrowPen = new Pen(TargetConnectorConnectedColor, lineWidth * 1.2f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                            {
                                DrawTargetConnectedIcon(drawer, rect, targetArrowPen, currentPointType); // Pass drawer
                            }
                            drawnSpecial = true;
                        }
                    }

                    if (!drawnSpecial) // Draw standard empty circle
                    {
                        drawer.FillEllipse(fillBrush, rect); // Use drawer
                        drawer.DrawEllipse(borderPen, rect);   // Use drawer
                    }
                }
            } // Dispose pens/brushes
        }

        // --- Drawing Helper: Dot for connected source ---
        // (This helper isn't strictly needed if logic is in DrawConnectionPointsForControl, but kept for clarity)
        // It would need to accept IDesignTimeDrawer if called directly.
        private void DrawSourceConnectedIcon(IDesignTimeDrawer drawer, Rectangle rect, Color borderColor, Color dotColor, float lineWidth)
        {
            if (rect.IsEmpty) return;
            using (var borderPen = new Pen(borderColor, lineWidth))
            using (var dotBrush = new SolidBrush(dotColor))
            {
                drawer.FillEllipse(dotBrush, rect); // Fill colored circle via drawer
                drawer.DrawEllipse(borderPen, rect); // Draw border via drawer
            }
        }


        // --- Drawing Helper: Arrow ONLY via Drawer ---
        private void DrawTargetConnectedIcon(IDesignTimeDrawer drawer, Rectangle rectSource, Pen arrowPen, PointType direction)
        {
            if (rectSource.IsEmpty) return;

            Point center = GetCenter(rectSource); // Calculate center relative to StackLayout

            // Arrow geometry calculation
            int arrowLength = (int)(DesignTimeConnectorSize * 0.70f);
            int arrowHalfWidth = (int)(DesignTimeConnectorSize * 0.30f);
            Point pTip, pWingLeft, pWingRight;

            switch (direction) // Calculate points relative to StackLayout
            {
                case PointType.Top: pTip = new Point(center.X, center.Y + arrowLength / 2); pWingLeft = new Point(center.X - arrowHalfWidth, center.Y - arrowLength / 2); pWingRight = new Point(center.X + arrowHalfWidth, center.Y - arrowLength / 2); break;
                case PointType.Bottom: pTip = new Point(center.X, center.Y - arrowLength / 2); pWingLeft = new Point(center.X - arrowHalfWidth, center.Y + arrowLength / 2); pWingRight = new Point(center.X + arrowHalfWidth, center.Y + arrowLength / 2); break;
                case PointType.Left: pTip = new Point(center.X + arrowLength / 2, center.Y); pWingLeft = new Point(center.X - arrowLength / 2, center.Y - arrowHalfWidth); pWingRight = new Point(center.X - arrowLength / 2, center.Y + arrowHalfWidth); break;
                case PointType.Right: default: pTip = new Point(center.X - arrowLength / 2, center.Y); pWingLeft = new Point(center.X + arrowLength / 2, center.Y - arrowHalfWidth); pWingRight = new Point(center.X + arrowLength / 2, center.Y + arrowHalfWidth); break;
            }

            // Draw the arrow lines using the passed drawer and arrowPen
            drawer.DrawLine(arrowPen, pWingLeft, pTip);
            drawer.DrawLine(arrowPen, pWingRight, pTip);
        }


        // --- Drawing Helper: Existing connections via Drawer (with overlap detection) ---
        private void DrawExistingConnections(IDesignTimeDrawer drawer, Color lineColor, float lineWidth, Color sourceDotColor, Color targetArrowColor, Color defaultBorder, Color defaultFill, int outwardOffset)
        {
            if (_designerHostDT == null || _designerHostDT.Container == null) return;

            int linesDrawn = 0;

            // Create GDI objects locally and dispose them
            using (Pen linePen = new Pen(lineColor, lineWidth) { StartCap = LineCap.RoundAnchor, EndCap = LineCap.RoundAnchor, LineJoin = LineJoin.Round })
            using (Pen borderPen = new Pen(defaultBorder, lineWidth)) // Pen for icon borders
            using (SolidBrush sourceDotBrush = new SolidBrush(sourceDotColor))
            {
                foreach (Control source in this.Controls.OfType<Control>().Where(c => c.Visible))
                {
                    var sourceProps = this.GetPropertiesOrDefault(source);
                    if (sourceProps.IsFloating && !string.IsNullOrEmpty(sourceProps.FloatTargetName))
                    {
                        Control target = null;
                        try { target = _designerHostDT.Container.Components[sourceProps.FloatTargetName] as Control; } catch { target = null; }

                        if (target != null && target.Parent == this && target.Visible)
                        {
                            var sourceBounds = GetControlBoundsInPanel(source);
                            var targetBounds = GetControlBoundsInPanel(target);

                            // Initial Point Type Determination
                            PointType initialTargetPointType = MapAlignmentToConnector(sourceProps.FloatAlignment);
                            PointType initialSourcePointType = GetOppositeConnectorType(initialTargetPointType);
                            if (initialSourcePointType == PointType.None) initialSourcePointType = GetClosestConnectionPointType(sourceBounds, GetCenter(targetBounds));
                            if (initialTargetPointType == PointType.None) initialTargetPointType = GetClosestConnectionPointType(targetBounds, GetCenter(sourceBounds));

                            var sourceRects = GetConnectorRects(sourceBounds, outwardOffset);
                            var targetRects = GetConnectorRects(targetBounds, outwardOffset);

                            // Get initial points
                            Point initialStartPt = Point.Empty, initialEndPt = Point.Empty;
                            if (sourceRects.TryGetValue(initialSourcePointType, out Rectangle initSourceRect)) initialStartPt = GetCenter(initSourceRect);
                            if (targetRects.TryGetValue(initialTargetPointType, out Rectangle initTargetRect)) initialEndPt = GetCenter(initTargetRect);

                            // Overlap Detection & Rerouting
                            PointType finalSourcePointType = initialSourcePointType;
                            PointType finalTargetPointType = initialTargetPointType;
                            bool rerouted = false;

                            if ((initialSourcePointType == PointType.Left && initialTargetPointType == PointType.Right && initialStartPt.X >= initialEndPt.X) ||
                                (initialSourcePointType == PointType.Right && initialTargetPointType == PointType.Left && initialStartPt.X <= initialEndPt.X))
                            { finalSourcePointType = PointType.Top; finalTargetPointType = PointType.Top; rerouted = true; LayoutLogger.Log($"       - Rerouting HORIZONTAL overlap for {source.Name}->{target.Name}. Using TOP connectors."); }
                            else if ((initialSourcePointType == PointType.Top && initialTargetPointType == PointType.Bottom && initialStartPt.Y >= initialEndPt.Y) ||
                                     (initialSourcePointType == PointType.Bottom && initialTargetPointType == PointType.Top && initialStartPt.Y <= initialEndPt.Y))
                            { finalSourcePointType = PointType.Left; finalTargetPointType = PointType.Left; rerouted = true; LayoutLogger.Log($"       - Rerouting VERTICAL overlap for {source.Name}->{target.Name}. Using LEFT connectors."); }

                            // Get Final Connection Points
                            Rectangle finalSourceConnRect, finalTargetConnRect; Point finalStartPt, finalEndPt;
                            if (!sourceRects.TryGetValue(finalSourcePointType, out finalSourceConnRect) || !targetRects.TryGetValue(finalTargetPointType, out finalTargetConnRect) || finalSourceConnRect.IsEmpty || finalTargetConnRect.IsEmpty)
                            { LayoutLogger.Log($"     - XXX FAILED DrawExistingConnections: Cannot get valid FINAL connector Rects for {source.Name}->{target.Name} (Types: {finalSourcePointType}/{finalTargetPointType}). Rerouted={rerouted}"); continue; }

                            finalStartPt = GetCenter(finalSourceConnRect); finalEndPt = GetCenter(finalTargetConnRect);

                            // Calculate Orthogonal Path
                            List<Point> path = CalculateOrthogonalPath(finalStartPt, finalEndPt, finalSourcePointType, finalTargetPointType, DesignTimeLineRoutingMargin);

                            if (path != null && path.Count >= 2)
                            {
                                try
                                {
                                    // Draw Lines via Drawer
                                    drawer.DrawLines(linePen, path.ToArray());
                                    linesDrawn++;

                                    // Draw Icons via Drawer
                                    drawer.FillEllipse(sourceDotBrush, finalSourceConnRect);
                                    drawer.DrawEllipse(borderPen, finalSourceConnRect);

                                    using (var targetArrowPen = new Pen(targetArrowColor, lineWidth * 1.2f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                                    {
                                        DrawTargetConnectedIcon(drawer, finalTargetConnRect, targetArrowPen, finalTargetPointType); // Pass drawer
                                    }
                                }
                                catch (Exception drawEx) { LayoutLogger.Log($"       - XXX ERROR during Drawer DrawLines/Icons: {drawEx.Message} XXX"); }
                            }
                            // else { /* Log path calc failure */ }
                        }
                        // else { /* Log target not found */ }
                    }
                }
            } // Dispose base pens/brushes
        }

        // --- Helper: Calculate Orthogonal Path ---
        // (Implementation remains the same as previous version - calculates points)
        private List<Point> CalculateOrthogonalPath(Point startPt, Point endPt, PointType startType, PointType endType, int margin)
        {
             if (startPt == endPt || startType == PointType.None || endType == PointType.None || margin <= 0)
             {
                 return new List<Point> { startPt, endPt };
             }

             var path = new List<Point>();
             path.Add(startPt); // 1. Start

             Point p1 = startPt; // Outward from source
             Point p2 = endPt;   // Outward from target

             // 2. Calculate p1
             switch (startType) { case PointType.Top: p1.Y -= margin; break; case PointType.Bottom: p1.Y += margin; break; case PointType.Left: p1.X -= margin; break; case PointType.Right: p1.X += margin; break; }
             // 3. Calculate p2
             switch (endType) { case PointType.Top: p2.Y -= margin; break; case PointType.Bottom: p2.Y += margin; break; case PointType.Left: p2.X -= margin; break; case PointType.Right: p2.X += margin; break; }

             path.Add(p1); // 4. Add outward source

             // 5. Calculate intermediate corner points
             bool startIsVertical = (startType == PointType.Top || startType == PointType.Bottom);
             bool endIsVertical = (endType == PointType.Top || endType == PointType.Bottom);

             if (startIsVertical != endIsVertical) // 'L' shape
             { if (startIsVertical) path.Add(new Point(p2.X, p1.Y)); else path.Add(new Point(p1.X, p2.Y)); }
             else // 'U' shape
             { if (startIsVertical) { int midX = (p1.X + p2.X) / 2; path.Add(new Point(midX, p1.Y)); path.Add(new Point(midX, p2.Y)); } else { int midY = (p1.Y + p2.Y) / 2; path.Add(new Point(p1.X, midY)); path.Add(new Point(p2.X, midY)); } }

             path.Add(p2); // 6. Add outward target
             path.Add(endPt); // 7. End

             // 8. Optimize path
             var optimizedPath = new List<Point>();
             if (path.Count > 0) { optimizedPath.Add(path[0]); for (int i = 1; i < path.Count; i++) { if (path[i] != path[i - 1]) optimizedPath.Add(path[i]); } }
             return optimizedPath;
        }


        // Helper: Drag line via Drawer
        private void DrawDragLine(IDesignTimeDrawer drawer, Control startControl, PointType startPointType, Point currentScreenPoint, Color lineColor, float lineWidth, int outwardOffset)
        {
            if (startControl == null || startPointType == PointType.None || startControl.IsDisposed) return;

            var startBounds = GetControlBoundsInPanel(startControl);
            var startRects = GetConnectorRects(startBounds, outwardOffset);

            if (startRects.TryGetValue(startPointType, out Rectangle startConnRect) && !startConnRect.IsEmpty)
            {
                Point startPtSource = GetCenter(startConnRect); // Panel coords for start

                // Convert screen point to client coordinates OF THIS PANEL
                Point endPtSource = this.PointToClient(currentScreenPoint); // <<< FIX: Use 'this' instead of _sourcePanel

                using (Pen tempPen = new Pen(lineColor, lineWidth) { DashStyle = DashStyle.Dash })
                {
                    // Draw Line via Drawer using panel-relative coords
                    drawer.DrawLine(tempPen, startPtSource, endPtSource);
                }
            }
        }

        #endregion

        #region Geometry, Mapping & State Helpers (Design-Time)
        // --- These helpers calculate coordinates/state relative to the StackLayout ---
        // --- They DO NOT perform drawing themselves ---

        // --- Get Connection State ---
        // Determines if 'control' is a source (dot) or target (arrow) at any point
        private void GetConnectionState(Control control, out bool isSourceConnected, out PointType sourcePoint, out bool isTargetConnected, out PointType targetPoint, out Control sourceControlForTarget)
        {
             isSourceConnected = false; sourcePoint = PointType.None;
             isTargetConnected = false; targetPoint = PointType.None;
             sourceControlForTarget = null;
             if (control == null || !this.DesignMode || _designerHostDT == null || _designerHostDT.Container == null) return;

             // Check if 'control' is a SOURCE
             var props = this.GetPropertiesOrDefault(control);
             if (props.IsFloating && !string.IsNullOrEmpty(props.FloatTargetName))
             {
                 Control targetControl = null;
                 try { targetControl = _designerHostDT.Container.Components[props.FloatTargetName] as Control; } catch { }

                 if (targetControl != null && targetControl.Parent == this && targetControl.Visible)
                 {
                     var controlBounds = GetControlBoundsInPanel(control);
                     var targetBounds = GetControlBoundsInPanel(targetControl);
                     PointType targetConnType = MapAlignmentToConnector(props.FloatAlignment);
                     sourcePoint = GetOppositeConnectorType(targetConnType); // Use opposite for source icon placement
                     if (sourcePoint == PointType.None) sourcePoint = GetClosestConnectionPointType(controlBounds, GetCenter(targetBounds));
                     isSourceConnected = true;
                 }
             }

            // Check if 'control' is a TARGET
             if (!string.IsNullOrEmpty(control.Name)) // Check name first
             {
                 foreach (IComponent component in _designerHostDT.Container.Components)
                 {
                     if (component is Control potentialSource && potentialSource.Parent == this && potentialSource != control && potentialSource.Visible)
                     {
                         var sourceProps = this.GetPropertiesOrDefault(potentialSource);
                         if (sourceProps.IsFloating && sourceProps.FloatTargetName == control.Name)
                         {
                             targetPoint = MapAlignmentToConnector(sourceProps.FloatAlignment);
                             isTargetConnected = true;
                             sourceControlForTarget = potentialSource;
                             break; // Found the first connection targeting this control
                         }
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

            int topY = controlBounds.Top - CurrentConnectorSize - outwardOffset;
            int bottomY = controlBounds.Bottom + outwardOffset;
            int leftX = controlBounds.Left - CurrentConnectorSize - outwardOffset;
            int rightX = controlBounds.Right + outwardOffset;

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

        // --- Opposite Connector ---
        private PointType GetOppositeConnectorType(PointType type)
        {
            switch (type) { case PointType.Top: return PointType.Bottom; case PointType.Bottom: return PointType.Top; case PointType.Left: return PointType.Right; case PointType.Right: return PointType.Left; default: return PointType.None; }
        }

        // --- Closest Point Calculation ---
        private PointType GetClosestConnectionPointType(Rectangle sourceBounds, Point targetPoint)
        {
            var rects = GetConnectorRects(sourceBounds, DesignTimeConnectorOffset);
            PointType closest = PointType.None; double minDistSq = double.MaxValue;
            foreach (var kvp in rects) { if (kvp.Value.IsEmpty) continue; Point center = GetCenter(kvp.Value); double dx = center.X - targetPoint.X; double dy = center.Y - targetPoint.Y; double distSq = dx * dx + dy * dy; if (distSq < minDistSq) { minDistSq = distSq; closest = kvp.Key; } }
            return closest;
        }

        #endregion

        #region Design-Time Mouse Event Overrides & Selection Change

        protected override void OnMouseDown(MouseEventArgs e)
        {
            bool designTimeHandled = false;
            if (this.DesignMode && e.Button == MouseButtons.Left)
            {
                EnsureServicesDT();
                Point panelPoint = e.Location;
                Point screenPoint = this.PointToScreen(panelPoint);

                ResetDragStateDT();

                // --- Check for Starting a BREAK ---
                if (HitTestTargetArrow(panelPoint, out Control sourceControlForBreak, out Control targetControlHit, out PointType targetPointHit))
                {
                    LayoutLogger.Log($"StackLayoutDT [{this.Name}]: MouseDown - Hit BREAK arrow on '{targetControlHit.Name}' (Source='{sourceControlForBreak.Name}', Point={targetPointHit})");
                    _currentDragModeDT = DesignDragMode.Breaking;
                    _sourceControlDT = sourceControlForBreak;
                    _breakLinkTargetControlDT = targetControlHit;
                    _breakLinkTargetConnectorDT = targetPointHit;
                    _startConnectorTypeDT = targetPointHit;
                    _dragStartPointScreenDT = screenPoint;
                    _dragCurrentPointScreenDT = screenPoint;
                    this.Capture = true;
                    this.Invalidate(true); // Trigger repaint for feedback
                    designTimeHandled = true;
                }
                // --- Check for Starting a CONNECT ---
                else if (_selectionServiceDT != null && _selectionServiceDT.PrimarySelection is Control selectedControl && selectedControl.Parent == this)
                {
                    if (HitTestSourceConnector(panelPoint, selectedControl, out PointType sourcePointHit))
                    {
                        LayoutLogger.Log($"StackLayoutDT [{this.Name}]: MouseDown - Hit CONNECT point on '{selectedControl.Name}' (Point={sourcePointHit})");
                        _currentDragModeDT = DesignDragMode.Connecting;
                        _sourceControlDT = selectedControl;
                        _startConnectorTypeDT = sourcePointHit;
                        _dragStartPointScreenDT = screenPoint;
                        _dragCurrentPointScreenDT = screenPoint;
                        this.Capture = true;
                        this.Invalidate(true);
                        designTimeHandled = true;
                    }
                }

                if (!designTimeHandled) { /* LayoutLogger.Log($"StackLayoutDT [{this.Name}]: MouseDown - No design-time connector hit."); */ }
            }

            if (!designTimeHandled) { base.OnMouseDown(e); } // Call base for standard selection/move
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            bool designTimeHandled = false;
            if (this.DesignMode && _currentDragModeDT != DesignDragMode.None)
            {
                if (this.Capture)
                {
                    _dragCurrentPointScreenDT = this.PointToScreen(e.Location);
                    this.Invalidate(true); // Redraw drag line etc.
                    designTimeHandled = true;
                }
                else // Lost capture unexpectedly
                {
                    LayoutLogger.Log($"StackLayoutDT [{this.Name}]: MouseMove - Capture lost during drag. Resetting state.");
                    ResetDragStateDT();
                    this.Invalidate(true);
                    designTimeHandled = true;
                }
            }

            if (!designTimeHandled) { base.OnMouseMove(e); }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            bool designTimeHandled = false;
            if (this.DesignMode && _currentDragModeDT != DesignDragMode.None)
            {
                if (this.Capture) this.Capture = false; // Always release capture

                if (e.Button == MouseButtons.Left)
                {
                    Point panelPoint = e.Location;
                    HitTestConnector(panelPoint, out Control droppedOnControl, out PointType droppedOnPoint);

                    if (_currentDragModeDT == DesignDragMode.Connecting)
                    {
                        if (droppedOnControl != null && droppedOnControl != _sourceControlDT && droppedOnPoint != PointType.None)
                        {
                            LayoutLogger.Log($"StackLayoutDT [{this.Name}]: MouseUp - CONNECT drop on '{droppedOnControl.Name}', Point={droppedOnPoint}. Applying...");
                            ApplyConnectionDT(_sourceControlDT, droppedOnControl, droppedOnPoint);
                        }
                        else { LayoutLogger.Log($"StackLayoutDT [{this.Name}]: MouseUp - CONNECT drop missed a valid target."); }
                    }
                    else if (_currentDragModeDT == DesignDragMode.Breaking)
                    {
                        if (droppedOnControl == null) // Dropped off any connector
                        {
                            LayoutLogger.Log($"StackLayoutDT [{this.Name}]: MouseUp - BREAK drop missed connectors. Breaking connection...");
                            BreakConnectionDT(_sourceControlDT);
                        }
                        else { LayoutLogger.Log($"StackLayoutDT [{this.Name}]: MouseUp - BREAK drop hit connector on '{droppedOnControl.Name}'. No change made."); }
                    }
                }
                // else { LayoutLogger.Log($"StackLayoutDT [{this.Name}]: MouseUp - Ignored (button was not Left). Drag state reset."); }

                // Reset state and redraw AFTER processing
                ResetDragStateDT();
                this.Invalidate(true);
                designTimeHandled = true;
            }

            if (!designTimeHandled) { base.OnMouseUp(e); }
        }


        /// <summary>
        /// Handles the SelectionChanged event from the designer's ISelectionService.
        /// Forces a repaint of the StackLayout to clear potential artifacts (like old connectors).
        /// </summary>
        private void SelectionService_SelectionChanged(object sender, EventArgs e)
        {
            // LayoutLogger.Log($"StackLayoutDT [{this.Name}]: SelectionService_SelectionChanged Fired. Invalidating."); // Noisy

            // Check if handle created and not disposed before invalidating
            if (this.IsHandleCreated && !this.IsDisposed && !this.Disposing && this.DesignMode)
            {
                try
                {
                    // Invalidate the entire control surface to ensure artifacts are cleared
                    this.Invalidate(true);
                }
                catch (Exception ex)
                {
                     LayoutLogger.Log($"StackLayoutDT [{this.Name}]: ERROR during Invalidate in SelectionService_SelectionChanged: {ex.Message}");
                }
            }
            // else { LayoutLogger.Log($"StackLayoutDT [{this.Name}]: SelectionService_SelectionChanged - Skipped Invalidate (Handle not created or disposed)."); }
        }


        #endregion

        #region Design-Time Hit Testing Helpers

        // Checks if point is on *any* connector of *any* visible child
        private bool HitTestConnector(Point panelPoint, out Control hitControl, out PointType hitPointType)
        {
            hitControl = null;
            hitPointType = PointType.None;
            // Iterate in reverse Z-order (topmost first)
            foreach (Control child in this.Controls.OfType<Control>().Reverse().Where(c => c.Visible))
            {
                var bounds = GetControlBoundsInPanel(child);
                var rects = GetConnectorRects(bounds, DesignTimeConnectorOffset);
                foreach (var kvp in rects)
                {
                    if (kvp.Value.Contains(panelPoint))
                    {
                        hitControl = child;
                        hitPointType = kvp.Key;
                        return true; // Found topmost hit
                    }
                }
            }
            return false;
        }

        // Checks specifically if point is on a TARGET arrow icon
        private bool HitTestTargetArrow(Point panelPoint, out Control sourceControl, out Control targetControl, out PointType targetPoint)
        {
            sourceControl = null; targetControl = null; targetPoint = PointType.None;
            if (!this.DesignMode || _designerHostDT == null || _designerHostDT.Container == null) return false;

            // Iterate potential targets
            foreach (Control potentialTarget in this.Controls.OfType<Control>().Reverse().Where(c => c.Visible && !string.IsNullOrEmpty(c.Name)))
            {
                // Is potentialTarget targeted by anyone? Check all components in the container
                foreach (IComponent component in _designerHostDT.Container.Components)
                {
                     if (component is Control potentialSource && potentialSource.Parent == this && potentialSource != potentialTarget && potentialSource.Visible)
                     {
                         var sourceProps = this.GetPropertiesOrDefault(potentialSource);
                         if (sourceProps.IsFloating && sourceProps.FloatTargetName == potentialTarget.Name)
                         {
                             // Found connection: potentialSource -> potentialTarget
                             PointType pointOnTarget = MapAlignmentToConnector(sourceProps.FloatAlignment);
                             if (pointOnTarget != PointType.None) // Ensure valid alignment mapping
                             {
                                 var targetBounds = GetControlBoundsInPanel(potentialTarget);
                                 var targetRects = GetConnectorRects(targetBounds, DesignTimeConnectorOffset);
                                 if (targetRects.TryGetValue(pointOnTarget, out Rectangle arrowRect) && arrowRect.Contains(panelPoint))
                                 {
                                     // Hit the arrow!
                                     sourceControl = potentialSource;
                                     targetControl = potentialTarget;
                                     targetPoint = pointOnTarget;
                                     return true;
                                 }
                             }
                             // Only check the *first* connection targeting this control for hit-test simplicity
                             goto nextPotentialTarget; // Optimization
                         }
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
            var rects = GetConnectorRects(bounds, DesignTimeConnectorOffset);

            foreach (var kvp in rects)
            {
                if (kvp.Value.Contains(panelPoint))
                {
                    // Verify this point isn't ALSO a target arrow for some OTHER control
                    GetConnectionState(selectedControl, out _, out _, out bool isTargetAtThisPoint, out PointType targetPointType, out _);
                    if (isTargetAtThisPoint && kvp.Key == targetPointType)
                    {
                        return false; // Clicked on an arrow icon, let HitTestTargetArrow handle it.
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
            // LayoutLogger.Log($"StackLayoutDT [{this.Name}]: ResetDragStateDT called."); // Noisy
        }

        // Applies property changes for creating a connection (Uses DesignerTransaction)
        private void ApplyConnectionDT(Control source, Control target, PointType targetPointType)
        {
            EnsureServicesDT();
            if (_componentChangeServiceDT == null || _designerHostDT == null)
            {
                LayoutLogger.Log($"StackLayoutDT [{this.Name}]: ERROR - ApplyConnectionDT Cannot proceed: ComponentChangeService or DesignerHost unavailable.");
                this.Invalidate(true); // Still redraw to remove drag line
                return;
            }
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
            // Add others if needed (OffsetX, OffsetY, ZOrder - though maybe not set via basic connect)

            if (isFloatingProp == null || targetNameProp == null || alignmentProp == null)
            {
                LayoutLogger.Log($"StackLayoutDT [{this.Name}]: ERROR: ApplyConnectionDT - Required PropertyDescriptor not found for extender properties.");
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

                // Set NEW values using PropertyDescriptors
                isFloatingProp.SetValue(source, true);
                targetNameProp.SetValue(source, target.Name);
                alignmentProp.SetValue(source, alignment);
                // Optionally reset offsets here if desired upon new connection?
                // TypeDescriptor.GetProperties(source)["lay_FloatOffsetX"]?.SetValue(source, 0);
                // TypeDescriptor.GetProperties(source)["lay_FloatOffsetY"]?.SetValue(source, 0);

                // Announce changes completed
                _componentChangeServiceDT.OnComponentChanged(source, isFloatingProp, oldIsFloating, true);
                _componentChangeServiceDT.OnComponentChanged(source, targetNameProp, oldTargetName, target.Name);
                _componentChangeServiceDT.OnComponentChanged(source, alignmentProp, oldAlignment, alignment);

                transaction?.Commit();
                LayoutLogger.Log($"StackLayoutDT [{this.Name}]: Connection transaction committed.");

                // Trigger layout AFTER properties are committed
                this.PerformLayout(); // Perform layout on this panel instance

            }
            catch (Exception ex)
            {
                LayoutLogger.Log($"StackLayoutDT [{this.Name}]: ERROR Applying Connection Transaction: {ex.Message}\n{ex.StackTrace}");
                try { transaction?.Cancel(); } catch { } // Avoid exception in cancel
                LayoutLogger.Log($"StackLayoutDT [{this.Name}]: Connection transaction cancelled.");
            }
            finally
            {
                // Ensure redraw happens even if transaction failed
                this.Invalidate(true);
            }
        }

        // Applies property changes for breaking a connection (Uses DesignerTransaction)
        private void BreakConnectionDT(Control source)
        {
            EnsureServicesDT();
            if (_componentChangeServiceDT == null || _designerHostDT == null)
            {
                LayoutLogger.Log($"StackLayoutDT [{this.Name}]: ERROR - BreakConnectionDT Cannot proceed: ComponentChangeService or DesignerHost unavailable.");
                this.Invalidate(true);
                return;
            }
            if (source == null) { LayoutLogger.Log($"StackLayoutDT [{this.Name}]: ERROR: BreakConnectionDT source is null."); return; }

            LayoutLogger.Log($"StackLayoutDT [{this.Name}]: Breaking Connection for: Source='{source.Name}'");

            PropertyDescriptor isFloatingProp = TypeDescriptor.GetProperties(source)["lay_IsFloating"];
            PropertyDescriptor targetNameProp = TypeDescriptor.GetProperties(source)["lay_FloatTargetName"];
            PropertyDescriptor alignmentProp = TypeDescriptor.GetProperties(source)["lay_FloatAlignment"];
            // Add others if needed (OffsetX, OffsetY, ZOrder - reset them too?)

            if (isFloatingProp == null || targetNameProp == null || alignmentProp == null)
            {
                LayoutLogger.Log($"StackLayoutDT [{this.Name}]: ERROR: BreakConnectionDT - Required PropertyDescriptor not found.");
                return;
            }

            // Get OLD values to see if change is needed and for ComponentChanged event
            bool currentIsFloating = (bool)GetCurrentValueDT(source, "lay_IsFloating", false);
            string currentTargetName = (string)GetCurrentValueDT(source, "lay_FloatTargetName", "");
            FloatAlignment currentAlignment = (FloatAlignment)GetCurrentValueDT(source, "lay_FloatAlignment", FloatAlignment.TopLeft);
            // int currentOffsetX = (int)GetCurrentValueDT(source, "lay_FloatOffsetX", 0);
            // int currentOffsetY = (int)GetCurrentValueDT(source, "lay_FloatOffsetY", 0);

            if (!currentIsFloating && string.IsNullOrEmpty(currentTargetName))
            {
                LayoutLogger.Log($"StackLayoutDT [{this.Name}]: Connection already broken for '{source.Name}'. No change needed.");
                this.Invalidate(true); // Still invalidate to remove drag line if any
                return;
            }

            DesignerTransaction transaction = null;
            try
            {
                transaction = _designerHostDT.CreateTransaction($"Disconnect {source.Name}");
                LayoutLogger.Log($"StackLayoutDT [{this.Name}]: Started transaction: {transaction.Description}");

                // Announce changes
                _componentChangeServiceDT.OnComponentChanging(source, isFloatingProp);
                _componentChangeServiceDT.OnComponentChanging(source, targetNameProp);
                _componentChangeServiceDT.OnComponentChanging(source, alignmentProp);
                // Announce offset changes if resetting them
                // _componentChangeServiceDT.OnComponentChanging(source, TypeDescriptor.GetProperties(source)["lay_FloatOffsetX"]);
                // _componentChangeServiceDT.OnComponentChanging(source, TypeDescriptor.GetProperties(source)["lay_FloatOffsetY"]);

                // Set new (default) values
                isFloatingProp.SetValue(source, false);
                targetNameProp.SetValue(source, "");
                alignmentProp.SetValue(source, FloatAlignment.TopLeft); // Set to default
                // Optionally reset offsets
                // TypeDescriptor.GetProperties(source)["lay_FloatOffsetX"]?.SetValue(source, 0);
                // TypeDescriptor.GetProperties(source)["lay_FloatOffsetY"]?.SetValue(source, 0);

                // Announce completion
                _componentChangeServiceDT.OnComponentChanged(source, isFloatingProp, currentIsFloating, false);
                _componentChangeServiceDT.OnComponentChanged(source, targetNameProp, currentTargetName, "");
                _componentChangeServiceDT.OnComponentChanged(source, alignmentProp, currentAlignment, FloatAlignment.TopLeft);
                // Announce offset changes if reset
                // _componentChangeServiceDT.OnComponentChanged(source, TypeDescriptor.GetProperties(source)["lay_FloatOffsetX"], currentOffsetX, 0);
                // _componentChangeServiceDT.OnComponentChanged(source, TypeDescriptor.GetProperties(source)["lay_FloatOffsetY"], currentOffsetY, 0);


                transaction?.Commit();
                LayoutLogger.Log($"StackLayoutDT [{this.Name}]: Break connection transaction committed.");

                // Trigger layout AFTER properties are committed
                this.PerformLayout();

            }
            catch (Exception ex)
            {
                LayoutLogger.Log($"StackLayoutDT [{this.Name}]: ERROR Breaking Connection Transaction: {ex.Message}\n{ex.StackTrace}");
                try { transaction?.Cancel(); } catch { }
                LayoutLogger.Log($"StackLayoutDT [{this.Name}]: Break connection transaction cancelled.");
            }
            finally
            {
                this.Invalidate(true); // Ensure redraw
            }
        }

        // Helper to get property value safely using TypeDescriptor
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