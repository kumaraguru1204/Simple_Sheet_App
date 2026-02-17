using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Simple_Sheet_App.Models;
using Simple_Sheet_App.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.PointOfService;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Provider;

namespace Simple_Sheet_App.Views
{
    public sealed partial class MainWindow : Window
    {
        private readonly MainViewModel vm = new MainViewModel();
        private Dictionary<(int row, int col), TextBlock> cellMap = new Dictionary<(int row, int col), TextBlock>();

        public  MainWindow()
        {
            this.InitializeComponent();
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            await vm.LoadAsync();
            CreateGrid(100, 100);

            for (int r = 0; r < 100; r++)
            {
                for (int c = 0; c < 100; c++)
                {
                    var val = vm.GetValue(r, c);
                    if (!string.IsNullOrEmpty(val))
                    {
                        UpdateUI(r+1, c+1, val);
                    }
                }
            }
        }

        public void CreateGrid(int Rows, int Cols)
        {
            cellMap.Clear();

            for (int i = 0; i < Rows; i++)
                MyGrid.RowDefinitions.Add(new RowDefinition
                {
                    Height = new GridLength(40)
                });

            for (int j = 0; j < Cols; j++)
                MyGrid.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = new GridLength(128)
                });

            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Cols; j++)
                {
                    var border = new Border
                    {
                        BorderThickness = new Thickness(0.5),
                        BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.LavenderBlush),
                        Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent)
                    };

                    var text = new TextBlock
                    {
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };

                    border.Child = text;
                    Grid.SetRow(border, i);
                    Grid.SetColumn(border, j);
                    border.Tapped += Cell_Highlighter;

                    MyGrid.Children.Add(border);

                    cellMap[(i, j)] = text;
                }
            }
        }

        private void InsertVal_Button(Object Sender, RoutedEventArgs e)
        {
            if (int.TryParse(RowBox.Text, out int row) && int.TryParse(ColBox.Text, out int col))
            {
                String value = ValBox.Text;
                var cell = vm.InsertValue(row-1, col-1, value);
                if (cell != null)
                {
                    UpdateUI(row, col, value);
                }
            }
            else
            {
                StatBox.Text = "Row and Column values must be Integers";
            }
        }

        private Border selectedCell;
        private void Cell_Highlighter(Object Sender, RoutedEventArgs e)
        {
            Border clicked = (Border)Sender;

            if (selectedCell != null)
            {
                selectedCell.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.LavenderBlush);
            }

            selectedCell = clicked;
            selectedCell.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Yellow);

            CellEditor(selectedCell);
            StatBox.Text = "Tapped";
        }
        private string cellOriginalValue;
        private void CellEditor(Border border)
        {
            var text = border.Child as TextBlock;
            String newText;
            if (text is null)
            {
                newText = "";
            }
            else
            {
                newText = text.Text;
            }

            cellOriginalValue = newText;

            var box = new TextBox()
            {
                Text = newText
            };

            box.KeyDown += KeyEditor;

            border.Child = box;
            box.Focus(FocusState.Programmatic);
        }

        private void KeyEditor(Object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                TextBox box = (TextBox)sender;
                Border border = (Border)box.Parent;

                int row = Grid.GetRow(border);
                int col = Grid.GetColumn(border);

                var cell = vm.InsertValue(row, col, box.Text);

                var newTextBlock = new TextBlock
                {
                    Text = box.Text,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                border.Child = newTextBlock;

                cellMap[(row, col)] = newTextBlock;

                if (selectedCell is not null)
                {
                    selectedCell.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.LavenderBlush);
                }
                selectedCell = null;
            }
        }

        public void UpdateUI(int row, int col, String val)
        {
            if (row < 0 || col < 0 || row >= 100 || col >= 100)
            {
                StatBox.Text = "Enter rows and cols between 1 to 100";
                return;
            }

            if (cellMap.TryGetValue((row-1, col-1), out var textBlock))
            {
                textBlock.Text = val;
                if (RowBox.Text != "" && ColBox.Text == "")
                {
                    StatBox.Text = "Inserted";
                }
                RowBox.Text = "";
                ColBox.Text = "";
                ValBox.Text = "";
            }
        }

        public void Get_value(Object sender, RoutedEventArgs e)
        {
            if (int.TryParse(RowBox.Text, out int row) && int.TryParse(ColBox.Text, out int col))
            {
                String value = vm.GetValue(row - 1, col - 1);
                if (value == "")
                {
                    StatBox.Text = "Nothing There";
                }
                else
                {
                    StatBox.Text = $"The Value in ({row},{col}) is ' {value} '";
                }
            }
            else
            {
                StatBox.Text = "Row and Column values must be Integers";
            }
        }

        public async void Undo_Click(Object sender, RoutedEventArgs e)
        {
            var cell = await vm.HandleUndo();
            if (cell != null)
            {
                UpdateUI(cell.RowNum+1, cell.ColNum+1, cell.Value);
            }
        }

        public async void Redo_Click(Object sender, RoutedEventArgs e)
        {
            var cell = await vm.HandleRedo();
            if (cell != null)
            {
                UpdateUI(cell.RowNum+1, cell.ColNum+1, cell.Value);
            }
        }
    }
}