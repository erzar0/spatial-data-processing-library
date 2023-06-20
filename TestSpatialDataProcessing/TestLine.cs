using System.Data.SqlTypes;



namespace TestSpatialDataProcessing
{
    [TestClass]
    public class TestLine
    {
        [TestMethod]
        public void   Parse_ValidInput_ReturnsExpectedObject()
        {
            var line = Line.Parse(new SqlString(@" ( ( 1 2 )
                                                , ( 3 4 )   )"));
            Assert.AreEqual(line.A, new Point(1, 2));
            Assert.AreEqual(line.B, new Point(3, 4));
        }

        [TestMethod]
        public void   Parse_NullInput_ReturnsNullObject()
        {
            var line = Line.Parse(SqlString.Null);
            Assert.AreEqual(line.IsNull, true);
        }

        [TestMethod]
        public void   ToString_NullInput_ReturnsEmptyString()
        {
            var line = Line.Null;
            Assert.AreEqual(line.ToString(), "");
        }

        [TestMethod]
        public void   Length_NullObject_ReturnsNullObject()
        {
            var line = Line.Null;
            Assert.AreEqual(line.Length().IsNull, true);
        }

        [TestMethod]
        public void   Length_ValidInput_ReturnsValidValue()
        {
            var line = new Line(new Point(0, 0), new Point(3, 4));
            Assert.AreEqual(line.Length().Value, 5.0);
        }

        [TestMethod]
        public void   ContainsPoint_NullObject_ReturnsFalse()
        {
            var line = Line.Null;
            Assert.AreEqual(line.ContainsPoint(new Point(1, 2), 0.01), false);
        }

        [TestMethod]
        [DataRow(1, 1.1, 0.11, true)]
        [DataRow(1, 1.1, 0.01, false)]
        [DataRow(2, 1, 0.01, false)]
        public void   ContainsPoint_ValidInput_ReturnsExpectedValue(double x, double y, double eps, bool expected)
        {
            var line = new Line(new Point(0, 0), new Point(2, 2));
            Point point = new Point(x, y);
            bool result = (bool) line.ContainsPoint(point, eps);
            Assert.AreEqual(result, expected);
        }


        [TestMethod]
        public void  Intersects_NullObject_ReturnsFalse()
        {
            var line = Line.Null;
            var line2 = Line.Parse("((0 0), (0 1)");
            Assert.AreEqual(line.Intersects(line2), SqlBoolean.False);
        }

        [TestMethod]
        [DataRow("((0 0), (1 1))", "((0 1), (1 0))", true)]
        [DataRow("((0 0), (1 1))", "((0 1), (1 1))", false)]
        [DataRow("((0 0), (1 1))", "((0 1), (1 1.00000001))", false)]
        [DataRow("((0 0), (1 1))", "((0 1), (1 0.99999999))", true)]
        [DataRow("((1 1), (1 0))", "((0 0), (2 1))", true)]
        [DataRow("((0 0), (1 1))", "((1 0), (0 1))", true)]
        public void   Intersects_ValidInput_ReturnsExpectedValue(string str1, string str2, bool expected)
        {
            var line = Line.Parse(str1);
            var line2 = Line.Parse(str2);
            Console.WriteLine(line.Intersects(line2));
            Console.WriteLine(line.Intersects(line));

            Assert.AreEqual((bool)line.Intersects(line2), expected);
            Assert.AreEqual((bool)line2.Intersects(line), expected);
        }

        [TestMethod]
        public void GetSlopeValue_NullLine_ReturnsNull()
        {
            Line line = Line.Null;
            SqlDouble slope = line.GetSlopeValue();
            Assert.IsTrue(slope.IsNull);
        }

        [TestMethod]
        public void GetSlopeValue_ValidLine_ReturnsPositiveSlope()
        {
            Point a = new Point(1, 1);
            Point b = new Point(3, 5);
            Line line = new Line(a, b);
            SqlDouble slope = line.GetSlopeValue();
            Assert.AreEqual(2, slope.Value);
        }

        [TestMethod]
        public void GetSlopeValue_ValidLine_ReturnsNegativeSlope()
        {
            Point a = new Point(-1, 1);
            Point b = new Point(1, -1);
            Line line = new Line(a, b);
            SqlDouble slope = line.GetSlopeValue();
            Assert.AreEqual(-1, slope.Value);
        }

        [TestMethod]
        public void GetInterceptValue_NullLine_ReturnsNull()
        {
            Line line = Line.Null;
            SqlDouble intercept = line.GetInterceptValue();
            Assert.IsTrue(intercept.IsNull);
        }

        [TestMethod]
        public void GetInterceptValue_ValidLine_ReturnsIntercept()
        {
            Point a = new Point(1, 1);
            Point b = new Point(3, 5);
            Line line = new Line(a, b);
            SqlDouble intercept = line.GetInterceptValue();
            Assert.AreEqual(-1, intercept.Value);
        }
    }
}
