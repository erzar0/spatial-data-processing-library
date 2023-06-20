using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Utils;

namespace TestSpatialDataProcessing
{
    [TestClass]
    public class TestUtils
    {
        [TestMethod]
        public void Orientation_CollinearPoints_ReturnsCollinear()
        {
            Point p = new Point(1, 1);
            Point q = new Point(2, 2);
            Point r = new Point(3, 3);

            ORIENTATION orientation = Utils.Orientation(p, q, r);

            Assert.AreEqual(ORIENTATION.COLLINEAR, orientation);
        }

        [TestMethod]
        public void Orientation_ClockwisePoints_ReturnsClockwise()
        {
            Point p = new Point(1, 1);
            Point q = new Point(2, 2);
            Point r = new Point(3, 1);

            ORIENTATION orientation = Utils.Orientation(p, q, r);

            Assert.AreEqual(ORIENTATION.CLOCKWISE, orientation);
        }

        [TestMethod]
        public void Orientation_CounterClockwisePoints_ReturnsCounterClockwise()
        {
            Point p = new Point(1, 1);
            Point q = new Point(2, 2);
            Point r = new Point(1, 3);

            ORIENTATION orientation = Utils.Orientation(p, q, r);

            Assert.AreEqual(ORIENTATION.COUNTERCLOCKWISE, orientation);
        }

        [TestMethod]
        public void CalculateCentroid_EmptyArray_ReturnsNullPoint()
        {
            Point[] points = new Point[0];

            Point centroid = Utils.CalculateCentroid(points);

            Assert.IsTrue(centroid.IsNull);
        }

        [TestMethod]
        public void CalculateCentroid_SinglePointArray_ReturnsSamePoint()
        {
            Point[] points = { new Point(2, 3) };

            Point centroid = Utils.CalculateCentroid(points);

            Assert.AreEqual(points[0], centroid);
        }

        [TestMethod]
        public void CalculateCentroid_MultiplePointsArray_ReturnsCorrectCentroid()
        {
            Point[] points = {
                new Point(1, 1),
                new Point(2, 2),
                new Point(3, 3),
                new Point(4, 4)
                };

            Point centroid = Utils.CalculateCentroid(points);

            Assert.AreEqual(new Point(2.5, 2.5), centroid);
        }

        [TestMethod]
        public void SqlStringToPointsArray_NullString_ReturnsNullArray()
        {
            SqlString sqlString = SqlString.Null;

            Point[] points = Utils.SqlStringToPointsArray(sqlString);

            Assert.IsNull(points);
        }

        [TestMethod]
        public void SqlStringToPointsArray_EmptyString_ReturnsNullArray()
        {
            SqlString sqlString = new SqlString("");

            Point[] points = Utils.SqlStringToPointsArray(sqlString);

            Assert.IsNull(points);
        }

        [TestMethod]
        public void SqlStringToPointsArray_ValidString_ReturnsArrayOfPoints()
        {
            SqlString sqlString = new SqlString(@"(1
                        2)
                    , (3   4 ), ( 5 6 ) ");

            Point[] points = Utils.SqlStringToPointsArray(sqlString);

            Assert.IsNotNull(points);
            Assert.AreEqual(3, points.Length);
            Assert.AreEqual(new Point(1, 2), points[0]);
            Assert.AreEqual(new Point(3, 4), points[1]);
            Assert.AreEqual(new Point(5, 6), points[2]);
        }

        [TestMethod]
        public void PointsArrayToString_NullArray_ReturnsEmptyString()
        {
            Point[] points = Array.Empty<Point>();

            string result = Utils.PointsArrayToString(points);

            Assert.AreEqual("", result);
        }

        [TestMethod]
        public void PointsArrayToString_EmptyArray_ReturnsEmptyString()
        {
            Point[] points = new Point[0];

            string result = Utils.PointsArrayToString(points);

            Assert.AreEqual("", result);
        }

        [TestMethod]
        public void PointsArrayToString_ArrayWithOnePoint_ReturnsCorrectString()
        {
            Point[] points = new Point[] { new Point(1, 2) };

            string result = Utils.PointsArrayToString(points);

            Assert.AreEqual("((1 2))", result);
        }

        [TestMethod]
        public void PointsArrayToString_ArrayWithMultiplePoints_ReturnsCorrectString()
        {
            Point[] points = new Point[] { new Point(1, 2), new Point(3, 4), new Point(5, 6) };

            string result = Utils.PointsArrayToString(points);

            Assert.AreEqual("((1 2),(3 4),(5 6))", result);
        }
    }
}
