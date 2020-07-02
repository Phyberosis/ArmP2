using Communications;
using Controllers;
using Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Windows;
using ui;
using System.Windows.Input;

namespace Hub
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {            
            new Program();
        }

        private Com com;
        private Controller controllerDefault;
        private Window mainWindow;
        private Application app;

        private bool closing = false;

        private Program()
        {
            com = ComFactory.MakeDefault();
            //com = ComFactory.MakeDummy();
            com.setOnRead(tempOnRead);

            Task.Delay(0).ContinueWith((t) =>
            {
                string cmd;
                while (true)
                {
                    cmd = Console.ReadLine();
                    Console.WriteLine("echo: " + cmd);
                    if (cmd.Equals("x")) break;

                    if(cmd.Equals("r"))
                    {
                        com.Send(ComData.RequestClose());
                        continue;
                    }

                    com.Send(new ComData(cmd));
                }

                Console.WriteLine("close");
                EventBulletin.GetInstance().Notify(EventBulletin.Event.CLOSE, null, null);
            });


            controllerDefault = ControllerFactory.MakeDefault(com);
            mainWindow = new MainWindow(controllerDefault);
            mainWindow.Show();

            EventBulletin.Subscribe(EventBulletin.Event.CLOSE, (o, e) => { tempOnClose(); });

            app = new Application();
            app.Run(mainWindow);
        }

        private void tempOnRead(ComData raw)
        {
            switch (raw.getDataType())
            {
                case ComData.REQUEST:
                    Console.WriteLine("Requst: " + raw.GetRequest().ToString());
                    break;
                case ComData.STRING:
                    Console.WriteLine("String : "+raw.getMessage());
                    break;
                default:
                    Console.WriteLine(raw.getDataType());
                    break;
            }
        }

        private void tempOnClose()
        {
            lock (this)
            {
                if (closing) return;
                closing = true;
            }

            Console.ReadLine();
            Environment.Exit(0);
        }
    }
}
