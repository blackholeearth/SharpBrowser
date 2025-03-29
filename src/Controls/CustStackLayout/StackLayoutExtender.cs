using System;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Diagnostics; // Optional for debugging

namespace SharpBrowser.Controls // Your namespace
{
    // --- ProvideProperty Attributes ---
    // Note: Ensure the ExpandWeight one matches your final choice (lay_ or not)
    [ProvideProperty("lay_ExpandWeight", typeof(Control))]
    [ProvideProperty("lay_IsFloating", typeof(Control))]
    [ProvideProperty("lay_FloatTargetName", typeof(Control))]
    [ProvideProperty("lay_FloatOffsetX", typeof(Control))]
    [ProvideProperty("lay_FloatOffsetY", typeof(Control))]
    [ProvideProperty("lay_FloatAlignment", typeof(Control))] 
    [ProvideProperty("lay_FloatZOrder", typeof(Control))] 
    public class StackLayoutExtender : Component, IExtenderProvider
    {
        // --- Backing Hashtables ---
        private Hashtable _expandWeights = new Hashtable(); // Or _lay_expandWeights if you prefixed
        private Hashtable _lay_isFloatingFlags = new Hashtable();
        private Hashtable _lay_floatTargetNames = new Hashtable();
        private Hashtable _lay_floatOffsetsX = new Hashtable();
        private Hashtable _lay_floatOffsetsY = new Hashtable();
        private Hashtable _lay_floatAlignments = new Hashtable();
        private Hashtable _lay_floatZOrderModes = new Hashtable();

        #region IExtenderProvider Members

        public bool CanExtend(object extendee)
        {
            // Extend any Control whose Parent is a StackLayout
            return (extendee is Control control && control.Parent is StackLayout);
        }

        #endregion

        // --- Provided Properties ---

        #region lay_FloatZOrder Property (Provided)
        [DefaultValue(StackFloatZOrder.InFrontOfTarget)] // Default enum value
        [Category(StackLayout.categorySTR)] // Use the category defined in StackLayout
        [Description("Defines how this floating control's Z-order is managed relative to its target during layout. AboveTarget (Default), BelowTarget, or Manual (respects designer settings).")]
        public StackFloatZOrder Getlay_FloatZOrder(Control control) // Getter name MUST match ProvideProperty
        {
            // Return the stored value or the default if not set
            return _lay_floatZOrderModes.Contains(control) ? (StackFloatZOrder)_lay_floatZOrderModes[control] : StackFloatZOrder.InFrontOfTarget;
        }
        public void Setlay_FloatZOrder(Control control, StackFloatZOrder zOrderMode) // Setter name MUST match ProvideProperty
        {
            // Store the selected enum value
            if (Getlay_FloatZOrder(control) == zOrderMode) return; // No change
            _lay_floatZOrderModes[control] = zOrderMode;

            // IMPORTANT: Do NOT trigger PerformLayout here.
            // Changing the Z-order *mode* only affects how the *next* layout is performed.
            // It doesn't require an immediate re-layout itself and calling it here
            // could interfere with designer actions or cause unnecessary churn.
            // Z-order changes will be applied during the next natural layout cycle
            // triggered by other events (resize, visibility change, etc.)
        }
        #endregion


        #region lay_FloatAlignment Property (Provided)
        [DefaultValue(FloatAlignment.TopLeft)] // Set default
        [Category(StackLayout.categorySTR)]
        [Description("Specifies how the floating control is initially positioned relative to its target before offsets are applied.")]
        public FloatAlignment Getlay_FloatAlignment(Control control) // Prefixed
        {
            return _lay_floatAlignments.Contains(control) ? (FloatAlignment)_lay_floatAlignments[control] : FloatAlignment.TopLeft; // Default
        }
        public void Setlay_FloatAlignment(Control control, FloatAlignment alignment) // Prefixed
        {
            if (Getlay_FloatAlignment(control) == alignment) return; // No change
            _lay_floatAlignments[control] = alignment;
            if (control?.Parent is StackLayout parentStackLayout)
            {
                parentStackLayout.PerformLayout();
                parentStackLayout.Invalidate(true);
            }
        }
        #endregion


        #region lay_ExpandWeight Property (Provided)
        [DefaultValue(0)]
        [Description("The weight used to distribute extra space along the orientation axis. 0 = no expansion. Positive values distribute remaining space proportionally.")]
        [Category(StackLayout.categorySTR)]
        public int Getlay_ExpandWeight(Control control) // Ensure name matches ProvideProperty
        {
            // Use appropriate Hashtable name if you changed it
            return _expandWeights.Contains(control) ? (int)_expandWeights[control] : 0;
        }
        public void Setlay_ExpandWeight(Control control, int weight) // Ensure name matches ProvideProperty
        {
            weight = Math.Max(0, weight);
            // Use appropriate Hashtable name if you changed it
            int currentWeight = Getlay_ExpandWeight(control);
            if (currentWeight == weight) return;

            _expandWeights[control] = weight; // Use appropriate Hashtable name
            if (control?.Parent is StackLayout parentStackLayout)
            {
                // Debug.WriteLine($"StackLayoutExtender DEBUG: Setlay_ExpandWeight triggering PerformLayout on '{parentStackLayout.Name}' for control '{control.Name}'");
                parentStackLayout.PerformLayout();
                parentStackLayout.Invalidate(true);
            }
        }
        #endregion

        #region lay_IsFloating Property (Provided)
        [DefaultValue(false)]
        [Category(StackLayout.categorySTR)]
        [Description("If true, the control is positioned relative to a lay_FloatTargetName control instead of being part of the stack flow.")]
        public bool Getlay_IsFloating(Control control) // Prefixed
        {
            return _lay_isFloatingFlags.Contains(control) ? (bool)_lay_isFloatingFlags[control] : false;
        }
        public void Setlay_IsFloating(Control control, bool isFloating) // Prefixed
        {
            if (Getlay_IsFloating(control) == isFloating) return; // No change
            _lay_isFloatingFlags[control] = isFloating;
            if (control?.Parent is StackLayout parentStackLayout)
            {
                parentStackLayout.PerformLayout();
                parentStackLayout.Invalidate(true);
            }
        }
        #endregion

        #region lay_FloatTargetName Property (Provided)
        [DefaultValue("")]
        [Category(StackLayout.categorySTR)]
        [Description("The Name of the sibling control this floating control should be positioned relative to.")]
        public string Getlay_FloatTargetName(Control control) // Prefixed
        {
            return _lay_floatTargetNames.Contains(control) ? (string)_lay_floatTargetNames[control] : "";
        }
        public void Setlay_FloatTargetName(Control control, string targetName) // Prefixed
        {
            targetName = targetName ?? ""; // Normalize null
            if (Getlay_FloatTargetName(control) == targetName) return; // No change
            _lay_floatTargetNames[control] = targetName;
            if (control?.Parent is StackLayout parentStackLayout)
            {
                parentStackLayout.PerformLayout();
                parentStackLayout.Invalidate(true);
            }
        }
        #endregion

        #region lay_FloatOffsetX Property (Provided)
        [DefaultValue(0)]
        [Category(StackLayout.categorySTR)]
        [Description("The horizontal offset (in pixels) relative to the lay_FloatTargetName control's Left edge.")]
        public int Getlay_FloatOffsetX(Control control) // Prefixed
        {
            return _lay_floatOffsetsX.Contains(control) ? (int)_lay_floatOffsetsX[control] : 0;
        }
        public void Setlay_FloatOffsetX(Control control, int offsetX) // Prefixed
        {
            if (Getlay_FloatOffsetX(control) == offsetX) return; // No change
            _lay_floatOffsetsX[control] = offsetX;
            if (control?.Parent is StackLayout parentStackLayout)
            {
                parentStackLayout.PerformLayout();
                parentStackLayout.Invalidate(true);
            }
        }
        #endregion

        #region lay_FloatOffsetY Property (Provided)
        [DefaultValue(0)]
        [Category(StackLayout.categorySTR)]
        [Description("The vertical offset (in pixels) relative to the lay_FloatTargetName control's Top edge.")]
        public int Getlay_FloatOffsetY(Control control) // Prefixed
        {
            return _lay_floatOffsetsY.Contains(control) ? (int)_lay_floatOffsetsY[control] : 0;
        }
        public void Setlay_FloatOffsetY(Control control, int offsetY) // Prefixed
        {
            if (Getlay_FloatOffsetY(control) == offsetY) return; // No change
            _lay_floatOffsetsY[control] = offsetY;
            if (control?.Parent is StackLayout parentStackLayout)
            {
                parentStackLayout.PerformLayout();
                parentStackLayout.Invalidate(true);
            }
        }
        #endregion


        #region Component Designer Generated Code
        // --- Standard Component Designer Code ---
        private System.ComponentModel.IContainer components = null;
        public StackLayoutExtender(System.ComponentModel.IContainer container) { container.Add(this); InitializeComponent(); }
        public StackLayoutExtender() { InitializeComponent(); }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null){
                    components.Dispose();
                }
                // Clear all Hashtables to release control references
                _expandWeights?.Clear();
                _lay_isFloatingFlags?.Clear();
                _lay_floatTargetNames?.Clear();
                _lay_floatOffsetsX?.Clear();
                _lay_floatOffsetsY?.Clear();
                _lay_floatAlignments?.Clear();
                _lay_floatZOrderModes?.Clear();  
            }
            // No unmanaged resources to free, but call base class
            base.Dispose(disposing);
        }
        private void InitializeComponent() { components = new System.ComponentModel.Container(); }
        #endregion

    } // End Class StackLayoutExtender
} // End namespace