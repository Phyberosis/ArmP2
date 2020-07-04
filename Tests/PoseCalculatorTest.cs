using System;
using System.Numerics;
using System.Security.Policy;
using Data;
using Data.Arm;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tests;

namespace Devices.Arm
{
    [TestClass]
    public class PoseCalculatorTest : BaseTestClass
    {
        ArmConfig config;
        PoseCalculator calc;

        public PoseCalculatorTest()
        {
            config = new ArmConfig();
            calc = new PoseCalculator(config);
        }   

        private void testAngleSet(Angle[] exp, Angle[] ans)
        {
            for (int i = 0; i < ans.Length; i++)
            {
                AssertAngleEquals(exp[i], ans[i], " Axis " + (i + 1));
            }
        }

        [TestMethod]
        public void ptaTPose()
        {
            Quaternion dir = Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)Math.PI / 2f);
            Vector3 pos = Vector3.Zero;
            pos.X = config.SHOULDER + config.FOREARM;
            pos.Z = config.UPPER_ARM - config.HAND;

            Angle[] ans = calc.PoseToAngles(pos, dir);
            Angle[] expected = new Angle[]
            {
                0, PI / 2, PI / 2,
                0, -PI/2, 0
            };

            testAngleSet(expected, ans);
        }

        [TestMethod]
        public void ptaAngledHand()
        {
            Quaternion dir = Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)Math.PI / 2f);
            Vector3 pos = Vector3.Zero;
            pos.X = config.SHOULDER + 25;
            pos.Z = 15 - config.HAND;

            Angle[] ans = calc.PoseToAngles(pos, dir);
            Angle[] expected = new Angle[]
            {
                0, 1.33081274731f, 1.12159299352f,
                0, (-PI/2) + 0.689186912764f, 0
            };

            testAngleSet(expected, ans);
        }

        [TestMethod]
        public void ptaComplex()
        {
            // setup
            // 45deg CC axis 1
            Quaternion dir = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, PI / 2f);
            Vector3 pos = Vector3.Zero;
            pos.X = (float)Math.Sin(Math.PI/4) * (config.SHOULDER + 25);
            pos.Z = 15;
            pos.Y = (float)Math.Sin(Math.PI / 4) * (config.SHOULDER + 25) + config.HAND;

            // ==========

            //q is forearm
            Quaternion q = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, PI / 4);
            Vector3 v = Vector3.Transform(-Vector3.UnitY, q);
            Console.WriteLine(v);

            q = Quaternion.CreateFromAxisAngle(v, -0.689186912764f) * q;
            Vector3 v2 = Vector3.Transform(Vector3.UnitX, q);
            Console.WriteLine(v2);

            // get hand
            Vector3 pointing = Vector3.UnitY;   // DEBUG
            Vector3 hand = Vector3.Transform(pointing, Quaternion.Inverse(q));
            Console.WriteLine(hand);

            Vector2 handR = new Vector2(hand.Y, hand.Z);        // squash to X
            Angle wristRot = -Math.Atan(handR.X / handR.Y);     // abusing tan
            Console.WriteLine(wristRot.toDegrees());

            // subtract wrist rot
            hand = Vector3.Transform(hand,
                Quaternion.CreateFromAxisAngle(Vector3.UnitX, -(float)wristRot));

            Vector2 handF = new Vector2(hand.X, hand.Z);        // squash to Y
            Angle wristFlex = Math.Atan(handF.Y / handF.X);     // actual tan
            if (handF.X < 0) wristFlex += PI;
            Console.WriteLine(wristFlex.toDegrees());

            // resultant hand
            Quaternion handTransform = Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float)wristRot);
            Vector3 wristFlexAxis = Vector3.Transform(-Vector3.UnitY, handTransform);
            handTransform = Quaternion.CreateFromAxisAngle(wristFlexAxis, (float)wristFlex) * handTransform;
            Vector3 calcHand = Vector3.Transform(Vector3.UnitX, handTransform);
            Console.WriteLine("calcH"+calcHand);

            // differece resultant and actual
            Quaternion diff = Quaternion.Inverse(q * handTransform) * dir;
            Console.WriteLine(Vector3.Transform(Vector3.UnitX, dir));
            Console.WriteLine(Vector3.Transform(Vector3.UnitX, q * handTransform));
            Angle handRot = 2f * Math.Acos(diff.W);
            if (handRot != 0 && handRot != PI)
            {
                if (diff.X < 0) handRot = -handRot;
                //since rotAxis.X = diff.X / Sqrt(1 - (diff.W * diff.W));
            }
            Console.WriteLine(handRot.toDegrees());

            Angle[] ans = calc.PoseToAngles(pos, dir);
            Angle[] expected = new Angle[]
            {
                PI / 4, 1.33081274731f, 1.12159299352f,
                wristRot, wristFlex, handRot
            };

            testAngleSet(expected, ans);
        }
    }
}
