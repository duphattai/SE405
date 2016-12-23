using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Phone.UI.Input;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace AppVer2.SubPage
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TranslatePage : Page
    {
        private CoreApplicationView view;

        public TranslatePage()
        {
            this.InitializeComponent();
            view = CoreApplication.GetCurrentView();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            txtResult.Text = e.Parameter.ToString();
            //System.Windows.Clipboard.SetText(txtResult.Text);
        }

        private void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame != null && rootFrame.CanGoBack)
            {
                e.Handled = true;
                rootFrame.GoBack();
            }
        }

        private void CloseTranslate_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (stackPanelTranslate.Height != 0)
                HideStoryboard.Begin();
        }

        private void Translate_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if(stackPanelTranslate.Height == 0)
                ShowStoryboard.Begin();
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
                    case ActivationKind.PickSaveFileContinuation:

                        FileSavePickerContinuationEventArgs argsSave = args as FileSavePickerContinuationEventArgs;
                        if (argsSave != null)
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

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.NavigationCacheMode = NavigationCacheMode.Required;
            HardwareButtons.BackPressed += HardwareButtons_BackPressed;
        }
    }
}
