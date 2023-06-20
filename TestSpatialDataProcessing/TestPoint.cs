using System.Data.SqlTypes;

namespace TestSpatialDataProcessing
{
    [TestClass]
    public class TestPoint {
        [TestMethod]
        public void  Constructor_ValidInput_ReturnsPoint()
        {
            var point = new Point(new SqlDouble(1.0), new SqlDouble(2.0));
            Assert.AreEqual(1.0, point.X.Value);
            Assert.AreEqual(2.0, point.Y.Value);
        }

        [TestMethod]
        public void Constructor_PointInput_CopiesPoint()
        {
            var point = new Point(1, 2);
            var copiedPoint = new Point(point);

            Assert.IsTrue(copiedPoint.Equals(point));
            copiedPoint.X = 3;
            Assert.IsFalse(copiedPoint.Equals(point));
        }

        [TestMethod]
        public void OperatorPlus_ReturnsNewPoint()
        {
            Point p1 = new Point(1, 1);
            Point p2 = +p1;
            p1.X = 0;
            Assert.IsFalse(p1.Equals(p2));
        }

        [TestMethod]
        public void OperatorMinus_ReturnsNewNegativePoint()
        {
            Point p1 = new Point(1, 1);
            Point p2 = -p1;
            Assert.IsTrue((bool)(p1.X != p2.X));
        }

        [TestMethod]
        public void OperatorPlus_AddsPoint()
        {
            Point p1 = new Point(1, 1);
            Point p2 = new Point(2, 2);
            Point p3 = p1 + p2;
            Point p4 = new Point(3, 3);
            Assert.IsTrue(p3.Equals(p4));
        }

        [TestMethod]
        public void OperatorMinus_SubtractsPoint()
        {
            Point p1 = new Point(1, 1);
            Point p2 = new Point(2, 2);
            Point p3 = p1 - p2;
            Point p4 = new Point(-1, -1);
            Assert.IsTrue(p3.Equals(p4));
        }

        [TestMethod]
        public void OperatorMultiply_MultiplyPointByScale()
        {
            Point p1 = new Point(1, 1);
            Point p2 = p1 * 2;
            Point p3 = new Point(2, 2);
            Assert.IsTrue(p2.Equals(p3));
        }

        [TestMethod]
        public void OperatorDivide_DividesPointByScale()
        {
            Point p1 = new Point(2, 2);
            Point p2 = p1 / 2;
            Point p3 = new Point(1, 1);
            Assert.IsTrue(p2.Equals(p3));
        }

        [TestMethod]
        public void  Parse_ValidInput_ReturnsPoint()
        {
            var point = Point.Parse(new SqlString("(5.432 2.345)"));
            Assert.AreEqual(5.432, point.X.Value);
            Assert.AreEqual(2.345, point.Y.Value);
        }

        [TestMethod]
        public void  ToString_ValidInput_ReturnsExpectedString()
        {
            var point = new Point(new SqlDouble(1.234), new SqlDouble(4.321));
            Assert.AreEqual("(1.234 4.321)", point.ToString());
        }

        [TestMethod]
        public void  IsNull_NullInput_ReturnsExpectedValue()
        {
            var nullPoint = Point.Null;
            Assert.IsTrue(nullPoint.IsNull);

            var point = new Point(new SqlDouble(1.0), new SqlDouble(2.0));
            Assert.IsFalse(point.IsNull);
        }

        [TestMethod]
        public void  DistanceTo_ValidInput_ReturnsExpectedValue()
        {
            var point1 = new Point(new SqlDouble(0.0), new SqlDouble(0.0));
            var point2 = new Point(new SqlDouble(3.0), new SqlDouble(4.0));
            var distance = point1.DistanceTo(point2);
            Assert.AreEqual(5.0, distance.Value);
        }

        [TestMethod]
        public void  WriteAndRead_ValidInput_WorksCorrectly()
        {
            var point = new Point(new SqlDouble(1.0), new SqlDouble(2.0));
            using (var stream = new MemoryStream())
            {
                var writer = new BinaryWriter(stream);
                point.Write(writer);

                stream.Position = 0;

                var reader = new BinaryReader(stream);
                var deserializedPoint = new Point();
                deserializedPoint.Read(reader);

                Assert.AreEqual(point.X.Value, deserializedPoint.X.Value);
                Assert.AreEqual(point.Y.Value, deserializedPoint.Y.Value);
                Assert.AreEqual(point.IsNull, deserializedPoint.IsNull);
            }
      
        }   
    }
}