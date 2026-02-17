using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI;
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
using Windows.Security.Cryptography.Core;
using Windows.Storage.Provider;
using Windows.UI;

namespace Simple_Sheet_App.Views
{
    public sealed partial class MainWindow : Window
    {
        private readonly MainViewModel viewModel = new MainViewModel();

        private int totalRows = 100;
        private int totalCols = 100;
        private double cellW = 128;
        private double cellH = 40;

        private int selRow = -1;
        private int selCol = -1;
        public  MainWindow()
        {
            this.InitializeComponent();
            _ = InitializeAsync();
        }
        private async Task InitializeAsync()
        {
            await viewModel.LoadAsync();
            SheetCanvas.Width = totalCols * cellW;
            SheetCanvas.Height = totalRows * cellH;
            SheetCanvas.Invalidate();
        }

        private void SheetCanvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            var ds = args.DrawingSession;

            for (int r = 0; r <= totalRows; r++)
            {
                float y = (float)(r * cellH);
                ds.DrawLine(0, y, (float)(totalCols * cellW), y, Colors.LightGray, 0.5f);
            }

            for (int c = 0; c <= totalCols; c++)
            {
                float x = (float)(c * cellW);
                ds.DrawLine(x, 0, x, (float)(totalRows * cellH), Colors.LightGray, 0.5f);
            }

            var textFormat = new CanvasTextFormat
            {
                FontSize = 13,
                VerticalAlignment = CanvasVerticalAlignment.Center
            };

            for (int r = 0; r < totalRows; r++)
            {
                for (int c = 0; c < totalCols; c++)
                {
                    string val = viewModel.GetValue(r, c);
                    if (!string.IsNullOrEmpty(val))
                    {
                        ds.DrawText(val,
                            new Rect(c * cellW + 8, r * cellH, cellW - 16, cellH),
                            Colors.White,
                            textFormat);
                    }
                }
            }

            if (selRow >= 0 && selCol >= 0)
            {
                ds.DrawRectangle(
                    (float)(selCol * cellW),
                    (float)(selRow * cellH),
                    (float)cellW,
                    (float)cellH,
                    Colors.RosyBrown, 2f);
            }
        }

        private void SheetCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var point = e.GetCurrentPoint(SheetCanvas).Position;

            selRow = (int)(point.Y / cellH);
            selCol = (int)(point.X / cellW);

            SheetCanvas.Invalidate();
        }

        private void SheetCanvas_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (selRow < 0 || selCol < 0) return;
            double X = selCol * cellW;
            double Y = selRow * cellH;

            EditBox.Margin = new Thickness(X, Y, 0, 0);
            EditBox.Width = cellW;
            EditBox.Height = cellH;

            EditBox.Text = viewModel.GetValue(selRow, selCol);

            EditBox.Visibility = Visibility.Visible;
            EditBox.Focus(FocusState.Programmatic);
            EditBox.SelectAll();
        }

        private async void EditBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if(e.Key == Windows.System.VirtualKey.Enter)
            {
                await viewModel.InsertValue(selRow, selCol, EditBox.Text);
                EditBox.Visibility = Visibility.Collapsed;
                SheetCanvas.Invalidate();
                e.Handled = true;
            }
        }

        private async void EditBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (EditBox.Visibility == Visibility.Visible)
            {
                await viewModel.InsertValue(selRow, selCol, EditBox.Text);
                EditBox.Visibility = Visibility.Collapsed;
                SheetCanvas.Invalidate();
            }
        }

        private void InsertVal_Button(Object Sender, RoutedEventArgs e)
        {
            if (int.TryParse(RowBox.Text, out int row) && int.TryParse(ColBox.Text, out int col))
            {
                String value = ValBox.Text;
                var cell = viewModel.InsertValue(row-1, col-1, value);
            }
            else
            {
                StatBox.Text = "Row and Column values must be Integers";
            }
            SheetCanvas.Invalidate();
        }

        public void Get_value(Object sender, RoutedEventArgs e)
        {
            if (int.TryParse(RowBox.Text, out int row) && int.TryParse(ColBox.Text, out int col))
            {
                String value = viewModel.GetValue(row - 1, col - 1);
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
            var result = await viewModel.HandleUndo();
            if (result.Value.cell != null)
            {
                selRow = result.Value.row;
                selCol = result.Value.col;
                SheetCanvas.Invalidate();  
            }
        }
        public async void Redo_Click(Object sender, RoutedEventArgs e)
        {
            var result = await viewModel.HandleRedo();
            if (result.Value.cell != null)
            {
                selRow = result.Value.row;
                selCol = result.Value.col;
                SheetCanvas.Invalidate();
            }
        }
    }
}