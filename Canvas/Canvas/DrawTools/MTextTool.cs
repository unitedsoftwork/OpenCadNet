using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Xml;

namespace Canvas.DrawTools
{
	class NodePointMText : INodePoint
	{
		public enum ePoint
		{
            Position
		
		}
		static bool m_angleLocked = false;
		MText m_owner;
		MText m_clone;
		UnitPoint m_originalPoint;

		ePoint m_pointId;
		public NodePointMText(MText owner, ePoint id)
		{
			m_owner = owner;
			m_clone = m_owner.Clone() as MText;
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
				//pos = HitUtil.OrthoPointD(OtherPoint(m_pointId), pos, 45);
			if (m_angleLocked || Control.ModifierKeys == (Keys)(Keys.Control | Keys.Shift))
				pos = HitUtil.NearestPointOnLine(m_owner.Position, m_owner.Position, pos, true);
			SetPoint(m_pointId, pos, m_clone);
		}
		public void Finish()
		{
			
			m_owner.Position = m_clone.Position;
			
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
			//SetPoint(m_pointId, m_endPoint, m_owner);
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
			if (pointid == ePoint.Position)
				return m_clone.Position;

			return m_owner.Position;
		}

		protected void SetPoint(ePoint pointid, UnitPoint point, MText text)
		{
			if (pointid == ePoint.Position)
				text.Position = point;

		}
	}
	public class MText : DrawObjectBase, IDrawObject, ISerialize
	{


        private UnitPoint position;
        [XmlSerializable]
        public UnitPoint Position
        {
            get { return position; }
            set { position = value; }
           
        }

        [XmlSerializable]
        public string Value { get; set; }
        [XmlSerializable]
        public Font TextFont { get; set; }
        public Font OriginalFont { get; set; }
        public float Rotation { get; set; }
        
        public static string ObjectType
		{
			get { return "mtext"; }
		}
		public MText()
		{
		}
		public MText(netDxf.Entities.MText text)
		{
			Position = new UnitPoint(text.Position.X,text.Position.Y);
            Value = text.Value;
            TextFont = new Font(text.Style.FontFamilyName, (float)text.Style.Height);
            OriginalFont = TextFont;
            Rotation = (float)text.Rotation;
			Selected = false;
		}
		public override void InitializeFromModel(UnitPoint point, DrawingLayer layer, ISnapPoint snap)
		{
            Position  = point;
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
		public virtual void Copy(MText acopy)
		{
			base.Copy(acopy);
			Position = acopy.Position;
            Value = acopy.Value;
            TextFont = acopy.TextFont;
            OriginalFont = acopy.OriginalFont;
			Selected = acopy.Selected;
		}
		#region IDrawObject Members
		public virtual string Id
		{
			get { return ObjectType; }
		}
		public virtual IDrawObject Clone()
		{
			MText l = new MText();
			l.Copy(this);
			return l;
		}

        public RectangleF GetBoundingRect(ICanvas canvas)
		{
            SizeF textsize = TextRenderer.MeasureText(this.Value, this.OriginalFont);
            PointF dpi = PointF.Empty;
            dpi.X = canvas.Graphics.DpiX;
            dpi.Y = canvas.Graphics.DpiY;

            textsize = new SizeF((float)((textsize.Width) / dpi.X), (float)((textsize.Height) / dpi.Y));
            PointF textpos = new PointF((float)this.Position.X, (float)this.Position.Y);


            return new RectangleF(textpos,textsize );
		}


		public bool PointInObject(ICanvas canvas, UnitPoint point)
		{
            //Calculate threshold
			float thWidth = ThresholdWidth(canvas, Width);



            //Calculates the rectangle that would surround a text item.
            //textpos = new PointF(textpos.X * canvas.DataModel.Zoom, textpos.Y * canvas.DataModel.Zoom);
            RectangleF boundingRectangle = GetBoundingRect(canvas);
           // UnitPoint[] rotrec = HitUtil.RotateRectangle(boundingRectangle, new UnitPoint(boundingRectangle.Location.X, boundingRectangle.Location.Y), this.Rotation);




           // return HitUtil.IsPointInPolygon(new UnitPoint(point.X, point.Y), rotrec);

             return HitUtil.IsPointInRectangle(point, boundingRectangle);
        }
		public bool ObjectInShape(ICanvas canvas, List<PointF> shape, bool anyPoint)
		{
            RectangleF rect = Utils.GetRectangleFromPoints(shape);
            RectangleF boundingrect = GetBoundingRect(canvas);
			if (anyPoint)
				return HitUtil.RectanglesIntersect(boundingrect, rect);
			return rect.Contains((float) position.X,(float)position.Y );
		}
		public virtual void Draw(ICanvas canvas, RectangleF unitrect)
		{
			Color color = Color;
			Pen pen = canvas.CreatePen(color, Width);
			pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
			pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;


                TextFont = new Font(TextFont.FontFamily, OriginalFont.Size * canvas.DataModel.Zoom);


            canvas.DrawMultilineText(canvas, pen, Position, Value, TextFont, Rotation);

            if (Highlighted)
                //TODO USE DIFFENT BRUSH FOR HIGHLIGHT
                canvas.DrawMultilineText(canvas, DrawUtils.SelectedPen, Position, Value, TextFont, Rotation);
            if (Selected)
			{
                canvas.DrawMultilineText(canvas, DrawUtils.SelectedPen, Position, Value, TextFont, Rotation);
                if (Position.IsEmpty == false)
					DrawUtils.DrawNode(canvas, Position);

			}
		}
		public virtual void OnMouseMove(ICanvas canvas, UnitPoint point)
		{
			if (Control.ModifierKeys == Keys.Control)
				point = HitUtil.OrthoPointD(Position, point, 45);
			Position = point;
		}
		public virtual eDrawObjectMouseDown OnMouseDown(ICanvas canvas, UnitPoint point, ISnapPoint snappoint)
		{
			Selected = false;
			if (snappoint is PerpendicularSnapPoint && snappoint.Owner is Line)
			{
				Line src = snappoint.Owner as Line;
                Position = HitUtil.NearestPointOnLine(src.P1, src.P2, Position, true);
				return eDrawObjectMouseDown.DoneRepeat;
			}
			if (snappoint is PerpendicularSnapPoint && snappoint.Owner is Arc)
			{
				Arc src = snappoint.Owner as Arc;
                Position = HitUtil.NearestPointOnCircle(src.Center, src.Radius, Position, 0);
				return eDrawObjectMouseDown.DoneRepeat;
			}
			if (Control.ModifierKeys == Keys.Control)
				point = HitUtil.OrthoPointD(Position, point, 45);
            Position = point;
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
			get { return Position; }
		}
		public INodePoint NodePoint(ICanvas canvas, UnitPoint point)
		{
			float thWidth = ThresholdWidth(canvas, Width);
			if (HitUtil.CircleHitPoint(Position, thWidth, point))
				return new NodePointMText(this, NodePointMText.ePoint.Position);
			return null;
		}
	
		public void Move(UnitPoint offset)
		{
            Position = new UnitPoint(Position.X + offset.X,Position.Y+offset.Y);

		}
		public string GetInfoAsString()
		{
            return string.Format("mtext@{0}",



                Value);
		}
		#endregion
		#region ISerialize
		public void GetObjectData(XmlWriter wr)
		{
			wr.WriteStartElement("mtext");
			XmlUtil.WriteProperties(this, wr);
			wr.WriteEndElement();
		}
		public void AfterSerializedIn()
		{
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
                        if (HitUtil.CircleHitPoint(Position, thWidth, point))
                            return new VertextSnapPoint(canvas, this, Position);
                        if (HitUtil.CircleHitPoint(Position, thWidth, point))
                            return new VertextSnapPoint(canvas, this, Position);
                    }
                    if (snaptype == typeof(MidpointSnapPoint))
                    {
                       // Line otherline = Utils.FindObjectTypeInList(this, otherobjs, typeof(Line)) as Line;
                       // UnitPoint p = otherline.MidPoint(canvas, Position, Position, point);
                      // if (p != UnitPoint.Empty)
                      // return new MidpointSnapPoint(canvas, this, p);
                    }
                    if (snaptype == typeof(IntersectSnapPoint))
                    {
                        Line otherline = Utils.FindObjectTypeInList(this, otherobjs, typeof(Line)) as Line;
                        if (otherline == null)
                            continue;
                        UnitPoint p = HitUtil.LinesIntersectPoint(Position, Position, otherline.P1, otherline.P2    );
                        if (p != UnitPoint.Empty)
                            return new IntersectSnapPoint(canvas, this, p);
                    }
                }
                return null;
            }

            if (usersnaptype == typeof(MidpointSnapPoint))
                return new MidpointSnapPoint(canvas, this, HitUtil.LineMidpoint(Position, Position));
            if (usersnaptype == typeof(IntersectSnapPoint))
            {
                Line otherline = Utils.FindObjectTypeInList(this, otherobjs, typeof(Line)) as Line;
                if (otherline == null)
                    return null;
                UnitPoint p = HitUtil.LinesIntersectPoint(Position, Position, otherline.P1, otherline.P2);
                if (p != UnitPoint.Empty)
                    return new IntersectSnapPoint(canvas, this, p);
            }
            if (usersnaptype == typeof(VertextSnapPoint))
            {
                double d1 = HitUtil.Distance(point, Position);
                double d2 = HitUtil.Distance(point, Position);
                if (d1 <= d2)
                    return new VertextSnapPoint(canvas, this, Position);
                return new VertextSnapPoint(canvas, this, Position);
            }
            if (usersnaptype == typeof(NearestSnapPoint))
            {
                UnitPoint p = HitUtil.NearestPointOnLine(Position, Position, point);
                if (p != UnitPoint.Empty)
                    return new NearestSnapPoint(canvas, this, p);
            }
            if (usersnaptype == typeof(PerpendicularSnapPoint))
            {
                UnitPoint p = HitUtil.NearestPointOnLine(Position, Position, point);
                if (p != UnitPoint.Empty)
                    return new PerpendicularSnapPoint(canvas, this, p);
            }
            return null;
        }
        #endregion


    }
	class MTextEdit : MText, IObjectEditInstance
	{
		protected PerpendicularSnapPoint m_perSnap;
		protected TangentSnapPoint m_tanSnap;
		protected bool m_tanReverse = false;
		protected bool m_singleLineSegment = true;

		public override string Id
		{
			get
			{
				
					return "mtext";
				
			}
		}
        public MTextEdit()
            : base()
        {
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
				Position = HitUtil.NearestPointOnLine(src.P1, src.P2, point, true);
                Position = point;
			}
			if (m_perSnap.Owner is IArc)
			{
				IArc src = m_perSnap.Owner as IArc;
				Position = HitUtil.NearestPointOnCircle(src.Center, src.Radius, point, 0);
                Position = point;
			}
		}
		protected virtual void MouseMoveTangent(ICanvas canvas, UnitPoint point)
		{
			if (m_tanSnap.Owner is IArc)
			{
				IArc src = m_tanSnap.Owner as IArc;
				Position = HitUtil.TangentPointOnCircle(src.Center, src.Radius, point, m_tanReverse);
                Position = point;
				if (Position == UnitPoint.Empty)
                    Position = Position = src.Center;
			}
		}
		protected virtual void ReverseTangent(ICanvas canvas)
		{
			m_tanReverse = !m_tanReverse;
			MouseMoveTangent(canvas, Position);
			canvas.Invalidate();
		}

		public new void Copy(MTextEdit acopy)
		{
			base.Copy(acopy);
			m_perSnap = acopy.m_perSnap;
			m_tanSnap = acopy.m_tanSnap;
			m_tanReverse = acopy.m_tanReverse;
			m_singleLineSegment = acopy.m_singleLineSegment;
		}
		public override IDrawObject Clone()
		{
			MTextEdit l = new MTextEdit();
			l.Copy(this);
			return l;
		}

		#region IObjectEditInstance
		public IDrawObject GetDrawObject()
		{
			return new MText();
		}
		#endregion
	}
}
