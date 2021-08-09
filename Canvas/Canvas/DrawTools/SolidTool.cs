using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Xml;

namespace Canvas.DrawTools
{
	class NodePointSolid : INodePoint
	{
		public enum ePoint
		{
			Vertex1,
			Vertex2,
			Vertex3,
			Vertex4,
		}
		static bool m_angleLocked = false;
		Solid m_owner;
        Solid m_clone;
		UnitPoint m_originalPoint;
		UnitPoint m_endPoint;
		ePoint m_pointId;
		public NodePointSolid(Solid owner, ePoint id)
		{
			m_owner = owner;
			m_clone = m_owner.Clone() as Solid;
			m_pointId = id;
			m_originalPoint = GetPoint(m_pointId);
		}
		#region INodePoint Members
		public IDrawObject GetClone()
		{
			return m_clone;
		}
		public IDrawObject GetOriginal()
		{
			return m_owner;
		}
		public void SetPosition(UnitPoint pos)
		{
			SetPoint(m_pointId, pos, m_clone);
		}
		public void Finish()
		{
			m_endPoint = GetPoint(m_pointId);
			m_owner.Vertex1 = m_clone.Vertex1;
			m_owner.Vertex2 = m_clone.Vertex2;
			m_owner.Vertex3 = m_clone.Vertex3;
			m_owner.Vertex4 = m_clone.Vertex4;
			m_clone = null;
		}
		public void Cancel()
		{
		}
		public void Undo()
		{
			SetPoint(m_pointId, m_originalPoint, m_owner);
		}
		public void Redo()
		{
			SetPoint(m_pointId, m_endPoint, m_owner);
		}
		public void OnKeyDown(ICanvas canvas, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.L)
			{
				m_angleLocked = !m_angleLocked;
				e.Handled = true;
			}
		}
		#endregion
		protected UnitPoint GetPoint(ePoint pointid)
		{
            if (pointid == ePoint.Vertex1)
                return m_clone.Vertex1;
            if (pointid == ePoint.Vertex2)
                return m_clone.Vertex2;    
            if (pointid == ePoint.Vertex3)
                return m_clone.Vertex3;
            if (pointid == ePoint.Vertex4)
                return m_clone.Vertex4;
            return m_owner.Vertex1;
        }
		protected void SetPoint(ePoint pointid, UnitPoint point, Solid solid)
		{
			if (pointid == ePoint.Vertex1)
                solid.Vertex1 = point;
            if (pointid == ePoint.Vertex2)
                solid.Vertex2 = point;
            if (pointid == ePoint.Vertex3)
                solid.Vertex3 = point;
            if (pointid == ePoint.Vertex4)
                solid.Vertex4 = point;
        }
	}
	class Solid : DrawObjectBase, IDrawObject, ISerialize
	{
		protected UnitPoint m_p1, m_p2, m_p3, m_p4;

		public UnitPoint Vertex1
		{
			get { return m_p1; }
			set { m_p1 = value; }
		}
   
        public UnitPoint Vertex2
        {
            get { return m_p2; }
            set { m_p2 = value; }
        }
        public UnitPoint Vertex3
        {
            get { return m_p3; }
            set { m_p3 = value; }
        }

        public UnitPoint Vertex4
        {
            get { return m_p4; }
            set { m_p4 = value; }
        }

        public static string ObjectType
		{
			get { return "solid"; }
		}
		public Solid()
		{
		}
		public Solid(UnitPoint vertex1, UnitPoint vertex2, UnitPoint vertex3, UnitPoint vertex4, Color color)
		{
			Vertex1 = vertex1;
			Vertex2 = vertex2;
			Vertex3 = vertex3;
			Vertex4 = vertex4;
		
			Color = color;
			Selected = false;
		}
		public override void InitializeFromModel(UnitPoint point, DrawingLayer layer, ISnapPoint snap)
		{
			Vertex1 = Vertex2 = point;
			Width = layer.Width;
			Color = layer.Color;
			Selected = true;
		}

		static int ThresholdPixel = 6;
		static float ThresholdWidth(ICanvas canvas, float objectwidth)
		{
			return ThresholdWidth(canvas, objectwidth, ThresholdPixel);
		}
		public static float ThresholdWidth(ICanvas canvas, float objectwidth, float pixelwidth)
		{
			double minWidth = canvas.ToUnit(pixelwidth);
			double width = Math.Max(objectwidth / 2, minWidth);
			return (float)width;
		}
		public virtual void Copy(Solid acopy)
		{
			base.Copy(acopy);
			m_p1 = acopy.m_p1;
			m_p2 = acopy.m_p2;
			m_p3 = acopy.m_p3;
			m_p4 = acopy.m_p4;
			Color = acopy.Color;
			Selected = acopy.Selected;
		}
		#region IDrawObject Members
		public virtual string Id
		{
			get { return ObjectType; }
		}
		public virtual IDrawObject Clone()
		{
			Solid l = new Solid();
			l.Copy(this);
			return l;
		}
		public RectangleF GetBoundingRect(ICanvas canvas)
		{
			float thWidth = ThresholdWidth(canvas, Width);
			return ScreenUtils.GetRect(m_p1, m_p2, thWidth);
		}

		public UnitPoint MidPoint(ICanvas canvas, UnitPoint p1, UnitPoint p2, UnitPoint hitpoint)
		{
			UnitPoint mid = HitUtil.LineMidpoint(p1, p2);
			float thWidth = ThresholdWidth(canvas, Width);
			if (HitUtil.CircleHitPoint(mid, thWidth, hitpoint))
				return mid;
			return UnitPoint.Empty;
		}
		public bool PointInObject(ICanvas canvas, UnitPoint point)
		{
			float thWidth = ThresholdWidth(canvas, Width);
			return HitUtil.IsPointInLine(m_p1, m_p2, point, thWidth);
		}
		public bool ObjectInShape(ICanvas canvas, List<PointF> shape, bool anyPoint)
		{
            RectangleF rect = Utils.GetRectangleFromPoints(shape);
            RectangleF boundingrect = GetBoundingRect(canvas);
			if (anyPoint)
				return HitUtil.LineIntersectWithRect(m_p1, m_p2, rect);
			return rect.Contains(boundingrect);
		}
		public virtual void Draw(ICanvas canvas, RectangleF unitrect)
		{
			Color color = Color;
			Pen pen = canvas.CreatePen(color, Width);
			pen.EndCap = System.Drawing.Drawing2D.LineCap.Square;
			pen.StartCap = System.Drawing.Drawing2D.LineCap.Square;
			canvas.DrawSolid(canvas, pen, m_p1, m_p2, m_p3, m_p4);
			if (Highlighted)
                canvas.DrawSolid(canvas, DrawUtils.SelectedPen, m_p1, m_p2, m_p3, m_p4);
            if (Selected)
			{
                canvas.DrawSolid(canvas, DrawUtils.SelectedPen, m_p1, m_p2, m_p3, m_p4);
                if (m_p1.IsEmpty == false)
					DrawUtils.DrawNode(canvas, m_p1);
                if (m_p2.IsEmpty == false)
                    DrawUtils.DrawNode(canvas, m_p2);
                if (m_p3.IsEmpty == false)
                    DrawUtils.DrawNode(canvas, m_p3);
                if (m_p4.IsEmpty == false)
					DrawUtils.DrawNode(canvas, m_p4);
            }
		}
		public virtual void OnMouseMove(ICanvas canvas, UnitPoint point)
		{
			if (Control.ModifierKeys == Keys.Control)
				point = HitUtil.OrthoPointD(m_p1, point, 45);
			m_p2 = point;
		}
		public virtual eDrawObjectMouseDown OnMouseDown(ICanvas canvas, UnitPoint point, ISnapPoint snappoint)
		{
			Selected = false;
			if (snappoint is PerpendicularSnapPoint && snappoint.Owner is Line)
			{
				Line src = snappoint.Owner as Line;
				m_p2 = HitUtil.NearestPointOnLine(src.P1, src.P2, m_p1, true);
				return eDrawObjectMouseDown.DoneRepeat;
			}
			if (snappoint is PerpendicularSnapPoint && snappoint.Owner is Arc)
			{
				Arc src = snappoint.Owner as Arc;
				m_p2 = HitUtil.NearestPointOnCircle(src.Center, src.Radius, m_p1, 0);
				return eDrawObjectMouseDown.DoneRepeat;
			}
			if (Control.ModifierKeys == Keys.Control)
				point = HitUtil.OrthoPointD(m_p1, point, 45);
			m_p2 = point;
			return eDrawObjectMouseDown.DoneRepeat;
		}
		public void OnMouseUp(ICanvas canvas, UnitPoint point, ISnapPoint snappoint)
		{
		}
		public virtual void OnKeyDown(ICanvas canvas, KeyEventArgs e)
		{
		}
		public UnitPoint RepeatStartingPoint
		{
			get { return m_p2; }
		}
		public INodePoint NodePoint(ICanvas canvas, UnitPoint point)
		{
			float thWidth = ThresholdWidth(canvas, Width);
			if (HitUtil.CircleHitPoint(m_p1, thWidth, point))
				return new NodePointSolid(this, NodePointSolid.ePoint.Vertex1);
			if (HitUtil.CircleHitPoint(m_p2, thWidth, point))
				return new NodePointSolid(this, NodePointSolid.ePoint.Vertex2);
			return null;
		}
		public ISnapPoint SnapPoint(ICanvas canvas, UnitPoint point, List<IDrawObject> otherobjs, Type[] runningsnaptypes, Type usersnaptype)
		{
			float thWidth = ThresholdWidth(canvas, Width);
			if (runningsnaptypes != null)
			{
				foreach (Type snaptype in runningsnaptypes)
				{
					if (snaptype == typeof(VertextSnapPoint))
					{
						if (HitUtil.CircleHitPoint(m_p1, thWidth, point))
							return new VertextSnapPoint(canvas, this, m_p1);
						if (HitUtil.CircleHitPoint(m_p2, thWidth, point))
							return new VertextSnapPoint(canvas, this, m_p2);
					}
					if (snaptype == typeof(MidpointSnapPoint))
					{
						UnitPoint p = MidPoint(canvas, m_p1, m_p2, point);
						if (p != UnitPoint.Empty)
							return new MidpointSnapPoint(canvas, this, p);
					}
	
				}
				return null;
			}

			if (usersnaptype == typeof(MidpointSnapPoint))
				return new MidpointSnapPoint(canvas, this, HitUtil.LineMidpoint(m_p1, m_p2));
	
			if (usersnaptype == typeof(VertextSnapPoint))
			{
				double d1 = HitUtil.Distance(point, m_p1);
				double d2 = HitUtil.Distance(point, m_p2);
				if (d1 <= d2)
					return new VertextSnapPoint(canvas, this, m_p1);
				return new VertextSnapPoint(canvas, this, m_p2);
			}
			if (usersnaptype == typeof(NearestSnapPoint))
			{
				UnitPoint p = HitUtil.NearestPointOnLine(m_p1, m_p2, point);
				if (p != UnitPoint.Empty)
					return new NearestSnapPoint(canvas, this, p);
			}
			if (usersnaptype == typeof(PerpendicularSnapPoint))
			{
				UnitPoint p = HitUtil.NearestPointOnLine(m_p1, m_p2, point);
				if (p != UnitPoint.Empty)
					return new PerpendicularSnapPoint(canvas, this, p);
			}
			return null;
		}
		public void Move(UnitPoint offset)
		{
			m_p1.X += offset.X;
			m_p1.Y += offset.Y;
			m_p2.X += offset.X;
			m_p2.Y += offset.Y;
		}
		public string GetInfoAsString()
		{
			return string.Format("Line@{0},{1} - L={2:f4}<{3:f4}",
				Vertex1.PosAsString(),
				Vertex2.PosAsString(),
				HitUtil.Distance(Vertex1, Vertex2),
				HitUtil.RadiansToDegrees(HitUtil.LineAngleR(Vertex1, Vertex2, 0)));
		}
		#endregion
		#region ISerialize
		public void GetObjectData(XmlWriter wr)
		{
			wr.WriteStartElement("solid");
			XmlUtil.WriteProperties(this, wr);
			wr.WriteEndElement();
		}
		public void AfterSerializedIn()
		{
		}
		#endregion

		
	}
	class SolidEdit : Solid, IObjectEditInstance
	{
		protected PerpendicularSnapPoint m_perSnap;
		protected TangentSnapPoint m_tanSnap;
		protected bool m_tanReverse = false;
		protected bool m_singleLineSegment = true;

		public override string Id
		{
			get
			{
				if (m_singleLineSegment)
					return "solid";
				return "solids";
			}
		}
		public SolidEdit(bool singleLine)
			: base()
		{
			m_singleLineSegment = singleLine;
		}

		public override void InitializeFromModel(UnitPoint point, DrawingLayer layer, ISnapPoint snap)
		{
			base.InitializeFromModel(point, layer, snap);
			m_perSnap = snap as PerpendicularSnapPoint;
			m_tanSnap = snap as TangentSnapPoint;
		}
		public override void OnMouseMove(ICanvas canvas, UnitPoint point)
		{
			if (m_perSnap != null)
			{
				MouseMovePerpendicular(canvas, point);
				return;
			}
			if (m_tanSnap != null)
			{
				MouseMoveTangent(canvas, point);
				return;
			}
			base.OnMouseMove(canvas, point);
		}
		public override eDrawObjectMouseDown OnMouseDown(ICanvas canvas, UnitPoint point, ISnapPoint snappoint)
		{
			if (m_tanSnap != null && Control.MouseButtons == MouseButtons.Right)
			{
				ReverseTangent(canvas);
				return eDrawObjectMouseDown.Continue;
			}

			if (m_perSnap != null || m_tanSnap != null)
			{
				if (snappoint != null)
					point = snappoint.SnapPoint;
				OnMouseMove(canvas, point);
				if (m_singleLineSegment)
					return eDrawObjectMouseDown.Done;
				return eDrawObjectMouseDown.DoneRepeat;
			}
			eDrawObjectMouseDown result = base.OnMouseDown(canvas, point, snappoint);
			if (m_singleLineSegment)
				return eDrawObjectMouseDown.Done;
			return eDrawObjectMouseDown.DoneRepeat;
		}
		public override void OnKeyDown(ICanvas canvas, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.R && m_tanSnap != null)
			{
				ReverseTangent(canvas);
				e.Handled = true;
			}
		}
		protected virtual void MouseMovePerpendicular(ICanvas canvas, UnitPoint point)
		{
			if (m_perSnap.Owner is Line)
			{
				Line src = m_perSnap.Owner as Line;
				m_p1 = HitUtil.NearestPointOnLine(src.P1, src.P2, point, true);
				m_p2 = point;
			}
			if (m_perSnap.Owner is IArc)
			{
				IArc src = m_perSnap.Owner as IArc;
				m_p1 = HitUtil.NearestPointOnCircle(src.Center, src.Radius, point, 0);
				m_p2 = point;
			}
		}
		protected virtual void MouseMoveTangent(ICanvas canvas, UnitPoint point)
		{
			if (m_tanSnap.Owner is IArc)
			{
				IArc src = m_tanSnap.Owner as IArc;
				m_p1 = HitUtil.TangentPointOnCircle(src.Center, src.Radius, point, m_tanReverse);
				m_p2 = point;
				if (m_p1 == UnitPoint.Empty)
					m_p2 = m_p1 = src.Center;
			}
		}
		protected virtual void ReverseTangent(ICanvas canvas)
		{
			m_tanReverse = !m_tanReverse;
			MouseMoveTangent(canvas, m_p2);
			canvas.Invalidate();
		}

		public new void Copy(SolidEdit acopy)
		{
			base.Copy(acopy);
			m_perSnap = acopy.m_perSnap;
			m_tanSnap = acopy.m_tanSnap;
			m_tanReverse = acopy.m_tanReverse;
			m_singleLineSegment = acopy.m_singleLineSegment;
		}
		public override IDrawObject Clone()
		{
			LineEdit l = new LineEdit(false);
			l.Copy(this);
			return l;
		}

		#region IObjectEditInstance
		public IDrawObject GetDrawObject()
		{
			return new Line(Vertex1, Vertex2, Width, Color);
		}
		#endregion
	}
}
