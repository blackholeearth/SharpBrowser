using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SharpBrowser.Controls // Your namespace
{
    // Enums remain the same
    public enum StackOrientation { Vertical, Horizontal }

    //Stretch is Makes it act as Standart StackLayout.
    public enum StackChildAxisAlignment { Stretch, Start, Center, End }

    /// <summary>
    /// A panel that arranges its child controls in a single line either horizontally or vertically,
    /// with support for spacing, alignment, and weighted expansion of children.
    /// </summary>
    public class StackLayout : Panel
    {
        // --- Constants ---
        public const string categorySTR = "L_Layout2"; // Public constant for category name

        // --- Private Fields ---
        private int _spacing = 3;
        private StackOrientation _orientation = StackOrientation.Vertical;
        private StackChildAxisAlignment _childAxisAlignment = StackChildAxisAlignment.Stretch;
        private int _performLayout_calcMethod_No = 0; // Default to method 0

        private StackLayoutExtender _extender;
        private bool _extenderSearched = false;
        private IComponentChangeService _componentChangeService = null;
        private bool _isPerformingLayout = false;


        // --- Public Properties (Prefixed and Categorized) ---
        // Inside StackLayout.cs
        [Browsable(true)] // Make it visible in the designer
        [Category(categorySTR)] // Put it in our category
        [Description("Manually assigns the StackLayoutExtender instance providing layout weights for children.   -do it in form_load")]
        public StackLayoutExtender LayoutExtenderProvider { get; set; }
        private StackLayoutExtender FindExtender() => LayoutExtenderProvider;


        [DefaultValue(3)]
        [Description("The space in pixels between stacked controls.")]
        [Category(categorySTR)]
        public int lay_Spacing
        {
            get => _spacing;
            set { if (_spacing != value) { _spacing = value; PerformLayout(); Invalidate(); } }
        }

        [DefaultValue(StackOrientation.Vertical)]
        [Description("Specifies the direction in which child controls are stacked.")]
        [Category(categorySTR)]
        public StackOrientation lay_Orientation
        {
            get => _orientation;
            set { if (_orientation != value) { _orientation = value; PerformLayout(); Invalidate(); } }
        }

        [DefaultValue(StackChildAxisAlignment.Stretch)]
        [Description("Defines how child controls are aligned and sized perpendicular to the stacking direction. Mimics CSS align-items.")]
        [Category(categorySTR)]
        public StackChildAxisAlignment lay_ChildAxisAlignment
        {
            get => _childAxisAlignment;
            set { if (_childAxisAlignment != value) { _childAxisAlignment = value; PerformLayout(); Invalidate(); } }
        }

        [DefaultValue(0)]
        [Category(categorySTR)]
        [Description("Layout calculation method.\r\nAvailable Values: 0, 1, 4.\r\n0: Default, Flexible in Designer (allows manual resize of expanders).\r\n1: Uses GetPreferredSize for expanders (consistent, overrides manual resize).\r\n4: Distributes space purely by weight (consistent, overrides manual resize).")]
        public int lay_PerformLayout_calcMethod_No
        {
            get => _performLayout_calcMethod_No;
            set { if (_performLayout_calcMethod_No != value) { _performLayout_calcMethod_No = value; PerformLayout(); Invalidate(); } }
        }

        // --- Standard Properties (Now Visible & Categorized) ---


        // --- Constructor ---
        public StackLayout()
        {
            // Consider setting DoubleBuffered = true if you experience flickering during resize,
            // though it can have performance implications.
            // this.DoubleBuffered = true;
        }

        // --- Core Layout Logic Switch ---
        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);

            // Use the selected calculation method (uses the LAY_ prefixed property now)
            switch (lay_PerformLayout_calcMethod_No)
            {
                case 1:
                    PerformStackLayout_v1();
                    break;
                case 4:
                    PerformStackLayout_v4();
                    break;
                case 0: // Default and preferred for designer flexibility
                default:
                    PerformStackLayout_old_v0();
                    break;
            }
        }

        // --- Layout Method 0 (Original - Designer Flexible) ---
        private void PerformStackLayout_old_v0()
        {
            // No change at the very beginning
            if (_isPerformingLayout)
            {
                return;
            }
            _isPerformingLayout = true;
            Debug.WriteLine($"StackLayout DEBUG: ---> Starting PerformStackLayout_v{lay_PerformLayout_calcMethod_No}");

            try
            {
                StackLayoutExtender extender = FindExtender();
                var visibleControls = this.Controls.OfType<Control>().Where(c => c.Visible).ToList();

                // Expanded: Check for no visible controls
                if (visibleControls.Count == 0)
                {
                    if (AutoScroll)
                    {
                        AutoScrollMinSize = Size.Empty;
                    }
                    // Debug.WriteLine("StackLayout DEBUG: No visible controls, returning."); // Optional debug line
                    // Removed the commented-out ResumeLayout, it's handled in finally
                    return; // Exit the method early
                }

                Rectangle displayRect = this.DisplayRectangle;
                this.SuspendLayout(); // Suspend within method

                double totalWeight = 0;
                int totalPreferredSize = 0;
                int expandingChildCount = 0;
                var weights = new Dictionary<Control, int>();

                // Expanded: Loop calculating initial sizes and weights
                foreach (Control child in visibleControls)
                {
                    int weight = extender?.Getlay_ExpandWeight(child) ?? 0;
                    weights[child] = weight;
                    if (weight > 0)
                    {
                        totalWeight += weight;
                        expandingChildCount++;
                    }
                    // Determine size along the main axis
                    int sizeAlongAxis = (lay_Orientation == StackOrientation.Vertical) ? child.Height : child.Width;
                    totalPreferredSize += sizeAlongAxis;
                }

                // Calculations remain the same here
                int totalSpacing = (visibleControls.Count > 1) ? (visibleControls.Count - 1) * lay_Spacing : 0;
                int totalUsedBeforeExpand = totalPreferredSize + totalSpacing;
                int availableSpace = (lay_Orientation == StackOrientation.Vertical) ? displayRect.Height : displayRect.Width;
                double spaceToDistribute = Math.Max(0, availableSpace - totalUsedBeforeExpand);

                double fractionalSpace = 0.0;
                var extraSpaceMap = new Dictionary<Control, int>();

                // Expanded: Calculation of extra space distribution (initial allocation)
                if (spaceToDistribute > 0 && totalWeight > 0)
                {
                    // Distribute space based on weight
                    foreach (Control child in visibleControls)
                    {
                        int weight = weights[child];
                        if (weight > 0)
                        {
                            double exactShare = spaceToDistribute * (double)weight / totalWeight;
                            int wholePixels = (int)Math.Floor(exactShare);
                            extraSpaceMap[child] = wholePixels;
                            fractionalSpace += exactShare - wholePixels; // Keep track of leftover fractions
                        }
                        else
                        {
                            // Non-expanding children get 0 extra space initially
                            extraSpaceMap[child] = 0;
                        }
                    }
                }
                else
                {
                    // No space to distribute or no weighted children, so everyone gets 0 extra space
                    foreach (Control child in visibleControls)
                    {
                        extraSpaceMap[child] = 0;
                    }
                }

                // Calculate leftover pixels from rounding
                int leftoverPixels = (int)Math.Round(fractionalSpace);

                // Expanded: Distribute leftover pixels (due to rounding) one by one to weighted children
                if (leftoverPixels > 0 && expandingChildCount > 0)
                {
                    int distributedLeftovers = 0;
                    foreach (Control child in visibleControls)
                    {
                        // Only give leftovers to controls that are already expanding
                        if (weights[child] > 0)
                        {
                            // Ensure entry exists if somehow it wasn't added (shouldn't happen here)
                            if (!extraSpaceMap.ContainsKey(child))
                            {
                                extraSpaceMap[child] = 0;
                            }
                            extraSpaceMap[child]++; // Give one leftover pixel
                            distributedLeftovers++;
                            if (distributedLeftovers >= leftoverPixels)
                            {
                                // Stop once all leftovers are distributed
                                break;
                            }
                        }
                    }
                }

                // --- Positioning Loop (already reasonably formatted, but ensure consistency) ---
                int currentPos = (lay_Orientation == StackOrientation.Vertical) ? displayRect.Top : displayRect.Left;
                int maxCrossAxisSize = 0;
                for (int i = 0; i < visibleControls.Count; i++)
                {
                    Control child = visibleControls[i];
                    int weight = weights[child];
                    int initialSizeAlongAxis = (lay_Orientation == StackOrientation.Vertical) ? child.Height : child.Width;
                    int extraSpace = extraSpaceMap.ContainsKey(child) ? extraSpaceMap[child] : 0;
                    int calculatedSizeAlongAxis = initialSizeAlongAxis + extraSpace;

                    // Apply Min/Max constraints along the orientation axis
                    if (lay_Orientation == StackOrientation.Vertical)
                    {
                        if (child.MaximumSize.Height > 0 && calculatedSizeAlongAxis > child.MaximumSize.Height)
                        {
                            calculatedSizeAlongAxis = child.MaximumSize.Height;
                        }
                        if (calculatedSizeAlongAxis < child.MinimumSize.Height)
                        {
                            calculatedSizeAlongAxis = child.MinimumSize.Height;
                        }
                    }
                    else // Horizontal
                    {
                        if (child.MaximumSize.Width > 0 && calculatedSizeAlongAxis > child.MaximumSize.Width)
                        {
                            calculatedSizeAlongAxis = child.MaximumSize.Width;
                        }
                        if (calculatedSizeAlongAxis < child.MinimumSize.Width)
                        {
                            calculatedSizeAlongAxis = child.MinimumSize.Width;
                        }
                    }

                    int crossAxisPos, crossAxisSize;
                    BoundsSpecified boundsSpec = BoundsSpecified.None;

                    if (lay_Orientation == StackOrientation.Vertical)
                    {
                        /* Cross Axis V - Alignment & Sizing */
                        int availableWidth = displayRect.Width;
                        crossAxisPos = displayRect.Left;
                        switch (lay_ChildAxisAlignment)
                        {
                            case StackChildAxisAlignment.Stretch:
                                crossAxisSize = availableWidth;
                                if (child.MaximumSize.Width > 0 && crossAxisSize > child.MaximumSize.Width) crossAxisSize = child.MaximumSize.Width;
                                if (crossAxisSize < child.MinimumSize.Width) crossAxisSize = child.MinimumSize.Width;
                                boundsSpec |= BoundsSpecified.Width;
                                break;
                            case StackChildAxisAlignment.Center:
                                crossAxisSize = child.Width; // Use current width
                                crossAxisPos += (availableWidth - crossAxisSize) / 2;
                                if (crossAxisPos < displayRect.Left) crossAxisPos = displayRect.Left; // Clamp
                                break;
                            case StackChildAxisAlignment.End:
                                crossAxisSize = child.Width; // Use current width
                                crossAxisPos += availableWidth - crossAxisSize;
                                if (crossAxisPos < displayRect.Left) crossAxisPos = displayRect.Left; // Clamp
                                break;
                            case StackChildAxisAlignment.Start:
                            default:
                                crossAxisSize = child.Width; // Use current width
                                break; // Already aligned left (start)
                        }
                        child.SetBounds(crossAxisPos, currentPos, crossAxisSize, calculatedSizeAlongAxis, BoundsSpecified.Location | BoundsSpecified.Height | boundsSpec);
                        maxCrossAxisSize = Math.Max(maxCrossAxisSize, crossAxisSize);
                        currentPos += calculatedSizeAlongAxis;
                    }
                    else // Horizontal Orientation
                    {
                        /* Cross Axis H - Alignment & Sizing */
                        int availableHeight = displayRect.Height;
                        crossAxisPos = displayRect.Top;
                        switch (lay_ChildAxisAlignment)
                        {
                            case StackChildAxisAlignment.Stretch:
                                crossAxisSize = availableHeight;
                                if (child.MaximumSize.Height > 0 && crossAxisSize > child.MaximumSize.Height) crossAxisSize = child.MaximumSize.Height;
                                if (crossAxisSize < child.MinimumSize.Height) crossAxisSize = child.MinimumSize.Height;
                                boundsSpec |= BoundsSpecified.Height;
                                break;
                            case StackChildAxisAlignment.Center:
                                crossAxisSize = child.Height; // Use current height
                                crossAxisPos += (availableHeight - crossAxisSize) / 2;
                                if (crossAxisPos < displayRect.Top) crossAxisPos = displayRect.Top; // Clamp
                                break;
                            case StackChildAxisAlignment.End:
                                crossAxisSize = child.Height; // Use current height
                                crossAxisPos += availableHeight - crossAxisSize;
                                if (crossAxisPos < displayRect.Top) crossAxisPos = displayRect.Top; // Clamp
                                break;
                            case StackChildAxisAlignment.Start:
                            default:
                                crossAxisSize = child.Height; // Use current height
                                break; // Already aligned top (start)
                        }
                        child.SetBounds(currentPos, crossAxisPos, calculatedSizeAlongAxis, crossAxisSize, BoundsSpecified.Location | BoundsSpecified.Width | boundsSpec);
                        maxCrossAxisSize = Math.Max(maxCrossAxisSize, crossAxisSize);
                        currentPos += calculatedSizeAlongAxis;
                    }

                    // Add spacing if not the last control
                    if (i < visibleControls.Count - 1)
                    {
                        currentPos += lay_Spacing;
                    }
                } // End of positioning loop

                // --- AutoScroll Calculation ---
                if (AutoScroll)
                {
                    if (lay_Orientation == StackOrientation.Vertical)
                    {
                        int requiredHeight = currentPos - displayRect.Top + Padding.Bottom;
                        int requiredWidth = (lay_ChildAxisAlignment == StackChildAxisAlignment.Stretch) ? displayRect.Width : maxCrossAxisSize + Padding.Left + Padding.Right;
                        requiredWidth = Math.Max(displayRect.Width, requiredWidth); // Don't shrink less than panel width
                        this.AutoScrollMinSize = new Size(requiredWidth, requiredHeight);
                        // Debug.WriteLine($"StackLayout DEBUG: Setting AutoScrollMinSize (V): {this.AutoScrollMinSize}"); // Optional
                    }
                    else // Horizontal
                    {
                        int requiredWidth = currentPos - displayRect.Left + Padding.Right;
                        int requiredHeight = (lay_ChildAxisAlignment == StackChildAxisAlignment.Stretch) ? displayRect.Height : maxCrossAxisSize + Padding.Top + Padding.Bottom;
                        requiredHeight = Math.Max(displayRect.Height, requiredHeight); // Don't shrink less than panel height
                        this.AutoScrollMinSize = new Size(requiredWidth, requiredHeight);
                        // Debug.WriteLine($"StackLayout DEBUG: Setting AutoScrollMinSize (H): {this.AutoScrollMinSize}"); // Optional
                    }
                }
                else
                {
                    this.AutoScrollMinSize = Size.Empty;
                    // Debug.WriteLine("StackLayout DEBUG: AutoScroll is False."); // Optional
                }

                this.ResumeLayout(true); // Resume within method
            }
            finally
            {
                _isPerformingLayout = false;
                Debug.WriteLine($"StackLayout DEBUG: <--- Finished PerformStackLayout_v{lay_PerformLayout_calcMethod_No}");
            }
        }

        // --- Layout Method 1 (GetPreferredSize Base) ---
        // --- Layout Method 1 (GetPreferredSize Base) ---
        private void PerformStackLayout_v1()
        {
            if (_isPerformingLayout)
            {
                return;
            }
            _isPerformingLayout = true;
            Debug.WriteLine($"StackLayout DEBUG: ---> Starting PerformStackLayout_v{lay_PerformLayout_calcMethod_No}");

            try
            {
                StackLayoutExtender extender = FindExtender();
                var visibleControls = this.Controls.OfType<Control>().Where(c => c.Visible).ToList();

                // Expanded: Check for no visible controls
                if (visibleControls.Count == 0)
                {
                    if (AutoScroll)
                    {
                        AutoScrollMinSize = Size.Empty;
                    }
                    return; // Exit early
                }

                Rectangle displayRect = this.DisplayRectangle;
                this.SuspendLayout(); // Suspend within method

                double totalWeight = 0;
                int totalBaseSize = 0;
                int expandingChildCount = 0;
                var weights = new Dictionary<Control, int>();
                var baseSizes = new Dictionary<Control, int>();

                // Expanded: Loop calculating base sizes (using PreferredSize for expanders)
                foreach (Control child in visibleControls)
                {
                    int weight = extender?.Getlay_ExpandWeight(child) ?? 0;
                    weights[child] = weight;
                    int baseSizeAlongAxis;

                    if (weight == 0)
                    {
                        // Non-expanding: Use current size
                        baseSizeAlongAxis = (lay_Orientation == StackOrientation.Vertical) ? child.Height : child.Width;
                    }
                    else
                    {
                        // Expanding: Use GetPreferredSize as the base, respecting MinimumSize
                        Size preferredSize = child.GetPreferredSize(Size.Empty); // Pass Empty to get ideal size
                        baseSizeAlongAxis = (lay_Orientation == StackOrientation.Vertical) ? preferredSize.Height : preferredSize.Width;

                        int minSizeAlongAxis = (lay_Orientation == StackOrientation.Vertical) ? child.MinimumSize.Height : child.MinimumSize.Width;
                        if (minSizeAlongAxis > 0 && baseSizeAlongAxis < minSizeAlongAxis)
                        {
                            // Ensure base size isn't less than minimum
                            baseSizeAlongAxis = minSizeAlongAxis;
                        }
                        totalWeight += weight;
                        expandingChildCount++;
                    }
                    baseSizes[child] = baseSizeAlongAxis;
                    totalBaseSize += baseSizeAlongAxis;
                }

                // Calculations remain the same
                int totalSpacing = (visibleControls.Count > 1) ? (visibleControls.Count - 1) * lay_Spacing : 0;
                int availableSpace = (lay_Orientation == StackOrientation.Vertical) ? displayRect.Height : displayRect.Width;
                double spaceToDistribute = Math.Max(0, availableSpace - totalBaseSize - totalSpacing);

                double fractionalSpace = 0.0;
                var extraSpaceMap = new Dictionary<Control, int>();

                // Expanded: Calculate initial extra space distribution for expanding items
                if (spaceToDistribute > 0 && totalWeight > 0)
                {
                    // Filter for expanding controls only for this loop
                    foreach (var child in visibleControls.Where(c => weights[c] > 0))
                    {
                        int weight = weights[child];
                        double exactShare = spaceToDistribute * (double)weight / totalWeight;
                        int wholePixels = (int)Math.Floor(exactShare);
                        extraSpaceMap[child] = wholePixels;
                        fractionalSpace += exactShare - wholePixels;
                    }

                    // Calculate and distribute leftovers from rounding
                    int leftoverPixels = (int)Math.Round(fractionalSpace);
                    if (leftoverPixels > 0 && expandingChildCount > 0)
                    {
                        int distributedLeftovers = 0;
                        // Filter again for expanding controls to distribute leftovers
                        foreach (var child in visibleControls.Where(c => weights[c] > 0))
                        {
                            if (!extraSpaceMap.ContainsKey(child)) // Should exist, but safety check
                            {
                                extraSpaceMap[child] = 0;
                            }
                            extraSpaceMap[child]++;
                            distributedLeftovers++;
                            if (distributedLeftovers >= leftoverPixels)
                            {
                                break;
                            }
                        }
                    }
                }

                // Expanded: Ensure all controls have an entry in extraSpaceMap (even if it's 0)
                foreach (var child in visibleControls)
                {
                    if (!extraSpaceMap.ContainsKey(child))
                    {
                        extraSpaceMap[child] = 0;
                    }
                }


                // --- Positioning Loop ---
                int currentPos = (lay_Orientation == StackOrientation.Vertical) ? displayRect.Top : displayRect.Left;
                int maxCrossAxisSize = 0;
                for (int i = 0; i < visibleControls.Count; i++)
                {
                    Control child = visibleControls[i];
                    int weight = weights[child];
                    int baseSizeAlongAxis = baseSizes[child];
                    int extraSpace = extraSpaceMap[child]; // Already calculated above
                    int finalSizeAlongAxis = baseSizeAlongAxis + extraSpace;

                    // Expanded: Apply Min/Max constraints (only strictly needed for expanding items here)
                    if (weight > 0)
                    {
                        if (lay_Orientation == StackOrientation.Vertical)
                        {
                            if (child.MaximumSize.Height > 0 && finalSizeAlongAxis > child.MaximumSize.Height)
                            {
                                finalSizeAlongAxis = child.MaximumSize.Height;
                            }
                            if (finalSizeAlongAxis < child.MinimumSize.Height) // Check MinimumSize *after* adding extra space
                            {
                                finalSizeAlongAxis = child.MinimumSize.Height;
                            }
                        }
                        else // Horizontal
                        {
                            if (child.MaximumSize.Width > 0 && finalSizeAlongAxis > child.MaximumSize.Width)
                            {
                                finalSizeAlongAxis = child.MaximumSize.Width;
                            }
                            if (finalSizeAlongAxis < child.MinimumSize.Width) // Check MinimumSize *after* adding extra space
                            {
                                finalSizeAlongAxis = child.MinimumSize.Width;
                            }
                        }
                    } // Non-expanding items retain their baseSize (which was their current H/W)


                    int crossAxisPos, crossAxisSize;
                    BoundsSpecified boundsSpec = BoundsSpecified.None;

                    // Expanded: Cross-axis alignment and sizing (Vertical Orientation)
                    if (lay_Orientation == StackOrientation.Vertical)
                    {
                        int availableWidth = displayRect.Width;
                        crossAxisPos = displayRect.Left;
                        switch (lay_ChildAxisAlignment)
                        {
                            case StackChildAxisAlignment.Stretch:
                                crossAxisSize = availableWidth;
                                if (child.MaximumSize.Width > 0 && crossAxisSize > child.MaximumSize.Width) crossAxisSize = child.MaximumSize.Width;
                                if (crossAxisSize < child.MinimumSize.Width) crossAxisSize = child.MinimumSize.Width;
                                boundsSpec |= BoundsSpecified.Width;
                                break;
                            case StackChildAxisAlignment.Center:
                                crossAxisSize = child.Width;
                                crossAxisPos += (availableWidth - crossAxisSize) / 2;
                                if (crossAxisPos < displayRect.Left) crossAxisPos = displayRect.Left;
                                break;
                            case StackChildAxisAlignment.End:
                                crossAxisSize = child.Width;
                                crossAxisPos += availableWidth - crossAxisSize;
                                if (crossAxisPos < displayRect.Left) crossAxisPos = displayRect.Left;
                                break;
                            case StackChildAxisAlignment.Start:
                            default:
                                crossAxisSize = child.Width;
                                break;
                        }
                        child.SetBounds(crossAxisPos, currentPos, crossAxisSize, finalSizeAlongAxis, BoundsSpecified.Location | BoundsSpecified.Height | boundsSpec);
                        maxCrossAxisSize = Math.Max(maxCrossAxisSize, crossAxisSize);
                        currentPos += finalSizeAlongAxis;
                    }
                    // Expanded: Cross-axis alignment and sizing (Horizontal Orientation)
                    else
                    {
                        int availableHeight = displayRect.Height;
                        crossAxisPos = displayRect.Top;
                        switch (lay_ChildAxisAlignment)
                        {
                            case StackChildAxisAlignment.Stretch:
                                crossAxisSize = availableHeight;
                                if (child.MaximumSize.Height > 0 && crossAxisSize > child.MaximumSize.Height) crossAxisSize = child.MaximumSize.Height;
                                if (crossAxisSize < child.MinimumSize.Height) crossAxisSize = child.MinimumSize.Height;
                                boundsSpec |= BoundsSpecified.Height;
                                break;
                            case StackChildAxisAlignment.Center:
                                crossAxisSize = child.Height;
                                crossAxisPos += (availableHeight - crossAxisSize) / 2;
                                if (crossAxisPos < displayRect.Top) crossAxisPos = displayRect.Top;
                                break;
                            case StackChildAxisAlignment.End:
                                crossAxisSize = child.Height;
                                crossAxisPos += availableHeight - crossAxisSize;
                                if (crossAxisPos < displayRect.Top) crossAxisPos = displayRect.Top;
                                break;
                            case StackChildAxisAlignment.Start:
                            default:
                                crossAxisSize = child.Height;
                                break;
                        }
                        child.SetBounds(currentPos, crossAxisPos, finalSizeAlongAxis, crossAxisSize, BoundsSpecified.Location | BoundsSpecified.Width | boundsSpec);
                        maxCrossAxisSize = Math.Max(maxCrossAxisSize, crossAxisSize);
                        currentPos += finalSizeAlongAxis;
                    }

                    // Add spacing if not the last control
                    if (i < visibleControls.Count - 1)
                    {
                        currentPos += lay_Spacing;
                    }
                } // End positioning loop

                // Expanded: AutoScroll calculation block
                if (AutoScroll)
                {
                    if (lay_Orientation == StackOrientation.Vertical)
                    {
                        int requiredHeight = currentPos - displayRect.Top + Padding.Bottom;
                        int requiredWidth = (lay_ChildAxisAlignment == StackChildAxisAlignment.Stretch) ? displayRect.Width : maxCrossAxisSize + Padding.Left + Padding.Right;
                        requiredWidth = Math.Max(displayRect.Width, requiredWidth);
                        this.AutoScrollMinSize = new Size(requiredWidth, requiredHeight);
                    }
                    else // Horizontal
                    {
                        int requiredWidth = currentPos - displayRect.Left + Padding.Right;
                        int requiredHeight = (lay_ChildAxisAlignment == StackChildAxisAlignment.Stretch) ? displayRect.Height : maxCrossAxisSize + Padding.Top + Padding.Bottom;
                        requiredHeight = Math.Max(displayRect.Height, requiredHeight);
                        this.AutoScrollMinSize = new Size(requiredWidth, requiredHeight);
                    }
                }
                else
                {
                    this.AutoScrollMinSize = Size.Empty;
                }

                this.ResumeLayout(true); // Resume within method
            }
            finally
            {
                _isPerformingLayout = false;
                Debug.WriteLine($"StackLayout DEBUG: <--- Finished PerformStackLayout_v{lay_PerformLayout_calcMethod_No}");
            }
        }

        // --- Layout Method 4 (Pure Weight Distribution) ---
        // --- Layout Method 4 (Pure Weight Distribution) ---
        private void PerformStackLayout_v4()
        {
            if (_isPerformingLayout)
            {
                return;
            }
            _isPerformingLayout = true;
            Debug.WriteLine($"StackLayout DEBUG: ---> Starting PerformStackLayout_v{lay_PerformLayout_calcMethod_No}");

            try
            {
                StackLayoutExtender extender = FindExtender();
                var visibleControls = this.Controls.OfType<Control>().Where(c => c.Visible).ToList();

                // Expanded: Check for no visible controls
                if (visibleControls.Count == 0)
                {
                    if (AutoScroll)
                    {
                        AutoScrollMinSize = Size.Empty;
                    }
                    return; // Exit early
                }

                Rectangle displayRect = this.DisplayRectangle;
                this.SuspendLayout(); // Suspend within method

                double totalWeight = 0;
                int totalNonExpandingSize = 0;
                int expandingChildCount = 0;
                var weights = new Dictionary<Control, int>();
                var expandingControls = new List<Control>(); // Keep track of controls that should expand

                // Expanded: Loop separating expanding/non-expanding and summing sizes/weights
                foreach (Control child in visibleControls)
                {
                    int weight = extender?.Getlay_ExpandWeight(child) ?? 0;
                    weights[child] = weight;

                    if (weight == 0)
                    {
                        // Non-expanding: Sum their current size along the axis
                        int sizeAlongAxis = (lay_Orientation == StackOrientation.Vertical) ? child.Height : child.Width;
                        totalNonExpandingSize += sizeAlongAxis;
                    }
                    else
                    {
                        // Expanding: Sum weight, count them, add to list
                        totalWeight += weight;
                        expandingChildCount++;
                        expandingControls.Add(child);
                    }
                }

                // Calculations remain the same
                int totalSpacing = (visibleControls.Count > 1) ? (visibleControls.Count - 1) * lay_Spacing : 0;
                int availableSpace = (lay_Orientation == StackOrientation.Vertical) ? displayRect.Height : displayRect.Width;
                // Calculate space available ONLY for the expanding controls
                double spaceAvailableForExpanders = Math.Max(0, availableSpace - totalNonExpandingSize - totalSpacing);

                double fractionalSpace = 0.0;
                var calculatedExpanderSizes = new Dictionary<Control, int>();

                // Expanded: Calculate size distribution ONLY for expanding controls
                if (spaceAvailableForExpanders > 0 && totalWeight > 0)
                {
                    // Loop ONLY through the expanding controls list
                    foreach (Control child in expandingControls)
                    {
                        int weight = weights[child]; // Get weight from previously stored dictionary
                        double exactShare = spaceAvailableForExpanders * (double)weight / totalWeight;
                        int wholePixels = (int)Math.Floor(exactShare);
                        calculatedExpanderSizes[child] = wholePixels;
                        fractionalSpace += exactShare - wholePixels;
                    }

                    // Calculate and distribute leftovers from rounding
                    int leftoverPixels = (int)Math.Round(fractionalSpace);
                    if (leftoverPixels > 0 && expandingChildCount > 0)
                    {
                        int distributedLeftovers = 0;
                        // Loop ONLY through expanding controls to give leftovers
                        foreach (Control child in expandingControls)
                        {
                            if (!calculatedExpanderSizes.ContainsKey(child)) // Safety check
                            {
                                calculatedExpanderSizes[child] = 0;
                            }
                            calculatedExpanderSizes[child]++;
                            distributedLeftovers++;
                            if (distributedLeftovers >= leftoverPixels)
                            {
                                break;
                            }
                        }
                    }
                }
                else // No space or no expanders
                {
                    // Ensure expander size map is initialized (with 0 size) even if no space
                    foreach (Control child in expandingControls)
                    {
                        calculatedExpanderSizes[child] = 0;
                    }
                }

                // --- Positioning Loop ---
                int currentPos = (lay_Orientation == StackOrientation.Vertical) ? displayRect.Top : displayRect.Left;
                int maxCrossAxisSize = 0;
                for (int i = 0; i < visibleControls.Count; i++)
                {
                    Control child = visibleControls[i];
                    int weight = weights[child];
                    int sizeAlongAxis; // This will be the final calculated size

                    // Expanded: Determine sizeAlongAxis based on weight
                    if (weight == 0)
                    {
                        // Non-expanding: Use its current size
                        sizeAlongAxis = (lay_Orientation == StackOrientation.Vertical) ? child.Height : child.Width;
                    }
                    else
                    {
                        // Expanding: Use the purely calculated size based on weight distribution
                        sizeAlongAxis = calculatedExpanderSizes.ContainsKey(child) ? calculatedExpanderSizes[child] : 0;

                        // Expanded: Apply Min/Max constraints AFTER calculating purely weighted size
                        if (lay_Orientation == StackOrientation.Vertical)
                        {
                            if (child.MaximumSize.Height > 0 && sizeAlongAxis > child.MaximumSize.Height)
                            {
                                sizeAlongAxis = child.MaximumSize.Height;
                            }
                            if (sizeAlongAxis < child.MinimumSize.Height)
                            {
                                sizeAlongAxis = child.MinimumSize.Height;
                            }
                        }
                        else // Horizontal
                        {
                            if (child.MaximumSize.Width > 0 && sizeAlongAxis > child.MaximumSize.Width)
                            {
                                sizeAlongAxis = child.MaximumSize.Width;
                            }
                            if (sizeAlongAxis < child.MinimumSize.Width)
                            {
                                sizeAlongAxis = child.MinimumSize.Width;
                            }
                        }
                    }

                    int crossAxisPos, crossAxisSize;
                    BoundsSpecified boundsSpec = BoundsSpecified.None;

                    // Expanded: Cross-axis alignment and sizing (Vertical Orientation)
                    if (lay_Orientation == StackOrientation.Vertical)
                    {
                        int availableWidth = displayRect.Width;
                        crossAxisPos = displayRect.Left;
                        switch (lay_ChildAxisAlignment)
                        {
                            case StackChildAxisAlignment.Stretch:
                                crossAxisSize = availableWidth;
                                if (child.MaximumSize.Width > 0 && crossAxisSize > child.MaximumSize.Width) crossAxisSize = child.MaximumSize.Width;
                                if (crossAxisSize < child.MinimumSize.Width) crossAxisSize = child.MinimumSize.Width;
                                boundsSpec |= BoundsSpecified.Width;
                                break;
                            case StackChildAxisAlignment.Center:
                                crossAxisSize = child.Width;
                                crossAxisPos += (availableWidth - crossAxisSize) / 2;
                                if (crossAxisPos < displayRect.Left) crossAxisPos = displayRect.Left;
                                break;
                            case StackChildAxisAlignment.End:
                                crossAxisSize = child.Width;
                                crossAxisPos += availableWidth - crossAxisSize;
                                if (crossAxisPos < displayRect.Left) crossAxisPos = displayRect.Left;
                                break;
                            case StackChildAxisAlignment.Start:
                            default:
                                crossAxisSize = child.Width;
                                break;
                        }
                        child.SetBounds(crossAxisPos, currentPos, crossAxisSize, sizeAlongAxis, BoundsSpecified.Location | BoundsSpecified.Height | boundsSpec);
                        maxCrossAxisSize = Math.Max(maxCrossAxisSize, crossAxisSize);
                        currentPos += sizeAlongAxis;
                    }
                    // Expanded: Cross-axis alignment and sizing (Horizontal Orientation)
                    else
                    {
                        int availableHeight = displayRect.Height;
                        crossAxisPos = displayRect.Top;
                        switch (lay_ChildAxisAlignment)
                        {
                            case StackChildAxisAlignment.Stretch:
                                crossAxisSize = availableHeight;
                                if (child.MaximumSize.Height > 0 && crossAxisSize > child.MaximumSize.Height) crossAxisSize = child.MaximumSize.Height;
                                if (crossAxisSize < child.MinimumSize.Height) crossAxisSize = child.MinimumSize.Height;
                                boundsSpec |= BoundsSpecified.Height;
                                break;
                            case StackChildAxisAlignment.Center:
                                crossAxisSize = child.Height;
                                crossAxisPos += (availableHeight - crossAxisSize) / 2;
                                if (crossAxisPos < displayRect.Top) crossAxisPos = displayRect.Top;
                                break;
                            case StackChildAxisAlignment.End:
                                crossAxisSize = child.Height;
                                crossAxisPos += availableHeight - crossAxisSize;
                                if (crossAxisPos < displayRect.Top) crossAxisPos = displayRect.Top;
                                break;
                            case StackChildAxisAlignment.Start:
                            default:
                                crossAxisSize = child.Height;
                                break;
                        }
                        child.SetBounds(currentPos, crossAxisPos, sizeAlongAxis, crossAxisSize, BoundsSpecified.Location | BoundsSpecified.Width | boundsSpec);
                        maxCrossAxisSize = Math.Max(maxCrossAxisSize, crossAxisSize);
                        currentPos += sizeAlongAxis;
                    }

                    // Add spacing if not the last control
                    if (i < visibleControls.Count - 1)
                    {
                        currentPos += lay_Spacing;
                    }
                } // End positioning loop

                // Expanded: AutoScroll calculation block
                if (AutoScroll)
                {
                    if (lay_Orientation == StackOrientation.Vertical)
                    {
                        int requiredHeight = currentPos - displayRect.Top + Padding.Bottom;
                        int requiredWidth = (lay_ChildAxisAlignment == StackChildAxisAlignment.Stretch) ? displayRect.Width : maxCrossAxisSize + Padding.Left + Padding.Right;
                        requiredWidth = Math.Max(displayRect.Width, requiredWidth);
                        this.AutoScrollMinSize = new Size(requiredWidth, requiredHeight);
                    }
                    else // Horizontal
                    {
                        int requiredWidth = currentPos - displayRect.Left + Padding.Right;
                        int requiredHeight = (lay_ChildAxisAlignment == StackChildAxisAlignment.Stretch) ? displayRect.Height : maxCrossAxisSize + Padding.Top + Padding.Bottom;
                        requiredHeight = Math.Max(displayRect.Height, requiredHeight);
                        this.AutoScrollMinSize = new Size(requiredWidth, requiredHeight);
                    }
                }
                else
                {
                    this.AutoScrollMinSize = Size.Empty;
                }

                this.ResumeLayout(true); // Resume within method
            }
            finally
            {
                _isPerformingLayout = false;
                Debug.WriteLine($"StackLayout DEBUG: <--- Finished PerformStackLayout_v{lay_PerformLayout_calcMethod_No}");
            }
        }

        // --- Helper Methods ---
        // ---this doesnt work
        private StackLayoutExtender FindExtender_old()
        {
           var f=  this.FindForm();
          var par = this.Parent;
            var f2 =  f.Controls.OfType<StackLayoutExtender>().FirstOrDefault();

            if (!_extenderSearched && Site != null && Site.Container != null)
            {
                _extenderSearched = true;
                _extender = Site.Container.Components.OfType<StackLayoutExtender>().FirstOrDefault();
            }
            return _extender;
        }
        


        // --- Overrides for Layout Triggers and Designer Integration ---

        // --- Inside StackLayout.cs ---

        // 1. Modify OnControlAdded
        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);
            if (e.Control != null)
            {
                e.Control.VisibleChanged += ChildControl_VisibleChanged; // Subscribe
            }
            PerformLayout(); // Perform layout on add
        }

        // 2. Modify OnControlRemoved
        protected override void OnControlRemoved(ControlEventArgs e)
        {
            base.OnControlRemoved(e);
            if (e.Control != null)
            {
                e.Control.VisibleChanged -= ChildControl_VisibleChanged; // Unsubscribe IMPORTANT!
            }
            PerformLayout(); // Perform layout on remove
        }

        // 3. Add the Event Handler method
        private void ChildControl_VisibleChanged(object sender, EventArgs e)
        {
            // When a child's visibility changes AT RUNTIME, trigger a layout pass
            // Use BeginInvoke for safety if changes happen rapidly or cross-thread (less likely here)
            if (this.IsHandleCreated && !this.IsDisposed)
            {
                this.BeginInvoke((MethodInvoker)delegate {
                    this.PerformLayout();
                    this.Invalidate(true); // Ensure redraw
                });
            }
            else if (!this.IsDisposed) // Fallback if handle not created yet
            {
                this.PerformLayout();
                this.Invalidate(true);
            }
        }
         
        protected override void OnPaddingChanged(EventArgs e) { base.OnPaddingChanged(e); PerformLayout(); }
        protected override void OnSizeChanged(EventArgs e) { base.OnSizeChanged(e); PerformLayout(); } // Added for responsiveness when panel size changes


        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override ISite Site
        {
            get => base.Site;
            set
            {
                if (_componentChangeService != null) { _componentChangeService.ComponentChanged -= OnComponentChanged; _componentChangeService = null; }
                base.Site = value;
                _extender = null; _extenderSearched = false;
                if (base.Site != null) { _componentChangeService = (IComponentChangeService)base.Site.GetService(typeof(IComponentChangeService)); if (_componentChangeService != null) { _componentChangeService.ComponentChanged += OnComponentChanged; } }
            }
        }

        private void OnComponentChanged(object sender, ComponentChangedEventArgs e)
        {
            Debug.WriteLine($"StackLayout DEBUG: OnComponentChanged fired for {e.Component?.GetType().Name}, Member: {e.Member?.Name}, IsPerformingLayout: {_isPerformingLayout}");
            if (_isPerformingLayout)
            {
                Debug.WriteLine("StackLayout DEBUG: OnComponentChanged - Skipping because _isPerformingLayout is true.");
                return;
            }

            // Trigger layout if a direct child's visibility, size, or bounds change
            if (e.Component is Control changedControl && changedControl.Parent == this)
            {
                string memberName = e.Member?.Name;
                if (memberName == "Visible" || memberName == "Bounds" || memberName == "Size" || memberName == "Width" || memberName == "Height" || memberName == "MinimumSize" || memberName == "MaximumSize")
                {
                    Debug.WriteLine($"StackLayout DEBUG: OnComponentChanged triggering PerformLayout due to child '{changedControl.Name}' member '{memberName}' change.");
                    // Use BeginInvoke for safety in designer context
                    if (this.IsHandleCreated && !this.IsDisposed) { this.BeginInvoke((MethodInvoker)delegate { this.PerformLayout(); this.Invalidate(true); }); }
                    else { this.PerformLayout(); this.Invalidate(true); } // Fallback
                }
            }
            // Also trigger layout if the extender component itself changes (might be rare, but possible)
            else if (e.Component == _extender)
            {
                Debug.WriteLine($"StackLayout DEBUG: OnComponentChanged triggering PerformLayout due to extender change.");
                if (this.IsHandleCreated && !this.IsDisposed) { this.BeginInvoke((MethodInvoker)delegate { this.PerformLayout(); this.Invalidate(true); }); }
                else { this.PerformLayout(); this.Invalidate(true); } // Fallback
            }
        }

        // 4. Ensure Dispose unsubscribes too (Good Practice)
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Unsubscribe from all children to be safe
                foreach (Control c in this.Controls)
                {
                    if (c != null) 
                        c.VisibleChanged -= ChildControl_VisibleChanged;
                }

                if (_componentChangeService != null)
                {
                    _componentChangeService.ComponentChanged -= OnComponentChanged;
                    _componentChangeService = null;
                }
            }
            base.Dispose(disposing);
        }


    } // End class StackLayout
} // End namespace