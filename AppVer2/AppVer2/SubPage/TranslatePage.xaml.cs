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
        private Dictionary<string, string> _supportedTranslateLanguage;

        public Dictionary<string, string> SupportedTranslateLanguage
        {
            get { return _supportedTranslateLanguage; }
            set
            {
                _supportedTranslateLanguage = value;
            }
        }

        public TranslatePage()
        {
            this.InitializeComponent();
            view = CoreApplication.GetCurrentView();

            _supportedTranslateLanguage = new Dictionary<string, string>();
            _supportedTranslateLanguage.Add("vi", "Tiếng Việt");
            _supportedTranslateLanguage.Add("en", "English");
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var model = e.Parameter as TranslatePageModel;
            txtResult.Text = model.Text;
            imgSource.Source = model.Image;
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

        private async void Translate_Tapped(object sender, TappedRoutedEventArgs e)
        {
            TranslateText translator = new TranslateText();
            var targetLang = _supportedTranslateLanguage.Where(o => o.Value == selectedLanguage.SelectedValue.ToString());
            if(targetLang != null && targetLang.Count() > 0)
            {
                txtResult.Text += await translator.Translate(txtResult.Text, targetLang.FirstOrDefault().Key);
            }
            
            if (stackPanelTranslate.Height == 0)
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

        private void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            selectedLanguage.ItemsSource = _supportedTranslateLanguage.Select(o => o.Value);
            selectedLanguage.SelectedIndex = 0;
        }
    }
}
