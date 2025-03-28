using System;
using System.Collections; // For Hashtable (or use Dictionary<Control, int>)
using System.ComponentModel;
using System.Windows.Forms;

namespace SharpBrowser.Controls // Your namespace
{
    /// <summary>
    /// Provides the 'ExpandWeight' property to controls contained within a StackLayout.
    /// </summary>
    [ProvideProperty("ExpandWeight", typeof(Control))] // Tells VS this provides "ExpandWeight" for Controls
    public class StackLayoutExtender : Component, IExtenderProvider
    {
        //public const string categorySTR = "L_ayout2";

        // Use Hashtable or Dictionary<Control, int> to store weights for extended controls
        private Hashtable _expandWeights = new Hashtable();

        #region IExtenderProvider Members

        /// <summary>
        /// Specifies whether this object can provide its extender properties to the specified object.
        /// </summary>
        /// <param name="extendee">The object to receive the extender properties.</param>
        /// <returns>true if this object can provide extender properties to the specified object; otherwise, false.</returns>
        public bool CanExtend(object extendee)
        {
            // We can extend any Control that is sited (on the form)
            // and whose Parent is a StackLayout.
            if (extendee is Control control && control.Site != null && control.Parent is StackLayout)
            {
                return true;
            }
            return false;
        }

        #endregion

        #region ExpandWeight Property (Provided)

        /// <summary>
        /// Gets the weight used to distribute extra space along the orientation axis for a specific control within a StackLayout.
        /// A weight of 0 means the control does not expand. Positive values are relative weights.
        /// </summary>
        [DefaultValue(0)] // Default weight is 0 (no expansion)
        [Description("The weight used to distribute extra space along the orientation axis. 0 = no expansion. Positive values distribute remaining space proportionally.")]
        [Category(StackLayout.categorySTR)] // Use your category
        public int GetExpandWeight(Control control)
        {
            // Return the stored weight, or the default (0) if not found
            return _expandWeights.Contains(control) ? (int)_expandWeights[control] : 0;
        }

        /// <summary>
        /// Sets the weight used to distribute extra space for a specific control.
        /// </summary>
        public void SetExpandWeight(Control control, int weight)
        {
            // Ensure weight is not negative
            weight = Math.Max(0, weight);

            // Store the new weight
            _expandWeights[control] = weight;

            // --- Trigger Layout Update on Parent StackLayout ---
            // When the weight changes (esp. in the designer), we need to tell the parent StackLayout to re-calculate.
            if (control != null && control.Parent is StackLayout parentStackLayout)
            {
                // Check if we are in design mode - invoking PerforLayout directly might cause issues sometimes
                if (parentStackLayout.IsHandleCreated && !parentStackLayout.IsDisposed) // Check if safe to invoke
                {
                    // Use BeginInvoke for safety, esp. in designer or cross-thread scenarios
                    parentStackLayout.BeginInvoke((MethodInvoker)delegate {
                        parentStackLayout.PerformLayout();
                        parentStackLayout.Invalidate(); // Ensure repaint
                    });
                }
                else
                {
                    // Fallback for designer: Just invalidate to suggest a redraw, hoping PerformLayout gets called
                    parentStackLayout.Invalidate();
                }

                // Simpler alternative (might work fine, but BeginInvoke is safer):
                // parentStackLayout.PerformLayout();
                // parentStackLayout.Invalidate();
            }
        }

        #endregion

        #region Component Designer Generated Code (Simplified)

        private Container components = null;

        public StackLayoutExtender(IContainer container)
        {
            container.Add(this);
            InitializeComponent();
        }

        public StackLayoutExtender()
        {
            InitializeComponent();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
                // Clean up Hashtable if necessary (though GC should handle it)
                _expandWeights.Clear();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
        }

        #endregion
    }
}