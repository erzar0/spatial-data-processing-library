using System.Data.SqlTypes;



namespace TestSpatialDataProcessing
{
    [TestClass]
    public class LineTests
    {
        [TestMethod]
        public void Line_Parse_ValidInput()
        {
            var line = Line.Parse(new SqlString("((1 2),(3 4))"));
            Assert.AreEqual(line.A, new Point(1, 2));
            Assert.AreEqual(line.B, new Point(3, 4));
        }

        [TestMethod]
        public void Line_Parse_NullInput()
        {
            var line = Line.Parse(SqlString.Null);
            Assert.AreEqual(line.IsNull, true);
        }

        [TestMethod]
        public void Line_ToString_NullInput()
        {
            var line = Line.Null;
            Assert.AreEqual(line.ToString(), "NULL");
        }

        [TestMethod]
        public void Line_Length_NullInput()
        {
            var line = Line.Null;
            Assert.AreEqual(line.Length().IsNull, true);
        }

        [TestMethod]
        public void Line_Length_ValidInput()
        {
            var line = new Line(new Point(0, 0), new Point(3, 4));
            Assert.AreEqual(line.Length().Value, 5.0);
        }

        [TestMethod]
        public void Line_ContainsPoint_NullInput()
        {
            var line = Line.Null;
            Assert.AreEqual(line.ContainsPoint(new Point(1, 2), 0.01), false);
        }

        [TestMethod]
        public void Line_ContainsPoint_ValidInput()
        {
            var line = new Line(new Point(0, 0), new Point(2, 2));
            Assert.AreEqual(line.ContainsPoint(new Point(1, 1.1), 0.11), true);
        }

        [TestMethod]
        public void Line_ContainsPoint_ValidInput2()
        {
            var line = new Line(new Point(0, 0), new Point(2, 2));
            Assert.AreEqual(line.ContainsPoint(new Point(1, 1.1), 0.01), false);
        }

        [TestMethod]
        public void Line_ContainsPoint_ValidInput3()
        {
            var line = new Line(new Point(0, 1), new Point(1, 1));
            Assert.AreEqual(line.ContainsPoint(new Point(2, 1), 0.01), false);
        }

        [TestMethod]
        public void Line_CrossesLine_NullInput()
        {
            var line = Line.Null;
            var line2 = Line.Parse("((0 0), (0 1)");
            Assert.AreEqual(line.CrossesLine(line2), SqlBoolean.False);
        }

        [TestMethod]
        public void Line_CrossesLine_ValidInput()
        {
            var line = Line.Parse("((0 0), (1 1))");
            var line2 = Line.Parse("((0 1), (1 0))"); 
            Assert.AreEqual(line.CrossesLine(line2), SqlBoolean.True);
        }

        [TestMethod]
        public void Line_CrossesLine_ValidInput2()
        {
            var line = Line.Parse("((0 0), (1 1))");
            var line2 = Line.Parse("((0 1), (1 1))"); 
            Assert.AreEqual(line.CrossesLine(line2), SqlBoolean.True);
        }

        [TestMethod]
        public void Line_CrossesLine_ValidInput3()
        {
            var line = Line.Parse("((0 0), (1 1))");
            var line2 = Line.Parse("((0 1), (1 1.00000001))"); 
            Assert.AreEqual(line.CrossesLine(line2), SqlBoolean.False);
        }
    }
}
