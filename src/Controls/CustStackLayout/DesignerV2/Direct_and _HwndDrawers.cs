using System;
using System.Drawing;

namespace SharpBrowser.Controls.DesignTime;

/// <summary>
/// Implements IDesignTimeDrawer by drawing directly onto a given Graphics object.
/// </summary>
internal class DirectDrawer : IDesignTimeDrawer
{
    private readonly Graphics _graphics;

    public DirectDrawer(Graphics graphics)
    {
        _graphics = graphics ?? throw new ArgumentNullException(nameof(graphics));
        // Apply desired quality settings
        _graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        _graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        _graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
    }

    public void DrawEllipse(Pen pen, Rectangle rect)
    {
        _graphics.DrawEllipse(pen, rect);
    }

    public void DrawLine(Pen pen, Point p1, Point p2)
    {
        _graphics.DrawLine(pen, p1, p2);
    }

    public void DrawLines(Pen pen, Point[] points)
    {
        _graphics.DrawLines(pen, points);
    }

    public void DrawRectangle(Pen pen, Rectangle rect)
    {
        _graphics.DrawRectangle(pen, rect);
    }

    public void FillEllipse(Brush brush, Rectangle rect)
    {
        _graphics.FillEllipse(brush, rect);
    }
}




/// <summary>
/// Implements IDesignTimeDrawer by forwarding calls to an HwndDrawingHelper,
/// which handles coordinate transformation and drawing onto a target HWND.
/// </summary>
internal class HwndDrawer : IDesignTimeDrawer
{
    private readonly HwndDrawingHelper _helper;

    public HwndDrawer(HwndDrawingHelper helper)
    {
        _helper = helper ?? throw new ArgumentNullException(nameof(helper));
    }

    public void DrawEllipse(Pen pen, Rectangle rect)
    {
        _helper.DrawEllipse(pen, rect); // rect is relative to source panel
    }

    public void DrawLine(Pen pen, Point p1, Point p2)
    {
        _helper.DrawLine(pen, p1, p2); // points are relative to source panel
    }

    public void DrawLines(Pen pen, Point[] points)
    {
        _helper.DrawLines(pen, points); // points are relative to source panel
    }

    public void DrawRectangle(Pen pen, Rectangle rect)
    {
        _helper.DrawRectangle(pen, rect); // rect is relative to source panel
    }

    public void FillEllipse(Brush brush, Rectangle rect)
    {
        _helper.FillEllipse(brush, rect); // rect is relative to source panel
    }
}
