using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace UI
{
    internal class GridHeader
    {
        private Label h;
        private RowDefinition header;

        public GridHeader(string content)
        {
            header = new RowDefinition();
            header.Height = new GridLength(30);
            h = new Label();
            h.Content = "Library";
            h.VerticalAlignment = VerticalAlignment.Center;
            h.Foreground = new SolidColorBrush(Colors.White);
            h.Background = new SolidColorBrush(Color.FromArgb(70, 69, 119, 207));
        }

        public void AddToGrid(ref Grid grid)
        {
            grid.RowDefinitions.Insert(0, header);
            grid.Children.Add(h);
            Grid.SetRow(h, 0);
        }
    }
}
