using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImaPixelEr
{
	public static class WriteableBitmapExtentions
    {
        // Save the WriteableBitmap into a PNG file.
        public static void Save(this WriteableBitmap wbitmap, string filename)
        {
            // Save the bitmap into a file.
            using (FileStream stream = new FileStream(filename, FileMode.Create))
            {
                // Initialize a PngBitmapEncoder (encoder)
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                // Create and add a bitmapframe to (encoder)
                encoder.Frames.Add(BitmapFrame.Create(wbitmap));
                // Save the stream
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
        private BitmapImage bitmap;
        private bool image_loaded = false;
        private SolidColorBrush selectedColor;
        private SolidColorBrush clickedColor;
        private readonly SolidColorBrush emptyColor = new SolidColorBrush(Colors.Transparent);
        private int rows = 16;
        private int cols = 16;
        private int tool = 0;
        // Upgrade grid size from rows and cols vars
        private void UpdateGrid()
		{
            // Clean the grid
            canvas_ug.Children.Clear();
            // Set the rows count
            canvas_ug.Rows = rows;
            // Set the columns count
            canvas_ug.Columns = cols;
            // Set canvas size with image size
            canvas_ug.Width = image_img.Width;
            canvas_ug.Height = image_img.Height;
            // Loop from 0 to the rows * cols value
            for (int i = 0; i < (rows * cols); i++)
            {
                // Create a border(pixel) to draw pixel
                Border pixel = new Border
                {
                    // Set pixel's border thickness
                    BorderThickness = new Thickness(1),
                    // Set pixel's color
                    BorderBrush = Brushes.Black,
                    // Set pixel's background
                    Background = new SolidColorBrush(Colors.Transparent),
                    // Set pixel's width, it's image width / columns
                    Width = image_img.Width / cols,
                    // Set pixel's height, it's image height / rows
                    Height = image_img.Height / rows
                };
                // Add left click event to the pixel
                pixel.MouseLeftButtonDown += Pixel_Click;
                // Add mouse enter event to the pixel
                pixel.MouseEnter += Pixel_Enter;
                // Add mouse wheel event to the pixel
                pixel.MouseWheel += Pixel_MouseWheel;
                // Add mouse move event to pixel
                pixel.MouseMove += Pixel_Move;
                // Add pixel to the grid
                canvas_ug.Children.Add(pixel);
            }
        }
        // Load image button
        private void Load_img_btn_Click(object sender, RoutedEventArgs e)
        {
            // Create a OpenFileDialog
            OpenFileDialog dlg = new OpenFileDialog();
            // Set default directory
            dlg.InitialDirectory = "c:\\";
            // Set filter
            dlg.Filter = "Image Files|*.jpeg;*.png;*.jpg;*.gif;*.bmp|All Files (*.*)|*.*";
            // Enable restore directory
            dlg.RestoreDirectory = true;
            // If the dialog got an OK
            if (dlg.ShowDialog() == true)
            {
                // Get the filename
                string selectedFileName = dlg.FileName;
                // Put the filename on the textbox
                file_txt.Text = selectedFileName;
                // Create a bitmap image
                bitmap = new BitmapImage();
                // Init the bitmap image
                bitmap.BeginInit();
                // Set uri source to the file
                bitmap.UriSource = new Uri(selectedFileName);
                // End the bitmap image
                bitmap.EndInit();
                // Set the image source to the loaded image
                image_img.Source = bitmap;
                // Set the image size from the loaded image
                image_img.Width = bitmap.Width;
                image_img.Height = bitmap.Height;
                // Flag image_loaded true
                image_loaded = true;
                // Update the grid
                UpdateGrid();
            }
        }
        
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
