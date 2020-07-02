using Communications;
using Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Controllers
{
    public abstract class Controller
    {
        protected const bool VERBOSE = false;

        private bool hasControl = false;

        public delegate void UpdateLabels(VerboseInfo msg);
        private UpdateLabels updateLabelsDelegate;
        private VerboseInfo infoForLabels;

        protected delegate void sendDataDelegate(ComData d);
        protected sendDataDelegate sendData;

        public Controller(Com comObject)
        {
            sendData = comObject.Send;
        }

        public void GiveControl()
        {
            lock(this)
            {
                if (hasControl) return;
                begin();
                hasControl = true;
            }
        }

        protected abstract void begin();

        public void ReleaseControl()
        {
            lock (this)
            {
                if (!hasControl) return;
                end();
                hasControl = false;
            }
        }

        protected abstract void end();

        public bool inControl()
        {
            return hasControl;
        }

        //private long currentTimeMilis()
        //{
        //    return DateTime.Now.Ticks;
        //}

        //private void runControl(object sender, DoWorkEventArgs e)
        //{
        //    long last = currentTimeMilis();
        //    long lastAngleUpdate = currentTimeMilis();
        //    while (!worker.CancellationPending)
        //    {
        //        long now = currentTimeMilis();
        //        float dt = (now - last) / 10000000f;
        //        if (dt < 0.02) continue;

        //        ComData target = new ComData();
        //        if (step(ref target, dt))
        //        {
        //            comObject.sendData(target);
        //        }

        //        if ((now - lastAngleUpdate) / 10000000f >= 1f)
        //        {
        //            comObject.sendData(ComRequest.ANGLES);
        //            comObject.sendData(ComRequest.POSE);
        //            lastAngleUpdate = now;
        //        }

        //        updatePoseInfo();
        //        updateAngleInfo();
        //        infoForLabels.msg = "";

        //        lock (this)
        //        {
        //            foreach (Key k in keysDown)
        //            {
        //                infoForLabels.msg += k.ToString() + ".";
        //            }
        //        }
        //        updateLabelsDelegate(infoForLabels);

        //        last = now;
        //    }
        //}

        //protected void updatePoseInfo()
        //{
        //    ArmPose p = pose;
        //    Vector3 dir = p.direction;
        //    Vector3 pos = p.position;
        //    Vector3 hUp = p.handUp;

        //    infoForLabels.dir = "<" + dir.X.ToString("0.00") + ", " + dir.Y.ToString("0.00") + ", " + dir.Z.ToString("0.00") + ">";
        //    infoForLabels.pos = "<" + pos.X.ToString("0.00") + ", " + pos.Y.ToString("0.00") + ", " + pos.Z.ToString("0.00") + ">";
        //    infoForLabels.hUp = "<" + hUp.X.ToString("0.00") + ", " + hUp.Y.ToString("0.00") + ", " + hUp.Z.ToString("0.00") + ">";
        //}

        //protected void updateAngleInfo()
        //{
        //    if (servoAngles == null) return;
        //    Angle[] angles = servoAngles;
        //    infoForLabels.angles = "";
        //    for (int i = 0; i < angles.Length; i++)
        //    {
        //        servoAngles[i] = angles[i];
        //        infoForLabels.angles += servoAngles[i].get() + "\n";
        //    }
        //}

        //protected abstract bool step(ref ComData d, float dt);
    }
}
