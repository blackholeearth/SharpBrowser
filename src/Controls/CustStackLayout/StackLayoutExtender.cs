using System;
using System.Collections; // Required for Hashtable
using System.ComponentModel;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq; // Optional: If using LayoutLogger inside Setters

namespace SharpBrowser.Controls // Ensure this namespace matches StackLayout.cs
{

    // --- StackProperties Class ---
    internal class StackProperties
    {
        // Define properties with default values
        public int Weight { get; set; } = 0;
        public bool IsFloating { get; set; } = false;
        public string FloatTargetName { get; set; } = "";
        public int FloatOffsetX { get; set; } = 0;
        public int FloatOffsetY { get; set; } = 0;
        public FloatAlignment FloatAlignment { get; set; } = FloatAlignment.TopLeft;
        public StackFloatZOrder FloatZOrder { get; set; } = StackFloatZOrder.InFrontOfTarget;
        public bool IncludeHiddenInLayout { get; set; } = false;

        // Optional: Define constants for defaults
        public const int DefaultWeight = 0;
        // ... other defaults

        // Static instance holding default values (for GetPropertiesOrDefault)
        public static readonly StackProperties Defaults = new StackProperties();

        // Constructor could also explicitly set defaults if preferred over initializers
        // public StackProperties() { Weight = 0; IsFloating = false; ... }
    }

    /// <summary>
    /// Provides extender properties (like lay_ExpandWeight, lay_IsFloating) for controls
    /// contained within a StackLayout panel. This is a partial class definition,
    /// merged with StackLayout.cs by the compiler.
    /// </summary>
    [ProvideProperty("lay_ExpandWeight", typeof(Control))]
    [ProvideProperty("lay_IsFloating", typeof(Control))]
    [ProvideProperty("lay_IncludeHiddenInLayout", typeof(Control))]
    [ProvideProperty("lay_FloatTargetName", typeof(Control))]
    [ProvideProperty("lay_FloatOffsetX", typeof(Control))]
    [ProvideProperty("lay_FloatOffsetY", typeof(Control))]
    [ProvideProperty("lay_FloatAlignment", typeof(Control))]
    [ProvideProperty("lay_FloatZOrder", typeof(Control))]
    public partial class StackLayout : IExtenderProvider // Inherits Panel from other file, implements IExtenderProvider here
    {

        private new Dictionary<Control, StackProperties> _lay_properties = new Dictionary<Control, StackProperties>();

        //// --- Private Fields (Storage for Provided Properties) ---
        //// These Hashtables store the extender property values associated with each child control.
        //// They are instance members of the StackLayout class.
        //private new Dictionary<Control, int> _lay_expandWeights = new Dictionary<Control, int>();
        //private new Dictionary<Control, bool> _lay_isFloating = new Dictionary<Control, bool>();
        //private new Dictionary<Control, string> _lay_floatTargetNames = new Dictionary<Control, string>();
        //private new Dictionary<Control, int> _lay_floatOffsetsX = new Dictionary<Control, int>();
        //private new Dictionary<Control, int> _lay_floatOffsetsY = new Dictionary<Control, int>();
        //private new Dictionary<Control, FloatAlignment> _lay_floatAlignments = new Dictionary<Control, FloatAlignment>();
        //private new Dictionary<Control, StackFloatZOrder> _lay_floatZOrderModes = new Dictionary<Control, StackFloatZOrder>();
        //private new Dictionary<Control, bool> _lay_includeHiddenInLayout = new Dictionary<Control, bool>();

        #region IExtenderProvider Implementation

        /// <summary>
        /// Determines if this StackLayout panel can provide extender properties to the given object.
        /// It can only extend properties to controls that are its direct children.
        /// </summary>
        /// <param name="extendee">The object to potentially extend.</param>
        /// <returns>True if extendee is a Control and its Parent is this StackLayout, false otherwise.</returns>
        public bool CanExtend(object extendee)
        {
            // 'this' refers to the StackLayout instance itself
            return (extendee is Control control && control.Parent == this);
        }

        #endregion

        // --- Helper Method Implementation ---
        private StackProperties GetPropertiesOrDefault(Control control)
        {
            if (_lay_properties.TryGetValue(control, out StackProperties props))
            {
                return props;
            }
            // Return a shared, immutable instance containing default values
            // Avoids creating new objects constantly for controls with no set properties
            return StackProperties.Defaults;
        }

        //-- used in Get Set
        private StackProperties GetOrCreateProperties(Control control)
        {
            if (!_lay_properties.TryGetValue(control, out StackProperties props))
            {
                props = new StackProperties(); // Assumes StackProperties constructor sets defaults
                _lay_properties[control] = props;
            }
            return props;
        }

        #region Provided Property Getters/Setters
        private StackProperties Common_GetProp(Control control)
        {
            if (_lay_properties.TryGetValue(control, out StackProperties props))
            {
                return props;
            }
            // Return the default value if no props object exists
            return StackProperties.Defaults;
        }

        // --- lay_ExpandWeight ---
        [DefaultValue(0)]
        [Description("The weight used to distribute extra space along the orientation axis. 0 = no expansion. Positive values distribute remaining space proportionally.")]
        [Category(categorySTR)] // categorySTR constant is defined in StackLayout.cs
        public int Getlay_ExpandWeight(Control control) => Common_GetProp(control).Weight;
        public void Setlay_ExpandWeight(Control control, int weight)
        {
            weight = Math.Max(0, weight); // Ensure weight is not negative
            StackProperties props = GetOrCreateProperties(control); // Helper needed

            if (props.Weight != weight)
            {
                props.Weight = weight; // Set value in props object

                // If the control is actually parented by this panel, trigger layout
                if (control?.Parent == this)
                {
                    LayoutLogger.Log($"StackLayout [{this.Name}]: Setlay_ExpandWeight on '{control.Name}' to {weight}. Triggering PerformLayout.");
                    this.PerformLayout(); // 'this' refers to the StackLayout instance
                    this.Invalidate(true); // Ensure repaint reflects potential size changes
                }
            }
        }

        // --- lay_IsFloating ---
        [DefaultValue(false)]
        [Category(categorySTR)]
        [Description("If true, the control is positioned relative to a lay_FloatTargetName control instead of being part of the stack flow.")]
        public bool Getlay_IsFloating(Control control) => Common_GetProp(control).IsFloating;
        public void Setlay_IsFloating(Control control, bool isFloating)
        {
            StackProperties props = GetOrCreateProperties(control);

            if (props.IsFloating != isFloating)
            {
                props.IsFloating = isFloating;

                if (control?.Parent == this)
                {
                    LayoutLogger.Log($"StackLayout [{this.Name}]: Setlay_IsFloating on '{control.Name}' to {isFloating}. Triggering PerformLayout.");
                    PerformLayout();
                    Invalidate(true);
                }
            }
        }

        // --- lay_FloatTargetName ---
        [DefaultValue("")]
        [Category(categorySTR)]
        [Description("The Name of the sibling control this floating control should be positioned relative to.")]
        [TypeConverter(typeof(FloatTargetNameConverter))] // Optional: Add a TypeConverter for dropdown in designer
        public string Getlay_FloatTargetName(Control control) => Common_GetProp(control).FloatTargetName;
        public void Setlay_FloatTargetName(Control control, string targetName)
        {
            targetName = targetName ?? ""; // Normalize null to empty string
            StackProperties props = GetOrCreateProperties(control);

            if (props.FloatTargetName != targetName)
            {
                props.FloatTargetName = targetName;

                if (control?.Parent == this)
                {
                    LayoutLogger.Log($"StackLayout [{this.Name}]: Setlay_FloatTargetName on '{control.Name}' to '{targetName}'. Triggering PerformLayout.");
                    PerformLayout();
                    Invalidate(true);
                }
            }
        }

        // --- lay_FloatOffsetX ---
        [DefaultValue(0)]
        [Category(categorySTR)]
        [Description("The horizontal offset (in pixels) relative to the floating target's alignment point.")]
        public int Getlay_FloatOffsetX(Control control) => Common_GetProp(control).FloatOffsetX;
        public void Setlay_FloatOffsetX(Control control, int offsetX)
        {
            StackProperties props = GetOrCreateProperties(control);

            if (props.FloatOffsetX != offsetX)
            {
                props.FloatOffsetX = offsetX;

                if (control?.Parent == this)
                {
                    LayoutLogger.Log($"StackLayout [{this.Name}]: Setlay_FloatOffsetX on '{control.Name}' to {offsetX}. Triggering PerformLayout.");
                    PerformLayout();
                    Invalidate(true);
                }
            }
        }

        // --- lay_FloatOffsetY ---
        [DefaultValue(0)]
        [Category(categorySTR)]
        [Description("The vertical offset (in pixels) relative to the floating target's alignment point.")]
        public int Getlay_FloatOffsetY(Control control) => Common_GetProp(control).FloatOffsetY;
        public void Setlay_FloatOffsetY(Control control, int offsetY)
        {
            StackProperties props = GetOrCreateProperties(control);

            if (props.FloatOffsetY != offsetY)
            {
                props.FloatOffsetY = offsetY;

                if (control?.Parent == this)
                {
                    LayoutLogger.Log($"StackLayout [{this.Name}]: Setlay_FloatOffsetY on '{control.Name}' to {offsetY}. Triggering PerformLayout.");
                    PerformLayout();
                    Invalidate(true);
                }
            }
        }

        // --- lay_FloatAlignment ---
        [DefaultValue(FloatAlignment.TopLeft)]
        [Category(categorySTR)]
        [Description("Specifies how the floating control is initially positioned relative to its target before offsets are applied.")]
        public FloatAlignment Getlay_FloatAlignment(Control control) => Common_GetProp(control).FloatAlignment;
        public void Setlay_FloatAlignment(Control control, FloatAlignment alignment)
        {
            StackProperties props = GetOrCreateProperties(control);

            if (props.FloatAlignment != alignment)
            {
                props.FloatAlignment = alignment;

                if (control?.Parent == this)
                {
                    LayoutLogger.Log($"StackLayout [{this.Name}]: Setlay_FloatAlignment on '{control.Name}' to {alignment}. Triggering PerformLayout.");
                    PerformLayout();
                    Invalidate(true);
                }
            }
        }

        // --- lay_FloatZOrder ---
        [DefaultValue(StackFloatZOrder.InFrontOfTarget)]
        [Category(categorySTR)]
        [Description("Defines how this floating control's Z-order is managed relative to its target during layout.")]
        public StackFloatZOrder Getlay_FloatZOrder(Control control) => Common_GetProp(control).FloatZOrder;
        public void Setlay_FloatZOrder(Control control, StackFloatZOrder zOrderMode)
        {
            StackProperties props = GetOrCreateProperties(control);

            if (props.FloatZOrder != zOrderMode)
            {
                props.FloatZOrder = zOrderMode;
                // Changing the Z-order *mode* only affects how the *next* layout pass behaves.
                // It does not require an immediate PerformLayout() itself.
                // The Z-order will be applied during the next layout cycle triggered by other means.
                if (control?.Parent == this)
                {
                    LayoutLogger.Log($"StackLayout [{this.Name}]: Setlay_FloatZOrder on '{control.Name}' to {zOrderMode}. No immediate PerformLayout needed.");
                    // Optional: Could trigger Invalidate if visual debugging is needed immediately, but not required for layout.
                    // this.Invalidate(true);
                }
            }
        }

        // --- lay_IncludeHiddenInLayout (NEW PROPERTY) ---
        [DefaultValue(false)] // Default is NOT to include hidden controls
        [Category(categorySTR)]
        [Description("If true, this control will reserve space in the layout calculation even when its Visible property is false. If false (default), hidden controls are ignored by the layout unless they are floating.")]
        public bool Getlay_IncludeHiddenInLayout(Control control) => Common_GetProp(control).IncludeHiddenInLayout;
        public void Setlay_IncludeHiddenInLayout(Control control, bool include)
        {
            StackProperties props = GetOrCreateProperties(control);

            if (props.IncludeHiddenInLayout != include)
            {
                props.IncludeHiddenInLayout = include;
                // Changing this requires a layout recalculation
                if (control?.Parent == this)
                {
                    LayoutLogger.Log($"StackLayout [{this.Name}]: Setlay_IncludeHiddenInLayout on '{control.Name}' to {include}. Triggering PerformLayout.");
                    this.PerformLayout();
                    this.Invalidate(true); // Repaint might be needed if spacing changes
                }
            }
        }

        #endregion


        // --- Optional: TypeConverter for FloatTargetName ---
        // This helper class provides a dropdown list of sibling control names
        // in the designer for the lay_FloatTargetName property.
        internal class FloatTargetNameConverter : StringConverter
        {
            public override bool GetStandardValuesSupported(ITypeDescriptorContext context) => true;
            public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) => false; // Allow typing names too

            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                List<string> names = new List<string>();
                names.Add(""); // Option for no target (relative to panel)

                if (context?.Instance is Control sourceControl && sourceControl.Parent is StackLayout stackLayout)
                {
                    // Add names of all *other* visible controls within the same StackLayout
                    foreach (Control sibling in stackLayout.Controls.OfType<Control>())
                    {
                        if (sibling != sourceControl && sibling.Visible && !string.IsNullOrEmpty(sibling.Name))
                        {
                            names.Add(sibling.Name);
                        }
                    }
                }
                return new StandardValuesCollection(names.OrderBy(n => n).ToList());
            }
        }



    } // End partial class StackLayout
} // End namespace

