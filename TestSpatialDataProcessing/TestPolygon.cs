using System.Data.SqlTypes;

namespace TestSpatialDataProcessing {
    [TestClass]
    public class TestPolygon
    {
        [TestMethod]
        public void Constructor_ValidInput_ReturnsPolygon()
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
        [DataRow("((0 0), (1 1), (1 0), (0 1))")]
        [DataRow("((0 0), (0 1))")]
        [DataRow("(())")]
        [DataRow("()")]
        [DataRow("")]
        public void Constructor_InvalidInput_Throws(string pointsString)
        {
            Point[] points = Utils.SqlStringToPointsArray(new SqlString(pointsString));
            Assert.ThrowsException<SqlTypeException>(() => new Polygon(points));
        }


        [TestMethod]
        public void Parse_ValidInput_ReturnsPolygon()
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
        public void Parse_NullInput_ReturnsPolygon()
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

            Assert.AreEqual("", result);
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
        public void Circumference_NullInput_ReturnsNullValue()
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
            CollectionAssert.AreEqual(Polygon.ParseEdges(Array.Empty<Point>()), null);
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
            CollectionAssert.AreEquivalent(Polygon.ParseEdges(ps), edges);
        }

        [TestMethod]
        public void ContainsPoint_NullInput_ReturnsFalse()
        {
            Polygon polygon = new Polygon(new Point[] { new Point(0, 0), new Point(0, 1), new Point(1, 1) });
            Assert.AreEqual(polygon.ContainsPoint(Point.Null), false);
        }

        [TestMethod]
        public void ContainsPoint_NullObject_ReturnsFalse()
        {
            Assert.AreEqual(Polygon.Null.ContainsPoint(new Point(0,0)), false);
        }

        [TestMethod]
        [DataRow(0.5, 0.5, true)]
        [DataRow(0, -0.1, false)]
        [DataRow(0, 0, false)]
        [DataRow(0, 0.5, false)]
        [DataRow(1e-12, 1e-12, true)]
        public void ContainsPoint_ValidInput_ReturnsExpectedValue(double x, double y, bool expected)
        {
            Polygon polygon = new Polygon(new Point[] { new Point(0, 0), new Point(0, 1), new Point(1, 1), new Point(1, 0) });
            Point point = new Point(x, y);

            Boolean result = (bool) polygon.ContainsPoint(point);

            Assert.AreEqual(result, expected);
        }

        [TestMethod]
        [DataRow("((1 1), (2 2))", false)]
        [DataRow("((0 0), (1 1))", false)]
        [DataRow("((0 0), (2 1))", true)]
        public void IntersectsLine_ValidInput_ReturnsExpectedValue(string lineStr, bool expected)
        {
            var points = new Point[]
            {
                new Point(0, 0),
                new Point(0, 1),
                new Point(1, 1),
                new Point(1, 0)
            };
            Polygon p = new Polygon(points);
            Line l = Line.Parse(lineStr);
            Assert.AreEqual(p.IntersectsLine(l), expected);
        }
    } 
}
