using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DataGridExample
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<ContentClass> listOne = new ObservableCollection<ContentClass>();
        private List<DataGridRow> loadedRows = new List<DataGridRow>();
        private object locker = new object();
        public MainWindow()
        {
            InitializeComponent();

            var r = new Random();

            for (var i = 0; i < 200; i++)
            {
                listOne.Add(new ContentClass(RandomString(r.Next(10, 50 + i / 2)), RandomString(r.Next(15, 50 + i / 4))));
            }

            dataGrid.ItemsSource = listOne;
        }

        public static string RandomString(int size)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 0; i < size; i++)
            {
                //Генерируем число являющееся латинским символом в юникоде
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                //Конструируем строку со случайно сгенерированными символами
                builder.Append(ch);
            }
            return builder.ToString();
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        public static childItem FindVisualChild<childItem>(DependencyObject obj) where childItem : DependencyObject
        {
            foreach (childItem child in FindVisualChildren<childItem>(obj))
            {
                return child;
            }

            return null;
        }

        public static DataGridCell GetCell(DataGrid grid, DataGridRow row, int columnIndex)
        {
            if (row == null) return null;

            var presenter = FindVisualChild<DataGridCellsPresenter>(row);
            if (presenter == null) return null;

            var cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(columnIndex);
            if (cell != null) return cell;

            grid.ScrollIntoView(row, grid.Columns[columnIndex]);
            cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(columnIndex);

            return cell;
        }

        private void dataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            if (e.Row != null && !loadedRows.Contains(e.Row))
            {
                lock (locker)
                {
                    loadedRows.Add(e.Row);

                    this.ResizeCols();
                }
            }
        }

        private void dataGrid_UnloadingRow(object sender, DataGridRowEventArgs e)
        {
            if (e.Row != null && loadedRows.Contains(e.Row))
            {
                lock (locker)
                {
                    loadedRows.Remove(e.Row);
                }
            }
        }

        private void ResizeCols()
        {
            if (this.loadedRows.Count == 0)
            {
                return;
            }

            double maxWidthFirst = 0;
            double maxWidthSecond = 0;

            foreach (var row in this.loadedRows)
            {
                var cellOne = GetCell(dataGrid, row, 0);
                var cellTwo = GetCell(dataGrid, row, 1);

                var grid = FindVisualChild<Grid>(cellOne);
                var textBlock = FindVisualChild<TextBlock>(cellTwo);

                if (grid != null)
                {
                    var button = (Button)grid.Children[0];

                    if (maxWidthFirst < button.ActualWidth)
                    {
                        maxWidthFirst = button.ActualWidth;
                    }
                }

                if (textBlock != null)
                {
                    if (maxWidthSecond < textBlock.ActualWidth)
                    {
                        maxWidthSecond = textBlock.ActualWidth;
                    }
                }
            }

            this.lenghtLabel.Content = maxWidthFirst;
            this.dataGrid.Columns[0].Width = new DataGridLength(maxWidthFirst + 5);
            this.dataGrid.Columns[1].Width = new DataGridLength(maxWidthSecond + 5);
        }
    }

    public class ContentClass
    {
        public string FirstString { get; set; }
        public string SecondString { get; set; }

        public ContentClass(string s1, string s2)
        {
            this.FirstString = s1;
            this.SecondString = s2;
        }
    }
}
