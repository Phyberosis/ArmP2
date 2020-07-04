using System;
using System.IO;
using Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class BaseTestClass
    {
        protected const float PI = (float)Math.PI;
        protected const float DELTA = 0.001f;

        [TestMethod]
        public void AAARunFirst()
        {
        }

        public void AssertAngleEquals(Angle expected, Angle given, string msg = "")
        {
            Assert.IsTrue(Math.Abs((float)(expected - given)) < DELTA, "given " + given + " expected " + expected + msg);
        }
    }
}
