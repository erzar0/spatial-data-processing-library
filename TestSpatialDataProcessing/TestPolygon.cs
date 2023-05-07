using System.Data.SqlTypes;

namespace TestSpatialDataProcessing {
    [TestClass]
    public class TestPolygon
    {
        [TestMethod]
        public void Constructor_ValidInput_ReturnsValidObject()
        {
            var points = new Point[]
            {
                new Point(0, 0),
                new Point(0, 1),
                new Point(1, 1),
                new Point(1, 0)
            };

            var polygon = new Polygon(points);

            Assert.IsFalse(polygon.IsNull);
            CollectionAssert.AreEqual(points, polygon.Points);
        }

        [TestMethod]
        public void Constructor_InvalidInput_Throws()
        {
            var points = new Point[]
            {
                new Point(0, 0),
                new Point(0, 1)
            };

            Assert.ThrowsException<SqlTypeException>(() => new Polygon(points));
        }

        [TestMethod]
        public void Parse_ValidInput_ReturnsValidObject()
        {
            var expectedPoints = new Point[]
            {
                new Point(0, 0),
                new Point(0, 1),
                new Point(1, 1),
                new Point(1, 0)
            };
            var input = new SqlString("((0 0), (0 1), (1 1), (1 0))");

            var polygon = Polygon.Parse(input);

            Assert.IsFalse(polygon.IsNull);
            CollectionAssert.AreEqual(expectedPoints, polygon.Points);
        }

        [TestMethod]
        public void Parse_NullInput_ReturnsNullObject()
        {
            var input = SqlString.Null;

            var polygon = Polygon.Parse(input);

            Assert.IsTrue(polygon.IsNull);
        }

        [TestMethod]
        public void ToString_ValidInput_ReturnsValidString()
        {
            var points = new Point[]
            {
                new Point(0, 0),
                new Point(0, 1),
                new Point(1, 1),
                new Point(1, 0)
            };
            var expectedString = "((0 0),(0 1),(1 1),(1 0))";
            var polygon = new Polygon(points);

            var result = polygon.ToString();

            Assert.AreEqual(expectedString, result);
        }

        [TestMethod]
        public void ToString_NullInput_ReturnsNullString()
        {
            var polygon = Polygon.Null;

            var result = polygon.ToString();

            Assert.AreEqual("NULL", result);
        }

        [TestMethod]
        public void Circumference_ValidInput_ReturnsCircumference()
        {
            var points = new Point[]
            {
                new Point(0, 0),
                new Point(0, 1),
                new Point(1, 1),
                new Point(1, 0)
            };
            var polygon = new Polygon(points);
            var expectedCircumference = 4.0;

            var result = polygon.Circumference();

            Assert.AreEqual(expectedCircumference, result.Value);
        }

        [TestMethod]
        public void Circumference_NullInput_ReturnsNull()
        {
            var polygon = Polygon.Null;

            var result = polygon.Circumference();

            Assert.IsTrue(result.IsNull);
        }

        [TestMethod]
        public void Area_ValidInput_ValidArea()
        {
            var polygon = Polygon.Parse("((0 0), (0 1), (1 1), (1 0))");

            Assert.AreEqual(polygon.Area(), 1.0);
        }
        
        [TestMethod]
        public void Area_NullInput_ReturnsNull()
        {
            var polygon = Polygon.Null;

            Assert.AreEqual(polygon.Area(), SqlDouble.Null);
        }

        [TestMethod]
        public void ParseEdges_InvalidInput_ReturnsNull()
        {
            Point[] ps =
            {
                new Point(0, 0),
            };
            CollectionAssert.AreEqual(Polygon.ParsedEdges(ps), null);
            CollectionAssert.AreEqual(Polygon.ParsedEdges(null), null);

        }

        [TestMethod]
        public void ParseEdges_ValidInput_ReturnsPolygonEdges()
        {
            Point[] ps =
            {
                new Point(0, 0),
                new Point(0, 1),
                new Point(1, 1)
            };
            List<Line> edges = new List<Line>() {
                new Line(ps[0], ps[1]),
                new Line(ps[1], ps[2]),
                new Line(ps[2], ps[0])
            };
            CollectionAssert.AreEqual(Polygon.ParsedEdges(ps), edges);
        }

        [TestMethod]
        public void ContainsPoint_NullInput_ReturnsFalse()
        {
            Polygon polygon = new Polygon(new Point[] { new Point(0, 0), new Point(0, 1), new Point(1, 1) });
            Assert.AreEqual(polygon.ContainsPointInside(Point.Null), false);
        }

        [TestMethod]
        public void ContainsPoint_NullObject_ReturnsFalse()
        {
            Assert.AreEqual(Polygon.Null.ContainsPointInside(new Point(0,0)), false);
        }

        [TestMethod]
        [DataRow(0.5, 0.5, true)]
        [DataRow(0, -0.1, false)]
        [DataRow(0, 0, false)]
        [DataRow(0, 0.5, false)]
        [DataRow(10e-10, 10e-10, true)]
        public void ContainsPoint_ValidInput_ReturnsExpectedValue(double x, double y, bool expected)
        {
            Polygon polygon = new Polygon(new Point[] { new Point(0, 0), new Point(0, 1), new Point(1, 1), new Point(1, 0) });
            Point point = new Point(x, y);

            Boolean result = (bool) polygon.ContainsPointInside(point);

            Assert.AreEqual(result, expected);
        }


    } 
}
