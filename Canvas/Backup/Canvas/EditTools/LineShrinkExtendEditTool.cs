using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Canvas.EditTools
{
	class LineShrinkExtendEditTool : IEditTool
	{
		IEditToolOwner m_owner;
		public LineShrinkExtendEditTool(IEditToolOwner owner)
		{
			m_owner = owner;
			SetHint("Select line to extend");
		}
		void SetHint(string text)
		{
			if (m_owner != null)
				if (text.Length > 0)
					m_owner.SetHint("Extend Lines: " + text);
				else
					m_owner.SetHint("");
		}
		public IEditTool Clone()
		{
			LineShrinkExtendEditTool t = new LineShrinkExtendEditTool(m_owner);
			// nothing that needs to be cloned
			return t;
		}
		Dictionary<DrawTools.Line, LinePoints> m_originalLines = new Dictionary<DrawTools.Line, LinePoints>();
		Dictionary<DrawTools.Line, LinePoints> m_modifiedLines = new Dictionary<DrawTools.Line, LinePoints>();

		public void OnMouseMove(ICanvas canvas, UnitPoint point)
		{
		}

		public bool SupportSelection
		{
			get { return true; }
		}

		void ClearAll()
		{
			foreach (LinePoints p in m_originalLines.Values)
				p.Line.Highlighted = false;
			m_originalLines.Clear();
		}
		void AddLine(UnitPoint point, DrawTools.Line line)
		{
			if (m_originalLines.ContainsKey(line) == false)
			{
				line.Highlighted = true;
				LinePoints lp = new LinePoints();
				lp.SetLine(line);
				lp.MousePoint = point;
				m_originalLines.Add(line, lp);
			}
		}
		void RemoveLine(DrawTools.Line line)
		{
			if (m_originalLines.ContainsKey(line))
			{
				m_originalLines[line].Line.Highlighted = false;
				m_originalLines.Remove(line);
			}
		}
		public void SetHitObjects(UnitPoint point, List<IDrawObject> list)
		{
			// called when obj are selected from selection rectangle
			// if list is empty, do nothing
			// if no shift or ctrl, then replace selection with items from list
			// if shift then append
			// if ctrl then toggle
			if (list == null)
				return;
			List<DrawTools.Line> lines = GetLines(list);
			if (lines.Count == 0)
				return;

			bool shift = Control.ModifierKeys == Keys.Shift;
			bool ctrl = Control.ModifierKeys == Keys.Control;

			if (shift == false && ctrl == false)
				ClearAll();

			if (ctrl == false) // append all lines, either no-key or shift
			{
				foreach (DrawTools.Line line in lines)
					AddLine(point, line);
			}
			if (ctrl)
			{
				foreach (DrawTools.Line line in lines)
				{
					if (m_originalLines.ContainsKey(line))
						RemoveLine(line);
					else
						AddLine(point, line);
				}
			}
			SetSelectHint();
		}
		void SetSelectHint()
		{
			if (m_originalLines.Count == 0)
				SetHint("Select line to extend");
			else
				SetHint("Select Line to extend line(s) to, or [Ctrl+click] to extend more lines");
		}
		List<DrawTools.Line> GetLines(List<IDrawObject> objs)
		{
			List<DrawTools.Line> lines = new List<Canvas.DrawTools.Line>();
			foreach (IDrawObject obj in objs)
			{
				if (obj is DrawTools.Line)
					lines.Add((DrawTools.Line)obj);
			}
			return lines;
		}
		public eDrawObjectMouseDown OnMouseDown(ICanvas canvas, UnitPoint point, ISnapPoint snappoint)
		{
			List<IDrawObject> drawitems = canvas.DataModel.GetHitObjects(canvas, point);
			List<DrawTools.Line> lines = GetLines(drawitems);
			
			// add to source lines
			if (m_originalLines.Count == 0 || Control.ModifierKeys == Keys.Shift)
			{
				foreach (DrawTools.Line line in lines)
					AddLine(point, line);
				SetSelectHint();
				return eDrawObjectMouseDown.Continue;
			}
			if (m_originalLines.Count == 0 || Control.ModifierKeys == Keys.Control)
			{
				foreach (DrawTools.Line line in lines)
				{
					if (m_originalLines.ContainsKey(line))
						RemoveLine(line);
					else
						AddLine(point, line);
				}
				SetSelectHint();
				return eDrawObjectMouseDown.Continue;
			}

			if (drawitems.Count == 0)
				return eDrawObjectMouseDown.Continue;

			// all lines have been added, now find edge to where to extend
			if (drawitems[0] is DrawTools.Line)
			{
				DrawTools.Line edge = (DrawTools.Line)drawitems[0];
				bool modified = false;
				foreach (LinePoints originalLp in m_originalLines.Values)
				{
					UnitPoint intersectpoint = HitUtil.LinesIntersectPoint(edge.P1, edge.P2, originalLp.Line.P1, originalLp.Line.P2);
					// lines intersect so shrink line
					if (intersectpoint != UnitPoint.Empty)
					{
						LinePoints lp = new LinePoints();
						lp.SetLine(originalLp.Line);
						lp.MousePoint = originalLp.MousePoint;
						m_modifiedLines.Add(lp.Line, lp);
						lp.SetNewPoints(lp.Line, lp.MousePoint, intersectpoint);
						modified = true;
						continue;
					}
					// lines do not intersect, find apparent intersect point on existing edge line
					if (intersectpoint == UnitPoint.Empty)
					{
						UnitPoint apprarentISPoint = HitUtil.FindApparentIntersectPoint(
							edge.P1,
							edge.P2,
							originalLp.Line.P1,
							originalLp.Line.P2,
							false,
							true);
						if (apprarentISPoint == UnitPoint.Empty)
							continue;

						modified = true;
						originalLp.Line.ExtendLineToPoint(apprarentISPoint);

						LinePoints lp = new LinePoints();
						lp.SetLine(originalLp.Line);
						lp.MousePoint = point;
						m_modifiedLines.Add(lp.Line, lp);
					}
				}
				if (modified)
					canvas.DataModel.AfterEditObjects(this);
				return eDrawObjectMouseDown.Done;
			}
			if (drawitems[0] is DrawTools.Arc)
			{
				DrawTools.Arc edge = (DrawTools.Arc)drawitems[0];
				foreach (LinePoints originalLp in m_originalLines.Values)
				{
				}
				bool modified = false;
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
			foreach (LinePoints originalLp in m_originalLines.Values)
				originalLp.Line.Highlighted = false;
		}
		public void Undo()
		{
			foreach (LinePoints lp in m_originalLines.Values)
				lp.ResetLine();
		}
		public void Redo()
		{
			foreach (LinePoints lp in m_modifiedLines.Values)
				lp.ResetLine();
		}
	}
}