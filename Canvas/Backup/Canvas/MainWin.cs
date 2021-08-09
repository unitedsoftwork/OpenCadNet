using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

namespace Canvas
{
	public partial class MainWin : Form
	{
		MenuItemManager m_menuItems;
		public MainWin()
		{
			UnitPoint p = HitUtil.CenterPointFrom3Points(new UnitPoint(0,2), new UnitPoint(1.4142136f, 1.4142136f), new UnitPoint(2,0));

			InitializeComponent();
			Text = Program.AppName;
			string[] args = Environment.GetCommandLineArgs();
			if (args.Length == 2) // assume it points to a file
				OpenDocument(args[1]);
			else
				OpenDocument(string.Empty);
			
			m_menuItems = new MenuItemManager(this);
			m_menuItems.SetupStripPanels();
			SetupToolbars();
			
			Application.Idle += new EventHandler(OnIdle);
		}
		void SetupToolbars()
		{
			MenuItem mmitem = m_menuItems.GetItem("New");
			mmitem.Text = "&New";
			mmitem.Image = MenuImages16x16.Image(MenuImages16x16.eIndexes.NewDocument);
			mmitem.Click += new EventHandler(OnFileNew);
			mmitem.ToolTipText = "New document";

			mmitem = m_menuItems.GetItem("Open");
			mmitem.Text = "&Open";
			mmitem.Image = MenuImages16x16.Image(MenuImages16x16.eIndexes.OpenDocument);
			mmitem.Click += new EventHandler(OnFileOpen);
			mmitem.ToolTipText = "Open document";

			mmitem = m_menuItems.GetItem("Save");
			mmitem.Text = "&Save";
			mmitem.Image = MenuImages16x16.Image(MenuImages16x16.eIndexes.SaveDocument);
			mmitem.Click += new EventHandler(OnFileSave);
			mmitem.ToolTipText = "Save document";

			mmitem = m_menuItems.GetItem("SaveAs");
			mmitem.Text = "Save &As";
			mmitem.Click += new EventHandler(OnFileSaveAs);

			mmitem = m_menuItems.GetItem("Exit");
			mmitem.Text = "E&xit";
			mmitem.Click += new EventHandler(OnFileExit);

			ToolStrip strip = m_menuItems.GetStrip("file");
			strip.Items.Add(m_menuItems.GetItem("New").CreateButton());
			strip.Items.Add(m_menuItems.GetItem("Open").CreateButton());
			strip.Items.Add(m_menuItems.GetItem("Save").CreateButton());

			ToolStripMenuItem menuitem = m_menuItems.GetMenuStrip("file");
			menuitem.Text = "&File";
			menuitem.DropDownItems.Add(m_menuItems.GetItem("New").CreateMenuItem());
			menuitem.DropDownItems.Add(m_menuItems.GetItem("Open").CreateMenuItem());
			menuitem.DropDownItems.Add(m_menuItems.GetItem("Save").CreateMenuItem());
			menuitem.DropDownItems.Add(m_menuItems.GetItem("SaveAs").CreateMenuItem());
			menuitem.DropDownItems.Add(new ToolStripSeparator());
			menuitem.DropDownItems.Add(m_menuItems.GetItem("Exit").CreateMenuItem());
			m_mainMenu.Items.Insert(0, menuitem);

			ToolStripPanel panel = m_menuItems.GetStripPanel(DockStyle.Top);

			panel.Join(m_menuItems.GetStrip("layer"));
			panel.Join(m_menuItems.GetStrip("draw"));
			panel.Join(m_menuItems.GetStrip("edit"));
			panel.Join(m_menuItems.GetStrip("file"));
			panel.Join(m_mainMenu);

			panel = m_menuItems.GetStripPanel(DockStyle.Left);
			panel.Join(m_menuItems.GetStrip("modify"));

			panel = m_menuItems.GetStripPanel(DockStyle.Bottom);
			panel.Join(m_menuItems.GetStatusStrip("status"));
		}
		void OnIdle(object sender, EventArgs e)
		{
			m_activeDocument = this.ActiveMdiChild as DocumentForm;
			if (m_activeDocument != null)
				m_activeDocument.UpdateUI();

		}
		DocumentForm m_activeDocument = null;
		protected override void OnMdiChildActivate(EventArgs e)
		{
			DocumentForm olddocument = m_activeDocument;
			base.OnMdiChildActivate(e);
			m_activeDocument = this.ActiveMdiChild as DocumentForm;
			foreach (Control ctrl in Controls)
			{
				if (ctrl is ToolStripPanel)
					((ToolStripPanel)ctrl).SuspendLayout();
			}
			if (m_activeDocument != null)
			{
				ToolStripManager.RevertMerge(m_menuItems.GetStrip("edit"));
				ToolStripManager.RevertMerge(m_menuItems.GetStrip("draw"));
				ToolStripManager.RevertMerge(m_menuItems.GetStrip("layer"));
				ToolStripManager.RevertMerge(m_menuItems.GetStrip("status"));
				ToolStripManager.RevertMerge(m_menuItems.GetStrip("modify"));
				ToolStripManager.Merge(m_activeDocument.GetToolStrip("draw"), m_menuItems.GetStrip("draw"));
				ToolStripManager.Merge(m_activeDocument.GetToolStrip("edit"), m_menuItems.GetStrip("edit"));
				ToolStripManager.Merge(m_activeDocument.GetToolStrip("layer"), m_menuItems.GetStrip("layer"));
				ToolStripManager.Merge(m_activeDocument.GetToolStrip("status"), m_menuItems.GetStrip("status"));
				ToolStripManager.Merge(m_activeDocument.GetToolStrip("modify"), m_menuItems.GetStrip("modify"));
			}
			foreach (Control ctrl in Controls)
			{
				if (ctrl is ToolStripPanel)
					((ToolStripPanel)ctrl).ResumeLayout();
			}
		}
		private void OnFileOpen(object sender, EventArgs e)
		{
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.Filter = "Cad XML files (*.cadxml)|*.cadxml";
			if (dlg.ShowDialog(this) == DialogResult.OK)
				OpenDocument(dlg.FileName);
		}
		private void OnFileSave(object sender, EventArgs e)
		{
			DocumentForm doc = this.ActiveMdiChild as DocumentForm;
			if (doc != null)
				doc.Save();
		}
		private void OnFileNew(object sender, EventArgs e)
		{
			OpenDocument(string.Empty);
		}
		void OpenDocument(string filename)
		{
			DocumentForm f = new DocumentForm(filename);
			f.MdiParent = this;
			f.WindowState = FormWindowState.Maximized;
			f.Show();
		}

		private void OnFileSaveAs(object sender, EventArgs e)
		{
			DocumentForm doc = this.ActiveMdiChild as DocumentForm;
			if (doc != null)
				doc.SaveAs();
		}
		private void OnFileExit(object sender, EventArgs e)
		{
			Close();
		}
		private void OnUpdateMenuUI(object sender, EventArgs e)
		{


		}

		private void OnAbout(object sender, EventArgs e)
		{
			About dlg = new About();
			dlg.ShowDialog(this);
		}

		private void OnOptions(object sender, EventArgs e)
		{
			DocumentForm doc = this.ActiveMdiChild as DocumentForm;
			if (doc == null)
				return;

			Options.OptionsDlg dlg = new Canvas.Options.OptionsDlg();
			dlg.Config.Grid.CopyFromLayer(doc.Model.GridLayer as GridLayer);
			dlg.Config.Background.CopyFromLayer(doc.Model.BackgroundLayer as BackgroundLayer);
			foreach (DrawingLayer layer in doc.Model.Layers)
				dlg.Config.Layers.Add(new Options.OptionsLayer(layer));
			
			ToolStripItem item = sender as ToolStripItem;
			dlg.SelectPage(item.Tag);

			if (dlg.ShowDialog(this) == DialogResult.OK)
			{
				dlg.Config.Grid.CopyToLayer((GridLayer)doc.Model.GridLayer);
				dlg.Config.Background.CopyToLayer((BackgroundLayer)doc.Model.BackgroundLayer);
				foreach (Options.OptionsLayer optionslayer in dlg.Config.Layers)
				{
					DrawingLayer layer = (DrawingLayer)doc.Model.GetLayer(optionslayer.Layer.Id);
					if (layer != null)
						optionslayer.CopyToLayer(layer);
					else
					{
						// delete layer
					}
				}

				doc.Canvas.DoInvalidate(true);
			}
		}
	}
}