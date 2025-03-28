using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SharpBrowser.Controls // Your specified namespace
{
    // Orientation Enum (remains the same)
    public enum StackOrientation
    {
        Vertical,
        Horizontal
    }

    // Child Stretch Enum (remains the same)
    public enum StackChildStretch
    {
        Fill,
        Auto
    }

    // --- NEW ENUM for Child Alignment ---
    /// <summary>
    /// Defines how child controls are aligned perpendicular to the StackLayout's orientation
    /// when ChildStretch is set to Auto.
    /// </summary>
    public enum StackChildAlignment
    {
        /// <summary>
        /// Align child controls to the start (Left for Vertical, Top for Horizontal).
        /// </summary>
        Start,
        /// <summary>
        /// Align child controls to the center.
        /// </summary>
        Center,
        /// <summary>
        /// Align child controls to the end (Right for Vertical, Bottom for Horizontal).
        /// </summary>
        End
    }
    // --- END NEW ENUM ---

    public class StackLayout : Panel
    {
        private int _spacing = 3;
        private StackOrientation _orientation = StackOrientation.Vertical;
        private StackChildStretch _childStretch = StackChildStretch.Fill;
        private StackChildAlignment _childAlignment = StackChildAlignment.Start; // Default alignment

        // --- Properties ---
        const string category = "Layout2";

        [DefaultValue(3)]
        [Description("The space in pixels between stacked controls.")]
        [Category(category)]
        public int Spacing
        { /* ... getter/setter ... */
            get { return _spacing; }
            set { if (_spacing != value) { _spacing = value; PerformLayout(); Invalidate(); } }
        }

        [DefaultValue(StackOrientation.Vertical)]
        [Description("Specifies the direction in which child controls are stacked.")]
        [Category(category)]
        public StackOrientation Orientation
        { /* ... getter/setter ... */
            get { return _orientation; }
            set { if (_orientation != value) { _orientation = value; PerformLayout(); Invalidate(); } }
        }

        [DefaultValue(StackChildStretch.Fill)]
        [Description("Specifies how child controls are sized perpendicular to the stacking direction.")]
        [Category(category)]
        public StackChildStretch ChildStretch
        { /* ... getter/setter ... */
            get { return _childStretch; }
            set { if (_childStretch != value) { _childStretch = value; PerformLayout(); Invalidate(); } }
        }

        // --- NEW ChildAlignment PROPERTY ---
        [DefaultValue(StackChildAlignment.Start)]
        [Description("Specifies how child controls are aligned perpendicular to the stacking direction (only applies when ChildStretch is Auto).")]
        [Category(category)]
        public StackChildAlignment ChildAlignment
        {
            get { return _childAlignment; }
            set
            {
                if (_childAlignment != value)
                {
                    _childAlignment = value;
                    // Only need to relayout if stretch is Auto, but relayout anyway for simplicity
                    PerformLayout();
                    Invalidate();
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
            PerformStackLayout();
        }

        private void PerformStackLayout()
        {
            if (this.Controls.Count == 0)
            {
                if (AutoScroll) AutoScrollMinSize = Size.Empty;
                return;
            }

            Rectangle displayRect = this.DisplayRectangle;
            int currentPos = 0;
            bool firstVisibleControl = true;

            this.SuspendLayout();

            if (_orientation == StackOrientation.Vertical)
            {
                // --- Vertical Stacking ---
                currentPos = displayRect.Top;
                int availableWidth = displayRect.Width;
                int maxChildWidth = 0;

                foreach (Control child in this.Controls)
                {
                    if (!child.Visible) continue;
                    if (!firstVisibleControl) currentPos += _spacing; else firstVisibleControl = false;

                    int childWidth;
                    int childX = displayRect.Left; // Start with left edge
                    BoundsSpecified boundsSpec = BoundsSpecified.Location;

                    if (_childStretch == StackChildStretch.Fill)
                    {
                        childWidth = availableWidth;
                        if (child.MaximumSize.Width > 0 && childWidth > child.MaximumSize.Width) childWidth = child.MaximumSize.Width;
                        if (childWidth < child.MinimumSize.Width) childWidth = child.MinimumSize.Width;
                        boundsSpec |= BoundsSpecified.Width;
                    }
                    else // Auto Stretch - Apply Alignment
                    {
                        childWidth = child.Width; // Use child's own width
                        // Calculate X offset based on alignment
                        switch (_childAlignment)
                        {
                            case StackChildAlignment.Center:
                                childX += (availableWidth - childWidth) / 2;
                                break;
                            case StackChildAlignment.End:
                                childX += availableWidth - childWidth;
                                break;
                            case StackChildAlignment.Start:
                            default:
                                // childX remains displayRect.Left
                                break;
                        }
                        // Ensure X is not negative if child is wider than panel
                        if (childX < displayRect.Left) childX = displayRect.Left;
                    }

                    // Set bounds (X, Y, Width, Height)
                    child.SetBounds(childX, currentPos, childWidth, child.Height, boundsSpec);

                    currentPos += child.Height;
                    // Max width calculation should consider the actual width used by the child
                    int effectiveChildWidth = (boundsSpec.HasFlag(BoundsSpecified.Width)) ? childWidth : child.Width;
                    if (effectiveChildWidth > maxChildWidth) maxChildWidth = effectiveChildWidth;
                }

                // Update AutoScrollMinSize for Vertical
                if (AutoScroll)
                {
                    int requiredHeight = currentPos - displayRect.Top + Padding.Bottom;
                    // Required width depends on widest child, adjusted for padding
                    int requiredWidth = maxChildWidth + Padding.Left + Padding.Right;
                    requiredWidth = Math.Max(displayRect.Width, requiredWidth); // Cannot be smaller than panel width if not stretching
                    this.AutoScrollMinSize = new Size(requiredWidth, requiredHeight);
                }
            }
            else // --- Horizontal Stacking ---
            {
                currentPos = displayRect.Left;
                int availableHeight = displayRect.Height;
                int maxChildHeight = 0;

                foreach (Control child in this.Controls)
                {
                    if (!child.Visible) continue;
                    if (!firstVisibleControl) currentPos += _spacing; else firstVisibleControl = false;

                    int childHeight;
                    int childY = displayRect.Top; // Start with top edge
                    BoundsSpecified boundsSpec = BoundsSpecified.Location;

                    if (_childStretch == StackChildStretch.Fill)
                    {
                        childHeight = availableHeight;
                        if (child.MaximumSize.Height > 0 && childHeight > child.MaximumSize.Height) childHeight = child.MaximumSize.Height;
                        if (childHeight < child.MinimumSize.Height) childHeight = child.MinimumSize.Height;
                        boundsSpec |= BoundsSpecified.Height;
                    }
                    else // Auto Stretch - Apply Alignment
                    {
                        childHeight = child.Height; // Use child's own height
                        // Calculate Y offset based on alignment
                        switch (_childAlignment)
                        {
                            case StackChildAlignment.Center:
                                childY += (availableHeight - childHeight) / 2;
                                break;
                            case StackChildAlignment.End:
                                childY += availableHeight - childHeight;
                                break;
                            case StackChildAlignment.Start:
                            default:
                                // childY remains displayRect.Top
                                break;
                        }
                        // Ensure Y is not negative if child is taller than panel
                        if (childY < displayRect.Top) childY = displayRect.Top;
                    }

                    // Set bounds (X, Y, Width, Height)
                    child.SetBounds(currentPos, childY, child.Width, childHeight, boundsSpec);

                    currentPos += child.Width;
                    // Max height calculation should consider the actual height used by the child
                    int effectiveChildHeight = (boundsSpec.HasFlag(BoundsSpecified.Height)) ? childHeight : child.Height;
                    if (effectiveChildHeight > maxChildHeight) maxChildHeight = effectiveChildHeight;
                }

                // Update AutoScrollMinSize for Horizontal
                if (AutoScroll)
                {
                    int requiredWidth = currentPos - displayRect.Left + Padding.Right;
                    // Required height depends on tallest child, adjusted for padding
                    int requiredHeight = maxChildHeight + Padding.Top + Padding.Bottom;
                    requiredHeight = Math.Max(displayRect.Height, requiredHeight); // Cannot be smaller than panel height if not stretching
                    this.AutoScrollMinSize = new Size(requiredWidth, requiredHeight);
                }
            }

            this.ResumeLayout(true);
        }

        // --- Overrides to Trigger Layout ---
        // (Remain the same)
        protected override void OnControlAdded(ControlEventArgs e) { base.OnControlAdded(e); PerformLayout(); }
        protected override void OnControlRemoved(ControlEventArgs e) { base.OnControlRemoved(e); PerformLayout(); }
        protected override void OnPaddingChanged(EventArgs e) { base.OnPaddingChanged(e); PerformLayout(); }

        // ChildControl_Resize handler might still be useful if AutoScroll + AutoStretch + AutoAlignment
        // requires scrollbar updates when a child's size changes. Add if necessary.

    } // End class StackLayout
} // End namespace SharpBrowser.Controls