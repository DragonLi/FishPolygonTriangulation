using System;
using System.Collections.Generic;

namespace FishTriangulation
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            float[] x={140,220,140,250,180,100,10,120,150,150,110};
            float[] y={110,110,70,70,210,160,10,30,20,60,40};
            var arr = new List<Point2D>(x.Length);
            for (var i = 0; i < x.Length; i++)
            {
                var p = new Point2D {x = x[i], y = y[i]};
                arr.Add(p);
            }
            var poly = new Polygon2D(arr);
            var result = poly.EarClipping();
            for (var i = 0; i < result.Count; i++)
            {
                Console.WriteLine($"{i+1} "+Show(result[i]));
            }
        }

        private static string Show(Triangle triangle)
        {
            return ($"triangle:<{Show(triangle.a)},{Show(triangle.b)},{Show(triangle.c)}>");
        }

        private static string Show(Point2D tri)
        {
            return $"({tri.x},{tri.y})";
        }
    }
}