using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AppVer2
{
    class TranslateText
    {
        public async Task<string> Translate(string source, string targetLanguage)
        {
            string result = "";
            string yandexApiKey = "trnsl.1.1.20161219T152557Z.390c84297f36ab8b.09d9e851c72d37d3f3ccf9fbda2b48a6a01a2a88";
            string apiUrl = "https://translate.yandex.net/api/v1.5/tr.json/translate?key={0}&text={1}&lang={2}&format=plain";
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(string.Format(apiUrl, yandexApiKey, source, targetLanguage));
            request.Method = "GET";
            using (var response = (HttpWebResponse)(await Task<WebResponse>.Factory.FromAsync(request.BeginGetResponse, request.EndGetResponse, null)))
            {
                Stream stream = response.GetResponseStream();
                StreamReader reader = new StreamReader(stream);
                string htmlText = reader.ReadToEnd();
                TranslateResponse translate = JsonConvert.DeserializeObject<TranslateResponse>(htmlText);

                //not show translate if it's vietnamese or translate failed
                if (translate.code == 200 && !translate.lang.StartsWith("vi"))
                {
                    foreach (string translatedText in translate.text)
                    {
                        result += translatedText;
                    }
                }
            }
            return result;
        }
    }
}
