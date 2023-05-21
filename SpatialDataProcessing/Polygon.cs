using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.SqlServer.Server;

[Serializable]
[SqlUserDefinedType(Format.UserDefined, MaxByteSize = -1, IsByteOrdered = false)]
[StructLayout(LayoutKind.Sequential)]
public struct Polygon : INullable, IBinarySerialize
{
    public Polygon(Point[] points)
    {
        if (points == null || points.Length < 3)
        {
            throw new SqlTypeException("Polygon needs to have at least 3 points!");
        }
        foreach(Point p in points)
        {
            if (p.IsNull) { throw new SqlTypeException("Polygon must not have null points!"); }
        }
        if(DoesEdgesIntersect(ParseEdges(points).ToArray()))
        {
            throw new SqlTypeException("Edges of polygon must not intersect!");
        }

        _points = points;
        _isNull = false;
    }


    [SqlMethod(OnNullCall = false)]
    public static Polygon Parse(SqlString s)
    {
        if (s.IsNull || s.ToString().Trim() == "") { return Null; }

        return new Polygon(Utils.SqlStringToPointsArray(s));
    }
    public override string ToString()
    {
        if (IsNull) { return "NULL"; }

        return Utils.PointsArrayToString(_points);
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


    //Works only for non self-intersecting polygons
    //https://www.mathopenref.com/coordpolygonarea2.html
    public SqlDouble Area()
    {
        if (IsNull) { return SqlDouble.Null; }

        double area = 0;
        double partial;
        int i = 1;
        for(; i<_points.Length; i++)
        {
            Point a = _points[i - 1];
            Point b = _points[i];
            partial = (double) ((a.X + b.X) * (a.Y - b.Y));
            area += partial;
        }
        partial = (double) ((_points[i-1].X - _points[0].X) * (_points[i-1].Y - _points[0].Y));
        return area/2;
    }


    //http://alienryderflex.com/polygon/
    //Don't includes the boundries
    public SqlBoolean ContainsPoint(Point point)
    {
        if(point.IsNull || IsNull) { return false; }

        int intersectionCount = 0;
        for(int i = 0; i<_points.Length; i++)
        {
            Point p1 = _points[i];
            Point p2 = _points[(i+1) % _points.Length];

            if(point.Y > Math.Min((double) p1.Y, (double) p2.Y)
            && point.Y <= Math.Max((double) p1.Y, (double) p2.Y)
            && point.X <= Math.Max((double) p1.X, (double) p2.X)
            && p1.Y != p2.Y)
            {
                if(p1.X == p2.X )
                {
                    intersectionCount++;
                    continue;
                }

                double slope = (double) ((p2.Y - p1.Y) / (p2.X - p1.X));
                double xIntersection = (double)((point.Y - p1.Y) * slope + p1.X);  
                if(point.X <= xIntersection)
                {
                    intersectionCount++;
                }
            }
        }
        return intersectionCount % 2 == 1;
    }


    public SqlBoolean IntersectsLine(Line line)
    {
        if(IsNull || line.IsNull) return false;

        List<Line> edges = ParseEdges(_points);
        foreach(var edge in edges)
        {
            if(edge.Intersects(line))
            {
                return true;
            }
        }
        return false;
    }

    public static List<Line> ParseEdges(Point[] points)
    {
        if(points == null || points.Length < 2 ) { return null; }

        int i = 1;
        List<Line> result = new List<Line>();
        for(; i<points.Length; i++)
        {
            result.Add(new Line(points[i - 1], points[i]));
        }
        result.Add((new Line(points[i - 1], points[0])));
        return result;
    }

    public static bool DoesEdgesIntersect(Line[] edges)
    {
        if(edges == null || edges.Length < 1)  return false;

        for(int i = 0; i < edges.Length; i++)
        {
            for(int j = i+1; j < edges.Length; j++ )
            {
                if (edges[i].Intersects(edges[j]))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public Point GetCentroid() 
    {
        if (IsNull) { return Point.Null; }

        return Utils.CalculateCentroid(_points);
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