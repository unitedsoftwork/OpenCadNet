using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace Canvas
{
	public class MenuItem
	{
		public string Text = string.Empty;
		public string ToolTipText = string.Empty;
		public string ShortcutKeyDisplayString = string.Empty;
		public Shortcut ShortcutKeys = Shortcut.None;
		public Keys SingleKey = Keys.None;
		public Image Image;
		public System.EventHandler Click;
		bool m_enabled = true;
		public bool Enabled
		{
			get { return m_enabled; }
			set
			{
				foreach (ToolStripItem item in m_items)
					item.Enabled = value;
				m_enabled = value;
			}
		}
		public object Tag;
		List<ToolStripItem> m_items = new List<ToolStripItem>();
		public ToolStripItem[] Items
		{
			get { return m_items.ToArray(); }
		}
		ToolStripItem CreateItem(Type type)
		{
			ToolStripItem item = Activator.CreateInstance(type) as ToolStripItem;
			item.Text = Text;
			item.Image = Image;
			item.Click += Click;
			item.Tag = Tag;
			item.ToolTipText = ToolTipText;
			item.Enabled = Enabled;
			m_items.Add(item);

			ToolStripMenuItem menuitem = item as ToolStripMenuItem;
			if (menuitem != null)
			{
				if (ShortcutKeyDisplayString.Length > 0)
					menuitem.ShortcutKeyDisplayString = ShortcutKeyDisplayString;
				if (ShortcutKeys != Shortcut.None)
					menuitem.ShortcutKeys = (Keys)ShortcutKeys;
			}
			return item;
		}
		public ToolStripButton CreateButton()
		{
			ToolStripButton b = CreateItem(typeof(ToolStripButton)) as ToolStripButton;
			b.DisplayStyle = ToolStripItemDisplayStyle.Image;
			return b;
		}
		public ToolStripMenuItem CreateMenuItem()
		{
			return CreateItem(typeof(ToolStripMenuItem)) as ToolStripMenuItem;
		}
	}
	public class MenuItemManager
	{
		Control m_owner;
		Dictionary<object, MenuItem> m_items = new Dictionary<object, MenuItem>();
		Dictionary<object, ToolStrip> m_strips = new Dictionary<object,ToolStrip>();
		Dictionary<object, ToolStripItem> m_stripsItem= new Dictionary<object, ToolStripItem>();
		public MenuItemManager(Control owner)
		{
			m_owner = owner;
		}
		public MenuItemManager()
		{
		}
		public MenuItem GetItem(object key)
		{
			if (m_items.ContainsKey(key) == false)
				m_items[key] = new MenuItem();
			return m_items[key];
		}
		public ToolStrip GetStrip(object key)
		{
			if (m_strips.ContainsKey(key) == false)
				m_strips[key] = new ToolStrip();
			return m_strips[key];
		}
		public ToolStripMenuItem GetMenuStrip(object key)
		{
			if (m_stripsItem.ContainsKey(key) == false)
				m_stripsItem[key] = new ToolStripMenuItem();
			return m_stripsItem[key] as ToolStripMenuItem;
		}
		public StatusStrip GetStatusStrip(object key)
		{
			if (m_strips.ContainsKey(key) == false)
				m_strips[key] = new StatusStrip();
			return m_strips[key] as StatusStrip;
		}
		public void DisableAll()
		{
			foreach (MenuItem item in m_items.Values)
				item.Enabled = false;
		}
		public ToolStripPanel GetStripPanel(DockStyle dockedLocation)
		{
			if (m_owner == null)
				return null;
			foreach (Control ctrl in m_owner.Controls)
			{
				if (ctrl is ToolStripPanel && ((ToolStripPanel)ctrl).Dock == dockedLocation)
					return ctrl as ToolStripPanel;
			}
			return null;
		}
		void CreateStripPanel(DockStyle dockedLocation)
		{
			if (m_owner != null && GetStripPanel(dockedLocation) == null)
			{
				ToolStripPanel panel = new ToolStripPanel();
				panel.Dock = dockedLocation;
				m_owner.Controls.Add(panel);
			}
		}
		public void SetupStripPanels()
		{
			if (m_owner == null)
				return;
			CreateStripPanel(DockStyle.Left);
			CreateStripPanel(DockStyle.Right);
			CreateStripPanel(DockStyle.Top);
			CreateStripPanel(DockStyle.Bottom);
		}
		public MenuItem FindFromSingleKey(Keys key)
		{
			foreach (MenuItem item in m_items.Values)
			{
				if (item.SingleKey == key)
					return item;
			}
			return null;
		}
	}
}
