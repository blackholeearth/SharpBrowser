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

        public const string categorySTR = "L_ayout2";
        // --- NEW: Layout Flag ---
        private bool _isPerformingLayout = false;
        // ------------------------

        // --- Properties remain the same ---
        [DefaultValue(3)][Category(categorySTR)] public int Spacing { get => _spacing; set { if (_spacing != value) { _spacing = value; PerformLayout(); Invalidate(); } } }
        [DefaultValue(StackOrientation.Vertical)][Category(categorySTR)] public StackOrientation Orientation { get => _orientation; set { if (_orientation != value) { _orientation = value; PerformLayout(); Invalidate(); } } }
        [DefaultValue(StackChildAxisAlignment.Stretch)][Category(categorySTR)] public StackChildAxisAlignment ChildAxisAlignment { get => _childAxisAlignment; set { if (_childAxisAlignment != value) { _childAxisAlignment = value; PerformLayout(); Invalidate(); } } }

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
        private int _performLayout_calcMethod_No = 0;
        [DefaultValue(0)][Category(categorySTR)]
        [Description("Availabe Values: 0 ,1, 4,  \r\n0:is default, Works Nice in Designer")] 
        public int PerformLayout_calcMethod_No
        {
            get => _performLayout_calcMethod_No; 
            set { if (_performLayout_calcMethod_No != value) { _performLayout_calcMethod_No = value; PerformLayout(); Invalidate(); }
            } 
        }
        private void PerformStackLayout()
        {
            if(_performLayout_calcMethod_No ==0)
                PerformStackLayout_old_v0();
            else
            if (_performLayout_calcMethod_No == 1)
                PerformStackLayout_v1();
            else
            if (_performLayout_calcMethod_No == 4)
                PerformStackLayout_v4();
        }

        /// <summary>
        /// WorksNice
        /// </summary>
        private void PerformStackLayout_old_v0()
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
        private void PerformStackLayout_v4()
        {
            if (_isPerformingLayout) return; // Re-entrancy check

            _isPerformingLayout = true;
            try
            {
                StackLayoutExtender extender = FindExtender();
                var visibleControls = this.Controls.OfType<Control>().Where(c => c.Visible).ToList();

                if (visibleControls.Count == 0)
                {
                    if (AutoScroll) AutoScrollMinSize = Size.Empty;
                    this.ResumeLayout(false);
                    return;
                }

                Rectangle displayRect = this.DisplayRectangle;
                this.SuspendLayout();

                // --- NEW Approach #4 Variables ---
                double totalWeight = 0;
                int totalNonExpandingSize = 0; // Size ONLY of non-expanding controls
                int expandingChildCount = 0;
                var weights = new Dictionary<Control, int>();
                var expandingControls = new List<Control>(); // Keep track of expanding controls
                                                             // ---------------------------------

                // --- First Pass (Modified) ---
                // Calculate total size needed by non-expanding controls and total weight of expanding ones.
                // Debug.WriteLine("First Pass (Approach 4):");
                foreach (Control child in visibleControls)
                {
                    int weight = extender?.GetExpandWeight(child) ?? 0;
                    weights[child] = weight;

                    if (weight == 0) // Non-expanding control
                    {
                        int sizeAlongAxis = (_orientation == StackOrientation.Vertical) ? child.Height : child.Width;
                        totalNonExpandingSize += sizeAlongAxis;
                        // Debug.WriteLine($"  Child '{child.Name}': Non-Expanding, SizeAlongAxis={sizeAlongAxis}");
                    }
                    else // Expanding control
                    {
                        totalWeight += weight;
                        expandingChildCount++;
                        expandingControls.Add(child);
                        // Debug.WriteLine($"  Child '{child.Name}': Expanding, Weight={weight}");
                    }
                }
                // Debug.WriteLine($"Total NonExpandingSize: {totalNonExpandingSize}");
                // Debug.WriteLine($"Total Weight: {totalWeight}, Expanding Children: {expandingChildCount}");

                // --- Space Calculation (Modified) ---
                int totalSpacing = (visibleControls.Count > 1) ? (visibleControls.Count - 1) * _spacing : 0;
                int availableSpace = (_orientation == StackOrientation.Vertical) ? displayRect.Height : displayRect.Width;
                // Calculate space left *after* non-expanders and spacing are accounted for
                double spaceAvailableForExpanders = Math.Max(0, availableSpace - totalNonExpandingSize - totalSpacing);

                // Debug.WriteLine($"Total Spacing: {totalSpacing}");
                // Debug.WriteLine($"Available Space Along Axis: {availableSpace}");
                // Debug.WriteLine($"Space Available FOR Expanders: {spaceAvailableForExpanders}");

                // --- Distribution (Modified) ---
                // Distribute the 'spaceAvailableForExpanders' among the expanding controls based on weight.
                double fractionalSpace = 0.0;
                var calculatedExpanderSizes = new Dictionary<Control, int>(); // Store the calculated TOTAL size

                if (spaceAvailableForExpanders > 0 && totalWeight > 0)
                {
                    foreach (Control child in expandingControls) // Iterate only expanding controls
                    {
                        int weight = weights[child];
                        double exactShare = spaceAvailableForExpanders * (double)weight / totalWeight;
                        int wholePixels = (int)Math.Floor(exactShare);
                        calculatedExpanderSizes[child] = wholePixels; // Store the calculated size
                        fractionalSpace += exactShare - wholePixels;
                    }

                    // Distribute remaining fractional pixels for rounding accuracy
                    int leftoverPixels = (int)Math.Round(fractionalSpace);
                    if (leftoverPixels > 0 && expandingChildCount > 0)
                    {
                        int distributedLeftovers = 0;
                        foreach (Control child in expandingControls) // Distribute only among expanders
                        {
                            calculatedExpanderSizes[child]++;
                            distributedLeftovers++;
                            if (distributedLeftovers >= leftoverPixels) break;
                        }
                    }
                }
                else // If no space or no weight, calculated size is 0 (will be constrained by MinSize later)
                {
                    foreach (Control child in expandingControls) calculatedExpanderSizes[child] = 0;
                }

                // Debug.WriteLine("Calculated Expander Sizes (Before Min/Max):");
                // foreach(var kvp in calculatedExpanderSizes) Debug.WriteLine($"  Child '{kvp.Key.Name}': CalcSize={kvp.Value}");


                // --- Second Pass (Modified) ---
                // Position controls using calculated sizes for expanders.
                // Debug.WriteLine("Second Pass:");
                int currentPos = (_orientation == StackOrientation.Vertical) ? displayRect.Top : displayRect.Left;
                int maxCrossAxisSize = 0;

                for (int i = 0; i < visibleControls.Count; i++)
                {
                    Control child = visibleControls[i];
                    int weight = weights[child];
                    int sizeAlongAxis; // The final size to use for this control

                    if (weight == 0) // Non-expanding
                    {
                        sizeAlongAxis = (_orientation == StackOrientation.Vertical) ? child.Height : child.Width;
                        // Min/Max constraints are implicitly handled by the control itself for non-expanders
                    }
                    else // Expanding
                    {
                        sizeAlongAxis = calculatedExpanderSizes[child]; // Get the calculated size

                        // Apply Min/Max constraints to the calculated size
                        if (_orientation == StackOrientation.Vertical)
                        {
                            if (child.MaximumSize.Height > 0 && sizeAlongAxis > child.MaximumSize.Height) sizeAlongAxis = child.MaximumSize.Height;
                            if (sizeAlongAxis < child.MinimumSize.Height) sizeAlongAxis = child.MinimumSize.Height;
                        }
                        else
                        { // Horizontal
                            if (child.MaximumSize.Width > 0 && sizeAlongAxis > child.MaximumSize.Width) sizeAlongAxis = child.MaximumSize.Width;
                            if (sizeAlongAxis < child.MinimumSize.Width) sizeAlongAxis = child.MinimumSize.Width;
                        }
                    }
                    // Debug.WriteLine($"  Processing '{child.Name}': FinalSizeAlongAxis={sizeAlongAxis}");


                    // --- Calculate Cross-Axis Size and Position (NO CHANGES HERE) ---
                    int crossAxisPos, crossAxisSize;
                    BoundsSpecified boundsSpec = BoundsSpecified.None;
                    if (_orientation == StackOrientation.Vertical) { /* Vertical cross axis calc */ int availableWidth = displayRect.Width; crossAxisPos = displayRect.Left; switch (_childAxisAlignment) { case StackChildAxisAlignment.Stretch: crossAxisSize = availableWidth; if (child.MaximumSize.Width > 0 && crossAxisSize > child.MaximumSize.Width) crossAxisSize = child.MaximumSize.Width; if (crossAxisSize < child.MinimumSize.Width) crossAxisSize = child.MinimumSize.Width; boundsSpec |= BoundsSpecified.Width; break; case StackChildAxisAlignment.Center: crossAxisSize = child.Width; crossAxisPos += (availableWidth - crossAxisSize) / 2; if (crossAxisPos < displayRect.Left) crossAxisPos = displayRect.Left; break; case StackChildAxisAlignment.End: crossAxisSize = child.Width; crossAxisPos += availableWidth - crossAxisSize; if (crossAxisPos < displayRect.Left) crossAxisPos = displayRect.Left; break; case StackChildAxisAlignment.Start: default: crossAxisSize = child.Width; break; } child.SetBounds(crossAxisPos, currentPos, crossAxisSize, sizeAlongAxis, BoundsSpecified.Location | BoundsSpecified.Height | boundsSpec); maxCrossAxisSize = Math.Max(maxCrossAxisSize, crossAxisSize); currentPos += sizeAlongAxis; }
                    else { /* Horizontal cross axis calc */ int availableHeight = displayRect.Height; crossAxisPos = displayRect.Top; switch (_childAxisAlignment) { case StackChildAxisAlignment.Stretch: crossAxisSize = availableHeight; if (child.MaximumSize.Height > 0 && crossAxisSize > child.MaximumSize.Height) crossAxisSize = child.MaximumSize.Height; if (crossAxisSize < child.MinimumSize.Height) crossAxisSize = child.MinimumSize.Height; boundsSpec |= BoundsSpecified.Height; break; case StackChildAxisAlignment.Center: crossAxisSize = child.Height; crossAxisPos += (availableHeight - crossAxisSize) / 2; if (crossAxisPos < displayRect.Top) crossAxisPos = displayRect.Top; break; case StackChildAxisAlignment.End: crossAxisSize = child.Height; crossAxisPos += availableHeight - crossAxisSize; if (crossAxisPos < displayRect.Top) crossAxisPos = displayRect.Top; break; case StackChildAxisAlignment.Start: default: crossAxisSize = child.Height; break; } child.SetBounds(currentPos, crossAxisPos, sizeAlongAxis, crossAxisSize, BoundsSpecified.Location | BoundsSpecified.Width | boundsSpec); maxCrossAxisSize = Math.Max(maxCrossAxisSize, crossAxisSize); currentPos += sizeAlongAxis; }
                    // Debug.WriteLine($"    SetBounds: X/Y={...}, W/H={...}"); // Add details if needed

                    // Add spacing
                    if (i < visibleControls.Count - 1) currentPos += _spacing;
                }

                // --- Update AutoScroll MinSize (NO CHANGES NEEDED HERE) ---
                // It uses the final 'currentPos' which reflects the calculated sizes.
                if (AutoScroll) { /* ... same logic ... */ }
                else { this.AutoScrollMinSize = Size.Empty; }

                this.ResumeLayout(true);
            }
            finally
            {
                _isPerformingLayout = false; // Reset the flag
            }
        }
        private void PerformStackLayout_v1()
        {
            if (_isPerformingLayout) return; // Re-entrancy check

            _isPerformingLayout = true;
            try
            {
                StackLayoutExtender extender = FindExtender();
                var visibleControls = this.Controls.OfType<Control>().Where(c => c.Visible).ToList();

                if (visibleControls.Count == 0) { /* ... exit logic ... */ return; }

                Rectangle displayRect = this.DisplayRectangle;
                this.SuspendLayout();

                // --- Variables ---
                double totalWeight = 0;
                int totalBaseSize = 0; // Combined base size of ALL controls
                int expandingChildCount = 0;
                var weights = new Dictionary<Control, int>();
                var baseSizes = new Dictionary<Control, int>(); // Store the determined base size

                // --- First Pass (Revised Base Size Calculation) ---
                // Debug.WriteLine("First Pass (GetPreferredSize for Expanders):");
                foreach (Control child in visibleControls)
                {
                    int weight = extender?.GetExpandWeight(child) ?? 0;
                    weights[child] = weight;
                    int baseSizeAlongAxis;

                    if (weight == 0) // Non-expanding
                    {
                        baseSizeAlongAxis = (_orientation == StackOrientation.Vertical) ? child.Height : child.Width;
                        // Debug.WriteLine($"  Child '{child.Name}': Non-Expanding, BaseSize={baseSizeAlongAxis} (from current size)");
                    }
                    else // Expanding
                    {
                        Size preferredSize = child.GetPreferredSize(Size.Empty); // Get preferred size
                        baseSizeAlongAxis = (_orientation == StackOrientation.Vertical) ? preferredSize.Height : preferredSize.Width;

                        // --- Sanity Check / Minimum Size ---
                        // GetPreferredSize can sometimes be 0 or very small. Ensure a minimum.
                        // Use the control's MinimumSize property if set, otherwise maybe a small default?
                        int minSizeAlongAxis = (_orientation == StackOrientation.Vertical) ? child.MinimumSize.Height : child.MinimumSize.Width;
                        if (minSizeAlongAxis > 0 && baseSizeAlongAxis < minSizeAlongAxis)
                        {
                            baseSizeAlongAxis = minSizeAlongAxis;
                            // Debug.WriteLine($"    Adjusted BaseSize for '{child.Name}' to MinimumSize: {baseSizeAlongAxis}");
                        }
                        // Optional: Add a very small absolute minimum if minSizeAlongAxis is also 0? e.g., Math.Max(1, baseSizeAlongAxis);

                        // Debug.WriteLine($"  Child '{child.Name}': Expanding, Weight={weight}, BaseSize={baseSizeAlongAxis} (from GetPreferredSize/MinimumSize)");

                        totalWeight += weight;
                        expandingChildCount++;
                    }
                    baseSizes[child] = baseSizeAlongAxis; // Store the determined base size
                    totalBaseSize += baseSizeAlongAxis; // Add base size to total
                }
                // Debug.WriteLine($"Total Base Size: {totalBaseSize}");
                // Debug.WriteLine($"Total Weight: {totalWeight}, Expanding Children: {expandingChildCount}");


                // --- Space Calculation ---
                int totalSpacing = (visibleControls.Count > 1) ? (visibleControls.Count - 1) * _spacing : 0;
                int availableSpace = (_orientation == StackOrientation.Vertical) ? displayRect.Height : displayRect.Width;
                // Calculate space left *after* ALL base sizes and spacing are accounted for
                double spaceToDistribute = Math.Max(0, availableSpace - totalBaseSize - totalSpacing);
                // Debug.WriteLine($"Total Spacing: {totalSpacing}");
                // Debug.WriteLine($"Available Space Along Axis: {availableSpace}");
                // Debug.WriteLine($"Space To Distribute (Extra): {spaceToDistribute}");

                // --- Distribution of EXTRA space ---
                double fractionalSpace = 0.0;
                var extraSpaceMap = new Dictionary<Control, int>(); // Store ONLY the extra space

                if (spaceToDistribute > 0 && totalWeight > 0)
                {
                    // Distribute only among expanding controls
                    foreach (var child in visibleControls.Where(c => weights[c] > 0))
                    {
                        int weight = weights[child];
                        double exactShare = spaceToDistribute * (double)weight / totalWeight;
                        int wholePixels = (int)Math.Floor(exactShare);
                        extraSpaceMap[child] = wholePixels; // Store the EXTRA space
                        fractionalSpace += exactShare - wholePixels;
                    }
                    // ... (Rounding logic for fractional pixels - add to extraSpaceMap) ...
                    int leftoverPixels = (int)Math.Round(fractionalSpace); if (leftoverPixels > 0 && expandingChildCount > 0) { int distributedLeftovers = 0; foreach (var child in visibleControls.Where(c => weights[c] > 0)) { if (!extraSpaceMap.ContainsKey(child)) extraSpaceMap[child] = 0; extraSpaceMap[child]++; distributedLeftovers++; if (distributedLeftovers >= leftoverPixels) break; } }
                }
                // Ensure all controls are in the map, even if extra space is 0
                foreach (var child in visibleControls) if (!extraSpaceMap.ContainsKey(child)) extraSpaceMap[child] = 0;

                // Debug.WriteLine("Extra Space Allocation:");
                // foreach(var kvp in extraSpaceMap) Debug.WriteLine($"  Child '{kvp.Key.Name}': Extra={kvp.Value}");


                // --- Second Pass ---
                // Position controls using BaseSize + ExtraSpace for expanders.
                // Debug.WriteLine("Second Pass:");
                int currentPos = (_orientation == StackOrientation.Vertical) ? displayRect.Top : displayRect.Left;
                int maxCrossAxisSize = 0;

                for (int i = 0; i < visibleControls.Count; i++)
                {
                    Control child = visibleControls[i];
                    int weight = weights[child];
                    int baseSizeAlongAxis = baseSizes[child]; // Get the base size determined in pass 1
                    int extraSpace = extraSpaceMap[child];  // Get the distributed extra space
                    int finalSizeAlongAxis = baseSizeAlongAxis + extraSpace; // Calculate final size

                    // Apply Min/Max constraints *to the final calculated size*
                    if (weight > 0) // Only re-apply Min/Max to expanders here if needed, base size already considered MinSize
                    {
                        if (_orientation == StackOrientation.Vertical)
                        {
                            if (child.MaximumSize.Height > 0 && finalSizeAlongAxis > child.MaximumSize.Height) finalSizeAlongAxis = child.MaximumSize.Height;
                            // Minimum was already factored into baseSizeAlongAxis, but check again for safety? Math.Max(finalSizeAlongAxis, baseSizeAlongAxis)
                        }
                        else
                        { // Horizontal
                            if (child.MaximumSize.Width > 0 && finalSizeAlongAxis > child.MaximumSize.Width) finalSizeAlongAxis = child.MaximumSize.Width;
                            // Math.Max(finalSizeAlongAxis, baseSizeAlongAxis)
                        }
                    }
                    // Debug.WriteLine($"  Processing '{child.Name}': Base={baseSizeAlongAxis}, Extra={extraSpace}, FinalSize={finalSizeAlongAxis}");

                    // --- Calculate Cross-Axis Size and Position (NO CHANGES HERE) ---
                    int crossAxisPos, crossAxisSize; BoundsSpecified boundsSpec = BoundsSpecified.None;
                    // ... (same switch (_childAxisAlignment) logic as before) ...
                    if (_orientation == StackOrientation.Vertical) { /* Vertical cross axis calc */ int availableWidth = displayRect.Width; crossAxisPos = displayRect.Left; switch (_childAxisAlignment) { case StackChildAxisAlignment.Stretch: crossAxisSize = availableWidth; /*...*/ boundsSpec |= BoundsSpecified.Width; break; case StackChildAxisAlignment.Center: crossAxisSize = child.Width; /*...*/ break; case StackChildAxisAlignment.End: crossAxisSize = child.Width; /*...*/ break; case StackChildAxisAlignment.Start: default: crossAxisSize = child.Width; break; } child.SetBounds(crossAxisPos, currentPos, crossAxisSize, finalSizeAlongAxis, BoundsSpecified.Location | BoundsSpecified.Height | boundsSpec); maxCrossAxisSize = Math.Max(maxCrossAxisSize, crossAxisSize); currentPos += finalSizeAlongAxis; }
                    else { /* Horizontal cross axis calc */ int availableHeight = displayRect.Height; crossAxisPos = displayRect.Top; switch (_childAxisAlignment) { case StackChildAxisAlignment.Stretch: crossAxisSize = availableHeight; /*...*/ boundsSpec |= BoundsSpecified.Height; break; case StackChildAxisAlignment.Center: crossAxisSize = child.Height; /*...*/ break; case StackChildAxisAlignment.End: crossAxisSize = child.Height; /*...*/ break; case StackChildAxisAlignment.Start: default: crossAxisSize = child.Height; break; } child.SetBounds(currentPos, crossAxisPos, finalSizeAlongAxis, crossAxisSize, BoundsSpecified.Location | BoundsSpecified.Width | boundsSpec); maxCrossAxisSize = Math.Max(maxCrossAxisSize, crossAxisSize); currentPos += finalSizeAlongAxis; }

                    // Add spacing
                    if (i < visibleControls.Count - 1) currentPos += _spacing;
                }

                // --- Update AutoScroll MinSize (NO CHANGES NEEDED HERE) ---
                if (AutoScroll) { /* ... same logic ... */ } else { this.AutoScrollMinSize = Size.Empty; }

                this.ResumeLayout(true);
            }
            finally
            {
                _isPerformingLayout = false; // Reset the flag
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
            return;

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