using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Windows.Media.Capture;            //For MediaCapture
using Windows.Media.MediaProperties;    //For Encoding Image in JPEG format
using Windows.Storage;                  //For storing Capture Image in App storage or in Picture Library
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Core;
using Windows.Phone.UI.Input;    //For BitmapImage. for showing image on screen we need BitmapImage format.

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace AppVer2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CapturePage : Page
    {
        //Declare MediaCapture object globally
        Windows.Media.Capture.MediaCapture captureManager;
      
        public CapturePage()
        {
            this.InitializeComponent();
           
           
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
           
        }

       
       async private void Capture_Photo_Click(object sender, RoutedEventArgs e)
        {
            //Create JPEG image Encoding format for storing image in JPEG type
            ImageEncodingProperties imgFormat = ImageEncodingProperties.CreateJpeg();

            // create storage file in local app storage
            StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync("Photo.jpg", CreationCollisionOption.ReplaceExisting);

            // take photo and store it on file location.
            await captureManager.CapturePhotoToStorageFileAsync(imgFormat, file);

            //// create storage file in Picture Library
            //StorageFile file = await KnownFolders.PicturesLibrary.CreateFileAsync("Photo.jpg",CreationCollisionOption.GenerateUniqueName);

            // Get photo as a BitmapImage using storage file path.
            BitmapImage bmpImage = new BitmapImage(new Uri(file.Path));

            GlobalVariable.filecapture = file;
           //Stop camera if click capture
            await captureManager.StopPreviewAsync();    //stop camera capturing

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => Frame.Navigate(typeof(MainPage)));

        }

         private void Start_Capture_Preview_Click(object sender, RoutedEventArgs e)
        {
            start_Capture_Preview();
        }
        async private void start_Capture_Preview()
        {
            captureManager = new MediaCapture();        //Define MediaCapture object
            await captureManager.InitializeAsync();     //Initialize MediaCapture and 
            capturePreview.Source = captureManager;     //Start preiving on CaptureElement
            await captureManager.StartPreviewAsync();   //Start camera capturing 
        }
        async private void Stop_Capture_Preview()
        {
            await captureManager.StopPreviewAsync();    //stop camera capturing
        }
       private void Stop_Capture_Preview_Click(object sender, RoutedEventArgs e)
        {
            Stop_Capture_Preview();
        }
       void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
       {
           Frame rootFrame = Window.Current.Content as Frame;

           if (rootFrame != null && rootFrame.CanGoBack)
           {
               e.Handled = true;
               Stop_Capture_Preview();
               rootFrame.GoBack();
           }
       }

       private void Page_Loaded(object sender, RoutedEventArgs e)
       {
           this.NavigationCacheMode = NavigationCacheMode.Required;
           HardwareButtons.BackPressed += HardwareButtons_BackPressed;
           start_Capture_Preview();
       }
    }
}
