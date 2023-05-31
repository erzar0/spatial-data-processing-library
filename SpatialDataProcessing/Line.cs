using System;
using System.ComponentModel.Design;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;


[Serializable]
[SqlUserDefinedType(Format.Native)]
public struct Line : INullable
{
    public Line(Point a, Point b)
    {
        _a = a;
        _b = b;
        _isNull = false;
    }

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
    public override string ToString()
    {
        if (IsNull) { return "NULL"; }

        return $"({_a},{_b})";
    }

    public bool IsNull
    {
        get { return _isNull; }
    }

    public static Line Null
    {
        get
        {
            Line l = new Line(new Point(0, 0), new Point(0, 0));
            l._isNull = true;
            return l;
        }
    }

    public Point A
    {
        get { return _a; }
        set { _a = value; }
    }
    public Point B
    {
        get { return _b; }
        set { _b = value; }
    }
    public SqlDouble Length()
    {
        if (IsNull)
        {
            return SqlDouble.Null;
        }

        return _a.DistanceTo(_b);
    }

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


    public Point GetIntersection(Line another)
    {
        if (IsNull || another.IsNull)
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

    private SqlDouble GetSlopeValue()
    {
        if(IsNull) { return SqlDouble.Null; }

        double dY = (double) (_b.Y - _a.Y);
        double dX = Math.Max((double) (_b.X - _a.X), 1e-12);
        return dY / dX;
    }

    private SqlDouble GetInterceptValue()
    {
        if(IsNull) { return SqlDouble.Null; }
        
        return _a.Y - GetSlopeValue() * _a.X;
    }



    private Point _a;
    private Point _b;
    private bool _isNull;
}
