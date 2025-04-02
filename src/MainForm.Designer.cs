﻿using System.Windows.Forms;

namespace SharpBrowser
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            menuStripTab = new ContextMenuStrip(components);
            menuCloseTab = new ToolStripMenuItem();
            menuCloseOtherTabs = new ToolStripMenuItem();
            BtnRefresh = new FontAwesome.Sharp.IconButton();
            BtnStop = new FontAwesome.Sharp.IconButton();
            BtnForward = new FontAwesome.Sharp.IconButton();
            BtnBack = new FontAwesome.Sharp.IconButton();
            timer1 = new Timer(components);
            BtnDownloads = new FontAwesome.Sharp.IconButton();
            TxtURL = new TextBox();
            lbl_ZoomLevel = new Label();
            BtnMenu = new FontAwesome.Sharp.IconButton();
            BtnHome = new FontAwesome.Sharp.IconButton();
            TabPages = new SharpBrowser.Controls.BrowserTabStrip.BrowserTabStrip();
            tabStrip1 = new SharpBrowser.Controls.BrowserTabStrip.BrowserTabStripItem();
            tabStripAdd = new SharpBrowser.Controls.BrowserTabStrip.BrowserTabStripItem();
            PanelSearch = new Panel();
            BtnNextSearch = new Button();
            BtnPrevSearch = new Button();
            BtnCloseSearch = new Button();
            TxtSearch = new TextBox();
            PanelToolbar = new SharpBrowser.Controls.StackLayout();
            menuStripTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)TabPages).BeginInit();
            TabPages.SuspendLayout();
            PanelSearch.SuspendLayout();
            PanelToolbar.SuspendLayout();
            SuspendLayout();
            // 
            // menuStripTab
            // 
            menuStripTab.ImageScalingSize = new System.Drawing.Size(20, 20);
            menuStripTab.Items.AddRange(new ToolStripItem[] { menuCloseTab, menuCloseOtherTabs });
            menuStripTab.Name = "menuStripTab";
            menuStripTab.Size = new System.Drawing.Size(198, 52);
            // 
            // menuCloseTab
            // 
            menuCloseTab.Name = "menuCloseTab";
            menuCloseTab.ShortcutKeys = Keys.Control | Keys.F4;
            menuCloseTab.Size = new System.Drawing.Size(197, 24);
            menuCloseTab.Text = "Close tab";
            menuCloseTab.Click += menuCloseTab_Click;
            // 
            // menuCloseOtherTabs
            // 
            menuCloseOtherTabs.Name = "menuCloseOtherTabs";
            menuCloseOtherTabs.Size = new System.Drawing.Size(197, 24);
            menuCloseOtherTabs.Text = "Close other tabs";
            menuCloseOtherTabs.Click += menuCloseOtherTabs_Click;
            // 
            // BtnRefresh
            // 
            BtnRefresh.BackgroundImageLayout = ImageLayout.Zoom;
            BtnRefresh.FlatAppearance.BorderSize = 0;
            BtnRefresh.FlatStyle = FlatStyle.Flat;
            BtnRefresh.ForeColor = System.Drawing.Color.White;
            BtnRefresh.IconChar = FontAwesome.Sharp.IconChar.Refresh;
            BtnRefresh.IconColor = System.Drawing.Color.Black;
            BtnRefresh.IconFont = FontAwesome.Sharp.IconFont.Auto;
            BtnRefresh.IconSize = 30;
            PanelToolbar.Setlay_IncludeHiddenInLayout(BtnRefresh, true);
            BtnRefresh.Location = new System.Drawing.Point(96, 17);
            BtnRefresh.Margin = new Padding(3, 4, 3, 4);
            BtnRefresh.Name = "BtnRefresh";
            BtnRefresh.Size = new System.Drawing.Size(36, 30);
            BtnRefresh.TabIndex = 3;
            BtnRefresh.UseVisualStyleBackColor = true;
            BtnRefresh.Click += bRefresh_Click;
            // 
            // BtnStop
            // 
            BtnStop.BackgroundImageLayout = ImageLayout.Zoom;
            BtnStop.FlatStyle = FlatStyle.Flat;
            BtnStop.ForeColor = System.Drawing.Color.White;
            BtnStop.IconChar = FontAwesome.Sharp.IconChar.Cancel;
            BtnStop.IconColor = System.Drawing.Color.Black;
            BtnStop.IconFont = FontAwesome.Sharp.IconFont.Auto;
            BtnStop.IconSize = 30;
            PanelToolbar.Setlay_FloatTargetName(BtnStop, "BtnRefresh");
            PanelToolbar.Setlay_FloatZOrder(BtnStop, SharpBrowser.Controls.StackFloatZOrder.Manual);
            PanelToolbar.Setlay_IsFloating(BtnStop, true);
            BtnStop.Location = new System.Drawing.Point(96, 17);
            BtnStop.Margin = new Padding(3, 4, 3, 4);
            BtnStop.Name = "BtnStop";
            BtnStop.Size = new System.Drawing.Size(36, 30);
            BtnStop.TabIndex = 2;
            BtnStop.UseVisualStyleBackColor = true;
            BtnStop.Click += bStop_Click;
            // 
            // BtnForward
            // 
            BtnForward.BackgroundImageLayout = ImageLayout.Zoom;
            BtnForward.FlatAppearance.BorderSize = 0;
            BtnForward.FlatStyle = FlatStyle.Flat;
            BtnForward.ForeColor = System.Drawing.Color.White;
            BtnForward.IconChar = FontAwesome.Sharp.IconChar.ArrowRight;
            BtnForward.IconColor = System.Drawing.Color.Black;
            BtnForward.IconFont = FontAwesome.Sharp.IconFont.Auto;
            BtnForward.IconSize = 30;
            BtnForward.Location = new System.Drawing.Point(48, 17);
            BtnForward.Margin = new Padding(3, 4, 3, 4);
            BtnForward.Name = "BtnForward";
            BtnForward.Size = new System.Drawing.Size(36, 30);
            BtnForward.TabIndex = 1;
            BtnForward.UseVisualStyleBackColor = true;
            BtnForward.Click += bForward_Click;
            // 
            // BtnBack
            // 
            BtnBack.BackgroundImageLayout = ImageLayout.Zoom;
            BtnBack.FlatAppearance.BorderSize = 0;
            BtnBack.FlatStyle = FlatStyle.Flat;
            BtnBack.ForeColor = System.Drawing.Color.White;
            BtnBack.IconChar = FontAwesome.Sharp.IconChar.ArrowLeft;
            BtnBack.IconColor = System.Drawing.Color.Black;
            BtnBack.IconFont = FontAwesome.Sharp.IconFont.Auto;
            BtnBack.IconSize = 30;
            BtnBack.Location = new System.Drawing.Point(0, 17);
            BtnBack.Margin = new Padding(3, 4, 3, 4);
            BtnBack.Name = "BtnBack";
            BtnBack.Size = new System.Drawing.Size(36, 30);
            BtnBack.TabIndex = 0;
            BtnBack.UseVisualStyleBackColor = true;
            BtnBack.Click += bBack_Click;
            // 
            // timer1
            // 
            timer1.Interval = 50;
            timer1.Tick += timer1_Tick;
            // 
            // BtnDownloads
            // 
            BtnDownloads.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            BtnDownloads.BackgroundImageLayout = ImageLayout.Zoom;
            BtnDownloads.FlatAppearance.BorderSize = 0;
            BtnDownloads.FlatStyle = FlatStyle.Flat;
            BtnDownloads.ForeColor = System.Drawing.Color.White;
            BtnDownloads.IconChar = FontAwesome.Sharp.IconChar.Download;
            BtnDownloads.IconColor = System.Drawing.Color.Black;
            BtnDownloads.IconFont = FontAwesome.Sharp.IconFont.Auto;
            BtnDownloads.IconSize = 30;
            BtnDownloads.Location = new System.Drawing.Point(784, 17);
            BtnDownloads.Margin = new Padding(3, 4, 3, 4);
            BtnDownloads.Name = "BtnDownloads";
            BtnDownloads.Size = new System.Drawing.Size(36, 30);
            BtnDownloads.TabIndex = 4;
            BtnDownloads.Tag = "Downloads";
            BtnDownloads.UseVisualStyleBackColor = true;
            BtnDownloads.Click += bDownloads_Click;
            // 
            // TxtURL
            // 
            TxtURL.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            TxtURL.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            PanelToolbar.Setlay_ExpandWeight(TxtURL, 1);
            TxtURL.Location = new System.Drawing.Point(144, 15);
            TxtURL.Margin = new Padding(3, 4, 3, 4);
            TxtURL.Name = "TxtURL";
            TxtURL.Size = new System.Drawing.Size(628, 34);
            TxtURL.TabIndex = 5;
            TxtURL.Click += TxtURL_Click;
            TxtURL.Enter += TxtURL_Enter;
            TxtURL.KeyDown += TxtURL_KeyDown;
            // 
            // lbl_ZoomLevel
            // 
            lbl_ZoomLevel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lbl_ZoomLevel.AutoSize = true;
            lbl_ZoomLevel.FlatStyle = FlatStyle.Flat;
            PanelToolbar.Setlay_FloatAlignment(lbl_ZoomLevel, SharpBrowser.Controls.FloatAlignment.ToRightOf);
            PanelToolbar.Setlay_FloatOffsetX(lbl_ZoomLevel, -65);
            PanelToolbar.Setlay_FloatOffsetY(lbl_ZoomLevel, 7);
            PanelToolbar.Setlay_FloatTargetName(lbl_ZoomLevel, "TxtURL");
            PanelToolbar.Setlay_IsFloating(lbl_ZoomLevel, true);
            lbl_ZoomLevel.Location = new System.Drawing.Point(707, 22);
            lbl_ZoomLevel.Name = "lbl_ZoomLevel";
            lbl_ZoomLevel.Size = new System.Drawing.Size(42, 20);
            lbl_ZoomLevel.TabIndex = 9;
            lbl_ZoomLevel.Text = "???%";
            lbl_ZoomLevel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            lbl_ZoomLevel.Click += lbl_ZoomLevel_Click;
            lbl_ZoomLevel.MouseEnter += lbl_ZoomLevel_MouseEnter;
            // 
            // BtnMenu
            // 
            BtnMenu.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            BtnMenu.BackgroundImageLayout = ImageLayout.Zoom;
            BtnMenu.FlatAppearance.BorderSize = 0;
            BtnMenu.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Silver;
            BtnMenu.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(224, 224, 224);
            BtnMenu.FlatStyle = FlatStyle.Flat;
            BtnMenu.ForeColor = System.Drawing.Color.White;
            BtnMenu.IconChar = FontAwesome.Sharp.IconChar.Bars;
            BtnMenu.IconColor = System.Drawing.Color.Black;
            BtnMenu.IconFont = FontAwesome.Sharp.IconFont.Auto;
            BtnMenu.IconSize = 30;
            BtnMenu.Location = new System.Drawing.Point(880, 17);
            BtnMenu.Margin = new Padding(3, 4, 3, 4);
            BtnMenu.Name = "BtnMenu";
            BtnMenu.Size = new System.Drawing.Size(36, 30);
            BtnMenu.TabIndex = 7;
            BtnMenu.Tag = "Menu3dot";
            BtnMenu.UseVisualStyleBackColor = true;
            BtnMenu.Click += BtnMenu_Click;
            // 
            // BtnHome
            // 
            BtnHome.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            BtnHome.BackgroundImageLayout = ImageLayout.Zoom;
            BtnHome.FlatAppearance.BorderSize = 0;
            BtnHome.FlatStyle = FlatStyle.Flat;
            BtnHome.ForeColor = System.Drawing.Color.White;
            BtnHome.IconChar = FontAwesome.Sharp.IconChar.House;
            BtnHome.IconColor = System.Drawing.Color.Black;
            BtnHome.IconFont = FontAwesome.Sharp.IconFont.Auto;
            BtnHome.IconSize = 30;
            BtnHome.Location = new System.Drawing.Point(832, 17);
            BtnHome.Margin = new Padding(3, 4, 3, 4);
            BtnHome.Name = "BtnHome";
            BtnHome.Size = new System.Drawing.Size(36, 30);
            BtnHome.TabIndex = 6;
            BtnHome.Tag = "Home";
            BtnHome.UseVisualStyleBackColor = true;
            BtnHome.Click += BtnHome_Click;
            // 
            // TabPages
            // 
            TabPages.ContextMenuStrip = menuStripTab;
            TabPages.Dock = DockStyle.Fill;
            TabPages.Font = new System.Drawing.Font("Segoe UI", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            TabPages.Items.AddRange(new Controls.BrowserTabStrip.BrowserTabStripItem[] { tabStrip1, tabStripAdd });
            TabPages.Location = new System.Drawing.Point(0, 65);
            TabPages.Name = "TabPages";
            TabPages.Padding = new Padding(1, 49, 1, 1);
            TabPages.SelectedItem = tabStrip1;
            TabPages.Size = new System.Drawing.Size(916, 407);
            TabPages.TabIndex = 4;
            TabPages.Text = "faTabStrip1";
            TabPages.TabStripItemSelectionChanged += OnTabsChanged;
            TabPages.TabStripItemClosed += OnTabClosed;
            TabPages.MouseClick += tabPages_MouseClick;
            // 
            // tabStrip1
            // 
            tabStrip1.Dock = DockStyle.Fill;
            tabStrip1.IsDrawn = true;
            tabStrip1.Location = new System.Drawing.Point(1, 49);
            tabStrip1.Name = "tabStrip1";
            tabStrip1.Selected = true;
            tabStrip1.Size = new System.Drawing.Size(914, 357);
            tabStrip1.TabIndex = 0;
            tabStrip1.Title = "Loading...";
            // 
            // tabStripAdd
            // 
            tabStripAdd.CanClose = false;
            tabStripAdd.Dock = DockStyle.Fill;
            tabStripAdd.IsDrawn = true;
            tabStripAdd.Location = new System.Drawing.Point(0, 0);
            tabStripAdd.Name = "tabStripAdd";
            tabStripAdd.Size = new System.Drawing.Size(931, 601);
            tabStripAdd.TabIndex = 1;
            tabStripAdd.Title = "+";
            // 
            // PanelSearch
            // 
            PanelSearch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            PanelSearch.BackColor = System.Drawing.Color.White;
            PanelSearch.BorderStyle = BorderStyle.FixedSingle;
            PanelSearch.Controls.Add(BtnNextSearch);
            PanelSearch.Controls.Add(BtnPrevSearch);
            PanelSearch.Controls.Add(BtnCloseSearch);
            PanelSearch.Controls.Add(TxtSearch);
            PanelSearch.Location = new System.Drawing.Point(592, 115);
            PanelSearch.Name = "PanelSearch";
            PanelSearch.Size = new System.Drawing.Size(307, 49);
            PanelSearch.TabIndex = 9;
            PanelSearch.Visible = false;
            // 
            // BtnNextSearch
            // 
            BtnNextSearch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            BtnNextSearch.FlatStyle = FlatStyle.Flat;
            BtnNextSearch.ForeColor = System.Drawing.Color.White;
            BtnNextSearch.Image = (System.Drawing.Image)resources.GetObject("BtnNextSearch.Image");
            BtnNextSearch.Location = new System.Drawing.Point(239, 8);
            BtnNextSearch.Margin = new Padding(3, 4, 3, 4);
            BtnNextSearch.Name = "BtnNextSearch";
            BtnNextSearch.Size = new System.Drawing.Size(25, 30);
            BtnNextSearch.TabIndex = 9;
            BtnNextSearch.Tag = "Find next (Enter)";
            BtnNextSearch.UseVisualStyleBackColor = true;
            BtnNextSearch.Click += BtnNextSearch_Click;
            // 
            // BtnPrevSearch
            // 
            BtnPrevSearch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            BtnPrevSearch.FlatStyle = FlatStyle.Flat;
            BtnPrevSearch.ForeColor = System.Drawing.Color.White;
            BtnPrevSearch.Image = (System.Drawing.Image)resources.GetObject("BtnPrevSearch.Image");
            BtnPrevSearch.Location = new System.Drawing.Point(206, 8);
            BtnPrevSearch.Margin = new Padding(3, 4, 3, 4);
            BtnPrevSearch.Name = "BtnPrevSearch";
            BtnPrevSearch.Size = new System.Drawing.Size(25, 30);
            BtnPrevSearch.TabIndex = 8;
            BtnPrevSearch.Tag = "Find previous (Shift+Enter)";
            BtnPrevSearch.UseVisualStyleBackColor = true;
            BtnPrevSearch.Click += BtnPrevSearch_Click;
            // 
            // BtnCloseSearch
            // 
            BtnCloseSearch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            BtnCloseSearch.FlatStyle = FlatStyle.Flat;
            BtnCloseSearch.ForeColor = System.Drawing.Color.White;
            BtnCloseSearch.Image = (System.Drawing.Image)resources.GetObject("BtnCloseSearch.Image");
            BtnCloseSearch.Location = new System.Drawing.Point(272, 8);
            BtnCloseSearch.Margin = new Padding(3, 4, 3, 4);
            BtnCloseSearch.Name = "BtnCloseSearch";
            BtnCloseSearch.Size = new System.Drawing.Size(25, 30);
            BtnCloseSearch.TabIndex = 7;
            BtnCloseSearch.Tag = "Close (Esc)";
            BtnCloseSearch.UseVisualStyleBackColor = true;
            BtnCloseSearch.Click += BtnClearSearch_Click;
            // 
            // TxtSearch
            // 
            TxtSearch.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            TxtSearch.BorderStyle = BorderStyle.None;
            TxtSearch.Font = new System.Drawing.Font("Segoe UI", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            TxtSearch.Location = new System.Drawing.Point(9, 8);
            TxtSearch.Margin = new Padding(3, 4, 3, 4);
            TxtSearch.Name = "TxtSearch";
            TxtSearch.Size = new System.Drawing.Size(181, 31);
            TxtSearch.TabIndex = 6;
            TxtSearch.TextChanged += TxtSearch_TextChanged;
            TxtSearch.KeyDown += TxtSearch_KeyDown;
            // 
            // PanelToolbar
            // 
            PanelToolbar.Controls.Add(lbl_ZoomLevel);
            PanelToolbar.Controls.Add(BtnBack);
            PanelToolbar.Controls.Add(BtnForward);
            PanelToolbar.Controls.Add(BtnRefresh);
            PanelToolbar.Controls.Add(TxtURL);
            PanelToolbar.Controls.Add(BtnDownloads);
            PanelToolbar.Controls.Add(BtnHome);
            PanelToolbar.Controls.Add(BtnMenu);
            PanelToolbar.Controls.Add(BtnStop);
            PanelToolbar.Dock = DockStyle.Top;
            PanelToolbar.lay_ChildAxisAlignment = SharpBrowser.Controls.StackChildAxisAlignment.Center;
            PanelToolbar.lay_Orientation = SharpBrowser.Controls.StackOrientation.Horizontal;
            PanelToolbar.lay_PerformLayout_calcMethod_No = 4;
            PanelToolbar.lay_Spacing = 12;
            PanelToolbar.Location = new System.Drawing.Point(0, 0);
            PanelToolbar.Name = "PanelToolbar";
            PanelToolbar.Size = new System.Drawing.Size(916, 65);
            PanelToolbar.TabIndex = 10;
            // 
            // MainForm
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new System.Drawing.Size(916, 472);
            Controls.Add(PanelSearch);
            Controls.Add(TabPages);
            Controls.Add(PanelToolbar);
            Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            Margin = new Padding(4, 5, 4, 5);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Title";
            WindowState = FormWindowState.Maximized;
            FormClosing += MainForm_FormClosing;
            Load += MainForm_Load;
            menuStripTab.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)TabPages).EndInit();
            TabPages.ResumeLayout(false);
            PanelSearch.ResumeLayout(false);
            PanelSearch.PerformLayout();
            PanelToolbar.ResumeLayout(false);
            PanelToolbar.PerformLayout();
            ResumeLayout(false);
        }


        #endregion

        private SharpBrowser.Controls.BrowserTabStrip.BrowserTabStrip TabPages;
        private SharpBrowser.Controls.BrowserTabStrip.BrowserTabStripItem tabStrip1;
        private SharpBrowser.Controls.BrowserTabStrip.BrowserTabStripItem tabStripAdd;
		private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ContextMenuStrip menuStripTab;
        private System.Windows.Forms.ToolStripMenuItem menuCloseTab;
        private System.Windows.Forms.ToolStripMenuItem menuCloseOtherTabs;
		private FontAwesome.Sharp.IconButton BtnForward;
		private FontAwesome.Sharp.IconButton BtnBack;
		private FontAwesome.Sharp.IconButton BtnStop;
		private FontAwesome.Sharp.IconButton BtnRefresh;
		private FontAwesome.Sharp.IconButton BtnDownloads;
		private System.Windows.Forms.TextBox TxtURL;
		private System.Windows.Forms.Panel PanelSearch;
		private System.Windows.Forms.TextBox TxtSearch;
		private System.Windows.Forms.Button BtnCloseSearch;
		private System.Windows.Forms.Button BtnPrevSearch;
		private System.Windows.Forms.Button BtnNextSearch;
        private FontAwesome.Sharp.IconButton BtnHome;
        private FontAwesome.Sharp.IconButton BtnMenu;
        private Label lbl_ZoomLevel;
        private Controls.StackLayout PanelToolbar;
    }
}

