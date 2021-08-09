using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Xml;
using System.IO;

namespace Canvas
{
	public class DrawingLayer : ICanvasLayer, ISerialize
	{
		string m_id;
		string m_name = "<Layer>";
		Color m_color;
		float m_width = 0.00f;
		bool m_enabled = true;
		bool m_visible = true;
		[XmlSerializable]
		public Color Color
		{
			get { return m_color; }
			set { m_color = value; }
		}
		[XmlSerializable]
		public float Width
		{
			get { return m_width; }
			set { m_width = value; }
		}
		[XmlSerializable]
		public string Name
		{
			get { return m_name; }
			set { m_name = value; }
		}
		public DrawingLayer(string id, string name, Color color, float width)
		{
			m_id = id;
			m_name = name;
			m_color = color;
			m_width = width;
		}
		List<IDrawObject> m_objects = new List<IDrawObject>();
		Dictionary<IDrawObject, bool> m_objectMap = new Dictionary<IDrawObject, bool>();
		public void AddObject(IDrawObject drawobject)
		{
			if (m_objectMap.ContainsKey(drawobject))
				return; // this should never happen
			if (drawobject is DrawTools.DrawObjectBase)
				((DrawTools.DrawObjectBase)drawobject).Layer = this;
			m_objects.Add(drawobject);
			m_objectMap[drawobject] = true;
		}
		public List<IDrawObject> DeleteObjects(IEnumerable<IDrawObject> objects)
		{
			if (Enabled == false)
				return null;
			List<IDrawObject> removedobjects = new List<IDrawObject>();
			// first remove from map only
			foreach (IDrawObject obj in objects)
			{
				if (m_objectMap.ContainsKey(obj))
				{
					m_objectMap.Remove(obj);
					removedobjects.Add(obj);
				}
			}
			// need some smart algorithm here to either remove from existing list or build a new list
			// for now I will just ise the removed count;
			if (removedobjects.Count == 0)
				return null;
			if (removedobjects.Count < 10) // remove from existing list
			{
				foreach (IDrawObject obj in removedobjects)
					m_objects.Remove(obj);
			}
			else // else build new list;
			{
				List<IDrawObject> newlist = new List<IDrawObject>();
				foreach (IDrawObject obj in m_objects)
				{
					if (m_objectMap.ContainsKey(obj))
						newlist.Add(obj);
				}
				m_objects.Clear();
				m_objects = newlist;
			}
			return removedobjects;
		}
		public int Count
		{
			get { return m_objects.Count; }
		}
		public void Copy(DrawingLayer acopy, bool includeDrawObjects)
		{
			if (includeDrawObjects)
				throw new Exception("not supported yet");
			m_id = acopy.m_id;
			m_name = acopy.m_name;
			m_color = acopy.m_color;
			m_width = acopy.m_width;
			m_enabled = acopy.m_enabled;
			m_visible = acopy.m_visible;
		}
		#region ICanvasLayer Members
		public void Draw(ICanvas canvas, RectangleF unitrect)
		{
			CommonTools.Tracing.StartTrack(Program.TracePaint);
			int cnt = 0;
			foreach (IDrawObject drawobject in m_objects)
			{
				DrawTools.DrawObjectBase obj = drawobject as DrawTools.DrawObjectBase;
				if (obj is IDrawObject && ((IDrawObject)obj).ObjectInRectangle(canvas, unitrect, true) == false)
					continue;
				bool sel = obj.Selected;
				bool high = obj.Highlighted;
				obj.Selected = false;
				drawobject.Draw(canvas, unitrect);
				obj.Selected = sel;
				obj.Highlighted = high;
				cnt++;
			}
			CommonTools.Tracing.EndTrack(Program.TracePaint, "Draw Layer {0}, ObjCount {1}, Painted ObjCount {2}", Id, m_objects.Count, cnt);
		}
		public PointF SnapPoint(PointF unitmousepoint)
		{
			return PointF.Empty;
		}
		public string Id
		{
			get { return m_id; }
		}
		public ISnapPoint SnapPoint(ICanvas canvas, UnitPoint point, List<IDrawObject> otherobj)
		{
			foreach (IDrawObject obj in m_objects)
			{
				ISnapPoint sp = obj.SnapPoint(canvas, point, otherobj, null, null);
				if (sp != null)
					return sp;
			}
			return null;
		}
		public IEnumerable<IDrawObject> Objects
		{
			get { return m_objects; }
		}
		[XmlSerializable]
		public bool Enabled
		{
			get { return m_enabled && m_visible; }
			set { m_enabled = value; }
		}
		[XmlSerializable]
		public bool Visible
		{
			get { return m_visible; }
			set { m_visible = value; }
		}
		#endregion
		#region XML Serialize
		public void GetObjectData(XmlWriter wr)
		{
			wr.WriteStartElement("layer");
			wr.WriteAttributeString("Id", m_id);
			XmlUtil.WriteProperties(this, wr);
			wr.WriteStartElement("items");
			foreach (IDrawObject drawobj in m_objects)
			{
				if (drawobj is ISerialize)
					((ISerialize)drawobj).GetObjectData(wr);
			}
			wr.WriteEndElement();
			wr.WriteEndElement();
		}
		public void AfterSerializedIn()
		{
		}
		public static DrawingLayer NewLayer(XmlElement xmlelement)
		{
			string id = xmlelement.GetAttribute("Id");
			if (id.Length == 0)
				id = Guid.NewGuid().ToString();
			DrawingLayer layer = new DrawingLayer(id, string.Empty, Color.White, 0.0f);
			foreach (XmlElement node in xmlelement.ChildNodes)
			{
				XmlUtil.ParseProperty(node, layer);
				if (node.Name == "items")
				{
					foreach (XmlElement itemnode in node.ChildNodes)
					{
						object item = DataModel.NewDrawObject(itemnode.Name);
						if (item == null)
							continue;
						if (item != null)
							XmlUtil.ParseProperties(itemnode, item);
						if (item is ISerialize)
						   ((ISerialize)item).AfterSerializedIn();
						if (item is IDrawObject)
							layer.AddObject(item as IDrawObject);
					}
				}
			}
			return layer;
		}
		#endregion
	}
}
