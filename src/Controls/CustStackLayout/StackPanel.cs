using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SharpBrowser.Controls
{
    // --- Enums remain the same ---
    public enum StackOrientation { Vertical, Horizontal }
    public enum StackChildAxisAlignment { Stretch, Start, Center, End }

    public class StackLayout : Panel
    {
        // --- Properties remain the same ---
        private int _spacing = 3;
        private StackOrientation _orientation = StackOrientation.Vertical;
        private StackChildAxisAlignment _childAxisAlignment = StackChildAxisAlignment.Stretch;

        [DefaultValue(3)]
        [Description("The space in pixels between stacked controls.")]
        [Category("Layout2")]
        public int Spacing { get => _spacing; set { if (_spacing != value) { _spacing = value; PerformLayout(); Invalidate(); } } }

        [DefaultValue(StackOrientation.Vertical)]
        [Description("Specifies the direction in which child controls are stacked.")]
        [Category("Layout2")]
        public StackOrientation Orientation { get => _orientation; set { if (_orientation != value) { _orientation = value; PerformLayout(); Invalidate(); } } }

        [DefaultValue(StackChildAxisAlignment.Stretch)]
        [Description("Defines how child controls are aligned and sized perpendicular to the stacking direction. Mimics CSS align-items.")]
        [Category("Layout2")]
        public StackChildAxisAlignment ChildAxisAlignment { get => _childAxisAlignment; set { if (_childAxisAlignment != value) { _childAxisAlignment = value; PerformLayout(); Invalidate(); } } }


        // --- Extender Reference ---
        private StackLayoutExtender _extender;
        private bool _extenderSearched = false;

        private StackLayoutExtender FindExtender()
        {
            // Use Site property which is available
            if (!_extenderSearched && Site != null && Site.Container != null)
            {
                _extenderSearched = true;
                foreach (IComponent component in Site.Container.Components)
                {
                    if (component is StackLayoutExtender foundExtender)
                    {
                        _extender = foundExtender;
                        break; // Found it
                    }
                }
            }
            return _extender;
        }


        // --- Hidden Properties (Anchor, Dock) remain the same ---
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override AnchorStyles Anchor { get => base.Anchor; set => base.Anchor = value; }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override DockStyle Dock { get => base.Dock; set => base.Dock = value; }


        public StackLayout() { /* AutoScroll = true; */ }


        // --- Layout Logic (PerformStackLayout) remains the same as the previous version ---
        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            PerformStackLayout(); // No changes needed inside PerformStackLayout itself
        }

        private void PerformStackLayout()
        {
            StackLayoutExtender extender = FindExtender(); // Relies on Site being set
            var visibleControls = Controls.OfType<Control>().Where(c => c.Visible).ToList();

            // ... (The entire rest of PerformStackLayout is identical to the previous version) ...
            // ... (Includes First Pass, Space Calculation, Integer Rounding, Second Pass, AutoScroll update) ...


            if (visibleControls.Count == 0)
            {
                if (AutoScroll) AutoScrollMinSize = Size.Empty;
                ResumeLayout(false); // Small optimization: don't force layout if empty
                return;
            }

            Rectangle displayRect = DisplayRectangle;
            SuspendLayout();

            // --- Common Variables ---
            double totalWeight = 0;
            int totalPreferredSize = 0;
            int expandingChildCount = 0;
            var weights = new Dictionary<Control, int>();

            // --- First Pass: Calculate sizes and weights ---
            foreach (Control child in visibleControls)
            {
                int weight = extender?.GetExpandWeight(child) ?? 0;
                weights[child] = weight;

                if (weight > 0)
                {
                    totalWeight += weight;
                    expandingChildCount++;
                }
                totalPreferredSize += _orientation == StackOrientation.Vertical ? child.Height : child.Width;
            }

            // --- Calculate Available Space and Space to Distribute ---
            int totalSpacing = visibleControls.Count > 1 ? (visibleControls.Count - 1) * _spacing : 0;
            int totalUsedBeforeExpand = totalPreferredSize + totalSpacing;
            int availableSpace = _orientation == StackOrientation.Vertical ? displayRect.Height : displayRect.Width;
            double spaceToDistribute = Math.Max(0, availableSpace - totalUsedBeforeExpand);

            // --- Handle Integer Rounding ---
            double fractionalSpace = 0.0;
            var extraSpaceMap = new Dictionary<Control, int>();
            if (spaceToDistribute > 0 && totalWeight > 0)
            {
                foreach (Control child in visibleControls)
                {
                    int weight = weights[child];
                    if (weight > 0)
                    {
                        double exactShare = spaceToDistribute * weight / totalWeight;
                        int wholePixels = (int)Math.Floor(exactShare);
                        extraSpaceMap[child] = wholePixels;
                        fractionalSpace += exactShare - wholePixels;
                    }
                    else
                    {
                        // Ensure non-expanding controls are in the map for the second pass
                        extraSpaceMap[child] = 0;
                    }
                }
            }
            else
            {
                // Ensure all controls are in the map even if no space to distribute
                foreach (Control child in visibleControls) extraSpaceMap[child] = 0;
            }

            // Distribute remaining fractional pixels
            int leftoverPixels = (int)Math.Round(fractionalSpace);
            if (leftoverPixels > 0 && expandingChildCount > 0)
            {
                int distributedLeftovers = 0;
                foreach (Control child in visibleControls)
                {
                    if (weights[child] > 0)
                    {
                        extraSpaceMap[child]++;
                        distributedLeftovers++;
                        if (distributedLeftovers >= leftoverPixels) break;
                    }
                }
            }


            // --- Second Pass: Position Controls ---
            int currentPos = _orientation == StackOrientation.Vertical ? displayRect.Top : displayRect.Left;
            int maxCrossAxisSize = 0;

            for (int i = 0; i < visibleControls.Count; i++)
            {
                Control child = visibleControls[i];
                int weight = weights[child];
                int preferredSizeAlongAxis = _orientation == StackOrientation.Vertical ? child.Height : child.Width;
                int calculatedSizeAlongAxis = preferredSizeAlongAxis + extraSpaceMap[child];

                // Apply Min/Max constraints along the orientation axis
                if (_orientation == StackOrientation.Vertical)
                {
                    if (child.MaximumSize.Height > 0 && calculatedSizeAlongAxis > child.MaximumSize.Height) calculatedSizeAlongAxis = child.MaximumSize.Height;
                    if (calculatedSizeAlongAxis < child.MinimumSize.Height) calculatedSizeAlongAxis = child.MinimumSize.Height;
                }
                else
                {
                    if (child.MaximumSize.Width > 0 && calculatedSizeAlongAxis > child.MaximumSize.Width) calculatedSizeAlongAxis = child.MaximumSize.Width;
                    if (calculatedSizeAlongAxis < child.MinimumSize.Width) calculatedSizeAlongAxis = child.MinimumSize.Width;
                }

                // Calculate Cross-Axis Size and Position
                int crossAxisPos, crossAxisSize;
                BoundsSpecified boundsSpec = BoundsSpecified.None;

                if (_orientation == StackOrientation.Vertical)
                {
                    int availableWidth = displayRect.Width;
                    crossAxisPos = displayRect.Left;
                    switch (_childAxisAlignment)
                    {
                        case StackChildAxisAlignment.Stretch:
                            crossAxisSize = availableWidth;
                            if (child.MaximumSize.Width > 0 && crossAxisSize > child.MaximumSize.Width) crossAxisSize = child.MaximumSize.Width;
                            if (crossAxisSize < child.MinimumSize.Width) crossAxisSize = child.MinimumSize.Width;
                            boundsSpec |= BoundsSpecified.Width;
                            break;
                        case StackChildAxisAlignment.Center:
                            crossAxisSize = child.Width; crossAxisPos += (availableWidth - crossAxisSize) / 2; if (crossAxisPos < displayRect.Left) crossAxisPos = displayRect.Left; break;
                        case StackChildAxisAlignment.End:
                            crossAxisSize = child.Width; crossAxisPos += availableWidth - crossAxisSize; if (crossAxisPos < displayRect.Left) crossAxisPos = displayRect.Left; break;
                        case StackChildAxisAlignment.Start: default: crossAxisSize = child.Width; break;
                    }
                    child.SetBounds(crossAxisPos, currentPos, crossAxisSize, calculatedSizeAlongAxis, BoundsSpecified.Location | BoundsSpecified.Height | boundsSpec);
                    maxCrossAxisSize = Math.Max(maxCrossAxisSize, crossAxisSize);
                    currentPos += calculatedSizeAlongAxis;
                }
                else
                { // Horizontal
                    int availableHeight = displayRect.Height;
                    crossAxisPos = displayRect.Top;
                    switch (_childAxisAlignment)
                    {
                        case StackChildAxisAlignment.Stretch:
                            crossAxisSize = availableHeight;
                            if (child.MaximumSize.Height > 0 && crossAxisSize > child.MaximumSize.Height) crossAxisSize = child.MaximumSize.Height;
                            if (crossAxisSize < child.MinimumSize.Height) crossAxisSize = child.MinimumSize.Height;
                            boundsSpec |= BoundsSpecified.Height;
                            break;
                        case StackChildAxisAlignment.Center:
                            crossAxisSize = child.Height; crossAxisPos += (availableHeight - crossAxisSize) / 2; if (crossAxisPos < displayRect.Top) crossAxisPos = displayRect.Top; break;
                        case StackChildAxisAlignment.End:
                            crossAxisSize = child.Height; crossAxisPos += availableHeight - crossAxisSize; if (crossAxisPos < displayRect.Top) crossAxisPos = displayRect.Top; break;
                        case StackChildAxisAlignment.Start: default: crossAxisSize = child.Height; break;
                    }
                    child.SetBounds(currentPos, crossAxisPos, calculatedSizeAlongAxis, crossAxisSize, BoundsSpecified.Location | BoundsSpecified.Width | boundsSpec);
                    maxCrossAxisSize = Math.Max(maxCrossAxisSize, crossAxisSize);
                    currentPos += calculatedSizeAlongAxis;
                }

                if (i < visibleControls.Count - 1) currentPos += _spacing;
            }

            // Update AutoScroll MinSize
            if (AutoScroll)
            {
                if (_orientation == StackOrientation.Vertical)
                {
                    int requiredHeight = currentPos - displayRect.Top + Padding.Bottom;
                    int requiredWidth = _childAxisAlignment == StackChildAxisAlignment.Stretch ? displayRect.Width : maxCrossAxisSize + Padding.Left + Padding.Right;
                    requiredWidth = Math.Max(displayRect.Width, requiredWidth);
                    AutoScrollMinSize = new Size(requiredWidth, requiredHeight);
                }
                else
                { // Horizontal
                    int requiredWidth = currentPos - displayRect.Left + Padding.Right;
                    int requiredHeight = _childAxisAlignment == StackChildAxisAlignment.Stretch ? displayRect.Height : maxCrossAxisSize + Padding.Top + Padding.Bottom;
                    requiredHeight = Math.Max(displayRect.Height, requiredHeight);
                    AutoScrollMinSize = new Size(requiredWidth, requiredHeight);
                }
            }
            else
            {
                AutoScrollMinSize = Size.Empty;
            }

            ResumeLayout(true);

        } // End PerformStackLayout


        // --- Overrides ---
        protected override void OnControlAdded(ControlEventArgs e) { base.OnControlAdded(e); PerformLayout(); }
        protected override void OnControlRemoved(ControlEventArgs e) { base.OnControlRemoved(e); PerformLayout(); }
        protected override void OnPaddingChanged(EventArgs e) { base.OnPaddingChanged(e); PerformLayout(); }


        // --- CORRECTED: Override the Site property setter ---
        [Browsable(false)] // Hide Site property from general view, but allow override
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override ISite Site
        {
            get => base.Site;
            set
            {
                // Let the base class handle setting the site
                base.Site = value;

                // --- Our Logic: Reset extender search when site changes ---
                // This happens when added/removed from designer container
                _extender = null;       // Clear any cached extender reference
                _extenderSearched = false; // Allow searching again next time layout runs
            }
        }
        // --- End Site Override ---

    } // End class StackLayout
}