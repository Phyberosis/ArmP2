using Communications;
using Data;
using Data.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devices
{
    public abstract class Device
    {
        protected Com com;

        public Device()
        {
            com = ComFactory.MakeDefault();
            com.setOnRead(onComRead);

            EventBulletin.Subscribe(EventBulletin.Event.CLOSE, (o, e) => { onClose(); });
        }

        protected abstract void onComRead(ComData data);

        protected abstract void onClose();
    }
}
