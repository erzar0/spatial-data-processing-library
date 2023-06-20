using System;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;

/// <summary>
/// Utility class for performing operations on points.
/// </summary>
public class Utils
{
    /// <summary>
    /// Converts an array of points to a string representation.
    /// </summary>
    /// <param name="ps">The array of points.</param>
    /// <returns>A string representation of the points in the format "((x1, y1), (x2, y2), ..., (xn, yn))".</returns>
    public static string PointsArrayToString(Point[] ps)
    {
        if(ps == null || ps.Length == 0)
        {
            return "";
        }

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

    /// <summary>
    /// Parses a string representation of points in format "((x1, y1), (x2, y2), ..., (xn, yn))" 
    /// and returns an array of Point objects.
    /// </summary>
    /// <param name="s">The string representation of points.</param>
    /// <returns>An array of Point objects parsed from the input string. Returns null if the input string is null or empty.</returns>
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

    /// <summary>
    /// Calculates the centroid of an array of points.
    /// </summary>
    /// <param name="points">The array of points.</param>
    /// <returns>The centroid of the points.</returns>
    public static Point CalculateCentroid(Point[] points)
    {
        if(points == null || points.Length == 0) { return Point.Null; }
        return points.Aggregate(new Point(0, 0), (prev, curr) => prev + curr, (acc) => acc / points.Length);
    }

    /// <summary>
    /// Enumeration for posible orientations of 3 points.
    /// </summary>
    public enum ORIENTATION { CLOCKWISE, COUNTERCLOCKWISE, COLLINEAR}

    /// <summary>
    /// Determines the orientation of three points.
    /// </summary>
    /// <param name="p">The first point.</param>
    /// <param name="q">The second point.</param>
    /// <param name="r">The third point.</param>
    /// <returns>The orientation of the points: CLOCKWISE, COUNTERCLOCKWISE, or COLLINEAR.</returns>
    public static ORIENTATION Orientation(Point p, Point q, Point r)
    {
        //(pq.cross(qr))
        double crossProduct = (double) ((q.Y - p.Y) * (r.X - q.X) - (q.X - p.X) * (r.Y - q.Y));

        if(Math.Abs(crossProduct) < Utils.EPSILON) { return ORIENTATION.COLLINEAR; }
        return crossProduct > 0 ? ORIENTATION.CLOCKWISE : ORIENTATION.COUNTERCLOCKWISE; 
    }

    public static readonly double EPSILON = 1e-12;

}
