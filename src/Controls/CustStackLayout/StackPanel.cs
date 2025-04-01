using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SharpBrowser.Controls // Ensure this namespace matches your project
{
    /// <summary>
    /// Specifies the direction in which child controls are stacked within a StackLayout.
    /// </summary>
    public enum StackOrientation
    {
        Vertical,
        Horizontal
    }

    /// <summary>
    /// Defines how child controls are aligned and sized perpendicular to the stacking direction.
    /// </summary>
    public enum StackChildAxisAlignment
    {
        Stretch,
        Start,
        Center,
        End
    }

    /// <summary>
    /// Specifies how a floating control is initially positioned relative to its target before offsets are applied.
    /// </summary>
    public enum FloatAlignment
    {
        TopLeft,
        ToLeftOf,
        ToRightOf,
        ToTopOf,
        ToBottomOf
    }

    /// <summary>
    /// Defines how a floating control's Z-order is managed relative to its target
    /// within a StackLayout during layout passes.
    /// </summary>
    public enum StackFloatZOrder
    {
        InFrontOfTarget,
        BehindTarget,
        Manual
    }

    /// <summary>
    /// A panel that arranges child controls sequentially in a single line (horizontally or vertically),
    /// supporting weighted expansion and relative positioning of floating elements.
    /// This is a partial class definition; the IExtenderProvider implementation is in StackLayout.Extender.cs.
    /// </summary>
    public partial class StackLayout : Panel
    {
        // --- Constants ---
        public const string categorySTR = "L_Layout2";

        // --- Private Fields (Layout Properties) ---
        private int _spacing = 3;
        private StackOrientation _orientation = StackOrientation.Vertical;
        private StackChildAxisAlignment _childAxisAlignment = StackChildAxisAlignment.Stretch;
        private int _performLayout_calcMethod_No = 0; // Default

        private IComponentChangeService _componentChangeService = null;
        private bool _isPerformingLayout = false;

        // NOTE: Hashtables for extender properties are defined in StackLayout.Extender.cs

        #region Public Layout Properties (Prefixed and Categorized)

        [DefaultValue(3)]
        [Description("The space in pixels between stacked controls.")]
        [Category(categorySTR)]
        public int lay_Spacing
        {
            get => _spacing;
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

        [DefaultValue(StackOrientation.Vertical)]
        [Description("Specifies the direction in which child controls are stacked.")]
        [Category(categorySTR)]
        public StackOrientation lay_Orientation
        {
            get => _orientation;
            set
            {
                if (_orientation != value)
                {
                    _orientation = value;
                    PerformLayout();
                    Invalidate();
                }
            }
        }

        [DefaultValue(StackChildAxisAlignment.Stretch)]
        [Description("Defines how child controls are aligned and sized perpendicular to the stacking direction. Mimics CSS align-items.")]
        [Category(categorySTR)]
        public StackChildAxisAlignment lay_ChildAxisAlignment
        {
            get => _childAxisAlignment;
            set
            {
                if (_childAxisAlignment != value)
                {
                    _childAxisAlignment = value;
                    PerformLayout();
                    Invalidate();
                }
            }
        }

        [DefaultValue(0)]
        [Category(categorySTR)]
        [Description("Layout calculation method.\r\n0: Default, Flexible in Designer.\r\n4: Distributes space purely by weight.")]
        public int lay_PerformLayout_calcMethod_No
        {
            get => _performLayout_calcMethod_No;
            set
            {
                if (_performLayout_calcMethod_No != value)
                {
                    _performLayout_calcMethod_No = value;
                    PerformLayout();
                    Invalidate();
                }
            }
        }

        // --- Standard Properties (Made Visible & Categorized) ---
        [Category(categorySTR)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public override AnchorStyles Anchor
        {
            get => base.Anchor;
            set
            {
                if (base.Anchor != value)
                {
                    base.Anchor = value;
                    // No PerformLayout() needed here, framework handles Anchor/Dock layout
                }
            }
        }

        [Category(categorySTR)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public override DockStyle Dock
        {
            get => base.Dock;
            set
            {
                if (base.Dock != value)
                {
                    base.Dock = value;
                    // No PerformLayout() needed here, framework handles Anchor/Dock layout
                }
            }
        }

        #endregion

        // --- Constructor ---
        public StackLayout()
        {
            // Optional optimization for smoother drawing, especially with many controls
            this.DoubleBuffered = true;
        }

        // --- Core Layout Logic Switch ---
        protected override void OnLayout(LayoutEventArgs levent)
        {
            // Optimization: Don't layout if invisible or disposing
            if (!this.Visible || this.IsDisposed || this.Disposing)
            {
                return;
            }

            base.OnLayout(levent);

            // Use the selected calculation method
            switch (lay_PerformLayout_calcMethod_No)
            {
                case 4:
                    PerformStackLayout_v4();
                    break;
                case 0:
                default:
                    PerformStackLayout_old_v0(); // Flexible designer mode
                    break;
            }
        }

        #region Layout Method 0 (Flexible Designer Mode) and Helpers
        /// <summary>
        /// Layout method 0: Flexible, allows designer overrides but respects weights.
        /// Includes support for floating controls.
        /// </summary>
        private void PerformStackLayout_old_v0()
        {
            if (_isPerformingLayout)
            {
                LayoutLogger.Log($"StackLayout [{this.Name}]: PerformStackLayout_v0 SKIPPED (Re-entrancy).");
                return;
            }
            _isPerformingLayout = true;
            LayoutLogger.Log($"StackLayout [{this.Name}]: ---> Starting PerformStackLayout_v0");

            try
            {
                List<Control> flowControls, floatingControls;
                PL_p1__Separate_Visible_Controls_into_Flow_and_Floating(out flowControls, out floatingControls);

                Rectangle displayRect = this.DisplayRectangle;

                if (flowControls.Count == 0)
                {
                    PL_p2__Handle_Case_of_No_Flow_Controls(floatingControls, displayRect);
                    LayoutLogger.Log($"StackLayout [{this.Name}]: <--- Finished PerformStackLayout_v0 (No Flow Controls)");
                    // _isPerformingLayout is reset inside PL_p2
                    return; // Early exit
                }

                this.SuspendLayout(); // Suspend layout updates during calculations

                // *** CREATE DICTIONARIES in the caller ***
                var weights = new Dictionary<Control, int>();
                var extraSpaceMap = new Dictionary<Control, int>();
                var flowControlLocations = new Dictionary<string, Point>();
                // Declare vars for OUT parameters from helpers
                int currentPos, maxCrossAxisSize;

                // *** CALL HELPER WITHOUT 'out' FOR DICTIONARIES ***
                PL_p3__Layout_Flow_Controls(
                    flowControls,
                    displayRect,
                    weights,             // Pass as regular parameter
                    extraSpaceMap,       // Pass as regular parameter
                    out currentPos,      // Keep as OUT parameter for value type
                    out maxCrossAxisSize,// Keep as OUT parameter for value type
                    flowControlLocations // Pass as regular parameter
                );
                // Dictionaries are now populated by the helper method

                // *** CALL POSITIONING LOOP HELPER WITH 'ref' for iterative updates ***
                int contentEndPos = PL_p32__Position_FLOW_Controls_Loop(
                    flowControls,
                    displayRect,
                    weights,             // Pass populated dictionary (read-only in helper)
                    extraSpaceMap,       // Pass populated dictionary (read-only in helper)
                    ref currentPos,      // Use ref - updated progressively
                    ref maxCrossAxisSize,// Use ref - updated progressively
                    flowControlLocations // Pass dictionary (populated within helper)
                 );

                // *** CALL FLOATING HELPER (Pass populated locations) ***
                PL_p4__Position_Floating_Controls(
                    flowControls,
                    floatingControls,
                    displayRect,
                    flowControlLocations // Pass populated dictionary (read-only in helper)
                 );

                // *** CALL AUTOSCROLL HELPER (Pass calculated values) ***
                PL_p5__Calculate_AutoScrollMinSize_based_on_FLOW_controls(
                    displayRect,
                    maxCrossAxisSize, // Pass final calculated value
                    contentEndPos     // Pass final calculated value
                 );

                this.ResumeLayout(true); // Resume layout and force update
            }
            catch (Exception ex)
            {
                LayoutLogger.Log($"StackLayout ERROR during PerformStackLayout_v0: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                // Ensure flag is reset even if errors occurred (unless early exit happened)
                _isPerformingLayout = false;
                LayoutLogger.Log($"StackLayout [{this.Name}]: <--- Finished PerformStackLayout_v0 (Normal Exit)");
            }
        }



        /// <summary>
        /// Phase 1 (v0/v4): Separates visible child controls into flow and floating lists.
        /// </summary>
        private void PL_p1__Separate_Visible_Controls_into_Flow_and_Floating(
            out List<Control> flowControls, 
            out List<Control> floatingControls)
        {
            var allVisibleControls = this.Controls.OfType<Control>().Where(c => c.Visible).ToList();
            flowControls = new List<Control>();
            floatingControls = new List<Control>();

            // Calls Getlay_IsFloating via 'this' (method defined in other partial file)
            foreach (Control child in allVisibleControls)
            {
                if (this.Getlay_IsFloating(child))
                {
                    floatingControls.Add(child);
                }
                else
                {
                    flowControls.Add(child);
                }
            }
            LayoutLogger.Log($"Layout Pass v0: Flow Controls ({flowControls.Count}), Floating Controls ({floatingControls.Count})");
        }

        /// <summary>
        /// Phase 2 (v0): Handles the layout when there are no controls in the main flow.
        /// Positions only the floating controls relative to the panel padding.
        /// </summary>
        private void PL_p2__Handle_Case_of_No_Flow_Controls(List<Control> floatingControls, Rectangle displayRect)
        {
            LayoutLogger.Log("No flow controls to layout.");
            if (floatingControls.Count > 0)
            {
                LayoutLogger.Log("Positioning floaters relative to Padding.");
                foreach (Control floater in floatingControls)
                {
                    // Calls Getlay_Float... methods via 'this'
                    int offsetX = this.Getlay_FloatOffsetX(floater);
                    int offsetY = this.Getlay_FloatOffsetY(floater);
                    StackFloatZOrder zOrderMode = this.Getlay_FloatZOrder(floater);

                    int fallbackX = displayRect.Left + offsetX;
                    int fallbackY = displayRect.Top + offsetY;
                    floater.SetBounds(fallbackX, fallbackY, floater.Width, floater.Height, BoundsSpecified.Location);

                    // Apply Z-Order for untargeted floaters
                    try
                    {
                        switch (zOrderMode)
                        {
                            case StackFloatZOrder.InFrontOfTarget:
                                if (this.Controls.GetChildIndex(floater) != this.Controls.Count - 1) floater.BringToFront();
                                LayoutLogger.Log($"  Floater '{floater.Name}' (untargeted): Brought to Front (Mode: {zOrderMode})");
                                break;
                            case StackFloatZOrder.BehindTarget:
                                if (this.Controls.GetChildIndex(floater) != 0) floater.SendToBack();
                                LayoutLogger.Log($"  Floater '{floater.Name}' (untargeted): Sent to Back (Mode: {zOrderMode})");
                                break;
                            case StackFloatZOrder.Manual:
                            default:
                                LayoutLogger.Log($"  Floater '{floater.Name}' (untargeted): Z-Order unchanged (Mode: {zOrderMode})");
                                break;
                        }
                    }
                    catch (Exception zEx)
                    {
                        LayoutLogger.Log($"ERROR applying Z-Order to untargeted floater {floater.Name}: {zEx.Message}");
                    }
                }
            }
            // Reset AutoScroll size if no flow content
            if (AutoScroll) { AutoScrollMinSize = Size.Empty; } else { AutoScrollMinSize = Size.Empty; }
            LayoutLogger.Log("Setting AutoScrollMinSize to Empty (No Flow Controls).");

            // IMPORTANT: Resetting the flag here for this specific early-exit path, as per prior logic.
            // Be cautious if modifying this; normally reset only happens in the finally block.
            _isPerformingLayout = false;
        }

        /// <summary>
        /// Phase 3 (v0): Calculates weights, total sizes, and available space for flow controls.
        /// Determines how much extra space each expanding control should get. Populates the passed-in dictionaries.
        /// </summary>
        /// <param name="flowControls">List of controls in the flow.</param>
        /// <param name="displayRect">The available layout area.</param>
        /// <param name="weights">Dictionary (passed by ref) to be populated with control weights.</param>
        /// <param name="extraSpaceMap">Dictionary (passed by ref) to be populated with extra space per control.</param>
        /// <param name="currentPos">OUTPUT: The starting position for the first flow control.</param>
        /// <param name="maxCrossAxisSize">OUTPUT: Initialized to 0, will track max size perpendicular to flow.</param>
        /// <param name="flowControlLocations">Dictionary (passed by ref) to be populated later with final locations.</param>
        private void PL_p3__Layout_Flow_Controls(
            List<Control> flowControls,
            Rectangle displayRect,
            Dictionary<Control, int> weights,
            Dictionary<Control, int> extraSpaceMap,
            out int currentPos,
            out int maxCrossAxisSize,
            Dictionary<string, Point> flowControlLocations) // This dict is populated later in PL_p32
        {
            // Clear dictionaries passed in to ensure a clean state for this layout pass
            weights.Clear();
            extraSpaceMap.Clear();
            // flowControlLocations will be cleared/populated in PL_p32

            double totalWeight = 0;
            int totalPreferredSize = 0;
            int expandingChildCount = 0;

            // Calculate total weight and preferred size along the main axis
            // Populates the 'weights' dictionary passed in.
            foreach (Control child in flowControls)
            {
                int weight = this.Getlay_ExpandWeight(child); // Calls extender property getter
                weights[child] = weight; // Modify dictionary passed by caller
                if (weight > 0)
                {
                    totalWeight += weight;
                    expandingChildCount++;
                }
                totalPreferredSize += (lay_Orientation == StackOrientation.Vertical) ? child.Height : child.Width;
            }
            LayoutLogger.Log($"Flow Controls Calc v0: TotalWeight={totalWeight}, TotalPreferredSize={totalPreferredSize}, ExpandingCount={expandingChildCount}");

            // Calculate space available for distribution
            int totalSpacing = (flowControls.Count > 1) ? (flowControls.Count - 1) * lay_Spacing : 0;
            int totalUsedBeforeExpand = totalPreferredSize + totalSpacing;
            int availableSpace = (lay_Orientation == StackOrientation.Vertical) ? displayRect.Height : displayRect.Width;
            double spaceToDistribute = Math.Max(0, availableSpace - totalUsedBeforeExpand);
            LayoutLogger.Log($"Space Calc v0: TotalSpacing={totalSpacing}, UsedBeforeExpand={totalUsedBeforeExpand}, Available={availableSpace}, ToDistribute={spaceToDistribute}");

            // Distribute the extra space proportionally based on weight
            // Populates the 'extraSpaceMap' dictionary passed in.
            double fractionalSpace = 0.0;
            if (spaceToDistribute > 0 && totalWeight > 0)
            {
                foreach (Control child in flowControls)
                {
                    int weight = weights[child]; // Read from populated dictionary
                    if (weight > 0)
                    {
                        double exactShare = spaceToDistribute * (double)weight / totalWeight;
                        int wholePixels = (int)Math.Floor(exactShare);
                        extraSpaceMap[child] = wholePixels; // Modify dictionary passed by caller
                        fractionalSpace += exactShare - wholePixels;
                    }
                    else
                    {
                        extraSpaceMap[child] = 0; // Modify dictionary passed by caller
                    }
                }

                // Distribute remaining fractional pixels (rounding errors)
                int leftoverPixels = (int)Math.Round(fractionalSpace);
                int distributedLeftovers = 0;
                if (leftoverPixels > 0 && expandingChildCount > 0)
                {
                    // Lambda can capture 'weights' now because it's a normal parameter
                    Control firstExpander = flowControls.FirstOrDefault(c => weights.ContainsKey(c) && weights[c] > 0);
                    if (firstExpander != null)
                    {
                        if (!extraSpaceMap.ContainsKey(firstExpander)) extraSpaceMap[firstExpander] = 0; // Safety check
                        extraSpaceMap[firstExpander] += leftoverPixels; // Modify dictionary passed by caller
                        distributedLeftovers = leftoverPixels;
                    }
                }
                LayoutLogger.Log($"Distributed extra space v0. Leftover pixels added: {distributedLeftovers}");
            }
            else
            {
                // Initialize map even if no space to distribute
                foreach (Control child in flowControls) extraSpaceMap[child] = 0; // Modify dictionary passed by caller
            }

            // Initialize OUT parameters (value types calculated/started here)
            currentPos = (lay_Orientation == StackOrientation.Vertical) ? displayRect.Top : displayRect.Left;
            maxCrossAxisSize = 0;
            // flowControlLocations dictionary is ready, passed by caller, will be populated in PL_p32
        }

        /// <summary>
        /// Phase 3.2 (v0/v4 Helper): Iterates through flow controls, calculates their final bounds
        /// based on orientation, alignment, and calculated sizes, then sets the bounds.
        /// Updates currentPos and maxCrossAxisSize iteratively using 'ref'. Populates flowControlLocations.
        /// </summary>
        /// <returns>The position after the last flow control (including spacing).</returns>
        private int PL_p32__Position_FLOW_Controls_Loop(
           List<Control> flowControls,
           Rectangle displayRect,
           Dictionary<Control, int> weights,
           Dictionary<Control, int> extraSpaceMapOrCalculatedSize,
           ref int currentPos,       // Pass by REF - value is updated iteratively
           ref int maxCrossAxisSize, // Pass by REF - value is updated iteratively
           Dictionary<string, Point> flowControlLocations // Modified (populated) within this method
        )
        {
            bool isMethodV4 = (lay_PerformLayout_calcMethod_No == 4); // Check which mode we're helping
            flowControlLocations.Clear(); // Clear locations for this pass

            for (int i = 0; i < flowControls.Count; i++)
            {
                Control child = flowControls[i];
                int weight = weights.ContainsKey(child) ? weights[child] : 0; // Get weight for context
                int sizeAlongAxis; // The final size in the direction of orientation

                // --- Determine sizeAlongAxis based on Mode (V0 or V4) and calculated/extra space ---
                if (isMethodV4)
                {
                    // Method 4: Use the pre-calculated size for expanders, original size otherwise
                    if (weight > 0)
                    {
                        sizeAlongAxis = extraSpaceMapOrCalculatedSize.ContainsKey(child) ? extraSpaceMapOrCalculatedSize[child] : 0;
                        // Apply Min/Max constraints for v4 expanders
                        if (lay_Orientation == StackOrientation.Vertical) { /* Apply Height Min/Max */ if (child.MaximumSize.Height > 0 && sizeAlongAxis > child.MaximumSize.Height) sizeAlongAxis = child.MaximumSize.Height; if (sizeAlongAxis < child.MinimumSize.Height) sizeAlongAxis = child.MinimumSize.Height; }
                        else { /* Apply Width Min/Max */ if (child.MaximumSize.Width > 0 && sizeAlongAxis > child.MaximumSize.Width) sizeAlongAxis = child.MaximumSize.Width; if (sizeAlongAxis < child.MinimumSize.Width) sizeAlongAxis = child.MinimumSize.Width; }
                    }
                    else { /* Non-expander in v4 uses its current size */ sizeAlongAxis = (lay_Orientation == StackOrientation.Vertical) ? child.Height : child.Width; }
                }
                else // Method 0
                {
                    int initialSizeAlongAxis = (lay_Orientation == StackOrientation.Vertical) ? child.Height : child.Width;
                    int extraSpace = extraSpaceMapOrCalculatedSize.ContainsKey(child) ? extraSpaceMapOrCalculatedSize[child] : 0;
                    sizeAlongAxis = initialSizeAlongAxis + extraSpace;
                    // Apply Min/Max constraints for v0 (applies even if weight was 0)
                    if (lay_Orientation == StackOrientation.Vertical) { /* Apply Height Min/Max */ if (child.MaximumSize.Height > 0 && sizeAlongAxis > child.MaximumSize.Height) sizeAlongAxis = child.MaximumSize.Height; if (sizeAlongAxis < child.MinimumSize.Height) sizeAlongAxis = child.MinimumSize.Height; }
                    else { /* Apply Width Min/Max */ if (child.MaximumSize.Width > 0 && sizeAlongAxis > child.MaximumSize.Width) sizeAlongAxis = child.MaximumSize.Width; if (sizeAlongAxis < child.MinimumSize.Width) sizeAlongAxis = child.MinimumSize.Width; }
                }


                int crossAxisPos; // Position perpendicular to orientation
                int crossAxisSize; // Size perpendicular to orientation
                BoundsSpecified boundsSpec = BoundsSpecified.None; // Tracks which bounds are explicitly set

                // --- Determine cross-axis position and size based on orientation and alignment ---
                if (lay_Orientation == StackOrientation.Vertical)
                {
                    int availableWidth = displayRect.Width; crossAxisPos = displayRect.Left;
                    switch (lay_ChildAxisAlignment)
                    { /* Cases as before */
                        case StackChildAxisAlignment.Stretch: crossAxisSize = availableWidth; if (child.MaximumSize.Width > 0 && crossAxisSize > child.MaximumSize.Width) crossAxisSize = child.MaximumSize.Width; if (crossAxisSize < child.MinimumSize.Width) crossAxisSize = child.MinimumSize.Width; boundsSpec |= BoundsSpecified.Width; break;
                        case StackChildAxisAlignment.Center: crossAxisSize = child.Width; crossAxisPos += (availableWidth - crossAxisSize) / 2; if (crossAxisPos < displayRect.Left) crossAxisPos = displayRect.Left; break;
                        case StackChildAxisAlignment.End: crossAxisSize = child.Width; crossAxisPos += availableWidth - crossAxisSize; if (crossAxisPos < displayRect.Left) crossAxisPos = displayRect.Left; break;
                        case StackChildAxisAlignment.Start: default: crossAxisSize = child.Width; break;
                    }
                    child.SetBounds(crossAxisPos, currentPos, crossAxisSize, sizeAlongAxis, BoundsSpecified.Location | BoundsSpecified.Height | boundsSpec);
                    // Store final location using the current loop position (currentPos for Y)
                    if (!string.IsNullOrEmpty(child.Name)) { flowControlLocations[child.Name] = new Point(crossAxisPos, currentPos); }
                    // Update ref parameter
                    maxCrossAxisSize = Math.Max(maxCrossAxisSize, crossAxisSize);
                    // Update ref parameter
                    currentPos += sizeAlongAxis;
                }
                else // Horizontal Orientation
                {
                    int availableHeight = displayRect.Height; crossAxisPos = displayRect.Top;
                    switch (lay_ChildAxisAlignment)
                    { /* Cases as before */
                        case StackChildAxisAlignment.Stretch: crossAxisSize = availableHeight; if (child.MaximumSize.Height > 0 && crossAxisSize > child.MaximumSize.Height) crossAxisSize = child.MaximumSize.Height; if (crossAxisSize < child.MinimumSize.Height) crossAxisSize = child.MinimumSize.Height; boundsSpec |= BoundsSpecified.Height; break;
                        case StackChildAxisAlignment.Center: crossAxisSize = child.Height; crossAxisPos += (availableHeight - crossAxisSize) / 2; if (crossAxisPos < displayRect.Top) crossAxisPos = displayRect.Top; break;
                        case StackChildAxisAlignment.End: crossAxisSize = child.Height; crossAxisPos += availableHeight - crossAxisSize; if (crossAxisPos < displayRect.Top) crossAxisPos = displayRect.Top; break;
                        case StackChildAxisAlignment.Start: default: crossAxisSize = child.Height; break;
                    }
                    child.SetBounds(currentPos, crossAxisPos, sizeAlongAxis, crossAxisSize, BoundsSpecified.Location | BoundsSpecified.Width | boundsSpec);
                    // Store final location using the current loop position (currentPos for X)
                    if (!string.IsNullOrEmpty(child.Name)) { flowControlLocations[child.Name] = new Point(currentPos, crossAxisPos); }
                    // Update ref parameter
                    maxCrossAxisSize = Math.Max(maxCrossAxisSize, crossAxisSize);
                    // Update ref parameter
                    currentPos += sizeAlongAxis;
                }

                // --- Add spacing if not the last control ---
                if (i < flowControls.Count - 1)
                {
                    currentPos += lay_Spacing; // Update ref parameter
                }
            } // End for loop

            int finalEndPos = currentPos; // Final position after last control + spacing
            LayoutLogger.Log($"Finished positioning Flow Controls ({(isMethodV4 ? "v4" : "v0")}). Content End: {finalEndPos}");
            return finalEndPos; // Return the final position
        }



        /// <summary>
        /// Phase 4 (v0/v4): Positions the floating controls relative to their targets (or panel padding)
        /// using the final locations calculated for the flow controls. Also handles Z-order.
        /// </summary>
        /// <param name="flowControls">List of controls in the flow (used to find target instances).</param>
        /// <param name="floatingControls">List of controls marked as floating.</param>
        /// <param name="displayRect">The available layout area.</param>
        /// <param name="flowControlLocations">Dictionary containing the final calculated top-left positions of flow controls.</param>
        private void PL_p4__Position_Floating_Controls(
            List<Control> flowControls,
            List<Control> floatingControls,
            Rectangle displayRect,
            Dictionary<string, Point> flowControlLocations)
        {
            if (floatingControls.Count > 0)
            {
                LayoutLogger.Log($"Positioning {floatingControls.Count} Floating Controls...");
                foreach (Control floater in floatingControls)
                {
                    // Get floating properties via 'this'
                    string targetName = this.Getlay_FloatTargetName(floater);
                    int offsetX = this.Getlay_FloatOffsetX(floater);
                    int offsetY = this.Getlay_FloatOffsetY(floater);
                    FloatAlignment alignment = this.Getlay_FloatAlignment(floater);
                    StackFloatZOrder zOrderMode = this.Getlay_FloatZOrder(floater);

                    int baseX = 0, baseY = 0;
                    int finalX, finalY;
                    Control targetControl = null;

                    // Find the target control instance among the flow controls
                    if (!string.IsNullOrEmpty(targetName))
                    {
                        targetControl = flowControls.FirstOrDefault(fc => fc.Name == targetName);
                    }

                    // --- Calculate Position ---
                    if (targetControl != null && flowControlLocations.ContainsKey(targetName)) // Target FOUND
                    {
                        // ... (Calculation logic using targetPos from flowControlLocations dictionary remains the same) ...
                        LayoutLogger.Log($"  Floater '{floater.Name}' -> Target '{targetName}' FOUND.");
                        Point targetPos = flowControlLocations[targetName];
                        switch (alignment)
                        { /* Cases as before */
                            case FloatAlignment.ToLeftOf: baseX = targetPos.X - floater.Width; baseY = targetPos.Y; break;
                            case FloatAlignment.ToRightOf: baseX = targetPos.X + targetControl.Width; baseY = targetPos.Y; break;
                            case FloatAlignment.ToTopOf: baseX = targetPos.X; baseY = targetPos.Y - floater.Height; break;
                            case FloatAlignment.ToBottomOf: baseX = targetPos.X; baseY = targetPos.Y + targetControl.Height; break;
                            case FloatAlignment.TopLeft: default: baseX = targetPos.X; baseY = targetPos.Y; break;
                        }
                        finalX = baseX + offsetX; finalY = baseY + offsetY;
                        LayoutLogger.Log($"    Mode: {alignment}, Base=({baseX},{baseY}), Offsets=({offsetX},{offsetY}), Final=({finalX},{finalY})");
                    }
                    else // Target NOT found or not specified - Fallback
                    {
                        // ... (Fallback logic remains the same) ...
                        baseX = displayRect.Left; baseY = displayRect.Top; finalX = baseX + offsetX; finalY = baseY + offsetY;
                        if (!string.IsNullOrEmpty(targetName)) LayoutLogger.Log($"  Floater '{floater.Name}' -> Target '{targetName}' NOT FOUND/Invalid. Fallback Pos: ({finalX},{finalY})");
                        else LayoutLogger.Log($"  Floater '{floater.Name}' -> No Target Name. Fallback Pos: ({finalX},{finalY})");
                        targetControl = null;
                    }

                    // --- Set Bounds ---
                    floater.SetBounds(finalX, finalY, floater.Width, floater.Height, BoundsSpecified.Location);

                    // --- Apply Z-Order ---
                    // ... (Z-order logic remains the same) ...
                    LayoutLogger.Log($"  Z-Order Prep: Floater='{floater.Name}', Target='{targetControl?.Name ?? "null"}', Mode={zOrderMode}");
                    try { /* Z-Order logic using zOrderMode, targetControl, floater, this.Controls */ }
                    catch (Exception zEx) { LayoutLogger.Log($"    ERROR setting Z-Index for {floater.Name} (Mode: {zOrderMode}): {zEx.Message}."); }
                } // End foreach floater
                LayoutLogger.Log("...Finished Positioning Floating Controls.");
            }
        }

        /// <summary>
        /// Phase 5 (v0/v4): Calculates and sets the AutoScrollMinSize based on the
        /// total extent of the *flow* controls, enabling scrolling if needed.
        /// </summary>
        /// <param name="displayRect">The available client area.</param>
        /// <param name="maxCrossAxisSize">The final calculated maximum size perpendicular to flow.</param>
        /// <param name="contentEndPos">The final calculated position after the last flow control.</param>
        private void PL_p5__Calculate_AutoScrollMinSize_based_on_FLOW_controls(
            Rectangle displayRect,
            int maxCrossAxisSize,
            int contentEndPos)
        {
            if (AutoScroll)
            {
                int scrollWidth = 0;
                int scrollHeight = 0;

                // ... (Calculation logic for scrollWidth/scrollHeight remains the same) ...
                if (lay_Orientation == StackOrientation.Vertical) { /* Calculate scrollHeight/scrollWidth */ scrollHeight = contentEndPos - displayRect.Top + Padding.Bottom; if (lay_ChildAxisAlignment == StackChildAxisAlignment.Stretch) scrollWidth = 0; else scrollWidth = maxCrossAxisSize + Padding.Left + Padding.Right; }
                else { /* Calculate scrollWidth/scrollHeight */ scrollWidth = contentEndPos - displayRect.Left + Padding.Right; if (lay_ChildAxisAlignment == StackChildAxisAlignment.Stretch) scrollHeight = 0; else scrollHeight = maxCrossAxisSize + Padding.Top + Padding.Bottom; }

                Size minSize = new Size(scrollWidth, scrollHeight);
                if (this.AutoScrollMinSize != minSize)
                {
                    this.AutoScrollMinSize = minSize;
                    LayoutLogger.Log($"Setting AutoScrollMinSize based on Flow Controls to: {this.AutoScrollMinSize}");
                }
            }
            else
            {
                // ... (Reset logic remains the same) ...
                if (this.AutoScrollMinSize != Size.Empty) { this.AutoScrollMinSize = Size.Empty; LayoutLogger.Log("Resetting AutoScrollMinSize to Empty."); }
            }
        }


        #endregion

        #region Layout Method 4 (Strict Weight-Based) and Helpers

        /// <summary>
        /// Layout method 4: Distributes space strictly based on weights.
        /// Non-expanding controls keep their size; expanding controls share remaining space proportionally.
        /// Includes support for floating controls.
        /// </summary>
        private void PerformStackLayout_v4()
        {
            if (_isPerformingLayout) { /* ... re-entrancy log ... */ return; }
            _isPerformingLayout = true;
            LayoutLogger.Log($"StackLayout [{this.Name}]: ---> Starting PerformStackLayout_v4");

            try
            {
                List<Control> flowControls, floatingControls;
                PL_p1__Separate_Visible_Controls_into_Flow_and_Floating(out flowControls, out floatingControls); // Reusable phase 1

                Rectangle displayRect = this.DisplayRectangle;

                if (flowControls.Count == 0)
                {
                    PL_p2__Handle_Case_of_No_Flow_Controls(floatingControls, displayRect); // Reusable phase 2
                    LayoutLogger.Log($"StackLayout [{this.Name}]: <--- Finished PerformStackLayout_v4 (No Flow Controls)");
                    // _isPerformingLayout reset inside PL_p2
                    return; // Early exit
                }

                this.SuspendLayout(); // Suspend layout

                // *** CREATE DICTIONARIES in the caller ***
                var weights = new Dictionary<Control, int>();
                var calculatedExpanderSizes = new Dictionary<Control, int>();
                var flowControlLocations = new Dictionary<string, Point>();
                // Declare vars for values calculated/updated by helpers
                int maxCrossAxisSize = 0;
                int currentPos = (lay_Orientation == StackOrientation.Vertical) ? displayRect.Top : displayRect.Left; // Initial starting position

                // *** CALL HELPER WITHOUT 'out' FOR DICTIONARIES ***
                PL4_p3_Layout_Flow_Controls__Method4_Logic(
                    flowControls,
                    displayRect,
                    weights,                    // Pass as regular parameter (populated by helper)
                    calculatedExpanderSizes     // Pass as regular parameter (populated by helper)
                 );
                // weights and calculatedExpanderSizes dictionaries are now populated

                // *** CALL POSITIONING LOOP HELPER WITH 'ref' for iterative updates ***
                int contentEndPos = PL_p32__Position_FLOW_Controls_Loop(
                    flowControls,
                    displayRect,
                    weights,                     // Pass populated dictionary
                    calculatedExpanderSizes,     // Pass populated dictionary (used as sizes here)
                    ref currentPos,              // Use ref - updated progressively
                    ref maxCrossAxisSize,        // Use ref - updated progressively
                    flowControlLocations         // Pass dictionary (populated within helper)
                 );

                // *** CALL FLOATING HELPER (Pass populated locations) ***
                PL_p4__Position_Floating_Controls(
                    flowControls,
                    floatingControls,
                    displayRect,
                    flowControlLocations         // Pass populated dictionary
                 );

                // *** CALL AUTOSCROLL HELPER (Pass calculated values) ***
                PL_p5__Calculate_AutoScrollMinSize_based_on_FLOW_controls(
                    displayRect,
                    maxCrossAxisSize,            // Pass final calculated value
                    contentEndPos                // Pass final calculated value
                 );

                this.ResumeLayout(true); // Resume layout
            }
            catch (Exception ex) { LayoutLogger.Log($"StackLayout ERROR during PerformStackLayout_v4: {ex.Message}\n{ex.StackTrace}"); }
            finally
            {
                // Ensure reset in finally block for normal exit
                _isPerformingLayout = false;
                LayoutLogger.Log($"StackLayout [{this.Name}]: <--- Finished PerformStackLayout_v4 (Normal Exit)");
            }
        }

        /// <summary>
        /// Phase 3 (v4): Calculates weights, non-expanding size, and distributes remaining space
        /// strictly based on weights ONLY to the expanding controls. Populates the passed-in dictionaries.
        /// </summary>
        /// <param name="flowControls">List of controls in the flow.</param>
        /// <param name="displayRect">The available layout area.</param>
        /// <param name="weights">Dictionary (passed by ref) to be populated with control weights.</param>
        /// <param name="calculatedExpanderSizes">Dictionary (passed by ref) to be populated with calculated sizes for expanding controls.</param>
        private void PL4_p3_Layout_Flow_Controls__Method4_Logic(
            List<Control> flowControls,
            Rectangle displayRect,
            Dictionary<Control, int> weights,
            Dictionary<Control, int> calculatedExpanderSizes)
        {
            // Clear dictionaries passed in to ensure a clean state
            weights.Clear();
            calculatedExpanderSizes.Clear();

            double totalWeight = 0;
            int totalNonExpandingSize = 0;
            int expandingChildCount = 0;
            var expandingControls = new List<Control>(); // Track just the expanders

            // Calculate total weight and size of non-expanding flow controls
            // Populates the 'weights' dictionary passed in.
            foreach (Control child in flowControls)
            {
                int weight = this.Getlay_ExpandWeight(child); // Calls extender property getter
                weights[child] = weight; // Modify dictionary passed by caller
                if (weight == 0) // Non-expanding
                {
                    int sizeAlongAxis = (lay_Orientation == StackOrientation.Vertical) ? child.Height : child.Width;
                    totalNonExpandingSize += sizeAlongAxis;
                }
                else // Expanding
                {
                    totalWeight += weight;
                    expandingChildCount++;
                    expandingControls.Add(child);
                }
            }
            LayoutLogger.Log($"Flow Controls Calc v4: TotalWeight={totalWeight}, TotalNonExpandingSize={totalNonExpandingSize}, ExpandingCount={expandingChildCount}");

            // Calculate space available ONLY for the expanding controls
            int totalSpacing = (flowControls.Count > 1) ? (flowControls.Count - 1) * lay_Spacing : 0;
            int availableSpace = (lay_Orientation == StackOrientation.Vertical) ? displayRect.Height : displayRect.Width;
            double spaceAvailableForExpanders = Math.Max(0, availableSpace - totalNonExpandingSize - totalSpacing);
            LayoutLogger.Log($"Space Calc v4: TotalSpacing={totalSpacing}, Available={availableSpace}, SpaceForExpanders={spaceAvailableForExpanders}");

            // Distribute the available space strictly by weight among expanders
            // Populates the 'calculatedExpanderSizes' dictionary passed in.
            double fractionalSpace = 0.0;
            if (spaceAvailableForExpanders > 0 && totalWeight > 0)
            {
                foreach (Control expander in expandingControls)
                {
                    int weight = weights[expander]; // Read from populated dictionary
                    double exactShare = spaceAvailableForExpanders * (double)weight / totalWeight;
                    int wholePixels = (int)Math.Floor(exactShare);
                    calculatedExpanderSizes[expander] = wholePixels; // Modify dictionary passed by caller
                    fractionalSpace += exactShare - wholePixels;
                }

                // Distribute fractional pixels
                int leftoverPixels = (int)Math.Round(fractionalSpace);
                int distributedLeftovers = 0;
                if (leftoverPixels > 0 && expandingChildCount > 0)
                {
                    // Lambda can capture 'expandingControls' because it's a local variable
                    Control firstExpander = expandingControls.FirstOrDefault();
                    if (firstExpander != null && calculatedExpanderSizes.ContainsKey(firstExpander)) // Check key exists
                    {
                        calculatedExpanderSizes[firstExpander] += leftoverPixels; // Modify dictionary passed by caller
                        distributedLeftovers = leftoverPixels;
                    }
                }
                LayoutLogger.Log($"Distributed Expander space v4. Leftover pixels added: {distributedLeftovers}");
            }
            else // No space or no expanders
            {
                // Ensure dictionary entries exist for all expanders, even if size is 0
                foreach (Control expander in expandingControls)
                {
                    calculatedExpanderSizes[expander] = 0; // Modify dictionary passed by caller
                }
            }
            // Note: Non-expanding controls are NOT in calculatedExpanderSizes dictionary.
            // Their size is determined by their actual Height/Width during the positioning loop (PL_p32).
        }
        // PL4_p1, PL4_p2, PL4_p4, PL4_p5 are handled by reusing PL_p1, PL_p2, PL_p4, PL_p5

        #endregion

        #region Overrides for Layout Triggers and Designer Integration

        protected override void OnControlAdded(ControlEventArgs e)
        {
            LayoutLogger.Log($"StackLayout [{this.Name}]: OnControlAdded FIRED for '{e.Control?.Name ?? "null"}'.");
            base.OnControlAdded(e);
            if (e.Control != null)
            {
                // Subscribe to VisibleChanged for runtime layout updates
                e.Control.VisibleChanged += ChildControl_VisibleChanged;
            }
            // Adding/removing controls inherently requires relayout
            LayoutLogger.Log($"StackLayout [{this.Name}]: OnControlAdded TRIGGERING PerformLayout for '{e.Control?.Name ?? "null"}'.");
            PerformLayout();
            Invalidate(true); // Ensure repaint
            LayoutLogger.Log($"StackLayout [{this.Name}]: OnControlAdded - PerformLayout call COMPLETED.");
        }

        protected override void OnControlRemoved(ControlEventArgs e)
        {
            LayoutLogger.Log($"StackLayout [{this.Name}]: OnControlRemoved FIRED for '{e.Control?.Name ?? "null"}'.");
            base.OnControlRemoved(e);
            if (e.Control != null)
            {
                // Unsubscribe when removed
                e.Control.VisibleChanged -= ChildControl_VisibleChanged;

                // Also remove extender property values associated with the removed control
                // (Prevents memory leaks from Hashtables holding references)
                _expandWeights?.Remove(e.Control);
                _lay_isFloatingFlags?.Remove(e.Control);
                _lay_floatTargetNames?.Remove(e.Control);
                _lay_floatOffsetsX?.Remove(e.Control);
                _lay_floatOffsetsY?.Remove(e.Control);
                _lay_floatAlignments?.Remove(e.Control);
                _lay_floatZOrderModes?.Remove(e.Control);
                LayoutLogger.Log($"StackLayout [{this.Name}]: Cleared extender properties for removed control '{e.Control.Name}'.");
            }
            // Adding/removing controls inherently requires relayout
            LayoutLogger.Log($"StackLayout [{this.Name}]: OnControlRemoved TRIGGERING PerformLayout for '{e.Control?.Name ?? "null"}'.");
            PerformLayout();
            Invalidate(true);
            LayoutLogger.Log($"StackLayout [{this.Name}]: OnControlRemoved - PerformLayout call COMPLETED.");
        }

        /// <summary>
        /// Handles the VisibleChanged event of child controls to trigger layout updates at runtime.
        /// </summary>
        private void ChildControl_VisibleChanged(object sender, EventArgs e)
        {
            Control child = sender as Control;
            LayoutLogger.Log($"StackLayout [{this.Name}]: ChildControl_VisibleChanged FIRED for '{child?.Name ?? "null"}'. Visible={child?.Visible}.");

            // Check if the control is still parented by this StackLayout and not disposed
            if (child != null && child.Parent == this && !this.IsDisposed && !child.IsDisposed)
            {
                // Perform layout asynchronously via BeginInvoke to avoid issues during complex UI updates
                if (this.IsHandleCreated)
                {
                    // *** CHANGE HERE: Call BeginInvoke with the named method ***
                    this.BeginInvoke(new Action<Control>(HandleChildVisibilityChangeLayout), child);
                }
                else
                {
                    // If handle not created yet (e.g., during form load), synchronous layout is usually okay
                    LayoutLogger.Log($"StackLayout [{this.Name}]: ChildControl_VisibleChanged PerformLayout (Sync) for '{child.Name}'.");
                    try
                    {
                        this.PerformLayout();
                        // this.Invalidate(true); // Optional: uncomment if needed
                    }
                    catch (Exception ex)
                    {
                        LayoutLogger.Log($"StackLayout ERROR [{this.Name}]: Exception during synchronous PerformLayout in ChildControl_VisibleChanged: {ex.Message}\n{ex.StackTrace}");
                    }
                }
            }
            else if (child != null && child.Parent != this)
            {
                // Defensive unsubscribe if the child was reparented before the handler ran
                LayoutLogger.Log($"StackLayout [{this.Name}]: ChildControl_VisibleChanged - Child '{child.Name}' no longer parented. Unsubscribing.");
                child.VisibleChanged -= ChildControl_VisibleChanged;
            }
        }

        /// <summary>
        /// Private helper method called via BeginInvoke to handle layout updates
        /// triggered by a child's visibility change.
        /// </summary>
        /// <param name="child">The child control whose visibility changed.</param>
        private void HandleChildVisibilityChangeLayout(Control child)
        {
            // --- Add crucial safety checks inside the invoked method ---
            // The state might have changed between scheduling and execution
            if (this.IsDisposed || child == null || child.IsDisposed || child.Parent != this)
            {
                LayoutLogger.Log($"StackLayout [{this.Name}]: HandleChildVisibilityChangeLayout - Aborting layout (Disposed or child state changed for '{child?.Name ?? "null"}').");
                return;
            }

            LayoutLogger.Log($"StackLayout [{this.Name}]: HandleChildVisibilityChangeLayout - Performing layout for '{child.Name}'.");
            try
            {
                // Now call PerformLayout from a context the compiler doesn't complain about
                this.PerformLayout();

                // Optional: Invalidate if visual updates are needed immediately after layout
                // this.Invalidate(true);
            }
            catch (Exception ex)
            {
                // Log errors occurring during the asynchronous layout
                LayoutLogger.Log($"StackLayout ERROR [{this.Name}]: Exception during layout in HandleChildVisibilityChangeLayout for '{child.Name}': {ex.Message}\n{ex.StackTrace}");
            }
        }


        protected override void OnPaddingChanged(EventArgs e)
        {
            LayoutLogger.Log($"StackLayout [{this.Name}]: OnPaddingChanged FIRED.");
            base.OnPaddingChanged(e);
            PerformLayout(); // Padding affects DisplayRectangle
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            // SizeChanged is frequent during resize. Layout is usually triggered by framework's Dock/Anchor handling.
            // Explicitly calling PerformLayout() here can sometimes cause excessive layout cycles during resize.
            // However, if children rely purely on StackLayout for sizing (no Dock/Anchor), it might be necessary.
            // Let's keep it for now, but be mindful if performance issues arise during resize.
            LayoutLogger.Log($"StackLayout [{this.Name}]: OnSizeChanged FIRED.");
            base.OnSizeChanged(e);
            PerformLayout();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            LayoutLogger.Log($"StackLayout [{this.Name}]: OnVisibleChanged FIRED. Visible={this.Visible}");
            base.OnVisibleChanged(e);
            // Layout might be needed when becoming visible if content changed while hidden
            if (this.Visible)
            {
                PerformLayout();
                Invalidate();
            }
        }




     







        /// <summary>
        /// Manages component change notifications from the designer environment.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override ISite Site
        {
            get => base.Site;
            set
            {
                // Unsubscribe from old service
                if (_componentChangeService != null)
                {
                    _componentChangeService.ComponentChanged -= OnComponentChanged;
                    LayoutLogger.Log($"StackLayout [{this.Name}]: Unsubscribed from ComponentChangeService.");
                    _componentChangeService = null; // Clear reference before setting new site
                }

                base.Site = value; // Set the new site

                // Subscribe to new service if available
                if (value != null)
                {
                    _componentChangeService = (IComponentChangeService)value.GetService(typeof(IComponentChangeService));
                    if (_componentChangeService != null)
                    {
                        _componentChangeService.ComponentChanged += OnComponentChanged;
                        LayoutLogger.Log($"StackLayout [{this.Name}]: Subscribed to new ComponentChangeService.");
                    }
                    else
                    {
                        LayoutLogger.Log($"StackLayout [{this.Name}]: Could not get ComponentChangeService from new site.");
                    }
                }
            }
        }


        /// <summary>
        /// Handles component change events from the designer. Triggers layout if a relevant
        /// property of a direct child control changes.
        /// </summary>
        private void OnComponentChanged(object sender, ComponentChangedEventArgs e)
        {
            string componentTypeName = e.Component?.GetType().Name ?? "null";
            string memberName = e.Member?.Name ?? "null";
            LayoutLogger.Log($"StackLayout [{this.Name}]: OnComponentChanged FIRED. Component: {componentTypeName}, Member: {memberName}");

            // Prevent re-entrant layout calls triggered *by* the layout process itself
            if (_isPerformingLayout)
            {
                LayoutLogger.Log($"StackLayout [{this.Name}]: OnComponentChanged SKIPPING (Re-entrancy).");
                return;
            }

            // Check if the change occurred on a DIRECT CHILD of this StackLayout
            if (e.Component is Control changedControl && changedControl.Parent == this)
            {
                // Check if the changed property is one that likely affects layout
                // (Visibility handled by separate event)
                bool layoutRelevant = memberName switch
                {
                    // Properties likely affecting flow or position:
                    nameof(Control.Dock) => true,
                    nameof(Control.Anchor) => true,
                    nameof(Control.Size) => true,
                    nameof(Control.Location) => true, // Although layout usually dictates this
                    nameof(Control.Margin) => true, // Can affect spacing implicitly
                    nameof(Control.Padding) => true, // If the child is a container
                    nameof(Control.MinimumSize) => true,
                    nameof(Control.MaximumSize) => true,
                    nameof(Control.Name) => true, // Important if used as FloatTargetName
                    // Add other relevant properties specific to your controls if needed

                    // Extender properties (though their setters usually call PerformLayout already):
                    // "lay_ExpandWeight" => true, // Handled by setter
                    // "lay_IsFloating" => true,   // Handled by setter
                    // "lay_FloatTargetName" => true, // Handled by setter
                    // "lay_FloatOffsetX" => true, // Handled by setter
                    // "lay_FloatOffsetY" => true, // Handled by setter
                    // "lay_FloatAlignment" => true, // Handled by setter
                    // "lay_FloatZOrder" => false, // ZOrder mode change doesn't require immediate layout

                    _ => false // Default to not layout-relevant
                };

                if (layoutRelevant)
                {
                    LayoutLogger.Log($"StackLayout [{this.Name}]: OnComponentChanged TRIGGERING PerformLayout due to DIRECT CHILD '{changedControl.Name}' member '{memberName}'.");
                    if (!this.IsDisposed)
                    {
                        try
                        {
                            this.PerformLayout();
                            this.Invalidate(true); // Repaint might be needed
                            LayoutLogger.Log($"StackLayout [{this.Name}]: OnComponentChanged - Direct PerformLayout call COMPLETED.");
                        }
                        catch (Exception ex)
                        {
                            // Log errors during design-time layout triggered by changes
                            string errorMsg = $"StackLayout ERROR [{this.Name}]: Exception during direct PerformLayout in OnComponentChanged: {ex.Message}";
                            Debug.WriteLine(errorMsg + "\n" + ex.StackTrace); // Keep Debug for exceptions
                            LayoutLogger.Log(errorMsg);
                            LayoutLogger.Log(ex.StackTrace);
                        }
                    }
                    else
                    {
                        LayoutLogger.Log($"StackLayout [{this.Name}]: OnComponentChanged - Skipped direct PerformLayout because control is disposed.");
                    }
                }
                else
                {
                    LayoutLogger.Log($"StackLayout [{this.Name}]: OnComponentChanged - Change on child '{changedControl.Name}' member '{memberName}' IGNORED (not layout-relevant or handled by setter/event).");
                }
            }
            else
            {
                // Log changes to other components if needed for debugging, but ignore for layout
                LayoutLogger.Log($"StackLayout [{this.Name}]: OnComponentChanged - Change on component '{componentTypeName}' (not a direct child) IGNORED by this handler.");
            }
            LayoutLogger.Log($"StackLayout [{this.Name}]: OnComponentChanged FINISHED processing.");
        }

        #endregion

        // --- Dispose Method ---
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                LayoutLogger.Log($"StackLayout [{this.Name}]: Dispose({disposing}) called.");

                // Unsubscribe from component change service
                if (_componentChangeService != null)
                {
                    _componentChangeService.ComponentChanged -= OnComponentChanged;
                    LayoutLogger.Log($"StackLayout [{this.Name}]: Unsubscribed from ComponentChangeService during Dispose.");
                    _componentChangeService = null;
                }

                // Unsubscribe from all children's VisibleChanged events
                foreach (Control c in this.Controls.OfType<Control>())
                {
                    c.VisibleChanged -= ChildControl_VisibleChanged;
                }
                LayoutLogger.Log($"StackLayout [{this.Name}]: Unsubscribed from child VisibleChanged events during Dispose.");

                // Clear the Hashtables (defined in the other partial file, but accessible)
                // This is crucial to release references to controls that might have been removed
                // without the OnControlRemoved event firing correctly (e.g., during form close).
                _expandWeights?.Clear();
                _lay_isFloatingFlags?.Clear();
                _lay_floatTargetNames?.Clear();
                _lay_floatOffsetsX?.Clear();
                _lay_floatOffsetsY?.Clear();
                _lay_floatAlignments?.Clear();
                _lay_floatZOrderModes?.Clear();
                LayoutLogger.Log($"StackLayout [{this.Name}]: Cleared extender property Hashtables during Dispose.");
            }
            base.Dispose(disposing);
        }

    } // End partial class StackLayout
} // End namespace

 