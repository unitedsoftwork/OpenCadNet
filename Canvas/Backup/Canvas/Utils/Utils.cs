using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Xml;

namespace Canvas
{
	public struct UnitPoint
	{
		public static UnitPoint Empty;
		public static bool operator !=(UnitPoint left, UnitPoint right)
		{
			return !(left == right);
		}
		public static bool operator ==(UnitPoint left, UnitPoint right)
		{
			if (left.X == right.X)
				return left.Y == right.Y;
			if (left.IsEmpty && right.IsEmpty) // after changing Empty to use NaN this extra check is required
				return true;
			return false;
		}
		public static UnitPoint operator + (UnitPoint left, UnitPoint right)
		{
			left.X += right.X;
			left.Y += right.Y;
			return left;
		}
		double m_x;
		double m_y;
		static UnitPoint()
		{
			Empty = new UnitPoint();
			Empty.m_x = double.NaN;
			Empty.m_y = double.NaN;
		}
		public UnitPoint(double x, double y)
		{
			m_x = x;
			m_y = y;
		}
		public UnitPoint(PointF p)
		{
			m_x = p.X;
			m_y = p.Y;
		}
		public bool IsEmpty
		{
			get 
			{
				return double.IsNaN(X) && double.IsNaN(Y);
			}
		}
		public double X
		{
			get { return m_x; }
			set { m_x = value; }
		}
		public double Y
		{
			get { return m_y; }
			set { m_y = value; }
		}
		public PointF Point
		{
			get { return new PointF((float)m_x, (float)m_y); }
		}
		public override string ToString()
		{
			return string.Format("{{X={0}, Y={1}}}", XmlConvert.ToString(Math.Round(X,8)), XmlConvert.ToString(Math.Round(Y,8)));
		}
		public override bool Equals(object obj)
		{
			if (obj is UnitPoint)
				return (this == (UnitPoint)obj);
			return false;
		}
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
		public string PosAsString()
		{
			return string.Format("[{0:f4}, {1:f4}]", X, Y);
		}
	}
	public class ScreenUtils
	{
		public static PointF RightPoint(ICanvas canvas, RectangleF unitrect)
		{
			PointF leftpoint = unitrect.Location;
			float x = leftpoint.X + unitrect.Width;
			float y = leftpoint.Y + unitrect.Height;
			return new PointF(x, y);
		}
		public static RectangleF ToScreen(ICanvas canvas, RectangleF unitrect)
		{
			RectangleF r = new RectangleF();
			r.Location = canvas.ToScreen(new UnitPoint(unitrect.Location));
			r.Width = (float)Math.Round(canvas.ToScreen(unitrect.Width));
			r.Height = (float)Math.Round(canvas.ToScreen(unitrect.Height));
			return r;
		}
		public static RectangleF ToUnit(ICanvas canvas, Rectangle screenrect)
		{
			UnitPoint point = canvas.ToUnit(screenrect.Location);
			SizeF size = new SizeF((float)canvas.ToUnit(screenrect.Width), (float)canvas.ToUnit(screenrect.Height));
			RectangleF unitrect = new RectangleF(point.Point, size);
			return unitrect;
		}
		public static RectangleF ToScreenNormalized(ICanvas canvas, RectangleF unitrect)
		{
			RectangleF r = ToScreen(canvas, unitrect);
			r.Y = r.Y - r.Height;
			return r;
		}
		public static RectangleF ToUnitNormalized(ICanvas canvas, Rectangle screenrect)
		{
			UnitPoint point = canvas.ToUnit(screenrect.Location);
			SizeF size = new SizeF((float)canvas.ToUnit(screenrect.Width), (float)canvas.ToUnit(screenrect.Height));
			RectangleF unitrect = new RectangleF(point.Point, size);
			unitrect.Y = unitrect.Y - unitrect.Height; // 
			return unitrect;
		}
		public static Rectangle ConvertRect(RectangleF r)
		{
			return new Rectangle((int)r.Left, (int)r.Top, (int)r.Width, (int)r.Height);
		}
		public static RectangleF GetRect(UnitPoint p1, UnitPoint p2, double width)
		{
			double x = Math.Min(p1.X, p2.X);
			double y = Math.Min(p1.Y, p2.Y);
			double w = Math.Abs(p1.X - p2.X);
			double h = Math.Abs(p1.Y - p2.Y);
			RectangleF rect = ScreenUtils.GetRect(x, y, w, h);
			rect.Inflate((float)width, (float)width);
			return rect;
		}
		public static RectangleF GetRect(double x, double y, double w, double h)
		{
			return new RectangleF((float)x, (float)y, (float)w, (float)h);
		}
		public static Point ConvertPoint(PointF p)
		{
			return new Point((int)p.X, (int)p.Y);
		}
	}
	public class HitUtil
	{
		public static bool PointInPoint(UnitPoint p, UnitPoint tp, float tpThresHold)
		{
			if (p.IsEmpty || tp.IsEmpty)
				return false;
			if ((p.X < tp.X - tpThresHold) || (p.X > tp.X + tpThresHold))
				return false;
			if ((p.Y < tp.Y - tpThresHold) || (p.Y > tp.Y + tpThresHold))
				return false;
			return true;
		}
		
		public static double Distance(UnitPoint p1, UnitPoint p2)
		{
			return Distance(p1, p2, true);
		}
		public static double Distance(UnitPoint p1, UnitPoint p2, bool abs)
		{
			double dx = p1.X - p2.X;
			double dy = p1.Y - p2.Y;
			if (abs)
			{
				dx = Math.Abs(dx);
				dy = Math.Abs(dy);
			}
			if (dx == 0)
				return dy;
			if (dy == 0)
				return dx;
			return Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));
		}
		public static double RadiansToDegrees(double radians)
		{
			return radians * (180 / Math.PI);
		}
		public static double DegressToRadians(double degrees)
		{
			return degrees * (Math.PI / 180);
		}

		public static RectangleF CircleBoundingRect(UnitPoint center, float radius)
		{
			RectangleF r = new RectangleF(center.Point, new SizeF(0, 0));
			r.Inflate(radius, radius);
			return r;
		}
		public static bool CircleHitPoint(UnitPoint center, float radius, UnitPoint hitpoint)
		{
			// check bounding rect, this is faster than creating a new rectangle and call r.Contains
			double leftPoint = center.X - radius;
			double rightPoint = center.X + radius;
			if (hitpoint.X < leftPoint || hitpoint.X > rightPoint)
				return false;

			double bottomPoint = center.Y - radius;
			double topPoint = center.Y + radius;
			if (hitpoint.Y < bottomPoint || hitpoint.Y > topPoint)
				return false;

			return true;
		}
		public static bool CircleIntersectWithLine(UnitPoint center, float radius, UnitPoint lp1, UnitPoint lp2)
		{
			// check if both points are inside the circle, in that case the line does not intersect the circle
			if ((Distance(center, lp1) < radius && Distance(center, lp2) < radius))
				return false;

			// find the nearest point from the line to center, if that point is smaller than radius, 
			// then the line does intersect the circle
			UnitPoint np = NearestPointOnLine(lp1, lp2, center);
			double dist = Distance(center, np);
			if (dist <= radius)
				return true;
			return false;
		}
		public static bool IsPointInCircle(UnitPoint center, float radius, UnitPoint testpoint, float halflinewidth)
		{
			double dist = Distance(center, testpoint);
			if ((dist >= radius - halflinewidth) && (dist <= radius + halflinewidth))
				return true;
			return false;
		}
		public static UnitPoint NearestPointOnCircle(UnitPoint center, float radius, UnitPoint testpoint, double roundToAngleD)
		{
			double A = LineAngleR(center, testpoint, DegressToRadians(roundToAngleD));
			double dx = Math.Cos(A) * radius;
			double dy = Math.Sin(A) * radius;
			double dist = Math.Sqrt(Math.Pow(dx,2) + Math.Pow(dy,2));
			return new UnitPoint((float)(center.X + dx), (float)(center.Y + dy));
		}
		public static UnitPoint TangentPointOnCircle(UnitPoint center, float radius, UnitPoint testpoint, bool reverse)
		{
			// c = center, p = testpoint, r = radius
			// tangent point is formed by a rightangled triangle where 'cp' is the hyportenuse 
			// and the 2 legs are 'r' and cr.
			double h = Math.Sqrt(Math.Pow(center.X - testpoint.X, 2) + Math.Pow(center.Y - testpoint.Y, 2));
			double r = radius;
			if (Math.Abs(h) < r)	// point located inside the circle, no tangent point
				return UnitPoint.Empty;

			// A1 is the angle from center to testpoint
			// A2 is the angle from center to testpoint when radius is used as height for rightangled triangle
			// A3 is the remaining angle between 'cp' line (h) and the short leg of the tangent triangle
			double A1 = LineAngleR(center, testpoint, 0);
			double A2 = Math.Asin(r / h);
			double A3 = Math.Acos(r / h);
			
			// The angle for the tangent point is 90 + diff between A1 and A2
			double A = Math.PI/2 + (A1 - A2);
			// if reverse, mirror around A1
			if (reverse)
				A = A1 - A3;
			return LineEndpoint(center, A, radius);
		}
		public static UnitPoint PointOncircle(UnitPoint center, double radius, double angleR)
		{
			double y = center.Y + Math.Sin(angleR) * radius;
			double x = center.X + Math.Cos(angleR) * radius;
			return new UnitPoint((float)x, (float)y);
		}
		public static UnitPoint LineEndpoint(UnitPoint lp1, double angleR, double length)
		{
			double x = Math.Cos(angleR) * length;
			double y = Math.Sin(angleR) * length;
			return new UnitPoint(lp1.X + (float)x, lp1.Y + (float)y);
		}
		public static RectangleF LineBoundingRect(UnitPoint linepoint1, UnitPoint linepoint2, float halflinewidth)
		{
			double x = Math.Min(linepoint1.X, linepoint2.X);
			double y = Math.Min(linepoint1.Y, linepoint2.Y);
			double w = Math.Abs(linepoint1.X - linepoint2.X);
			double h = Math.Abs(linepoint1.Y - linepoint2.Y);
			RectangleF boundingrect = ScreenUtils.GetRect(x, y, w, h);
			boundingrect.Inflate(halflinewidth, halflinewidth);
			return boundingrect;
		}
		public static bool IsPointInLine(UnitPoint linepoint1, UnitPoint linepoint2, UnitPoint testpoint, float halflinewidth)
		{
			UnitPoint p1 = linepoint1;
			UnitPoint p2 = linepoint2;
			UnitPoint p3 = testpoint;

			// check bounding rect, this is faster than creating a new rectangle and call r.Contains
			double lineLeftPoint = Math.Min(p1.X, p2.X) - halflinewidth;
			double lineRightPoint = Math.Max(p1.X, p2.X) + halflinewidth;
			if (testpoint.X < lineLeftPoint || testpoint.X > lineRightPoint)
				return false;

			double lineBottomPoint = Math.Min(p1.Y, p2.Y) - halflinewidth;
			double lineTopPoint = Math.Max(p1.Y, p2.Y) + halflinewidth;
			if (testpoint.Y < lineBottomPoint || testpoint.Y > lineTopPoint)
				return false;

			// then check if it hits the endpoint
			if (CircleHitPoint(p1, halflinewidth, p3))
				return true;
			if (CircleHitPoint(p2, halflinewidth, p3))
				return true;

			if (p1.Y == p2.Y) // line is horizontal
			{
				double min = Math.Min(p1.X, p2.X) - halflinewidth;
				double max = Math.Max(p1.X, p2.X) + halflinewidth;
				if (p3.X >= min && p3.X <= max)
					return true;
				return false;
			}
			if (p1.X == p2.X) // line is vertical
			{
				double min = Math.Min(p1.Y, p2.Y) - halflinewidth;
				double max = Math.Max(p1.Y, p2.Y) + halflinewidth;
				if (p3.Y >= min && p3.Y <= max)
					return true;
				return false;
			}

			// using COS law
			// a^2 = b^2 + c^2 - 2bc COS A
			// A = ACOS ((a^2 - b^2 - c^2) / (-2bc))
			double xdiff = Math.Abs(p2.X - p3.X);
			double ydiff = Math.Abs(p2.Y - p3.Y);
			double aSquare = Math.Pow(xdiff, 2) + Math.Pow(ydiff, 2);
			double a = Math.Sqrt(aSquare);

			xdiff = Math.Abs(p1.X - p2.X);
			ydiff = Math.Abs(p1.Y - p2.Y);
			double bSquare = Math.Pow(xdiff, 2) + Math.Pow(ydiff, 2);
			double b = Math.Sqrt(bSquare);

			xdiff = Math.Abs(p1.X - p3.X);
			ydiff = Math.Abs(p1.Y - p3.Y);
			double cSquare = Math.Pow(xdiff, 2) + Math.Pow(ydiff, 2);
			double c = Math.Sqrt(cSquare);
			double A = Math.Acos(((aSquare - bSquare - cSquare) / (-2 * b * c)));

			// once we have A we can find the height (distance from the line)
			// SIN(A) = (h / c)
			// h = SIN(A) * c;
			double h = Math.Sin(A) * c;

			// now if height is smaller than half linewidth, the hitpoint is within the line
			return h <= halflinewidth;
		}
		private static bool LinesIntersect(UnitPoint lp1, UnitPoint lp2, UnitPoint lp3, UnitPoint lp4, ref double x, ref double y, 
			bool returnpoint, 
			bool extendA,
			bool extendB)
		{
			// http://local.wasp.uwa.edu.au/~pbourke/geometry/lineline2d/
			// line a is given by P1 and P2, point of intersect for line a (Pa) and b (Pb)
			// Pa = P1 + ua ( P2 - P1 )
			// Pb = P3 + ub ( P4 - P3 )

			// ua(x) = ub(x) and ua(y) = ub (y)
			// x1 + ua (x2 - x1) = x3 + ub (x4 - x3)
			// y1 + ua (y2 - y1) = y3 + ub (y4 - y3)

			// ua = ((x4-x3)(y1-y3) - (y4-y3)(x1-x3)) / ((x4-x3)(x2-x1) - (x4-x3)(y2-y1))
			// ub = ((x2-x1)(y1-y3) - (y2-y1)(x1-x3)) / ((y4-y3)(x2-x1) - (x4-x3)(y2-y1))

			// intersect point x = x1 + ua (x2 - x1)
			// intersect point y = y1 + ua (y2 - y1) 
			double x1 = lp1.X;
			double x2 = lp2.X;
			double x3 = lp3.X;
			double x4 = lp4.X;
			double y1 = lp1.Y;
			double y2 = lp2.Y;
			double y3 = lp3.Y;
			double y4 = lp4.Y;

			double denominator = ((y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1));
			if (denominator == 0) // lines are parallel
				return false;
			double numerator_ua = ((x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3));
			double numerator_ub = ((x2 - x1) * (y1 - y3) - (y2 - y1) * (x1 - x3));
			double ua = numerator_ua / denominator;
			double ub = numerator_ub / denominator;
			// if a line is not extended then ua (or ub) must be between 0 and 1
			if (extendA == false)
			{
				if (ua < 0 || ua > 1)
					return false;
			}
			if (extendB == false)
			{
				if (ub < 0 || ub > 1)
					return false;
			}
			if (extendA || extendB) // no need to chck range of ua and ub if check is one on lines 
			{
				x = x1 + ua * (x2 - x1);
				y = y1 + ua * (y2 - y1);
				return true;
			}
			if (ua >= 0 && ua <= 1 && ub >= 0 && ub <= 1)
			{
				if (returnpoint)
				{
					x = x1 + ua * (x2 - x1);
					y = y1 + ua * (y2 - y1);
				}
				return true;
			}
			return false;
		}
		public static bool LinesIntersect(UnitPoint lp1, UnitPoint lp2, UnitPoint lp3, UnitPoint lp4)
		{
			double x = 0;
			double y = 0;
			return LinesIntersect(lp1, lp2, lp3, lp4, ref x, ref y, false, false, false);
		}
		public static UnitPoint LinesIntersectPoint(UnitPoint lp1, UnitPoint lp2, UnitPoint lp3, UnitPoint lp4)
		{
			double x = 0;
			double y = 0;
			if (LinesIntersect(lp1, lp2, lp3, lp4, ref x, ref y, true, false, false))
				return new UnitPoint(x, y);
			return UnitPoint.Empty;
		}
		public static UnitPoint FindApparentIntersectPoint(UnitPoint lp1, UnitPoint lp2, UnitPoint lp3, UnitPoint lp4)
		{
			double x = 0;
			double y = 0;
			if (LinesIntersect(lp1, lp2, lp3, lp4, ref x, ref y, true, true, true))
				return new UnitPoint(x, y);
			return UnitPoint.Empty;
		}
		public static UnitPoint FindApparentIntersectPoint(UnitPoint lp1, UnitPoint lp2, UnitPoint lp3, UnitPoint lp4, bool extendA, bool extendB)
		{
			double x = 0;
			double y = 0;
			if (LinesIntersect(lp1, lp2, lp3, lp4, ref x, ref y, true, extendA, extendB))
				return new UnitPoint(x, y);
			return UnitPoint.Empty;
		}
		public static bool LineIntersectWithRect(UnitPoint lp1, UnitPoint lp2, RectangleF r)
		{
			if (r.Contains(lp1.Point))
				return true;
			if (r.Contains(lp2.Point))
				return true;

			// the rectangle bottom is top in world units and top is bottom!, confused?
			// check left
			UnitPoint p3 = new UnitPoint(r.Left, r.Top);
			UnitPoint p4 = new UnitPoint(r.Left, r.Bottom);
			if (LinesIntersect(lp1, lp2, p3, p4))
				return true;
			// check bottom
			p4.Y = r.Top;
			p4.X = r.Right;
			if (LinesIntersect(lp1, lp2, p3, p4))
				return true;
			// check right
			p3.X = r.Right;
			p3.Y = r.Top;
			p4.X = r.Right;
			p4.Y = r.Bottom;
			if (LinesIntersect(lp1, lp2, p3, p4))
				return true;
			return false;
		}
		public static UnitPoint LineMidpoint(UnitPoint lp1, UnitPoint lp2)
		{
			UnitPoint mid = new UnitPoint();
			mid.X = (lp1.X + lp2.X) / 2;
			mid.Y = (lp1.Y + lp2.Y) / 2;
			return mid;
		}
		public static double LineAngleR(UnitPoint lp1, UnitPoint lp2, double roundToAngleR)
		{
			if (lp1.X == lp2.X)
			{
				if (lp1.Y > lp2.Y)
					return Math.PI * 6/4;
				if (lp1.Y < lp2.Y)
					return Math.PI/2;
				return 0;
			}
			// first find angle A
			double adjacent = lp2.X - lp1.X;
			double opposite = lp2.Y - lp1.Y;
			// find angle A
			double A = Math.Atan(opposite / adjacent);
			if (adjacent < 0)			// 2nd and 3rd quadrant
				A += Math.PI;			// + 180
			if (adjacent > 0 && opposite < 0) // 4th quadrant
				A += Math.PI * 2;		// +360
			if (roundToAngleR != 0)
			{
				double roundUnit = Math.Round(A / roundToAngleR);
				A = roundToAngleR * roundUnit;
			}
			return A;
		}
		public static UnitPoint OrthoPointD(UnitPoint lp1, UnitPoint lp2, double roundToAngleR)
		{
			return OrthoPointR(lp1, lp2, DegressToRadians(roundToAngleR));
		}
		public static UnitPoint OrthoPointR(UnitPoint lp1, UnitPoint lp2, double roundToAngleR)
		{
			return NearestPointOnLine(lp1, lp2, lp2, roundToAngleR);
		}
		public static UnitPoint NearestPointOnLine(UnitPoint lp1, UnitPoint lp2, UnitPoint tp)
		{
			return NearestPointOnLine(lp1, lp2, tp, false);
		}
		public static UnitPoint NearestPointOnLine(UnitPoint lp1, UnitPoint lp2, UnitPoint tp, bool beyondSegment)
		{
			if (lp1.X == lp2.X)
			{
				// if both points are to below, then choose the points closest to tp.y
				if (beyondSegment == false && lp1.Y < tp.Y && lp2.Y < tp.Y)
					return new UnitPoint(lp1.X, Math.Max(lp1.Y, lp2.Y));
				// if both points are above, then choose the points closest to tp.y
				if (beyondSegment == false && lp1.Y > tp.Y && lp2.Y > tp.Y)
					return new UnitPoint(lp1.X, Math.Min(lp1.Y, lp2.Y));
				// else the line is on both sides and pick tp.x as y point
				return new UnitPoint(lp1.X, tp.Y);
			}
			if (lp1.Y == lp2.Y)
			{
				// if both points are to the right, then choose the points closest to tp.x
				if (beyondSegment == false && lp1.X < tp.X && lp2.X < tp.X)
					return new UnitPoint(Math.Max(lp1.X, lp2.X), lp1.Y);
				// if both points are to the left, then choose the points closest to tp.x
				if (beyondSegment == false && lp1.X > tp.X && lp2.X > tp.X)
					return new UnitPoint(Math.Min(lp1.X, lp2.X), lp1.Y);
				// else the line is on both sides and pick tp.x as x point
				return new UnitPoint(tp.X, lp1.Y);
			}
			return NearestPointOnLine(lp1, lp2, tp, 0);
		}
		private static UnitPoint NearestPointOnLine(UnitPoint lp1, UnitPoint lp2, UnitPoint testpoint, double roundToAngleR)
		{
			if (lp1.X == testpoint.X)
			{
				UnitPoint tmp = lp1;
				lp1 = lp2;
				lp2 = tmp;
			}
			double A1 = LineAngleR(lp1, testpoint, 0);
			double A2 = LineAngleR(lp1, lp2, roundToAngleR);
			double A = A1 - A2;
			double h1 = (lp1.X - testpoint.X) / Math.Cos(A1);
			double h2 = Math.Cos(A) * h1;
			double x = Math.Cos(A2) * h2;
			double y = Math.Sin(A2) * h2;
			x = lp1.X - x;
			y = lp1.Y - y;
			return new UnitPoint((float)x, (float)y);
		}
		public static double LineSlope(UnitPoint p1, UnitPoint p2)
		{
			return (p2.Y - p1.Y) / (p2.X - p1.X);
		}
		public static UnitPoint CenterPointFrom3Points(UnitPoint p1, UnitPoint p2, UnitPoint p3)
		{
			// http://local.wasp.uwa.edu.au/~pbourke/geometry/circlefrom3/
			// 3 points forms 2 lines a and b, slope of a is ma, b is mb
			// ma = (y2-y1) / (x2-x1)
			// mb = (y3-y2) / (x3-x2)
			double ma = (p2.Y - p1.Y) / (p2.X - p1.X);
			double mb = (p3.Y - p2.Y) / (p3.X - p2.X);
			
			// this is just a hacky work around for when p1-p2 are either horizontal or vertical.
			// A real solution should be found, but for now this will work
			if (double.IsInfinity(ma))
				ma = 1000000000000;
			if (double.IsInfinity(mb))
				mb = 1000000000000;
			if (ma == 0)
				ma = 0.000000000001;
			if (mb == 0f)
				mb = 0.000000000001;

			// centerpoint x = mamb(y1-y3) + mb(x1+x2) - ma(x2+x3) / 2(mb-ma)
			double center_x = (ma * mb * (p1.Y - p3.Y) + mb * (p1.X + p2.X) - ma * (p2.X + p3.X)) / (2 * (mb - ma));
			double center_y = (-1 * (center_x - (p1.X + p2.X) / 2) / ma) + ((p1.Y + p2.Y) / 2);

			//m_Center.m_y = -1*(m_Center.x() - (pt1->x()+pt2->x())/2)/aSlope +  (pt1->y()+pt2->y())/2;
			return new UnitPoint((float)center_x, (float)center_y);
		}
	}
	public class SelectionRectangle
	{
		PointF m_point1;
		PointF m_point2;
		public SelectionRectangle(PointF mousedownpoint)
		{
			m_point1 = mousedownpoint;
			m_point2 = PointF.Empty;
		}
		public void Reset()
		{
			m_point2 = PointF.Empty;
		}
		public void SetMousePoint(Graphics dc, PointF mousepoint)
		{
			if (m_point2 != PointF.Empty)
				XorGdi.DrawRectangle(dc, PenStyles.PS_DOT, 1, GetColor(), m_point1, m_point2);
			m_point2 = mousepoint;
			XorGdi.DrawRectangle(dc, PenStyles.PS_DOT, 1, GetColor(), m_point1, m_point2);
		}
		public Rectangle ScreenRect()
		{
			float x = Math.Min(m_point1.X, m_point2.X);
			float y = Math.Min(m_point1.Y, m_point2.Y);
			float w = Math.Abs(m_point1.X - m_point2.X);
			float h = Math.Abs(m_point1.Y - m_point2.Y);
			if (m_point2 == PointF.Empty)
				return Rectangle.Empty;
			if (w < 4 || h < 4) // if no selection was made return empty rectangle (giving a 4 pixel threshold)
				return Rectangle.Empty;
			return new Rectangle((int)x, (int)y, (int)w, (int)h);
		}
		public RectangleF Selection(ICanvas canvas)
		{
			Rectangle screenRect = ScreenRect();
			if (screenRect.IsEmpty)
				return RectangleF.Empty;
			return ScreenUtils.ToUnitNormalized(canvas, screenRect);
		}
		public bool AnyPoint()
		{
			return (m_point1.X > m_point2.X);
		}
		private Color GetColor()
		{
			if (AnyPoint())
				return Color.Blue;
			return Color.Green;
		}
	}
	public class MoveHelper
	{
		List<IDrawObject> m_originals = new List<IDrawObject>();
		List<IDrawObject> m_copies = new List<IDrawObject>();
		UnitPoint m_originPoint = UnitPoint.Empty;
		UnitPoint m_lastPoint = UnitPoint.Empty;
		CanvasCtrl m_canvas;
		public UnitPoint OriginPoint
		{
			get { return m_originPoint; }
		}
		public UnitPoint LastPoint
		{
			get { return m_lastPoint; }
		}
		public IEnumerable<IDrawObject> Copies
		{
			get { return m_copies; }
		}
		public MoveHelper(CanvasCtrl canvas)
		{
			m_canvas = canvas;
		}
		public bool IsEmpty
		{
			get { return m_copies.Count == 0; }
		}
		public bool HandleMouseMoveForMove(UnitPoint mouseunitpoint)
		{
			if (m_originals.Count == 0)
				return false;
			double x = mouseunitpoint.X - m_lastPoint.X;
			double y = mouseunitpoint.Y - m_lastPoint.Y;
			UnitPoint offset = new UnitPoint(x, y);
			m_lastPoint = mouseunitpoint;
			foreach (IDrawObject obj in m_copies)
				obj.Move(offset);
			m_canvas.DoInvalidate(true);
			return true;
		}
		public void HandleCancelMove()
		{
			foreach (IDrawObject obj in m_originals)
				m_canvas.Model.AddSelectedObject(obj);
			m_originals.Clear();
			m_copies.Clear();
			m_canvas.DoInvalidate(true);
		}
		public void HandleMouseDownForMove(UnitPoint mouseunitpoint, ISnapPoint snappoint)
		{
			UnitPoint p = mouseunitpoint;
			if (snappoint != null)
				p = snappoint.SnapPoint;

			if (m_originals.Count == 0) // first step of move
			{
				foreach (IDrawObject obj in m_canvas.Model.SelectedObjects)
				{
					m_originals.Add(obj);
					m_copies.Add(obj.Clone());
				}
				m_canvas.Model.ClearSelectedObjects();
				m_originPoint = p;
				m_lastPoint = p;
			}
			else // move complete
			{
				double x = p.X - m_originPoint.X;
				double y = p.Y - m_originPoint.Y;
				UnitPoint offset = new UnitPoint(x, y);

				// do copy
				if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
				{
					m_canvas.Model.CopyObjects(offset, m_originals);
				}
				else
				{
					// do move
					m_canvas.Model.MoveObjects(offset, m_originals);
					foreach (IDrawObject obj in m_originals)
						m_canvas.Model.AddSelectedObject(obj);
				}
				m_originals.Clear();
				m_copies.Clear();

			}
			m_canvas.DoInvalidate(true);
		}
		public void DrawObjects(ICanvas canvas, RectangleF r)
		{
			if (m_copies.Count == 0)
				return;
			canvas.Graphics.DrawLine(Pens.Wheat, canvas.ToScreen(OriginPoint), canvas.ToScreen(LastPoint));
			foreach (IDrawObject obj in Copies)
				obj.Draw(canvas, r);
		}
	}
	public class NodeMoveHelper
	{
		List<INodePoint> m_nodes = new List<INodePoint>();
		UnitPoint m_originPoint = UnitPoint.Empty;
		UnitPoint m_lastPoint = UnitPoint.Empty;
		CanvasWrapper m_canvas;
		public bool IsEmpty
		{
			get { return m_nodes.Count == 0; }
		}
		public NodeMoveHelper(CanvasWrapper canvas)
		{
			m_canvas = canvas;
		}
		public RectangleF HandleMouseMoveForNode(UnitPoint mouseunitpoint)
		{
			RectangleF r = RectangleF.Empty;
			if (m_nodes.Count == 0)
				return r;
			r = new RectangleF(m_originPoint.Point, new Size(0,0));
			r = RectangleF.Union(r, new RectangleF(mouseunitpoint.Point, new SizeF(0,0)));
			if (m_lastPoint != UnitPoint.Empty)
				r = RectangleF.Union(r, new RectangleF(m_lastPoint.Point, new SizeF(0, 0)));
			
			m_lastPoint = mouseunitpoint;
			foreach (INodePoint p in m_nodes)
			{
				if (r == RectangleF.Empty)
					r = p.GetClone().GetBoundingRect(m_canvas);
				else
					r = RectangleF.Union(r, p.GetClone().GetBoundingRect(m_canvas));
				p.SetPosition(mouseunitpoint);
				//m_canvas.RepaintObject(p.GetClone());
			}
			return r;
		}
		public bool HandleMouseDown(UnitPoint mouseunitpoint, ref bool handled)
		{
			handled = false;
			if (m_nodes.Count == 0) // no nodes selected yet
			{
				if (m_canvas.Model.SelectedCount > 0)
				{
					foreach (IDrawObject obj in m_canvas.Model.SelectedObjects)
					{
						INodePoint p = obj.NodePoint(m_canvas, mouseunitpoint);
						if (p != null)
							m_nodes.Add(p);
					}
				}
				handled = m_nodes.Count > 0;
				if (handled)
					m_originPoint = mouseunitpoint;
				return handled;
			}
			// update selected nodes
			m_canvas.Model.MoveNodes(mouseunitpoint, m_nodes);
			m_nodes.Clear();
			handled = true;
			m_canvas.CanvasCtrl.DoInvalidate(true);
			return handled;
		}
		public void HandleCancelMove()
		{
			foreach (INodePoint p in m_nodes)
				p.Cancel();

			m_nodes.Clear();
		}
		public void DrawOriginalObjects(ICanvas canvas, RectangleF r)
		{
			foreach (INodePoint node in m_nodes)
				node.GetOriginal().Draw(canvas, r);
		}
		public void DrawObjects(ICanvas canvas, RectangleF r)
		{
			if (m_nodes.Count == 0)
				return;
			//canvas.Graphics.DrawLine(Pens.Wheat, canvas.ToScreen(m_originPoint), canvas.ToScreen(m_lastPoint));
			foreach (INodePoint node in m_nodes)
				node.GetClone().Draw(canvas, r);
		}
		public void OnKeyDown(ICanvas canvas, KeyEventArgs keyevent)
		{
			foreach (INodePoint nodepoint in m_nodes)
			{
				nodepoint.OnKeyDown(canvas, keyevent);
				if (keyevent.Handled)
					return;

				IDrawObject drawobject = nodepoint.GetClone();
				if (drawobject == null)
					continue;
				drawobject.OnKeyDown(canvas, keyevent);
			}
		}
	}
	public class Utils
	{
		public static IDrawObject FindObjectTypeInList(IDrawObject caller, List<IDrawObject> list, Type type)
		{
			foreach (IDrawObject obj in list)
			{
				if (object.ReferenceEquals(caller, obj))
					continue;
				if (obj.GetType() == type)
					return obj;
			}
			return null;
		}
	}
}
