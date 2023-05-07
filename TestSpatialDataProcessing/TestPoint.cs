using System.Data.SqlTypes;

namespace TestSpatialDataProcessing
{
    [TestClass]
    public class TestPoint {
        [TestMethod]
        public void  Constructor_ValidInput_ReturnsValidObject()
        {
            var point = new Point(new SqlDouble(1.0), new SqlDouble(2.0));
            Assert.AreEqual(1.0, point.X.Value);
            Assert.AreEqual(2.0, point.Y.Value);
        }

        [TestMethod]
        public void  Parse_ValidInput_ReturnsValidObject()
        {
            var point = Point.Parse(new SqlString("(5.432 2.345)"));
            Assert.AreEqual(5.432, point.X.Value);
            Assert.AreEqual(2.345, point.Y.Value);
        }

        [TestMethod]
        public void  ToString_ValidInput_ReturnsValidString()
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
        public void  DistanceTo_ValidInput_ReturnsValidValue()
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