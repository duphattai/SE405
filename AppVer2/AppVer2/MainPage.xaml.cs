using AppVer2.SubPage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using WindowsPreview.Media.Ocr;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace AppVer2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private WriteableBitmap wbBitmap = null;
        CoreApplicationView view;
        private ExtractTextOcr extract = new ExtractTextOcr();
        Point Point1, Point2;
        bool isCropEnable = false;

        private OcrLanguage _language;
        private Dictionary<OcrLanguage, string> _supportLanguage;

        public MainPage()
        {
            this.InitializeComponent();
            DrawerLayout.InitializeDrawerLayout();
            this.NavigationCacheMode = NavigationCacheMode.Required;
            view = CoreApplication.GetCurrentView();

            loadImageFromFile();

            _supportLanguage = new Dictionary<OcrLanguage, string>();
            _supportLanguage.Add(OcrLanguage.English, "English");
            _supportLanguage.Add(OcrLanguage.Italian, "Italian");
            _supportLanguage.Add(OcrLanguage.Japanese, "Japanese");
            _supportLanguage.Add(OcrLanguage.French, "French");

            CompositionTarget.Rendering += new EventHandler<object>(CompositionTarget_Rendering);
        }

        private void CompositionTarget_Rendering(object sender, object e)
        {
            //Used for rendering the cropping rectangle on the image.  
            rect.SetValue(Canvas.LeftProperty, (Point1.X < Point2.X) ? Point1.X : Point2.X);
            rect.SetValue(Canvas.TopProperty, (Point1.Y < Point2.Y) ? Point1.Y : Point2.Y);
            rect.Width = (int)Math.Abs(Point2.X - Point1.X);
            rect.Height = (int)Math.Abs(Point2.Y - Point1.Y);
        }

        async void loadImageFromFile()
        {
            if (GlobalVariable.filecapture != null)
            {
                BitmapImage bmpImage = new BitmapImage(new Uri(GlobalVariable.filecapture.Path));
                imgSource.Source = bmpImage;

                var stream = await GlobalVariable.filecapture.OpenAsync(FileAccessMode.Read);
                wbBitmap = new WriteableBitmap((int)imgSource.Width, (int)imgSource.Height);
                wbBitmap.SetSource(stream);
            }
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        async protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Windows.Phone.UI.Input.HardwareButtons.BackPressed += HardwareButtons_BackPressed;

            if (GlobalVariable.filecapture != null)
            {
                var stream = await GlobalVariable.filecapture.OpenAsync(FileAccessMode.Read);

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.SetSource(await GlobalVariable.filecapture.OpenAsync(FileAccessMode.Read));

                wbBitmap = new WriteableBitmap(bitmapImage.PixelWidth, bitmapImage.PixelHeight);
                wbBitmap.SetSource(stream);

                imgSource.Source = wbBitmap;
            }
        }

        void HardwareButtons_BackPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
            if (DrawerLayout.IsDrawerOpen)
            {
                e.Handled = true;
                DrawerLayout.CloseDrawer();
            }
        }

        private void DrawerIcon_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (DrawerLayout.IsDrawerOpen)
            {
                DrawerLayout.CloseDrawer();
            }
            else
            {
                DrawerLayout.OpenDrawer();
            }
        }

        private void OpenFileButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FileOpenPicker filePicker = new FileOpenPicker();
            filePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            filePicker.ViewMode = PickerViewMode.Thumbnail;

            // Filter to include a sample subset of file types
            filePicker.FileTypeFilter.Clear();
            filePicker.FileTypeFilter.Add(".bmp");
            filePicker.FileTypeFilter.Add(".png");
            filePicker.FileTypeFilter.Add(".jpeg");
            filePicker.FileTypeFilter.Add(".jpg");

            filePicker.PickSingleFileAndContinue();
            view.Activated += viewActivated;
        }

        private void SaveButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FileSavePicker savePicker = new FileSavePicker();
            savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            // Dropdown of file types the user can save the file a
            savePicker.FileTypeChoices.Add("Plain Text", new List<string>() { ".doc" });
            // Default extension if the user does not select a choice explicitly from the dropdown
            savePicker.DefaultFileExtension = ".doc";
            // Default file name if the user does not type one in or select a file to replace
            savePicker.SuggestedFileName = "New Document";
            savePicker.PickSaveFileAndContinue();
            view.Activated += viewActivated;
        }

        private async void viewActivated(CoreApplicationView sender, IActivatedEventArgs args)
        {
            if (args != null)
            {
                switch (args.Kind)
                {
                    case ActivationKind.PickFileContinuation:

                        FileOpenPickerContinuationEventArgs argsOpen = args as FileOpenPickerContinuationEventArgs;
                        if (argsOpen != null)
                        {
                            if (argsOpen.Files.Count == 0) return;

                            view.Activated -= viewActivated;
                            StorageFile storageFile = argsOpen.Files[0];

                            var stream = await storageFile.OpenAsync(FileAccessMode.Read);

                            BitmapImage bitmapImage = new BitmapImage();
                            bitmapImage.SetSource(await storageFile.OpenAsync(FileAccessMode.Read));

                            wbBitmap = new WriteableBitmap(bitmapImage.PixelWidth, bitmapImage.PixelHeight);
                            wbBitmap.SetSource(stream);

                            imgSource.Source = wbBitmap;
                        }

                        break;
                }
            }
        }

        async private void ConvertButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            WriteableBitmap temp = wbBitmap;
            string text = "";
            if (wbBitmap == null)
                text = "Please select an image to convert.";
            else
            {
                if (isCropEnable)
                {
                    if (Point1 != null && Point2 != null && Point1 != Point2)
                    {
                        int left = Convert.ToInt32((Point1.X < Point2.X) ? Point1.X : Point2.X);
                        int top = Convert.ToInt32((Point1.Y < Point2.Y) ? Point1.Y : Point2.Y);
                        temp = wbBitmap.Crop(left, top, (int)Math.Abs(Point2.X - Point1.X), (int)Math.Abs(Point2.Y - Point1.Y));
                    }
                }
            }

            text = await extract.ExtractText(temp, imgSource, 16, _language);

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => Frame.Navigate(typeof(TranslatePage), 
                new TranslatePageModel { Text = text, Image = temp }));
        }

        async private void Camera_Click(object sender, RoutedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => Frame.Navigate(typeof(CapturePage)));
        }

        async private void SendEmailButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => Frame.Navigate(typeof(SendEmailPage)));
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            // ... Get the ComboBox reference.
            var comboBox = sender as ComboBox;

            // ... Assign the ItemsSource to the List.
            comboBox.ItemsSource = _supportLanguage.Select(o => o.Value);

            // ... Make the first item selected.
            comboBox.SelectedIndex = 0;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string text = (sender as ComboBox).SelectedItem as string;
            _language = _supportLanguage.FirstOrDefault(x => x.Value == text).Key;
        }

        private void imgSource_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if(isCropEnable)
            {
                Point1 = e.GetCurrentPoint(imgSource).Position;//Set first touchable cordinates as point1
                Point2 = Point1;
            }
        }

        private void imgSource_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if(isCropEnable)
            {
                Point2 = e.GetCurrentPoint(imgSource).Position;
            }
        }

        private void imgSource_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if(isCropEnable)
            {
                Point2 = e.GetCurrentPoint(imgSource).Position;
            }
        }

        private void btnCrop_Click(object sender, RoutedEventArgs e)
        {
            isCropEnable = !isCropEnable;
            rect.Visibility = isCropEnable ? Visibility.Visible : Visibility.Collapsed;
            scrollView.HorizontalScrollMode = isCropEnable ? ScrollMode.Disabled : ScrollMode.Enabled;
            scrollView.VerticalScrollMode = isCropEnable ? ScrollMode.Disabled : ScrollMode.Enabled;
            scrollView.ZoomMode = isCropEnable ? ZoomMode.Disabled : ZoomMode.Enabled;
        }
    }
}
