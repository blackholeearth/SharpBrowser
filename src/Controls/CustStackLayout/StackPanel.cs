using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SharpBrowser.Controls
{
    public enum StackOrientation { Vertical, Horizontal }
    public enum StackChildAxisAlignment { Stretch, Start, Center, End }

    public class StackLayout : Panel
    {
        private int _spacing = 3;
        private StackOrientation _orientation = StackOrientation.Vertical;
        private StackChildAxisAlignment _childAxisAlignment = StackChildAxisAlignment.Stretch;

        private StackLayoutExtender _extender;
        private bool _extenderSearched = false;
        private IComponentChangeService _componentChangeService = null;

        // --- NEW: Layout Flag ---
        private bool _isPerformingLayout = false;
        // ------------------------

        // --- Properties remain the same ---
        [DefaultValue(3)][Category("Layout2")] public int Spacing { get => _spacing; set { if (_spacing != value) { _spacing = value; PerformLayout(); Invalidate(); } } }
        [DefaultValue(StackOrientation.Vertical)][Category("Layout2")] public StackOrientation Orientation { get => _orientation; set { if (_orientation != value) { _orientation = value; PerformLayout(); Invalidate(); } } }
        [DefaultValue(StackChildAxisAlignment.Stretch)][Category("Layout2")] public StackChildAxisAlignment ChildAxisAlignment { get => _childAxisAlignment; set { if (_childAxisAlignment != value) { _childAxisAlignment = value; PerformLayout(); Invalidate(); } } }

        private StackLayoutExtender FindExtender()
        { /* ... same as before ... */
            if (!_extenderSearched && Site != null && Site.Container != null)
            {
                _extenderSearched = true;
                _extender = Site.Container.Components.OfType<StackLayoutExtender>().FirstOrDefault();
            }
            return _extender;
        }

        // --- Hidden Properties remain the same ---
        [Browsable(false)][EditorBrowsable(EditorBrowsableState.Never)][DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] public override AnchorStyles Anchor { get => base.Anchor; set => base.Anchor = value; }
        [Browsable(false)][EditorBrowsable(EditorBrowsableState.Never)][DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] public override DockStyle Dock { get => base.Dock; set => base.Dock = value; }

        public StackLayout() { /* AutoScroll = true; */ }

        protected override void OnLayout(LayoutEventArgs levent) { base.OnLayout(levent); PerformStackLayout(); }

        // --- MODIFIED PerformStackLayout ---
        private void PerformStackLayout()
        {
            // Prevent re-entrancy via ComponentChanged
            if (_isPerformingLayout) return;

            // Set the flag
            _isPerformingLayout = true;
            try // Use finally to ensure the flag is always reset
            {
                // --- Existing PerformStackLayout Logic Starts Here ---
                StackLayoutExtender extender = FindExtender();
                var visibleControls = this.Controls.OfType<Control>().Where(c => c.Visible).ToList();

                if (visibleControls.Count == 0)
                {
                    if (AutoScroll) AutoScrollMinSize = Size.Empty;
                    this.ResumeLayout(false); // Don't suspend/resume if already exiting
                    return; // Exit before SuspendLayout
                }

                Rectangle displayRect = this.DisplayRectangle;
                // SuspendLayout AFTER checking for empty controls
                this.SuspendLayout();

                double totalWeight = 0; int totalPreferredSize = 0; int expandingChildCount = 0;
                var weights = new Dictionary<Control, int>();
                // ... (First Pass: Calculate weights and preferred sizes - NO CHANGES) ...
                foreach (Control child in visibleControls) { int weight = extender?.GetExpandWeight(child) ?? 0; weights[child] = weight; if (weight > 0) { totalWeight += weight; expandingChildCount++; } totalPreferredSize += (_orientation == StackOrientation.Vertical) ? child.Height : child.Width; }


                int totalSpacing = (visibleControls.Count > 1) ? (visibleControls.Count - 1) * _spacing : 0;
                int totalUsedBeforeExpand = totalPreferredSize + totalSpacing;
                int availableSpace = (_orientation == StackOrientation.Vertical) ? displayRect.Height : displayRect.Width;
                double spaceToDistribute = Math.Max(0, availableSpace - totalUsedBeforeExpand);

                // ... (Integer Rounding logic - NO CHANGES) ...
                double fractionalSpace = 0.0; var extraSpaceMap = new Dictionary<Control, int>(); if (spaceToDistribute > 0 && totalWeight > 0) { foreach (Control child in visibleControls) { int weight = weights[child]; if (weight > 0) { double exactShare = spaceToDistribute * (double)weight / totalWeight; int wholePixels = (int)Math.Floor(exactShare); extraSpaceMap[child] = wholePixels; fractionalSpace += exactShare - wholePixels; } else { extraSpaceMap[child] = 0; } } } else { foreach (Control child in visibleControls) extraSpaceMap[child] = 0; }
                int leftoverPixels = (int)Math.Round(fractionalSpace); if (leftoverPixels > 0 && expandingChildCount > 0) { int distributedLeftovers = 0; foreach (Control child in visibleControls) { if (weights[child] > 0) { extraSpaceMap[child]++; distributedLeftovers++; if (distributedLeftovers >= leftoverPixels) break; } } }


                int currentPos = (_orientation == StackOrientation.Vertical) ? displayRect.Top : displayRect.Left;
                int maxCrossAxisSize = 0;
                // ... (Second Pass: Position Controls - calls SetBounds - NO CHANGES) ...
                for (int i = 0; i < visibleControls.Count; i++)
                {
                    Control child = visibleControls[i]; int weight = weights[child];
                    int initialSizeAlongAxis = (_orientation == StackOrientation.Vertical) ? child.Height : child.Width;
                    int calculatedSizeAlongAxis = initialSizeAlongAxis + extraSpaceMap[child];
                    // Apply Min/Max constraints...
                    if (_orientation == StackOrientation.Vertical) { if (child.MaximumSize.Height > 0 && calculatedSizeAlongAxis > child.MaximumSize.Height) calculatedSizeAlongAxis = child.MaximumSize.Height; if (calculatedSizeAlongAxis < child.MinimumSize.Height) calculatedSizeAlongAxis = child.MinimumSize.Height; } else { if (child.MaximumSize.Width > 0 && calculatedSizeAlongAxis > child.MaximumSize.Width) calculatedSizeAlongAxis = child.MaximumSize.Width; if (calculatedSizeAlongAxis < child.MinimumSize.Width) calculatedSizeAlongAxis = child.MinimumSize.Width; }
                    // Calc Cross Axis...
                    int crossAxisPos, crossAxisSize; BoundsSpecified boundsSpec = BoundsSpecified.None;
                    if (_orientation == StackOrientation.Vertical) { /* Vertical cross axis calc */ int availableWidth = displayRect.Width; crossAxisPos = displayRect.Left; switch (_childAxisAlignment) { case StackChildAxisAlignment.Stretch: crossAxisSize = availableWidth; if (child.MaximumSize.Width > 0 && crossAxisSize > child.MaximumSize.Width) crossAxisSize = child.MaximumSize.Width; if (crossAxisSize < child.MinimumSize.Width) crossAxisSize = child.MinimumSize.Width; boundsSpec |= BoundsSpecified.Width; break; case StackChildAxisAlignment.Center: crossAxisSize = child.Width; crossAxisPos += (availableWidth - crossAxisSize) / 2; if (crossAxisPos < displayRect.Left) crossAxisPos = displayRect.Left; break; case StackChildAxisAlignment.End: crossAxisSize = child.Width; crossAxisPos += availableWidth - crossAxisSize; if (crossAxisPos < displayRect.Left) crossAxisPos = displayRect.Left; break; case StackChildAxisAlignment.Start: default: crossAxisSize = child.Width; break; } child.SetBounds(crossAxisPos, currentPos, crossAxisSize, calculatedSizeAlongAxis, BoundsSpecified.Location | BoundsSpecified.Height | boundsSpec); maxCrossAxisSize = Math.Max(maxCrossAxisSize, crossAxisSize); currentPos += calculatedSizeAlongAxis; }
                    else { /* Horizontal cross axis calc */ int availableHeight = displayRect.Height; crossAxisPos = displayRect.Top; switch (_childAxisAlignment) { case StackChildAxisAlignment.Stretch: crossAxisSize = availableHeight; if (child.MaximumSize.Height > 0 && crossAxisSize > child.MaximumSize.Height) crossAxisSize = child.MaximumSize.Height; if (crossAxisSize < child.MinimumSize.Height) crossAxisSize = child.MinimumSize.Height; boundsSpec |= BoundsSpecified.Height; break; case StackChildAxisAlignment.Center: crossAxisSize = child.Height; crossAxisPos += (availableHeight - crossAxisSize) / 2; if (crossAxisPos < displayRect.Top) crossAxisPos = displayRect.Top; break; case StackChildAxisAlignment.End: crossAxisSize = child.Height; crossAxisPos += availableHeight - crossAxisSize; if (crossAxisPos < displayRect.Top) crossAxisPos = displayRect.Top; break; case StackChildAxisAlignment.Start: default: crossAxisSize = child.Height; break; } child.SetBounds(currentPos, crossAxisPos, calculatedSizeAlongAxis, crossAxisSize, BoundsSpecified.Location | BoundsSpecified.Width | boundsSpec); maxCrossAxisSize = Math.Max(maxCrossAxisSize, crossAxisSize); currentPos += calculatedSizeAlongAxis; }
                    if (i < visibleControls.Count - 1) currentPos += _spacing;
                }

                // ... (AutoScroll logic - NO CHANGES) ...
                if (AutoScroll) { /* ... */ } else { this.AutoScrollMinSize = Size.Empty; }

                // ResumeLayout before resetting the flag
                this.ResumeLayout(true);
                // --- Existing PerformStackLayout Logic Ends Here ---
            }
            finally
            {
                // Reset the flag ensure it happens even if an error occurs
                _isPerformingLayout = false;
            }
        }
        // --- END MODIFIED PerformStackLayout ---

        // --- Overrides remain the same ---
        protected override void OnControlAdded(ControlEventArgs e) { base.OnControlAdded(e); PerformLayout(); }
        protected override void OnControlRemoved(ControlEventArgs e) { base.OnControlRemoved(e); PerformLayout(); }
        protected override void OnPaddingChanged(EventArgs e) { base.OnPaddingChanged(e); PerformLayout(); }
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override ISite Site { get => base.Site; set { /* ... same as before, handles service sub/unsub ... */ if (_componentChangeService != null) { _componentChangeService.ComponentChanged -= OnComponentChanged; _componentChangeService = null; } base.Site = value; _extender = null; _extenderSearched = false; if (base.Site != null) { _componentChangeService = (IComponentChangeService)base.Site.GetService(typeof(IComponentChangeService)); if (_componentChangeService != null) { _componentChangeService.ComponentChanged += OnComponentChanged; } } } }

        // --- MODIFIED OnComponentChanged ---
        private void OnComponentChanged(object sender, ComponentChangedEventArgs e)
        {
            // --- ADD Check for the flag ---
            if (_isPerformingLayout)
            {
                // Debug.WriteLine("OnComponentChanged: Ignoring change during layout."); // Optional Debug
                return; // Don't trigger layout if we are already in layout
            }
            // --------------------------------

            if (e.Component is Control changedControl && changedControl.Parent == this)
            {
                string memberName = e.Member?.Name;
                if (memberName == "Bounds" || memberName == "Size" || memberName == "Location" ||
                    memberName == "Width" || memberName == "Height" || memberName == "Visible")
                {
                    // Debug.WriteLine($"OnComponentChanged: Child '{changedControl.Name}', Member '{memberName}' changed. Queuing layout."); // Optional Debug
                    if (this.IsHandleCreated && !this.IsDisposed)
                    {
                        this.BeginInvoke((MethodInvoker)delegate {
                            this.PerformLayout();
                            this.Invalidate(true);
                        });
                    }
                    else
                    {
                        this.PerformLayout();
                        this.Invalidate(true);
                    }
                }
            }
        }
        // --- END MODIFIED ---

        // --- Dispose Method remains the same ---
        protected override void Dispose(bool disposing) { /* ... same as before, unsubscribes from service ... */ if (disposing) { if (_componentChangeService != null) { _componentChangeService.ComponentChanged -= OnComponentChanged; _componentChangeService = null; } } base.Dispose(disposing); }

    } // End class
} // End namespace