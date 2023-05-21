using System.Data.SqlTypes;
using System.Runtime.InteropServices;
using System.Timers;

namespace TestSpatialDataProcessing
{
    [TestClass]
    public class TestPointSet
    {
        [TestMethod]
        public void  Constructor_ValidInput_ReturnsPointSet()
        {
            var points = new Point[]
            {
                new Point(0, 0),
                new Point(0, 1),
                new Point(1, 1),
                new Point(1, 0)
            };

            var pointSet = new PointSet(points);

            Assert.IsFalse(pointSet.IsNull);
            CollectionAssert.AreEqual(points, pointSet.Points);
        }

        [TestMethod]
        public void  Constructor_NullInput_ReturnsPointset()
        {
            var points = new Point[] { };

            Assert.AreEqual(new PointSet(points), PointSet.Null);
            Assert.AreEqual(new PointSet(null), PointSet.Null);
        }

        [TestMethod]
        public void  Constructor_PolygonInput_ReturnsPointSet()
        {
            var input = new SqlString("((0 0), (0 1), (1 1), (1 0))");
            var polygon = Polygon.Parse(input);

            CollectionAssert.AreEquivalent(new PointSet(polygon).Points, polygon.Points);
            Assert.AreEqual(new PointSet(Polygon.Null), PointSet.Null);
        }

        [TestMethod]
        public void  Parse_ValidInput_ReturnsValidObject()
        {
            var expectedPoints = new Point[]
            {
                new Point(0, 0),
                new Point(0, 1),
                new Point(1, 1),
                new Point(1, 0)
            };
            var input = new SqlString("((0 0), (0 1), (1 1), (1 0))");

            var pointSet = PointSet.Parse(input);

            Assert.IsFalse(pointSet.IsNull);
            CollectionAssert.AreEqual(expectedPoints, pointSet.Points);
        }

        [TestMethod]
        public void  Parse_NullInput_ReturnsNullObject()
        {
            var input = SqlString.Null;

            var pointSet = PointSet.Parse(input);

            Assert.IsTrue(pointSet.IsNull);
        }

        [TestMethod]
        public void  ToString_ValidInput_ReturnsValidString()
        {
            var points = new Point[]
            {
                new Point(0, 0),
                new Point(0, 1),
                new Point(1, 1),
                new Point(1, 0)
            };
            var expectedString = "((0 0),(0 1),(1 1),(1 0))";
            var pointSet = new PointSet(points);

            var result = pointSet.ToString();

            Assert.AreEqual(expectedString, result);
        }

        [TestMethod]
        public void  ToString_NullInput_ReturnsNullString()
        {
            var pointSet = PointSet.Null;

            var result = pointSet.ToString();

            Assert.AreEqual("NULL", result);
        }

        [TestMethod]
        public void  FindConvexHull_NullInput_ReturnsNullPolygon()
        {
            Assert.AreEqual(new PointSet(null).FindConvexHull(), Polygon.Null);
        }

        [TestMethod]
        public void  FindConvexHull_ValidInput_ReturnsValidPolygon()
        {
            Point[] convexHullPoints = {
                new Point(-10, -10),
                new Point(10, 10),
                new Point(10, -10),
                new Point(-10, 10)
            };
            Point[] pointSetPoints = new Point[]
            {
                new Point(0, 0),
                new Point(-1, -1),
                new Point(1, 1),
                new Point(1, -1),
                new Point(-1, -1),
            }.Concat(convexHullPoints).ToArray();

            PointSet pointset = new PointSet(pointSetPoints);

            pointset.FindConvexHull().Points.ToList().ForEach(e => Console.WriteLine(e));
            CollectionAssert.AreEquivalent(pointset.FindConvexHull().Points, convexHullPoints);
        }

    }
}
