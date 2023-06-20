using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.SqlServer.Server;
using System.Linq;

/// <summary>
/// Represents a set of points.
/// </summary>
[Serializable]
[SqlUserDefinedType(Format.UserDefined, MaxByteSize = -1, IsByteOrdered = false)]
[StructLayout(LayoutKind.Sequential)]
public struct PointSet: INullable, IBinarySerialize
{
    /// <summary>
    /// Initializes a new instance of the PointSet struct with the specified points.
    /// </summary>
    /// <param name="points">The array of points.</param>
    /// <returns>A new instance of PointSet.</returns>
    public PointSet(Point[] points)
    {
        if (points == null || points.Length < 1 )
        {
            _isNull = true;
            _points = null;
        }
        else {
            _points = points;
            _isNull = false;
        }
    }

    /// <summary>
    /// Initializes a new instance of the PointSet struct with the points of the specified Polygon.
    /// </summary>
    /// <param name="p">The Polygon to retrieve the points from.</param>
    /// <returns>New PointSet instance.</returns>
    public PointSet(Polygon p)
    {
        if(p.IsNull)
        {
            _isNull = true;
            _points = null;
        }
        else
        {
            _isNull = false;
            _points = new Point[p.Points.Length];
            Array.Copy(p.Points, _points, _points.Length);
        }
    }


    /// <summary>
    /// Parses a string representation of a PointSet and creates a new instance of the PointSet struct.
    /// </summary>
    /// <param name="s">The string representation of the PointSet.</param>
    /// <returns>A new instance of the PointSet struct parsed from the input string. 
    /// Returns PointSet.Null if the input string is null or empty.</returns>
    [SqlMethod(OnNullCall = false)]
    public static PointSet Parse(SqlString s)
    {
        if (s.IsNull || s.ToString().Trim() == "") { return Null; }

        return new PointSet(Utils.SqlStringToPointsArray(s));
    }

    /// <summary>
    /// Returns a string representation of the PointSet.
    /// </summary>
    /// <returns>A string representation of the PointSet. 
    /// Returns "" if the PointSet is null.</returns>
    public override string ToString()
    {
        if (IsNull) { return ""; }

        return Utils.PointsArrayToString(_points);
    }

    /// <summary>
    /// Gets the null instance of the PointSet.
    /// </summary
    public static PointSet Null
    {
        get
        {
            PointSet p = new PointSet();
            p._isNull = true;
            return p;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the PointSet is null.
    /// </summary>
    public bool IsNull
    {
        get { return _isNull; }
    }

    /// <summary>
    /// Gets an array of points in the PointSet.
    /// </summary>
    public Point[] Points { get { return _points; } }

    /// <summary>
    /// Finds the convex hull of points in the PointSet. 
    /// Used algorithm can be found here: https://pl.wikipedia.org/wiki/Algorytm_Grahama
    /// </summary>
    /// <returns>
    /// The convex hull of the PointSet as a Polygon. 
    /// Returns Polygon.Null if the PointSet is null or contains less than 3 points.
    /// </returns>
    public PointSet FindConvexHull()
    {
        if (IsNull || _points.Length < 3) { return PointSet.Null;  }

        Point anchor = _points.OrderBy(p => p.Y).ThenBy(p => p.X).First();
        Point[] sortedPoints = _points.OrderBy(p => (p - anchor).PolarAngle()).ToArray();

        Stack<Point> stack = new Stack<Point>();
        stack.Push(sortedPoints[0]);
        stack.Push(sortedPoints[1]);

        for(int i = 2; i < sortedPoints.Length; i++)
        {
            Point top = stack.Pop();
            Utils.ORIENTATION orientation = Utils.Orientation(stack.Peek(), top, sortedPoints[i]);
            while(stack.Count > 1 && (orientation != Utils.ORIENTATION.COUNTERCLOCKWISE))
            {
                top = stack.Pop();
                orientation = Utils.Orientation(stack.Peek(), top, sortedPoints[i]);
            }
            stack.Push(top);
            stack.Push(sortedPoints[i]);
        }

        return new PointSet(stack.ToArray());
    }

    /// <summary>
    /// Checks if point is in PointSet with eps accuracy
    /// </summary>
    /// <returns>
    /// True if distance between passed point and any point in PointSet is less than eps; 
    /// false otherwise 
    /// </returns>
    public SqlBoolean ContainsPoint(Point point, double eps)
    {
        if(point.IsNull) { return SqlBoolean.False; }

        foreach(Point p in _points)
        {
            if(point.DistanceTo(p) <= eps)
            {
                return SqlBoolean.True;
            }
        }
        return SqlBoolean.False;
    }


    /// <summary>
    /// Calculates centroid of points in PointSet.
    /// </summary>
    /// <returns>Point representing centroid of PointSet</returns>
    public Point GetCentroid() {
        if(IsNull || _points.Length < 1) { return  Point.Null; }
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

    private Point[] _points;
    private bool _isNull;
}