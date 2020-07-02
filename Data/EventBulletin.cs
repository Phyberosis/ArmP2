using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Data
{
    public class EventBulletin
    {
        private static EventBulletin instance;

        public static EventBulletin GetInstance()
        {
            if (instance == null)
                instance = new EventBulletin();

            return instance;
        }

        public enum Event
        {
            CLOSE, KEY_DOWN, KEY_UP
        }

        private List<onEventDelegate>[] phoneBook;

        private EventBulletin()
        {
            phoneBook = new List<onEventDelegate>[Enum.GetNames(typeof(Event)).Length];
            for(int i = 0; i < phoneBook.Length; i++)
            {
                phoneBook[i] = new List<onEventDelegate>(1);
            }
        }

        public static void Subscribe(Event e, onEventDelegate d)
        {
            EventBulletin eb = GetInstance();
            lock(eb)
            {
                eb.phoneBook[(int)e].Add(d);
            }
        }

        public void Notify(Event e, object o, EventArgs args)
        {
            lock (this)
            {
                foreach (onEventDelegate d in phoneBook[(int)e])
                {
                    Task.Delay(0).ContinueWith((t) => { d(o, args); });
                    //Console.WriteLine("ev");
                }
            }
        }
    }
}
