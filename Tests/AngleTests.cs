using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tests;

namespace Data
{
    [TestClass]
    public class AngleTests : BaseTestClass
    {
        float[] raw, expected;

        public AngleTests()
        {
            raw = new float[]
            {
                0, 1, -1, 1 + PI, -1 - PI
            };

            expected = new float[]
            {
                0, 1, -1, -PI + 1, PI - 1
            };
        }

        //[TestMethod]
        //public void AAARunFirst() { }

        [TestMethod]
        public void Constructor()
        {
            for (int i = 0; i < raw.Length; i++)
            {
                float f = (float)new Angle(raw[i]);
                AssertAngleEquals(expected[i], f);
            }
        }

        [TestMethod]
        public void MathsTrivial()
        {
            Angle a = -1f, b = 1f, c = 2f;

            Assert.AreEqual(b, a+c);
            Assert.AreEqual(b, c-b);
            Assert.AreEqual(a, b-c);
        }

        [TestMethod]
        public void MathsRectify()
        {
            Angle a = 3f, b = 1f, c = -PI + (4 - PI);

            Assert.AreEqual(a + b, c);
            Assert.AreEqual(-a - b, -c);
        }

        [TestMethod]
        public void Intercepts()
        {
            Angle a = 1, b = 2, i = 1.5f, j = 3;
            Angle c = 3, d = -2, h = -3, k = 0;

            Assert.IsTrue(a.Intercepts(i, b), "i");
            Assert.IsFalse(a.Intercepts(j, b), "j");
            Assert.IsTrue(c.Intercepts(h, d), "h");
            Assert.IsFalse(c.Intercepts(k, d), "k");
        }
    }
}