using Input;
using Data;
using System;
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Communications;

namespace UI
{
    public struct VerboseInfo
    {
        public string pos;
        public string dir;
        public string hUp;
        public string angles;
        public string msg;
    }

    internal static class Constants
    {
        public class Colors
        {
            public static Color Orange = Color.FromRgb(220, 136, 51);
            public static Color Grey0 = Color.FromRgb(247, 247, 247);
            public static Color Grey1 = Color.FromRgb(41, 41, 41);
            public static Color Blue0 = Color.FromRgb(217, 235, 255);
            public static Color Green = Color.FromRgb(79, 168, 87);
        }
    }

    public partial class MainWindow : Window
    {
        private Controller controller;
        private Com com;
        private EventBulletin eventBulletin;

        private Library lib;
        //private Com com;

        public MainWindow(Com com)
        {
            this.com = com;
            controller = ControllerFactory.MakeDefault(com);
            eventBulletin = EventBulletin.GetInstance();

            InitializeComponent();

            lib = new Library(grdLeft, grdMid, this.Resources["MyContentTray"] as Style);
        }

        private void Click_Control(object sender, RoutedEventArgs e)
        {
            Button b = (Button)sender;
            if (controller.inControl())
            {
                controller.ReleaseControl();
                b.Content = "Take Control";
            }
            else
            {
                controller.GiveControl();
                b.Content = "Release Control";
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

        private void FrmMain_Closing(object sender, CancelEventArgs e)
        {
            controller.ReleaseControl();
            eventBulletin.Notify(EventBulletin.Event.CLOSE, sender, e);
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            lib.AddNew();
        }

        private void btnAddKeyframe_Click(object sender, RoutedEventArgs e)
        {
            lib.AddKeyFrame(controller.GetCursor());
        }

        private void btnToggleRun_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnKeyboardControl_Click(object sender, RoutedEventArgs e)
        {
            if (controller.inControl()) return;

            controller = ControllerFactory.MakeKeyboardController(com);
        }

        private void btnHandControl_Click(object sender, RoutedEventArgs e)
        {
            if (controller.inControl()) return;

            controller = ControllerFactory.MakeHandController(com);
        }
    }
}
