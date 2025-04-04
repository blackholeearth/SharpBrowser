using CefSharp.DevTools.LayerTree;
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
            TxtURL = new System.Windows.Forms.TextBox();
            TxtURL2 = new System.Windows.Forms.TextBox();
            button2 = new System.Windows.Forms.Button();
            BtnStop = new FontAwesome.Sharp.IconButton();
            lbl_ZoomLevel = new System.Windows.Forms.Label();
            BtnStop2 = new FontAwesome.Sharp.IconButton();
            button1 = new System.Windows.Forms.Button();
            label1 = new System.Windows.Forms.Label();
            button3 = new System.Windows.Forms.Button();
            button4 = new System.Windows.Forms.Button();
            stackLayout_no0 = new StackLayout();
            BtnBack = new FontAwesome.Sharp.IconButton();
            BtnForward = new FontAwesome.Sharp.IconButton();
            BtnRefresh = new FontAwesome.Sharp.IconButton();
            BtnDownloads = new FontAwesome.Sharp.IconButton();
            BtnHome = new FontAwesome.Sharp.IconButton();
            BtnMenu = new FontAwesome.Sharp.IconButton();
            stackLayout_no4 = new StackLayout();
            ıconButton1 = new FontAwesome.Sharp.IconButton();
            ıconButton2 = new FontAwesome.Sharp.IconButton();
            BtnRefresh2 = new FontAwesome.Sharp.IconButton();
            ıconButton5 = new FontAwesome.Sharp.IconButton();
            ıconButton6 = new FontAwesome.Sharp.IconButton();
            ıconButton7 = new FontAwesome.Sharp.IconButton();
            stackLayout1 = new StackLayout();
            radioButton1 = new System.Windows.Forms.RadioButton();
            button5 = new System.Windows.Forms.Button();
            flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            button6 = new System.Windows.Forms.Button();
            button7 = new System.Windows.Forms.Button();
            button8 = new System.Windows.Forms.Button();
            radioButton2 = new System.Windows.Forms.RadioButton();
            stackLayout_no0.SuspendLayout();
            stackLayout_no4.SuspendLayout();
            stackLayout1.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // TxtURL
            // 
            TxtURL.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            TxtURL.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            stackLayout_no0.Setlay_ExpandWeight(TxtURL, 1);
            TxtURL.Location = new System.Drawing.Point(228, 40);
            TxtURL.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            TxtURL.Name = "TxtURL";
            TxtURL.Size = new System.Drawing.Size(406, 34);
            TxtURL.TabIndex = 25;
            // 
            // TxtURL2
            // 
            TxtURL2.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            TxtURL2.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            stackLayout_no4.Setlay_ExpandWeight(TxtURL2, 6);
            TxtURL2.Location = new System.Drawing.Point(156, 11);
            TxtURL2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            TxtURL2.Name = "TxtURL2";
            TxtURL2.Size = new System.Drawing.Size(430, 34);
            TxtURL2.TabIndex = 25;
            // 
            // button2
            // 
            stackLayout_no4.Setlay_ExpandWeight(button2, 1);
            button2.Location = new System.Drawing.Point(742, 13);
            button2.Name = "button2";
            button2.Size = new System.Drawing.Size(71, 29);
            button2.TabIndex = 17;
            button2.Text = "button2";
            button2.UseVisualStyleBackColor = true;
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
            stackLayout_no0.Setlay_FloatAlignment(BtnStop, FloatAlignment.ToRightOf);
            stackLayout_no0.Setlay_FloatOffsetX(BtnStop, -36);
            stackLayout_no0.Setlay_FloatTargetName(BtnStop, "BtnRefresh");
            stackLayout_no0.Setlay_FloatZOrder(BtnStop, StackFloatZOrder.Manual);
            stackLayout_no0.Setlay_IsFloating(BtnStop, true);
            BtnStop.Location = new System.Drawing.Point(152, 39);
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
            stackLayout_no0.Setlay_FloatAlignment(lbl_ZoomLevel, FloatAlignment.ToRightOf);
            stackLayout_no0.Setlay_FloatOffsetX(lbl_ZoomLevel, -60);
            stackLayout_no0.Setlay_FloatOffsetY(lbl_ZoomLevel, 7);
            stackLayout_no0.Setlay_FloatTargetName(lbl_ZoomLevel, "TxtURL");
            stackLayout_no0.Setlay_FloatZOrder(lbl_ZoomLevel, StackFloatZOrder.Manual);
            stackLayout_no0.Setlay_IsFloating(lbl_ZoomLevel, true);
            lbl_ZoomLevel.Location = new System.Drawing.Point(574, 47);
            lbl_ZoomLevel.Name = "lbl_ZoomLevel";
            lbl_ZoomLevel.Size = new System.Drawing.Size(42, 20);
            lbl_ZoomLevel.TabIndex = 26;
            lbl_ZoomLevel.Text = "???%";
            lbl_ZoomLevel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            lbl_ZoomLevel.Visible = false;
            // 
            // BtnStop2
            // 
            BtnStop2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            BtnStop2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            BtnStop2.ForeColor = System.Drawing.Color.White;
            BtnStop2.IconChar = FontAwesome.Sharp.IconChar.Cancel;
            BtnStop2.IconColor = System.Drawing.Color.Black;
            BtnStop2.IconFont = FontAwesome.Sharp.IconFont.Auto;
            BtnStop2.IconSize = 30;
            stackLayout_no4.Setlay_FloatAlignment(BtnStop2, FloatAlignment.ToRightOf);
            stackLayout_no4.Setlay_FloatOffsetX(BtnStop2, -36);
            stackLayout_no4.Setlay_FloatTargetName(BtnStop2, "BtnRefresh2");
            stackLayout_no4.Setlay_IsFloating(BtnStop2, true);
            BtnStop2.Location = new System.Drawing.Point(108, 10);
            BtnStop2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            BtnStop2.Name = "BtnStop2";
            BtnStop2.Size = new System.Drawing.Size(36, 36);
            BtnStop2.TabIndex = 22;
            BtnStop2.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            stackLayout_no0.Setlay_ExpandWeight(button1, 1);
            button1.Location = new System.Drawing.Point(674, 32);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(133, 49);
            button1.TabIndex = 17;
            button1.Text = "button1";
            button1.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            label1.AutoSize = true;
            label1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            stackLayout_no4.Setlay_FloatAlignment(label1, FloatAlignment.ToRightOf);
            stackLayout_no4.Setlay_FloatOffsetX(label1, -60);
            stackLayout_no4.Setlay_FloatOffsetY(label1, 7);
            stackLayout_no4.Setlay_FloatTargetName(label1, "TxtURL2");
            stackLayout_no4.Setlay_FloatZOrder(label1, StackFloatZOrder.Manual);
            stackLayout_no4.Setlay_IsFloating(label1, true);
            label1.Location = new System.Drawing.Point(526, 18);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(42, 20);
            label1.TabIndex = 26;
            label1.Text = "???%";
            label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
            label1.Visible = false;
            // 
            // button3
            // 
            button3.Location = new System.Drawing.Point(598, 33);
            button3.Name = "button3";
            button3.Size = new System.Drawing.Size(88, 29);
            button3.TabIndex = 27;
            button3.Text = "button3";
            button3.UseVisualStyleBackColor = true;
            // 
            // button4
            // 
            stackLayout1.Setlay_ExpandWeight(button4, 1);
            button4.Location = new System.Drawing.Point(137, 33);
            button4.Name = "button4";
            button4.Size = new System.Drawing.Size(118, 29);
            button4.TabIndex = 0;
            button4.Text = "button4";
            button4.UseVisualStyleBackColor = true;
            // 
            // stackLayout_no0
            // 
            stackLayout_no0.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            stackLayout_no0.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            stackLayout_no0.Controls.Add(lbl_ZoomLevel);
            stackLayout_no0.Controls.Add(BtnBack);
            stackLayout_no0.Controls.Add(BtnForward);
            stackLayout_no0.Controls.Add(BtnRefresh);
            stackLayout_no0.Controls.Add(TxtURL);
            stackLayout_no0.Controls.Add(button1);
            stackLayout_no0.Controls.Add(BtnStop);
            stackLayout_no0.Controls.Add(BtnDownloads);
            stackLayout_no0.Controls.Add(BtnHome);
            stackLayout_no0.Controls.Add(BtnMenu);
            stackLayout_no0.lay_ChildAxisAlignment = StackChildAxisAlignment.Center;
            stackLayout_no0.lay_Orientation = StackOrientation.Horizontal;
            stackLayout_no0.lay_Spacing = 40;
            stackLayout_no0.Location = new System.Drawing.Point(43, 35);
            stackLayout_no0.Name = "stackLayout_no0";
            stackLayout_no0.Size = new System.Drawing.Size(827, 116);
            stackLayout_no0.TabIndex = 17;
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
            BtnBack.Location = new System.Drawing.Point(0, 39);
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
            BtnForward.Location = new System.Drawing.Point(76, 39);
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
            BtnRefresh.Location = new System.Drawing.Point(152, 39);
            BtnRefresh.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            BtnRefresh.Name = "BtnRefresh";
            BtnRefresh.Size = new System.Drawing.Size(36, 36);
            BtnRefresh.TabIndex = 23;
            BtnRefresh.UseVisualStyleBackColor = true;
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
            BtnDownloads.Location = new System.Drawing.Point(847, 39);
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
            BtnHome.Location = new System.Drawing.Point(923, 39);
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
            BtnMenu.Location = new System.Drawing.Point(999, 39);
            BtnMenu.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            BtnMenu.Name = "BtnMenu";
            BtnMenu.Size = new System.Drawing.Size(36, 36);
            BtnMenu.TabIndex = 19;
            BtnMenu.Tag = "Menu3dot";
            BtnMenu.UseVisualStyleBackColor = true;
            // 
            // stackLayout_no4
            // 
            stackLayout_no4.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            stackLayout_no4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            stackLayout_no4.Controls.Add(label1);
            stackLayout_no4.Controls.Add(ıconButton1);
            stackLayout_no4.Controls.Add(ıconButton2);
            stackLayout_no4.Controls.Add(BtnRefresh2);
            stackLayout_no4.Controls.Add(BtnStop2);
            stackLayout_no4.Controls.Add(TxtURL2);
            stackLayout_no4.Controls.Add(ıconButton5);
            stackLayout_no4.Controls.Add(ıconButton6);
            stackLayout_no4.Controls.Add(ıconButton7);
            stackLayout_no4.Controls.Add(button2);
            stackLayout_no4.lay_ChildAxisAlignment = StackChildAxisAlignment.Center;
            stackLayout_no4.lay_Orientation = StackOrientation.Horizontal;
            stackLayout_no4.lay_PerformLayout_calcMethod_No = 4;
            stackLayout_no4.lay_Spacing = 12;
            stackLayout_no4.Location = new System.Drawing.Point(42, 212);
            stackLayout_no4.Name = "stackLayout_no4";
            stackLayout_no4.Padding = new System.Windows.Forms.Padding(12, 0, 12, 0);
            stackLayout_no4.Size = new System.Drawing.Size(827, 58);
            stackLayout_no4.TabIndex = 27;
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
            ıconButton1.Location = new System.Drawing.Point(12, 10);
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
            ıconButton2.Location = new System.Drawing.Point(60, 10);
            ıconButton2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            ıconButton2.Name = "ıconButton2";
            ıconButton2.Size = new System.Drawing.Size(36, 36);
            ıconButton2.TabIndex = 21;
            ıconButton2.UseVisualStyleBackColor = true;
            // 
            // BtnRefresh2
            // 
            BtnRefresh2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            BtnRefresh2.FlatAppearance.BorderSize = 0;
            BtnRefresh2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            BtnRefresh2.ForeColor = System.Drawing.Color.White;
            BtnRefresh2.IconChar = FontAwesome.Sharp.IconChar.Refresh;
            BtnRefresh2.IconColor = System.Drawing.Color.Black;
            BtnRefresh2.IconFont = FontAwesome.Sharp.IconFont.Auto;
            BtnRefresh2.IconSize = 30;
            BtnRefresh2.Location = new System.Drawing.Point(108, 10);
            BtnRefresh2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            BtnRefresh2.Name = "BtnRefresh2";
            BtnRefresh2.Size = new System.Drawing.Size(36, 36);
            BtnRefresh2.TabIndex = 23;
            BtnRefresh2.UseVisualStyleBackColor = true;
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
            ıconButton5.Location = new System.Drawing.Point(598, 10);
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
            ıconButton6.Location = new System.Drawing.Point(646, 10);
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
            ıconButton7.Location = new System.Drawing.Point(694, 10);
            ıconButton7.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            ıconButton7.Name = "ıconButton7";
            ıconButton7.Size = new System.Drawing.Size(36, 36);
            ıconButton7.TabIndex = 19;
            ıconButton7.Tag = "Menu3dot";
            ıconButton7.UseVisualStyleBackColor = true;
            // 
            // stackLayout1
            // 
            stackLayout1.Controls.Add(radioButton1);
            stackLayout1.Controls.Add(button4);
            stackLayout1.Controls.Add(button5);
            stackLayout1.lay_ChildAxisAlignment = StackChildAxisAlignment.Center;
            stackLayout1.lay_Orientation = StackOrientation.Horizontal;
            stackLayout1.lay_PerformLayout_calcMethod_No = 4;
            stackLayout1.lay_Spacing = 20;
            stackLayout1.Location = new System.Drawing.Point(42, 323);
            stackLayout1.Name = "stackLayout1";
            stackLayout1.Size = new System.Drawing.Size(418, 96);
            stackLayout1.TabIndex = 28;
            // 
            // radioButton1
            // 
            radioButton1.AutoSize = true;
            radioButton1.Location = new System.Drawing.Point(0, 36);
            radioButton1.Name = "radioButton1";
            radioButton1.Size = new System.Drawing.Size(117, 24);
            radioButton1.TabIndex = 29;
            radioButton1.TabStop = true;
            radioButton1.Text = "radioButton1";
            radioButton1.UseVisualStyleBackColor = true;
            // 
            // button5
            // 
            button5.Location = new System.Drawing.Point(275, 33);
            button5.Name = "button5";
            button5.Size = new System.Drawing.Size(143, 29);
            button5.TabIndex = 30;
            button5.Text = "button5";
            button5.UseVisualStyleBackColor = true;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Controls.Add(button6);
            flowLayoutPanel1.Controls.Add(button7);
            flowLayoutPanel1.Controls.Add(button8);
            flowLayoutPanel1.Controls.Add(radioButton2);
            flowLayoutPanel1.Location = new System.Drawing.Point(497, 323);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new System.Drawing.Size(325, 174);
            flowLayoutPanel1.TabIndex = 29;
            // 
            // button6
            // 
            button6.Location = new System.Drawing.Point(3, 3);
            button6.Name = "button6";
            button6.Size = new System.Drawing.Size(94, 29);
            button6.TabIndex = 0;
            button6.Text = "button6";
            button6.UseVisualStyleBackColor = true;
            // 
            // button7
            // 
            button7.Location = new System.Drawing.Point(103, 3);
            button7.Name = "button7";
            button7.Size = new System.Drawing.Size(94, 29);
            button7.TabIndex = 2;
            button7.Text = "button7";
            button7.UseVisualStyleBackColor = true;
            // 
            // button8
            // 
            button8.Location = new System.Drawing.Point(203, 3);
            button8.Name = "button8";
            button8.Size = new System.Drawing.Size(94, 29);
            button8.TabIndex = 3;
            button8.Text = "button8";
            button8.UseVisualStyleBackColor = true;
            // 
            // radioButton2
            // 
            radioButton2.AutoSize = true;
            radioButton2.Location = new System.Drawing.Point(3, 38);
            radioButton2.Name = "radioButton2";
            radioButton2.Size = new System.Drawing.Size(117, 24);
            radioButton2.TabIndex = 1;
            radioButton2.TabStop = true;
            radioButton2.Text = "radioButton2";
            radioButton2.UseVisualStyleBackColor = true;
            // 
            // Form2_testStackpanel
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(950, 546);
            Controls.Add(button3);
            Controls.Add(flowLayoutPanel1);
            Controls.Add(stackLayout1);
            Controls.Add(stackLayout_no4);
            Controls.Add(stackLayout_no0);
            Name = "Form2_testStackpanel";
            Text = "Form2_testStackpanel";
            Load += Form2_testStackpanel_Load;
            stackLayout_no0.ResumeLayout(false);
            stackLayout_no0.PerformLayout();
            stackLayout_no4.ResumeLayout(false);
            stackLayout_no4.PerformLayout();
            stackLayout1.ResumeLayout(false);
            stackLayout1.PerformLayout();
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private StackLayout stackLayout_no0;
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
        private StackLayout stackLayout_no4;
        private FontAwesome.Sharp.IconButton ıconButton1;
        private FontAwesome.Sharp.IconButton ıconButton2;
        private FontAwesome.Sharp.IconButton BtnRefresh2;
        private FontAwesome.Sharp.IconButton BtnStop2;
        private System.Windows.Forms.TextBox TxtURL2;
        private System.Windows.Forms.Label label1;
        private FontAwesome.Sharp.IconButton ıconButton5;
        private FontAwesome.Sharp.IconButton ıconButton6;
        private FontAwesome.Sharp.IconButton ıconButton7;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private StackLayout stackLayout1;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Button button8;
    }
}