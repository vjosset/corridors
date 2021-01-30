using System.Windows.Forms;

namespace Corridors
{
    partial class frmMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.mnuMain = new System.Windows.Forms.MenuStrip();
            this.mnuMainFile = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMainFileNew = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMainFileOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMainFileSaveAs = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMainFileSaveMap = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuMainFilePreferences = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuMainFileExit = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMainEdit = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMainEditSelectAll = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuMainEditCopy = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuMainEditRotateCW = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMainEditRotateCCW = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMainEditMirrorVertical = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMainEditMirrorHorizontal = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuMainEditUndo = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMainEditRedo = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.lsvModules = new System.Windows.Forms.ListView();
            this.Canvas = new Corridors.DBPanel();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.prg = new System.Windows.Forms.ToolStripProgressBar();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnModeDraw = new System.Windows.Forms.ToolStripButton();
            this.btnModeEraser = new System.Windows.Forms.ToolStripButton();
            this.btnModeSelect = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.btnClear = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.btnZoomOut = new System.Windows.Forms.ToolStripButton();
            this.btnZoomIn = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.btnRotateCW = new System.Windows.Forms.ToolStripButton();
            this.btnRotateCCW = new System.Windows.Forms.ToolStripButton();
            this.btnMirrorVertical = new System.Windows.Forms.ToolStripButton();
            this.btnMirrorHorizontal = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnDelete = new System.Windows.Forms.ToolStripButton();
            this.btnViewLog = new System.Windows.Forms.ToolStripButton();
            this.ofd = new System.Windows.Forms.OpenFileDialog();
            this.sfd = new System.Windows.Forms.SaveFileDialog();
            this.mnuMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // mnuMain
            // 
            this.mnuMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuMainFile,
            this.mnuMainEdit});
            this.mnuMain.Location = new System.Drawing.Point(0, 0);
            this.mnuMain.Name = "mnuMain";
            this.mnuMain.Size = new System.Drawing.Size(1245, 24);
            this.mnuMain.TabIndex = 5;
            this.mnuMain.Text = "Main Menu";
            // 
            // mnuMainFile
            // 
            this.mnuMainFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuMainFileNew,
            this.mnuMainFileOpen,
            this.mnuMainFileSaveAs,
            this.mnuMainFileSaveMap,
            this.toolStripMenuItem4,
            this.mnuMainFilePreferences,
            this.toolStripMenuItem1,
            this.mnuMainFileExit});
            this.mnuMainFile.Name = "mnuMainFile";
            this.mnuMainFile.Size = new System.Drawing.Size(37, 20);
            this.mnuMainFile.Text = "File";
            // 
            // mnuMainFileNew
            // 
            this.mnuMainFileNew.Image = global::Corridors.Properties.Resources.NewFile_16x;
            this.mnuMainFileNew.Name = "mnuMainFileNew";
            this.mnuMainFileNew.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.mnuMainFileNew.Size = new System.Drawing.Size(195, 22);
            this.mnuMainFileNew.Text = "New";
            this.mnuMainFileNew.Click += new System.EventHandler(this.mnuMainFileNew_Click);
            // 
            // mnuMainFileOpen
            // 
            this.mnuMainFileOpen.Image = global::Corridors.Properties.Resources.OpenFolder_16x;
            this.mnuMainFileOpen.Name = "mnuMainFileOpen";
            this.mnuMainFileOpen.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.mnuMainFileOpen.Size = new System.Drawing.Size(195, 22);
            this.mnuMainFileOpen.Text = "Open...";
            this.mnuMainFileOpen.Click += new System.EventHandler(this.mnuMainFileOpen_Click);
            // 
            // mnuMainFileSaveAs
            // 
            this.mnuMainFileSaveAs.Image = global::Corridors.Properties.Resources.Save_16x;
            this.mnuMainFileSaveAs.Name = "mnuMainFileSaveAs";
            this.mnuMainFileSaveAs.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.S)));
            this.mnuMainFileSaveAs.Size = new System.Drawing.Size(195, 22);
            this.mnuMainFileSaveAs.Text = "Save As...";
            this.mnuMainFileSaveAs.Click += new System.EventHandler(this.mnuMainFileSaveAs_Click);
            // 
            // mnuMainFileSaveMap
            // 
            this.mnuMainFileSaveMap.Image = global::Corridors.Properties.Resources.Save_16x;
            this.mnuMainFileSaveMap.Name = "mnuMainFileSaveMap";
            this.mnuMainFileSaveMap.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.mnuMainFileSaveMap.Size = new System.Drawing.Size(195, 22);
            this.mnuMainFileSaveMap.Text = "Save";
            this.mnuMainFileSaveMap.Click += new System.EventHandler(this.mnuMainFileSaveMap_Click);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(192, 6);
            // 
            // mnuMainFilePreferences
            // 
            this.mnuMainFilePreferences.Image = global::Corridors.Properties.Resources.Settings_16x;
            this.mnuMainFilePreferences.Name = "mnuMainFilePreferences";
            this.mnuMainFilePreferences.Size = new System.Drawing.Size(195, 22);
            this.mnuMainFilePreferences.Text = "Preferences";
            this.mnuMainFilePreferences.Click += new System.EventHandler(this.mnuMainFilePreferences_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(192, 6);
            // 
            // mnuMainFileExit
            // 
            this.mnuMainFileExit.Name = "mnuMainFileExit";
            this.mnuMainFileExit.Size = new System.Drawing.Size(195, 22);
            this.mnuMainFileExit.Text = "Exit";
            // 
            // mnuMainEdit
            // 
            this.mnuMainEdit.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuMainEditSelectAll,
            this.toolStripMenuItem3,
            this.mnuMainEditCopy,
            this.toolStripMenuItem2,
            this.mnuMainEditRotateCW,
            this.mnuMainEditRotateCCW,
            this.mnuMainEditMirrorVertical,
            this.mnuMainEditMirrorHorizontal,
            this.toolStripMenuItem5,
            this.mnuMainEditUndo,
            this.mnuMainEditRedo});
            this.mnuMainEdit.Name = "mnuMainEdit";
            this.mnuMainEdit.Size = new System.Drawing.Size(39, 20);
            this.mnuMainEdit.Text = "Edit";
            // 
            // mnuMainEditSelectAll
            // 
            this.mnuMainEditSelectAll.Image = global::Corridors.Properties.Resources.SelectAll_16x;
            this.mnuMainEditSelectAll.Name = "mnuMainEditSelectAll";
            this.mnuMainEditSelectAll.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A)));
            this.mnuMainEditSelectAll.Size = new System.Drawing.Size(283, 22);
            this.mnuMainEditSelectAll.Text = "Select All";
            this.mnuMainEditSelectAll.Click += new System.EventHandler(this.mnuMainEditSelectAll_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(280, 6);
            // 
            // mnuMainEditCopy
            // 
            this.mnuMainEditCopy.Image = global::Corridors.Properties.Resources.Copy_16x;
            this.mnuMainEditCopy.Name = "mnuMainEditCopy";
            this.mnuMainEditCopy.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.mnuMainEditCopy.Size = new System.Drawing.Size(283, 22);
            this.mnuMainEditCopy.Text = "Copy";
            this.mnuMainEditCopy.Click += new System.EventHandler(this.mnuMainEditCopy_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(280, 6);
            // 
            // mnuMainEditRotateCW
            // 
            this.mnuMainEditRotateCW.Image = global::Corridors.Properties.Resources.RotateRight_16x;
            this.mnuMainEditRotateCW.Name = "mnuMainEditRotateCW";
            this.mnuMainEditRotateCW.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.R)));
            this.mnuMainEditRotateCW.Size = new System.Drawing.Size(283, 22);
            this.mnuMainEditRotateCW.Text = "Rotate Clockwise";
            this.mnuMainEditRotateCW.Click += new System.EventHandler(this.mnuMainEditRotateCW_Click);
            // 
            // mnuMainEditRotateCCW
            // 
            this.mnuMainEditRotateCCW.Image = global::Corridors.Properties.Resources.RotateLeft_16x;
            this.mnuMainEditRotateCCW.Name = "mnuMainEditRotateCCW";
            this.mnuMainEditRotateCCW.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.R)));
            this.mnuMainEditRotateCCW.Size = new System.Drawing.Size(283, 22);
            this.mnuMainEditRotateCCW.Text = "Rotate Counter Clockwise";
            this.mnuMainEditRotateCCW.Click += new System.EventHandler(this.mnuMainEditRotateCCW_Click);
            // 
            // mnuMainEditMirrorVertical
            // 
            this.mnuMainEditMirrorVertical.Image = global::Corridors.Properties.Resources.FlipVertical_16x;
            this.mnuMainEditMirrorVertical.Name = "mnuMainEditMirrorVertical";
            this.mnuMainEditMirrorVertical.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.M)));
            this.mnuMainEditMirrorVertical.Size = new System.Drawing.Size(283, 22);
            this.mnuMainEditMirrorVertical.Text = "Mirror - Vertical";
            this.mnuMainEditMirrorVertical.Click += new System.EventHandler(this.mnuMainEditMirrorVertical_Click);
            // 
            // mnuMainEditMirrorHorizontal
            // 
            this.mnuMainEditMirrorHorizontal.Image = global::Corridors.Properties.Resources.FlipHorizontal_16x;
            this.mnuMainEditMirrorHorizontal.Name = "mnuMainEditMirrorHorizontal";
            this.mnuMainEditMirrorHorizontal.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.M)));
            this.mnuMainEditMirrorHorizontal.Size = new System.Drawing.Size(283, 22);
            this.mnuMainEditMirrorHorizontal.Text = "Mirror - Horizontal";
            this.mnuMainEditMirrorHorizontal.Click += new System.EventHandler(this.mnuMainEditMirrorHorizontal_Click);
            // 
            // toolStripMenuItem5
            // 
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            this.toolStripMenuItem5.Size = new System.Drawing.Size(280, 6);
            // 
            // mnuMainEditUndo
            // 
            this.mnuMainEditUndo.Image = global::Corridors.Properties.Resources.Undo_16x;
            this.mnuMainEditUndo.Name = "mnuMainEditUndo";
            this.mnuMainEditUndo.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z)));
            this.mnuMainEditUndo.Size = new System.Drawing.Size(283, 22);
            this.mnuMainEditUndo.Text = "Undo";
            // 
            // mnuMainEditRedo
            // 
            this.mnuMainEditRedo.Image = global::Corridors.Properties.Resources.Redo_16x;
            this.mnuMainEditRedo.Name = "mnuMainEditRedo";
            this.mnuMainEditRedo.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.Z)));
            this.mnuMainEditRedo.Size = new System.Drawing.Size(283, 22);
            this.mnuMainEditRedo.Text = "Redo";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(0, 52);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.txtSearch);
            this.splitContainer1.Panel1.Controls.Add(this.lsvModules);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.Canvas);
            this.splitContainer1.Size = new System.Drawing.Size(1245, 663);
            this.splitContainer1.SplitterDistance = 335;
            this.splitContainer1.TabIndex = 2;
            // 
            // txtSearch
            // 
            this.txtSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSearch.Location = new System.Drawing.Point(0, 3);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(335, 20);
            this.txtSearch.TabIndex = 1;
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
            // 
            // lsvModules
            // 
            this.lsvModules.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lsvModules.BackColor = System.Drawing.SystemColors.Window;
            this.lsvModules.FullRowSelect = true;
            this.lsvModules.GridLines = true;
            this.lsvModules.HideSelection = false;
            this.lsvModules.Location = new System.Drawing.Point(0, 29);
            this.lsvModules.MultiSelect = false;
            this.lsvModules.Name = "lsvModules";
            this.lsvModules.Size = new System.Drawing.Size(335, 634);
            this.lsvModules.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.lsvModules.TabIndex = 0;
            this.lsvModules.UseCompatibleStateImageBehavior = false;
            this.lsvModules.SelectedIndexChanged += new System.EventHandler(this.lsvModules_SelectedIndexChanged);
            // 
            // Canvas
            // 
            this.Canvas.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Canvas.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Canvas.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Canvas.Location = new System.Drawing.Point(0, 0);
            this.Canvas.Name = "Canvas";
            this.Canvas.Size = new System.Drawing.Size(906, 663);
            this.Canvas.TabIndex = 1;
            this.Canvas.Paint += new System.Windows.Forms.PaintEventHandler(this.Canvas_Paint);
            this.Canvas.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Canvas_MouseDown);
            this.Canvas.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Canvas_MouseMove);
            this.Canvas.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Canvas_MouseUp);
            this.Canvas.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.Canvas_MouseWheel);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.prg,
            this.lblStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 718);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1245, 22);
            this.statusStrip1.TabIndex = 6;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // prg
            // 
            this.prg.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.prg.Name = "prg";
            this.prg.Size = new System.Drawing.Size(100, 16);
            this.prg.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            // 
            // lblStatus
            // 
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(39, 17);
            this.lblStatus.Text = "Ready";
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnModeDraw,
            this.btnModeEraser,
            this.btnModeSelect,
            this.toolStripSeparator4,
            this.btnClear,
            this.toolStripSeparator3,
            this.btnZoomOut,
            this.btnZoomIn,
            this.toolStripSeparator2,
            this.btnRotateCW,
            this.btnRotateCCW,
            this.btnMirrorVertical,
            this.btnMirrorHorizontal,
            this.toolStripSeparator1,
            this.btnDelete,
            this.btnViewLog});
            this.toolStrip1.Location = new System.Drawing.Point(0, 24);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1245, 25);
            this.toolStrip1.TabIndex = 7;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // btnModeDraw
            // 
            this.btnModeDraw.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnModeDraw.Image = global::Corridors.Properties.Resources.Pen4_16x;
            this.btnModeDraw.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnModeDraw.Name = "btnModeDraw";
            this.btnModeDraw.Size = new System.Drawing.Size(23, 22);
            this.btnModeDraw.Text = "Draw";
            this.btnModeDraw.Click += new System.EventHandler(this.btnModeDraw_Click);
            // 
            // btnModeEraser
            // 
            this.btnModeEraser.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnModeEraser.Image = global::Corridors.Properties.Resources.Eraser_16x;
            this.btnModeEraser.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnModeEraser.Name = "btnModeEraser";
            this.btnModeEraser.Size = new System.Drawing.Size(23, 22);
            this.btnModeEraser.Text = "Eraser";
            this.btnModeEraser.Click += new System.EventHandler(this.btnModeEraser_Click);
            // 
            // btnModeSelect
            // 
            this.btnModeSelect.Checked = true;
            this.btnModeSelect.CheckState = System.Windows.Forms.CheckState.Checked;
            this.btnModeSelect.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnModeSelect.Image = global::Corridors.Properties.Resources.Select_16x;
            this.btnModeSelect.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnModeSelect.Name = "btnModeSelect";
            this.btnModeSelect.Size = new System.Drawing.Size(23, 22);
            this.btnModeSelect.Text = "Select";
            this.btnModeSelect.Click += new System.EventHandler(this.btnModeSelect_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
            // 
            // btnClear
            // 
            this.btnClear.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnClear.Image = global::Corridors.Properties.Resources.ClearCollection_16x;
            this.btnClear.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(23, 22);
            this.btnClear.Text = "Clear All";
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // btnZoomOut
            // 
            this.btnZoomOut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnZoomOut.Image = global::Corridors.Properties.Resources.ZoomOut_16x;
            this.btnZoomOut.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnZoomOut.Name = "btnZoomOut";
            this.btnZoomOut.Size = new System.Drawing.Size(23, 22);
            this.btnZoomOut.Text = "Zoom Out";
            this.btnZoomOut.Click += new System.EventHandler(this.btnZoomOut_Click);
            // 
            // btnZoomIn
            // 
            this.btnZoomIn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnZoomIn.Image = global::Corridors.Properties.Resources.ZoomIn_16x;
            this.btnZoomIn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnZoomIn.Name = "btnZoomIn";
            this.btnZoomIn.Size = new System.Drawing.Size(23, 22);
            this.btnZoomIn.Text = "Zoom In";
            this.btnZoomIn.Click += new System.EventHandler(this.btnZoomIn_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // btnRotateCW
            // 
            this.btnRotateCW.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnRotateCW.Image = global::Corridors.Properties.Resources.RotateRight_16x;
            this.btnRotateCW.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnRotateCW.Name = "btnRotateCW";
            this.btnRotateCW.Size = new System.Drawing.Size(23, 22);
            this.btnRotateCW.Text = "Rotate Clockwise";
            this.btnRotateCW.Click += new System.EventHandler(this.btnRotateCW_Click);
            // 
            // btnRotateCCW
            // 
            this.btnRotateCCW.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnRotateCCW.Image = global::Corridors.Properties.Resources.RotateLeft_16x;
            this.btnRotateCCW.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnRotateCCW.Name = "btnRotateCCW";
            this.btnRotateCCW.Size = new System.Drawing.Size(23, 22);
            this.btnRotateCCW.Text = "Rotate Counter-Clockwise";
            this.btnRotateCCW.Click += new System.EventHandler(this.btnRotateCCW_Click);
            // 
            // btnMirrorVertical
            // 
            this.btnMirrorVertical.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnMirrorVertical.Image = global::Corridors.Properties.Resources.FlipVertical_16x;
            this.btnMirrorVertical.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnMirrorVertical.Name = "btnMirrorVertical";
            this.btnMirrorVertical.Size = new System.Drawing.Size(23, 22);
            this.btnMirrorVertical.Text = "Mirror - Vertical";
            this.btnMirrorVertical.Click += new System.EventHandler(this.btnMirrorVertical_Click);
            // 
            // btnMirrorHorizontal
            // 
            this.btnMirrorHorizontal.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnMirrorHorizontal.Image = global::Corridors.Properties.Resources.FlipHorizontal_16x;
            this.btnMirrorHorizontal.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnMirrorHorizontal.Name = "btnMirrorHorizontal";
            this.btnMirrorHorizontal.Size = new System.Drawing.Size(23, 22);
            this.btnMirrorHorizontal.Text = "Mirror - Horizontal";
            this.btnMirrorHorizontal.Click += new System.EventHandler(this.btnMirrorHorizontal_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // btnDelete
            // 
            this.btnDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnDelete.Image = global::Corridors.Properties.Resources.Close_red_16x;
            this.btnDelete.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(23, 22);
            this.btnDelete.Text = "Delete Selected";
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnViewLog
            // 
            this.btnViewLog.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnViewLog.Image = ((System.Drawing.Image)(resources.GetObject("btnViewLog.Image")));
            this.btnViewLog.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnViewLog.Name = "btnViewLog";
            this.btnViewLog.Size = new System.Drawing.Size(59, 22);
            this.btnViewLog.Text = "View Log";
            this.btnViewLog.Click += new System.EventHandler(this.btnViewLog_Click);
            // 
            // ofd
            // 
            this.ofd.DefaultExt = "map";
            this.ofd.Filter = "Map files|*.map|All files|*.*";
            // 
            // sfd
            // 
            this.sfd.DefaultExt = "map";
            this.sfd.Filter = "Map files|*.map|All files|*.*";
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1245, 740);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.mnuMain);
            this.DoubleBuffered = true;
            this.KeyPreview = true;
            this.MainMenuStrip = this.mnuMain;
            this.Name = "frmMain";
            this.Text = "Corridors";
            this.Shown += new System.EventHandler(this.frmMain_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.frmMain_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.frmMain_KeyUp);
            this.mnuMain.ResumeLayout(false);
            this.mnuMain.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private MenuStrip mnuMain;
        private ToolStripMenuItem mnuMainFile;
        private SplitContainer splitContainer1;
        private ListView lsvModules;
        private DBPanel Canvas;
        private StatusStrip statusStrip1;
        private ToolStripProgressBar prg;
        private ToolStripStatusLabel lblStatus;
        private ToolStrip toolStrip1;
        private ToolStripButton btnClear;
        private ToolStripButton btnZoomIn;
        private ToolStripButton btnZoomOut;
        private ToolStripMenuItem mnuMainFilePreferences;
        private ToolStripSeparator toolStripMenuItem1;
        private ToolStripMenuItem mnuMainFileExit;
        private ToolStripMenuItem mnuMainEdit;
        private ToolStripMenuItem mnuMainEditCopy;
        private ToolStripButton btnRotateCW;
        private ToolStripButton btnRotateCCW;
        private ToolStripMenuItem mnuMainEditSelectAll;
        private ToolStripSeparator toolStripMenuItem3;
        private ToolStripSeparator toolStripMenuItem2;
        private ToolStripMenuItem mnuMainEditUndo;
        private ToolStripMenuItem mnuMainEditRedo;
        private ToolStripMenuItem mnuMainFileOpen;
        private ToolStripMenuItem mnuMainFileSaveMap;
        private ToolStripSeparator toolStripMenuItem4;
        private OpenFileDialog ofd;
        private SaveFileDialog sfd;
        private ToolStripButton btnDelete;
        private ToolStripMenuItem mnuMainFileSaveAs;
        private ToolStripMenuItem mnuMainFileNew;
        private ToolStripMenuItem mnuMainEditRotateCW;
        private ToolStripMenuItem mnuMainEditRotateCCW;
        private ToolStripMenuItem mnuMainEditMirrorVertical;
        private ToolStripMenuItem mnuMainEditMirrorHorizontal;
        private ToolStripSeparator toolStripMenuItem5;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripButton btnMirrorVertical;
        private ToolStripButton btnMirrorHorizontal;
        private ToolStripSeparator toolStripSeparator1;
        private TextBox txtSearch;
        private ToolStripButton btnModeDraw;
        private ToolStripButton btnModeEraser;
        private ToolStripButton btnModeSelect;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripButton btnViewLog;
    }
}

