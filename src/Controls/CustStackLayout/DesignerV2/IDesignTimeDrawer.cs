using System.Drawing;

namespace SharpBrowser.Controls.DesignTime
{
    /// <summary>
    /// Interface for abstracting design-time drawing operations,
    /// allowing different underlying drawing mechanisms (Direct Graphics vs. HWND).
    /// </summary>
    internal interface IDesignTimeDrawer
    {
        void DrawLine(Pen pen, Point p1, Point p2);
        void DrawLines(Pen pen, Point[] points);
        void DrawRectangle(Pen pen, Rectangle rect);
        void FillEllipse(Brush brush, Rectangle rect);
        void DrawEllipse(Pen pen, Rectangle rect);
        // Add other methods like DrawString if needed
    }
}