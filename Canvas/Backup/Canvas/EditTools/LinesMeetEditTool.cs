using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Canvas.EditTools
{
	class LinePoints
	{
		DrawTools.Line	m_line;
		UnitPoint		m_p1;
		UnitPoint		m_p2;
		public UnitPoint MousePoint;
		public DrawTools.Line Line
		{
			get { return m_line; }
		}
		public void SetLine(DrawTools.Line l)
		{
			m_line = l;
			m_p1 = l.P1;
			m_p2 = l.P2;
		}
		public void ResetLine()
		{
			m_line.P1 = m_p1;
			m_line.P2 = m_p2;
		}
		public void SetNewPoints(DrawTools.Line l, UnitPoint hitpoint, UnitPoint intersectpoint)
		{
			SetLine(l);
			double hitToVp1 = HitUtil.Distance(hitpoint, l.P1); // hit point to vertex point
			double ispToVp1 = HitUtil.Distance(intersectpoint, l.P1); // intersect point to vertex point
			// if hit is closer than intersect point, then keep this point and adjust the other
			if (hitToVp1 <= ispToVp1)
				m_p2 = intersectpoint;
			else
				m_p1 = intersectpoint;
			ResetLine();
		}
	}
	class LinesMeetEditTool : IEditTool
	{
		IEditToolOwner m_owner;
		public LinesMeetEditTool(IEditToolOwner owner)
		{
			m_owner = owner;
			SetHint("Select first line");
		}
		void SetHint(string text)
		{
			if (m_owner != null)
			{
				if (text.Length > 0)
					m_owner.SetHint("Lines Meet: " + text);
				else
					m_owner.SetHint("");
			}
		}
		public IEditTool Clone()
		{
			LinesMeetEditTool t = new LinesMeetEditTool(m_owner);
			// nothing that needs to be cloned
			return t;
		}
		LinePoints m_l1Original = new LinePoints();
		LinePoints m_l2Original = new LinePoints();
		LinePoints m_l1NewPoint = new LinePoints();
		LinePoints m_l2NewPoint = new LinePoints();

		public bool SupportSelection 
		{ 
			get { return false; } 
		}
		public void SetHitObjects(UnitPoint point, List<IDrawObject> list)
		{
		}
		public void OnMouseMove(ICanvas canvas, UnitPoint point)
		{
		}
		public eDrawObjectMouseDown OnMouseDown(ICanvas canvas, UnitPoint point, ISnapPoint snappoint)
		{
			List<IDrawObject> items = canvas.DataModel.GetHitObjects(canvas, point);
			DrawTools.Line line = null;
			// find first line
			foreach (IDrawObject item in items)
			{
				if (item is DrawTools.Line)
				{
					line = item as DrawTools.Line;
					if (line != m_l1Original.Line)
						break;
				}
			}
			if (line == null)
			{
				if (m_l1Original.Line == null)
					SetHint("No line selected. Select first line");
				else
					SetHint("No line selected. Select second line");
				return eDrawObjectMouseDown.Continue;
			}
			if (m_l1Original.Line == null)
			{
				line.Highlighted = true;
				m_l1Original.SetLine(line);
				m_l1Original.MousePoint = point;
				SetHint("Select second line");
				return eDrawObjectMouseDown.Continue;
			}
			if (m_l2Original.Line == null)
			{
				line.Highlighted = true;
				m_l2Original.SetLine(line);
				m_l2Original.MousePoint = point;

				UnitPoint intersectpoint = HitUtil.LinesIntersectPoint(
					m_l1Original.Line.P1, 
					m_l1Original.Line.P2, 
					m_l2Original.Line.P1, 
					m_l2Original.Line.P2);
				
				// if lines do not intersect then extend lines to intersect point
				if (intersectpoint == UnitPoint.Empty)
				{
					UnitPoint apprarentISPoint = HitUtil.FindApparentIntersectPoint(m_l1Original.Line.P1, m_l1Original.Line.P2, m_l2Original.Line.P1, m_l2Original.Line.P2);
					if (apprarentISPoint == UnitPoint.Empty)
						return eDrawObjectMouseDown.Done;
					m_l1Original.Line.ExtendLineToPoint(apprarentISPoint);
					m_l2Original.Line.ExtendLineToPoint(apprarentISPoint);
					m_l1NewPoint.SetLine(m_l1Original.Line);
					m_l2NewPoint.SetLine(m_l2Original.Line);
					canvas.DataModel.AfterEditObjects(this);
					return eDrawObjectMouseDown.Done;
				}

				m_l1NewPoint.SetNewPoints(m_l1Original.Line, m_l1Original.MousePoint, intersectpoint);
				m_l2NewPoint.SetNewPoints(m_l2Original.Line, m_l2Original.MousePoint, intersectpoint);
				canvas.DataModel.AfterEditObjects(this);
				return eDrawObjectMouseDown.Done;
			}
			return eDrawObjectMouseDown.Done;
		}
		public void OnMouseUp(ICanvas canvas, UnitPoint point, ISnapPoint snappoint)
		{
		}
		public void OnKeyDown(ICanvas canvas, KeyEventArgs e)
		{
		}
		public void Finished()
		{
			SetHint("");
			if (m_l1Original.Line != null)
				m_l1Original.Line.Highlighted = false;
			if (m_l2Original.Line != null)
				m_l2Original.Line.Highlighted = false;
		}
		public void Undo()
		{
			m_l1Original.ResetLine();
			m_l2Original.ResetLine();
		}
		public void Redo()
		{
			m_l1NewPoint.ResetLine();
			m_l2NewPoint.ResetLine();
		}
	}
}
