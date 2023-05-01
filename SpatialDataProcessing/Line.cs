using System;
using System.ComponentModel.Design;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
                        s.ToString().Trim('(', ')', ' '), @"\s*,\s*");
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

        SqlDouble m = GetSlopeValue();
        SqlDouble c = GetInterceptValue();
        return ((point.Y - (m * point.X + c)) < eps);
    }

    private SqlDouble GetSlopeValue()
    {
        return (_b.Y - _a.Y) / (_b.X - _a.X);
    }

    private SqlDouble GetInterceptValue()
    {
        return _a.Y - GetSlopeValue() * _a.X;
    }



    private Point _a;
    private Point _b;
    private bool _isNull;
}
