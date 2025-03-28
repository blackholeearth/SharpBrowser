using SharpBrowser.Controls;

namespace SharpBrowser
{
    partial class Form2_testStackpanel
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            SLayE1 = new StackLayoutExtender(components);
            TxtURL = new System.Windows.Forms.TextBox();
            button1 = new System.Windows.Forms.Button();
            textBox1 = new System.Windows.Forms.TextBox();
            button2 = new System.Windows.Forms.Button();
            button3 = new System.Windows.Forms.Button();
            stackLayout2 = new StackLayout();
            BtnBack = new FontAwesome.Sharp.IconButton();
            BtnForward = new FontAwesome.Sharp.IconButton();
            BtnRefresh = new FontAwesome.Sharp.IconButton();
            BtnStop = new FontAwesome.Sharp.IconButton();
            lbl_ZoomLevel = new System.Windows.Forms.Label();
            BtnDownloads = new FontAwesome.Sharp.IconButton();
            BtnHome = new FontAwesome.Sharp.IconButton();
            BtnMenu = new FontAwesome.Sharp.IconButton();
            stackLayout1 = new StackLayout();
            ıconButton1 = new FontAwesome.Sharp.IconButton();
            ıconButton2 = new FontAwesome.Sharp.IconButton();
            ıconButton3 = new FontAwesome.Sharp.IconButton();
            ıconButton4 = new FontAwesome.Sharp.IconButton();
            label1 = new System.Windows.Forms.Label();
            ıconButton5 = new FontAwesome.Sharp.IconButton();
            ıconButton6 = new FontAwesome.Sharp.IconButton();
            ıconButton7 = new FontAwesome.Sharp.IconButton();
            stackLayout2.SuspendLayout();
            stackLayout1.SuspendLayout();
            SuspendLayout();
            // 
            // TxtURL
            // 
            TxtURL.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            SLayE1.SetExpandWeight(TxtURL, 1);
            TxtURL.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            TxtURL.Location = new System.Drawing.Point(184, 27);
            TxtURL.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            TxtURL.Name = "TxtURL";
            TxtURL.Size = new System.Drawing.Size(117, 34);
            TxtURL.TabIndex = 25;
            // 
            // button1
            // 
            SLayE1.SetExpandWeight(button1, 1);
            button1.Location = new System.Drawing.Point(501, 30);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(206, 29);
            button1.TabIndex = 17;
            button1.Text = "button1";
            button1.UseVisualStyleBackColor = true;
            // 
            // textBox1
            // 
            textBox1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            SLayE1.SetExpandWeight(textBox1, 1);
            textBox1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            textBox1.Location = new System.Drawing.Point(184, 27);
            textBox1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            textBox1.Name = "textBox1";
            textBox1.Size = new System.Drawing.Size(225, 34);
            textBox1.TabIndex = 25;
            // 
            // button2
            // 
            SLayE1.SetExpandWeight(button2, 1);
            button2.Location = new System.Drawing.Point(609, 30);
            button2.Name = "button2";
            button2.Size = new System.Drawing.Size(224, 29);
            button2.TabIndex = 17;
            button2.Text = "button2";
            button2.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            SLayE1.SetExpandWeight(button3, 1);
            button3.Location = new System.Drawing.Point(717, 30);
            button3.Name = "button3";
            button3.Size = new System.Drawing.Size(172, 29);
            button3.TabIndex = 27;
            button3.Text = "button3";
            button3.UseVisualStyleBackColor = true;
            // 
            // stackLayout2
            // 
            stackLayout2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            stackLayout2.ChildAxisAlignment = StackChildAxisAlignment.Center;
            stackLayout2.Controls.Add(BtnBack);
            stackLayout2.Controls.Add(BtnForward);
            stackLayout2.Controls.Add(BtnRefresh);
            stackLayout2.Controls.Add(BtnStop);
            stackLayout2.Controls.Add(TxtURL);
            stackLayout2.Controls.Add(lbl_ZoomLevel);
            stackLayout2.Controls.Add(BtnDownloads);
            stackLayout2.Controls.Add(BtnHome);
            stackLayout2.Controls.Add(BtnMenu);
            stackLayout2.Controls.Add(button1);
            stackLayout2.Controls.Add(button3);
            stackLayout2.Location = new System.Drawing.Point(42, 90);
            stackLayout2.Name = "stackLayout2";
            stackLayout2.Orientation = StackOrientation.Horizontal;
            stackLayout2.Size = new System.Drawing.Size(835, 91);
            stackLayout2.Spacing = 10;
            stackLayout2.TabIndex = 17;
            // 
            // BtnBack
            // 
            BtnBack.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            BtnBack.FlatAppearance.BorderSize = 0;
            BtnBack.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            BtnBack.ForeColor = System.Drawing.Color.White;
            BtnBack.IconChar = FontAwesome.Sharp.IconChar.ArrowLeft;
            BtnBack.IconColor = System.Drawing.Color.Black;
            BtnBack.IconFont = FontAwesome.Sharp.IconFont.Auto;
            BtnBack.IconSize = 30;
            BtnBack.Location = new System.Drawing.Point(0, 26);
            BtnBack.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            BtnBack.Name = "BtnBack";
            BtnBack.Size = new System.Drawing.Size(36, 36);
            BtnBack.TabIndex = 20;
            BtnBack.UseVisualStyleBackColor = true;
            // 
            // BtnForward
            // 
            BtnForward.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            BtnForward.FlatAppearance.BorderSize = 0;
            BtnForward.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            BtnForward.ForeColor = System.Drawing.Color.White;
            BtnForward.IconChar = FontAwesome.Sharp.IconChar.ArrowRight;
            BtnForward.IconColor = System.Drawing.Color.Black;
            BtnForward.IconFont = FontAwesome.Sharp.IconFont.Auto;
            BtnForward.IconSize = 30;
            BtnForward.Location = new System.Drawing.Point(46, 26);
            BtnForward.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            BtnForward.Name = "BtnForward";
            BtnForward.Size = new System.Drawing.Size(36, 36);
            BtnForward.TabIndex = 21;
            BtnForward.UseVisualStyleBackColor = true;
            // 
            // BtnRefresh
            // 
            BtnRefresh.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            BtnRefresh.FlatAppearance.BorderSize = 0;
            BtnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            BtnRefresh.ForeColor = System.Drawing.Color.White;
            BtnRefresh.IconChar = FontAwesome.Sharp.IconChar.Refresh;
            BtnRefresh.IconColor = System.Drawing.Color.Black;
            BtnRefresh.IconFont = FontAwesome.Sharp.IconFont.Auto;
            BtnRefresh.IconSize = 30;
            BtnRefresh.Location = new System.Drawing.Point(92, 26);
            BtnRefresh.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            BtnRefresh.Name = "BtnRefresh";
            BtnRefresh.Size = new System.Drawing.Size(36, 36);
            BtnRefresh.TabIndex = 23;
            BtnRefresh.UseVisualStyleBackColor = true;
            // 
            // BtnStop
            // 
            BtnStop.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            BtnStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            BtnStop.ForeColor = System.Drawing.Color.White;
            BtnStop.IconChar = FontAwesome.Sharp.IconChar.Cancel;
            BtnStop.IconColor = System.Drawing.Color.Black;
            BtnStop.IconFont = FontAwesome.Sharp.IconFont.Auto;
            BtnStop.IconSize = 30;
            BtnStop.Location = new System.Drawing.Point(138, 26);
            BtnStop.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            BtnStop.Name = "BtnStop";
            BtnStop.Size = new System.Drawing.Size(36, 36);
            BtnStop.TabIndex = 22;
            BtnStop.UseVisualStyleBackColor = true;
            // 
            // lbl_ZoomLevel
            // 
            lbl_ZoomLevel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            lbl_ZoomLevel.AutoSize = true;
            lbl_ZoomLevel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            lbl_ZoomLevel.Location = new System.Drawing.Point(311, 34);
            lbl_ZoomLevel.Name = "lbl_ZoomLevel";
            lbl_ZoomLevel.Size = new System.Drawing.Size(42, 20);
            lbl_ZoomLevel.TabIndex = 26;
            lbl_ZoomLevel.Text = "???%";
            lbl_ZoomLevel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // BtnDownloads
            // 
            BtnDownloads.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            BtnDownloads.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            BtnDownloads.FlatAppearance.BorderSize = 0;
            BtnDownloads.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            BtnDownloads.ForeColor = System.Drawing.Color.White;
            BtnDownloads.IconChar = FontAwesome.Sharp.IconChar.Download;
            BtnDownloads.IconColor = System.Drawing.Color.Black;
            BtnDownloads.IconFont = FontAwesome.Sharp.IconFont.Auto;
            BtnDownloads.IconSize = 30;
            BtnDownloads.Location = new System.Drawing.Point(363, 26);
            BtnDownloads.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            BtnDownloads.Name = "BtnDownloads";
            BtnDownloads.Size = new System.Drawing.Size(36, 36);
            BtnDownloads.TabIndex = 24;
            BtnDownloads.Tag = "Downloads";
            BtnDownloads.UseVisualStyleBackColor = true;
            // 
            // BtnHome
            // 
            BtnHome.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            BtnHome.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            BtnHome.FlatAppearance.BorderSize = 0;
            BtnHome.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            BtnHome.ForeColor = System.Drawing.Color.White;
            BtnHome.IconChar = FontAwesome.Sharp.IconChar.House;
            BtnHome.IconColor = System.Drawing.Color.Black;
            BtnHome.IconFont = FontAwesome.Sharp.IconFont.Auto;
            BtnHome.IconSize = 30;
            BtnHome.Location = new System.Drawing.Point(409, 26);
            BtnHome.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            BtnHome.Name = "BtnHome";
            BtnHome.Size = new System.Drawing.Size(36, 36);
            BtnHome.TabIndex = 18;
            BtnHome.Tag = "Home";
            BtnHome.UseVisualStyleBackColor = true;
            // 
            // BtnMenu
            // 
            BtnMenu.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            BtnMenu.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            BtnMenu.FlatAppearance.BorderSize = 0;
            BtnMenu.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Silver;
            BtnMenu.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(224, 224, 224);
            BtnMenu.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            BtnMenu.ForeColor = System.Drawing.Color.White;
            BtnMenu.IconChar = FontAwesome.Sharp.IconChar.Bars;
            BtnMenu.IconColor = System.Drawing.Color.Black;
            BtnMenu.IconFont = FontAwesome.Sharp.IconFont.Auto;
            BtnMenu.IconSize = 30;
            BtnMenu.Location = new System.Drawing.Point(455, 26);
            BtnMenu.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            BtnMenu.Name = "BtnMenu";
            BtnMenu.Size = new System.Drawing.Size(36, 36);
            BtnMenu.TabIndex = 19;
            BtnMenu.Tag = "Menu3dot";
            BtnMenu.UseVisualStyleBackColor = true;
            // 
            // stackLayout1
            // 
            stackLayout1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            stackLayout1.ChildAxisAlignment = StackChildAxisAlignment.Center;
            stackLayout1.Controls.Add(ıconButton1);
            stackLayout1.Controls.Add(ıconButton2);
            stackLayout1.Controls.Add(ıconButton3);
            stackLayout1.Controls.Add(ıconButton4);
            stackLayout1.Controls.Add(textBox1);
            stackLayout1.Controls.Add(label1);
            stackLayout1.Controls.Add(ıconButton5);
            stackLayout1.Controls.Add(ıconButton6);
            stackLayout1.Controls.Add(ıconButton7);
            stackLayout1.Controls.Add(button2);
            stackLayout1.Location = new System.Drawing.Point(42, 206);
            stackLayout1.Name = "stackLayout1";
            stackLayout1.Orientation = StackOrientation.Horizontal;
            stackLayout1.PerformLayout_calcMethod_No = 4;
            stackLayout1.Size = new System.Drawing.Size(835, 91);
            stackLayout1.Spacing = 10;
            stackLayout1.TabIndex = 27;
            // 
            // ıconButton1
            // 
            ıconButton1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            ıconButton1.FlatAppearance.BorderSize = 0;
            ıconButton1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            ıconButton1.ForeColor = System.Drawing.Color.White;
            ıconButton1.IconChar = FontAwesome.Sharp.IconChar.ArrowLeft;
            ıconButton1.IconColor = System.Drawing.Color.Black;
            ıconButton1.IconFont = FontAwesome.Sharp.IconFont.Auto;
            ıconButton1.IconSize = 30;
            ıconButton1.Location = new System.Drawing.Point(0, 26);
            ıconButton1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            ıconButton1.Name = "ıconButton1";
            ıconButton1.Size = new System.Drawing.Size(36, 36);
            ıconButton1.TabIndex = 20;
            ıconButton1.UseVisualStyleBackColor = true;
            // 
            // ıconButton2
            // 
            ıconButton2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            ıconButton2.FlatAppearance.BorderSize = 0;
            ıconButton2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            ıconButton2.ForeColor = System.Drawing.Color.White;
            ıconButton2.IconChar = FontAwesome.Sharp.IconChar.ArrowRight;
            ıconButton2.IconColor = System.Drawing.Color.Black;
            ıconButton2.IconFont = FontAwesome.Sharp.IconFont.Auto;
            ıconButton2.IconSize = 30;
            ıconButton2.Location = new System.Drawing.Point(46, 26);
            ıconButton2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            ıconButton2.Name = "ıconButton2";
            ıconButton2.Size = new System.Drawing.Size(36, 36);
            ıconButton2.TabIndex = 21;
            ıconButton2.UseVisualStyleBackColor = true;
            // 
            // ıconButton3
            // 
            ıconButton3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            ıconButton3.FlatAppearance.BorderSize = 0;
            ıconButton3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            ıconButton3.ForeColor = System.Drawing.Color.White;
            ıconButton3.IconChar = FontAwesome.Sharp.IconChar.Refresh;
            ıconButton3.IconColor = System.Drawing.Color.Black;
            ıconButton3.IconFont = FontAwesome.Sharp.IconFont.Auto;
            ıconButton3.IconSize = 30;
            ıconButton3.Location = new System.Drawing.Point(92, 26);
            ıconButton3.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            ıconButton3.Name = "ıconButton3";
            ıconButton3.Size = new System.Drawing.Size(36, 36);
            ıconButton3.TabIndex = 23;
            ıconButton3.UseVisualStyleBackColor = true;
            // 
            // ıconButton4
            // 
            ıconButton4.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            ıconButton4.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            ıconButton4.ForeColor = System.Drawing.Color.White;
            ıconButton4.IconChar = FontAwesome.Sharp.IconChar.Cancel;
            ıconButton4.IconColor = System.Drawing.Color.Black;
            ıconButton4.IconFont = FontAwesome.Sharp.IconFont.Auto;
            ıconButton4.IconSize = 30;
            ıconButton4.Location = new System.Drawing.Point(138, 26);
            ıconButton4.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            ıconButton4.Name = "ıconButton4";
            ıconButton4.Size = new System.Drawing.Size(36, 36);
            ıconButton4.TabIndex = 22;
            ıconButton4.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            label1.AutoSize = true;
            label1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            label1.Location = new System.Drawing.Point(419, 34);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(42, 20);
            label1.TabIndex = 26;
            label1.Text = "???%";
            label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // ıconButton5
            // 
            ıconButton5.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            ıconButton5.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            ıconButton5.FlatAppearance.BorderSize = 0;
            ıconButton5.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            ıconButton5.ForeColor = System.Drawing.Color.White;
            ıconButton5.IconChar = FontAwesome.Sharp.IconChar.Download;
            ıconButton5.IconColor = System.Drawing.Color.Black;
            ıconButton5.IconFont = FontAwesome.Sharp.IconFont.Auto;
            ıconButton5.IconSize = 30;
            ıconButton5.Location = new System.Drawing.Point(471, 26);
            ıconButton5.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            ıconButton5.Name = "ıconButton5";
            ıconButton5.Size = new System.Drawing.Size(36, 36);
            ıconButton5.TabIndex = 24;
            ıconButton5.Tag = "Downloads";
            ıconButton5.UseVisualStyleBackColor = true;
            // 
            // ıconButton6
            // 
            ıconButton6.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            ıconButton6.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            ıconButton6.FlatAppearance.BorderSize = 0;
            ıconButton6.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            ıconButton6.ForeColor = System.Drawing.Color.White;
            ıconButton6.IconChar = FontAwesome.Sharp.IconChar.House;
            ıconButton6.IconColor = System.Drawing.Color.Black;
            ıconButton6.IconFont = FontAwesome.Sharp.IconFont.Auto;
            ıconButton6.IconSize = 30;
            ıconButton6.Location = new System.Drawing.Point(517, 26);
            ıconButton6.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            ıconButton6.Name = "ıconButton6";
            ıconButton6.Size = new System.Drawing.Size(36, 36);
            ıconButton6.TabIndex = 18;
            ıconButton6.Tag = "Home";
            ıconButton6.UseVisualStyleBackColor = true;
            // 
            // ıconButton7
            // 
            ıconButton7.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            ıconButton7.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            ıconButton7.FlatAppearance.BorderSize = 0;
            ıconButton7.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Silver;
            ıconButton7.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(224, 224, 224);
            ıconButton7.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            ıconButton7.ForeColor = System.Drawing.Color.White;
            ıconButton7.IconChar = FontAwesome.Sharp.IconChar.Bars;
            ıconButton7.IconColor = System.Drawing.Color.Black;
            ıconButton7.IconFont = FontAwesome.Sharp.IconFont.Auto;
            ıconButton7.IconSize = 30;
            ıconButton7.Location = new System.Drawing.Point(563, 26);
            ıconButton7.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            ıconButton7.Name = "ıconButton7";
            ıconButton7.Size = new System.Drawing.Size(36, 36);
            ıconButton7.TabIndex = 19;
            ıconButton7.Tag = "Menu3dot";
            ıconButton7.UseVisualStyleBackColor = true;
            // 
            // Form2_testStackpanel
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(909, 450);
            Controls.Add(stackLayout1);
            Controls.Add(stackLayout2);
            Name = "Form2_testStackpanel";
            Text = "Form2_testStackpanel";
            stackLayout2.ResumeLayout(false);
            stackLayout2.PerformLayout();
            stackLayout1.ResumeLayout(false);
            stackLayout1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private StackLayoutExtender SLayE1;
        private StackLayout stackLayout2;
        private FontAwesome.Sharp.IconButton BtnBack;
        private FontAwesome.Sharp.IconButton BtnForward;
        private FontAwesome.Sharp.IconButton BtnRefresh;
        private FontAwesome.Sharp.IconButton BtnStop;
        private System.Windows.Forms.TextBox TxtURL;
        private System.Windows.Forms.Label lbl_ZoomLevel;
        private FontAwesome.Sharp.IconButton BtnDownloads;
        private FontAwesome.Sharp.IconButton BtnHome;
        private FontAwesome.Sharp.IconButton BtnMenu;
        private System.Windows.Forms.Button button1;
        private StackLayout stackLayout1;
        private FontAwesome.Sharp.IconButton ıconButton1;
        private FontAwesome.Sharp.IconButton ıconButton2;
        private FontAwesome.Sharp.IconButton ıconButton3;
        private FontAwesome.Sharp.IconButton ıconButton4;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label1;
        private FontAwesome.Sharp.IconButton ıconButton5;
        private FontAwesome.Sharp.IconButton ıconButton6;
        private FontAwesome.Sharp.IconButton ıconButton7;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
    }
}