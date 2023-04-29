using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;


    [Serializable]
    [SqlUserDefinedType(Format.Native)]
    public struct Point2d : INullable
    {
        public Point2d(double x=0, double y = 0)
        {
            this._x = x;
            this._y = y;
            this._isNull = false;
        }
        

        [SqlMethod(OnNullCall = false)]
        public static Point2d Parse(SqlString s)
        {
            if (s.IsNull) { return Null; }

            string[] xy = s.ToString().Trim('(', ')', ' ').Split(",".ToCharArray());
            double x = Double.Parse(xy[0]);
            double y = Double.Parse(xy[1]);
            return new Point2d(x, y);
        }
        public override string ToString()
        {
            if (this.IsNull) { return "NULL"; }

            return $"({_x}, {_y})";
        }

        public static Point2d Null
        {
            get
            {
                Point2d p = new Point2d();
                p._isNull = true;
                return p;
            }
        }
        
        public bool IsNull
        {
            get
            {
                return _isNull;
            }
        }


        public double X
        {
            get { return _x; }
            set { _x = value; }
        }
        public double Y 
        {
            get { return _y; }
            set { _y = value; }
        }
        public SqlDouble DistanceTo(Point2d p2)
        {
            if(this.IsNull || p2.IsNull)
            {
                return SqlDouble.Null;
            }

        return Math.Sqrt(Math.Pow(this.X - p2.X, 2) + Math.Pow(this.Y - p2.Y, 2));

    }

    private bool _isNull;
        private double _x;
        private double _y;
    }  

