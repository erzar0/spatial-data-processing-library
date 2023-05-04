using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.SqlServer.Server;


[Serializable]
[SqlUserDefinedType(Format.UserDefined, MaxByteSize = -1, IsByteOrdered = false)]
[StructLayout(LayoutKind.Sequential)]
public struct Polygon : INullable, IBinarySerialize
{
    public Polygon(Point[] points)
    {
        if (points.Length < 3)
        {
            throw new SqlTypeException("Polygon needs to have at least 3 points");
        }
        _points = points;
        _isNull = false;
    }


    [SqlMethod(OnNullCall = false)]
    public static Polygon Parse(SqlString s)
    {
        if (s.IsNull || s.ToString().Trim() == "") { return Polygon.Null; }

        string[] points = System.Text.RegularExpressions.Regex.Split(
                        s.ToString().Trim('(', ')', ' '), @"\s*,\s*");

        Point[] ps = new Point[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            ps[i] = Point.Parse(points[i]);
        }
        return new Polygon(ps);
    }
    public override string ToString()
    {
        if (IsNull) { return "NULL"; }
        StringBuilder sb = new StringBuilder();
        sb.Append("(");
        foreach (Point p in _points)
        {
            sb.Append($"{p.ToString()},");
        }
        sb.Remove(sb.Length - 1, 1);
        sb.Append(")");

        return sb.ToString();
    }

    public static Polygon Null
    {
        get
        {
            Polygon p = new Polygon();
            p._isNull = true;
            return p;
        }
    }
    public bool IsNull
    {
        get { return _isNull; }
    }

    public Point[] Points { get { return _points; } }

    public SqlDouble Circumference()
    {
        if (IsNull)
        {
            return SqlDouble.Null;
        }

        SqlDouble circumference = 0;
        int i = 1;
        for (; i < _points.Length; i++)
        {
            circumference += _points[i].DistanceTo(_points[i - 1]);
        }
        circumference += _points[i-1].DistanceTo(_points[0]);
        return circumference;
    }

    public SqlDouble Area()
    {
        //Works only for simple polygons
        //https://www.mathopenref.com/coordpolygonarea2.html

        if (IsNull) { return SqlDouble.Null; }

        double area = 0;
        double partial;
        int i = 1;
        for(; i<_points.Length; i++)
        {
            Point a = _points[i - 1];
            Point b = _points[i];
            partial = (double) ((a.X + b.X) * (a.Y - b.Y));
            Console.WriteLine(partial);
            area += partial;
        }
        partial = (double) ((_points[i-1].X - _points[0].X) * (_points[i-1].Y - _points[0].Y));
        Console.Write(partial);
        return area/2;
    }

    public SqlBoolean ContainsPoint(Point point, SqlDouble eps)
    {
        //if (this.IsNull) { return SqlBoolean.False; }
        //double m = getSlopeValue();
        //double c = getInterceptValue();
        //return ((point.Y - (m * point.X + c)) < eps);
        return true;
    }

    public void Read(BinaryReader reader)
    {
        int numPoints = reader.ReadInt32();
        _points = new Point[numPoints];
        for (int i = 0; i < numPoints; i++)
        {
            _points[i].Read(reader);
        }
        _isNull = false;
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(_points.Length);
        foreach (Point p in _points)
        {
            p.Write(writer);
        }
    }
    private Point[] _points;
    private bool _isNull;
}