
using System;

namespace TVM.WPF.Library.Animation
{
	public static class PennerInterpolator
	{
		public static double Linear(double from, double to, double t)
		{
			return from + ((to - from)*t);
		}

		// Quadratic 
		public static double QuadEaseIn(double from, double to, double t)
		{
			return (to - from)*Math.Pow(t, 2) + from;
		}

		public static double QuadEaseOut(double from, double to, double t)
		{
			return -1*(to - from)*t*(t - 2) + from;
		}

		// Cubic
		public static double CubicEaseIn(double from, double to, double t)
		{
			return (to - from)*Math.Pow(t, 3) + from;
		}

		public static double CubicEaseOut(double from, double to, double t)
		{
			return (to - from)*(Math.Pow(t - 1, 3) + 1) + from;
		}

		// Quartic
		public static double QuarticEaseIn(double from, double to, double t)
		{
			return (to - from)*Math.Pow(t, 4) + from;
		}

		public static double QuarticEaseOut(double from, double to, double t)
		{
			return -1*(to - from)*(Math.Pow(t - 1, 4) - 1) + from;
		}

		// Quintic
		public static double QuinticEaseIn(double from, double to, double t)
		{
			return (to - from)*Math.Pow(t, 5) + from;
		}

		public static double QuinticEaseOut(double from, double to, double t)
		{
			return (to - from)*(Math.Pow(t - 1, 5) + 1) + from;
		}

		// Sine
		public static double SineEaseIn(double from, double to, double t)
		{
			return -1*(to - from)*Math.Cos(t*(Math.PI/2)) + to;
		}

		public static double SineEaseOut(double from, double to, double t)
		{
			return (to - from)*Math.Sin(t*(Math.PI/2)) + from;
		}

		// Exponential
		public static double ExpoEaseIn(double from, double to, double t)
		{
			return (t == 0) ? from : (to - from)*Math.Pow(2, 10*(t - 1)) + from;
		}

		public static double ExpoEaseOut(double from, double to, double t)
		{
			return (t == 1) ? to : (to - from)*(-1*Math.Pow(2, -10*t) + 1) + from;
		}

		// Circular
		public static double CircularEaseIn(double from, double to, double t)
		{
			return -1*(to - from)*(Math.Sqrt(1 - t*t) - 1) + from;
		}

		public static double CircularEaseOut(double from, double to, double t)
		{
			t = t - 1;
			return (to - from)*Math.Sqrt(1 - t*t) + from;
		}

		// Elastic
		public static double ElasticEaseIn(double from, double to, double t)
		{
			if (t == 0)
			{
				return from;
			}
			if (t == 1)
			{
				return to;
			}

			double d = 100;
			double p = 0.3*d;
			double a = to - from;
			double s = p/4;

			t = t - 1;
			return -(a*Math.Pow(2, 10*t)*Math.Sin((t*d - s)*(2*Math.PI)/p)) + from;
		}

		public static double ElasticEaseOut(double from, double to, double t)
		{
			if (t == 0)
			{
				return from;
			}
			if (t == 1)
			{
				return to;
			}
			double d = 100;
			double p = d*0.3;
			double a = to - from;
			double s = p/4;
			return (a*Math.Pow(2, -10*t)*Math.Sin((t*d - s)*(2*Math.PI)/p) + to);
		}

		// Bounce
		public static double BounceEaseIn(double from, double to, double t)
		{
			double c = to - from;
			return c - BounceEaseOut(0, to, 1.0 - t) + from;
		}

		public static double BounceEaseOut(double from, double to, double t)
		{
			double c = to - from;

			if (t < (1/2.75))
			{
				return c*(7.5625*t*t) + from;
			}
			else if (t < (2/2.75))
			{
				t -= 1.5/2.75;
				return c*(7.5625*t*t + .75) + from;
			}
			else if (t < (2.5/2.75))
			{
				t -= 2.25/2.75;
				return c*(7.5625*t*t + .9375) + from;
			}
			else
			{
				t -= 2.625/2.75;
				return c*(7.5625*t*t + .984375) + from;
			}
		}

		// Back
		public static double BackEaseIn(double from, double to, double t)
		{
			double s = 1.70158;
			return (to - from)*(t*t*((s + 1)*t - s)) + from;
		}

		public static double BackEaseOut(double from, double to, double t)
		{
			double s = 1.70158;
			t = t - 1;
			return (to - from)*(t*t*((s + 1)*t + s) + 1) + from;
		}
	}
}