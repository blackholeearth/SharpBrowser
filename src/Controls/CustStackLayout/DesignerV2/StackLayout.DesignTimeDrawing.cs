// StackLayout.DesignTimeDrawing.cs
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design; // Required for services
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D; // Required for drawing
using System.Linq;
using System.Runtime.InteropServices; // For GCHandle potentially if needed elsewhere
using System.Text; // For StringBuilder if needed elsewhere
using System.Threading; // For Interlocked and Thread.Sleep
using System.Threading.Tasks; // For Task.Run
using System.Windows.Forms;
using SharpBrowser.Controls.DesignTime; // <<< Namespace for IDesignTimeDrawer, Drawers, HwndDrawingHelper, NativeMethods, Snapshot classes

// Ensure this namespace matches your StackLayout.cs and StackLayout.Extender.cs
namespace SharpBrowser.Controls
{
    // --- Snapshot classes defined here for convenience, or move to separate file ---

    // Holds a snapshot of the state needed for drawing adorners
    // Captured on the UI thread to be passed to the background thread
    internal class DesignTimeStateSnapshot
    {
        public bool IsValid { get; set; } = false; // Was snapshot successful?
        public Point PanelScreenLocation { get; set; } = Point.Empty;

        // Control Data (Panel-Relative Bounds and Necessary Properties)
        public List<ControlSnapshot> VisibleControls { get; } = new List<ControlSnapshot>();

        // Selection/Drag State
        public string SelectedControlName { get; set; } = null;
        public StackLayout.DesignDragMode CurrentDragMode { get; set; } = StackLayout.DesignDragMode.None; // Use fully qualified name
        public string DragSourceControlName { get; set; } = null;
        public StackLayout.PointType DragStartConnectorType { get; set; } = StackLayout.PointType.None; // Use fully qualified name
        public Point DragCurrentScreenPoint { get; set; } = Point.Empty; // Already screen coords
        public string BreakTargetControlName { get; set; } = null;
        public StackLayout.PointType BreakTargetConnectorType { get; set; } = StackLayout.PointType.None; // Use fully qualified name

        // Connection Data (Source Name -> Target Info)
        public Dictionary<string, ConnectionInfo> Connections { get; } = new Dictionary<string, ConnectionInfo>();
    }

    internal class ControlSnapshot
    {
        public string Name { get; set; }
        public Rectangle Bounds { get; set; } // Relative to StackLayout Panel
        public bool IsVisible { get; set; } // Actual visibility

        // Store connection state derived on UI thread
        public bool IsConnectionSource { get; set; }
        public StackLayout.PointType SourceConnector { get; set; } = StackLayout.PointType.None; // Use fully qualified name
        public bool IsConnectionTarget { get; set; }
        public StackLayout.PointType TargetConnector { get; set; } = StackLayout.PointType.None; // Use fully qualified name
        public string TargetControlName { get; set; } // If it's a source
        public FloatAlignment TargetAlignment { get; set; } // If it's a source - FloatAlignment comes from StackLayout's namespace
    }

    internal class ConnectionInfo
    {
        public string TargetName { get; set; }
        public FloatAlignment Alignment { get; set; } // FloatAlignment comes from StackLayout's namespace
        // Add Offsets, ZOrder etc. if needed by drawing logic
    }
    // --- End Snapshot Classes ---


    // The other part of StackLayout - focused ONLY on Design-Time behavior
    public partial class StackLayout
    {
        #region Design-Time Constants & Fields

        // --- Configuration ---
        private const int DesignTimeLineRoutingMargin = 15;
        private const int DesignTimeConnectorSize = 10;
        private const int DesignTimeConnectorOffset = 0 - DesignTimeConnectorSize / 2; // 0 touch outside--  
        private const float DesignTimeLineWidth = 2.5f;
        private const int DesignTimeHalfConnectorSize = DesignTimeConnectorSize / 2;

        // --- Cached Services ---
        private ISelectionService _selectionServiceDT = null;
        private IComponentChangeService _componentChangeServiceDT = null;
        private IDesignerHost _designerHostDT = null;
        private ISelectionService _selectionServiceDT_ForEvents = null;

        // --- HWND Drawing Helper (Only used if FindTargetWindow is called, potentially obsolete now) ---
        private HwndDrawingHelper _hwndDrawingHelper = null;

        // --- Screen Drawing Task Flag ---
        private static int _screenDrawTaskRunning = 0;

        // --- Design-Time Interaction State ---
        // Nested enum definitions need to be accessible or defined outside if used across files easily
        // Or use fully qualified names like StackLayout.DesignDragMode
        internal enum DesignDragMode { None, Connecting, Breaking } // Made internal for snapshot access visibility if needed
        private DesignDragMode _currentDragModeDT = DesignDragMode.None;
        private Control _sourceControlDT = null;
        internal enum PointType { None, Top, Bottom, Left, Right } // Made internal for snapshot access visibility
        private PointType _startConnectorTypeDT = PointType.None;
        private Point _dragStartPointScreenDT = Point.Empty;
        private Point _dragCurrentPointScreenDT = Point.Empty;
        private Control _breakLinkTargetControlDT = null;
        private PointType _breakLinkTargetConnectorDT = PointType.None;
        private bool _servicesCheckedDT = false;

        // --- Colors ---
        private Color TargetConnectorConnectedColor = Color.Red;
        private Color SourceConnectorConnectedColor = Color.Blue;
        private Color ConnectionLineColor;

        // --- Local Enum --- Already defined above internal PointType ---
        // private enum PointType { None, Top, Bottom, Left, Right }

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
                    // Note: _componentChangeService is defined in StackLayout.cs
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
                _componentChangeService = null; // Clear main reference first (defined in StackLayout.cs)
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

        #region OnPaint Override (Design-Time) - Selects Drawing Mode

        Throttler throttle1 = new Throttler();
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (this.DesignMode && lay__DesignTimeDrawingMethod == DesignTimeDrawingMethod.HwndExperimental)
            {
                // --- Launch SCREEN Drawing Task (No HWND needed directly here) ---
                if (Interlocked.CompareExchange(ref _screenDrawTaskRunning, 1, 0) == 0)
                {
                    DesignTimeStateSnapshot snapshot = CaptureDesignTimeState();
                    if (snapshot.IsValid)
                    {

                        LayoutLogger.Log($"Throttling 200ms  ---  Task.Run(() => DrawAdornersOnScreen_FromSnapshot(snapshot)); "); 
                        throttle1.Throttle(200,_ => {

                            Task.Run(() => DrawAdornersOnScreen_FromSnapshot(snapshot)); // Call screen draw method
                        });
                        //Task.Run(() => DrawAdornersOnScreen_FromSnapshot(snapshot)); // Call screen draw method
                        //// LayoutLogger.Log($"StackLayoutDT [{this.Name}]: Launched SCREEN drawing task."); // Noisy
                    }
                    else
                    {
                        LayoutLogger.Log($"StackLayoutDT [{this.Name}]: Failed state capture. Skipping screen draw.");
                        Interlocked.Exchange(ref _screenDrawTaskRunning, 0); // Reset flag if capture failed immediately
                    }
                    // Flag is reset inside the Task's finally block on successful launch
                }
                // else: Task already running, skip.
            }
            else if (this.DesignMode && lay__DesignTimeDrawingMethod == DesignTimeDrawingMethod.Direct)
            {
                // --- Execute standard Direct Drawing ---
                // Dispose HWND helper if mode changed away from HWND
                _hwndDrawingHelper?.Dispose();
                _hwndDrawingHelper = null;

                IDesignTimeDrawer drawer = new DirectDrawer(e.Graphics); // Use standard Graphics
                try
                {
                    EnsureServicesDT();
                    PaintDesignTimeVisuals(drawer); // Call direct drawing logic using the interface
                }
                catch (Exception ex) { LayoutLogger.Log($"StackLayoutDT [{this.Name}]: ERROR Direct Paint: {ex.Message}"); try { e.Graphics.DrawString("DT Paint Error", Font, Brushes.Red, 3, 3); } catch { } }
                // No EndDraw needed for DirectDrawer
            }
            else // Not design mode
            {
                // Ensure helpers/flags related to design-time are cleaned up if necessary
                _hwndDrawingHelper?.Dispose();
                _hwndDrawingHelper = null;
                // Consider if _screenDrawTaskRunning needs reset here if app closes while task runs? Probably not critical.
            }
        }
        #endregion // OnPaint Override

        #region State Capture for Background Thread

        // Run on UI Thread (called from OnPaint in HwndExperimental mode)
        private DesignTimeStateSnapshot CaptureDesignTimeState()
        {
            var snapshot = new DesignTimeStateSnapshot();
            if (!this.IsHandleCreated || this.IsDisposed || !this.DesignMode) return snapshot; // Check DesignMode too

            try
            {
                EnsureServicesDT(); // Ensure _selectionServiceDT, _designerHostDT etc are available

                if (_designerHostDT == null || _selectionServiceDT == null)
                {
                    LayoutLogger.Log("CaptureDesignTimeState WARNING: Designer services not available.");
                    // Decide if snapshot should be invalid or proceed with limited info
                    // return snapshot; // Return invalid snapshot
                }


                snapshot.PanelScreenLocation = this.PointToScreen(Point.Empty);

                // Capture Selection/Drag State (Check for null services)
                if (_selectionServiceDT != null && _selectionServiceDT.PrimarySelection is Control selCtrl && selCtrl.Parent == this)
                { snapshot.SelectedControlName = selCtrl.Name; }
                snapshot.CurrentDragMode = _currentDragModeDT;
                snapshot.DragSourceControlName = _sourceControlDT?.Name;
                snapshot.DragStartConnectorType = _startConnectorTypeDT;
                snapshot.DragCurrentScreenPoint = _dragCurrentPointScreenDT;
                snapshot.BreakTargetControlName = _breakLinkTargetControlDT?.Name;
                snapshot.BreakTargetConnectorType = _breakLinkTargetConnectorDT;

                // Capture Control Data and Connections
                // Need DesignerHost to lookup targets by name if needed during connection analysis
                var componentLookup = _designerHostDT?.Container?.Components; // Check for null host/container

                foreach (Control c in this.Controls.OfType<Control>())
                {
                    // Use GetPropertiesOrDefault defined in StackLayout.Extender.cs
                    StackProperties stackProps = GetPropertiesOrDefault(c);
                    if (!c.Visible && !stackProps.IncludeHiddenInLayout) continue;

                    var controlSnap = new ControlSnapshot { Name = c.Name, Bounds = c.Bounds, IsVisible = c.Visible };

                    // Get connection state (requires UI thread access potentially via GetPropertiesOrDefault)
                    // Pass lookup for safety if GetConnectionState needs it
                    GetConnectionState(c, out bool isSource, out PointType sourceConn, out bool isTarget, out PointType targetConn, out Control srcCtrlForTarget);
                    controlSnap.IsConnectionSource = isSource;
                    controlSnap.SourceConnector = sourceConn;
                    controlSnap.IsConnectionTarget = isTarget;
                    controlSnap.TargetConnector = targetConn;

                    if (isSource)
                    {
                        // var props = GetPropertiesOrDefault(c); // Already got stackProps
                        controlSnap.TargetControlName = stackProps.FloatTargetName;
                        controlSnap.TargetAlignment = stackProps.FloatAlignment;

                        if (!string.IsNullOrEmpty(controlSnap.TargetControlName) && !snapshot.Connections.ContainsKey(controlSnap.Name))
                        {
                            snapshot.Connections.Add(controlSnap.Name, new ConnectionInfo
                            { TargetName = controlSnap.TargetControlName, Alignment = controlSnap.TargetAlignment });
                        }
                    }
                    snapshot.VisibleControls.Add(controlSnap);
                }
                snapshot.IsValid = true; // Mark as successful if no exceptions
            }
            catch (Exception ex)
            { LayoutLogger.Log($"ERROR Capturing DesignTime State: {ex.Message}\n{ex.StackTrace}"); snapshot.IsValid = false; }
            return snapshot;
        }

        #endregion








        #region Background Screen Drawing Task & Helpers (HwndExperimental Mode)


        // Runs on Background Thread - Gets DC/Graphics ONCE and passes it down
        private void DrawAdornersOnScreen_FromSnapshot(DesignTimeStateSnapshot snapshot)
        {
            if (snapshot == null || !snapshot.IsValid) { Interlocked.Exchange(ref _screenDrawTaskRunning, 0); return; }

            LayoutLogger.Log($"DrawAdornersOnScreen: Starting screen drawing task.");
            IntPtr screenDC = IntPtr.Zero;
            Graphics screenGraphics = null; // Single Graphics object for this task run
            try
            {
                Thread.Sleep(200); // Keep timing hack

                // *** GET DC AND GRAPHICS ONCE ***
                screenDC = NativeMethods.GetDC(IntPtr.Zero);
                if (screenDC == IntPtr.Zero) { throw new Exception("Failed Screen DC."); }
                screenGraphics = Graphics.FromHdc(screenDC);
                if (screenGraphics == null) { throw new Exception("Failed Graphics from DC."); }
                screenGraphics.SmoothingMode = SmoothingMode.AntiAlias; // Set quality

                // Colors
                Color stdConnectorBorder = Color.DimGray, stdConnectorFill = Color.FromArgb(240, 240, 240);
                Color targetConnectorBorder = Color.DarkGray, targetConnectorFill = Color.FromArgb(220, 220, 220);
                // Instance colors (capture for safety)
                Color sourceIconColor = this.SourceConnectorConnectedColor;
                Color targetIconColor = this.TargetConnectorConnectedColor;

                // --- Create Pens/Brushes (Need disposal) ---
                using (Pen stdBorderPen = new Pen(stdConnectorBorder, DesignTimeLineWidth))
                using (SolidBrush stdFillBrush = new SolidBrush(stdConnectorFill))
                using (Pen targetBorderPen = new Pen(targetConnectorBorder, DesignTimeLineWidth))
                using (SolidBrush targetFillBrush = new SolidBrush(targetConnectorFill))
                using (Pen sourceBorderPen = new Pen(Color.DimGray, DesignTimeLineWidth)) // Example border
                using (SolidBrush sourceDotBrush = new SolidBrush(sourceIconColor))
                {
                    // --- Pass screenGraphics to helpers ---

                    // 1. Draw Connectors on Selected Child
                    ControlSnapshot selectedControlSnap = snapshot.VisibleControls.FirstOrDefault(c => c.Name == snapshot.SelectedControlName);
                    bool isBreakingFromSelected = (snapshot.CurrentDragMode == DesignDragMode.Breaking && snapshot.DragSourceControlName == snapshot.SelectedControlName);
                    if (selectedControlSnap != null && !isBreakingFromSelected)
                    {
                        // Pass the single screenGraphics object
                        DrawConnectionPointsOnScreen(screenGraphics, snapshot.PanelScreenLocation, selectedControlSnap,
                                                     stdBorderPen, stdFillBrush, sourceBorderPen, sourceDotBrush,
                                                     DesignTimeConnectorOffset, false);
                    }

                    // 2. Draw Existing Connections
                    // Pass the single screenGraphics object
                    DrawExistingConnectionsOnScreen(screenGraphics, snapshot);

                    // 3. Draw Drag Feedback
                    if (snapshot.CurrentDragMode != DesignDragMode.None)
                    {
                        ControlSnapshot dragSourceSnap = snapshot.VisibleControls.FirstOrDefault(c => c.Name == snapshot.DragSourceControlName);
                        ControlSnapshot breakTargetSnap = snapshot.VisibleControls.FirstOrDefault(c => c.Name == snapshot.BreakTargetControlName);

                        if (snapshot.CurrentDragMode == DesignDragMode.Connecting && dragSourceSnap != null)
                        {
                            // Draw potential target points
                            foreach (var controlSnap in snapshot.VisibleControls)
                            {
                                if (controlSnap.Name != snapshot.DragSourceControlName && controlSnap.IsVisible)
                                {
                                    // Pass the single screenGraphics object
                                    DrawConnectionPointsOnScreen(screenGraphics, snapshot.PanelScreenLocation, controlSnap,
                                                                 targetBorderPen, targetFillBrush, null, null,
                                                                 DesignTimeConnectorOffset, true);
                                }
                            }
                            // Draw drag line
                            // Pass the single screenGraphics object
                            DrawDragLineOnScreen(screenGraphics, snapshot.PanelScreenLocation, dragSourceSnap, snapshot.DragStartConnectorType, snapshot.DragCurrentScreenPoint,
                                                 Color.Blue, DesignTimeLineWidth, DesignTimeConnectorOffset);
                        }
                        else if (snapshot.CurrentDragMode == DesignDragMode.Breaking && breakTargetSnap != null)
                        {
                            // Draw drag line
                            // Pass the single screenGraphics object
                            DrawDragLineOnScreen(screenGraphics, snapshot.PanelScreenLocation, breakTargetSnap, snapshot.BreakTargetConnectorType, snapshot.DragCurrentScreenPoint,
                                                 Color.Red, DesignTimeLineWidth, DesignTimeConnectorOffset);
                        }
                    }
                } // End using Pens/Brushes
            }
            catch (Exception ex) { LayoutLogger.Log($"DrawAdornersOnScreen: Error: {ex.Message}\n{ex.StackTrace}"); }
            finally
            {
                // *** RELEASE DC AND GRAPHICS ONCE AT THE END ***
                screenGraphics?.Dispose();
                if (screenDC != IntPtr.Zero) { NativeMethods.ReleaseDC(IntPtr.Zero, screenDC); }
                Interlocked.Exchange(ref _screenDrawTaskRunning, 0);
                LayoutLogger.Log($"DrawAdornersOnScreen: Task finished.");
            }
        }

        // Draws connection points for ONE control onto the screen graphics
        private void DrawConnectionPointsOnScreen(Graphics screenGraphics, Point panelScreenOrigin, ControlSnapshot controlSnap,
                                                 Pen borderPen, Brush fillBrush, Pen sourceBorderPen, Brush sourceDotBrush,
                                                 int outwardOffset, bool forceStandard)
        {
            // Safety check
            if (screenGraphics == null || controlSnap == null) return;

            // Calculate connector rects relative to the PANEL (Same as Direct Mode)
            var connectorRectsPanel = GetConnectorRects(controlSnap.Bounds, outwardOffset);

            foreach (var kvp in connectorRectsPanel)
            {
                PointType currentPointType = kvp.Key;
                Rectangle rectPanel = kvp.Value; // Panel-relative rectangle
                if (rectPanel.IsEmpty) continue;

                // Convert panel-relative rect to SCREEN coordinates for drawing
                Rectangle rectScreen = new Rectangle(
                    panelScreenOrigin.X + rectPanel.Left,
                    panelScreenOrigin.Y + rectPanel.Top,
                    rectPanel.Width,
                    rectPanel.Height);

                bool drawnSpecial = false;
                if (!forceStandard)
                {
                    // Use snapshot data for connection state (Same logic as Direct Mode uses GetConnectionState)
                    if (controlSnap.IsConnectionSource && currentPointType == controlSnap.SourceConnector && sourceDotBrush != null && sourceBorderPen != null)
                    {
                        // Draw Source Dot on Screen using passed Graphics
                        screenGraphics.FillEllipse(sourceDotBrush, rectScreen);
                        screenGraphics.DrawEllipse(sourceBorderPen, rectScreen);
                        drawnSpecial = true;
                    }
                    else if (controlSnap.IsConnectionTarget && currentPointType == controlSnap.TargetConnector)
                    {
                        // Draw Target Arrow on Screen using passed Graphics
                        // Create arrow pen (using instance color field - mirror Direct Mode behavior)
                        using (var arrowPen = new Pen(this.TargetConnectorConnectedColor, DesignTimeLineWidth * 1.2f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                        {
                            // Pass SCREEN rectangle and graphics to the arrow drawing helper
                            DrawTargetArrowOnScreen(screenGraphics, rectScreen, arrowPen, currentPointType);
                        }
                        drawnSpecial = true;
                    }
                }

                if (!drawnSpecial) // Draw standard empty circle
                {
                    if (fillBrush != null && borderPen != null)
                    {
                        // Draw standard connector on Screen using passed Graphics
                        screenGraphics.FillEllipse(fillBrush, rectScreen);
                        screenGraphics.DrawEllipse(borderPen, rectScreen);
                    }
                }
            }
        }

        // Draws just the target arrow shape onto screen graphics at the given screen rectangle
        // ACCEPTS the passed screenGraphics object
        private void DrawTargetArrowOnScreen(Graphics screenGraphics, Rectangle rectScreen, Pen arrowPen, PointType direction)
        {
            // Safety check
            if (screenGraphics == null || rectScreen.IsEmpty) return;

            Point centerScreen = GetCenter(rectScreen); // Center is now in screen coords

            int arrowLength = (int)(DesignTimeConnectorSize * 0.70f);
            int arrowHalfWidth = (int)(DesignTimeConnectorSize * 0.30f);
            Point pTip, pWingLeft, pWingRight; // These will be screen coords

            switch (direction)
            { /* Calculation logic using centerScreen - same formulas as before */
                case PointType.Top: pTip = new Point(centerScreen.X, centerScreen.Y + arrowLength / 2); pWingLeft = new Point(centerScreen.X - arrowHalfWidth, centerScreen.Y - arrowLength / 2); pWingRight = new Point(centerScreen.X + arrowHalfWidth, centerScreen.Y - arrowLength / 2); break;
                case PointType.Bottom: pTip = new Point(centerScreen.X, centerScreen.Y - arrowLength / 2); pWingLeft = new Point(centerScreen.X - arrowHalfWidth, centerScreen.Y + arrowLength / 2); pWingRight = new Point(centerScreen.X + arrowHalfWidth, centerScreen.Y + arrowLength / 2); break;
                case PointType.Left: pTip = new Point(centerScreen.X + arrowLength / 2, centerScreen.Y); pWingLeft = new Point(centerScreen.X - arrowLength / 2, centerScreen.Y - arrowHalfWidth); pWingRight = new Point(centerScreen.X - arrowLength / 2, centerScreen.Y + arrowHalfWidth); break;
                case PointType.Right: default: pTip = new Point(centerScreen.X - arrowLength / 2, centerScreen.Y); pWingLeft = new Point(centerScreen.X + arrowLength / 2, centerScreen.Y - arrowHalfWidth); pWingRight = new Point(centerScreen.X + arrowLength / 2, centerScreen.Y + arrowHalfWidth); break;
            }
            // Use the passed screenGraphics object
            screenGraphics.DrawLine(arrowPen, pWingLeft, pTip);
            screenGraphics.DrawLine(arrowPen, pWingRight, pTip);
        }

        // Draws drag line onto screen graphics
        // ACCEPTS the passed screenGraphics object
        private void DrawDragLineOnScreen(Graphics screenGraphics, Point panelScreenOrigin, ControlSnapshot startControlSnap, PointType startPointType, Point currentScreenPoint, // End is already screen
                                         Color lineColor, float lineWidth, int outwardOffset)
        {
            // Safety check
            if (screenGraphics == null || startControlSnap == null || startPointType == PointType.None) return;

            var startRectsPanel = GetConnectorRects(startControlSnap.Bounds, outwardOffset);
            if (startRectsPanel.TryGetValue(startPointType, out var startConnRectPanel) && !startConnRectPanel.IsEmpty)
            {
                Point startPtPanel = GetCenter(startConnRectPanel);
                // Convert start point to screen coordinates
                Point startPtScreen = new Point(panelScreenOrigin.X + startPtPanel.X, panelScreenOrigin.Y + startPtPanel.Y);
                Point endPtScreen = currentScreenPoint; // End point is already screen coordinates

                using (Pen tempPen = new Pen(lineColor, lineWidth) { DashStyle = DashStyle.Dash })
                {
                    try
                    {
                        // Use the passed screenGraphics object
                        screenGraphics.DrawLine(tempPen, startPtScreen, endPtScreen);
                    }
                    catch (Exception ex) { LayoutLogger.Log($"DrawDragLineOnScreen Error: {ex.Message}"); }
                }
            }
        }



        // Draws existing connections onto screen graphics using snapshot data
        // ACCEPTS the single screenGraphics object passed from the main task method
        private void DrawExistingConnectionsOnScreen(Graphics screenGraphics, DesignTimeStateSnapshot snapshot)
        {
            // ... [Initial null checks and dictionary creation] ...
            if (screenGraphics == null || snapshot?.Connections == null || snapshot.VisibleControls == null) { /* ... */ return; }
            Point panelOrigin = snapshot.PanelScreenLocation;
            Dictionary<string, ControlSnapshot> controlsDic = null;
            try { controlsDic = snapshot.VisibleControls.Where(cs => !string.IsNullOrEmpty(cs.Name)).ToDictionary(cs => cs.Name); }
            catch (ArgumentException argEx) { LayoutLogger.Log($"ERROR creating control dict: {argEx.Message}"); return; }
            if (controlsDic == null || controlsDic.Count == 0) return;

            Color currentSourceColor = this.SourceConnectorConnectedColor;
            Color currentTargetColor = this.TargetConnectorConnectedColor;

            using (Pen linePen = new Pen(ConnectionLineColor, DesignTimeLineWidth) { StartCap = LineCap.RoundAnchor, EndCap = LineCap.RoundAnchor, LineJoin = LineJoin.Round })
            using (Pen sourceBorderPen = new Pen(Color.DimGray, DesignTimeLineWidth))
            using (SolidBrush sourceDotBrush = new SolidBrush(currentSourceColor))
            {
                foreach (var kvpConnection in snapshot.Connections)
                {
                    string sourceName = kvpConnection.Key; ConnectionInfo connInfo = kvpConnection.Value; string targetName = connInfo.TargetName;
                    if (controlsDic.TryGetValue(sourceName, out var sourceSnap) && controlsDic.TryGetValue(targetName, out var targetSnap))
                    {
                        // --- Determine ICON Connection Points & Reroute Logic (Panel Relative) ---

                        // 1. Determine INITIAL types for ICONS based on alignment and opposite/closest
                        PointType initialTargetIconPointType = MapAlignmentToConnector(connInfo.Alignment);
                        PointType initialSourceIconPointType = GetOppositeConnectorType(initialTargetIconPointType);
                        // Fallbacks if alignment is None or opposite doesn't work well
                        if (initialSourceIconPointType == PointType.None)
                        {
                            initialSourceIconPointType = GetClosestConnectionPointType(sourceSnap.Bounds, GetCenter(targetSnap.Bounds));
                        }
                        if (initialTargetIconPointType == PointType.None)
                        {
                            initialTargetIconPointType = GetClosestConnectionPointType(targetSnap.Bounds, GetCenter(sourceSnap.Bounds));
                            if (initialTargetIconPointType == PointType.None) continue; // Cannot proceed if target is still None
                        }
                        // Fallback for source if still None after closest (unlikely but possible)
                        if (initialSourceIconPointType == PointType.None)
                        {
                            initialSourceIconPointType = GetClosestConnectionPointType(sourceSnap.Bounds, GetCenter(targetSnap.Bounds));
                            if (initialSourceIconPointType == PointType.None) continue; // Cannot proceed if source is None
                        }


                        // 2. Get all potential connector rectangles (Panel Relative)
                        var sourceRectsPanel = GetConnectorRects(sourceSnap.Bounds, DesignTimeConnectorOffset);
                        var targetRectsPanel = GetConnectorRects(targetSnap.Bounds, DesignTimeConnectorOffset);

                        // 3. Get INITIAL points based on INITIAL icon types (for overlap check)
                        Point initialStartPtPanel = Point.Empty, initialEndPtPanel = Point.Empty;
                        if (sourceRectsPanel.TryGetValue(initialSourceIconPointType, out var initSourceRect)) initialStartPtPanel = GetCenter(initSourceRect); else continue; // Cannot check overlap without start point
                        if (targetRectsPanel.TryGetValue(initialTargetIconPointType, out var initTargetRect)) initialEndPtPanel = GetCenter(initTargetRect); else continue; // Cannot check overlap without end point

                        // 4. Determine FINAL types/rects for ICON drawing (these don't usually change unless we force icons elsewhere)
                        // For now, final icon types ARE the initial icon types.
                        PointType finalSourceIconPointType = initialSourceIconPointType;
                        PointType finalTargetIconPointType = initialTargetIconPointType;

                        // Get the rectangles for drawing the final icons
                        if (!sourceRectsPanel.TryGetValue(finalSourceIconPointType, out Rectangle finalSourceIconRectPanel) ||
                            !targetRectsPanel.TryGetValue(finalTargetIconPointType, out Rectangle finalTargetIconRectPanel) ||
                            finalSourceIconRectPanel.IsEmpty || finalTargetIconRectPanel.IsEmpty)
                        {
                            LayoutLogger.Log($" -> Could not get FINAL ICON rects for {sourceName}->{targetName}. Skipping.");
                            continue; // Skip if final icon rects are invalid
                        }
                        // --- END OF SECTION TO FILL ---


                        // --- Determine PATH types based on overlap/rerouting logic ---
                        PointType pathSourceType = initialSourceIconPointType; // Start assuming path uses icon types
                        PointType pathTargetType = initialTargetIconPointType;
                        bool rerouted = false;
                        if ((initialSourceIconPointType == PointType.Left && initialTargetIconPointType == PointType.Right && initialStartPtPanel.X >= initialEndPtPanel.X) || (initialSourceIconPointType == PointType.Right && initialTargetIconPointType == PointType.Left && initialStartPtPanel.X <= initialEndPtPanel.X))
                        { pathSourceType = PointType.Top; pathTargetType = PointType.Top; rerouted = true; } // Reroute path vertically
                        else if ((initialSourceIconPointType == PointType.Top && initialTargetIconPointType == PointType.Bottom && initialStartPtPanel.Y >= initialEndPtPanel.Y) || (initialSourceIconPointType == PointType.Bottom && initialTargetIconPointType == PointType.Top && initialStartPtPanel.Y <= initialEndPtPanel.Y))
                        { pathSourceType = PointType.Left; pathTargetType = PointType.Left; rerouted = true; } // Reroute path horizontally


                        // --- Calculate Orthogonal Path (Panel Relative) ---
                        // Path starts from Source TopLeft, ends at Target Icon Center
                        // Uses PATH types determined above for routing
                        Point sourceTopLeftPanel = sourceSnap.Bounds.Location;
                        Point targetIconCenterPanel = GetCenter(finalTargetIconRectPanel); // Center of where target icon goes

                        List<Point> pathPanel = CalculateOrthogonalPath_TopLeftSource(
                            sourceTopLeftPanel,         // Start visually from source TopLeft
                            targetIconCenterPanel,      // End logically near target icon center
                            pathTargetType,             // Use PATH target type for routing
                            DesignTimeLineRoutingMargin
                        );

                        LayoutLogger.Log($"Path Calc (TL Source) for {sourceName}->{targetName}: TargetPathType={pathTargetType}, Points={(pathPanel?.Count ?? 0)}. Rerouted={rerouted}");


                        // --- Draw Path and Icons on Screen ---
                        if (pathPanel != null && pathPanel.Count >= 2)
                        {
                            Point[] pathScreen = pathPanel.Select(p => new Point(panelOrigin.X + p.X, panelOrigin.Y + p.Y)).ToArray();
                            try
                            {
                                // --- Draw Lines ---
                                screenGraphics.DrawLines(linePen, pathScreen);
                                // LayoutLogger.Log($" -> DrawLines attempted for {sourceName}->{targetName}");

                                // --- Draw Icons (Using FINAL ICON types and Rects converted to screen) ---
                                // Source Dot
                                Rectangle sourceIconRectScreen = new Rectangle(panelOrigin.X + finalSourceIconRectPanel.X, panelOrigin.Y + finalSourceIconRectPanel.Y, finalSourceIconRectPanel.Width, finalSourceIconRectPanel.Height);
                                screenGraphics.FillEllipse(sourceDotBrush, sourceIconRectScreen);
                                screenGraphics.DrawEllipse(sourceBorderPen, sourceIconRectScreen);

                                // Target Arrow
                                Rectangle targetIconRectScreen = new Rectangle(panelOrigin.X + finalTargetIconRectPanel.X, panelOrigin.Y + finalTargetIconRectPanel.Y, finalTargetIconRectPanel.Width, finalTargetIconRectPanel.Height);
                                using (var arrowPen = new Pen(currentTargetColor, DesignTimeLineWidth * 1.2f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                                { DrawTargetArrowOnScreen(screenGraphics, targetIconRectScreen, arrowPen, finalTargetIconPointType); } // Use FINAL target icon type
                            }
                            catch (Exception ex) { LayoutLogger.Log($" -> Drawing FAILED for {sourceName}->{targetName}: {ex.Message}"); }
                            // No finally block needed here for DC/Graphics
                        }
                        else { LayoutLogger.Log($" -> DrawLines SKIPPED for {sourceName}->{targetName} due to invalid path."); }
                    } // End if snapshots found
                } // End foreach connection
            } // End Using Pens/Brushes
        } // End DrawExistingConnectionsOnScreen Method

        #endregion

        #region Methods for Direct Drawing Mode (Using IDesignTimeDrawer)

        // This method contains the core logic for drawing directly onto the panel's Graphics context
        // It uses the IDesignTimeDrawer interface passed to it.
        private void PaintDesignTimeVisuals(IDesignTimeDrawer drawer)
        {
            // LayoutLogger.Log("PaintDesignTimeVisuals using Direct Drawer"); // Example Log

            // --- Define Colors ---
            Color stdConnectorBorder = Color.DimGray;
            Color stdConnectorFill = Color.FromArgb(240, 240, 240);
            Color targetConnectorBorder = Color.DarkGray;
            Color targetConnectorFill = Color.FromArgb(220, 220, 220);
            Color ConnectionLineColor = Color.DarkSlateBlue;
            // SourceConnectorConnectedColor/TargetConnectorConnectedColor are fields

            // --- Get Selection Info ---
            Control selectedChild = null;
            if (_selectionServiceDT != null && _selectionServiceDT.PrimarySelection is Control sc && sc.Parent == this) { selectedChild = sc; }

            // --- 1. Draw Connectors on Currently Selected Child ---
            if (selectedChild != null)
            {
                bool isBreakingFromSelected = (_currentDragModeDT == DesignDragMode.Breaking && _sourceControlDT == selectedChild);
                if (!isBreakingFromSelected)
                {
                    DrawConnectionPointsForControl(drawer, selectedChild, stdConnectorBorder, stdConnectorFill, DesignTimeLineWidth, DesignTimeConnectorOffset); // Uses Drawer
                }
            }

            // --- 2. Draw Existing Connections ---
            DrawExistingConnections(drawer, ConnectionLineColor, DesignTimeLineWidth, SourceConnectorConnectedColor, TargetConnectorConnectedColor, stdConnectorBorder, stdConnectorFill, DesignTimeConnectorOffset); // Uses Drawer

            // --- 3. Draw Drag Feedback ---
            if (_currentDragModeDT != DesignDragMode.None)
            {
                if (_currentDragModeDT == DesignDragMode.Connecting && _sourceControlDT != null)
                {
                    foreach (Control child in this.Controls.OfType<Control>())
                    {
                        if (child != _sourceControlDT && child.Visible)
                        { DrawConnectionPointsForControl(drawer, child, targetConnectorBorder, targetConnectorFill, DesignTimeLineWidth, DesignTimeConnectorOffset, true); }
                    } // Uses Drawer
                    DrawDragLine(drawer, _sourceControlDT, _startConnectorTypeDT, _dragCurrentPointScreenDT, Color.Blue, DesignTimeLineWidth, DesignTimeConnectorOffset); // Uses Drawer
                }
                else if (_currentDragModeDT == DesignDragMode.Breaking && _breakLinkTargetControlDT != null)
                {
                    DrawDragLine(drawer, _breakLinkTargetControlDT, _breakLinkTargetConnectorDT, _dragCurrentPointScreenDT, Color.Red, DesignTimeLineWidth, DesignTimeConnectorOffset); // Uses Drawer
                }
            }
        }

        // --- Keep IDesignTimeDrawer versions of helpers for Direct Mode ---

        // Helper: Connectors for a single control using IDesignTimeDrawer
        private void DrawConnectionPointsForControl(IDesignTimeDrawer drawer, Control c, Color borderColor, Color fillColor, float lineWidth, int outwardOffset, bool forceStandard = false)
        {
            if (c == null || !c.Visible) return;
            var bounds = GetControlBoundsInPanel(c); var connectorRectsPanel = GetConnectorRects(bounds, outwardOffset);
            GetConnectionState(c, out bool isSource, out PointType sourcePoint, out bool isTarget, out PointType targetPoint, out _);

            using (var borderPen = new Pen(borderColor, lineWidth)) using (var fillBrush = new SolidBrush(fillColor))
            using (var sourceDotBrush = new SolidBrush(SourceConnectorConnectedColor))
            {
                foreach (var kvp in connectorRectsPanel)
                {
                    PointType currentPointType = kvp.Key; Rectangle rect = kvp.Value; if (rect.IsEmpty) continue;
                    bool drawnSpecial = false; if (!forceStandard)
                    {
                        if (isSource && currentPointType == sourcePoint) { drawer.FillEllipse(sourceDotBrush, rect); drawer.DrawEllipse(borderPen, rect); drawnSpecial = true; }
                        else if (isTarget && currentPointType == targetPoint) { using (var arrowPen = new Pen(TargetConnectorConnectedColor, lineWidth * 1.2f) { StartCap = LineCap.Round, EndCap = LineCap.Round }) { DrawTargetConnectedIcon(drawer, rect, arrowPen, currentPointType); } drawnSpecial = true; }
                    }
                    if (!drawnSpecial) { drawer.FillEllipse(fillBrush, rect); drawer.DrawEllipse(borderPen, rect); }
                }
            }
        }

        // Helper: Arrow ONLY via Drawer
        private void DrawTargetConnectedIcon(IDesignTimeDrawer drawer, Rectangle rectSource, Pen arrowPen, PointType direction)
        {
            if (rectSource.IsEmpty) return; Point center = GetCenter(rectSource);
            int arrowLength = (int)(DesignTimeConnectorSize * 0.70f), arrowHalfWidth = (int)(DesignTimeConnectorSize * 0.30f); Point pTip, pWingLeft, pWingRight;
            switch (direction)
            { /* Calculation logic using center */
                case PointType.Top: pTip = new Point(center.X, center.Y + arrowLength / 2); pWingLeft = new Point(center.X - arrowHalfWidth, center.Y - arrowLength / 2); pWingRight = new Point(center.X + arrowHalfWidth, center.Y - arrowLength / 2); break;
                case PointType.Bottom: pTip = new Point(center.X, center.Y - arrowLength / 2); pWingLeft = new Point(center.X - arrowHalfWidth, center.Y + arrowLength / 2); pWingRight = new Point(center.X + arrowHalfWidth, center.Y + arrowLength / 2); break;
                case PointType.Left: pTip = new Point(center.X + arrowLength / 2, center.Y); pWingLeft = new Point(center.X - arrowLength / 2, center.Y - arrowHalfWidth); pWingRight = new Point(center.X - arrowLength / 2, center.Y + arrowHalfWidth); break;
                case PointType.Right: default: pTip = new Point(center.X - arrowLength / 2, center.Y); pWingLeft = new Point(center.X + arrowLength / 2, center.Y - arrowHalfWidth); pWingRight = new Point(center.X + arrowLength / 2, center.Y + arrowHalfWidth); break;
            }
            drawer.DrawLine(arrowPen, pWingLeft, pTip); drawer.DrawLine(arrowPen, pWingRight, pTip); // Use drawer
        }

        // Helper: Existing connections via Drawer (with TopLeft source path logic) - FOR DIRECT MODE
        private void DrawExistingConnections(IDesignTimeDrawer drawer, Color lineColor, float lineWidth, Color sourceDotColor, Color targetArrowColor, Color defaultBorder, Color defaultFill, int outwardOffset)
        {
            if (_designerHostDT == null || _designerHostDT.Container == null) return;

            using (Pen linePen = new Pen(lineColor, lineWidth) { StartCap = LineCap.RoundAnchor, EndCap = LineCap.RoundAnchor, LineJoin = LineJoin.Round })
            using (Pen borderPen = new Pen(defaultBorder, lineWidth)) // For icon borders
            using (SolidBrush sourceDotBrush = new SolidBrush(sourceDotColor)) // For source icon fill
            {
                foreach (Control source in this.Controls.OfType<Control>().Where(c => c.Visible))
                {
                    var sourceProps = this.GetPropertiesOrDefault(source);
                    if (sourceProps.IsFloating && !string.IsNullOrEmpty(sourceProps.FloatTargetName))
                    {
                        Control target = null;
                        try { target = _designerHostDT.Container.Components[sourceProps.FloatTargetName] as Control; }
                        catch { target = null; }

                        if (target != null && target.Parent == this && target.Visible)
                        {
                            var sourceBounds = GetControlBoundsInPanel(source);
                            var targetBounds = GetControlBoundsInPanel(target);

                            // --- Determine ICON Connection Points (Panel Relative) ---
                            PointType targetIconPointType = MapAlignmentToConnector(sourceProps.FloatAlignment);
                            PointType sourceIconPointType = GetOppositeConnectorType(targetIconPointType);
                            if (sourceIconPointType == PointType.None) sourceIconPointType = GetClosestConnectionPointType(sourceBounds, GetCenter(targetBounds));
                            if (targetIconPointType == PointType.None)
                            {
                                // LayoutLogger.Log($"Skipping Direct connection {source.Name}->{target.Name}: Invalid Target Alignment {sourceProps.FloatAlignment}");
                                continue; // Cannot draw line or target icon
                            }

                            var sourceRectsPanel = GetConnectorRects(sourceBounds, outwardOffset);
                            var targetRectsPanel = GetConnectorRects(targetBounds, outwardOffset);

                            Rectangle sourceIconConnRectPanel, targetIconConnRectPanel;
                            Point targetIconCenterPanel;

                            if (!sourceRectsPanel.TryGetValue(sourceIconPointType, out sourceIconConnRectPanel) ||
                                !targetRectsPanel.TryGetValue(targetIconPointType, out targetIconConnRectPanel) ||
                                sourceIconConnRectPanel.IsEmpty || targetIconConnRectPanel.IsEmpty)
                            {
                                // LayoutLogger.Log($"Skipping Direct connection {source.Name}->{target.Name}: Could not get valid ICON connector rects.");
                                continue;
                            }
                            targetIconCenterPanel = GetCenter(targetIconConnRectPanel);

                            // --- Calculate Orthogonal Path (Using TopLeftSource Function) ---
                            Point sourceTopLeftPanel = sourceBounds.Location; // Source TopLeft

                            // *** CALL THE NEW PATH FUNCTION ***
                            List<Point> pathPanel = CalculateOrthogonalPath_TopLeftSource(
                                sourceTopLeftPanel,
                                targetIconCenterPanel,
                                targetIconPointType,
                                DesignTimeLineRoutingMargin
                            );

                            // --- Logging for Debugging ---
                            // LayoutLogger.Log($"Direct Path Calc (TL Source) for {source.Name}->{target.Name}: TargetType={targetIconPointType}, Points={(pathPanel?.Count ?? 0)}.");

                            // --- Draw Path and Icons using the Drawer ---
                            if (pathPanel != null && pathPanel.Count >= 2)
                            {
                                try
                                {
                                    // Draw Path using Drawer
                                    drawer.DrawLines(linePen, pathPanel.ToArray());

                                    // Draw Icons using Drawer (at their original connector locations)
                                    // Source Dot
                                    drawer.FillEllipse(sourceDotBrush, sourceIconConnRectPanel);
                                    drawer.DrawEllipse(borderPen, sourceIconConnRectPanel);

                                    // Target Arrow
                                    using (var arrowPen = new Pen(targetArrowColor, DesignTimeLineWidth * 1.2f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                                    {
                                        DrawTargetConnectedIcon(drawer, targetIconConnRectPanel, arrowPen, targetIconPointType); // Pass drawer
                                    }
                                }
                                catch (Exception drawEx) { LayoutLogger.Log($"ERROR Direct DrawExistingConnections DrawLines/Icons: {drawEx.Message}"); }
                            }
                            // else { LayoutLogger.Log($" -> Direct DrawLines SKIPPED for {source.Name}->{target.Name} due to invalid path."); }
                        } // End if target valid
                    } // End if source is floating
                } // End foreach source control
            } // End Using Pens/Brushes
        } // End Method
        // Helper: Drag line via Drawer
        private void DrawDragLine(IDesignTimeDrawer drawer, Control startControl, PointType startPointType, Point currentScreenPoint, Color lineColor, float lineWidth, int outwardOffset)
        {
            if (startControl == null || startPointType == PointType.None || startControl.IsDisposed) return;
            var startBounds = GetControlBoundsInPanel(startControl); var startRectsPanel = GetConnectorRects(startBounds, outwardOffset);
            if (startRectsPanel.TryGetValue(startPointType, out var startConnRectPanel) && !startConnRectPanel.IsEmpty)
            {
                Point startPtSource = GetCenter(startConnRectPanel); Point endPtSource = this.PointToClient(currentScreenPoint);
                using (Pen tempPen = new Pen(lineColor, lineWidth) { DashStyle = DashStyle.Dash }) { drawer.DrawLine(tempPen, startPtSource, endPtSource); }
            } // Use drawer
        }

        #endregion

        #region Geometry, Mapping & State Helpers (Shared by Capture/Direct Mode)

        // --- Get Connection State ---
        private void GetConnectionState(Control control, out bool isSourceConnected, out PointType sourcePoint, out bool isTargetConnected, out PointType targetPoint, out Control sourceControlForTarget)
        {
            isSourceConnected = false; sourcePoint = PointType.None; isTargetConnected = false; targetPoint = PointType.None; sourceControlForTarget = null;
            if (control == null || !this.DesignMode) return; // Check design mode
            EnsureServicesDT(); // Make sure _designerHostDT is available if needed
            if (_designerHostDT == null || _designerHostDT.Container == null) return; // Need host to check connections

            // Check if 'control' is a SOURCE
            var props = GetPropertiesOrDefault(control); // Extender method from other partial
            if (props.IsFloating && !string.IsNullOrEmpty(props.FloatTargetName))
            {
                Control targetControl = null; try { targetControl = _designerHostDT.Container.Components[props.FloatTargetName] as Control; } catch { }
                if (targetControl != null && targetControl.Parent == this && targetControl.Visible)
                {
                    var controlBounds = GetControlBoundsInPanel(control); var targetBounds = GetControlBoundsInPanel(targetControl);
                    PointType targetConnType = MapAlignmentToConnector(props.FloatAlignment); sourcePoint = GetOppositeConnectorType(targetConnType);
                    if (sourcePoint == PointType.None) sourcePoint = GetClosestConnectionPointType(controlBounds, GetCenter(targetBounds));
                    isSourceConnected = true;
                }
            }
            // Check if 'control' is a TARGET
            if (!string.IsNullOrEmpty(control.Name))
            {
                foreach (IComponent component in _designerHostDT.Container.Components)
                {
                    if (component is Control potentialSource && potentialSource.Parent == this && potentialSource != control && potentialSource.Visible)
                    {
                        var sourceProps = GetPropertiesOrDefault(potentialSource);
                        if (sourceProps.IsFloating && sourceProps.FloatTargetName == control.Name)
                        {
                            targetPoint = MapAlignmentToConnector(sourceProps.FloatAlignment); isTargetConnected = true; sourceControlForTarget = potentialSource; break;
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
            var dict = new Dictionary<PointType, Rectangle>(); if (controlBounds.IsEmpty) return dict;
            int cs = DesignTimeConnectorSize, hcs = DesignTimeHalfConnectorSize; int midX = controlBounds.Left + controlBounds.Width / 2, midY = controlBounds.Top + controlBounds.Height / 2;
            int topY = controlBounds.Top - cs - outwardOffset, bottomY = controlBounds.Bottom + outwardOffset, leftX = controlBounds.Left - cs - outwardOffset, rightX = controlBounds.Right + outwardOffset;
            int cxTB = midX - hcs, cyLR = midY - hcs;
            dict[PointType.Top] = new Rectangle(cxTB, topY, cs, cs); dict[PointType.Bottom] = new Rectangle(cxTB, bottomY, cs, cs); dict[PointType.Left] = new Rectangle(leftX, cyLR, cs, cs); dict[PointType.Right] = new Rectangle(rightX, cyLR, cs, cs);
            return dict;
        }

        // --- Center Point ---
        private Point GetCenter(Rectangle rect) => rect.IsEmpty ? Point.Empty : new Point(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2);

        // --- Enum Mappings ---
        private FloatAlignment MapConnectorToAlignment(PointType targetPointType) { switch (targetPointType) { case PointType.Top: return FloatAlignment.ToTopOf; case PointType.Bottom: return FloatAlignment.ToBottomOf; case PointType.Left: return FloatAlignment.ToLeftOf; case PointType.Right: return FloatAlignment.ToRightOf; default: return FloatAlignment.TopLeft; } }
        private PointType MapAlignmentToConnector(FloatAlignment alignment) { switch (alignment) { case FloatAlignment.ToTopOf: return PointType.Top; case FloatAlignment.ToBottomOf: return PointType.Bottom; case FloatAlignment.ToLeftOf: return PointType.Left; case FloatAlignment.ToRightOf: return PointType.Right; default: return PointType.None; } }

        // --- Opposite Connector ---
        private PointType GetOppositeConnectorType(PointType type) { switch (type) { case PointType.Top: return PointType.Bottom; case PointType.Bottom: return PointType.Top; case PointType.Left: return PointType.Right; case PointType.Right: return PointType.Left; default: return PointType.None; } }

        // --- Closest Point Calculation ---
        private PointType GetClosestConnectionPointType(Rectangle sourceBounds, Point targetPoint) { var rects = GetConnectorRects(sourceBounds, DesignTimeConnectorOffset); PointType closest = PointType.None; double minDistSq = double.MaxValue; foreach (var kvp in rects) { if (kvp.Value.IsEmpty) continue; Point center = GetCenter(kvp.Value); double dx = center.X - targetPoint.X; double dy = center.Y - targetPoint.Y; double distSq = dx * dx + dy * dy; if (distSq < minDistSq) { minDistSq = distSq; closest = kvp.Key; } } return closest; }

        // --- Calculate Orthogonal Path --- (User Provided Version)
        private List<Point> CalculateOrthogonalPath(Point startPt, Point endPt, PointType startType, PointType endType, int margin)
        {
            // Use DesignTimeLineRoutingMargin defined in this file
            // int margin = DesignTimeLineRoutingMargin; // Margin is passed in

            if (startPt == endPt) return new List<Point> { startPt, endPt }; // Avoid zero-length

            // Handle cases where start or end type might be None (e.g., from fallback logic)
            if (startType == PointType.None || endType == PointType.None)
            {
                // Default to a direct line if types are invalid for routing
                // LayoutLogger.Log($"CalculateOrthogonalPath WARN: Invalid PointType (Start={startType}, End={endType}). Returning direct line.");
                return new List<Point> { startPt, endPt };
            }


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
                    // No default needed due to None check above
            }

            // Calculate p2 based on endType
            switch (endType)
            {
                case PointType.Top: p2.Y -= margin; break;
                case PointType.Bottom: p2.Y += margin; break;
                case PointType.Left: p2.X -= margin; break;
                case PointType.Right: p2.X += margin; break;
                    // No default needed due to None check above
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

            // Optimize path - remove duplicate consecutive points
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

            // LayoutLogger.Log($"      - Calculated Path (User Version): {string.Join(", ", optimizedPath.Select(p => p.ToString()))}");
            return optimizedPath;
        }

        /// <summary>
        /// Calculates an orthogonal path starting visually from the Top-Left corner
        /// of the source bounds and ending at the center of the target connector.
        /// </summary>
        /// <param name="sourceTopLeft">The Top-Left coordinate of the source control's bounds (panel relative).</param>
        /// <param name="targetConnectorCenter">The center coordinate of the target connector (panel relative).</param>
        /// <param name="targetType">The type (Top, Bottom, Left, Right) of the target connector.</param>
        /// <param name="margin">The routing margin.</param>
        /// <returns>A list of points for the orthogonal path.</returns>
        private List<Point> CalculateOrthogonalPath_TopLeftSource(Point sourceTopLeft, Point targetConnectorCenter, PointType targetType, int margin)
        {
            // Validate inputs
            if (targetType == PointType.None || margin <= 0)
            {
                // Cannot route properly without a target type or margin, return direct line
                return new List<Point> { sourceTopLeft, targetConnectorCenter };
            }
            if (sourceTopLeft == targetConnectorCenter)
            {
                return new List<Point> { sourceTopLeft, targetConnectorCenter };
            }

            var path = new List<Point>();
            Point startPt = sourceTopLeft;
            Point endPt = targetConnectorCenter;

            path.Add(startPt); // 1. Start at Source Top-Left

            // 2. Calculate p2: Point 'margin' distance away from the target connector center
            Point p2 = endPt;
            switch (targetType)
            {
                case PointType.Top: p2.Y -= margin; break;
                case PointType.Bottom: p2.Y += margin; break;
                case PointType.Left: p2.X -= margin; break;
                case PointType.Right: p2.X += margin; break;
            }

            // 3. Calculate the intermediate corner point(s)
            bool targetIsVertical = (targetType == PointType.Top || targetType == PointType.Bottom);

            Point pIntermediate;

            if (targetIsVertical)
            {
                // Target connection is vertical, so final segment is vertical (from p2 to endPt).
                // The segment before p2 must be horizontal.
                // The corner point shares Y with p2 and X with startPt.
                pIntermediate = new Point(startPt.X, p2.Y);
                path.Add(pIntermediate); // Add corner first
                path.Add(p2);           // Then add outward target point
            }
            else // Target connection is horizontal
            {
                // Target connection is horizontal, so final segment is horizontal.
                // The segment before p2 must be vertical.
                // The corner point shares X with p2 and Y with startPt.
                pIntermediate = new Point(p2.X, startPt.Y);
                path.Add(pIntermediate); // Add corner first
                path.Add(p2);           // Then add outward target point
            }


            path.Add(endPt); // 4. End at the target connector center


            // 5. Optimize path (remove consecutive duplicates)
            var optimizedPath = new List<Point>();
            if (path.Count > 0)
            {
                optimizedPath.Add(path[0]);
                for (int i = 1; i < path.Count; i++)
                {
                    if (path[i] != path[i - 1]) optimizedPath.Add(path[i]);
                }
            }

            LayoutLogger.Log($"      - Calculated Path (TopLeft Source): {string.Join(", ", optimizedPath.Select(p => p.ToString()))}");
            return optimizedPath;
        }

        #endregion

     

        #region Design-Time Mouse Event Overrides & Selection Change

        protected override void OnMouseDown(MouseEventArgs e)
        {
            bool designTimeHandled = false;
            if (this.DesignMode && e.Button == MouseButtons.Left)
            {
                EnsureServicesDT(); Point panelPoint = e.Location; Point screenPoint = this.PointToScreen(panelPoint); ResetDragStateDT();
                if (HitTestTargetArrow(panelPoint, out Control sourceControlForBreak, out Control targetControlHit, out PointType targetPointHit))
                { _currentDragModeDT = DesignDragMode.Breaking; _sourceControlDT = sourceControlForBreak; _breakLinkTargetControlDT = targetControlHit; _breakLinkTargetConnectorDT = targetPointHit; _startConnectorTypeDT = targetPointHit; _dragStartPointScreenDT = screenPoint; _dragCurrentPointScreenDT = screenPoint; this.Capture = true; designTimeHandled = true; }
                else if (_selectionServiceDT != null && _selectionServiceDT.PrimarySelection is Control selectedControl && selectedControl.Parent == this)
                {
                    if (HitTestSourceConnector(panelPoint, selectedControl, out PointType sourcePointHit))
                    { _currentDragModeDT = DesignDragMode.Connecting; _sourceControlDT = selectedControl; _startConnectorTypeDT = sourcePointHit; _dragStartPointScreenDT = screenPoint; _dragCurrentPointScreenDT = screenPoint; this.Capture = true; designTimeHandled = true; }
                }
                if (designTimeHandled) { this.Invalidate(true); /* Trigger OnPaint */ }
            }
            if (!designTimeHandled) { base.OnMouseDown(e); }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (!this.DesignMode || _currentDragModeDT == DesignDragMode.None) { base.OnMouseMove(e); return; } // Exit if not dragging or not design mode
            if (this.Capture) { _dragCurrentPointScreenDT = this.PointToScreen(e.Location); this.Invalidate(true); /* Trigger OnPaint */ }
            else { LayoutLogger.Log($"StackLayoutDT [{this.Name}]: MouseMove - Capture lost. Resetting."); ResetDragStateDT(); this.Invalidate(true); }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (!this.DesignMode || _currentDragModeDT == DesignDragMode.None) { base.OnMouseUp(e); return; } // Exit if not dragging or not design mode
            bool stateChanged = false;
            if (this.Capture) { this.Capture = false; }
            if (e.Button == MouseButtons.Left)
            {
                Point panelPoint = e.Location; HitTestConnector(panelPoint, out Control droppedOnControl, out PointType droppedOnPoint);
                if (_currentDragModeDT == DesignDragMode.Connecting)
                {
                    if (droppedOnControl != null && droppedOnControl != _sourceControlDT && droppedOnPoint != PointType.None)
                    { ApplyConnectionDT(_sourceControlDT, droppedOnControl, droppedOnPoint); stateChanged = true; /* ApplyConnection calls Invalidate */ }
                }
                else if (_currentDragModeDT == DesignDragMode.Breaking)
                {
                    if (droppedOnControl == null) // Dropped clear
                    { BreakConnectionDT(_sourceControlDT); stateChanged = true; /* BreakConnection calls Invalidate */ }
                }
            }
            // Reset state AFTER potentially changing properties
            ResetDragStateDT();
            // Invalidate AFTER resetting state, regardless of whether properties changed, to clear drag visuals
            this.Invalidate(true);
        }

        // Handles SelectionChanged event from ISelectionService
        private void SelectionService_SelectionChanged(object sender, EventArgs e)
        {
            // LayoutLogger.Log($"StackLayoutDT [{this.Name}]: SelectionService_SelectionChanged Fired."); // Noisy
            if (this.IsHandleCreated && !this.IsDisposed && this.DesignMode) { try { this.Invalidate(true); } catch (Exception ex) { LayoutLogger.Log($"ERROR Invalidate in SelectionChanged: {ex.Message}"); } }
        }

        #endregion

        #region Design-Time Hit Testing Helpers

        // Checks if point is on *any* connector of *any* visible child
        private bool HitTestConnector(Point panelPoint, out Control hitControl, out PointType hitPointType)
        {
            hitControl = null; hitPointType = PointType.None;
            foreach (Control child in this.Controls.OfType<Control>().Reverse().Where(c => c.Visible))
            {
                var bounds = GetControlBoundsInPanel(child); var rects = GetConnectorRects(bounds, DesignTimeConnectorOffset);
                foreach (var kvp in rects) { if (kvp.Value.Contains(panelPoint)) { hitControl = child; hitPointType = kvp.Key; return true; } }
            }
            return false;
        }

        // Checks specifically if point is on a TARGET arrow icon
        private bool HitTestTargetArrow(Point panelPoint, out Control sourceControl, out Control targetControl, out PointType targetPoint)
        {
            sourceControl = null; targetControl = null; targetPoint = PointType.None;
            if (!this.DesignMode || _designerHostDT == null || _designerHostDT.Container == null) return false;
            foreach (Control potentialTarget in this.Controls.OfType<Control>().Reverse().Where(c => c.Visible && !string.IsNullOrEmpty(c.Name)))
            {
                foreach (IComponent component in _designerHostDT.Container.Components)
                {
                    if (component is Control potentialSource && potentialSource.Parent == this && potentialSource != potentialTarget && potentialSource.Visible)
                    {
                        var sourceProps = this.GetPropertiesOrDefault(potentialSource);
                        if (sourceProps.IsFloating && sourceProps.FloatTargetName == potentialTarget.Name)
                        {
                            PointType pointOnTarget = MapAlignmentToConnector(sourceProps.FloatAlignment);
                            if (pointOnTarget != PointType.None)
                            {
                                var targetBounds = GetControlBoundsInPanel(potentialTarget); var targetRects = GetConnectorRects(targetBounds, DesignTimeConnectorOffset);
                                if (targetRects.TryGetValue(pointOnTarget, out Rectangle arrowRect) && arrowRect.Contains(panelPoint))
                                {
                                    sourceControl = potentialSource; targetControl = potentialTarget; targetPoint = pointOnTarget; return true;
                                }
                            }
                            goto nextPotentialTarget; // Optimization
                        }
                    }
                }
            nextPotentialTarget:;
            }
            return false;
        }

        // Checks if point is on a standard source connector of the *currently selected* control
        private bool HitTestSourceConnector(Point panelPoint, Control selectedControl, out PointType sourcePoint)
        {
            sourcePoint = PointType.None; if (selectedControl == null || !selectedControl.Visible) return false;
            var bounds = GetControlBoundsInPanel(selectedControl); var rects = GetConnectorRects(bounds, DesignTimeConnectorOffset);
            foreach (var kvp in rects)
            {
                if (kvp.Value.Contains(panelPoint))
                {
                    GetConnectionState(selectedControl, out _, out _, out bool isTargetAtThisPoint, out PointType targetPointType, out _);
                    if (isTargetAtThisPoint && kvp.Key == targetPointType) { return false; /* Is an arrow */ }
                    sourcePoint = kvp.Key; return true; /* Hit standard/source */
                }
            }
            return false;
        }

        #endregion

        #region Design-Time State & Property Change Logic

        // Resets the internal state variables used for tracking drags
        private void ResetDragStateDT()
        { _currentDragModeDT = DesignDragMode.None; _sourceControlDT = null; _startConnectorTypeDT = PointType.None; _dragStartPointScreenDT = Point.Empty; _dragCurrentPointScreenDT = Point.Empty; _breakLinkTargetControlDT = null; _breakLinkTargetConnectorDT = PointType.None; }

        // Applies property changes for creating a connection (Uses DesignerTransaction)
        private void ApplyConnectionDT(Control source, Control target, PointType targetPointType)
        {
            EnsureServicesDT(); if (_componentChangeServiceDT == null || _designerHostDT == null) { LayoutLogger.Log($"ERROR ApplyConnectionDT: Services unavailable."); this.Invalidate(true); return; }
            if (source == null || target == null || string.IsNullOrEmpty(target.Name)) { LayoutLogger.Log($"ERROR ApplyConnectionDT: Args invalid."); return; }
            FloatAlignment alignment = MapConnectorToAlignment(targetPointType);
            PropertyDescriptor isFloatingProp = TypeDescriptor.GetProperties(source)["lay_IsFloating"], targetNameProp = TypeDescriptor.GetProperties(source)["lay_FloatTargetName"], alignmentProp = TypeDescriptor.GetProperties(source)["lay_FloatAlignment"];
            if (isFloatingProp == null || targetNameProp == null || alignmentProp == null) { LayoutLogger.Log($"ERROR ApplyConnectionDT: Props not found."); return; }
            DesignerTransaction transaction = null; try
            {
                transaction = _designerHostDT.CreateTransaction($"Connect {source.Name} to {target.Name}");
                bool oldIsFloating = (bool)GetCurrentValueDT(source, "lay_IsFloating", false); string oldTargetName = (string)GetCurrentValueDT(source, "lay_FloatTargetName", ""); FloatAlignment oldAlignment = (FloatAlignment)GetCurrentValueDT(source, "lay_FloatAlignment", FloatAlignment.TopLeft);
                _componentChangeServiceDT.OnComponentChanging(source, isFloatingProp); _componentChangeServiceDT.OnComponentChanging(source, targetNameProp); _componentChangeServiceDT.OnComponentChanging(source, alignmentProp);
                isFloatingProp.SetValue(source, true); targetNameProp.SetValue(source, target.Name); alignmentProp.SetValue(source, alignment);
                // Optionally reset offsets
                // TypeDescriptor.GetProperties(source)["lay_FloatOffsetX"]?.SetValue(source, 0); TypeDescriptor.GetProperties(source)["lay_FloatOffsetY"]?.SetValue(source, 0);
                _componentChangeServiceDT.OnComponentChanged(source, isFloatingProp, oldIsFloating, true); _componentChangeServiceDT.OnComponentChanged(source, targetNameProp, oldTargetName, target.Name); _componentChangeServiceDT.OnComponentChanged(source, alignmentProp, oldAlignment, alignment);
                transaction?.Commit(); LayoutLogger.Log($"StackLayoutDT [{this.Name}]: Connection transaction committed."); this.PerformLayout();
            }
            catch (Exception ex) { LayoutLogger.Log($"ERROR ApplyConnectionDT Tx: {ex.Message}"); try { transaction?.Cancel(); } catch { } }
            finally { this.Invalidate(true); } // Ensure redraw
        }

        // Applies property changes for breaking a connection (Uses DesignerTransaction)
        private void BreakConnectionDT(Control source)
        {
            EnsureServicesDT(); if (_componentChangeServiceDT == null || _designerHostDT == null) { LayoutLogger.Log($"ERROR BreakConnectionDT: Services unavailable."); this.Invalidate(true); return; }
            if (source == null) { LayoutLogger.Log($"ERROR BreakConnectionDT: Source null."); return; }
            PropertyDescriptor isFloatingProp = TypeDescriptor.GetProperties(source)["lay_IsFloating"], targetNameProp = TypeDescriptor.GetProperties(source)["lay_FloatTargetName"], alignmentProp = TypeDescriptor.GetProperties(source)["lay_FloatAlignment"];
            if (isFloatingProp == null || targetNameProp == null || alignmentProp == null) { LayoutLogger.Log($"ERROR BreakConnectionDT: Props not found."); return; }
            bool currentIsFloating = (bool)GetCurrentValueDT(source, "lay_IsFloating", false); string currentTargetName = (string)GetCurrentValueDT(source, "lay_FloatTargetName", ""); FloatAlignment currentAlignment = (FloatAlignment)GetCurrentValueDT(source, "lay_FloatAlignment", FloatAlignment.TopLeft);
            if (!currentIsFloating && string.IsNullOrEmpty(currentTargetName)) { LayoutLogger.Log($"Already broken: {source.Name}."); this.Invalidate(true); return; }
            DesignerTransaction transaction = null; try
            {
                transaction = _designerHostDT.CreateTransaction($"Disconnect {source.Name}");
                _componentChangeServiceDT.OnComponentChanging(source, isFloatingProp); _componentChangeServiceDT.OnComponentChanging(source, targetNameProp); _componentChangeServiceDT.OnComponentChanging(source, alignmentProp);
                isFloatingProp.SetValue(source, false); targetNameProp.SetValue(source, ""); alignmentProp.SetValue(source, FloatAlignment.TopLeft); // Default
                                                                                                                                                     // Optionally reset offsets
                                                                                                                                                     // TypeDescriptor.GetProperties(source)["lay_FloatOffsetX"]?.SetValue(source, 0); TypeDescriptor.GetProperties(source)["lay_FloatOffsetY"]?.SetValue(source, 0);
                _componentChangeServiceDT.OnComponentChanged(source, isFloatingProp, currentIsFloating, false); _componentChangeServiceDT.OnComponentChanged(source, targetNameProp, currentTargetName, ""); _componentChangeServiceDT.OnComponentChanged(source, alignmentProp, currentAlignment, FloatAlignment.TopLeft);
                transaction?.Commit(); LayoutLogger.Log($"StackLayoutDT [{this.Name}]: Break connection transaction committed."); this.PerformLayout();
            }
            catch (Exception ex) { LayoutLogger.Log($"ERROR BreakConnectionDT Tx: {ex.Message}"); try { transaction?.Cancel(); } catch { } }
            finally { this.Invalidate(true); } // Ensure redraw
        }

        // Helper to get property value safely using TypeDescriptor
        private object GetCurrentValueDT(Control ctrl, string propName, object defaultValue)
        {
            try { if (ctrl == null) return defaultValue; PropertyDescriptor prop = TypeDescriptor.GetProperties(ctrl)[propName]; return prop?.GetValue(ctrl) ?? defaultValue; }
            catch (Exception) { /* Log Warning Optional */ return defaultValue; }
        }

        #endregion

    } // End partial class StackLayout
} // End namespace