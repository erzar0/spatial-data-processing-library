using System.Data.SqlTypes;

namespace TestSpatialDataProcessing {
    [TestClass]
    public class TestPolygon
    {
        [TestMethod]
        public void Polygon_Constructor_ValidInput()
        {
            // Arrange
            var points = new Point[]
            {
                new Point(0, 0),
                new Point(0, 1),
                new Point(1, 1),
                new Point(1, 0)
            };

            // Act
            var polygon = new Polygon(points);

            // Assert
            Assert.IsFalse(polygon.IsNull);
            CollectionAssert.AreEqual(points, polygon.Points);
        }

        [TestMethod]
        public void Polygon_Constructor_InvalidInput()
        {
            // Arrange
            var points = new Point[]
            {
                new Point(0, 0),
                new Point(0, 1)
            };

            // Act & Assert
            Assert.ThrowsException<SqlTypeException>(() => new Polygon(points));
        }

        [TestMethod]
        public void Polygon_Parse_ValidInput()
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
        public void Polygon_Parse_NullInput()
        {
            var input = SqlString.Null;

            var polygon = Polygon.Parse(input);

            Assert.IsTrue(polygon.IsNull);
        }

        [TestMethod]
        public void Polygon_ToString_ValidInput()
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
        public void Polygon_ToString_NullInput()
        {
            var polygon = Polygon.Null;

            var result = polygon.ToString();

            Assert.AreEqual("NULL", result);
        }

        [TestMethod]
        public void Polygon_Circumference_ValidInput()
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
        public void Polygon_Circumference_NullInput()
        {
            var polygon = Polygon.Null;

            var result = polygon.Circumference();

            Assert.IsTrue(result.IsNull);
        }

        [TestMethod]
        public void Polygon_Area_ValidInput()
        {
            var polygon = Polygon.Parse("((0 0), (0 1), (1 1), (1 0))");

            Assert.AreEqual(polygon.Area(), 1.0);
        }
        
        [TestMethod]
        public void Polygon_Area_NullInput()
        {
            var polygon = Polygon.Null;

            Assert.AreEqual(polygon.Area(), SqlDouble.Null);
        }

        [TestMethod]
        public void Polygon_ParseEdges_InvalidInput()
        {
            Point[] ps =
            {
                new Point(0, 0),
            };
            CollectionAssert.AreEqual(Polygon.ParsedEdges(ps), null);
            CollectionAssert.AreEqual(Polygon.ParsedEdges(null), null);

        }

        [TestMethod]
        public void Polygon_ParseEdges_ValidInput()
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

    } 
}
