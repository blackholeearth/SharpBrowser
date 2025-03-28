using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SharpBrowser.Controls // Your chosen namespace
{
    //made with gemini 2.5 experimental

    // Use ToolboxBitmap attribute for a custom icon in the toolbox (optional)
    // [ToolboxBitmap(typeof(TextBox))] // Example: Use TextBox icon
    public partial class BorderedTextBox2 : UserControl
    {
        // --- Child Control ---
        private TextBox textBox;

        // --- Fields for Border Properties ---
        //private Color _borderColor = Color.MediumSlateBlue;
        private Color _borderColor = Color.DarkGray;
        private Color _borderFocusColor = Color.SkyBlue;
        private int _borderThickness = 2;
        private int _borderRadius = 5;
        private bool _isFocused = false; // Track focus state based on inner TextBox

        // --- Properties ---

        [Category("Appearance")]
        [Description("The border color of the control.")]
        public Color BorderColor
        {
            get => _borderColor;
            set
            {
                if (_borderColor != value)
                {
                    _borderColor = value;
                    this.Invalidate(); // Redraw the container's border
                }
            }
        }

        [Category("Appearance")]
        [Description("The border color of the control when the inner TextBox has focus.")]
        public Color BorderFocusColor
        {
            get => _borderFocusColor;
            set => _borderFocusColor = value; // Store, redraw happens on focus change
        }

        [Category("Appearance")]
        [Description("The thickness of the border.")]
        public int BorderThickness
        {
            get => _borderThickness;
            set
            {
                // Ensure thickness is non-negative
                _borderThickness = Math.Max(0, value);
                UpdateTextBoxPadding(); // Adjust inner TextBox position
                this.Invalidate();     // Redraw the container's border
            }
        }

        [Category("Appearance")]
        [Description("The radius for the rounded corners of the border.")]
        public int BorderRadius
        {
            get => _borderRadius;
            set
            {
                // Ensure radius is non-negative
                _borderRadius = Math.Max(0, value);
                this.Invalidate(); // Redraw the container's border
            }
        }

        // --- Expose Inner TextBox Properties (Proxy Properties) ---

        [Category("Behavior")]
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public override string Text
        {
            get => textBox.Text;
            set => textBox.Text = value;
        }

        [Category("Behavior")]
        [DefaultValue(false)]
        public bool ReadOnly
        {
            get => textBox.ReadOnly;
            set => textBox.ReadOnly = value;
        }

        [Category("Behavior")]
        [DefaultValue(false)]
        public bool Multiline
        {
            get => textBox.Multiline;
            set
            {
                textBox.Multiline = value;
                // Maybe adjust height based on multiline? Typically handled by AutoSize or anchors.
            }
        }

        [Category("Behavior")]
        [DefaultValue(HorizontalAlignment.Left)]
        public HorizontalAlignment TextAlign
        {
            get => textBox.TextAlign;
            set => textBox.TextAlign = value;
        }

        [Category("Behavior")]
        [DefaultValue('\0')] // Using \0 to represent null char for default PasswordChar
        public char PasswordChar
        {
            get => textBox.PasswordChar;
            set => textBox.PasswordChar = value;
        }

        [Category("Behavior")]
        [DefaultValue(32767)]
        public int MaxLength
        {
            get => textBox.MaxLength;
            set => textBox.MaxLength = value;
        }

        // Expose essential properties for appearance that should match the TextBox
        public override Color ForeColor { get => textBox.ForeColor; set => textBox.ForeColor = value; }
        public override Font Font { get => textBox.Font; set { textBox.Font = value; base.Font = value; UpdateTextBoxPadding(); } } // Update padding if font changes size
        public override Color BackColor { get => base.BackColor; set { base.BackColor = value; textBox.BackColor = value; } } // Match TextBox BackColor to container

        // --- Constructor ---
        public BorderedTextBox2()
        {
            InitializeComponentManual(); // Manually create and configure the inner TextBox

            this.DoubleBuffered = true; // Enable double buffering for the UserControl
            this.ResizeRedraw = true;   // Redraw when the UserControl resizes

            // Set initial Padding based on default border thickness
            UpdateTextBoxPadding();
        }


        // --- Manual Initialization (Instead of using the designer for the inner TextBox) ---
        private void InitializeComponentManual()
        {
            this.textBox = new TextBox();
            this.SuspendLayout();
            //
            // textBox
            //
            this.textBox.BorderStyle = BorderStyle.None; // Crucial: No default border
            this.textBox.Dock = DockStyle.Fill;        // Fill the padded area of the UserControl
            this.textBox.Location = new Point(0, 0);    // Position will be controlled by Padding
            this.textBox.Name = "textBox";
            this.textBox.Size = new Size(150, 20);    // Initial size, Dock=Fill takes precedence
            this.textBox.TabIndex = 0;
            // Wire up events from the inner TextBox to the UserControl's events/methods
            this.textBox.TextChanged += TextBox_TextChanged;
            this.textBox.Click += TextBox_Click;
            this.textBox.DoubleClick += TextBox_DoubleClick;
            this.textBox.Enter += TextBox_Enter;
            this.textBox.Leave += TextBox_Leave;
            this.textBox.KeyDown += TextBox_KeyDown;
            this.textBox.KeyPress += TextBox_KeyPress;
            this.textBox.KeyUp += TextBox_KeyUp;
            this.textBox.MouseEnter += TextBox_MouseEnter;
            this.textBox.MouseLeave += TextBox_MouseLeave;
            this.textBox.MouseMove += TextBox_MouseMove;
            // Add more events as needed...

            //
            // BorderedTextBox (UserControl)
            //
            this.AutoScaleMode = AutoScaleMode.None; // Prevent scaling issues
            this.Controls.Add(this.textBox);
            this.Name = "BorderedTextBox";
            this.Padding = new Padding(5); // Initial padding, will be updated
            this.Size = new Size(170, 30); // Example initial size
            this.BackColor = SystemColors.Window; // Default background
            this.textBox.BackColor = this.BackColor; // Match inner textbox background

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        // --- Forward events from inner TextBox to the outside world
        private void TextBox_TextChanged(object sender, EventArgs e) => this.OnTextChanged(e);
        private void TextBox_Click(object sender, EventArgs e) => this.OnClick(e);
        private void TextBox_DoubleClick(object sender, EventArgs e) => this.OnDoubleClick(e);
        private void TextBox_KeyDown(object sender, KeyEventArgs e) => this.OnKeyDown(e);
        private void TextBox_KeyPress(object sender, KeyPressEventArgs e) => this.OnKeyPress(e);
        private void TextBox_KeyUp(object sender, KeyEventArgs e) => this.OnKeyUp(e);
        private void TextBox_MouseEnter(object sender, EventArgs e) => this.OnMouseEnter(e);
        private void TextBox_MouseLeave(object sender, EventArgs e) => this.OnMouseLeave(e);
        private void TextBox_MouseMove(object sender, MouseEventArgs e) => this.OnMouseMove(e);
        // Add more forwarded events here...


        // --- Event Handlers for Inner TextBox (Forwarding / Updating State) ---
        private void TextBox_Enter(object sender, EventArgs e)
        {
            _isFocused = true;
            this.Invalidate(); // Redraw border with focus color
            this.OnEnter(e);   // Raise the UserControl's Enter event
        }
        private void TextBox_Leave(object sender, EventArgs e)
        {
            _isFocused = false;
            this.Invalidate(); // Redraw border with non-focus color
            this.OnLeave(e);   // Raise the UserControl's Leave event
        }
         
        /// <summary>
        /// Updates the Padding of the UserControl based on the BorderThickness.
        /// This creates the space around the inner TextBox where the border will be drawn.
        /// </summary>
        private void UpdateTextBoxPadding()
        {
            // Add a little extra padding (e.g., 2px) inside the border for visual spacing
            int internalPadding = 2;
            // Calculate total padding needed
            int totalPadding = _borderThickness + internalPadding;
            // Ensure padding is not negative if borderThickness is 0
            totalPadding = Math.Max(1, totalPadding); // Minimum padding of 1

            // Adjust TextBox height slightly if multiline or font is large? Usually handled by Dock=Fill/AutoSize.
            // For simple cases, just set the Padding.
            this.Padding = new Padding(totalPadding);

            // Optional: Adjust minimum height based on font and padding
            this.MinimumSize = new Size(0, textBox.PreferredHeight + this.Padding.Top + this.Padding.Bottom);
        }

        // --- Overrides ---

        /// <summary>
        /// Override OnPaint to draw the custom border.
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e); // Let the UserControl draw its background

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Determine the current border color
            Color currentColor = _isFocused ? _borderFocusColor : _borderColor;

            // Only draw if border thickness > 0
            if (_borderThickness > 0)
            {
                using (Pen borderPen = new Pen(currentColor, _borderThickness))
                {
                    // Use PenAlignment.Inset to draw border inside the UserControl's bounds
                    borderPen.Alignment = PenAlignment.Inset;

                    // Define the rectangle for the border using the UserControl's ClientRectangle
                    RectangleF rectF = new RectangleF(this.ClientRectangle.X,
                                                     this.ClientRectangle.Y,
                                                     this.ClientRectangle.Width - 1, // Adjust for Inset
                                                     this.ClientRectangle.Height - 1); // Adjust for Inset

                    // Ensure rectangle is valid before drawing
                    if (rectF.Width >= 1 && rectF.Height >= 1)
                    {
                        // Create and draw the rounded rectangle path
                        using (GraphicsPath path = GetRoundedRectPath(rectF, _borderRadius))
                        {
                            // Draw the border
                            g.DrawPath(borderPen, path);

                            // Optional: Fill the path interior if needed (usually not for a border)
                            // using(SolidBrush backBrush = new SolidBrush(this.BackColor)) {
                            //    g.FillPath(backBrush, path);
                            // }
                        }
                    }
                }
            }
            // Optional Debugging: Draw rectangle around client area
            // g.DrawRectangle(Pens.Magenta, 0,0, this.ClientSize.Width-1, this.ClientSize.Height-1);
        }

        /// <summary>
        /// When the container UserControl gets focus, pass it to the inner TextBox.
        /// </summary>
        protected override void OnEnter(EventArgs e)
        {
            // base.OnEnter(e); // Don't call base.OnEnter directly, use the TextBox's Enter event
            if (textBox != null && !textBox.IsDisposed)
                textBox.Focus();
        }

        /// <summary>
        /// Handle resizing.
        /// </summary>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            // Optional: Add logic here if specific recalculations are needed on resize,
            // but ResizeRedraw=true and Dock=Fill handle most cases.
            // Invalidate(); // Already handled by ResizeRedraw=true
        }


        // --- GraphicsPath Helper (Same as before) ---
        private GraphicsPath GetRoundedRectPath(RectangleF bounds, float radius)
        {
            GraphicsPath path = new GraphicsPath();
            float diameter = radius * 2.0f;

            if (radius <= 0 || bounds.Width < 1 || bounds.Height < 1) { path.AddRectangle(bounds); return path; }
            if (diameter > bounds.Width) diameter = bounds.Width;
            if (diameter > bounds.Height) diameter = bounds.Height;

            RectangleF arcRectTopLeft = new RectangleF(bounds.Left, bounds.Top, diameter, diameter);
            RectangleF arcRectTopRight = new RectangleF(bounds.Right - diameter, bounds.Top, diameter, diameter);
            RectangleF arcRectBottomRight = new RectangleF(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter);
            RectangleF arcRectBottomLeft = new RectangleF(bounds.Left, bounds.Bottom - diameter, diameter, diameter);

            if (arcRectTopLeft.Width > 0 && arcRectTopLeft.Height > 0) path.AddArc(arcRectTopLeft, 180, 90); else path.AddLine(bounds.Left, bounds.Top, bounds.Left, bounds.Top);
            if (arcRectTopRight.Width > 0 && arcRectTopRight.Height > 0) path.AddArc(arcRectTopRight, 270, 90); else path.AddLine(bounds.Right, bounds.Top, bounds.Right, bounds.Top);
            if (arcRectBottomRight.Width > 0 && arcRectBottomRight.Height > 0) path.AddArc(arcRectBottomRight, 0, 90); else path.AddLine(bounds.Right, bounds.Bottom, bounds.Right, bounds.Bottom);
            if (arcRectBottomLeft.Width > 0 && arcRectBottomLeft.Height > 0) path.AddArc(arcRectBottomLeft, 90, 90); else path.AddLine(bounds.Left, bounds.Bottom, bounds.Left, bounds.Bottom);

            path.CloseFigure();
            return path;
        }

        // --- Dispose ---
        // Make sure to dispose the inner TextBox if you manually created it
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (textBox != null)
                {
                    // Unhook events to prevent memory leaks
                    textBox.TextChanged -= TextBox_TextChanged;
                    textBox.Click -= TextBox_Click;
                    textBox.DoubleClick -= TextBox_DoubleClick;
                    textBox.Enter -= TextBox_Enter;
                    textBox.Leave -= TextBox_Leave;
                    textBox.KeyDown -= TextBox_KeyDown;
                    textBox.KeyPress -= TextBox_KeyPress;
                    textBox.KeyUp -= TextBox_KeyUp;
                    textBox.MouseEnter -= TextBox_MouseEnter;
                    textBox.MouseLeave -= TextBox_MouseLeave;
                    textBox.MouseMove -= TextBox_MouseMove;
                    // ... unhook others ...

                    textBox.Dispose();
                    textBox = null;
                }
            }
            base.Dispose(disposing);
        }
    }
}