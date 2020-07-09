using Data.Arm;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using UI.Properties;

namespace UI
{
    internal class Library
    {
        private LinkedList<KeyFrameList> animations;
        private Grid uiMe, uiDetails;
        Style s;

        int sel = -1;

        public Library(Grid me, Grid details, Style style)
        {
            animations = new LinkedList<KeyFrameList>();
            s = style;
            uiMe = me;
            uiDetails = details;

            //new GridHeader("Library").AddToGrid(ref uiMe);

            Button first = AddNew();
            displayDetails(first, null);
        }
        private void displayDetails(object sender, RoutedEventArgs args)
        {
            // todo - dont set with name!
            sel = (int)((Button)sender).Content - 1;
            animations.ElementAt(sel).Connect(uiDetails);
        }

        public Button AddNew()
        {
            Button b = new Button();
            b.Style = s;
            b.Click += displayDetails;
            b.Content = animations.Count + 1;
            RowDefinition def = new RowDefinition();
            def.Height = new GridLength(b.Height);
            uiMe.RowDefinitions.Add(def);
            uiMe.Children.Add(b);
            Grid.SetRow(b, animations.Count);

            animations.AddLast(new KeyFrameList(s));

            return b;
        }

        public void AddKeyFrame(ArmCursor cursor)
        {
            if (sel < 0) return;
            animations.ElementAt(sel).Add(new KeyFrame(cursor, 0));
            Console.WriteLine(sel);
        }
    }
}