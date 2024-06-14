using Microsoft.UI.Windowing;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.Graphics;
using System.Globalization;
using Windows.Devices.Enumeration;
using Windows.Devices.Display;
using System.Linq;
using System.Threading.Tasks;

// If you enjoy this project, you can support it by making a donation!
// Donation link: https://buymeacoffee.com/_ronniexcoder
// You can also visit my YouTube channel for more content: https://www.youtube.com/@ronniexcoder

namespace Prank
{

    public sealed partial class MainWindow : Window
    {
        private DispatcherTimer timer;
        private TimeSpan timeSpan;
        private int selectedOption;
        private int screenWidth;
        private int screenHeight;

        public MainWindow()
        {
            this.InitializeComponent();
            this.Title = "Prank!";
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WindowId WndID = Win32Interop.GetWindowIdFromWindow(hwnd);
            AppWindow appW = AppWindow.GetFromWindowId(WndID);
            appW.SetIcon("D:/VisualStudioProjects/Prank/Assets/icon_16.ico");
            appW.Resize(new SizeInt32 { Width = 350, Height = 700 });
            OverlappedPresenter presenter = appW.Presenter as OverlappedPresenter;
            presenter.IsMaximizable = false;
            presenter.IsResizable = false;
            GetScreenSize();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private async Task GetScreenSize()
        {
            var displayList = await DeviceInformation.FindAllAsync(DisplayMonitor.GetDeviceSelector());

            if (!displayList.Any())
                return;

            var monitorInfo = await DisplayMonitor.FromInterfaceIdAsync(displayList[0].Id);

            screenHeight = monitorInfo.NativeResolutionInRawPixels.Height;
            screenWidth = monitorInfo.NativeResolutionInRawPixels.Width;

        }

        private void Timer_Tick(object sender, object e)
        {
            DateTime currentTime = DateTime.Now;

            TimeBlock.Text = currentTime.ToString("HH:mm:ss", CultureInfo.InvariantCulture);

            if (StartTimerButton.IsChecked == true && currentTime.Hour == timeSpan.Hours &&
                currentTime.Minute == timeSpan.Minutes && currentTime.Second == timeSpan.Seconds)
            {
                FullscreenWindow fullscreenWindow = new FullscreenWindow(screenWidth, screenHeight);

                switch (selectedOption)
                {
                    case 0:
                        fullscreenWindow.StartFreezing();
                        break;
                    case 1:
                        fullscreenWindow.StartUpsideDown();
                        break;
                    case 2:
                        fullscreenWindow.StartRandomColor();
                        break;
                    case 3:
                        fullscreenWindow.StartEarthquake();
                        break;
                    case 4:
                        fullscreenWindow.StartSnow();
                        break;
                    case 5:
                        fullscreenWindow.StartLines();
                        break;
                    case 6:
                        fullscreenWindow.StartPuzzle();
                        break;
                    case 7:
                        fullscreenWindow.StartScreenOfDeath();
                        break;
                }

                fullscreenWindow.Activate();
                StartTimerButton.IsChecked = false;
                StartTimerButton.Content = "Start Timer";
                ComboBox.IsEnabled = true;
            }
        }
        private void PreviewButton_Click(object sender, RoutedEventArgs e)
        {
            FullscreenWindow fullscreenWindow = new FullscreenWindow(screenWidth, screenHeight);

            string buttonName = ((Button)sender).Name;

            switch (buttonName)
            {
                case "FreezingButton":
                    fullscreenWindow.StartFreezing();
                    break;
                case "UpsideDownButton":
                    fullscreenWindow.StartUpsideDown();
                    break;
                case "RandomColorButton":
                    fullscreenWindow.StartRandomColor();
                    break;
                case "EarthquakeButton":
                    fullscreenWindow.StartEarthquake();
                    break;
                case "SnowButton":
                    fullscreenWindow.StartSnow();
                    break;
                case "LinesButton":
                    fullscreenWindow.StartLines();
                    break;
                case "PuzzleButton":
                    fullscreenWindow.StartPuzzle();
                    break;
                case "ScreenOfDeathButton":
                    fullscreenWindow.StartScreenOfDeath();
                    break;
            }
            fullscreenWindow.Activate();
        }
        private void StartTimerButton_Click(object sender, RoutedEventArgs e)
        {
            if (StartTimerButton.IsChecked == true)
            {
                timeSpan = TimePicker.Time;
                selectedOption = ComboBox.SelectedIndex;
                StartTimerButton.Content = "Stop Timer";
                ComboBox.IsEnabled = false;
            }
            else
            {
                StartTimerButton.Content = "Start Timer";
                ComboBox.IsEnabled = true;
            }
        }
        private void StartupToggleButton_Click(object sender, RoutedEventArgs e)
        {
            //Not implemented
        }
        private void HideToTrayButton_Click(object sender, RoutedEventArgs e)
        {
            //Not implemented
        }

    }
}

       
