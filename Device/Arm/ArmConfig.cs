using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unosquare.RaspberryIO;
using Unosquare.WiringPi;

namespace Devices.Arm
{
    public class ArmConfig
    {
        public ArmConfig()
        {
            SHOULDER_SQ = (float)Math.Pow(SHOULDER, 2);
            UPPER_ARM_SQ = (float)Math.Pow(UPPER_ARM, 2);
            FOREARM_SQ = (float)Math.Pow(FOREARM, 2);
            HAND_SQ = (float)Math.Pow(HAND, 2);

            const int axis = 6;
            const float PI = (float)Math.PI;
            BOUNDS_RAD = new float[axis, 2];
            for(int i = 0; i < axis; i++)
            {
                BOUNDS_RAD[i, 0] = BOUNDS_DEG[i, 0] * PI / 180f;
                BOUNDS_RAD[i, 1] = BOUNDS_DEG[i, 1] * PI / 180f;
            }
        }

        private delegate Servo ServoConstructor(ArmConfig config, int i);

        private Servo[] createServos(ServoConstructor constructor)
        {
            Servo[] s = new Servo[6];
            for(int i = 0; i < 6; i++)
            {
                s[i] = constructor(this, i);
            }

            return s;
        }

        public Servo[] CreateSteppers()
        {
            return createServos(Servo.MakeStepper);
        }

        public Servo[] CreateTestServos()
        {
            return createServos(Servo.MakeTestServo);
        }

        public readonly float
            SHOULDER =      6f,
            UPPER_ARM =     30.5f,
            FOREARM =       23f,
            HAND =          5.5f;

        public readonly float
            SHOULDER_SQ,
            UPPER_ARM_SQ,
            FOREARM_SQ,
            HAND_SQ;

        public readonly int[,] SERVO_SPECS =
        {
            { 16000,  2, 10, 0},
            { 14667,  3,  9, 0},
            { 24000,  4, 11, 0},
            { 16669, 22, 13, 0},
            { 8000 , 17,  5, 0},
            { 7680 , 27,  6, 1}
        };

        public readonly float[,] BOUNDS_DEG =
        {
            {-105f, 105f},  //1
            {15f, 135f},    //2
            {15f, 180f},    //3
            {-170f, 170f},  //4
            {-120f, 120f},  //5
            {0f, 0f}        //6 - no bounds
        };

        public readonly float[,] BOUNDS_RAD;
    }
}
