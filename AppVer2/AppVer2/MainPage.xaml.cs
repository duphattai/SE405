using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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
using Newtonsoft.Json;

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

        public MainPage()
        {
            this.InitializeComponent();
            DrawerLayout.InitializeDrawerLayout();
            this.NavigationCacheMode = NavigationCacheMode.Required;
            view = CoreApplication.GetCurrentView();
            
            loadImageFromFile();
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
        async  protected override void OnNavigatedTo(NavigationEventArgs e)
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
               
            }
            else
            {
                Application.Current.Exit();
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
            if(args != null)
            {
                switch(args.Kind)
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
                    case ActivationKind.PickSaveFileContinuation:

                        FileSavePickerContinuationEventArgs argsSave = args as FileSavePickerContinuationEventArgs;
                        if(argsSave != null)
                        {
                            view.Activated -= viewActivated;

                            CachedFileManager.DeferUpdates(argsSave.File);
                            await FileIO.WriteTextAsync(argsSave.File, txtResult.Text.ToString());
                            FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(argsSave.File);
                        }
                        break;
                }
            }
        }

        private async void ConvertButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (wbBitmap == null)
            {
                txtResult.Text = "Please select an image to convert.";
            }
            else
            {
                OcrEngine ocrEngine = new OcrEngine(OcrLanguage.English);
                // Supported image dimensions are between 40 and 2600 pixels.   
                if (wbBitmap.PixelHeight < 40 ||
                    wbBitmap.PixelHeight > 2600 ||
                    wbBitmap.PixelWidth < 40 ||
                    wbBitmap.PixelWidth > 2600)
                {
                    txtResult.Text = "Image size is not supported." +
                                        Environment.NewLine +
                                        "Loaded image size is " + wbBitmap.PixelWidth + "x" + wbBitmap.PixelHeight + "." +
                                        Environment.NewLine +
                                        "Supported image dimensions are between 40 and 2600 pixels.";

                    return;
                }

                // This main API call to extract text from image.   
                OcrResult ocrResult = await ocrEngine.RecognizeAsync((uint)wbBitmap.PixelHeight, (uint)wbBitmap.PixelWidth, wbBitmap.PixelBuffer.ToArray());

                // OCR result does not contain any lines, no text was recognized.    
                if (ocrResult.Lines != null)
                {
                    // Used for text overlay.   
                    // Prepare scale transform for words since image is not displayed in original format.   
                    var scaleTrasform = new ScaleTransform
                    {
                        CenterX = 0,
                        CenterY = 0,
                        ScaleX = imgSource.ActualWidth / wbBitmap.PixelWidth,
                        ScaleY = imgSource.ActualHeight / wbBitmap.PixelHeight,
                    };

                    if (ocrResult.TextAngle != null)
                    {

                        imgSource.RenderTransform = new RotateTransform
                        {
                            Angle = (double)ocrResult.TextAngle,
                            CenterX = imgSource.ActualWidth / 2,
                            CenterY = imgSource.ActualHeight / 2
                        };
                    }

                    string extractedText = "";
                    // Iterate over recognized lines of text.   
                    foreach (var line in ocrResult.Lines)
                    {
                        // Iterate over words in line.   
                        foreach (var word in line.Words)
                        {
                            var originalRect = new Rect(word.Left, word.Top, word.Width, word.Height);
                            var overlayRect = scaleTrasform.TransformBounds(originalRect);

                            var wordTextBlock = new TextBlock()
                            {
                                Height = overlayRect.Height,
                                Width = overlayRect.Width,
                                FontSize = overlayRect.Height * 1.2,
                                Text = word.Text,

                            };

                            // Define position, background, etc.   
                            var border = new Border()
                            {
                                Margin = new Thickness(overlayRect.Left, overlayRect.Top, 0, 0),
                                Height = overlayRect.Height,
                                Width = overlayRect.Width,
                                Background = new SolidColorBrush(Colors.Orange),
                                Opacity = 0.5,
                                HorizontalAlignment = HorizontalAlignment.Left,
                                VerticalAlignment = VerticalAlignment.Top,
                                Child = wordTextBlock,

                            };

                            // Put the filled textblock in the results grid.   
                            extractedText += word.Text + " ";
                        }
                        extractedText += Environment.NewLine;
                    }

                    txtResult.Text = extractedText;

                    //translate found text
                    string yandexApiKey = "trnsl.1.1.20161219T152557Z.390c84297f36ab8b.09d9e851c72d37d3f3ccf9fbda2b48a6a01a2a88";
                    string apiUrl = "https://translate.yandex.net/api/v1.5/tr.json/translate?key={0}&text={1}&lang={2}&format=plain";
                    HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(string.Format(apiUrl, yandexApiKey, extractedText, "vi"));
                    request.Method = "GET";
                    using (var response = (HttpWebResponse)(await Task<WebResponse>.Factory.FromAsync(request.BeginGetResponse, request.EndGetResponse, null)))
                    {
                        Stream stream = response.GetResponseStream();
                        StreamReader reader = new StreamReader(stream);
                        string htmlText = reader.ReadToEnd();
                        TranslateResponse translate = JsonConvert.DeserializeObject<TranslateResponse>(htmlText);

                        //not show translate if it's vietnamese or translate failed
                        if(translate.code == 200 && !translate.lang.StartsWith("vi"))
                        {
                            foreach(string text in translate.text)
                            {
                                txtResult.Text += text;
                            }
                        }

                    }
                    //HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                }
                else
                {
                    txtResult.Text = "No text.";
                }
            }
        }

        async private void Camera_Click(object sender, RoutedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>Frame.Navigate(typeof(CapturePage)));
        }

        async private void SendEmailButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => Frame.Navigate(typeof(SubPage.SendEmailPage)));
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
