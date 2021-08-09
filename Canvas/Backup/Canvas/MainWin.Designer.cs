namespace Canvas
{
	partial class MainWin
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
			this.BottomToolStripPanel = new System.Windows.Forms.ToolStripPanel();
			this.TopToolStripPanel = new System.Windows.Forms.ToolStripPanel();
			this.RightToolStripPanel = new System.Windows.Forms.ToolStripPanel();
			this.LeftToolStripPanel = new System.Windows.Forms.ToolStripPanel();
			this.m_mainMenu = new System.Windows.Forms.MenuStrip();
			this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.optionsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			this.layersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.m_windowMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.m_helpMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.m_aboutMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.m_mainMenu.SuspendLayout();
			this.SuspendLayout();
			// 
			// BottomToolStripPanel
			// 
			this.BottomToolStripPanel.Location = new System.Drawing.Point(0, 0);
			this.BottomToolStripPanel.Name = "BottomToolStripPanel";
			this.BottomToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
			this.BottomToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
			this.BottomToolStripPanel.Size = new System.Drawing.Size(0, 0);
			// 
			// TopToolStripPanel
			// 
			this.TopToolStripPanel.Location = new System.Drawing.Point(0, 0);
			this.TopToolStripPanel.Name = "TopToolStripPanel";
			this.TopToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
			this.TopToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
			this.TopToolStripPanel.Size = new System.Drawing.Size(0, 0);
			// 
			// RightToolStripPanel
			// 
			this.RightToolStripPanel.Location = new System.Drawing.Point(0, 0);
			this.RightToolStripPanel.Name = "RightToolStripPanel";
			this.RightToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
			this.RightToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
			this.RightToolStripPanel.Size = new System.Drawing.Size(0, 0);
			// 
			// LeftToolStripPanel
			// 
			this.LeftToolStripPanel.Location = new System.Drawing.Point(0, 0);
			this.LeftToolStripPanel.Name = "LeftToolStripPanel";
			this.LeftToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
			this.LeftToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
			this.LeftToolStripPanel.Size = new System.Drawing.Size(0, 0);
			// 
			// m_mainMenu
			// 
			this.m_mainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.optionsToolStripMenuItem,
            this.m_windowMenu,
            this.m_helpMenu});
			this.m_mainMenu.Location = new System.Drawing.Point(0, 0);
			this.m_mainMenu.MdiWindowListItem = this.m_windowMenu;
			this.m_mainMenu.Name = "m_mainMenu";
			this.m_mainMenu.Size = new System.Drawing.Size(803, 24);
			this.m_mainMenu.TabIndex = 0;
			// 
			// optionsToolStripMenuItem
			// 
			this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.optionsToolStripMenuItem1,
            this.layersToolStripMenuItem});
			this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
			this.optionsToolStripMenuItem.Size = new System.Drawing.Size(56, 20);
			this.optionsToolStripMenuItem.Text = "&Options";
			// 
			// optionsToolStripMenuItem1
			// 
			this.optionsToolStripMenuItem1.Name = "optionsToolStripMenuItem1";
			this.optionsToolStripMenuItem1.Size = new System.Drawing.Size(152, 22);
			this.optionsToolStripMenuItem1.Tag = "Grid";
			this.optionsToolStripMenuItem1.Text = "&Grid";
			this.optionsToolStripMenuItem1.Click += new System.EventHandler(this.OnOptions);
			// 
			// layersToolStripMenuItem
			// 
			this.layersToolStripMenuItem.Name = "layersToolStripMenuItem";
			this.layersToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
			this.layersToolStripMenuItem.Tag = "Layers";
			this.layersToolStripMenuItem.Text = "Layers";
			this.layersToolStripMenuItem.Click += new System.EventHandler(this.OnOptions);
			// 
			// m_windowMenu
			// 
			this.m_windowMenu.Name = "m_windowMenu";
			this.m_windowMenu.Size = new System.Drawing.Size(57, 20);
			this.m_windowMenu.Text = "&Window";
			this.m_windowMenu.DropDownOpening += new System.EventHandler(this.OnUpdateMenuUI);
			// 
			// m_helpMenu
			// 
			this.m_helpMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_aboutMenuItem});
			this.m_helpMenu.Name = "m_helpMenu";
			this.m_helpMenu.Size = new System.Drawing.Size(40, 20);
			this.m_helpMenu.Text = "&Help";
			// 
			// m_aboutMenuItem
			// 
			this.m_aboutMenuItem.Name = "m_aboutMenuItem";
			this.m_aboutMenuItem.Size = new System.Drawing.Size(114, 22);
			this.m_aboutMenuItem.Text = "&About";
			this.m_aboutMenuItem.Click += new System.EventHandler(this.OnAbout);
			// 
			// MainWin
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(803, 509);
			this.Controls.Add(this.m_mainMenu);
			this.IsMdiContainer = true;
			this.MainMenuStrip = this.m_mainMenu;
			this.Name = "MainWin";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "MainWin";
			this.m_mainMenu.ResumeLayout(false);
			this.m_mainMenu.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.MenuStrip m_mainMenu;
		private System.Windows.Forms.ToolStripMenuItem m_windowMenu;
		private System.Windows.Forms.ToolStripPanel BottomToolStripPanel;
		private System.Windows.Forms.ToolStripPanel TopToolStripPanel;
		private System.Windows.Forms.ToolStripPanel RightToolStripPanel;
		private System.Windows.Forms.ToolStripPanel LeftToolStripPanel;
		private System.Windows.Forms.ToolStripMenuItem m_helpMenu;
		private System.Windows.Forms.ToolStripMenuItem m_aboutMenuItem;
		private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem layersToolStripMenuItem;


	}
}