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
    public enum StackOrientation
    {
        Vertical,
        Horizontal
    }
    public enum StackChildAxisAlignment
    {
        Stretch,
        Start,
        Center,
        End
    }
    public enum FloatAlignment
    {
        TopLeft,
        ToLeftOf,
        ToRightOf,
        // Potential future additions:
        // Above,
        // Below,
        // Center,
        // TopRight, BottomLeft, BottomRight etc.
    }

        /// <summary>
        /// Defines how a floating control's Z-order is managed relative to its target
        /// within a StackLayout during layout passes.
        /// </summary>
        public enum StackFloatZOrder
        {
            /// <summary>
            /// The floating control is placed visually ON TOP of its target control.
            /// If no target exists, the floater is brought to the front of all controls in the StackLayout. (Default)
            /// </summary>
            InFrontOfTarget,  

            /// <summary>
            /// The floating control is placed visually BEHIND its target control.
            /// If no target exists, the floater is sent to the back of all controls in the StackLayout.
            /// </summary>
            BehindTarget, 

            /// <summary>
            /// The StackLayout's layout process will NOT modify the floating control's Z-order.
            /// Its Z-order should be managed manually (e.g., via designer 'Bring to Front'/'Send to Back').
            /// </summary>
            Manual
        }


    public class StackLayout : Panel
    {
        // --- Constants ---
        public
        const string categorySTR = "L_Layout2";

        // --- Private Fields ---
        private int _spacing = 3;
        private StackOrientation _orientation = StackOrientation.Vertical;
        private StackChildAxisAlignment _childAxisAlignment = StackChildAxisAlignment.Stretch;
        private int _performLayout_calcMethod_No = 0; // Default

        // Removed _extender, _extenderSearched fields as we use LayoutExtenderProvider now
        private IComponentChangeService _componentChangeService = null;
        private bool _isPerformingLayout = false;

        #region Public Properties (Prefixed and Categorized)
        // --- Public Properties (Prefixed and Categorized) ---
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
        [Description("Layout calculation method.\r\n0: Default, Flexible in Designer.\r\n1: Uses GetPreferredSize (Can be problematic).\r\n4: Distributes space purely by weight.")]
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

        // --- Manual Extender Assignment Property ---
        [Browsable(true)]
        [Category(categorySTR)]
        [Description("Manually assigns the StackLayoutExtender instance providing layout weights/floating properties for children. Assign this in Form_Load or Form_Shown.")]
        public StackLayoutExtender LayoutExtenderProvider
        {
            get;
            set;
        }

        // --- Standard Properties (Visible & Categorized) ---
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
                }
            }
        }
        #endregion


        // --- Constructor ---
        public StackLayout()
        {
            /* Optional: this.DoubleBuffered = true; */
        }

        // --- Extender Finder (Simplified) ---
        // Returns the manually assigned provider.
        private StackLayoutExtender GetExtender() => LayoutExtenderProvider;
        private StackLayoutExtender extender => LayoutExtenderProvider;

        // --- Core Layout Logic Switch ---
        protected override void OnLayout(LayoutEventArgs levent)
        {
            // Optimization: Don't layout if invisible or disposing
            if (!this.Visible || this.IsDisposed || this.Disposing) return;

            base.OnLayout(levent);

            // Use the selected calculation method
            switch (lay_PerformLayout_calcMethod_No)
            {
                case 4:
                    PerformStackLayout_v4();
                    break;
                case 0:
                default:
                    PerformStackLayout_old_v0(); // This is the one we modified
                    break;
            }
        }

        // Layout Method 0 (Original - Designer Flexible) - MODIFIED FOR FLOATING & ALIGNMENT
        private void PerformStackLayout_old_v0()
        {
            // Prevent re-entrancy
            if (_isPerformingLayout)
            {
                Debug.WriteLine("StackLayout DEBUG: PerformStackLayout_old_v0 skipped due to re-entrancy flag.");
                return;
            }
            _isPerformingLayout = true;
            Debug.WriteLine($"StackLayout DEBUG: ---> Starting PerformStackLayout_v{lay_PerformLayout_calcMethod_No}");

            //StackLayoutExtender extender = this.LayoutExtenderProvider; // Use manual property
            if (extender == null && this.Controls.OfType<IComponent>().Any())
            {
                var warnMSG = $"StackLayout WARNING: LayoutExtenderProvider is NULL in '{this.Name}', floating/expansion properties will not work!";
                Debug.WriteLine(warnMSG);
                this.CreateGraphics().DrawString(warnMSG, this.Font, Brushes.Red, this.DisplayRectangle);
            }

            try
            {
                // --- 1. Separate Visible Controls into Flow and Floating ---
                List<Control> flowControls, floatingControls;
                PL_p1__Separate_Visible_Controls_into_Flow_and_Floating(out flowControls, out floatingControls);

                Rectangle displayRect = this.DisplayRectangle;

                // --- 2. Handle Case of No Flow Controls ---
                if (flowControls.Count == 0)
                {
                    PL_p2__Handle_Case_of_No_Flow_Controls(floatingControls, displayRect);
                    return;
                }

                // --- 3. Layout Flow Controls ---
                Dictionary<Control, int> weights, extraSpaceMap;
                int currentPos, maxCrossAxisSize;
                Dictionary<string, Point> flowControlLocations;
                PL_p3__Layout_Flow_Controls(flowControls, displayRect, out weights, out extraSpaceMap, out currentPos, out maxCrossAxisSize, out flowControlLocations);

                // --- Position FLOW Controls Loop ---
                int contentEndPos = PL_p32__Position_FLOW_Controls_Loop(flowControls, displayRect, weights, extraSpaceMap, ref currentPos, ref maxCrossAxisSize, flowControlLocations);

                // --- 4. Position Floating Controls ---
                PL_p4__Position_Floating_Controls(flowControls, floatingControls, displayRect, flowControlLocations);

                // --- 5. Calculate AutoScrollMinSize based on FLOW controls ---
                PL_p5__Calculate_AutoScrollMinSize_based_on_FLOW_controls(displayRect, maxCrossAxisSize, contentEndPos);

                // --- Resume Layout ---
                this.ResumeLayout(true); // Force immediate layout
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"StackLayout ERROR during PerformStackLayout_old_v0: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                _isPerformingLayout = false;
                Debug.WriteLine($"StackLayout DEBUG: <--- Finished PerformStackLayout_v{lay_PerformLayout_calcMethod_No}");
            }
        } // End PerformStackLayout_old_v0


        private void PL_p1__Separate_Visible_Controls_into_Flow_and_Floating(out List<Control> flowControls, out List<Control> floatingControls)
        {
            var allVisibleControls = this.Controls.OfType<Control>().Where(c => c.Visible).ToList();
            flowControls = new List<Control>();
            floatingControls = new List<Control>();
            if (extender != null)
            {
                foreach (Control child in allVisibleControls)
                {
                    if (extender.Getlay_IsFloating(child))
                    {
                        floatingControls.Add(child);
                    }
                    else
                    {
                        flowControls.Add(child);
                    }
                }
            }
            else
            {
                flowControls.AddRange(allVisibleControls);
            } // No extender, all are flow
            Debug.WriteLine($"Layout Pass: Flow Controls ({flowControls.Count}), Floating Controls ({floatingControls.Count})");
        }
        private void PL_p2__Handle_Case_of_No_Flow_Controls(List<Control> floatingControls, Rectangle displayRect)
        {
            Debug.WriteLine("No flow controls to layout.");
            if (floatingControls.Count > 0 && extender != null)
            {
                Debug.WriteLine("Positioning floaters relative to Padding.");
                foreach (Control floater in floatingControls)
                {
                    // Use prefixed methods
                    int offsetX = extender.Getlay_FloatOffsetX(floater);
                    int offsetY = extender.Getlay_FloatOffsetY(floater);
                    int fallbackX = displayRect.Left + offsetX;
                    int fallbackY = displayRect.Top + offsetY;
                    floater.SetBounds(fallbackX, fallbackY, floater.Width, floater.Height, BoundsSpecified.Location);
                    // Decide Z-order for untargeted floaters (e.g., send to back)
                    floater.SendToBack();
                }
            }
            if (AutoScroll)
            {
                AutoScrollMinSize = Size.Empty;
                Debug.WriteLine("Setting AutoScrollMinSize to Empty (No Flow Controls).");
            }
            else
            {
                AutoScrollMinSize = Size.Empty;
            }

            _isPerformingLayout = false; // Reset flag before returning
            Debug.WriteLine($"StackLayout DEBUG: <--- Finished PerformStackLayout (No Flow Controls)");
        }
        private void PL_p3__Layout_Flow_Controls(List<Control> flowControls, Rectangle displayRect, out Dictionary<Control, int> weights,
            out Dictionary<Control, int> extraSpaceMap, out int currentPos, out int maxCrossAxisSize, out Dictionary<string, Point> flowControlLocations)
        {
            this.SuspendLayout();

            double totalWeight = 0;
            int totalPreferredSize = 0;
            int expandingChildCount = 0;
            weights = new Dictionary<Control, int>();
            foreach (Control child in flowControls)
            {
                // Use prefixed method (or correct non-prefixed name)
                int weight = extender?.Getlay_ExpandWeight(child) ?? 0;
                weights[child] = weight;
                if (weight > 0)
                {
                    totalWeight += weight;
                    expandingChildCount++;
                }
                totalPreferredSize += (lay_Orientation == StackOrientation.Vertical) ? child.Height : child.Width;
            }
            Debug.WriteLine($"Flow Controls Calc: TotalWeight={totalWeight}, TotalPreferredSize={totalPreferredSize}, ExpandingCount={expandingChildCount}");

            int totalSpacing = (flowControls.Count > 1) ? (flowControls.Count - 1) * lay_Spacing : 0;
            int totalUsedBeforeExpand = totalPreferredSize + totalSpacing;
            int availableSpace = (lay_Orientation == StackOrientation.Vertical) ? displayRect.Height : displayRect.Width;
            double spaceToDistribute = Math.Max(0, availableSpace - totalUsedBeforeExpand);
            Debug.WriteLine($"Space Calc: TotalSpacing={totalSpacing}, UsedBeforeExpand={totalUsedBeforeExpand}, Available={availableSpace}, ToDistribute={spaceToDistribute}");

            double fractionalSpace = 0.0;
            extraSpaceMap = new Dictionary<Control, int>();
            if (spaceToDistribute > 0 && totalWeight > 0)
            {
                foreach (Control child in flowControls)
                {
                    int weight = weights[child];
                    if (weight > 0)
                    {
                        double exactShare = spaceToDistribute * (double)weight / totalWeight;
                        int wholePixels = (int)Math.Floor(exactShare);
                        extraSpaceMap[child] = wholePixels;
                        fractionalSpace += exactShare - wholePixels;
                    }
                    else
                    {
                        extraSpaceMap[child] = 0;
                    }
                }
                int leftoverPixels = (int)Math.Round(fractionalSpace);
                int distributedLeftovers = 0;
                if (leftoverPixels > 0 && expandingChildCount > 0)
                {
                    foreach (Control child in flowControls)
                    {
                        if (weights[child] > 0)
                        {
                            if (!extraSpaceMap.ContainsKey(child)) extraSpaceMap[child] = 0;
                            extraSpaceMap[child]++;
                            distributedLeftovers++;
                            if (distributedLeftovers >= leftoverPixels) break;
                        }
                    }
                }
                Debug.WriteLine($"Distributed extra space. Leftover pixels added: {distributedLeftovers}");
            }
            else
            {
                foreach (Control child in flowControls) extraSpaceMap[child] = 0;
            }

            currentPos = (lay_Orientation == StackOrientation.Vertical) ? displayRect.Top : displayRect.Left;
            maxCrossAxisSize = 0;
            // Dictionary to store final LOCATION (Point) keyed by control NAME for flow controls.
            flowControlLocations = new Dictionary<string, Point>();
        }
        private int PL_p32__Position_FLOW_Controls_Loop(List<Control> flowControls, Rectangle displayRect, Dictionary<Control, int> weights,
            Dictionary<Control, int> extraSpaceMap, ref int currentPos, ref int maxCrossAxisSize, Dictionary<string, Point> flowControlLocations)
        {
            for (int i = 0; i < flowControls.Count; i++)
            {
                Control child = flowControls[i];
                int weight = weights[child];
                int initialSizeAlongAxis = (lay_Orientation == StackOrientation.Vertical) ? child.Height : child.Width;
                int extraSpace = extraSpaceMap.ContainsKey(child) ? extraSpaceMap[child] : 0;
                int calculatedSizeAlongAxis = initialSizeAlongAxis + extraSpace;

                // Apply Min/Max constraints along orientation axis
                if (lay_Orientation == StackOrientation.Vertical)
                {
                    if (child.MaximumSize.Height > 0 && calculatedSizeAlongAxis > child.MaximumSize.Height) calculatedSizeAlongAxis = child.MaximumSize.Height;
                    if (calculatedSizeAlongAxis < child.MinimumSize.Height) calculatedSizeAlongAxis = child.MinimumSize.Height;
                }
                else
                {
                    if (child.MaximumSize.Width > 0 && calculatedSizeAlongAxis > child.MaximumSize.Width) calculatedSizeAlongAxis = child.MaximumSize.Width;
                    if (calculatedSizeAlongAxis < child.MinimumSize.Width) calculatedSizeAlongAxis = child.MinimumSize.Width;
                }

                int crossAxisPos, crossAxisSize;
                BoundsSpecified boundsSpec = BoundsSpecified.None;

                // Determine cross-axis position and size
                if (lay_Orientation == StackOrientation.Vertical)
                { // Vertical Stack -> Align Horizontally
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
                    // Set bounds for flow control
                    child.SetBounds(crossAxisPos, currentPos, crossAxisSize, calculatedSizeAlongAxis, BoundsSpecified.Location | BoundsSpecified.Height | boundsSpec);
                    // Store final location (Top-Left corner)
                    if (!string.IsNullOrEmpty(child.Name)) flowControlLocations[child.Name] = new Point(crossAxisPos, currentPos);
                    maxCrossAxisSize = Math.Max(maxCrossAxisSize, crossAxisSize); // Update max cross size
                    currentPos += calculatedSizeAlongAxis; // Advance main axis position
                }
                else
                { // Horizontal Stack -> Align Vertically
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
                    // Set bounds for flow control
                    child.SetBounds(currentPos, crossAxisPos, calculatedSizeAlongAxis, crossAxisSize, BoundsSpecified.Location | BoundsSpecified.Width | boundsSpec);
                    // Store final location (Top-Left corner)
                    if (!string.IsNullOrEmpty(child.Name)) flowControlLocations[child.Name] = new Point(currentPos, crossAxisPos);
                    maxCrossAxisSize = Math.Max(maxCrossAxisSize, crossAxisSize); // Update max cross size
                    currentPos += calculatedSizeAlongAxis; // Advance main axis position
                }
                // Add spacing between flow controls
                if (i < flowControls.Count - 1) currentPos += lay_Spacing;
            }
            int contentEndPos = currentPos; // Store final position after last flow control
            Debug.WriteLine($"Finished positioning Flow Controls. Content End: {contentEndPos}");
            return contentEndPos;
        }
        private void PL_p4__Position_Floating_Controls__backup(List<Control> flowControls, List<Control> floatingControls,
            Rectangle displayRect, Dictionary<string, Point> flowControlLocations)
        {
            if (floatingControls.Count > 0 && extender != null)
            {
                Debug.WriteLine("Positioning Floating Controls...");
                foreach (Control floater in floatingControls)
                {
                    // Get all relevant properties using prefixed names
                    string targetName = extender.Getlay_FloatTargetName(floater);
                    int offsetX = extender.Getlay_FloatOffsetX(floater);
                    int offsetY = extender.Getlay_FloatOffsetY(floater);
                    FloatAlignment alignment = extender.Getlay_FloatAlignment(floater); // Get alignment

                    int baseX = 0, baseY = 0;
                    int finalX, finalY; // Declare here
                    Control targetControl = null;

                    // Try find target control instance directly from flowControls list
                    if (!string.IsNullOrEmpty(targetName))
                    {
                        targetControl = flowControls.FirstOrDefault(fc => fc.Name == targetName);
                    }

                    // Calculate position based on whether target was found
                    if (targetControl != null) // Target FOUND and was part of the flow layout
                    {
                        Debug.WriteLine($"  Floater '{floater.Name}' -> Target '{targetName}' FOUND.");
                        // Retrieve the final location we stored earlier
                        Point targetPos = flowControlLocations[targetName]; // Use stored location

                        // Calculate BASE position based on alignment mode and stored location
                        switch (alignment)
                        {
                            case FloatAlignment.ToLeftOf:
                                baseX = targetPos.X - floater.Width; // Use stored X
                                baseY = targetPos.Y; // Use stored Y
                                Debug.WriteLine($"    Mode: ToLeftOf, Base=({baseX},{baseY})");
                                break;
                            case FloatAlignment.ToRightOf:
                                // Need target's Width for Right edge. Get from targetControl instance.
                                baseX = targetPos.X + targetControl.Width;
                                baseY = targetPos.Y; // Use stored Y
                                Debug.WriteLine($"    Mode: ToRightOf, Base=({baseX},{baseY})");
                                break;
                            case FloatAlignment.TopLeft:
                            default:
                                baseX = targetPos.X; // Use stored X
                                baseY = targetPos.Y; // Use stored Y
                                Debug.WriteLine($"    Mode: TopLeft, Base=({baseX},{baseY})");
                                break;
                        }
                        // Apply offsets
                        finalX = baseX + offsetX;
                        finalY = baseY + offsetY;
                        Debug.WriteLine($"    Offsets=({offsetX},{offsetY}), Final=({finalX},{finalY})");
                    }
                    else // Target NOT found (or name was empty) - Use Fallback
                    {
                        baseX = displayRect.Left;
                        baseY = displayRect.Top;
                        finalX = baseX + offsetX; // Assign in fallback path
                        finalY = baseY + offsetY; // Assign in fallback path
                        Debug.WriteLine($"  Floater '{floater.Name}' -> Target '{targetName}' NOT FOUND/Invalid. Fallback Pos: ({finalX},{finalY})");
                    }

                    // --- Set Bounds (finalX/Y are now guaranteed to be assigned) ---
                    floater.SetBounds(finalX, finalY, floater.Width, floater.Height, BoundsSpecified.Location);

                    // --- Apply Z-Order: Place Floater BEHIND Target (if target exists) ---
                    if (targetControl != null)
                    {
                        try // Add try-catch for safety when manipulating Controls collection
                        {
                            int floaterIndex = this.Controls.GetChildIndex(floater);
                            int targetIndex = this.Controls.GetChildIndex(targetControl);
                            // Only move if floater is currently in front of target
                            if (floaterIndex > targetIndex)
                            {
                                //this.Controls.SetChildIndex(floater, targetIndex); // Move just behind target
                                Debug.WriteLine($"    Z-Index set behind Target '{targetControl.Name}'");
                            }
                        }
                        catch (Exception zEx)
                        {
                            Debug.WriteLine($"    ERROR setting Z-Index for {floater.Name}: {zEx.Message}");
                        }
                    }
                    else
                    {
                        // Fallback Z-order for untargeted floaters (e.g., send to back)
                        //floater.SendToBack();
                        Debug.WriteLine($"    Z-Index set to Back (untargeted)");
                    }

                } // End foreach floater
            } // End if floatingControls
        }
        private void PL_p4__Position_Floating_Controls(List<Control> flowControls, List<Control> floatingControls,
                    Rectangle displayRect, Dictionary<string, Point> flowControlLocations)
        {
            // Ensure we have an extender instance to get floating properties
            if (floatingControls.Count > 0 && extender != null)
            {
                Debug.WriteLine("Positioning Floating Controls (v0)...");
                foreach (Control floater in floatingControls)
                {
                    // --- Get Extender Properties ---
                    string targetName = extender.Getlay_FloatTargetName(floater);
                    int offsetX = extender.Getlay_FloatOffsetX(floater);
                    int offsetY = extender.Getlay_FloatOffsetY(floater);
                    FloatAlignment alignment = extender.Getlay_FloatAlignment(floater);
                    StackFloatZOrder zOrderMode = extender.Getlay_FloatZOrder(floater); // Get the Z-Order mode

                    int baseX = 0, baseY = 0;
                    int finalX, finalY;
                    Control targetControl = null;

                    // --- Find Target Control (only among previously laid out flow controls) ---
                    if (!string.IsNullOrEmpty(targetName))
                    {
                        targetControl = flowControls.FirstOrDefault(fc => fc.Name == targetName);
                        if (targetControl == null)
                        {
                            Debug.WriteLine($"  Floater '{floater.Name}' -> Target '{targetName}' NOT FOUND among flow controls.");
                            // Note: Does not currently support targeting other floaters easily.
                        }
                    }

                    // --- Calculate Position ---
                    if (targetControl != null && flowControlLocations.ContainsKey(targetName)) // Target FOUND
                    {
                        Debug.WriteLine($"  Floater '{floater.Name}' -> Target '{targetName}' FOUND.");
                        Point targetPos = flowControlLocations[targetName]; // Use the calculated location from flow layout pass

                        // Calculate base position based on alignment
                        switch (alignment)
                        {
                            case FloatAlignment.ToLeftOf:
                                baseX = targetPos.X - floater.Width;
                                baseY = targetPos.Y;
                                break;
                            case FloatAlignment.ToRightOf:
                                baseX = targetPos.X + targetControl.Width; // Need target's actual width
                                baseY = targetPos.Y;
                                break;
                            case FloatAlignment.TopLeft:
                            default:
                                baseX = targetPos.X;
                                baseY = targetPos.Y;
                                break;
                        }
                        // Apply offsets
                        finalX = baseX + offsetX;
                        finalY = baseY + offsetY;
                        Debug.WriteLine($"    Mode: {alignment}, Base=({baseX},{baseY}), Offsets=({offsetX},{offsetY}), Final=({finalX},{finalY})");
                    }
                    else // Target NOT found or not specified - Use Fallback (relative to displayRect)
                    {
                        baseX = displayRect.Left;
                        baseY = displayRect.Top;
                        finalX = baseX + offsetX;
                        finalY = baseY + offsetY;
                        if (!string.IsNullOrEmpty(targetName))
                            Debug.WriteLine($"  Floater '{floater.Name}' -> Target '{targetName}' NOT FOUND/Invalid. Fallback Pos: ({finalX},{finalY})");
                        else
                            Debug.WriteLine($"  Floater '{floater.Name}' -> No Target Name. Fallback Pos: ({finalX},{finalY})");
                        targetControl = null; // Explicitly nullify if not found
                    }

                    // --- Set Bounds ---
                    // Apply the calculated position. Size is usually determined by the control itself unless changed elsewhere.
                    floater.SetBounds(finalX, finalY, floater.Width, floater.Height, BoundsSpecified.Location);

                    // --- Apply Z-Order based on Mode ---
                    try // Wrap Z-order manipulation in a try-catch for safety
                    {
                        switch (zOrderMode)
                        {
                            case StackFloatZOrder.InFrontOfTarget: // Place floater visually ON TOP of target
                                if (targetControl != null)
                                {
                                    int targetIndex = this.Controls.GetChildIndex(targetControl);
                                    // Calculate desired index (one position after target)
                                    int desiredIndex = targetIndex + 1;

                                    // If target is the last control, bringing to front is equivalent
                                    if (desiredIndex >= this.Controls.Count)
                                    {
                                        // Check if already at front before calling (minor optimization)
                                        if (this.Controls.GetChildIndex(floater) != this.Controls.Count - 1)
                                        {
                                            floater.BringToFront();
                                            Debug.WriteLine($"    Z-Index (AboveTarget): '{floater.Name}' brought to front (target '{targetControl.Name}' was last/near last).");
                                        }
                                        else
                                        {
                                            Debug.WriteLine($"    Z-Index (AboveTarget): '{floater.Name}' already at front.");
                                        }
                                    }
                                    // Otherwise, set the specific index if not already there
                                    else if (this.Controls.GetChildIndex(floater) != desiredIndex)
                                    {
                                        this.Controls.SetChildIndex(floater, desiredIndex);
                                        Debug.WriteLine($"    Z-Index (AboveTarget): Moved '{floater.Name}' to index {desiredIndex} (after target '{targetControl.Name}').");
                                    }
                                    else
                                    {
                                        Debug.WriteLine($"    Z-Index (AboveTarget): '{floater.Name}' already at index {desiredIndex}.");
                                    }
                                }
                                else // No target specified, bring the floater to the front of all controls
                                {
                                    if (this.Controls.GetChildIndex(floater) != this.Controls.Count - 1)
                                    {
                                        floater.BringToFront();
                                        Debug.WriteLine($"    Z-Index (AboveTarget): '{floater.Name}' brought to front (no target).");
                                    }
                                    else
                                    {
                                        Debug.WriteLine($"    Z-Index (AboveTarget): '{floater.Name}' already at front (no target).");
                                    }
                                }
                                break;

                            case StackFloatZOrder.BehindTarget: // Place floater visually BEHIND target
                                if (targetControl != null)
                                {
                                    int targetIndex = this.Controls.GetChildIndex(targetControl);
                                    // Setting the floater's index to the target's index pushes the target forward
                                    if (this.Controls.GetChildIndex(floater) != targetIndex)
                                    {
                                        this.Controls.SetChildIndex(floater, targetIndex);
                                        Debug.WriteLine($"    Z-Index (BelowTarget): Moved '{floater.Name}' to index {targetIndex} (behind target '{targetControl.Name}').");
                                    }
                                    else
                                    {
                                        Debug.WriteLine($"    Z-Index (BelowTarget): '{floater.Name}' already at index {targetIndex}.");
                                    }
                                }
                                else // No target specified, send the floater to the back of all controls
                                {
                                    if (this.Controls.GetChildIndex(floater) != 0)
                                    {
                                        floater.SendToBack();
                                        Debug.WriteLine($"    Z-Index (BelowTarget): '{floater.Name}' sent to back (no target).");
                                    }
                                    else
                                    {
                                        Debug.WriteLine($"    Z-Index (BelowTarget): '{floater.Name}' already at back (no target).");
                                    }
                                }
                                break;

                            case StackFloatZOrder.Manual: // Do not manage Z-order
                            default:
                                // Layout process leaves the Z-order untouched.
                                Debug.WriteLine($"    Z-Index (Manual): Layout pass skipped Z-order management for '{floater.Name}'.");
                                break;
                        }
                    }
                    catch (Exception zEx)
                    {
                        // Log error if manipulating child indices fails
                        Debug.WriteLine($"    ERROR setting Z-Index for {floater.Name} (Mode: {zOrderMode}): {zEx.Message}. Z-order might be incorrect.");
                        // Avoid further Z-order changes on error to prevent potential loops
                    }
                    // --- END Z-Order Section ---

                } // End foreach floater
                Debug.WriteLine("...Finished Positioning Floating Controls (v0)");
            }
            else if (floatingControls.Count > 0 && extender == null)
            {
                Debug.WriteLine("WARNING: Floating controls exist but LayoutExtenderProvider is not assigned. Cannot position floaters or manage Z-order.");
            }
        }
        private void PL_p5__Calculate_AutoScrollMinSize_based_on_FLOW_controls(Rectangle displayRect, int maxCrossAxisSize, int contentEndPos)
        {
            if (AutoScroll)
            {
                if (lay_Orientation == StackOrientation.Vertical)
                {
                    int requiredHeight = contentEndPos - displayRect.Top + Padding.Bottom;
                    int requiredWidth = displayRect.Width;
                    if (lay_ChildAxisAlignment != StackChildAxisAlignment.Stretch)
                    {
                        requiredWidth = maxCrossAxisSize + Padding.Left + Padding.Right;
                    }
                    requiredWidth = Math.Max(displayRect.Width, requiredWidth);
                    this.AutoScrollMinSize = new Size(requiredWidth, requiredHeight);
                }
                else
                {
                    int requiredWidth = contentEndPos - displayRect.Left + Padding.Right;
                    int requiredHeight = displayRect.Height;
                    if (lay_ChildAxisAlignment != StackChildAxisAlignment.Stretch)
                    {
                        requiredHeight = maxCrossAxisSize + Padding.Top + Padding.Bottom;
                    }
                    requiredHeight = Math.Max(displayRect.Height, requiredHeight);
                    this.AutoScrollMinSize = new Size(requiredWidth, requiredHeight);
                }
                Debug.WriteLine($"Setting AutoScrollMinSize based on Flow Controls to: {this.AutoScrollMinSize}");

            }
            else
            {
                this.AutoScrollMinSize = Size.Empty;
            }
        }


        // --- Other Layout Methods ( v4) ---
        // IMPORTANT: You would need to apply the same flow/floating separation logic
        // to   PerformStackLayout_v4 if you intend to use
        // those methods with floating controls. The logic will be very similar:
        // 1. Separate controls using extender.Getlay_IsFloating()
        // 2. Perform main calculations/loops ONLY on flowControls
        // 3. Add the loop to position floatingControls at the end
        // 4. Adjust AutoScrollMinSize based on flowControls
        private void PerformStackLayout_v4()
        {
            // Prevent re-entrancy
            if (_isPerformingLayout)
            {
                Debug.WriteLine("StackLayout DEBUG: PerformStackLayout_v4 skipped due to re-entrancy flag.");
                return;
            }
            _isPerformingLayout = true;
            Debug.WriteLine($"StackLayout DEBUG: ---> Starting PerformStackLayout_v{lay_PerformLayout_calcMethod_No}");

            //StackLayoutExtender extender = this.LayoutExtenderProvider; // Use manual property
            if (extender == null && this.Controls.OfType<IComponent>().Any())
            {
                Debug.WriteLine($"StackLayout WARNING: LayoutExtenderProvider is NULL in '{this.Name}', floating/expansion properties will not work!");
            }

            try
            {
                // --- 1. Separate Visible Controls into Flow and Floating ---
                List<Control> flowControls, floatingControls;
                PL4_p1_Separate_Visible_Controls_into_Flow_and_Floating(out flowControls, out floatingControls);

                Rectangle displayRect = this.DisplayRectangle;

                // --- 2. Handle Case of No Flow Controls ---
                if (flowControls.Count == 0)
                {
                    PL4_p2_Handle_Case_of_No_Flow_Controls(floatingControls, displayRect);

                    _isPerformingLayout = false; // Reset flag before returning
                    return;
                }

                // --- 3. Layout Flow Controls (Method 4 Logic) ---
                this.SuspendLayout();

                Dictionary<Control, int> weights, calculatedExpanderSizes;
                PL4_p3_Layout_Flow_Controls__Method4_Logic(flowControls, displayRect, out weights, out calculatedExpanderSizes);

                // --- Position FLOW Controls Sequentially ---
                int maxCrossAxisSize, contentEndPos;
                Dictionary<string, Point> flowControlLocations;
                PL4_p32_Position_FLOW_Controls_Sequentially(flowControls, displayRect, weights, calculatedExpanderSizes, out maxCrossAxisSize, out flowControlLocations, out contentEndPos);

                // --- 4. Position Floating Controls (Identical logic to v0) ---
                PL4_p4_Position_Floating_Controls(flowControls, floatingControls, displayRect, flowControlLocations);

                // --- 5. Calculate AutoScrollMinSize based on FLOW controls (Identical logic to v0) ---
                PL4_p5_Calculate_AutoScrollMinSize_based_on_FLOW_controls(displayRect, maxCrossAxisSize, contentEndPos);

                // --- Resume Layout ---
                this.ResumeLayout(true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"StackLayout ERROR during PerformStackLayout_v4: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                _isPerformingLayout = false;
                Debug.WriteLine($"StackLayout DEBUG: <--- Finished PerformStackLayout_v{lay_PerformLayout_calcMethod_No}");
            }
        } // End PerformStackLayout_v4

        private void PL4_p1_Separate_Visible_Controls_into_Flow_and_Floating(out List<Control> flowControls, out List<Control> floatingControls)
        {
            var allVisibleControls = this.Controls.OfType<Control>().Where(c => c.Visible).ToList();
            flowControls = new List<Control>();
            floatingControls = new List<Control>();
            if (extender != null)
            {
                foreach (Control child in allVisibleControls)
                {
                    // Use prefixed method
                    if (extender.Getlay_IsFloating(child))
                    {
                        floatingControls.Add(child);
                    }
                    else
                    {
                        flowControls.Add(child);
                    }
                }
            }
            else
            {
                flowControls.AddRange(allVisibleControls);
            } // No extender, all are flow
            Debug.WriteLine($"Layout Pass v4: Flow Controls ({flowControls.Count}), Floating Controls ({floatingControls.Count})");
        }
        private void PL4_p2_Handle_Case_of_No_Flow_Controls(List<Control> floatingControls, Rectangle displayRect)
        {
            Debug.WriteLine("No flow controls to layout.");
            if (floatingControls.Count > 0 && extender != null)
            {
                Debug.WriteLine("Positioning floaters relative to Padding.");
                foreach (Control floater in floatingControls)
                {
                    int offsetX = extender.Getlay_FloatOffsetX(floater);
                    int offsetY = extender.Getlay_FloatOffsetY(floater);
                    int fallbackX = displayRect.Left + offsetX;
                    int fallbackY = displayRect.Top + offsetY;
                    floater.SetBounds(fallbackX, fallbackY, floater.Width, floater.Height, BoundsSpecified.Location);
                    floater.SendToBack();
                }
            }
            if (AutoScroll)
            {
                AutoScrollMinSize = Size.Empty;
                Debug.WriteLine("Setting AutoScrollMinSize to Empty (No Flow Controls).");
            }
            else
            {
                AutoScrollMinSize = Size.Empty;
            }
            Debug.WriteLine($"StackLayout DEBUG: <--- Finished PerformStackLayout (No Flow Controls)");
        }
        private void PL4_p3_Layout_Flow_Controls__Method4_Logic(List<Control> flowControls, Rectangle displayRect, out Dictionary<Control, int> weights, out Dictionary<Control, int> calculatedExpanderSizes)
        {
            // --- Calculate sizes/weights based ONLY on flowControls ---
            double totalWeight = 0;
            int totalNonExpandingSize = 0;
            int expandingChildCount = 0;
            weights = new Dictionary<Control,
              int>();
            var expandingControls = new List<Control>(); // Track expanding flow controls

            foreach (Control child in flowControls) // Iterate flow controls
            {
                // Use prefixed method (or correct non-prefixed name)
                int weight = extender?.Getlay_ExpandWeight(child) ?? 0;
                weights[child] = weight;
                if (weight == 0) // Non-expanding flow control
                {
                    int sizeAlongAxis = (lay_Orientation == StackOrientation.Vertical) ? child.Height : child.Width;
                    totalNonExpandingSize += sizeAlongAxis;
                }
                else // Expanding flow control
                {
                    totalWeight += weight;
                    expandingChildCount++;
                    expandingControls.Add(child);
                }
            }
            Debug.WriteLine($"Flow Controls Calc v4: TotalWeight={totalWeight}, TotalNonExpandingSize={totalNonExpandingSize}, ExpandingCount={expandingChildCount}");

            // --- Calculate available space based on FLOW controls ---
            int totalSpacing = (flowControls.Count > 1) ? (flowControls.Count - 1) * lay_Spacing : 0;
            int availableSpace = (lay_Orientation == StackOrientation.Vertical) ? displayRect.Height : displayRect.Width;
            // Space purely for the expanding items
            double spaceAvailableForExpanders = Math.Max(0, availableSpace - totalNonExpandingSize - totalSpacing);
            Debug.WriteLine($"Space Calc v4: TotalSpacing={totalSpacing}, Available={availableSpace}, SpaceForExpanders={spaceAvailableForExpanders}");

            // --- Distribute space purely by weight among expanding FLOW controls ---
            double fractionalSpace = 0.0;
            calculatedExpanderSizes = new Dictionary<Control,
              int>();
            if (spaceAvailableForExpanders > 0 && totalWeight > 0)
            {
                // Calculate share for each expanding flow control
                foreach (Control child in expandingControls) // Use list of expanders
                {
                    int weight = weights[child]; // Already have weight
                    double exactShare = spaceAvailableForExpanders * (double)weight / totalWeight;
                    int wholePixels = (int)Math.Floor(exactShare);
                    calculatedExpanderSizes[child] = wholePixels;
                    fractionalSpace += exactShare - wholePixels;
                }
                // Distribute leftover fractional pixels
                int leftoverPixels = (int)Math.Round(fractionalSpace);
                int distributedLeftovers = 0;
                if (leftoverPixels > 0 && expandingChildCount > 0)
                {
                    foreach (Control child in expandingControls) // Use list of expanders
                    {
                        // Dictionary entry should already exist, but safe check
                        if (!calculatedExpanderSizes.ContainsKey(child)) calculatedExpanderSizes[child] = 0;
                        calculatedExpanderSizes[child]++;
                        distributedLeftovers++;
                        if (distributedLeftovers >= leftoverPixels) break;
                    }
                }
                Debug.WriteLine($"Distributed Expander space. Leftover pixels added: {distributedLeftovers}");
            }
            else // No space for expanders or no expanding controls
            {
                // Ensure dictionary entries exist even if size is 0 for expanding controls
                foreach (Control child in expandingControls) calculatedExpanderSizes[child] = 0;
            }
        }
        private void PL4_p32_Position_FLOW_Controls_Sequentially(List<Control> flowControls, Rectangle displayRect,
            Dictionary<Control, int> weights, Dictionary<Control, int> calculatedExpanderSizes, out int maxCrossAxisSize, out Dictionary<string, Point> flowControlLocations, out int contentEndPos)
        {
            int currentPos = (lay_Orientation == StackOrientation.Vertical) ? displayRect.Top : displayRect.Left;
            maxCrossAxisSize = 0;
            flowControlLocations = new Dictionary<string,
              Point>();
            for (int i = 0; i < flowControls.Count; i++) // Loop through ALL flow controls
            {
                Control child = flowControls[i];
                int weight = weights[child]; // Already have weight
                int sizeAlongAxis; // Final size for this control

                if (weight == 0) // Non-expanding control size
                {
                    sizeAlongAxis = (lay_Orientation == StackOrientation.Vertical) ? child.Height : child.Width;
                }
                else // Expanding control size (use calculated value)
                {
                    sizeAlongAxis = calculatedExpanderSizes.ContainsKey(child) ? calculatedExpanderSizes[child] : 0;
                    // Apply Min/Max constraints along the orientation axis for expanders
                    if (lay_Orientation == StackOrientation.Vertical)
                    {
                        if (child.MaximumSize.Height > 0 && sizeAlongAxis > child.MaximumSize.Height) sizeAlongAxis = child.MaximumSize.Height;
                        if (sizeAlongAxis < child.MinimumSize.Height) sizeAlongAxis = child.MinimumSize.Height;
                    }
                    else
                    {
                        if (child.MaximumSize.Width > 0 && sizeAlongAxis > child.MaximumSize.Width) sizeAlongAxis = child.MaximumSize.Width;
                        if (sizeAlongAxis < child.MinimumSize.Width) sizeAlongAxis = child.MinimumSize.Width;
                    }
                }

                int crossAxisPos, crossAxisSize;
                BoundsSpecified boundsSpec = BoundsSpecified.None;

                // Determine cross-axis position and size (same logic as v0)
                if (lay_Orientation == StackOrientation.Vertical)
                { // Vertical Stack -> Align Horizontally
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
                    if (!string.IsNullOrEmpty(child.Name)) flowControlLocations[child.Name] = new Point(crossAxisPos, currentPos);
                    maxCrossAxisSize = Math.Max(maxCrossAxisSize, crossAxisSize);
                    currentPos += sizeAlongAxis;
                }
                else
                { // Horizontal Stack -> Align Vertically
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
                    if (!string.IsNullOrEmpty(child.Name)) flowControlLocations[child.Name] = new Point(currentPos, crossAxisPos);
                    maxCrossAxisSize = Math.Max(maxCrossAxisSize, crossAxisSize);
                    currentPos += sizeAlongAxis;
                }
                // Add spacing between flow controls
                if (i < flowControls.Count - 1) currentPos += lay_Spacing;
            }
            contentEndPos = currentPos;
            Debug.WriteLine($"Finished positioning Flow Controls v4. Content End: {contentEndPos}");
        }

        /// <summary>
        /// identical_ to v0
        /// </summary>
        private void PL4_p4_Position_Floating_Controls__backup(List<Control> flowControls, List<Control> floatingControls,
            Rectangle displayRect, Dictionary<string, Point> flowControlLocations)
        {
            if (floatingControls.Count > 0 && extender != null)
            {
                Debug.WriteLine("Positioning Floating Controls...");
                foreach (Control floater in floatingControls)
                {
                    string targetName = extender.Getlay_FloatTargetName(floater);
                    int offsetX = extender.Getlay_FloatOffsetX(floater);
                    int offsetY = extender.Getlay_FloatOffsetY(floater);
                    FloatAlignment alignment = extender.Getlay_FloatAlignment(floater);

                    int baseX = 0, baseY = 0;
                    int finalX, finalY;
                    Control targetControl = null;

                    if (!string.IsNullOrEmpty(targetName))
                    {
                        targetControl = flowControls.FirstOrDefault(fc => fc.Name == targetName);
                    }

                    if (targetControl != null)
                    {
                        Debug.WriteLine($"  Floater '{floater.Name}' -> Target '{targetName}' FOUND.");
                        Point targetPos = flowControlLocations[targetName]; // Use stored location
                        switch (alignment)
                        {
                            /* ... Alignment cases identical to v0 ... */
                            case FloatAlignment.ToLeftOf:
                                baseX = targetPos.X - floater.Width;
                                baseY = targetPos.Y;
                                Debug.WriteLine($"    Mode: ToLeftOf, Base=({baseX},{baseY})");
                                break;
                            case FloatAlignment.ToRightOf:
                                baseX = targetPos.X + targetControl.Width;
                                baseY = targetPos.Y;
                                Debug.WriteLine($"    Mode: ToRightOf, Base=({baseX},{baseY})");
                                break;
                            case FloatAlignment.TopLeft:
                            default:
                                baseX = targetPos.X;
                                baseY = targetPos.Y;
                                Debug.WriteLine($"    Mode: TopLeft, Base=({baseX},{baseY})");
                                break;
                        }
                        finalX = baseX + offsetX;
                        finalY = baseY + offsetY;
                        Debug.WriteLine($"    Offsets=({offsetX},{offsetY}), Final=({finalX},{finalY})");
                    }
                    else
                    {
                        baseX = displayRect.Left;
                        baseY = displayRect.Top;
                        finalX = baseX + offsetX;
                        finalY = baseY + offsetY;
                        Debug.WriteLine($"  Floater '{floater.Name}' -> Target '{targetName}' NOT FOUND/Invalid. Fallback Pos: ({finalX},{finalY})");
                    }

                    floater.SetBounds(finalX, finalY, floater.Width, floater.Height, BoundsSpecified.Location);

                    if (targetControl != null)
                    {
                        try
                        {
                            int fIdx = this.Controls.GetChildIndex(floater);
                            int tIdx = this.Controls.GetChildIndex(targetControl);
                            if (fIdx > tIdx)
                            {
                                this.Controls.SetChildIndex(floater, tIdx);
                                Debug.WriteLine($"    Z-Index set behind Target '{targetControl.Name}'");
                            }
                        }
                        catch (Exception zEx)
                        {
                            Debug.WriteLine($"    ERROR setting Z-Index for {floater.Name}: {zEx.Message}");
                        }
                    }
                    else
                    {
                        floater.SendToBack();
                        Debug.WriteLine($"    Z-Index set to Back (untargeted)");
                    }
                }
            }
        }

        /// <summary>
        /// identical to 0 --  added Zorder  
        /// </summary>
        private void PL4_p4_Position_Floating_Controls(List<Control> flowControls, List<Control> floatingControls,
            Rectangle displayRect, Dictionary<string, Point> flowControlLocations)
        {
            // Ensure we have an extender instance to get floating properties
            if (floatingControls.Count > 0 && extender != null)
            {
                Debug.WriteLine("Positioning Floating Controls (v4)...");
                foreach (Control floater in floatingControls)
                {
                    // --- Get Extender Properties ---
                    string targetName = extender.Getlay_FloatTargetName(floater);
                    int offsetX = extender.Getlay_FloatOffsetX(floater);
                    int offsetY = extender.Getlay_FloatOffsetY(floater);
                    FloatAlignment alignment = extender.Getlay_FloatAlignment(floater);
                    StackFloatZOrder zOrderMode = extender.Getlay_FloatZOrder(floater); // Get the Z-Order mode

                    int baseX = 0, baseY = 0;
                    int finalX, finalY;
                    Control targetControl = null;

                    // --- Find Target Control (only among previously laid out flow controls) ---
                    if (!string.IsNullOrEmpty(targetName))
                    {
                        targetControl = flowControls.FirstOrDefault(fc => fc.Name == targetName);
                        if (targetControl == null)
                        {
                            Debug.WriteLine($"  Floater '{floater.Name}' -> Target '{targetName}' NOT FOUND among flow controls.");
                            // Note: Does not currently support targeting other floaters easily.
                        }
                    }

                    // --- Calculate Position ---
                    if (targetControl != null && flowControlLocations.ContainsKey(targetName)) // Target FOUND
                    {
                        Debug.WriteLine($"  Floater '{floater.Name}' -> Target '{targetName}' FOUND.");
                        Point targetPos = flowControlLocations[targetName]; // Use the calculated location from flow layout pass

                        // Calculate base position based on alignment
                        switch (alignment)
                        {
                            case FloatAlignment.ToLeftOf:
                                baseX = targetPos.X - floater.Width;
                                baseY = targetPos.Y;
                                break;
                            case FloatAlignment.ToRightOf:
                                baseX = targetPos.X + targetControl.Width; // Need target's actual width
                                baseY = targetPos.Y;
                                break;
                            case FloatAlignment.TopLeft:
                            default:
                                baseX = targetPos.X;
                                baseY = targetPos.Y;
                                break;
                        }
                        // Apply offsets
                        finalX = baseX + offsetX;
                        finalY = baseY + offsetY;
                        Debug.WriteLine($"    Mode: {alignment}, Base=({baseX},{baseY}), Offsets=({offsetX},{offsetY}), Final=({finalX},{finalY})");
                    }
                    else // Target NOT found or not specified - Use Fallback (relative to displayRect)
                    {
                        baseX = displayRect.Left;
                        baseY = displayRect.Top;
                        finalX = baseX + offsetX;
                        finalY = baseY + offsetY;
                        if (!string.IsNullOrEmpty(targetName))
                            Debug.WriteLine($"  Floater '{floater.Name}' -> Target '{targetName}' NOT FOUND/Invalid. Fallback Pos: ({finalX},{finalY})");
                        else
                            Debug.WriteLine($"  Floater '{floater.Name}' -> No Target Name. Fallback Pos: ({finalX},{finalY})");
                        targetControl = null; // Explicitly nullify if not found
                    }

                    // --- Set Bounds ---
                    // Apply the calculated position. Size is usually determined by the control itself unless changed elsewhere.
                    floater.SetBounds(finalX, finalY, floater.Width, floater.Height, BoundsSpecified.Location);

                    // --- Apply Z-Order based on Mode ---
                    try // Wrap Z-order manipulation in a try-catch for safety
                    {
                        switch (zOrderMode)
                        {
                            case StackFloatZOrder.InFrontOfTarget: // Place floater visually ON TOP of target
                                if (targetControl != null)
                                {
                                    int targetIndex = this.Controls.GetChildIndex(targetControl);
                                    // Calculate desired index (one position after target)
                                    int desiredIndex = targetIndex + 1;

                                    // If target is the last control, bringing to front is equivalent
                                    if (desiredIndex >= this.Controls.Count)
                                    {
                                        // Check if already at front before calling (minor optimization)
                                        if (this.Controls.GetChildIndex(floater) != this.Controls.Count - 1)
                                        {
                                            floater.BringToFront();
                                            Debug.WriteLine($"    Z-Index (AboveTarget): '{floater.Name}' brought to front (target '{targetControl.Name}' was last/near last).");
                                        }
                                        else
                                        {
                                            Debug.WriteLine($"    Z-Index (AboveTarget): '{floater.Name}' already at front.");
                                        }
                                    }
                                    // Otherwise, set the specific index if not already there
                                    else if (this.Controls.GetChildIndex(floater) != desiredIndex)
                                    {
                                        this.Controls.SetChildIndex(floater, desiredIndex);
                                        Debug.WriteLine($"    Z-Index (AboveTarget): Moved '{floater.Name}' to index {desiredIndex} (after target '{targetControl.Name}').");
                                    }
                                    else
                                    {
                                        Debug.WriteLine($"    Z-Index (AboveTarget): '{floater.Name}' already at index {desiredIndex}.");
                                    }
                                }
                                else // No target specified, bring the floater to the front of all controls
                                {
                                    if (this.Controls.GetChildIndex(floater) != this.Controls.Count - 1)
                                    {
                                        floater.BringToFront();
                                        Debug.WriteLine($"    Z-Index (AboveTarget): '{floater.Name}' brought to front (no target).");
                                    }
                                    else
                                    {
                                        Debug.WriteLine($"    Z-Index (AboveTarget): '{floater.Name}' already at front (no target).");
                                    }
                                }
                                break;

                            case StackFloatZOrder.BehindTarget: // Place floater visually BEHIND target
                                if (targetControl != null)
                                {
                                    int targetIndex = this.Controls.GetChildIndex(targetControl);
                                    // Setting the floater's index to the target's index pushes the target forward
                                    if (this.Controls.GetChildIndex(floater) != targetIndex)
                                    {
                                        this.Controls.SetChildIndex(floater, targetIndex);
                                        Debug.WriteLine($"    Z-Index (BelowTarget): Moved '{floater.Name}' to index {targetIndex} (behind target '{targetControl.Name}').");
                                    }
                                    else
                                    {
                                        Debug.WriteLine($"    Z-Index (BelowTarget): '{floater.Name}' already at index {targetIndex}.");
                                    }
                                }
                                else // No target specified, send the floater to the back of all controls
                                {
                                    if (this.Controls.GetChildIndex(floater) != 0)
                                    {
                                        floater.SendToBack();
                                        Debug.WriteLine($"    Z-Index (BelowTarget): '{floater.Name}' sent to back (no target).");
                                    }
                                    else
                                    {
                                        Debug.WriteLine($"    Z-Index (BelowTarget): '{floater.Name}' already at back (no target).");
                                    }
                                }
                                break;

                            case StackFloatZOrder.Manual: // Do not manage Z-order
                            default:
                                // Layout process leaves the Z-order untouched.
                                Debug.WriteLine($"    Z-Index (Manual): Layout pass skipped Z-order management for '{floater.Name}'.");
                                break;
                        }
                    }
                    catch (Exception zEx)
                    {
                        // Log error if manipulating child indices fails
                        Debug.WriteLine($"    ERROR setting Z-Index for {floater.Name} (Mode: {zOrderMode}): {zEx.Message}. Z-order might be incorrect.");
                        // Avoid further Z-order changes on error to prevent potential loops
                    }
                    // --- END Z-Order Section ---

                } // End foreach floater
                Debug.WriteLine("...Finished Positioning Floating Controls (v4)");
            }
            else if (floatingControls.Count > 0 && extender == null)
            {
                Debug.WriteLine("WARNING: Floating controls exist but LayoutExtenderProvider is not assigned. Cannot position floaters or manage Z-order.");
            }
        }

        /// <summary>
        /// identical to v0
        /// </summary>
        private void PL4_p5_Calculate_AutoScrollMinSize_based_on_FLOW_controls(Rectangle displayRect, int maxCrossAxisSize,
            int contentEndPos)
        {
            if (AutoScroll)
            {
                if (lay_Orientation == StackOrientation.Vertical)
                {
                    int requiredHeight = contentEndPos - displayRect.Top + Padding.Bottom;
                    int requiredWidth = displayRect.Width;
                    if (lay_ChildAxisAlignment != StackChildAxisAlignment.Stretch)
                    {
                        requiredWidth = maxCrossAxisSize + Padding.Left + Padding.Right;
                    }
                    requiredWidth = Math.Max(displayRect.Width, requiredWidth);
                    this.AutoScrollMinSize = new Size(requiredWidth, requiredHeight);
                }
                else
                {
                    int requiredWidth = contentEndPos - displayRect.Left + Padding.Right;
                    int requiredHeight = displayRect.Height;
                    if (lay_ChildAxisAlignment != StackChildAxisAlignment.Stretch)
                    {
                        requiredHeight = maxCrossAxisSize + Padding.Top + Padding.Bottom;
                    }
                    requiredHeight = Math.Max(displayRect.Height, requiredHeight);
                    this.AutoScrollMinSize = new Size(requiredWidth, requiredHeight);
                }
                Debug.WriteLine($"Setting AutoScrollMinSize based on Flow Controls to: {this.AutoScrollMinSize}");
            }
            else
            {
                this.AutoScrollMinSize = Size.Empty;
            }
        }






        // --- Overrides for Layout Triggers and Designer Integration ---
        // Hook into VisibleChanged at runtime
        protected override void OnControlAdded(ControlEventArgs e)
        {
            LayoutLogger.Log($"StackLayout [{this.Name}]: OnControlAdded FIRED for '{e.Control?.Name ?? "null"}'. Current _isPerformingLayout = {_isPerformingLayout}");
            base.OnControlAdded(e);
            if (e.Control != null) e.Control.VisibleChanged += ChildControl_VisibleChanged; // Assuming you have this handler

            // --- BEFORE TRIGGERING LAYOUT ---
            LayoutLogger.Log($"StackLayout [{this.Name}]: OnControlAdded TRIGGERING PerformLayout for '{e.Control?.Name ?? "null"}'.");
            PerformLayout();
            LayoutLogger.Log($"StackLayout [{this.Name}]: OnControlAdded - PerformLayout call COMPLETED.");
        }

        protected override void OnControlRemoved(ControlEventArgs e)
        {
            LayoutLogger.Log($"StackLayout [{this.Name}]: OnControlRemoved FIRED for '{e.Control?.Name ?? "null"}'. Current _isPerformingLayout = {_isPerformingLayout}");
            base.OnControlRemoved(e);
            if (e.Control != null) e.Control.VisibleChanged -= ChildControl_VisibleChanged; // Assuming you have this handler

            // --- BEFORE TRIGGERING LAYOUT ---
            LayoutLogger.Log($"StackLayout [{this.Name}]: OnControlRemoved TRIGGERING PerformLayout for '{e.Control?.Name ?? "null"}'.");
            PerformLayout();
            LayoutLogger.Log($"StackLayout [{this.Name}]: OnControlRemoved - PerformLayout call COMPLETED.");
        }
        private void ChildControl_VisibleChanged(object sender, EventArgs e)
        {
            if (this.IsHandleCreated && !this.IsDisposed) this.BeginInvoke((MethodInvoker)delegate {
                this.PerformLayout();
                this.Invalidate(true);
            });
            else if (!this.IsDisposed)
            {
                this.PerformLayout();
                this.Invalidate(true);
            }
        }
        protected override void OnPaddingChanged(EventArgs e)
        {
            base.OnPaddingChanged(e);
            PerformLayout();
        }
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            PerformLayout();
        }

        // Site property remains the same (handling ComponentChangeService)
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override ISite Site
        {
            get => base.Site;
            set
            {
                ISite oldSite = base.Site;
                IComponentChangeService oldService = _componentChangeService;
                if (oldService != null && value?.GetService(typeof(IComponentChangeService)) != oldService)
                {
                    Debug.WriteLine("Site Setter: Unsubscribing from old ComponentChangeService.");
                    oldService.ComponentChanged -= OnComponentChanged;
                    _componentChangeService = null;
                }
                base.Site = value;
                // No need to reset extender cache here as we use LayoutExtenderProvider now
                if (value != null && (_componentChangeService == null || oldSite != value))
                {
                    _componentChangeService = (IComponentChangeService)value.GetService(typeof(IComponentChangeService));
                    if (_componentChangeService != null)
                    {
                        Debug.WriteLine("Site Setter: Subscribing to new ComponentChangeService.");
                        _componentChangeService.ComponentChanged += OnComponentChanged;
                    }
                    else
                    {
                        Debug.WriteLine("Site Setter: Could not get ComponentChangeService from new site.");
                    }
                }
                else if (value == null && oldService != null)
                {
                    Debug.WriteLine("Site Setter: Site set to null, ComponentChangeService reference cleared.");
                    _componentChangeService = null;
                }
            }
        }


        // Corrected ComponentChanged handler
        private void OnComponentChanged(object sender, ComponentChangedEventArgs e)
        {

            //return;
            LayoutLogger.Log($"OnComponentChanged  -  after return -- ");



            string componentTypeName = e.Component?.GetType().Name ?? "null";
            string memberName = e.Member?.Name ?? "null";
            // --- AT THE VERY BEGINNING ---
            LayoutLogger.Log($"StackLayout [{this.Name}]: OnComponentChanged FIRED. Component: {componentTypeName}, Member: {memberName}, Current _isPerformingLayout = {_isPerformingLayout}");

            // --- THE RE-ENTRANCY GUARD CHECK ---
            if (_isPerformingLayout)
            {
                LayoutLogger.Log($"StackLayout [{this.Name}]: OnComponentChanged SKIPPING: _isPerformingLayout is TRUE.");
                return;
            }

            // --- Check for DIRECT CHILD changes ---
            if (e.Component is Control changedControl && changedControl.Parent == this)
            {
                if (memberName == "Visible" || /* ... other relevant members ... */ memberName == "Name")
                {
                    // --- BEFORE TRIGGERING LAYOUT ---
                    LayoutLogger.Log($"StackLayout [{this.Name}]: OnComponentChanged TRIGGERING PerformLayout directly due to DIRECT CHILD '{changedControl.Name}' member '{memberName}'.");

                    if (!this.IsDisposed)
                    {
                        try
                        {
                            this.PerformLayout();
                            this.Invalidate(true);
                            // --- AFTER TRIGGERING LAYOUT --- (Inside the check)
                            LayoutLogger.Log($"StackLayout [{this.Name}]: OnComponentChanged - Direct PerformLayout call COMPLETED.");
                        }
                        catch (Exception ex)
                        {
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
                    // --- WHEN IGNORING CHILD CHANGE ---
                    LayoutLogger.Log($"StackLayout [{this.Name}]: OnComponentChanged - Change on child '{changedControl.Name}' member '{memberName}' IGNORED (not layout-relevant).");
                }
            }
            else
            {
                // --- WHEN IGNORING NON-CHILD CHANGE ---
                LayoutLogger.Log($"StackLayout [{this.Name}]: OnComponentChanged - Change on component '{componentTypeName}' (not a direct child) IGNORED by this handler.");
            }
            // --- AT THE VERY END ---
            LayoutLogger.Log($"StackLayout [{this.Name}]: OnComponentChanged FINISHED processing.");
        }

        // Dispose method remains largely the same
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Unsubscribe from all children
                foreach (Control c in this.Controls)
                {
                    if (c != null) c.VisibleChanged -= ChildControl_VisibleChanged;
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