using System;
using System.Data.SqlTypes;
using System.Globalization;
using System.IO;
using Microsoft.SqlServer.Server;


[Serializable]
[SqlUserDefinedType(Format.UserDefined, MaxByteSize = 20, IsByteOrdered = false)]
public struct Point : INullable, IBinarySerialize
{
    public Point(SqlDouble x, SqlDouble y)
    {
        _x = (double) x;
        _y = (double) y;
        _isNull = false;
    }


    [SqlMethod(OnNullCall = false)]
    public static Point Parse(SqlString s)
    {
        if (s.IsNull || s.ToString().Trim() == "") { return Null; }

        string[] xy = System.Text.RegularExpressions.Regex.Split(
                    s.ToString()
                    .Trim('(', ')', ' '), @"\s+");
        double x = double.Parse(xy[0].Trim(' '), CultureInfo.InvariantCulture);
        double y = double.Parse(xy[1].Trim(' '), CultureInfo.InvariantCulture);
        return new Point(x, y);
    }
    public override string ToString()
    {
        if (IsNull) { return "NULL"; }

        return $"({_x.ToString().Replace(",", ".")} {_y.ToString().Replace(",", ".")})";
    }

    public static Point Null
    {
        get
        {
            Point p = new Point();
            p._isNull = true;
            return p;
        }
    }

    public bool IsNull
    {
        get { return _isNull;  }
    }

    public SqlDouble X
    {
        get { return _x; }
        set { _x = (double) value; }
    }
    public SqlDouble Y
    {
        get { return _y; }
        set { _y = (double) value; }
    }

    public SqlDouble DistanceTo(Point p2)
    {
        if (IsNull || p2.IsNull)
        {
            return SqlDouble.Null;
        }

        return Math.Sqrt(Math.Pow((double)(X - p2.X), 2) + Math.Pow((double)(Y - p2.Y), 2));
    }


    public void Write(BinaryWriter w)
    {
        w.Write(_isNull);
        if (!_isNull)
        {
            w.Write(_x);
            w.Write(_y);
        }
    }

    public void Read(BinaryReader r)
    {
        _isNull = r.ReadBoolean();
        if (!_isNull)
        {
            _x = r.ReadDouble();
            _y = r.ReadDouble();
        }
    }

    private double _x;
    private double _y;
    private bool _isNull;
}  
