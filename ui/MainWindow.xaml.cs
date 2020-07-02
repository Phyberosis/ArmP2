using Controllers;
using Data;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace ui
{
    public struct VerboseInfo
    {
        public string pos;
        public string dir;
        public string hUp;
        public string angles;
        public string msg;
    }

    public partial class MainWindow : Window
    {
        private Controller controller;
        private EventBulletin eventBulletin;
        //private Com com;

        public MainWindow(Controller mainController)
        {
            controller = mainController;
            eventBulletin = EventBulletin.GetInstance();
            InitializeComponent();
            //log = Log.getInstance();
            //log.Show();
        }

        private void Click_Control(object sender, RoutedEventArgs e)
        {
            if (controller.inControl())
            {
                controller.ReleaseControl();
            }
            else
            {
                controller.GiveControl();
            }
        }

        private void FrmMain_KeyDown(object sender, KeyEventArgs e)
        {
            eventBulletin.Notify(EventBulletin.Event.KEY_DOWN, sender, e);
        }

        private void FrmMain_KeyUp(object sender, KeyEventArgs e)
        {
            eventBulletin.Notify(EventBulletin.Event.KEY_UP, sender, e);
        }

        public void setArmDataLabels(VerboseInfo info)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                lblArmPos.Content = "Position: " + info.pos;
                lblArmDir.Content = "Gimbal: " + info.dir;
                lblMsg.Content = info.angles + "\n" + info.msg;
            }));
        }

        private void FrmMain_Closing(object sender, CancelEventArgs e)
        {
            controller.ReleaseControl();
            eventBulletin.Notify(EventBulletin.Event.CLOSE, sender, e);
        }
    }
}
