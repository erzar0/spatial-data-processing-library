using System.Data.SqlTypes;

namespace TestSpatialDataProcessing
{
    [TestClass]
    public class TestPointSet
    {
        [TestMethod]
        public void PointSet_Constructor_ValidInput()
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
        public void PointSet_Constructor_NullInput()
        {
            var points = new Point[] { };

            Assert.AreEqual(new PointSet(points), PointSet.Null);
            Assert.AreEqual(new PointSet(null), PointSet.Null);
        }

        [TestMethod]
        public void PointSet_Parse_ValidInput()
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
        public void PointSet_Parse_NullInput()
        {
            var input = SqlString.Null;

            var pointSet = PointSet.Parse(input);

            Assert.IsTrue(pointSet.IsNull);
        }

        [TestMethod]
        public void PointSet_ToString_ValidInput()
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
        public void PointSet_ToString_NullInput()
        {
            var pointSet = PointSet.Null;

            var result = pointSet.ToString();

            Assert.AreEqual("NULL", result);
        }

        [TestMethod]
        public void PointSet_FindConvexHull_NullInput()
        {
            Assert.AreEqual(new PointSet(null).FindConvexHull(), Polygon.Null);
        }

        [TestMethod]
        public void PointSet_FindConvexHull_ValidInput()
        {
            Point[] convexHullPoints = {
                new Point(-10, -10),
                new Point(10, 10),
                new Point(10, -10),
                new Point(-10, 10),
                new Point(-10, 0),
            };
            Point[] pointSetPoints = new Point[]
            {
                new Point(0, 0),
                new Point(0, 1),
                new Point(1, 1),
                new Point(1, 0)
            }.Concat(convexHullPoints).ToArray();

            PointSet pointset = new PointSet(pointSetPoints);
            Polygon polygon = new Polygon(convexHullPoints);
            Console.WriteLine(pointset.FindConvexHull() + " " + polygon);

            CollectionAssert.AreEqual(pointset.FindConvexHull().Points.OrderBy(p => p.X).ThenBy(p => p.Y).ToArray()
                                    , polygon.Points.OrderBy(p => p.X).ThenBy(p => p.Y).ToArray());
        }

    }
}
