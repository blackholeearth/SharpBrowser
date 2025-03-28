using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace SharpBrowser.Controls 
{
    public class StackLayout : Panel
    {
        private int _spacing = 3; // Default spacing between controls

        // --- Properties ---

        [DefaultValue(3)]
        [Description("The vertical space in pixels between stacked controls.")]
        [Category("Layout")]
        public int Spacing
        {
            get { return _spacing; }
            set
            {
                if (_spacing != value)
                {
                    _spacing = value;
                    // Trigger a layout recalculation when spacing changes
                    PerformLayout();
                    Invalidate(); // Ensure repaint
                }
            }
        }

        // Optional: Hide properties that don't make sense for StackLayout
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override AnchorStyles Anchor { get => base.Anchor; set => base.Anchor = value; }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override DockStyle Dock { get => base.Dock; set => base.Dock = value; }


        // --- Constructor ---

        public StackLayout()
        {
            // Optional: Set some default styles useful for layout panels
            // AutoScroll = true; // Automatically add scrollbars if content overflows
        }


        // --- Layout Logic ---

        /// <summary>
        /// Overrides the default layout logic to stack controls vertically.
        /// </summary>
        /// <param name="levent">Layout event arguments.</param>
        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent); // Call base class layout
            PerformVerticalStackLayout();
        }

        /// <summary>
        /// Arranges the visible child controls vertically.
        /// </summary>
        private void PerformVerticalStackLayout()
        {
            // Use DisplayRectangle to account for Padding and Scroll position
            Rectangle displayRect = this.DisplayRectangle;
            int currentY = displayRect.Top;
            bool firstVisibleControl = true;

            // Performance optimization: Suspend layout for children during repositioning
            this.SuspendLayout();

            foreach (Control child in this.Controls)
            {
                if (!child.Visible)
                {
                    continue; // Skip invisible controls
                }

                // Add spacing before the control, but not before the very first one
                if (!firstVisibleControl)
                {
                    currentY += _spacing;
                }
                else
                {
                    firstVisibleControl = false;
                }

                // Respect the child's margin (optional, adds complexity)
                // currentY += child.Margin.Top;

                // --- Positioning ---
                // Set the child's location and width. Height is determined by the child itself.
                // We use SetBoundsCore for potentially better performance and to avoid recursive layout calls.
                // Parameters: X, Y, Width, Height, Specified flag (indicates which bounds are being set)
                int childWidth = displayRect.Width; // Stretch width to fill panel (minus padding)

                // Optional: Respect child's Maximum/Minimum Size for Width
                if (child.MaximumSize.Width > 0 && childWidth > child.MaximumSize.Width)
                {
                    childWidth = child.MaximumSize.Width;
                }
                if (childWidth < child.MinimumSize.Width)
                {
                    childWidth = child.MinimumSize.Width;
                }


                // Set the bounds
                // Using child.Bounds = new Rectangle(...) can sometimes trigger extra layout cycles.
                // SetBoundsCore is generally preferred within OnLayout overrides.
                child.SetBounds(
                    displayRect.Left, // + child.Margin.Left (if respecting margins)
                    currentY,
                    childWidth,       // - child.Margin.Left - child.Margin.Right (if respecting margins)
                    child.Height,     // Use the child's current height
                    BoundsSpecified.Location | BoundsSpecified.Width // Specify we are setting Location (X,Y) and Width
                );
                // Note: We let the child control manage its own Height. If you wanted the StackLayout
                // to *force* a height, you'd include BoundsSpecified.Height and calculate it.

                // Update Y for the next control
                currentY += child.Height; // + child.Margin.Bottom (if respecting margins)

            } // End foreach loop

            // Resume layout after all controls are positioned
            this.ResumeLayout(true); // true performs a layout if one was pending

            // --- AutoScroll MinSize ---
            // If AutoScroll is true, update the virtual size needed to contain controls
            if (AutoScroll)
            {
                int requiredHeight = currentY - displayRect.Top + Padding.Bottom; // Calculate total content height
                int requiredWidth = displayRect.Width; // Width is determined by panel width

                // Calculate the minimum size required to display all content without scrolling
                this.AutoScrollMinSize = new Size(requiredWidth, requiredHeight);
            }
        }


        // --- Optional Overrides to Trigger Layout ---

        // Called when a control is added
        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);
            PerformLayout(); // Ensure layout is updated
        }

        // Called when a control is removed
        protected override void OnControlRemoved(ControlEventArgs e)
        {
            base.OnControlRemoved(e);
            // PerformLayout might be needed if removing affects scrollbars, etc.
            // Often OnLayout is triggered automatically, but explicit call is safer.
            PerformLayout();
        }

        // Called when padding changes
        protected override void OnPaddingChanged(EventArgs e)
        {
            base.OnPaddingChanged(e);
            PerformLayout();
        }

        // You might also need to handle child control's Resize or VisibleChanged events
        // if AutoLayout doesn't catch them correctly in all scenarios, but start without
        // this added complexity unless needed. OnLayout called during parent Resize
        // usually handles most cases.

    } // End class StackLayout
} // End namespace
