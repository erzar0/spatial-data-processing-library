using System;
using System.Data.SqlTypes;
using System.Globalization;
using System.IO;
using Microsoft.SqlServer.Server;

/// <summary>
/// Represents a point in a two-dimensional coordinate system.
/// </summary>
[Serializable]
[SqlUserDefinedType(Format.UserDefined, MaxByteSize = 20, IsByteOrdered = false)]
public struct Point : INullable, IBinarySerialize
{
    /// <summary>
    /// Initializes a new instance of the Point struct with the specified X and Y coordinates.
    /// </summary>
    /// <param name="x">The X-coordinate of the point.</param>
    /// <param name="y">The Y-coordinate of the point.</param>
    public Point(SqlDouble x, SqlDouble y)
    {
        _x = (double) x;
        _y = (double) y;
        _isNull = false;
    }

    /// <summary>
    /// Initializes a new instance of the Point struct with the same coordinates as another Point.
    /// </summary>
    /// <param name="another">Another Point object to copy the coordinates from.</param>
    public Point(Point another)
    {
        _x = (double) another.X;
        _y = (double) another.Y;
        _isNull = another.IsNull;

    }

    /// <summary>
    /// Converts the string representation of a point to its Point equivalent.
    /// </summary>
    /// <param name="s">A string containing the coordinates of the point in the format "(X Y)".</param>
    /// <returns>A Point object representing the coordinates parsed from the input string.</returns>
    [SqlMethod(OnNullCall = false)]
    public static Point Parse(SqlString s)
    {
        if (s.IsNull || s.ToString().Trim() == "") { return Null; }

        string[] xy = System.Text.RegularExpressions.Regex.Split(
                        s.ToString().Replace('\n', ' ').Trim('(', ')', ' '), @"\s+");
        double x = double.Parse(xy[0].Trim(' '), CultureInfo.InvariantCulture);
        double y = double.Parse(xy[1].Trim(' '), CultureInfo.InvariantCulture);
        return new Point(x, y);
    }

    /// <summary>
    /// Converts the Point object to its string representation.
    /// </summary>
    /// <returns>A string representation of the Point object in the format "(X Y)".</returns>
    public override string ToString()
    {
        if (IsNull) { return "NULL"; }

        return $"({_x.ToString().Replace(",", ".")} {_y.ToString().Replace(",", ".")})";
    }

    /// <summary>
    /// Gets a Point object representing a null value.
    /// </summary>
    public static Point Null
    {
        get
        {
            Point p = new Point();
            p._isNull = true;
            return p;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the Point object is null.
    /// </summary>
    public bool IsNull
    {
        get { return _isNull;  }
    }

    /// <summary>
    /// Gets or sets the X-coordinate of the point.
    /// </summary>
    public SqlDouble X
    {
        get { return _x; }
        set { _x = (double) value; }
    }

    /// <summary>
    /// Gets or sets the Y-coordinate of the point.
    /// </summary>
    public SqlDouble Y
    {
        get { return _y; }
        set { _y = (double) value; }
    }

    /// <summary>
    /// Returns unchanged Point
    /// </summary>
    /// <param name="p">The Point object.</param>
    /// <returns>A new Point object.</returns>
    public static Point operator +(Point p) => new Point(p);

    /// <summary>
    /// Negates a Point object.
    /// </summary>
    /// <param name="p">The Point object to be negated.</param>
    /// <returns>A new Point object with negated coordinates.</returns>
    public static Point operator -(Point p) => new Point(-p.X, -p.Y);

    /// <summary>
    /// Adds two Point objects.
    /// </summary>
    /// <param name="p">The first Point object.</param>
    /// <param name="another">The second Point object.</param>
    /// <returns>A new Point object that represents the sum of the two points.</returns>
    public static Point operator +(Point p, Point another)
        => new Point(p.X + another.X, p.Y + another.Y);

    /// <summary>
    /// Subtracts a Point object from another Point object.
    /// </summary>
    /// <param name="p">The Point object to subtract from.</param>
    /// <param name="another">The Point object to be subtracted.</param>
    /// <returns>A new Point object that represents the difference between the two points.</returns>
    public static Point operator -(Point p, Point another)
        => p + (-another);

    /// <summary>
    /// Determines whether two Point objects are equal.
    /// </summary>
    /// <param name="p">The first Point object.</param>
    /// <param name="another">The second Point object.</param>
    /// <returns>true if the two points are equal; otherwise, false.</returns>
    public static bool operator ==(Point p, Point another)
    {
        return (bool)( p.X == another.X && p.Y == another.Y);
    }

    /// <summary>
    /// Determines whether two Point objects are not equal.
    /// </summary>
    /// <param name="p">The first Point object.</param>
    /// <param name="another">The second Point object.</param>
    /// <returns>true if the two points are not equal; otherwise, false.</returns>
    public static bool operator !=(Point p, Point another)
    {
        return !(p==another);
    }

    /// <summary>
    /// Multiplies a Point object by a scale factor.
    /// </summary>
    /// <param name="p">The Point object to be multiplied.</param>
    /// <param name="scale">The scale factor.</param>
    /// <returns>A new Point object with scaled coordinates.</returns>
    public static Point operator *(Point p, double scale)
    {
        return new Point(p.X * scale, p.Y * scale);
    }

    /// <summary>
    /// Divides a Point object by a scale factor.
    /// </summary>
    /// <param name="p">The Point object to be divided.</param>
    /// <param name="scale">The scale factor.</param>
    /// <returns>A new Point object with divided coordinates.</returns>
    public static Point operator /(Point p, double scale)
    {
        return new Point(p.X / scale, p.Y / scale);
    }

    /// <summary>
    /// Calculates the Euclidean distance between two Point objects.
    /// </summary>
    /// <param name="p2">The other Point object.</param>
    /// <returns>The distance between the two points.</returns>
    public SqlDouble DistanceTo(Point p2)
    {
        if (IsNull || p2.IsNull)
        {
            return SqlDouble.Null;
        }

        return Math.Sqrt(Math.Pow((double)(X - p2.X), 2) + Math.Pow((double)(Y - p2.Y), 2));
    }

    /// <summary>
    /// Calculates the polar angle of the point.
    /// </summary>
    /// <returns>The polar angle of the point in radians. Returns 0 if the point is null.</returns>
    public double PolarAngle()
    {
        if (IsNull) { return 0; }

        return Math.Atan2(_y, _x);
    }


    /// <summary>
    /// Serializes object
    /// </summary>
    public void Write(BinaryWriter w)
    {
        w.Write(_isNull);
        if (!_isNull)
        {
            w.Write(_x);
            w.Write(_y);
        }
    }

    /// <summary>
    /// Deserializes object
    /// </summary>
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
