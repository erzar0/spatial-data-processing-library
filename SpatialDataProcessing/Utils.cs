using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Text;

namespace SpatialDataProcessing
{
    class Utils
    {
        public static string PointsArrayToString(Point[] ps)
        {
            
            StringBuilder sb = new StringBuilder();
            sb.Append("(");
            foreach (Point p in ps)
            {
                sb.Append($"{p},");
            }
            sb.Remove(sb.Length - 1, 1);
            sb.Append(")");

            return sb.ToString();
        }

        public static Point[] SqlStringToPointsArray(SqlString s)
        {
            if (s.IsNull || s.ToString().Trim() == "") { return null; }

            string[] points = System.Text.RegularExpressions.Regex.Split(
                            s.ToString().Replace('\n', ' ').Trim('(', ')', ' '), @"\s*,\s*");

            Point[] ps = new Point[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                ps[i] = Point.Parse(points[i]);
            }
            return ps;
        }
    }
}
