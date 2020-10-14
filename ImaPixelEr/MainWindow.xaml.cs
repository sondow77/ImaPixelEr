using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ImaPixelEr
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public static class WriteableBitmapExtentions
    {
        // Save the WriteableBitmap into a PNG file.
        public static void Save(this WriteableBitmap wbitmap,
            string filename)
        {
            // Save the bitmap into a file.
            using (FileStream stream =
                new FileStream(filename, FileMode.Create))
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(wbitmap));
                encoder.Save(stream);
            }
        }
    }
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void UpdateGrid()
		{
            canvas_ug.Children.Clear();
            canvas_ug.Rows = rows;
            canvas_ug.Columns = cols;
            canvas_ug.Width = image_img.Width;
            canvas_ug.Height = image_img.Height;
            for (int i = 0; i < (rows * cols); i++)
            {
                Border pixel = new Border
                {
                    BorderThickness = new Thickness(1),
                    BorderBrush = Brushes.Black,
                    Background = new SolidColorBrush(Colors.Transparent),
                    Width = image_img.Width / cols,
                    Height = image_img.Height / rows
                };
                pixel.MouseLeftButtonDown += Pixel_Click;
                pixel.MouseEnter += Pixel_Enter;
                pixel.MouseWheel += Pixel_MouseWheel;
                pixel.MouseMove += Pixel_Move;
                canvas_ug.Children.Add(pixel);
            }
        }
        private void Load_img_btn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = "c:\\";
            dlg.Filter = "Image Files|*.jpeg;*.png;*.jpg;*.gif;*.bmp|All Files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() == true)
            {
                string selectedFileName = dlg.FileName;
                file_txt.Text = selectedFileName;
                bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(selectedFileName);
                bitmap.EndInit();
                image_img.Source = bitmap;
                image_img.Width = bitmap.Width;
                image_img.Height = bitmap.Height;
                image_loaded = true;

                UpdateGrid();
            }
        }
        private BitmapImage bitmap;
        private bool image_loaded = false;
        private SolidColorBrush selectedColor;
        private SolidColorBrush clickedColor;
        private readonly SolidColorBrush emptyColor = new SolidColorBrush(Colors.Transparent);
        private int rows = 16;
        private int cols = 16;
        private int tool = 0;
        private void Update_btn_Click(object sender, RoutedEventArgs e)
        {
            if (image_loaded)
            {
                rows = Convert.ToInt32(height_dud.Value);
                cols = Convert.ToInt32(width_dud.Value);
                if (canvas_ug.Rows != rows || canvas_ug.Columns != cols)
                {
                    UpdateGrid();
                }
            }
        }

        private void Pixel_Move(object sender, MouseEventArgs e)
        {
            switch (tool)
            {

                case 3:
                    if(e.LeftButton == MouseButtonState.Pressed)
                    {
                        clickedColor = (SolidColorBrush)(((Border)e.Source).Background);
                        if (clickedColor != null && clickedColor.ToString() != emptyColor.ToString())
                        {
                            selectedColor = clickedColor;
                            color_cpk.SelectedColor = selectedColor.Color;
                        }
                        else
                        {
                            Point mpos = e.GetPosition(relativeTo: image_img);
                            int mx = (int)(bitmap.PixelWidth * mpos.X / image_img.Width);
                            int my = (int)(bitmap.PixelHeight * mpos.Y / image_img.Height);
                            selectedColor = new SolidColorBrush(GetPixelColor(bitmap, mx, my));
                            color_cpk.SelectedColor = selectedColor.Color;
                        }
                    }
                    break;
            }
        }

		private void Pixel_Click(object sender, MouseEventArgs e)
        {
            switch (tool)
            {
                case 0:
                    ((Border)e.Source).Background = selectedColor;
                    break;
                case 1:
                    ((Border)e.Source).Background = emptyColor;
                    break;
                case 2:
                    clickedColor = (SolidColorBrush)(((Border)e.Source).Background);
                    int cid = canvas_ug.Children.IndexOf((Border)e.Source);
                    if (cid > 0)
                    {
                        Fill(idToPos(cid, cols, rows));
                    }
                    break;
                case 3:
                    clickedColor = (SolidColorBrush)(((Border)e.Source).Background);
                    if (clickedColor != null && clickedColor.ToString() != emptyColor.ToString())
                    {
                        selectedColor = clickedColor;
                        color_cpk.SelectedColor = selectedColor.Color;
                    }
                    else
                    {
                        Point mpos = e.GetPosition(relativeTo: image_img);
                        int mx = (int)(bitmap.PixelWidth * mpos.X / image_img.Width);
                        int my = (int)(bitmap.PixelHeight * mpos.Y / image_img.Height);
                        selectedColor = new SolidColorBrush(GetPixelColor(bitmap,mx, my));
                        color_cpk.SelectedColor = selectedColor.Color;
                    }
                    break;
            }
        }
        public void Fill(int[] pos)
		{
            SolidColorBrush clPos = ((SolidColorBrush)(((Border)canvas_ug.Children[posToId(pos[0], pos[1], cols)]).Background));
            if (!(clickedColor.ToString().Equals(selectedColor.ToString())) && (clPos.ToString().Equals(clickedColor.ToString())))
            {
                ((Border)canvas_ug.Children[posToId(pos[0], pos[1], cols)]).Background = selectedColor;
                if(pos[0] > 0)
				{
                    Fill(new int[] { pos[0] - 1, pos[1] });
                }
                if(pos[0] < (cols-1))
				{
                    Fill(new int[] { pos[0] + 1, pos[1] });
                }
                if(pos[1] > 0)
                {
                    Fill(new int[] { pos[0], pos[1] - 1 });
                }
                if (pos[1] < (rows - 1))
                {
                    Fill(new int[] { pos[0], pos[1] + 1 });
                }
            }
		}
        public int[] idToPos(int id, int w, int h)
		{
			int[] res = new int[2];
            res[1] = id / w;
            res[0] = id - (res[1] * w);
            return res;
		}
        public int posToId(int x, int y, int w)
		{
            return y * w + x;
		}
        public static Color GetPixelColor(BitmapSource source, int x, int y)
        {
            Color c = Colors.White;
            if (source != null)
            {
                try
                {
                    CroppedBitmap cb = new CroppedBitmap(source, new Int32Rect(x, y, 1, 1));
                    var pixels = new byte[4];
                    cb.CopyPixels(pixels, 4, 0);
                    c = Color.FromRgb(pixels[2], pixels[1], pixels[0]);
                }
                catch (Exception) { }
            }
            return c;
        }
        private void Pixel_Enter(object sender, MouseEventArgs e)
        {
            switch (tool)
            {
                case 0:
                    if (e.LeftButton == MouseButtonState.Pressed)
                    {
                        ((Border)e.Source).Background = selectedColor;
                    }
                    break;
                case 1:
                    if (e.LeftButton == MouseButtonState.Pressed)
                    {
                        ((Border)e.Source).Background = emptyColor;
                    }
                    break;
            }
        }
        private void Color_cpk_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (color_cpk.SelectedColor.HasValue)
            {
                selectedColor = new SolidColorBrush((Color)color_cpk.SelectedColor);
            }
        }

        private void Pixel_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Alt)
            {
                if (e.Delta > 0)
                {
                    image_img.Width *= 1.25;
                    image_img.Height *= 1.25;
                    canvas_ug.Width *= 1.25;
                    canvas_ug.Height *= 1.25;
                    double mx = Mouse.GetPosition(this).X;
                    double my = Mouse.GetPosition(this).Y;
                    image_scv.ScrollToHorizontalOffset((int)(mx - 1.25 * (mx - image_scv.HorizontalOffset)));
                    image_scv.ScrollToVerticalOffset((int)(my - 1.25 * (my - image_scv.VerticalOffset)));
                }
                else if (e.Delta < 0)
                {
                    image_img.Width *= 0.8;
                    image_img.Height *= 0.8;
                    canvas_ug.Width *= 0.8;
                    canvas_ug.Height *= 0.8;
                    double mx = Mouse.GetPosition(this).X;
                    double my = Mouse.GetPosition(this).Y;
                    image_scv.ScrollToHorizontalOffset((int)(mx - 0.80 * (mx - image_scv.HorizontalOffset)));
                    image_scv.ScrollToVerticalOffset((int)(my - 0.80 * (my - image_scv.VerticalOffset)));
                }
            }
        }

        private void Save_btn_Click(object sender, RoutedEventArgs e)
        {
            byte[,,] pixels = new byte[cols, rows, 4];
            for (int c = 0; c < cols; c++)
            {
                for (int r = 0; r < rows; r++)
                {
                    Color color = ((SolidColorBrush)(((Border)canvas_ug.Children[(c * rows) + r]).Background)).Color;
                    pixels[c, r, 0] = color.B;
                    pixels[c, r, 1] = color.G;
                    pixels[c, r, 2] = color.R;
                    pixels[c, r, 3] = color.A;
                }
            }
            byte[] pixels1d = new byte[rows * cols * 4];
            int index = 0;
            for (int c = 0; c < cols; c++)
            {
                for (int r = 0; r < rows; r++)
                {
                    for (int i = 0; i < 4; i++)
                        pixels1d[index++] = pixels[c, r, i];
                }
            }
            Int32Rect rect = new Int32Rect(0, 0, cols, rows);
            WriteableBitmap bmp = new WriteableBitmap(cols, rows, 72, 72, PixelFormats.Bgra32, BitmapPalettes.WebPaletteTransparent);
            bmp.WritePixels(rect, pixels1d, 4 * cols, 0);
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.InitialDirectory = "c:\\";
            dlg.Filter = "PNG Files|*.png";
            dlg.RestoreDirectory = true;
            if (dlg.ShowDialog() == true)
            {
                string save_dir = dlg.FileName;
                bmp.Save(save_dir);
            }
        }

        private void Tools_lv_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            tool = tools_lv.SelectedIndex;
        }

		private void width_dud_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
            if((int)e.NewValue <= 0)
			{
                width_dud.Value = 1;
			}
            if ((int)e.NewValue > image_img.Width)
            {
                width_dud.Value = (int)image_img.Width;
            }
        }

		private void height_dud_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
            if ((int)e.NewValue <= 0)
            {
                height_dud.Value = 1;
            }
            if ((int)e.NewValue > image_img.Width)
            {
                height_dud.Value = (int)image_img.Width;
            }
        }
	}
}
