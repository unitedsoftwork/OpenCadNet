using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Xml;

namespace Canvas.DrawTools
{
	class NodePointLine : INodePoint
	{
		public enum ePoint
		{
			P1,
			P2,
		}
		static bool m_angleLocked = false;
		Line m_owner;
		Line m_clone;
		UnitPoint m_originalPoint;
		UnitPoint m_endPoint;
		ePoint m_pointId;
		public NodePointLine(Line owner, ePoint id)
		{
			m_owner = owner;
			m_clone = m_owner.Clone() as Line;
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
			if (Control.ModifierKeys == Keys.Control)
				pos = HitUtil.OrthoPointD(OtherPoint(m_pointId), pos, 45);
			if (m_angleLocked || Control.ModifierKeys == (Keys)(Keys.Control | Keys.Shift))
				pos = HitUtil.NearestPointOnLine(m_owner.P1, m_owner.P2, pos, true);
			SetPoint(m_pointId, pos, m_clone);
		}
		public void Finish()
		{
			m_endPoint = GetPoint(m_pointId);
			m_owner.P1 = m_clone.P1;
			m_owner.P2 = m_clone.P2;
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
			if (pointid == ePoint.P1)
				return m_clone.P1;
			if (pointid == ePoint.P2)
				return m_clone.P2;
			return m_owner.P1;
		}
		protected UnitPoint OtherPoint(ePoint currentpointid)
		{
			if (currentpointid == ePoint.P1)
				return GetPoint(ePoint.P2);
			return GetPoint(ePoint.P1);
		}
		protected void SetPoint(ePoint pointid, UnitPoint point, Line line)
		{
			if (pointid == ePoint.P1)
				line.P1 = point;
			if (pointid == ePoint.P2)
				line.P2 = point;
		}
	}
	class Line : DrawObjectBase, IDrawObject, ISerialize
	{
		protected UnitPoint m_p1, m_p2;

		[XmlSerializable]
		public UnitPoint P1
		{
			get { return m_p1; }
			set { m_p1 = value; }
		}
		[XmlSerializable]
		public UnitPoint P2
		{
			get { return m_p2; }
			set { m_p2 = value; }
		}

		public static string ObjectType
		{
			get { return "line"; }
		}
		public Line()
		{
		}
		public Line(UnitPoint point, UnitPoint endpoint, float width, Color color)
		{
			P1 = point;
			P2 = endpoint;
			Width = width;
			Color = color;
			Selected = false;
		}
		public override void InitializeFromModel(UnitPoint point, DrawingLayer layer, ISnapPoint snap)
		{
			P1 = P2 = point;
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
		public virtual void Copy(Line acopy)
		{
			base.Copy(acopy);
			m_p1 = acopy.m_p1;
			m_p2 = acopy.m_p2;
			Selected = acopy.Selected;
		}
		#region IDrawObject Members
		public virtual string Id
		{
			get { return ObjectType; }
		}
		public virtual IDrawObject Clone()
		{
			Line l = new Line();
			l.Copy(this);
			return l;
		}
		public RectangleF GetBoundingRect(ICanvas canvas)
		{
			float thWidth = ThresholdWidth(canvas, Width);
			return ScreenUtils.GetRect(m_p1, m_p2, thWidth);
		}

		UnitPoint MidPoint(ICanvas canvas, UnitPoint p1, UnitPoint p2, UnitPoint hitpoint)
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
		public bool ObjectInRectangle(ICanvas canvas, RectangleF rect, bool anyPoint)
		{
			RectangleF boundingrect = GetBoundingRect(canvas);
			if (anyPoint)
				return HitUtil.LineIntersectWithRect(m_p1, m_p2, rect);
			return rect.Contains(boundingrect);
		}
		public virtual void Draw(ICanvas canvas, RectangleF unitrect)
		{
			Color color = Color;
			Pen pen = canvas.CreatePen(color, Width);
			pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
			pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
			canvas.DrawLine(canvas, pen, m_p1, m_p2);
			if (Highlighted)
				canvas.DrawLine(canvas, DrawUtils.SelectedPen, m_p1, m_p2);
			if (Selected)
			{
				canvas.DrawLine(canvas, DrawUtils.SelectedPen, m_p1, m_p2);
				if (m_p1.IsEmpty == false)
					DrawUtils.DrawNode(canvas, m_p1);
				if (m_p2.IsEmpty == false)
					DrawUtils.DrawNode(canvas, m_p2);
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
				return new NodePointLine(this, NodePointLine.ePoint.P1);
			if (HitUtil.CircleHitPoint(m_p2, thWidth, point))
				return new NodePointLine(this, NodePointLine.ePoint.P2);
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
					if (snaptype == typeof(IntersectSnapPoint))
					{
						Line otherline = Utils.FindObjectTypeInList(this, otherobjs, typeof(Line)) as Line;
						if (otherline == null)
							continue;
						UnitPoint p = HitUtil.LinesIntersectPoint(m_p1, m_p2, otherline.m_p1, otherline.m_p2);
						if (p != UnitPoint.Empty)
							return new IntersectSnapPoint(canvas, this, p);
					}
				}
				return null;
			}

			if (usersnaptype == typeof(MidpointSnapPoint))
				return new MidpointSnapPoint(canvas, this, HitUtil.LineMidpoint(m_p1, m_p2));
			if (usersnaptype == typeof(IntersectSnapPoint))
			{
				Line otherline = Utils.FindObjectTypeInList(this, otherobjs, typeof(Line)) as Line;
				if (otherline == null)
					return null;
				UnitPoint p = HitUtil.LinesIntersectPoint(m_p1, m_p2, otherline.m_p1, otherline.m_p2);
				if (p != UnitPoint.Empty)
					return new IntersectSnapPoint(canvas, this, p);
			}
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
				P1.PosAsString(),
				P2.PosAsString(),
				HitUtil.Distance(P1, P2),
				HitUtil.RadiansToDegrees(HitUtil.LineAngleR(P1, P2, 0)));
		}
		#endregion
		#region ISerialize
		public void GetObjectData(XmlWriter wr)
		{
			wr.WriteStartElement("line");
			XmlUtil.WriteProperties(this, wr);
			wr.WriteEndElement();
		}
		public void AfterSerializedIn()
		{
		}
		#endregion

		public void ExtendLineToPoint(UnitPoint newpoint)
		{
			UnitPoint newlinepoint = HitUtil.NearestPointOnLine(P1, P2, newpoint, true);
			if (HitUtil.Distance(newlinepoint, P1) < HitUtil.Distance(newlinepoint, P2))
				P1 = newlinepoint;
			else
				P2 = newlinepoint;
		}
	}
	class LineEdit : Line, IObjectEditInstance
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
					return "line";
				return "lines";
			}
		}
		public LineEdit(bool singleLine)
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

		public new void Copy(LineEdit acopy)
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
			return new Line(P1, P2, Width, Color);
		}
		#endregion
	}
}
