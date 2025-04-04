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

    public enum DesignTimeDrawingMethod
    {
        /// <summary>
        /// Draws adorners directly onto the StackLayout panel's Graphics context (can be obscured by child controls).
        /// </summary>
        Direct,
        /// <summary>
        /// EXPERIMENTAL: Attempts to draw adorners onto the Visual Studio designer frame's HWND (may draw over children, but fragile and prone to failure/flickering).
        /// </summary>
        HwndExperimental
    }

  
    // The other part of StackLayout - focused ONLY on Design-Time behavior
    public partial class StackLayout
    {

        // --- Add this Property (e.g., in the Public Layout Properties region) ---
        private DesignTimeDrawingMethod _designTimeDrawingMethod = DesignTimeDrawingMethod.Direct; // Default to direct

        [DefaultValue(DesignTimeDrawingMethod.Direct)]
        [Category(categorySTR)] // Assuming categorySTR is defined
        [Description("Specifies the method used to draw design-time adorners like connection points and lines. 'Direct' draws on the panel (safer but can be obscured). 'HwndExperimental' attempts to draw on the VS designer surface (fragile).")]
        public DesignTimeDrawingMethod lay__DesignTimeDrawingMethod
        {
            get => _designTimeDrawingMethod;
            set
            {
                if (_designTimeDrawingMethod != value)
                {
                    _designTimeDrawingMethod = value;
                    // Changing drawing method requires a full repaint
                    if (this.IsHandleCreated && !this.IsDisposed && this.DesignMode)
                    {
                        this.Invalidate(true);
                    }
                }
            }
        }
    }


} // End namespace