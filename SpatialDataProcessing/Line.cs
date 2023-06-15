using System;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;


/// <summary>
/// Represents a line segment defined by two points.
/// </summary>
[Serializable]
[SqlUserDefinedType(Format.Native)]
public struct Line : INullable
{
    /// <summary>
    /// Initializes a new instance of the Line struct.
    /// </summary>
    /// <param name="a">The first point defining the line.</param>
    /// <param name="b">The second point defining the line.</param>
    public Line(Point a, Point b)
    {
        _a = a;
        _b = b;
        _isNull = false;
    }

    /// <summary>
    /// Parses a string representation of a Line object.
    /// </summary>
    /// <param name="s">The string representation of the Line object.</param>
    /// <returns>A Line object parsed from the input string. Returns Null if the input is null or empty.</returns>
    [SqlMethod(OnNullCall = false)]
    public static Line Parse(SqlString s)
    {
        if (s.IsNull || s.ToString().Trim() == "") { return Null; }

        string[] points = System.Text.RegularExpressions.Regex.Split(
                        s.ToString().Replace('\n', ' ').Trim('(', ')', ' '), @"\s*,\s*");
        Point a = Point.Parse(points[0]);
        Point b = Point.Parse(points[1]);
        return new Line(a, b);
    }

    /// <summary>
    /// Returns a string representation of the Line object.
    /// </summary>
    /// <returns>A string representation of the Line object. Returns "NULL" if the line is null.</returns>
    public override string ToString()
    {
        if (IsNull) { return "NULL"; }

        return $"({_a},{_b})";
    }

    /// <summary>
    /// Gets a value indicating whether the Line object is null.
    /// </summary>
    public bool IsNull
    {
        get { return _isNull; }
    }

    /// <summary>
    /// Gets a null Line object.
    /// </summary>
    public static Line Null
    {
        get
        {
            Line l = new Line(new Point(0, 0), new Point(0, 0));
            l._isNull = true;
            return l;
        }
    }

    /// <summary>
    /// Gets or sets the first point of the Line.
    /// </summary>
    public Point A
    {
        get { return _a; }
        set { _a = value; }
    }

    /// <summary>
    /// Gets or sets the second point of the Line.
    /// </summary>
    public Point B
    {
        get { return _b; }
        set { _b = value; }
    }

    /// <summary>
    /// Calculates the length of the Line.
    /// </summary>
    /// <returns>The length of the Line. Returns Null if the Line is null.</returns>
    public SqlDouble Length()
    {
        if (IsNull)
        {
            return SqlDouble.Null;
        }

        return _a.DistanceTo(_b);
    }

    /// <summary>
    /// Checks if the Line contains the specified point within a given tolerance. 
    /// Points lying on the end of line are not includes.
    /// </summary>
    /// <param name="point">The point to check.</param>
    /// <param name="eps">The tolerance value.</param>
    /// <returns>true if the Line contains the point within the specified tolerance, false otherwise.</returns>
    public SqlBoolean ContainsPoint(Point point, SqlDouble eps)
    {
        if (IsNull) { return SqlBoolean.False; }

        // ends of line check 
        if( _a.DistanceTo(point) < 2 * eps || _b.DistanceTo(point) < 2 * eps)
        {
            return false;
        }
        
        double minX = Math.Min(_a.X.Value, _b.X.Value);
        double maxX = Math.Max(_a.X.Value, _b.X.Value);
        double minY = Math.Min(_a.Y.Value, _b.Y.Value);
        double maxY = Math.Max(_a.Y.Value, _b.Y.Value);
        
        // bounds check
        if ((minX - point.X > eps || point.X - maxX > eps
            || minY - point.Y > eps || point.Y - maxY > eps)
        )
        {
            return false;
        }

        SqlDouble m = GetSlopeValue();
        SqlDouble c = GetInterceptValue();
        double distance = Math.Abs((double) (point.Y - (m * point.X + c)));
        Console.WriteLine(distance);
        return distance <= (double) 2 * eps;
    }

    /// <summary>
    /// Checks if the Line intersects with another Line.
    /// </summary>
    /// <param name="another">The other Line to check for intersection.</param>
    /// <returns>true if the Lines intersect, false otherwise. Returns false if either Line is null.</returns>
    public SqlBoolean Intersects(Line another)
    {
        if (IsNull || another.IsNull)
        {
            return SqlBoolean.False;
        }

        Utils.ORIENTATION o1 = Utils.Orientation(A, B, another.A);
        Utils.ORIENTATION o2 = Utils.Orientation(A, B, another.B);
        Utils.ORIENTATION o3 = Utils.Orientation(another.A, another.B, A);
        Utils.ORIENTATION o4 = Utils.Orientation(another.A, another.B, B);

        if (o1 != o2 && o3 != o4 && ContainsPoint(GetIntersection(another), 1e-12)) return true;
        if (o1 == Utils.ORIENTATION.COLLINEAR && ContainsPoint(another.A, 1e-12)) return true;
        if (o2 == Utils.ORIENTATION.COLLINEAR && ContainsPoint(another.B, 1e-12)) return true;
        if (o3 == Utils.ORIENTATION.COLLINEAR && another.ContainsPoint(A, 1e-12)) return true;
        if (o4 == Utils.ORIENTATION.COLLINEAR && another.ContainsPoint(B, 1e-12)) return true;

        return SqlBoolean.False;
    }


    /// <summary>
    /// Calculates the intersection point of the Line with another Line.
    /// </summary>
    /// <param name="another">The other Line to find the intersection point with.</param>
    /// <returns>The intersection point of the Lines. 
    /// Returns Point.Null if either Line is null or they do not intersect.</returns>
    public Point GetIntersection(Line another)
    {
        if (IsNull || another.IsNull || !Intersects(another) )
        {
            return Point.Null;
        }
        double m1 = (double) GetSlopeValue();
        double c1 = (double) GetInterceptValue();
        double m2 = (double) another.GetSlopeValue();
        double c2 = (double) another.GetInterceptValue();

        double dm = m1 - m2;
        dm = dm > 0 ? Math.Max(dm, 1e-12) : Math.Min(dm, -1e-12); 
        double dc = c2 - c1;

        double x = dc / dm;
        double y = m1 * x + c1;
        return new Point(x, y);

    }

    /// <summary>
    /// Calculates the slope of the Line.
    /// </summary>
    /// <returns>The slope of the Line. Returns SqlDouble.Null if the Line is null.</returns>
    private SqlDouble GetSlopeValue()
    {
        if(IsNull) { return SqlDouble.Null; }

        double dY = (double) (_b.Y - _a.Y);
        double dX = Math.Max((double) (_b.X - _a.X), 1e-12);
        return dY / dX;
    }

    /// <summary>
    /// Calculates the intercept of the Line.
    /// </summary>
    /// <returns>The intercept of the Line. Returns SqlDouble.Null if the Line is null.</returns>
    private SqlDouble GetInterceptValue()
    {
        if(IsNull) { return SqlDouble.Null; }
        
        return _a.Y - GetSlopeValue() * _a.X;
    }

    private Point _a;
    private Point _b;
    private bool _isNull;
}
