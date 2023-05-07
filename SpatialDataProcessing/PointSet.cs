using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.SqlServer.Server;
using System.Linq;
using SpatialDataProcessing;


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


    [SqlMethod(OnNullCall = false)]
    public static PointSet Parse(SqlString s)
    {
        if (s.IsNull || s.ToString().Trim() == "") { return Null; }

        Point[] ps = Utils.SqlStringToPointsArray(s);
        return new PointSet(ps);
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
        if (IsNull) { return Polygon.Null;  }

        Point anchor = _points.OrderBy(p => p.X).ThenBy(p => p.Y).First();
        Point[] sortedPoints = _points.OrderBy(p => (p - anchor).PolarAngle()).ToArray();

        Stack<Point> stack = new Stack<Point>();
        stack.Push(sortedPoints[0]);
        stack.Push(sortedPoints[1]);

        for(int i = 2; i < sortedPoints.Length; i++)
        {
            Point top = stack.Pop();
            while(stack.Count > 1 && Orientation(stack.Peek(), top, sortedPoints[i]) != ORIENTATION.COUNTERCLOCKWISE)
            {
                top = stack.Pop();
            }
            stack.Push(top);
            stack.Push(sortedPoints[i]);
        }

        return new Polygon(stack.ToArray());
    }

    private enum ORIENTATION { CLOCKWISE, COUNTERCLOCKWISE, COLLINEAR}

    private static ORIENTATION Orientation(Point p, Point q, Point r)
    {
        //(pq.cross(qr))
        double crossProduct = (double) ((q.Y - p.Y) * (r.X - p.X) - (q.X - p.X) * (r.Y - q.Y));

        if(crossProduct == 0) { return ORIENTATION.COLLINEAR; }
        return crossProduct > 0 ? ORIENTATION.CLOCKWISE : ORIENTATION.COUNTERCLOCKWISE; 
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