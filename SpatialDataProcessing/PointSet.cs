using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.SqlServer.Server;
using System.Linq;
using System.Reflection;


[Serializable]
[SqlUserDefinedType(Format.UserDefined, MaxByteSize = -1, IsByteOrdered = false)]
[StructLayout(LayoutKind.Sequential)]
public struct PointSet: INullable, IBinarySerialize
{
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


    [SqlMethod(OnNullCall = false)]
    public static PointSet Parse(SqlString s)
    {
        if (s.IsNull || s.ToString().Trim() == "") { return Null; }

        return new PointSet(Utils.SqlStringToPointsArray(s));
    }
    public override string ToString()
    {
        if (IsNull) { return "NULL"; }

        return Utils.PointsArrayToString(_points);
    }

    public static PointSet Null
    {
        get
        {
            PointSet p = new PointSet();
            p._isNull = true;
            return p;
        }
    }
    public bool IsNull
    {
        get { return _isNull; }
    }

    public Point[] Points { get { return _points; } }

    public Polygon FindConvexHull()
    {
        //https://pl.wikipedia.org/wiki/Algorytm_Grahama
        if (IsNull || _points.Length < 3) { return Polygon.Null;  }

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

        return new Polygon(stack.ToArray());
    }


    public Point GetCentroid() {
        if(IsNull || _points.Length < 1) { return  Point.Null; }
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