using System;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Diagnostics; // Optional for debugging

namespace SharpBrowser.Controls // Your namespace
{
    /// <summary>
    /// Provides the 'lay_ExpandWeight' property to controls contained within a StackLayout.
    /// </summary>
    [ProvideProperty("lay_IsFloating", typeof(Control))] 
    [ProvideProperty("lay_FloatTargetName", typeof(Control))] 
    [ProvideProperty("lay_FloatOffsetX", typeof(Control))] 
    [ProvideProperty("lay_FloatOffsetY", typeof(Control))] 
    [ProvideProperty("lay_ExpandWeight", typeof(Control))] 
    public class StackLayoutExtender : Component, IExtenderProvider
    {
        private Hashtable _expandWeights = new Hashtable();

        #region IExtenderProvider Members

        public bool CanExtend(object extendee)
        {
            return (extendee is Control control && control.Parent is StackLayout);
        }

        #endregion

        #region lay_ExpandWeight Property (Provided)

        /// <summary>
        /// Gets the weight used to distribute extra space along the orientation axis for a specific control within a StackLayout.
        /// A weight of 0 means the control does not expand. Positive values are relative weights.
        /// </summary>
        [DefaultValue(0)]
        [Description("The weight used to distribute extra space along the orientation axis. 0 = no expansion. Positive values distribute remaining space proportionally.")]
        // *** UPDATED Category to use constant from StackLayout ***
        [Category(StackLayout.categorySTR)] // Use the constant
        // *** UPDATED Method Name ***
        public int Getlay_ExpandWeight(Control control)
        {
            return _expandWeights.Contains(control) ? (int)_expandWeights[control] : 0;
        }

        /// <summary>
        /// Sets the weight used to distribute extra space for a specific control.
        /// </summary>
        // *** UPDATED Method Name ***
        public void Setlay_ExpandWeight(Control control, int weight)
        {
            weight = Math.Max(0, weight); // Ensure non-negative

            // *** UPDATED to call Getlay_ExpandWeight ***
            int currentWeight = Getlay_ExpandWeight(control);
            if (currentWeight == weight)
            {
                return; // No change
            }

            _expandWeights[control] = weight;
            // Debug.WriteLine($"Setlay_ExpandWeight: Set '{control?.Name}' to {weight}"); // Optional Debug

            // Trigger Layout Update on Parent StackLayout
            if (control?.Parent is StackLayout parentStackLayout)
            {
                Debug.WriteLine($"StackLayoutExtender DEBUG: Setlay_ExpandWeight triggering PerformLayout on '{parentStackLayout.Name}' for control '{control.Name}'"); // DEBUG Line
                parentStackLayout.PerformLayout();
                parentStackLayout.Invalidate(true);
            }
        }

        #endregion


        // --- Add these Hashtables in StackLayoutExtender.cs ---
        private Hashtable _lay_isFloatingFlags = new Hashtable();
        private Hashtable _lay_floatTargetNames = new Hashtable();
        private Hashtable _lay_floatOffsetsX = new Hashtable();
        private Hashtable _lay_floatOffsetsY = new Hashtable();


        // --- Implement the Get/Set methods in StackLayoutExtender.cs ---

        #region IsFloating Property (Provided)
        [DefaultValue(false)]
        [Category(StackLayout.categorySTR)]
        [Description("If true, the control is positioned relative to a FloatTargetName control instead of being part of the stack flow.")]
        public bool GetIsFloating(Control control)
        {
            return _isFloatingFlags.Contains(control) ? (bool)_isFloatingFlags[control] : false;
        }
        public void SetIsFloating(Control control, bool isFloating)
        {
            if (GetIsFloating(control) == isFloating) return; // No change
            _isFloatingFlags[control] = isFloating;
            control.Parent?.PerformLayout(); // Trigger layout on parent
            control.Parent?.Invalidate(true);
        }
        #endregion

        #region FloatTargetName Property (Provided)
        [DefaultValue("")]
        [Category(StackLayout.categorySTR)]
        [Description("The Name of the sibling control this floating control should be positioned relative to.")]
        public string GetFloatTargetName(Control control)
        {
            return _floatTargetNames.Contains(control) ? (string)_floatTargetNames[control] : "";
        }
        public void SetFloatTargetName(Control control, string targetName)
        {
            // Normalize null to empty string for consistency
            targetName = targetName ?? "";
            if (GetFloatTargetName(control) == targetName) return; // No change
            _floatTargetNames[control] = targetName;
            control.Parent?.PerformLayout(); // Trigger layout on parent
            control.Parent?.Invalidate(true);
        }
        #endregion

        #region FloatOffsetX Property (Provided)
        [DefaultValue(0)]
        [Category(StackLayout.categorySTR)]
        [Description("The horizontal offset (in pixels) relative to the FloatTargetName control's Left edge.")]
        public int GetFloatOffsetX(Control control)
        {
            return _floatOffsetsX.Contains(control) ? (int)_floatOffsetsX[control] : 0;
        }
        public void SetFloatOffsetX(Control control, int offsetX)
        {
            if (GetFloatOffsetX(control) == offsetX) return; // No change
            _floatOffsetsX[control] = offsetX;
            control.Parent?.PerformLayout(); // Trigger layout on parent
            control.Parent?.Invalidate(true);
        }
        #endregion

        #region FloatOffsetY Property (Provided)
        [DefaultValue(0)]
        [Category(StackLayout.categorySTR)]
        [Description("The vertical offset (in pixels) relative to the FloatTargetName control's Top edge.")]
        public int GetFloatOffsetY(Control control)
        {
            return _floatOffsetsY.Contains(control) ? (int)_floatOffsetsY[control] : 0;
        }
        public void SetFloatOffsetY(Control control, int offsetY)
        {
            if (GetFloatOffsetY(control) == offsetY) return; // No change
            _floatOffsetsY[control] = offsetY;
            control.Parent?.PerformLayout(); // Trigger layout on parent
            control.Parent?.Invalidate(true);
        }
        #endregion

        // Remember to clear these Hashtables in Dispose:
        // protected override void Dispose( bool disposing ) { ... _isFloatingFlags?.Clear(); _floatTargetNames?.Clear(); ... }








        #region Component Designer Generated Code
        // --- Standard Component Designer Code (No changes needed here) ---
        private System.ComponentModel.IContainer components = null;
        public StackLayoutExtender(System.ComponentModel.IContainer container) { container.Add(this); InitializeComponent(); }
        public StackLayoutExtender() { InitializeComponent(); }
        protected override void Dispose(bool disposing) { if (disposing) { if (components != null) { components.Dispose(); } _expandWeights?.Clear(); } base.Dispose(disposing); }
        private void InitializeComponent() { components = new System.ComponentModel.Container(); }
        #endregion

    } // End Class StackLayoutExtender
} // End namespace