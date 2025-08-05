using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
//https
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.NetworkOperators;

// users
using System.Collections.ObjectModel;
using System.Text.Json;



// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Cherry_Labs
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private ObservableCollection<string> ChatMessages = new();
        public MainWindow()
        {
            InitializeComponent();
            ChatList.ItemsSource = ChatMessages;
        }
        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            ChatInput.Document.GetText(Microsoft.UI.Text.TextGetOptions.None, out string userInput);
            userInput = userInput.Trim();

            if (!string.IsNullOrEmpty(userInput))
            {
                ChatMessages.Add("You: " + userInput);

                string assistantReply = await SendGeminiRequest(userInput);
                ChatMessages.Add("Bot: " + assistantReply);

                // Clear input
                ChatInput.Document.SetText(Microsoft.UI.Text.TextSetOptions.None, "");
            }
        }


        private async Task<string> SendGeminiRequest(string user_input)
        {
            string api_key = Environment.GetEnvironmentVariable("GEMINI_API_KEY").Trim();

            if (string.IsNullOrWhiteSpace(api_key))
            {
                return "Gemini API Key has not been set. Please edit your system's enviorment variables and add key named \"GEMINI_API_KEY\" with its value being the api key. \nThank you.";
            }


            //set global endpoint
            string endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={api_key}";

            // create a new request
            var request = new
            {
                contents = new[]
                {
                    new {
                        parts = new[] { new { text = user_input } }
                }
            }
            };

            // convert to JSON
            string jsonBody = JsonSerializer.Serialize(request);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            using var client = new HttpClient();
            var response = await client.PostAsync(endpoint, content);
            string response_json = await response.Content.ReadAsStringAsync();


            return ExtractGeminiReply(response_json);
        }

        private string ExtractGeminiReply(string json)
        {
        try
        {
            var doc = JsonDocument.Parse(json);
            return doc.RootElement
                      .GetProperty("candidates")[0]
                      .GetProperty("content")
                      .GetProperty("parts")[0]
                      .GetProperty("text")
                      .GetString() ?? "(empty)";
        }
        catch
        {
            return "Failed to parse Gemini response.";
        }
    }



}
}
