﻿using SharpBrowser.Controls;

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
            stackLayout1 = new StackLayout();
            BtnBack = new FontAwesome.Sharp.IconButton();
            BtnForward = new FontAwesome.Sharp.IconButton();
            BtnRefresh = new FontAwesome.Sharp.IconButton();
            BtnStop = new FontAwesome.Sharp.IconButton();
            TxtURL = new System.Windows.Forms.TextBox();
            lbl_ZoomLevel = new System.Windows.Forms.Label();
            BtnDownloads = new FontAwesome.Sharp.IconButton();
            BtnHome = new FontAwesome.Sharp.IconButton();
            BtnMenu = new FontAwesome.Sharp.IconButton();
            sLayExt1 = new StackLayoutExtender(components);
            stackLayout1.SuspendLayout();
            SuspendLayout();
            // 
            // stackLayout1
            // 
            stackLayout1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            stackLayout1.ChildAxisAlignment = StackChildAxisAlignment.Center;
            stackLayout1.Controls.Add(BtnBack);
            stackLayout1.Controls.Add(BtnForward);
            stackLayout1.Controls.Add(BtnRefresh);
            stackLayout1.Controls.Add(BtnStop);
            stackLayout1.Controls.Add(TxtURL);
            stackLayout1.Controls.Add(lbl_ZoomLevel);
            stackLayout1.Controls.Add(BtnDownloads);
            stackLayout1.Controls.Add(BtnHome);
            stackLayout1.Controls.Add(BtnMenu);
            stackLayout1.Location = new System.Drawing.Point(141, 90);
            stackLayout1.Name = "stackLayout1";
            stackLayout1.Orientation = StackOrientation.Horizontal;
            stackLayout1.Size = new System.Drawing.Size(682, 91);
            stackLayout1.Spacing = 10;
            stackLayout1.TabIndex = 0;
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
            BtnBack.Location = new System.Drawing.Point(0, 25);
            BtnBack.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            BtnBack.Name = "BtnBack";
            BtnBack.Size = new System.Drawing.Size(36, 39);
            BtnBack.TabIndex = 10;
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
            BtnForward.Location = new System.Drawing.Point(46, 25);
            BtnForward.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            BtnForward.Name = "BtnForward";
            BtnForward.Size = new System.Drawing.Size(36, 39);
            BtnForward.TabIndex = 11;
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
            BtnRefresh.Location = new System.Drawing.Point(92, 25);
            BtnRefresh.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            BtnRefresh.Name = "BtnRefresh";
            BtnRefresh.Size = new System.Drawing.Size(36, 39);
            BtnRefresh.TabIndex = 13;
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
            BtnStop.Location = new System.Drawing.Point(138, 25);
            BtnStop.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            BtnStop.Name = "BtnStop";
            BtnStop.Size = new System.Drawing.Size(36, 39);
            BtnStop.TabIndex = 12;
            BtnStop.UseVisualStyleBackColor = true;
            // 
            // TxtURL
            // 
            TxtURL.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            sLayExt1.SetExpandWeight(TxtURL, 1);
            TxtURL.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            TxtURL.Location = new System.Drawing.Point(184, 27);
            TxtURL.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            TxtURL.Name = "TxtURL";
            TxtURL.Size = new System.Drawing.Size(210, 34);
            TxtURL.TabIndex = 15;
            // 
            // lbl_ZoomLevel
            // 
            lbl_ZoomLevel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            lbl_ZoomLevel.AutoSize = true;
            lbl_ZoomLevel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            lbl_ZoomLevel.Location = new System.Drawing.Point(404, 34);
            lbl_ZoomLevel.Name = "lbl_ZoomLevel";
            lbl_ZoomLevel.Size = new System.Drawing.Size(42, 20);
            lbl_ZoomLevel.TabIndex = 16;
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
            BtnDownloads.Location = new System.Drawing.Point(456, 25);
            BtnDownloads.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            BtnDownloads.Name = "BtnDownloads";
            BtnDownloads.Size = new System.Drawing.Size(36, 39);
            BtnDownloads.TabIndex = 14;
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
            BtnHome.Location = new System.Drawing.Point(502, 25);
            BtnHome.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            BtnHome.Name = "BtnHome";
            BtnHome.Size = new System.Drawing.Size(36, 39);
            BtnHome.TabIndex = 8;
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
            BtnMenu.Location = new System.Drawing.Point(548, 25);
            BtnMenu.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            BtnMenu.Name = "BtnMenu";
            BtnMenu.Size = new System.Drawing.Size(36, 39);
            BtnMenu.TabIndex = 9;
            BtnMenu.Tag = "Menu3dot";
            BtnMenu.UseVisualStyleBackColor = true;
            // 
            // Form2_testStackpanel
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(909, 450);
            Controls.Add(stackLayout1);
            Name = "Form2_testStackpanel";
            Text = "Form2_testStackpanel";
            stackLayout1.ResumeLayout(false);
            stackLayout1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private StackLayout stackLayout1;
        private System.Windows.Forms.Label lbl_ZoomLevel;
        private System.Windows.Forms.TextBox TxtURL;
        private FontAwesome.Sharp.IconButton BtnDownloads;
        private FontAwesome.Sharp.IconButton BtnStop;
        private FontAwesome.Sharp.IconButton BtnForward;
        private FontAwesome.Sharp.IconButton BtnRefresh;
        private FontAwesome.Sharp.IconButton BtnBack;
        private FontAwesome.Sharp.IconButton BtnHome;
        private FontAwesome.Sharp.IconButton BtnMenu;
        private StackLayoutExtender sLayExt1;
    }
}