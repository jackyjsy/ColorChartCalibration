using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using Windows.Media;
using Windows.Media.Capture;
using Windows.Media.Devices;
using Windows.Media.MediaProperties;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;

using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;



using Windows.UI.Xaml.Media.Imaging;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;


namespace CameraManualControls
{
    ///// <summary>
    ///// Use timetick to call process frames methods
    ///// wait for last process to complete before trigger next
    ///// </summary>
    public sealed partial class MainPage : Page
    {
        #region Get and process frames

        private DispatcherTimer timer = new DispatcherTimer();
        static bool TimerHandlerComplete = true;
        private async void TimeTickHandler(object sender, object e)
        {
            if (TimerHandlerComplete == true)
            {
                TimerHandlerComplete = false;
                await GetFrameAsync();
                TimerHandlerComplete = true;
            }
        }
        private async Task GetFrameAsync()
        {
            try
            {
                if (_mediaCapture != null)
                {
                    // Get information about the preview
                    var previewProperties = _mediaCapture.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview) as VideoEncodingProperties;

                    // Create the video frame to request a SoftwareBitmap preview frame
                    var videoFrame = new VideoFrame(BitmapPixelFormat.Bgra8, (int)previewProperties.Width, (int)previewProperties.Height);
                    // Capture the preview frame
                    var currentFrame = await _mediaCapture.GetPreviewFrameAsync(videoFrame);
                    currentFrame_sbmp = currentFrame.SoftwareBitmap;
                    currentFrame_wbmp = new WriteableBitmap(currentFrame_sbmp.PixelWidth, currentFrame_sbmp.PixelHeight);
                    currentFrame_sbmp.CopyToBuffer(currentFrame_wbmp.PixelBuffer);
                    // Check start check box
                    if (StartCheckBox.IsChecked == true)
                    {
                        ColorChartRectangle.Visibility = Visibility.Visible;
                        DetectColor();
                    }
                    else
                    {
                        ColorChartRectangle.Visibility = Visibility.Collapsed;
                    }
                }

            }
            catch (System.Runtime.InteropServices.COMException)
            {
                Debug.Write("Error in GetFrameAsync capture frame: COMException\n");
            }
            catch (System.NullReferenceException)
            {
                Debug.Write("Error in GetFrameAsync capture frame: NullReferenceException\n");
            }
        }

        private void DetectColor()
        {

            var preview_width = PreviewControl.ActualWidth;
            var preview_height = PreviewControl.ActualHeight;

            ColorChartRectangle.Width = preview_width / 5;
            ColorChartRectangle.Height = preview_height / 5;


            var color_chart_x = preview_width * 2 / 5;
            var color_chart_y = preview_height * 2 / 5;

            int color_chart_cols = 6;
            int color_chart_rows = 4;
            //Convert preview coordinates to frame coordinates
            var row_coord = color_chart_y + ColorChartRectangle.Height * (color_chart_rows - 0.5) / color_chart_rows;
            row_coord = row_coord / preview_height * currentFrame_wbmp.PixelHeight;
            var col_coord = color_chart_x + ColorChartRectangle.Width * 0.5 / color_chart_cols;
            col_coord = col_coord / preview_width * currentFrame_wbmp.PixelWidth;
            var col_width = ColorChartRectangle.Width / color_chart_cols / preview_width * currentFrame_wbmp.PixelWidth;

            List<Color> color_list = new List<Color>();
            for (int i = 0; i < color_chart_cols; i++)
            {
                Color color = currentFrame_wbmp.GetPixel(Convert.ToInt32(col_coord + i * col_width), Convert.ToInt32(row_coord));
                //Debug.WriteLine(color.ToString());
                color_list.Add(color);
            }
            DrawColorChart(color_list);
        }

        private void DrawColorChart(List<Color> color_list)
        {
            WriteableBitmap patch = new WriteableBitmap(color_list.Count * 100, 100);

            for (int i = 0; i < color_list.Count; i++)
            {
                patch.FillRectangle(100 * i, 0, 100 * (i + 1), 100, color_list[i]);

            }
            image.Source = patch;

        }

        
        private void StartCheckBox_CheckChanged(object sender, object e)
        {
            if (StartCheckBox.IsChecked == true)
            {
                //DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = new TimeSpan(0, 0, 0, 0, 33);
                timer.Tick += TimeTickHandler;
                timer.Start();
            }
        }
        #endregion
    }
}
