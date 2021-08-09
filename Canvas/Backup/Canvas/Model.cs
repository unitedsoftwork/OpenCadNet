using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Reflection;

namespace Canvas
{
	public class DataModel : IModel
	{
		static Dictionary<string, Type> m_toolTypes = new Dictionary<string,Type>();
		static public IDrawObject NewDrawObject(string objecttype)
		{
			if (m_toolTypes.ContainsKey(objecttype))
			{
				string type = m_toolTypes[objecttype].ToString();
				return Assembly.GetExecutingAssembly().CreateInstance(type) as IDrawObject;
			}
			return null;
		}

		Dictionary<string, IDrawObject> m_drawObjectTypes = new Dictionary<string, IDrawObject>(); 
		DrawTools.DrawObjectBase CreateObject(string objecttype)
		{
			if (m_drawObjectTypes.ContainsKey(objecttype))
			{
				return m_drawObjectTypes[objecttype].Clone() as DrawTools.DrawObjectBase;
			}
			return null;
		}


		Dictionary<string, IEditTool> m_editTools = new Dictionary<string, IEditTool>(); 
		public void AddEditTool(string key, IEditTool tool)
		{
			m_editTools.Add(key, tool);
		}

		public bool IsDirty
		{
			get { return m_undoBuffer.Dirty; }
		}
		UndoRedoBuffer m_undoBuffer = new UndoRedoBuffer();
		
		UnitPoint m_centerPoint = UnitPoint.Empty;
		[XmlSerializable]
		public UnitPoint CenterPoint
		{
			get { return m_centerPoint; }
			set { m_centerPoint = value; }
		}

		float m_zoom = 1;
		GridLayer m_gridLayer = new GridLayer();
		BackgroundLayer m_backgroundLayer = new BackgroundLayer();
		List<ICanvasLayer> m_layers = new List<ICanvasLayer>();
		ICanvasLayer m_activeLayer;
		Dictionary<IDrawObject, bool> m_selection = new Dictionary<IDrawObject, bool>();
		public DataModel()
		{
			m_toolTypes.Clear();
			m_toolTypes[DrawTools.Line.ObjectType] = typeof(DrawTools.Line);
			m_toolTypes[DrawTools.Circle.ObjectType] = typeof(DrawTools.Circle);
			m_toolTypes[DrawTools.Arc.ObjectType] = typeof(DrawTools.Arc);
			m_toolTypes[DrawTools.Arc3Point.ObjectType] = typeof(DrawTools.Arc3Point);
			DefaultLayer();
			m_centerPoint = new UnitPoint(0,0);
		}
		public void AddDrawTool(string key, IDrawObject drawtool)
		{
			m_drawObjectTypes[key] = drawtool;
		}
		public void Save(string filename)
		{
			try
			{
				XmlTextWriter wr = new XmlTextWriter(filename, null);
				wr.Formatting = Formatting.Indented;
				wr.WriteStartElement("CanvasDataModel");
				m_backgroundLayer.GetObjectData(wr);
				m_gridLayer.GetObjectData(wr);
				foreach (ICanvasLayer layer in m_layers)
				{
					if (layer is ISerialize)
						((ISerialize)layer).GetObjectData(wr);
				}
				XmlUtil.WriteProperties(this, wr);
				wr.WriteEndElement();
				wr.Close();
				m_undoBuffer.Dirty = false;
			}
			catch { }
		}
		public bool Load(string filename)
		{
			try
			{
				StreamReader sr = new StreamReader(filename);
				//XmlTextReader rd = new XmlTextReader(sr);
				XmlDocument doc = new XmlDocument();
				doc.Load(sr);
				sr.Dispose();
				XmlElement root = doc.DocumentElement;
				if (root.Name != "CanvasDataModel")
					return false;

				m_layers.Clear();
				m_undoBuffer.Clear();
				m_undoBuffer.Dirty = false;
				foreach (XmlElement childnode in root.ChildNodes)
				{
					if (childnode.Name == "backgroundlayer")
					{
						XmlUtil.ParseProperties(childnode, m_backgroundLayer);
						continue;
					}
					if (childnode.Name == "gridlayer")
					{
						XmlUtil.ParseProperties(childnode, m_gridLayer);
						continue;
					}
					if (childnode.Name == "layer")
					{
						DrawingLayer l = DrawingLayer.NewLayer(childnode as XmlElement);
						m_layers.Add(l);
					}
					if (childnode.Name == "property")
						XmlUtil.ParseProperty(childnode, this);
				}
				return true;
			}
			catch (Exception e)
			{
				DefaultLayer();
				Console.WriteLine("Load exception - {0}", e.Message);
			}
			return false;
		}
		void DefaultLayer()
		{
			m_layers.Clear();
			m_layers.Add(new DrawingLayer("layer0", "Hairline Layer", Color.White, 0.0f));
			m_layers.Add(new DrawingLayer("layer1", "0.005 Layer", Color.Red, 0.005f));
			m_layers.Add(new DrawingLayer("layer2", "0.025 Layer", Color.Green, 0.025f));
		}
		public IDrawObject GetFirstSelected()
		{
			if (m_selection.Count > 0)
			{
				Dictionary<IDrawObject, bool>.KeyCollection.Enumerator e = m_selection.Keys.GetEnumerator();
				e.MoveNext();
				return e.Current;
			}
			return null;
		}
		#region IModel Members
		[XmlSerializable]
		public float Zoom
		{
			get { return m_zoom; }
			set { m_zoom = value; }
		}
		public ICanvasLayer BackgroundLayer
		{
			get { return m_backgroundLayer; }
		}
		public ICanvasLayer GridLayer
		{
			get { return m_gridLayer; }
		}
		public ICanvasLayer[] Layers
		{
			get { return m_layers.ToArray(); }
		}
		public ICanvasLayer ActiveLayer
		{
			get
			{
				if (m_activeLayer == null)
					m_activeLayer = m_layers[0];
				return m_activeLayer;
			}
			set
			{
				m_activeLayer = value;
			}
		}
		public ICanvasLayer GetLayer(string id)
		{
			foreach (ICanvasLayer layer in m_layers)
			{
				if (layer.Id == id)
					return layer;
			}
			return null;
		}
		public IDrawObject CreateObject(string type, UnitPoint point, ISnapPoint snappoint)
		{
			DrawingLayer layer = ActiveLayer as DrawingLayer;
			if (layer.Enabled == false)
				return null;
			DrawTools.DrawObjectBase newobj = CreateObject(type);
			if (newobj != null)
			{
				newobj.Layer = layer;
				newobj.InitializeFromModel(point, layer, snappoint);
			}
			return newobj as IDrawObject;
		}
		public void AddObject(ICanvasLayer layer, IDrawObject drawobject)
		{
			if (drawobject is DrawTools.IObjectEditInstance)
				drawobject = ((DrawTools.IObjectEditInstance)drawobject).GetDrawObject();
			if (m_undoBuffer.CanCapture)
				m_undoBuffer.AddCommand(new EditCommandAdd(layer, drawobject));
			((DrawingLayer)layer).AddObject(drawobject);
		}
		public void DeleteObjects(IEnumerable<IDrawObject> objects)
		{
			EditCommandRemove undocommand = null;
			if (m_undoBuffer.CanCapture)
				undocommand = new EditCommandRemove();
			foreach (ICanvasLayer layer in m_layers)
			{
				List<IDrawObject> removedobjects = ((DrawingLayer)layer).DeleteObjects(objects);
				if (removedobjects != null && undocommand != null)
					undocommand.AddLayerObjects(layer, removedobjects);
			}
			if (undocommand != null)
				m_undoBuffer.AddCommand(undocommand);
		}
		public void MoveObjects(UnitPoint offset, IEnumerable<IDrawObject> objects)
		{
			if (m_undoBuffer.CanCapture)
				m_undoBuffer.AddCommand(new EditCommandMove(offset, objects));
			foreach (IDrawObject obj in objects)
				obj.Move(offset);
		}
		public void CopyObjects(UnitPoint offset, IEnumerable<IDrawObject> objects)
		{
			ClearSelectedObjects();
			List<IDrawObject> newobjects = new List<IDrawObject>();
			foreach (IDrawObject obj in objects)
			{
				IDrawObject newobj = obj.Clone();
				newobjects.Add(newobj);
				newobj.Move(offset);
				((DrawingLayer)ActiveLayer).AddObject(newobj);
				AddSelectedObject(newobj);
			}
			if (m_undoBuffer.CanCapture)
				m_undoBuffer.AddCommand(new EditCommandAdd(ActiveLayer, newobjects));
		}
		public void AfterEditObjects(IEditTool edittool)
		{
			edittool.Finished();
			if (m_undoBuffer.CanCapture)
				m_undoBuffer.AddCommand(new EditCommandEditTool(edittool));
		}
		public IEditTool GetEditTool(string edittoolid)
		{
			if (m_editTools.ContainsKey(edittoolid))
				return m_editTools[edittoolid].Clone();
			return null;
		}
		public void MoveNodes(UnitPoint position, IEnumerable<INodePoint> nodes)
		{
			if (m_undoBuffer.CanCapture)
				m_undoBuffer.AddCommand(new EditCommandNodeMove(nodes));
			foreach (INodePoint node in nodes)
			{
				node.SetPosition(position);
				node.Finish();
			}
		}
		public List<IDrawObject> GetHitObjects(ICanvas canvas, RectangleF selection, bool anyPoint)
		{
			List<IDrawObject> selected = new List<IDrawObject>();
			foreach (ICanvasLayer layer in m_layers)
			{
				if (layer.Visible == false)
					continue;
				foreach (IDrawObject drawobject in layer.Objects)
				{
					if (drawobject.ObjectInRectangle(canvas, selection, anyPoint))
						selected.Add(drawobject);
				}
			}
			return selected;
		}
		public List<IDrawObject> GetHitObjects(ICanvas canvas, UnitPoint point)
		{
			List<IDrawObject> selected = new List<IDrawObject>();
			foreach (ICanvasLayer layer in m_layers)
			{
				if (layer.Visible == false)
					continue;
				foreach (IDrawObject drawobject in layer.Objects)
				{
					if (drawobject.PointInObject(canvas, point))
						selected.Add(drawobject);
				}
			}
			return selected;
		}
		public bool IsSelected(IDrawObject drawobject)
		{
			return m_selection.ContainsKey(drawobject);
		}
		public void AddSelectedObject(IDrawObject drawobject)
		{
			DrawTools.DrawObjectBase obj = drawobject as DrawTools.DrawObjectBase;
			RemoveSelectedObject(drawobject);
			m_selection[drawobject] = true;
			obj.Selected = true;
		}
		public void RemoveSelectedObject(IDrawObject drawobject)
		{
			if (m_selection.ContainsKey(drawobject))
			{
				DrawTools.DrawObjectBase obj = drawobject as DrawTools.DrawObjectBase;
				obj.Selected = false;
				m_selection.Remove(drawobject);
			}
		}
		public IEnumerable<IDrawObject> SelectedObjects
		{
			get
			{
				return m_selection.Keys;
			}
		}
		public int SelectedCount
		{
			get { return m_selection.Count; }
		}
		public void ClearSelectedObjects()
		{
			IEnumerable<IDrawObject> x = SelectedObjects;
			foreach (IDrawObject drawobject in x)
			{
				DrawTools.DrawObjectBase obj = drawobject as DrawTools.DrawObjectBase;
				obj.Selected = false;
			}
			m_selection.Clear();
		}
		public ISnapPoint SnapPoint(ICanvas canvas, UnitPoint point, Type[] runningsnaptypes, Type usersnaptype)
		{
			List<IDrawObject> objects = GetHitObjects(canvas, point);
			if (objects.Count == 0)
				return null;

			foreach (IDrawObject obj in objects)
			{
				ISnapPoint snap = obj.SnapPoint(canvas, point, objects, runningsnaptypes, usersnaptype);
				if (snap != null)
					return snap;
			}
			return null;
		}

		public bool CanUndo()
		{
			return m_undoBuffer.CanUndo;
		}
		public bool DoUndo()
		{
			return m_undoBuffer.DoUndo(this);
		}
		public bool CanRedo()
		{
			return m_undoBuffer.CanRedo;

		}
		public bool DoRedo()
		{
			return m_undoBuffer.DoRedo(this);
		}
		#endregion
	}
}
