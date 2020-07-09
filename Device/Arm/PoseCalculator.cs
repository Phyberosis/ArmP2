using Data;
using Data.Arm;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http.Headers;
using System.Numerics;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using Unosquare.RaspberryIO;

namespace Devices.Arm
{
    public class PoseCalculator
    {
        private float PI = (float)Math.PI;

        private ArmConfig config;

        public PoseCalculator(ArmConfig c)
        {
            config = c;
        }

        public Angle[] PoseToAngles(Vector3 pos, Quaternion dir)
        {
            Console.WriteLine("Pose to angles");
            //orientation offset
            Vector3 handOffset = config.HAND * Vector3.Transform(Vector3.UnitX, dir);
            Vector3 target = pos - handOffset;
            //Console.WriteLine("handoffset " + handOffset);
            Console.Write("ho : " + handOffset + " targ : " + target);

            //project onto Z plane
            Vector2 xy = new Vector2(target.X, target.Y);

            //project onto (distZ, Z) plane
            float distZ = xy.Length();
            distZ -= config.SHOULDER;
            Vector2 dz = new Vector2(distZ, target.Z);
            Console.WriteLine(" distZ : " + distZ);

            // shoulder rot
            Angle shoulderRot = (float)Math.Atan(xy.Y / xy.X);
            if (target.X < 0 ^ distZ < 0) shoulderRot += PI;

            float l1sq = config.UPPER_ARM_SQ;
            float l2sq = config.FOREARM_SQ;
            float l3sq = dz.LengthSquared();

            float l1 = config.UPPER_ARM;
            float l2 = config.FOREARM;
            float l3 = (float)Math.Sqrt(l3sq);

            // elbow
            Angle L2 = (float)Math.Acos((l1sq - l2sq + l3sq) / (2 * l1 * l3));
            Angle L3;
            if (l3 < config.UPPER_ARM + config.FOREARM) // range guard
                L3 = (float)Math.Acos((l1sq + l2sq - l3sq) / (2 * l1 * l2));
            else
                L3 = PI;    // todo min range guard
            Angle elbow = L3;

            // upper arm elevation
            Angle targetElevation = (float)Math.Atan(target.Z / distZ);
            if (distZ < 0)
                targetElevation += PI;
            Angle upperArmElevation = L2 + targetElevation;

            // forearm elevation
            Angle forearmElevation = -PI + elbow + upperArmElevation;

            //Console.WriteLine("ga");
            return getWristAngles(handOffset, dir,
                shoulderRot, upperArmElevation, elbow, forearmElevation);
        }

        private Angle[] getWristAngles(Vector3 handOffset, Quaternion dir,
            Angle shoulderRot, Angle upperArmElevation, Angle elbow, Angle forearmElevation)
        {
            // using my yaw(Z) / pitch(X)
            Quaternion toForearm = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (float)shoulderRot);
            Vector3 shoulderJoint = Vector3.Transform(-Vector3.UnitY, toForearm);
            toForearm = Quaternion.CreateFromAxisAngle(shoulderJoint, (float)forearmElevation) * toForearm;

            // get hand
            Vector3 pointing = Vector3.Transform(Vector3.UnitX, dir);
            Vector3 hand = Vector3.Transform(pointing, Quaternion.Inverse(toForearm));

            Vector2 handR = new Vector2(hand.Y, hand.Z);        // squash to X
            Angle wristRot = -Math.Atan(handR.X / handR.Y);     // abusing tan

            // subtract wrist rot
            hand = Vector3.Transform(hand,
                Quaternion.CreateFromAxisAngle(Vector3.UnitX, -(float)wristRot));

            Vector2 handF = new Vector2(hand.X, hand.Z);        // squash to Y
            Angle wristFlex = Math.Atan(handF.Y / handF.X);     // actual tan
            if (handF.X < 0) wristFlex += PI;

            // resultant hand
            Quaternion handTransform = Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float)wristRot);
            Vector3 wristFlexAxis = Vector3.Transform(-Vector3.UnitY, handTransform);
            handTransform = Quaternion.CreateFromAxisAngle(wristFlexAxis, (float)wristFlex) * handTransform;

            // differece resultant and actual
            Quaternion diff = Quaternion.Inverse(toForearm * handTransform) * dir;
            float w = diff.W;
            w = w > 1 ? 1 : w < -1 ? -1 : w;
            Angle handRot = 2f * Math.Acos(w);
            if (handRot != 0 && handRot != PI)
            {
                if (diff.X < 0) handRot = -handRot;
                //since rotAxis.X = diff.X / Sqrt(1 - (diff.W * diff.W));
            }

            //debug
            //Console.WriteLine("method printout");
            //Console.WriteLine("forearm: {0}", Vector3.Transform(Vector3.UnitX, toForearm));
            //Console.WriteLine("fElev " + forearmElevation);
            //Console.WriteLine("pt" + pointing);
            //Console.WriteLine(hand);
            //Console.WriteLine(wristRot.toDegrees());
            //Console.WriteLine(wristFlex.toDegrees());
            //Vector3 calcHand = Vector3.Transform(Vector3.UnitX, handTransform);
            //Console.WriteLine("calcH" + calcHand);
            //Console.WriteLine(Vector3.Transform(Vector3.UnitX, dir));
            //Console.WriteLine(Vector3.Transform(Vector3.UnitX, toForearm * handTransform));
            //Console.WriteLine(handRot.toDegrees());

            //Console.WriteLine("ga done");
            return new Angle[]
            {
                shoulderRot,
                upperArmElevation,
                elbow,
                wristRot,
                wristFlex,
                handRot
            };
        }

        public ArmCursor AnglesToPose(Angle[] angles)
        {
            Console.WriteLine("angles to ArmCursor");

            //upperarm + shoulder
            Quaternion shoulderRot = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (float)angles[0]);
            Vector3 shoulderJoint = Vector3.Transform(-Vector3.UnitY, shoulderRot);
            Vector3 shoulderOffset = Vector3.Transform(Vector3.UnitX * config.SHOULDER, shoulderRot);
            Quaternion shoulderTransform = Quaternion.CreateFromAxisAngle(shoulderJoint, (float)angles[1]) * shoulderRot;
            Vector3 upperArm = Vector3.Transform(Vector3.UnitX * config.UPPER_ARM, shoulderTransform);
            //Console.WriteLine("ua "+ upperArm);
            //Console.WriteLine("soff " + shoulderOffset);

            //forearm
            Quaternion forearmTransform = 
                Quaternion.CreateFromAxisAngle(shoulderJoint, -(float)(Math.PI - angles[2] - angles[1]))
                * shoulderRot;
            Vector3 forearm = Vector3.Transform(Vector3.UnitX * config.FOREARM, forearmTransform);
            //Console.WriteLine("fore " + forearm);

            //hand
            Quaternion handTransform = Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float)angles[3]);
            Vector3 wristFlexAxis = Vector3.Transform(-Vector3.UnitY, handTransform);
            //Console.WriteLine("hand fax" + wristFlexAxis);

            handTransform = Quaternion.CreateFromAxisAngle(wristFlexAxis, (float)angles[4]) * handTransform;
            Vector3 handRotAxis = -Vector3.Transform(Vector3.UnitX, handTransform);
            //Console.WriteLine("hand rax" + handRotAxis);

            handTransform = Quaternion.CreateFromAxisAngle(handRotAxis, (float)angles[5]) * handTransform;
            handTransform = forearmTransform * handTransform;
            Vector3 handOffset = Vector3.Transform(Vector3.UnitX * config.HAND, handTransform);
            //Console.WriteLine("hand " + handOffset);

            //final loc
            Vector3 target = upperArm + forearm;
            Console.WriteLine("target: " + target);
            target += shoulderOffset;
            target += handOffset;
            Console.WriteLine("wrist targ: " + target);

            return new ArmCursor(target, handTransform);
        }

        //// project raw onto target
        //private Vector3 project(Vector3 raw, Vector3 target)
        //{
        //    Vector3 dir = Vector3.Normalize(target);
        //    float l = Vector3.Dot(raw, dir);
        //    Vector3 result = Vector3.Multiply(dir, l);

        //    return result;
        //}

        //// squash raw onto plain
        //private Vector3 squash(Vector3 raw, Vector3 normal)
        //{
        //    Vector3 difference = project(raw, normal);
        //    Vector3 result = Vector3.Subtract(raw, difference);

        //    return result;
        //}

        //// get angle between a and b
        //private float angleBetween(Vector3 a, Vector3 b)
        //{
        //    float top = Vector3.Dot(a, b);
        //    float bot = a.Length() * b.Length();
        //    float raw = (float)Math.Acos(top / bot);

        //    return raw;
        //}
    }
}
