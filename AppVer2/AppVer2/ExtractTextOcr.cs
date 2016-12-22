using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using WindowsPreview.Media.Ocr;

namespace AppVer2
{
    class ExtractTextOcr
    {
        public string text;

        public async Task<string> ExtractText(WriteableBitmap bitmap, Image prevImage)
        {
            if (bitmap == null)
            {
                text = "Please select an image to convert.";
            }
            else
            {
                OcrEngine ocrEngine = new OcrEngine(OcrLanguage.English);
                // Supported image dimensions are between 40 and 2600 pixels.   
                if (bitmap.PixelHeight < 40 ||
                    bitmap.PixelHeight > 2600 ||
                    bitmap.PixelWidth < 40 ||
                    bitmap.PixelWidth > 2600)
                {
                    text = "Image size is not supported." +
                                        Environment.NewLine +
                                        "Loaded image size is " + bitmap.PixelWidth + "x" + bitmap.PixelHeight + "." +
                                        Environment.NewLine +
                                        "Supported image dimensions are between 40 and 2600 pixels.";

                    return text;
                }

                // This main API call to extract text from image.   
                OcrResult ocrResult = await ocrEngine.RecognizeAsync((uint)bitmap.PixelHeight, (uint)bitmap.PixelWidth, bitmap.PixelBuffer.ToArray());

                // OCR result does not contain any lines, no text was recognized.    
                if (ocrResult.Lines != null)
                {
                    // Used for text overlay.   
                    // Prepare scale transform for words since image is not displayed in original format.   
                    var scaleTrasform = new ScaleTransform
                    {
                        CenterX = 0,
                        CenterY = 0,
                        ScaleX = prevImage.ActualWidth / bitmap.PixelWidth,
                        ScaleY = prevImage.ActualHeight / bitmap.PixelHeight,
                    };

                    if (ocrResult.TextAngle != null)
                    {

                        prevImage.RenderTransform = new RotateTransform
                        {
                            Angle = (double)ocrResult.TextAngle,
                            CenterX = prevImage.ActualWidth / 2,
                            CenterY = prevImage.ActualHeight / 2
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
                        extractedText += " ";
                    }

                    text = extractedText;
                }
                else
                {
                    text = "No text.";
                }
            }
            return text;
        }  
    }
}
