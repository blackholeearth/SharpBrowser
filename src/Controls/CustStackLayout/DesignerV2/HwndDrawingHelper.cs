using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static SharpBrowser.UIAutoPinvoke; // Required for FindForm, Control reference

namespace SharpBrowser.Controls.DesignTime
{
    /// <summary>
    /// WARNING: Experimental helper for drawing design-time visuals directly onto
    /// Visual Studio designer window handles (HWNDs). Highly fragile, likely to break
    /// between VS versions/updates, and fundamentally incompatible with the
    /// .NET 5+ out-of-process designer architecture in many scenarios. Use with extreme caution.
    /// </summary>
    internal class HwndDrawingHelper : IDisposable
    {
        private readonly StackLayout _sourcePanel;
        private IntPtr _targetHwnd = IntPtr.Zero;
        private Graphics _targetGraphics = null;
        private Point _cachedSourcePanelScreenLocation = Point.Empty;

        private static Process _ProcessToDrawOn = null;
        private static IntPtr _cachedDesignerFrameHwnd = IntPtr.Zero;
        private static IEnumerable<nint> _prochandles;

        // Add caching for the form-container HWND if needed

        public HwndDrawingHelper(StackLayout sourcePanel)
        {
            _sourcePanel = sourcePanel ?? throw new ArgumentNullException(nameof(sourcePanel));
        }

        /// <summary>
        /// Attempts to find the target HWND and prepare the Graphics object.
        /// </summary>
        /// <returns>True if successful, false otherwise.</returns>
        public bool BeginDraw()
        {
            if (_targetGraphics != null)
            {
                LayoutLogger.Log("HwndDrawingHelper WARN: BeginDraw called while already drawing. Disposing previous Graphics.");
                EndDraw(); // Dispose previous graphics just in case
            }

            _targetHwnd = FindTargetWindow();
            if (_targetHwnd == IntPtr.Zero)
            {
                LayoutLogger.Log("HwndDrawingHelper ERROR: Could not find target HWND ('DesignerFrame'). Drawing aborted.");
                return false;
            }

        
            
            try
            {
                //Task.Run(delegate
                //{
                //    //Thread.Sleep(500);
                //    //_targetGraphics = Graphics.FromHwnd(_targetHwnd);

                //    for (int j = 0; j < 4; j++)
                //    {
                //        Thread.Sleep(200);
                //        _targetGraphics = Graphics.FromHwnd(_targetHwnd);
                //        for (int i = 0; i < 1920; i = i + 50)
                //        {
                //            // TEST DRAW: Draw a bright rectangle near the top-left of the target HWND
                //            _targetGraphics.FillRectangle(Brushes.Lime, i, i, 30, 30);
                //            //LayoutLogger.Log($"HwndDrawingHelper: TEST DRAW executed on HWND {_targetHwnd}.");
                //        }

                //        // Get target window's client rectangle coordinates (relative to itself)
                //        if (!NativeMethods.GetClientRect(_targetHwnd, out NativeMethods.RECT clientRect))
                //        {
                //            LayoutLogger.Log($"DrawTestPatternOnHwnd, Failed to get ClientRect for HWND {_targetHwnd}.");
                //            return;
                //        }
                //        LayoutLogger.Log($"DrawTestPatternOnHwnd, Target HWND {_targetHwnd} ClientRect: W={clientRect.Width}, H={clientRect.Height}");
                //        _targetGraphics.FillRectangle(Brushes.LimeGreen, 
                //            clientRect.Top+5, clientRect.Left, 
                //            clientRect.Width+5, clientRect.Height);

                //    }


                //});


                // Cache the StackLayout's current screen position for coordinate conversion
                // Get this *after* finding the HWND, as the layout might have shifted
                _cachedSourcePanelScreenLocation = _sourcePanel.PointToScreen(Point.Empty);
                LayoutLogger.Log($"HwndDrawingHelper: Obtained Graphics for HWND {_targetHwnd}. Source Panel Screen Pos: {_cachedSourcePanelScreenLocation}");
                return true;
            }
            catch (Exception ex)
            {
                LayoutLogger.Log($"HwndDrawingHelper ERROR: Failed to get Graphics from HWND {_targetHwnd}. Exception: {ex.Message}");
                _targetHwnd = IntPtr.Zero;
                _targetGraphics = null;
                return false;
            }
        }

        /// <summary>
        /// Releases the Graphics object.
        /// </summary>
        public void EndDraw()
        {
            _targetGraphics?.Dispose();
            _targetGraphics = null;
            _targetHwnd = IntPtr.Zero; // Reset HWND after finishing drawing
            // LayoutLogger.Log("HwndDrawingHelper: EndDraw called, Graphics disposed."); // Can be noisy
        }

        /// <summary>
        ///  nice!!! this one doesnt freeze target process ... when calling gettext...
        /// unfortunately my custom lib freezes  UIAutoPInvoke.gethandle_bytext freezes  :(( 
        /// might copy this one into my lib future
        /// </summary>
        /// <returns></returns>
        // MODIFIED: Searches all windows, then checks visibility of the result.
        public static IntPtr FindTargetWindow()
        {
            //var procName = "DesignToolsServer";
            //var procWinCaption = "DesignerFrame";
            var procName = "devenv";
            var procWinCaption = "WinForms Window Surface";
            //var procWinCaption = "DesignerView"; 
            //var procWinCaption = "[Design]";  //worked with task..


            // 1. Find DesignToolsServer.exe Process (Cache it)
            if (_ProcessToDrawOn == null || _ProcessToDrawOn.HasExited)
            {
                var processes = Process.GetProcessesByName(procName);
                if (processes.Length == 0)
                {
                    LayoutLogger.Log($"HwndDrawingHelper ERROR: {procName} process not found.");
                    return IntPtr.Zero;
                }
                _ProcessToDrawOn = processes[0];
                _cachedDesignerFrameHwnd = IntPtr.Zero; // Reset cache
                LayoutLogger.Log($"HwndDrawingHelper: Found {procName} process (PID: {_ProcessToDrawOn.Id}).");
            }

            // 2. Use Cached Handle if available (no visibility check here anymore)
            if (_cachedDesignerFrameHwnd != IntPtr.Zero)
            {
                // LayoutLogger.Log($"HwndDrawingHelper: Using cached DesignerFrame HWND: {_cachedDesignerFrameHwnd}"); // Noisy
                // We will check visibility *after* returning from cache or finding it below
            }
            else // Cache miss or invalidated, perform search
            {
                LayoutLogger.Log($"HwndDrawingHelper: Searching for {procWinCaption} HWND (ignoring visibility during search)...");
                IntPtr foundHwnd = IntPtr.Zero;
                GCHandle searchDataHandle = default;

                try
                {
                    // Search data no longer needs the visibility flag
                    var searchData = new WindowSearchData
                    {
                        ProcessId = (uint)_ProcessToDrawOn.Id,
                        TargetCaption = procWinCaption,
                        // EnsureIsVisible removed
                    };
                    searchDataHandle = GCHandle.Alloc(searchData);

                    // 3. Enumerate top-level windows
                    NativeMethods.EnumWindows((hWnd, lParam) =>
                    {
                        var data = (WindowSearchData)((GCHandle)lParam).Target;
                        NativeMethods.GetWindowThreadProcessId(hWnd, out uint windowPid);

                        // Check Process ID only
                        if (windowPid == data.ProcessId)
                        {
                            // Search children of this process's window for the target caption
                            IntPtr childHwnd = FindChildWindowByCaptionRecursive(hWnd, data.TargetCaption , _0exact_1contains:1);
                            if (childHwnd != IntPtr.Zero)
                            {
                                data.ResultHwnd = childHwnd;
                                return false; // Stop enumeration, window found
                            }
                        }
                        return true; // Continue enumeration
                    }, (IntPtr)searchDataHandle);

                    foundHwnd = searchData.ResultHwnd;
                }
                finally
                {
                    if (searchDataHandle.IsAllocated) searchDataHandle.Free();
                }

                // Cache the result regardless of visibility
                _cachedDesignerFrameHwnd = foundHwnd;

                if (foundHwnd != IntPtr.Zero)
                {
                    LayoutLogger.Log($"HwndDrawingHelper: Found {procWinCaption} HWND: {foundHwnd}. Caching.");
                }
                else
                {
                    LayoutLogger.Log($"HwndDrawingHelper ERROR: {procWinCaption} HWND not found within {procName} process.");
                }
            } // End if (cache miss)


            // 4. Perform the visibility check *after* finding/retrieving the HWND
            if (_cachedDesignerFrameHwnd != IntPtr.Zero)
            {
                bool isVisible = NativeMethods.IsWindowVisible(_cachedDesignerFrameHwnd);
                LayoutLogger.Log($"HwndDrawingHelper: Target HWND {_cachedDesignerFrameHwnd} IsVisible = {isVisible}.");
            }
            // Else: HWND not found, logged earlier.

            // 5. Return the cached/found HWND, visible or not
            return _cachedDesignerFrameHwnd;
        }

        // FindChildWindowByCaptionRecursive remains unchanged (doesn't check visibility)
        private static IntPtr FindChildWindowByCaptionRecursive(IntPtr hwndParent, string targetCaption, int _0exact_1contains = 0)
        {
            var funcExact = (string sbstr, string curcap) =>  sbstr == curcap; 
            var funcContains = (string sbstr, string curcap) => sbstr.Contains(curcap); 

            var funcSelected = _0exact_1contains == 0? funcExact : funcContains;

            IntPtr resultHwnd = IntPtr.Zero;
            GCHandle searchCaptionHandle = default;
            try
            {
                searchCaptionHandle = GCHandle.Alloc(targetCaption);
                NativeMethods.EnumChildWindows(hwndParent, (hWnd, lParam) =>
                {
                    string currentCaption = (string)((GCHandle)lParam).Target;
                    int len = NativeMethods.GetWindowTextLength(hWnd);
                    if (len > 0)
                    {
                        StringBuilder sb = new StringBuilder(len + 1);
                        NativeMethods.GetWindowText(hWnd, sb, sb.Capacity);

                        if (funcSelected(sb.ToString(),currentCaption))
                        {
                            LayoutLogger.Log("foundWindow GetText:" + sb.ToString());
                            resultHwnd = hWnd;
                            return false; // Stop searching children
                        }
                    }
                    return true; // Continue searching siblings
                }, (IntPtr)searchCaptionHandle);
            }
            finally { if (searchCaptionHandle.IsAllocated) searchCaptionHandle.Free(); }
            return resultHwnd;
        }

        // WindowSearchData class no longer needs EnsureIsVisible field
        private class WindowSearchData
        {
            public uint ProcessId;
            public string TargetCaption;
            // EnsureIsVisible removed
            public IntPtr ResultHwnd = IntPtr.Zero;
        }







        // --- Coordinate Transformation ---

        /// <summary>
        /// Converts a point from StackLayout client coordinates to the Target HWND's client coordinates.
        /// </summary>
        private bool TryTransformPointToTarget(Point sourcePanelClientPoint, out Point targetHwndClientPoint)
        {
            targetHwndClientPoint = Point.Empty;
            if (_targetHwnd == IntPtr.Zero || _sourcePanel == null || !_sourcePanel.IsHandleCreated)
            {
                return false;
            }

            try
            {
                // 1. StackLayout Client -> Screen
                Point screenPoint = _sourcePanel.PointToScreen(sourcePanelClientPoint);

                // 2. Screen -> Target HWND Client
                NativeMethods.Point targetPointNative = screenPoint; // Implicit conversion
                if (NativeMethods.ScreenToClient(_targetHwnd, ref targetPointNative))
                {
                    targetHwndClientPoint = targetPointNative; // Implicit conversion
                    return true;
                }
                else
                {
                    LayoutLogger.Log("HwndDrawingHelper WARN: ScreenToClient failed.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                // PointToScreen can fail if handle is destroyed during operation
                LayoutLogger.Log($"HwndDrawingHelper WARN: Coordinate transformation failed: {ex.Message}");
                return false;
            }
        }


        /// <summary>
        /// Converts a rectangle from StackLayout client coordinates to the Target HWND's client coordinates.
        /// </summary>
        private bool TryTransformRectToTarget(Rectangle sourcePanelClientRect, out Rectangle targetHwndClientRect)
        {
            targetHwndClientRect = Rectangle.Empty;

            // Transform Top-Left and Bottom-Right points
            if (TryTransformPointToTarget(sourcePanelClientRect.Location, out Point targetTopLeft) &&
                TryTransformPointToTarget(new Point(sourcePanelClientRect.Right, sourcePanelClientRect.Bottom), out Point targetBottomRight))
            {
                targetHwndClientRect = Rectangle.FromLTRB(targetTopLeft.X, targetTopLeft.Y, targetBottomRight.X, targetBottomRight.Y);
                return true;
            }
            return false;
        }


        // --- Drawing Methods ---
        // These methods take coordinates relative to the _sourcePanel

        public void DrawLine(Pen pen, Point pt1Source, Point pt2Source)
        {
            if (_targetGraphics == null) return;
            if (TryTransformPointToTarget(pt1Source, out Point pt1Target) &&
                TryTransformPointToTarget(pt2Source, out Point pt2Target))
            {
                // LayoutLogger.Log($"HwndDraw: Line ({pt1Source} -> {pt2Source}) => ({pt1Target} -> {pt2Target})"); // Noisy
                _targetGraphics.DrawLine(pen, pt1Target, pt2Target);
            }
        }
        public void DrawLines(Pen pen, Point[] pointsSource)
        {
            if (_targetGraphics == null || pointsSource == null || pointsSource.Length < 2) return;

            Point[] pointsTarget = new Point[pointsSource.Length];
            for (int i = 0; i < pointsSource.Length; i++)
            {
                if (!TryTransformPointToTarget(pointsSource[i], out pointsTarget[i]))
                {
                    // LayoutLogger.Log($"HwndDraw: Lines - Failed to transform point {i}"); // Noisy
                    return; // Abort drawing if any point fails transformation
                }
            }
            // LayoutLogger.Log($"HwndDraw: Lines ({pointsSource.Length} pts)"); // Noisy
            _targetGraphics.DrawLines(pen, pointsTarget);
        }


        public void DrawRectangle(Pen pen, Rectangle rectSource)
        {
            if (_targetGraphics == null) return;
            if (TryTransformRectToTarget(rectSource, out Rectangle rectTarget))
            {
                _targetGraphics.DrawRectangle(pen, rectTarget);
            }
        }

        public void FillEllipse(Brush brush, Rectangle rectSource)
        {
            if (_targetGraphics == null) return;
            if (TryTransformRectToTarget(rectSource, out Rectangle rectTarget))
            {
                // LayoutLogger.Log($"HwndDraw: Ellipse Fill ({rectSource}) => ({rectTarget})"); // Noisy
                _targetGraphics.FillEllipse(brush, rectTarget);
            }
        }
        public void DrawEllipse(Pen pen, Rectangle rectSource)
        {
            if (_targetGraphics == null) return;
            if (TryTransformRectToTarget(rectSource, out Rectangle rectTarget))
            {
                // LayoutLogger.Log($"HwndDraw: Ellipse Draw ({rectSource}) => ({rectTarget})"); // Noisy
                _targetGraphics.DrawEllipse(pen, rectTarget);
            }
        }

        // Add other drawing methods (DrawString, etc.) as needed,
        // ensuring they also call TryTransformPointToTarget or TryTransformRectToTarget.

        public void Dispose()
        {
            EndDraw();
            // Static process reference doesn't need explicit disposal here,
            // but clear it if the helper instance is definitively finished.
            // _ProcessToDrawOn = null;
            // _cachedDesignerFrameHwnd = IntPtr.Zero;
        }
    }
}


/*
 usage:

class Program
{
    [DllImport("user32.dll", EntryPoint = "FindWindowEx")]
    public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

    static void Main(string[] args)
    {
        Process[] anotherApps = Process.GetProcessesByName("AnotherApp");
        if (anotherApps.Length == 0) return;
        if (anotherApps[0] != null)
        {
            var allChildWindows = new WindowHandleInfo(anotherApps[0].MainWindowHandle).GetAllChildHandles();
        }
    }
}
 */
//  https://stackoverflow.com/questions/1363167/how-can-i-get-the-child-windows-of-a-window-given-its-hwnd?noredirect=1&lq=1
public class WindowHandleInfo
{
    /// <summary>
    /// do not write ".exe"  for procesName
    /// </summary>
    /// <param name="processName"></param>
    public static List<nint> GetAllChildWindowsbyProcessName(string processName ) 
    {
        Process[] anotherApps = Process.GetProcessesByName(processName);
        if (anotherApps.Length == 0) 
            return null;

        if (anotherApps[0] != null)
        {
            var allChildWindows = new WindowHandleInfo(anotherApps[0].MainWindowHandle).GetAllChildHandles();
            return allChildWindows;
        }
        return null;
    }

    private delegate bool EnumWindowProc(IntPtr hwnd, IntPtr lParam);

    [DllImport("user32")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EnumChildWindows(IntPtr window, EnumWindowProc callback, IntPtr lParam);

    private IntPtr _MainHandle;

    public WindowHandleInfo(IntPtr handle)
    {
        this._MainHandle = handle;
    }

    public List<IntPtr> GetAllChildHandles()
    {
        List<IntPtr> childHandles = new List<IntPtr>();

        GCHandle gcChildhandlesList = GCHandle.Alloc(childHandles);
        IntPtr pointerChildHandlesList = GCHandle.ToIntPtr(gcChildhandlesList);

        try
        {
            EnumWindowProc childProc = new EnumWindowProc(EnumWindow);
            EnumChildWindows(this._MainHandle, childProc, pointerChildHandlesList);
        }
        finally
        {
            gcChildhandlesList.Free();
        }

        return childHandles;
    }

    private bool EnumWindow(IntPtr hWnd, IntPtr lParam)
    {
        GCHandle gcChildhandlesList = GCHandle.FromIntPtr(lParam);

        if (gcChildhandlesList == null || gcChildhandlesList.Target == null)
        {
            return false;
        }

        List<IntPtr> childHandles = gcChildhandlesList.Target as List<IntPtr>;
        childHandles.Add(hWnd);

        return true;
    }
}