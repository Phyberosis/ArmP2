using Communications;
using Data;
using Data.Arm;
using Data.JSON;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Controllers
{
    internal class ControllerKeyboard : LoopController
    {
        private Collection<Key> keysDown;
        private Collection<Key> keysToggle;

        private KeyProcessor kpPosDir, kpRaw;
        public ControllerKeyboard(Com com) : base(com)
        {
            kpPosDir = new PosDirProcessor();
            kpRaw = new RawAnglesProcessor();

            keysDown = new Collection<Key>();
            keysToggle = new Collection<Key>();

            EventBulletin.Subscribe(EventBulletin.Event.KEY_DOWN, (o, e) =>
            {
                keyDown((KeyEventArgs)e);
            });
            EventBulletin.Subscribe(EventBulletin.Event.KEY_UP, (o, e) =>
            {
                keyUp((KeyEventArgs)e);
            });
        }

        private void keyDown(KeyEventArgs e)
        {
            lock (this)
            {
                if (!keysDown.Contains(e.Key)) keysDown.Add(e.Key);
                if (!keysToggle.Contains(e.Key))
                {
                    keysToggle.Add(e.Key);
                }
                else
                {
                    keysToggle.Remove(e.Key);
                }

                //Console.WriteLine(e.Key.ToString());
            }
        }

        private void keyUp(KeyEventArgs e)
        {
            lock (this)
            {
                if (keysDown.Contains(e.Key)) keysDown.Remove(e.Key);
            }
        }

        protected override bool update(float dt, out ComData msg)
        {
            KeyProcessor keyProcessor = keysToggle.Contains(Key.CapsLock) ? kpRaw : kpPosDir;

            float rate = keysToggle.Contains(Key.F) ? 10 : 2.5f;

            lock (this)
            {
                return keyProcessor.getNextTarget(ref keysDown, ref keysToggle, rate * dt, out msg);
            }
        }

        private abstract class KeyProcessor
        {
            protected const float CTrans = 1f;
            protected const float CRot = 0.5f;

            public abstract bool getNextTarget(ref Collection<Key> keysDown, ref Collection<Key> keysToggle, float stepAmount, out ComData msg);
        }

        private class PosDirProcessor : KeyProcessor
        {
            private ArmCursor offset = new ArmCursor(Vector3.Zero, Quaternion.Identity);

            public override bool getNextTarget(ref Collection<Key> keysDown, ref Collection<Key> keysToggle, float stepAmount, out ComData msg)
            {
                Vector3 dPos = Vector3.Zero;
                Vector3 dDir = Vector3.Zero;   // just for storing dRotation angles
                if (keysDown.Contains(Key.A))
                    dPos.X += -1;
                if (keysDown.Contains(Key.D))
                    dPos.X += 1;
                if (keysDown.Contains(Key.S))
                    dPos.Y += -1;
                if (keysDown.Contains(Key.W))
                    dPos.Y += 1;
                if (keysDown.Contains(Key.C))
                    dPos.Z += -1;
                if (keysDown.Contains(Key.Z))
                    dPos.Z += 1;

                // acting as temp rotation vector
                if (keysDown.Contains(Key.J))
                    dDir.Y += -1;
                if (keysDown.Contains(Key.L))
                    dDir.Y += 1;
                if (keysDown.Contains(Key.I))
                    dDir.X += -1;
                if (keysDown.Contains(Key.K))
                    dDir.X += 1;
                if (keysDown.Contains(Key.M))
                    dDir.Z += -1;
                if (keysDown.Contains(Key.OemPeriod))
                    dDir.Z += 1;

                if (dPos == Vector3.Zero && dDir == Vector3.Zero)
                {
                    msg = null;
                    return false;
                }

                dPos = dPos.LengthSquared() != 0 ? Vector3.Normalize(dPos) * stepAmount * CTrans : dPos;
                dDir = dDir.LengthSquared() != 0 ? Vector3.Normalize(dDir) * stepAmount * CRot : dDir;

                Quaternion q = Quaternion.CreateFromYawPitchRoll(dDir.Y, dDir.X, dDir.Z);
                q *= offset.Dir;

                offset.Set(offset.Pos + dPos, q);

                msg = new ComData(new KeyFrame(offset, Time.Now()));
                if (VERBOSE) Console.WriteLine("Cursor: \n{0}\n", offset.ToString());
                return true;
            }
        }

        private class RawAnglesProcessor : KeyProcessor
        {
            Key[] toCheck = {
                Key.D0,
                Key.D1, Key.D2,
                Key.D3, Key.D4,
                Key.D5, Key.D6 };
            float[] offsets;

            public RawAnglesProcessor()
            {
                offsets = new float[toCheck.Length + 1];
            }

            public override bool getNextTarget(ref Collection<Key> keysDown, ref Collection<Key> keysToggle, float stepAmount, out ComData msg)
            {
                stepAmount *= CRot;
                if (keysDown.Contains(Key.LeftShift) || keysDown.Contains(Key.RightShift))
                {
                    stepAmount *= -1;
                }
                if (keysDown.Contains(Key.LeftCtrl) || keysDown.Contains(Key.RightCtrl))
                {
                    stepAmount *= 2;
                }

                //Angle[] dAngles = new Angle[toCheck.Length];
                bool changed = false;
                for (int i = 0; i < toCheck.Length; i++)
                {
                    Key k = toCheck[i];
                    if (keysDown.Contains(k))
                    {
                        changed = true;
                        offsets[i+1] = (float)new Angle(offsets[i+1] + stepAmount);
                    }
                }
                
                offsets[0] = Time.Now();
                msg = null;
                if (changed)
                {
                    msg = new ComData(new JSONableArray<float>(offsets, float.Parse));
                    if(VERBOSE) Console.WriteLine("raw: {1}[\n{0}\n]", string.Join("\n", offsets), changed);
                }
                return changed;
            }
        }
    }
}
