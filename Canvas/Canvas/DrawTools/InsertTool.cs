//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows.Forms;
//
//namespace Canvas.DrawTools
//{
//	class NodePointInsert : INodePoint
//	{
//		public enum ePoint
//		{
//			P1,
//			P2,
//		}
//		static bool m_angleLocked = false;
//		Insert m_owner;
//		Insert m_clone;
//		UnitPoint m_originalPoint;
//		UnitPoint m_endPoint;
//		ePoint m_pointId;
//		public NodePointInsert(Insert owner, ePoint id)
//		{
//			m_owner = owner;
//			m_clone = m_owner.Clone() as Insert;
//			m_pointId = id;
//			m_originalPoint = m_clone.Position;
//		}
//		#region INodePoint Members
//		public IDrawObject GetClone()
//		{
//			return m_clone;
//		}
//		public IDrawObject GetOriginal()
//		{
//			return m_owner;
//		}
//		public void SetPosition(UnitPoint pos)
//		{
//		
//			SetPoint( pos, m_clone);
//		}
//		public void Finish()
//		{
//			
//			m_owner.Position = m_clone.Position;
//		
//			m_clone = null;
//		}
//		public void Cancel()
//		{
//		}
//		public void Undo()
//		{
//			SetPoint(m_originalPoint, m_owner);
//		}
//		public void Redo()
//		{
//			SetPoint(m_endPoint, m_owner);
//		}
//		public void OnKeyDown(ICanvas canvas, KeyEventArgs e)
//		{
//			if (e.KeyCode == Keys.L)
//			{
//				m_angleLocked = !m_angleLocked;
//				e.Handled = true;
//			}
//		}
//		#endregion
//
//
//		protected void SetPoint( UnitPoint point, Insert line)
//		{
//			line.Position = point;
//
//		}
//	}
//	class Insert: netDxf.Entities.Insert, IDrawObject
//    {
//
//		public Insert(netDxf.Blocks.Block block)
//			:base(block)
//		{
//			
//		}
//
//		public virtual string Id
//		{
//			get { return this.Handle; }
//		}
//
//		public UnitPoint RepeatStartingPoint
//		{
//			get { return (UnitPoint)this.Position; }
//		}
//
//		public void Draw(ICanvas canvas, RectangleF unitrect)
//        {
//			Color color = Color;
//			Pen pen = canvas.CreatePen(color, Width);
//			pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
//			pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
//
//
//
//
//
//
//
//
//
//
//			if (!Highlighted && !Selected)
//			{
//				canvas.DrawText(canvas, pen, Position, Value, TextFont, Rotation);
//
//			}
//			if (Highlighted)
//			{
//				//TODO USE DIFFENT BRUSH FOR HIGHLIGHT
//				canvas.DrawText(canvas, DrawUtils.SelectedPen, Position, Value, TextFont, Rotation);
//
//			}
//			if (Selected)
//			{
//				canvas.DrawText(canvas, DrawUtils.SelectedPen, Position, Value, TextFont, Rotation);
//
//				if (Position.IsEmpty == false)
//					DrawUtils.DrawNode(canvas, Position);
//
//			}
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//		}
//
//		public RectangleF GetBoundingRect(ICanvas canvas)
//        {
//            throw new NotImplementedException();
//        }
//
//        public string GetInfoAsString()
//        {
//            throw new NotImplementedException();
//        }
//
//        public void Move(UnitPoint offset)
//        {
//            throw new NotImplementedException();
//        }
//
//        public INodePoint NodePoint(ICanvas canvas, UnitPoint point)
//        {
//            throw new NotImplementedException();
//        }
//
//        public bool ObjectInShape(ICanvas canvas, List<PointF> shape, bool anyPoint)
//        {
//            throw new NotImplementedException();
//        }
//
//        public void OnKeyDown(ICanvas canvas, KeyEventArgs e)
//        {
//            throw new NotImplementedException();
//        }
//
//        public eDrawObjectMouseDown OnMouseDown(ICanvas canvas, UnitPoint point, ISnapPoint snappoint)
//        {
//            throw new NotImplementedException();
//        }
//
//        public void OnMouseMove(ICanvas canvas, UnitPoint point)
//        {
//            throw new NotImplementedException();
//        }
//
//        public void OnMouseUp(ICanvas canvas, UnitPoint point, ISnapPoint snappoint)
//        {
//            throw new NotImplementedException();
//        }
//
//        public bool PointInObject(ICanvas canvas, UnitPoint point)
//        {
//            throw new NotImplementedException();
//        }
//
//        public ISnapPoint SnapPoint(ICanvas canvas, UnitPoint point, List<IDrawObject> otherobj, Type[] runningsnaptypes, Type usersnaptype)
//        {
//            throw new NotImplementedException();
//        }
//
//        IDrawObject IDrawObject.Clone()
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
