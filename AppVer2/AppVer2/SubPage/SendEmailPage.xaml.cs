using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Email;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Phone.UI.Input;
using Windows.Storage;
using Windows.Storage.Pickers;
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
    public sealed partial class SendEmailPage : Page
    {
        private CoreApplicationView view;
        private List<StorageFile> attachFiles;
        public SendEmailPage()
        {
            this.InitializeComponent();
            view = CoreApplication.GetCurrentView();
            attachFiles = new List<StorageFile>();
          
          
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            HardwareButtons.BackPressed += HardwareButtons_BackPressed;
        }

        void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame != null && rootFrame.CanGoBack)
            {
                e.Handled = true;
                rootFrame.GoBack();
            }
        }
        private void AttachFileButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FileOpenPicker filePicker = new FileOpenPicker();
            filePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            filePicker.ViewMode = PickerViewMode.Thumbnail;

            // Filter to include a sample subset of file types
            filePicker.FileTypeFilter.Clear();
            filePicker.FileTypeFilter.Add(".docx");
            filePicker.FileTypeFilter.Add(".doc");
            filePicker.FileTypeFilter.Add(".txt");

            filePicker.PickSingleFileAndContinue();
            view.Activated += viewActivated; 
        }

        private void viewActivated(CoreApplicationView sender, IActivatedEventArgs args)
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
                            attachFiles.Add(argsOpen.Files[0]);
                        }

                        break;
                }
            }
        }

        async private void SendButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            EmailRecipient sendTo = new EmailRecipient()
            {
                Address = ToAddress.Text
            };

            EmailMessage mail = new EmailMessage();
            mail.Subject = Subject.Text;
            mail.Body = Body.Text;
 
            // Add recipients to the mail object
            mail.To.Add(sendTo);
            if(attachFiles.Count != 0)
            {
                foreach (var file in attachFiles)
                    mail.Attachments.Add(new EmailAttachment(file.Name, file));
            }
               
 
            // Open the share contract with Mail only:
            await EmailManager.ShowComposeNewEmailAsync(mail);                
        }

        private void SendEmailForm_Loaded(object sender, RoutedEventArgs e)
        {
            this.NavigationCacheMode = NavigationCacheMode.Required;
           
        }

    }
}
