using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.SqlServer.Server;

/// <summary>
/// Represents a Polygon structure that supports geometric operations.
/// </summary>
[Serializable]
[SqlUserDefinedType(Format.UserDefined, MaxByteSize = -1, IsByteOrdered = false)]
[StructLayout(LayoutKind.Sequential)]
public struct Polygon : INullable, IBinarySerialize
{
    /// <summary>
    /// Initializes a new instance of the Polygon struct with the provided array of points.
    /// </summary>
    /// <param name="points">An array of points representing the vertices of the polygon.</param>
    /// <exception cref="SqlTypeException">Thrown when the points array is null, has less than 3 points, contains null points, or the edges of the polygon intersect.</exception>
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

    public Polygon(PointSet ps): this(ps.Points) { }

    /// <summary>
    /// Parses a string representation of a Polygon and returns a new instance.
    /// </summary>
    /// <param name="s">The string representation of the Polygon.</param>
    /// <returns>A new instance of the Polygon struct parsed from the input string.</returns>
    [SqlMethod(OnNullCall = false)]
    public static Polygon Parse(SqlString s)
    {
        if (s.IsNull || s.ToString().Trim() == "") { return Null; }

        return new Polygon(Utils.SqlStringToPointsArray(s));
    }

    /// <summary>
    /// Returns a string representation of the Polygon.
    /// </summary>
    /// <returns>A string representation of the Polygon.</returns>
    public override string ToString()
    {
        if (IsNull) { return ""; }

        return Utils.PointsArrayToString(_points);
    }

    /// <summary>
    /// Gets the null instance of the Polygon.
    /// </summary>
    public static Polygon Null
    {
        get
        {
            Polygon p = new Polygon();
            p._isNull = true;
            return p;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the Polygon is null.
    /// </summary>
    public bool IsNull
    {
        get { return _isNull; }
    }

    /// <summary>
    /// Gets an array of points representing the vertices of the Polygon.
    /// </summary>
    public Point[] Points { get { return _points; } }

    /// <summary>
    /// Calculates and returns the circumference of the Polygon.
    /// </summary>
    /// <returns>The circumference of the Polygon.</returns>
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


    /// <summary>
    /// Calculates and returns the area of the Polygon
    /// , works only for non self-intersecting polygons. 
    /// Algorithm used can be found here: https://www.mathopenref.com/coordpolygonarea2.html
    /// </summary>
    /// <returns>The area of the Polygon.</returns>
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
        area += (double) ((_points[i-1].X - _points[0].X) * (_points[i-1].Y - _points[0].Y));
        return area/2;
    }


    
    /// <summary>
    /// Determines whether the Polygon contains the specified point excluding boundries. 
    /// Used algorithm can be found here: http://alienryderflex.com/polygon/
    /// </summary>
    /// <param name="point">The point to check.</param>
    /// <returns>True if the Polygon contains the point, false otherwise.</returns>
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


    /// <summary>
    /// Determines whether the Polygon intersects with the specified line.
    /// </summary>
    /// <param name="line">The line to check.</param>
    /// <returns>True if the Polygon intersects with the line, false otherwise.</returns>
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

    /// <summary>
    /// Parses an array of points and returns a list of Line objects representing the edges of a polygon.
    /// </summary>
    /// <param name="points">An array of points representing the vertices of a polygon.</param>
    /// <returns>A list of Line objects representing the edges of the polygon.</returns>
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


    /// <summary>
    /// Calculates centroid if Polygon
    /// </summary>
    /// <returns>Point instance representing centroid of polygon</returns>
    public Point GetCentroid() 
    {
        if (IsNull) { return Point.Null; }

        return Utils.CalculateCentroid(_points);
    }

    /// <summary>
    /// Deserializes object
    /// </summary>
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

    /// <summary>
    /// Serializes object
    /// </summary>
    public void Write(BinaryWriter writer)
    {
        writer.Write(_points.Length);
        foreach (Point p in _points)
        {
            p.Write(writer);
        }
    }

    /// <summary>
    /// Checks if any edge in edges intersect any other edge
    /// </summary>
    /// <param name="edges">An array of edges</param>
    /// <returns>true if any two edges in array intersects; false otherwise</returns>
    private static bool DoesEdgesIntersect(Line[] edges)
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

    private Point[] _points;
    private bool _isNull;
}