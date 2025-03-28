using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;


using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SharpBrowser.Controls // Your specified namespace
{
    /// <summary>
    /// Defines the orientation for stacking controls within the StackLayout.
    /// </summary>
    public enum StackOrientation
    {
        Vertical,
        Horizontal
    }

    public class StackLayout : Panel
    {
        private int _spacing = 3;
        private StackOrientation _orientation = StackOrientation.Vertical; // Default orientation

        // --- Properties ---

        [DefaultValue(3)]
        [Description("The space in pixels between stacked controls.")]
        [Category("Layout")]
        public int Spacing
        {
            get { return _spacing; }
            set
            {
                if (_spacing != value)
                {
                    _spacing = value;
                    PerformLayout();
                    Invalidate();
                }
            }
        }

        // --- NEW ORIENTATION PROPERTY ---
        [DefaultValue(StackOrientation.Vertical)]
        [Description("Specifies the direction in which child controls are stacked.")]
        [Category("Layout")]
        public StackOrientation Orientation
        {
            get { return _orientation; }
            set
            {
                if (_orientation != value)
                {
                    _orientation = value;
                    PerformLayout(); // Trigger layout recalculation when orientation changes
                    Invalidate(); // Ensure repaint
                }
            }
        }
        // --- END NEW PROPERTY ---


        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override AnchorStyles Anchor { get => base.Anchor; set => base.Anchor = value; }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override DockStyle Dock { get => base.Dock; set => base.Dock = value; }


        public StackLayout()
        {
            // AutoScroll = true; // Enable if needed
        }


        // --- Layout Logic ---

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            PerformStackLayout(); // Call the updated layout method
        }

        /// <summary>
        /// Arranges the visible child controls based on the Orientation property.
        /// </summary>
        private void PerformStackLayout()
        {
            if (this.Controls.Count == 0)
            {
                if (AutoScroll) AutoScrollMinSize = Size.Empty; // Reset scroll size if empty
                return;
            }

            Rectangle displayRect = this.DisplayRectangle;
            int currentPos = 0; // Will be Y for Vertical, X for Horizontal
            bool firstVisibleControl = true;

            this.SuspendLayout();

            if (_orientation == StackOrientation.Vertical)
            {
                // --- Vertical Stacking Logic ---
                currentPos = displayRect.Top;
                int availableWidth = displayRect.Width;

                foreach (Control child in this.Controls)
                {
                    if (!child.Visible) continue;

                    if (!firstVisibleControl) currentPos += _spacing; else firstVisibleControl = false;

                    int childWidth = availableWidth; // Stretch width

                    // Optional: Respect child's Max/Min Width
                    if (child.MaximumSize.Width > 0 && childWidth > child.MaximumSize.Width) childWidth = child.MaximumSize.Width;
                    if (childWidth < child.MinimumSize.Width) childWidth = child.MinimumSize.Width;

                    // Set bounds (X, Y, Width, Height)
                    child.SetBounds(displayRect.Left, currentPos, childWidth, child.Height, BoundsSpecified.Location | BoundsSpecified.Width);

                    currentPos += child.Height;
                }

                // Update AutoScrollMinSize for Vertical
                if (AutoScroll)
                {
                    int requiredHeight = currentPos - displayRect.Top + Padding.Bottom;
                    int requiredWidth = availableWidth; // Usually panel width for vertical
                    this.AutoScrollMinSize = new Size(requiredWidth, requiredHeight);
                }
            }
            else // --- Horizontal Stacking Logic ---
            {
                currentPos = displayRect.Left;
                int availableHeight = displayRect.Height;

                foreach (Control child in this.Controls)
                {
                    if (!child.Visible) continue;

                    if (!firstVisibleControl) currentPos += _spacing; else firstVisibleControl = false;

                    int childHeight = availableHeight; // Stretch height

                    // Optional: Respect child's Max/Min Height
                    if (child.MaximumSize.Height > 0 && childHeight > child.MaximumSize.Height) childHeight = child.MaximumSize.Height;
                    if (childHeight < child.MinimumSize.Height) childHeight = child.MinimumSize.Height;


                    // Set bounds (X, Y, Width, Height)
                    child.SetBounds(currentPos, displayRect.Top, child.Width, childHeight, BoundsSpecified.Location | BoundsSpecified.Height);

                    currentPos += child.Width;
                }

                // Update AutoScrollMinSize for Horizontal
                if (AutoScroll)
                {
                    int requiredWidth = currentPos - displayRect.Left + Padding.Right;
                    int requiredHeight = availableHeight; // Usually panel height for horizontal
                    this.AutoScrollMinSize = new Size(requiredWidth, requiredHeight);
                }
            }


            this.ResumeLayout(true);
        }


        // --- Optional Overrides to Trigger Layout ---
        // (These remain the same)
        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);
            // Add listener for child resize ONLY IF we need to react to it in horizontal mode
            // e.Control.Resize += ChildControl_Resize;
            PerformLayout();
        }

        protected override void OnControlRemoved(ControlEventArgs e)
        {
            base.OnControlRemoved(e);
            // Remove listener if added
            // e.Control.Resize -= ChildControl_Resize;
            PerformLayout();
        }

        protected override void OnPaddingChanged(EventArgs e)
        {
            base.OnPaddingChanged(e);
            PerformLayout();
        }

        /* // Example handler if needed for Horizontal layout reacting to child width changes
           // Generally not needed if child controls manage their own Width correctly.
        private void ChildControl_Resize(object sender, EventArgs e)
        {
            // Only trigger layout if orientation is Horizontal AND the sender is a direct child
            Control child = sender as Control;
            if (_orientation == StackOrientation.Horizontal && child != null && child.Parent == this)
            {
                // Avoid potential recursive loops if possible, though PerforLayout should handle it
                PerformLayout();
            }
        }
        */

    } // End class StackLayout
} // End namespace SharpBrowser.Controls