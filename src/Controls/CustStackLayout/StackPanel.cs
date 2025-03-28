﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
// --- ADD ---
using System.ComponentModel.Design;
// -----------
using System.Diagnostics;
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

        // --- Extender Reference remains the same ---
        private StackLayoutExtender _extender;
        private bool _extenderSearched = false;

        // --- NEW: Reference for Component Change Service ---
        private IComponentChangeService _componentChangeService = null;
        // ---------------------------------------------


        // [Property attributes and getters/setters for Spacing, Orientation, ChildAxisAlignment]
        [DefaultValue(3)][Category("Layout2")] public int Spacing { get => _spacing; set { if (_spacing != value) { _spacing = value; PerformLayout(); Invalidate(); } } }
        [DefaultValue(StackOrientation.Vertical)][Category("Layout2")] public StackOrientation Orientation { get => _orientation; set { if (_orientation != value) { _orientation = value; PerformLayout(); Invalidate(); } } }
        [DefaultValue(StackChildAxisAlignment.Stretch)][Category("Layout2")] public StackChildAxisAlignment ChildAxisAlignment { get => _childAxisAlignment; set { if (_childAxisAlignment != value) { _childAxisAlignment = value; PerformLayout(); Invalidate(); } } }


        private StackLayoutExtender FindExtender()
        {
            if (!_extenderSearched && Site != null && Site.Container != null)
            {
                _extenderSearched = true;
                _extender = Site.Container.Components.OfType<StackLayoutExtender>().FirstOrDefault();
            }
            return _extender;
        }


        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override AnchorStyles Anchor { get => base.Anchor; set => base.Anchor = value; }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override DockStyle Dock { get => base.Dock; set => base.Dock = value; }


        public StackLayout() { /* AutoScroll = true; */ }


        // --- Layout Logic (PerformStackLayout) remains the same ---
        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            // Add a DesignMode check if PerformStackLayout does anything too heavy for constant designer updates
            // if (!DesignMode || IsHandleCreated) // Example guard
            PerformStackLayout();
        }

        private void PerformStackLayout()
        {
            // ... NO CHANGES needed inside PerformStackLayout itself ...
            // It will just be *called* more often now in the designer.
            // The existing logic with Debug lines (if you still have them) is fine.

            StackLayoutExtender extender = FindExtender();
            var visibleControls = this.Controls.OfType<Control>().Where(c => c.Visible).ToList();
            // ... rest of PerformStackLayout ...


            if (visibleControls.Count == 0)
            {
                if (AutoScroll) AutoScrollMinSize = Size.Empty;
                this.ResumeLayout(false);
                return;
            }

            Rectangle displayRect = this.DisplayRectangle;
            this.SuspendLayout();

            double totalWeight = 0; int totalPreferredSize = 0; int expandingChildCount = 0;
            var weights = new Dictionary<Control, int>();
            foreach (Control child in visibleControls)
            {
                int weight = extender?.GetExpandWeight(child) ?? 0; weights[child] = weight;
                if (weight > 0) { totalWeight += weight; expandingChildCount++; }
                totalPreferredSize += (_orientation == StackOrientation.Vertical) ? child.Height : child.Width;
            }

            int totalSpacing = (visibleControls.Count > 1) ? (visibleControls.Count - 1) * _spacing : 0;
            int totalUsedBeforeExpand = totalPreferredSize + totalSpacing;
            int availableSpace = (_orientation == StackOrientation.Vertical) ? displayRect.Height : displayRect.Width;
            double spaceToDistribute = Math.Max(0, availableSpace - totalUsedBeforeExpand);

            double fractionalSpace = 0.0; var extraSpaceMap = new Dictionary<Control, int>();
            if (spaceToDistribute > 0 && totalWeight > 0)
            {
                foreach (Control child in visibleControls)
                {
                    int weight = weights[child]; if (weight > 0)
                    {
                        double exactShare = spaceToDistribute * (double)weight / totalWeight; int wholePixels = (int)Math.Floor(exactShare);
                        extraSpaceMap[child] = wholePixels; fractionalSpace += exactShare - wholePixels;
                    }
                    else { extraSpaceMap[child] = 0; }
                }
            }
            else { foreach (Control child in visibleControls) extraSpaceMap[child] = 0; }

            int leftoverPixels = (int)Math.Round(fractionalSpace);
            if (leftoverPixels > 0 && expandingChildCount > 0)
            {
                int distributedLeftovers = 0; foreach (Control child in visibleControls)
                {
                    if (weights[child] > 0) { extraSpaceMap[child]++; distributedLeftovers++; if (distributedLeftovers >= leftoverPixels) break; }
                }
            }

            int currentPos = (_orientation == StackOrientation.Vertical) ? displayRect.Top : displayRect.Left;
            int maxCrossAxisSize = 0;
            for (int i = 0; i < visibleControls.Count; i++)
            {
                Control child = visibleControls[i]; int weight = weights[child];
                int initialSizeAlongAxis = (_orientation == StackOrientation.Vertical) ? child.Height : child.Width;
                int calculatedSizeAlongAxis = initialSizeAlongAxis + extraSpaceMap[child];

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

                int crossAxisPos, crossAxisSize; BoundsSpecified boundsSpec = BoundsSpecified.None;
                if (_orientation == StackOrientation.Vertical)
                {
                    int availableWidth = displayRect.Width; crossAxisPos = displayRect.Left;
                    switch (_childAxisAlignment)
                    {
                        case StackChildAxisAlignment.Stretch: crossAxisSize = availableWidth; if (child.MaximumSize.Width > 0 && crossAxisSize > child.MaximumSize.Width) crossAxisSize = child.MaximumSize.Width; if (crossAxisSize < child.MinimumSize.Width) crossAxisSize = child.MinimumSize.Width; boundsSpec |= BoundsSpecified.Width; break;
                        case StackChildAxisAlignment.Center: crossAxisSize = child.Width; crossAxisPos += (availableWidth - crossAxisSize) / 2; if (crossAxisPos < displayRect.Left) crossAxisPos = displayRect.Left; break;
                        case StackChildAxisAlignment.End: crossAxisSize = child.Width; crossAxisPos += availableWidth - crossAxisSize; if (crossAxisPos < displayRect.Left) crossAxisPos = displayRect.Left; break;
                        case StackChildAxisAlignment.Start: default: crossAxisSize = child.Width; break;
                    }
                    child.SetBounds(crossAxisPos, currentPos, crossAxisSize, calculatedSizeAlongAxis, BoundsSpecified.Location | BoundsSpecified.Height | boundsSpec);
                    maxCrossAxisSize = Math.Max(maxCrossAxisSize, crossAxisSize); currentPos += calculatedSizeAlongAxis;
                }
                else
                { // Horizontal
                    int availableHeight = displayRect.Height; crossAxisPos = displayRect.Top;
                    switch (_childAxisAlignment)
                    {
                        case StackChildAxisAlignment.Stretch: crossAxisSize = availableHeight; if (child.MaximumSize.Height > 0 && crossAxisSize > child.MaximumSize.Height) crossAxisSize = child.MaximumSize.Height; if (crossAxisSize < child.MinimumSize.Height) crossAxisSize = child.MinimumSize.Height; boundsSpec |= BoundsSpecified.Height; break;
                        case StackChildAxisAlignment.Center: crossAxisSize = child.Height; crossAxisPos += (availableHeight - crossAxisSize) / 2; if (crossAxisPos < displayRect.Top) crossAxisPos = displayRect.Top; break;
                        case StackChildAxisAlignment.End: crossAxisSize = child.Height; crossAxisPos += availableHeight - crossAxisSize; if (crossAxisPos < displayRect.Top) crossAxisPos = displayRect.Top; break;
                        case StackChildAxisAlignment.Start: default: crossAxisSize = child.Height; break;
                    }
                    child.SetBounds(currentPos, crossAxisPos, calculatedSizeAlongAxis, crossAxisSize, BoundsSpecified.Location | BoundsSpecified.Width | boundsSpec);
                    maxCrossAxisSize = Math.Max(maxCrossAxisSize, crossAxisSize); currentPos += calculatedSizeAlongAxis;
                }
                if (i < visibleControls.Count - 1) currentPos += _spacing;
            }

            if (AutoScroll)
            {
                if (_orientation == StackOrientation.Vertical) { int requiredHeight = currentPos - displayRect.Top + Padding.Bottom; int requiredWidth = (_childAxisAlignment == StackChildAxisAlignment.Stretch) ? displayRect.Width : maxCrossAxisSize + Padding.Left + Padding.Right; requiredWidth = Math.Max(displayRect.Width, requiredWidth); this.AutoScrollMinSize = new Size(requiredWidth, requiredHeight); }
                else { int requiredWidth = currentPos - displayRect.Left + Padding.Right; int requiredHeight = (_childAxisAlignment == StackChildAxisAlignment.Stretch) ? displayRect.Height : maxCrossAxisSize + Padding.Top + Padding.Bottom; requiredHeight = Math.Max(displayRect.Height, requiredHeight); this.AutoScrollMinSize = new Size(requiredWidth, requiredHeight); }
            }
            else { this.AutoScrollMinSize = Size.Empty; }

            this.ResumeLayout(true);
        }


        // --- Overrides ---
        protected override void OnControlAdded(ControlEventArgs e) { base.OnControlAdded(e); PerformLayout(); }
        protected override void OnControlRemoved(ControlEventArgs e) { base.OnControlRemoved(e); PerformLayout(); }
        protected override void OnPaddingChanged(EventArgs e) { base.OnPaddingChanged(e); PerformLayout(); }

        // --- MODIFIED Site property setter ---
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override ISite Site
        {
            get => base.Site;
            set
            {
                // Unsubscribe from old service if it exists
                if (_componentChangeService != null)
                {
                    _componentChangeService.ComponentChanged -= OnComponentChanged;
                    _componentChangeService = null; // Release reference
                }

                // Let the base class handle setting the site
                base.Site = value;

                // Reset extender search
                _extender = null;
                _extenderSearched = false;

                // If sited, get the new service and subscribe
                if (base.Site != null)
                {
                    _componentChangeService = (IComponentChangeService)base.Site.GetService(typeof(IComponentChangeService));
                    if (_componentChangeService != null)
                    {
                        _componentChangeService.ComponentChanged += OnComponentChanged;
                        // Perform an initial layout when first sited in designer? Optional.
                        // PerformLayout();
                        // Invalidate(true);
                    }
                }
            }
        }
        // --- End Site Override ---


        // --- NEW: Event Handler for Component Changes ---
        private void OnComponentChanged(object sender, ComponentChangedEventArgs e)
        {
            // Check if the changed component is a direct child of this StackLayout
            if (e.Component is Control changedControl && changedControl.Parent == this)
            {
                // Check if a relevant property changed (more could be added)
                // Check MemberDescriptor name for robustness
                string memberName = e.Member?.Name;
                if (memberName == "Bounds" || memberName == "Size" || memberName == "Location" ||
                   memberName == "Width" || memberName == "Height" || memberName == "Visible")
                {
                    // Debug.WriteLine($"OnComponentChanged: Child '{changedControl.Name}', Member '{memberName}' changed. Performing layout."); // Optional Debug

                    // A child control's size or position changed, trigger layout update
                    // Use BeginInvoke to avoid potential issues with modifying layout
                    // during the change notification itself, especially in the designer.
                    if (this.IsHandleCreated && !this.IsDisposed)
                    {
                        this.BeginInvoke((MethodInvoker)delegate {
                            this.PerformLayout();
                            this.Invalidate(true); // Ensure redraw
                        });
                    }
                    else
                    {
                        // Fallback if handle not created (less likely in designer context but safer)
                        this.PerformLayout();
                        this.Invalidate(true);
                    }
                }
            }
        }
        // --- END NEW Event Handler ---


        // --- Dispose Method - Ensure Unsubscription ---
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Unsubscribe from service if we are subscribed
                if (_componentChangeService != null)
                {
                    _componentChangeService.ComponentChanged -= OnComponentChanged;
                    _componentChangeService = null;
                }
                // (components?.Dispose() if you add other disposable members)
            }
            base.Dispose(disposing);
        }
        // --- END Dispose Method ---


    } // End class StackLayout
}