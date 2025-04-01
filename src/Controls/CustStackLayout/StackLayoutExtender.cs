using System;
using System.Collections; // Required for Hashtable
using System.ComponentModel;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq; // Optional: If using LayoutLogger inside Setters

namespace SharpBrowser.Controls // Ensure this namespace matches StackLayout.cs
{
    /// <summary>
    /// Provides extender properties (like lay_ExpandWeight, lay_IsFloating) for controls
    /// contained within a StackLayout panel. This is a partial class definition,
    /// merged with StackLayout.cs by the compiler.
    /// </summary>
    [ProvideProperty("lay_ExpandWeight", typeof(Control))]
    [ProvideProperty("lay_IsFloating", typeof(Control))]
    [ProvideProperty("lay_FloatTargetName", typeof(Control))]
    [ProvideProperty("lay_FloatOffsetX", typeof(Control))]
    [ProvideProperty("lay_FloatOffsetY", typeof(Control))]
    [ProvideProperty("lay_FloatAlignment", typeof(Control))]
    [ProvideProperty("lay_FloatZOrder", typeof(Control))]
    public partial class StackLayout : IExtenderProvider // Inherits Panel from other file, implements IExtenderProvider here
    {
        // --- Private Fields (Storage for Provided Properties) ---
        // These Hashtables store the extender property values associated with each child control.
        // They are instance members of the StackLayout class.
        private Hashtable _expandWeights = new Hashtable();
        private Hashtable _lay_isFloatingFlags = new Hashtable();
        private Hashtable _lay_floatTargetNames = new Hashtable();
        private Hashtable _lay_floatOffsetsX = new Hashtable();
        private Hashtable _lay_floatOffsetsY = new Hashtable();
        private Hashtable _lay_floatAlignments = new Hashtable();
        private Hashtable _lay_floatZOrderModes = new Hashtable();


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

        #region Provided Property Getters/Setters

        // --- lay_ExpandWeight ---
        [DefaultValue(0)]
        [Description("The weight used to distribute extra space along the orientation axis. 0 = no expansion. Positive values distribute remaining space proportionally.")]
        [Category(categorySTR)] // categorySTR constant is defined in StackLayout.cs
        public int Getlay_ExpandWeight(Control control)
        {
            // Retrieve the value from the Hashtable or return the default (0)
            return _expandWeights.Contains(control) ? (int)_expandWeights[control] : 0;
        }
        public void Setlay_ExpandWeight(Control control, int weight)
        {
            weight = Math.Max(0, weight); // Ensure weight is not negative
            int currentWeight = Getlay_ExpandWeight(control);

            if (currentWeight != weight)
            {
                _expandWeights[control] = weight; // Store the new value

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
        public bool Getlay_IsFloating(Control control)
        {
            return _lay_isFloatingFlags.Contains(control) ? (bool)_lay_isFloatingFlags[control] : false;
        }
        public void Setlay_IsFloating(Control control, bool isFloating)
        {
            if (Getlay_IsFloating(control) != isFloating)
            {
                _lay_isFloatingFlags[control] = isFloating;
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
        public string Getlay_FloatTargetName(Control control)
        {
            return _lay_floatTargetNames.Contains(control) ? (string)_lay_floatTargetNames[control] : "";
        }
        public void Setlay_FloatTargetName(Control control, string targetName)
        {
            targetName = targetName ?? ""; // Normalize null to empty string
            if (Getlay_FloatTargetName(control) != targetName)
            {
                _lay_floatTargetNames[control] = targetName;
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
        public int Getlay_FloatOffsetX(Control control)
        {
            return _lay_floatOffsetsX.Contains(control) ? (int)_lay_floatOffsetsX[control] : 0;
        }
        public void Setlay_FloatOffsetX(Control control, int offsetX)
        {
            if (Getlay_FloatOffsetX(control) != offsetX)
            {
                _lay_floatOffsetsX[control] = offsetX;
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
        public int Getlay_FloatOffsetY(Control control)
        {
            return _lay_floatOffsetsY.Contains(control) ? (int)_lay_floatOffsetsY[control] : 0;
        }
        public void Setlay_FloatOffsetY(Control control, int offsetY)
        {
            if (Getlay_FloatOffsetY(control) != offsetY)
            {
                _lay_floatOffsetsY[control] = offsetY;
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
        public FloatAlignment Getlay_FloatAlignment(Control control)
        {
            return _lay_floatAlignments.Contains(control) ? (FloatAlignment)_lay_floatAlignments[control] : FloatAlignment.TopLeft;
        }
        public void Setlay_FloatAlignment(Control control, FloatAlignment alignment)
        {
            if (Getlay_FloatAlignment(control) != alignment)
            {
                _lay_floatAlignments[control] = alignment;
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
        public StackFloatZOrder Getlay_FloatZOrder(Control control)
        {
            return _lay_floatZOrderModes.Contains(control) ? (StackFloatZOrder)_lay_floatZOrderModes[control] : StackFloatZOrder.InFrontOfTarget;
        }
        public void Setlay_FloatZOrder(Control control, StackFloatZOrder zOrderMode)
        {
            if (Getlay_FloatZOrder(control) != zOrderMode)
            {
                _lay_floatZOrderModes[control] = zOrderMode;
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

// --- END OF FILE StackLayout.Extender.cs ---