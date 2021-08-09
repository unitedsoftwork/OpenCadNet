using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Xml;

namespace Canvas
{
	public class GridLayer : ICanvasLayer, ISerialize
	{
		public enum eStyle
		{
			Dots,
			Lines,
		}
		public SizeF m_spacing = new SizeF(1f, 1f); // 12"
		private bool m_enabled = true;
		private int m_minSize = 15;
		private eStyle m_gridStyle = eStyle.Lines;
		private Color m_color = Color.FromArgb(50, Color.Gray);
		[XmlSerializable]
		public SizeF Spacing
		{
			get { return m_spacing; }
			set { m_spacing = value; }
		}
		[XmlSerializable]
		public int MinSize
		{
			get { return m_minSize; }
			set { m_minSize = value; }
		}
		[XmlSerializable]
		public eStyle GridStyle
		{
			get { return m_gridStyle; }
			set { m_gridStyle = value; }
		}
		[XmlSerializable]
		public Color Color
		{
			get { return m_color; }
			set { m_color = value; }
		}

		public void Copy(GridLayer acopy)
		{
			m_enabled = acopy.m_enabled;
			m_spacing = acopy.m_spacing;
			m_minSize = acopy.m_minSize;
			m_gridStyle = acopy.m_gridStyle;
			m_color = acopy.m_color;
		}
		#region ICanvasLayer Members
		public void Draw(ICanvas canvas, RectangleF unitrect)
		{
			if (Enabled == false)
				return;
			float gridX = Spacing.Width;
			float gridY = Spacing.Height;
			float gridscreensizeX = canvas.ToScreen(gridX);
			float gridscreensizeY = canvas.ToScreen(gridY);
			if (gridscreensizeX < MinSize || gridscreensizeY < MinSize)
				return;

			PointF leftpoint = unitrect.Location;
			PointF rightpoint = ScreenUtils.RightPoint(canvas, unitrect);

			float left = (float)Math.Round(leftpoint.X / gridX) * gridX;
			float top = unitrect.Height + unitrect.Y;
			float right = rightpoint.X;
			float bottom = (float)Math.Round(leftpoint.Y / gridY) * gridY;

			if (GridStyle == eStyle.Dots)
			{
				GDI gdi = new GDI();
				gdi.BeginGDI(canvas.Graphics);
				for (float x = left; x <= right; x += gridX)
				{
					for (float y = bottom; y <= top; y += gridY)
					{
						PointF p1 = canvas.ToScreen(new UnitPoint(x, y));
						gdi.SetPixel((int)p1.X, (int)p1.Y, m_color.ToArgb());
					}
				}
				gdi.EndGDI();
			}
			if (GridStyle == eStyle.Lines)
			{
				Pen pen = new Pen(m_color);
				GraphicsPath path = new GraphicsPath();

				// draw vertical lines
				while (left < right)
				{
					PointF p1 = canvas.ToScreen(new UnitPoint(left, leftpoint.Y));
					PointF p2 = canvas.ToScreen(new UnitPoint(left, rightpoint.Y));
					path.AddLine(p1, p2);
					path.CloseFigure();
					left += gridX;
				}

				// draw horizontal lines
				while (bottom < top)
				{
					PointF p1 = canvas.ToScreen(new UnitPoint(leftpoint.X, bottom));
					PointF p2 = canvas.ToScreen(new UnitPoint(rightpoint.X, bottom));
					path.AddLine(p1, p2);
					path.CloseFigure();
					bottom += gridY;
				}
				canvas.Graphics.DrawPath(pen, path);
			}
		}
		public string Id
		{
			get { return "grid"; }
		}
		public ISnapPoint SnapPoint(ICanvas canvas, UnitPoint point, List<IDrawObject> otherobj)
		{
			if (Enabled == false)
				return null;
			UnitPoint snappoint = new UnitPoint();
			UnitPoint mousepoint = point;
			float gridX = Spacing.Width;
			float gridY = Spacing.Height;
			snappoint.X = (float)(Math.Round(mousepoint.X / gridX)) * gridX;
			snappoint.Y = (float)(Math.Round(mousepoint.Y / gridY)) * gridY;
			double threshold = canvas.ToUnit(/*ThresholdPixel*/6);
			if ((snappoint.X < point.X - threshold) || (snappoint.X > point.X + threshold))
				return null;
			if ((snappoint.Y < point.Y - threshold) || (snappoint.Y > point.Y + threshold))
				return null;
			return new GridSnapPoint(canvas, snappoint);
		}
		public IEnumerable<IDrawObject> Objects
		{
			get { return null; }
		}
		[XmlSerializable]
		public bool Enabled
		{
			get { return m_enabled; }
			set { m_enabled = value; }
		}
		public bool Visible
		{
			get { return true; }
		}
		#endregion
		#region ISerialize
		public void GetObjectData(XmlWriter wr)
		{
			wr.WriteStartElement("gridlayer");
			XmlUtil.WriteProperties(this, wr);
			wr.WriteEndElement();
		}
		public void AfterSerializedIn()
		{
		}
		#endregion
	}
}
