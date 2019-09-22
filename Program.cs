using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;


namespace TriangleArea
{
	struct Point2
	{
		public double x;
		public double y;

		public Point2(double _x, double _y)
		{
			x = _x;
			y = _y;
		}

		static public Point2 operator -(Point2 a, Point2 b)
		{
			Point2 res;
			res.x = a.x - b.x;
			res.y = a.y - b.y;
			return res;
		}

		static public Point2 operator +(Point2 a, Point2 b)
		{
			Point2 res;
			res.x = a.x + b.x;
			res.y = a.y + b.y;
			return res;
		}

		static public Point2 operator *(Point2 a, double scale)
		{
			Point2 res;
			res.x = a.x * scale;
			res.y = a.y * scale;
			return res;
		}

		public double Dot(Point2 other)
		{
			return x * other.x + y * other.y;
		}

		static public Point2 Max(Point2 a, Point2 b)
		{
			Point2 res;
			res.x = Math.Max(a.x, b.x);
			res.y = Math.Max(a.y, b.y);
			return res;
		}

		static public Point2 Min(Point2 a, Point2 b)
		{
			Point2 res;
			res.x = Math.Min(a.x, b.x);
			res.y = Math.Min(a.y, b.y);
			return res;
		}


		static public Point2 Origin = new Point2(0, 0);

	}

	class Tri
	{
		HalfPlane[] Planes;

		public Tri(HalfPlane[] planes)
		{
			Create(planes);
		}
		protected Tri()	{ }

		public void Create(HalfPlane[] planes)
		{
			Debug.Assert(planes.Length == 3);
			Planes = planes;
		}

		public bool TestPoint(Point2 pt)
		{
			foreach (var p in Planes)
			{
				if (!p.Test(pt))
					return false;
			}
			return true;
		}
	}

	class FullTri : Tri
	{
		public Point2[] Points = new Point2[3];

		public Point2 PtMin;
		public Point2 PtMax;

		public FullTri(double x0, double y0, double x1, double y1, double x2, double y2)
		{
			Points[0] = new Point2(x0, y0);
			Points[1] = new Point2(x1, y1);
			Points[2] = new Point2(x2, y2);

			var planes = new HalfPlane[3];
			planes[0] = new HalfPlane(Points[0], Points[1]);
			planes[1] = new HalfPlane(Points[1], Points[2]);
			planes[2] = new HalfPlane(Points[2], Points[0]);

			Create(planes);

			PtMin = Point2.Min(Points[0], Points[1]);
			PtMin = Point2.Min(PtMin, Points[2]);

			PtMax = Point2.Max(Points[0], Points[1]);
			PtMax = Point2.Max(PtMax, Points[2]);
		}

		public Tri CreateThreedian()
		{
			Point2 p01 = Points[0] + (Points[1] - Points[0]) * ((double) 1/ 3);
			Point2 p12 = Points[1] + (Points[2] - Points[1]) * ((double)1 / 3);
			Point2 p20 = Points[2] + (Points[0] - Points[2]) * ((double)1 / 3);

			var planes = new HalfPlane[3];
			planes[0] = new HalfPlane(Points[0], p12);
			planes[1] = new HalfPlane(Points[1], p20);
			planes[2] = new HalfPlane(Points[2], p01);

			return new Tri(planes);
		}

	}


	class AreaMeasure
	{
		public delegate bool PointTester (Point2 test_pt);
		const int Samples = 1000000;
		static public bool Log = false;
		static public double Calc(PointTester tester, double dx = 1, double dy = 1, double xs = 0, double ys = 0)
		{
			int cnt = 0;
			var rnd = new System.Random();

			for (int i = 0; i < Samples; i++)
			{
				Point2 pt;
				pt.x = rnd.NextDouble() * dx + xs;
				pt.y = rnd.NextDouble() * dy + ys;

				bool b = tester(pt);
				if (Log)
					Console.WriteLine(String.Format("({0}, {1}) -> {2}", pt.x, pt.y, b));
				if (b)
					cnt++;
			}

			double maxa = dx * dy;

			return maxa * cnt / Samples;
		}

		static public double Calc(Tri tri, double dx = 1, double dy = 1, double xs = 0, double ys = 0)
		{
			return Calc(pt => tri.TestPoint(pt), dx, dy, xs, ys);
		}

		static public double Calc(FullTri tri, double dx = 1, double dy = 1, double xs = 0, double ys = 0)
		{
			var del = tri.PtMax - tri.PtMin;
			return Calc((Tri) tri, del.x, del.y, tri.PtMin.x, tri.PtMin.y);
		}
	}

	class HalfPlane
	{
		Point2 Norm;
		double D;

		public HalfPlane(Point2 p1, Point2 p2)
		{
			var dp = p2 - p1;

			Norm.x = -dp.y;
			Norm.y = dp.x;

			D = Norm.Dot(p1);
		}

		public bool Test(Point2 pt)
		{
			var d = pt.Dot(Norm);

			return d >= D;
		}
	}

	class Tests
	{
		static public void Run()
		{
			//var tri = new Tri(planes);
			var tri = new FullTri(0, 0, 1, 0, 0, 1);
			Debug.Assert(tri.TestPoint(new Point2(.1, .1)));
			Debug.Assert(!tri.TestPoint(new Point2(-.1, .1)));
			Debug.Assert(!tri.TestPoint(new Point2(.1, -.1)));
			Debug.Assert(!tri.TestPoint(new Point2(1, 1)));

			double a = AreaMeasure.Calc(tri);
			Debug.Assert(Math.Abs(a - .5) < .01);

			tri = new FullTri(0, 0, 10, 0, 0, 20);
			Debug.Assert(tri.TestPoint(new Point2(2, 2)));
			a = AreaMeasure.Calc(tri);
			Debug.Assert(Math.Abs(a - 100) < 1);

			tri = new FullTri(0 - 1, 0 + 2, 1 - 1, 0 + 2, 0 - 1, 1 + 2);
			a = AreaMeasure.Calc(tri);
			Debug.Assert(Math.Abs(a - .5) < .01);

			tri = new FullTri(-3, -2, 6, -7, -2, 5);
			a = AreaMeasure.Calc(tri);
			Debug.Assert(Math.Abs(a - 34) < .1);

		}


	}


	class Program
	{
		static void ThreedianCalc(FullTri tri)
		{
			double a = AreaMeasure.Calc(tri);

			var threed = tri.CreateThreedian();

			var del = tri.PtMax - tri.PtMin;
			double ta = AreaMeasure.Calc(threed, del.x, del.y, tri.PtMin.x, tri.PtMin.y);

			var ratio = ta / a;

			Console.WriteLine(string.Format("Full Area: {0} Threed Area: {1} Ratio: {2}", a, ta, ratio));

		}


		static void Main(string[] args)
		{
			Tests.Run();

			var tri = new FullTri(0, 0, 1, 0, 0, 1);
			ThreedianCalc(tri);

			tri = new FullTri(-3, -2, 6, -7, -2, 5);
			ThreedianCalc(tri);


		}
	}
}
