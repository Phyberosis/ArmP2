using Data.Arm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace UI
{
    internal class KeyFrameTray : Button
    {
        public readonly KeyFrame Frame;

        public KeyFrameTray(KeyFrame kf, Style style) : base()
        {
            Content = kf.ToString();
            HorizontalContentAlignment = HorizontalAlignment.Left;
            Frame = kf;
            this.Style = style;
            Height *= 2.5;
        }
    }

    internal class KeyFrameList
    {
        //private ObservableCollection<KeyFrameTray> col;
        private LinkedList<KeyFrameTray> frames;
        private Grid grid;
        private Style s;

        public KeyFrameList(Style style)
        {
            frames = new LinkedList<KeyFrameTray>();
            s = style;
            //listbox1.DisplayMemberPath = "Name";
            //listbox1.ItemsSource = _empList;

            //Style itemContainerStyle = new Style(typeof(ListBoxItem));
            //itemContainerStyle.Setters.Add(new Setter(ListBoxItem.AllowDropProperty, true));
            //itemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(s_PreviewMouseLeftButtonDown)));
            //itemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.DropEvent, new DragEventHandler(listbox1_Drop)));
            //listbox1.ItemContainerStyle = itemContainerStyle;
        }

        public void Connect(Grid gd)
        {
            grid = gd;

            grid.Children.Clear();
            grid.RowDefinitions.Clear();

            //new GridHeader("Keyframes").AddToGrid(ref grid);

            int i = 0;
            foreach(var f in frames)
            {
                addToGrid(i, f);
                i++;
            }
            //ListBoxItem.mouse

            Console.WriteLine("here " + grid);
        }

        public void Add(KeyFrame kf)
        {
            KeyFrameTray f = new KeyFrameTray(kf, s);

            frames.AddLast(f);
            addToGrid(frames.Count - 1, f);
        }

        private void addToGrid(int i, KeyFrameTray f)
        {
            var d = new RowDefinition();
            d.Height = new GridLength(f.Height);
            grid.RowDefinitions.Add(d);
            grid.Children.Add(f);
            Grid.SetRow(f, i);
        }

        //void s_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{

        //    if (sender is ListBoxItem)
        //    {
        //        ListBoxItem draggedItem = sender as ListBoxItem;
        //        DragDrop.DoDragDrop(draggedItem, draggedItem.DataContext, DragDropEffects.Move);
        //        draggedItem.IsSelected = true;
        //    }
        //}

        //void listbox1_Drop(object sender, DragEventArgs e)
        //{
        //    Emp droppedData = e.Data.GetData(typeof(Emp)) as Emp;
        //    Emp target = ((ListBoxItem)(sender)).DataContext as Emp;

        //    int removedIdx = listbox1.Items.IndexOf(droppedData);
        //    int targetIdx = listbox1.Items.IndexOf(target);

        //    if (removedIdx < targetIdx)
        //    {
        //        _empList.Insert(targetIdx + 1, droppedData);
        //        _empList.RemoveAt(removedIdx);
        //    }
        //    else
        //    {
        //        int remIdx = removedIdx + 1;
        //        if (_empList.Count + 1 > remIdx)
        //        {
        //            _empList.Insert(targetIdx, droppedData);
        //            _empList.RemoveAt(remIdx);
        //        }
        //    }
        //}
    }
}