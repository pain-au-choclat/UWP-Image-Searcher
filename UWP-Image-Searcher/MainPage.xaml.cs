using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Windows.Data.Json;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;

namespace UWP_Image_Searcher
{
    public sealed partial class MainPage : Page
    {
        const string SubscriptionKey = "put your key here!!!";
        const string UriBase = "https://api.cognitive.microsoft.com/bing/v7.0/images/search";

        public MainPage()
        {
            this.InitializeComponent();
        }

        private void OnKeyDownHandler(object sender, KeyRoutedEventArgs e)
        {

            if (e.Key == Windows.System.VirtualKey.Enter &&
                    searchTermsTextBox.Text.Trim().Length > 0)
            {
                string imageUrl = FindUrlOfImage(searchTermsTextBox.Text);

                foundObjectImage.Source = new BitmapImage(new Uri(imageUrl, UriKind.Absolute));

            }
        }

        struct SearchResult
        {
            public String jsonResult;
            public Dictionary<String, String> relevantHeaders;
        }

        private string FindUrlOfImage(string targetString)
        {
            SearchResult result = PerformBingImageSearch(targetString);

            JsonObject jsonObj = JsonObject.Parse(result.jsonResult);
            JsonArray results = jsonObj.GetNamedArray("value");
            if (results.Count > 0)
            {
                JsonObject first_result = results.GetObjectAt(0);
                String imageUrl = first_result.GetNamedString("contentUrl");
                return imageUrl;
            }
            else
                return "https://docs.microsoft.com/learn/windows/build-internet-connected-windows10-apps/media/imagenotfound.png";
        }

        static SearchResult PerformBingImageSearch(string searchTerms)
        {
            string uriQuery = UriBase + "?q=" + Uri.EscapeDataString(searchTerms);
            WebRequest request = WebRequest.Create(uriQuery);
            request.Headers["Ocp-Apim-Subscription-Key"] = SubscriptionKey;
            HttpWebResponse response = (HttpWebResponse)request.GetResponseAsync().Result;
            string json = new StreamReader(response.GetResponseStream()).ReadToEnd();

            var searchResult = new SearchResult()
            {
                jsonResult = json,
                relevantHeaders = new Dictionary<String, String>()
            };

            foreach (String header in response.Headers)
            {
                if (header.StartsWith("BingAPIs-") || header.StartsWith("X-MSEdge-"))
                    searchResult.relevantHeaders[header] = response.Headers[header];
            }

            return searchResult;
        }

    }
}
