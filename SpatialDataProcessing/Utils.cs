using System;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;

public class Utils
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
        if (s.IsNull == null || s.ToString().Trim() == "") { return null; }

        string[] points = System.Text.RegularExpressions.Regex.Split(
                        s.ToString().Replace('\n', ' ').Trim('(', ')', ' '), @"\s*,\s*");

        Point[] ps = new Point[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            ps[i] = Point.Parse(points[i]);
        }
        return ps;
    }

    public static Point CalculateCentroid(Point[] points)
    {
        return points.Aggregate(new Point(0, 0), (prev, curr) => prev + curr, (acc) => acc / points.Length);
    }


    public enum ORIENTATION { CLOCKWISE, COUNTERCLOCKWISE, COLLINEAR}
    public static ORIENTATION Orientation(Point p, Point q, Point r)
    {
        //(pq.cross(qr))
        double crossProduct = (double) ((q.Y - p.Y) * (r.X - q.X) - (q.X - p.X) * (r.Y - q.Y));

        if(Math.Abs(crossProduct) < 1e-12) { return ORIENTATION.COLLINEAR; }
        return crossProduct > 0 ? ORIENTATION.CLOCKWISE : ORIENTATION.COUNTERCLOCKWISE; 
    }

}
