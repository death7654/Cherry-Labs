using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
// users
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
//https
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.NetworkOperators;
using Windows.Storage;

// file management
using Windows.Storage.Pickers;
using WinRT.Interop;




// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Cherry_Labs
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private List<(string role, string text)> contextStack = new();

        private ObservableCollection<string> ChatMessages = new();
        public MainWindow()
        {
            InitializeComponent();
            ChatList.ItemsSource = ChatMessages;
        }

        // detect if key pressed is "ENTER"
        private void ChatInput_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                // prevents a new line from being created
                e.Handled = true;

                // sends request to the request handling function
                SendButton_Click(this, new RoutedEventArgs());
            }
        }
        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {

            // gets the value in the rich text box
            ChatInput.Document.GetText(Microsoft.UI.Text.TextGetOptions.None, out string user_input);
            user_input = user_input.Trim();

            // only sends a request to gemini if the user_input has something of value
            if (!string.IsNullOrEmpty(user_input))
            {
                // clear input
                ChatInput.Document.SetText(Microsoft.UI.Text.TextSetOptions.None, "");

                // add to chat section
                ChatMessages.Add("You: " + user_input + "\n");

                // sends the request to gemini and updates the chat
                string assistantReply = await SendGeminiRequest(user_input);
                ChatMessages.Add("Bot: " + assistantReply);
            }
        }


        private async Task<string> SendGeminiRequest(string user_input)
        {
            string api_key = Environment.GetEnvironmentVariable("GEMINI_API_KEY")?.Trim();


            // checks if key exists
            if (string.IsNullOrWhiteSpace(api_key))
            {
                return "Gemini API Key has not been set. Please edit your system's environment variables and add key named \"GEMINI_API_KEY\" with its value being the API key.\nThank you.";
            }

            // link
            string endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={api_key}";

            // system-level instruction to keep context
            const string systemPrompt = "You are a helpful video assistant. When the user refers to 'video', 'it', or other vague terms, use the previous conversation history to resolve what they are talking about. Do not ask the user for clarification unless absolutely necessary. Always remember and refer to earlier messages when generating a response.";

            // build the prompt content array
            var contents = new List<object>
{
                new {
    role = "user",
    parts = new[] { new { text = systemPrompt } }
} };

            // add prior conversation context
            foreach (var (role, text) in contextStack)
            {
                contents.Add(new
                {
                    role = role,
                    parts = new[] { new { text = text } }
                });
            }

            // add current user message
            contents.Add(new
            {
                role = "user",
                parts = new[] { new { text = user_input } }
            });

            var request = new
            {
                contents = contents
            };

            string jsonBody = JsonSerializer.Serialize(request);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            using var client = new HttpClient();
            var response = await client.PostAsync(endpoint, content);
            string response_json = await response.Content.ReadAsStringAsync();

            // extract assistant's reply
            string assistantReply = ExtractGeminiReply(response_json);

            // add new messages to context
            contextStack.Add(("user", user_input));
            contextStack.Add(("model", assistantReply));

            // limit stack size
            if (contextStack.Count > 32)
                contextStack = contextStack.Skip(contextStack.Count - 32).ToList();

            return assistantReply;
        }

        private async void AttachButton_Click(object sender, RoutedEventArgs e)
        {
            ChatInput.Document.GetText(Microsoft.UI.Text.TextGetOptions.None, out string user_input);

            var picker = new FileOpenPicker();

            // Associate the picker with the current window
            var hwnd = WindowNative.GetWindowHandle(this);
            InitializeWithWindow.Initialize(picker, hwnd);

            // filter file type
            picker.FileTypeFilter.Add(".mp4");
            picker.FileTypeFilter.Add(".mov");
            picker.FileTypeFilter.Add(".mkv");

            // let user pick a file
            var file = await picker.PickSingleFileAsync();

            if (file == null)
            {
                ChatMessages.Add("No file has been selected or an error has occured\n");
                return;
            }
            if (!(await check_ffmpeg()))
            {
                ChatMessages.Add("Ffmpeg is not install or not in path");
                return;
            }
            ChatMessages.Add("Your file is being processed \n\n ");
            await ProcessVideoWithStorageFileAsync(file);


            string selectedPath = file.Path;
            ChatMessages.Add("Your file has been processed successfully with path \n\n " + selectedPath);


            return;
        }

        private async Task<bool> check_ffmpeg()
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "ffmpeg",
                        Arguments = "-version",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                return output.Contains("ffmpeg version");
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> ProcessVideoWithStorageFileAsync(StorageFile videoFile)
        {
            string ffmpegPath = "ffmpeg";

            string videoPath = videoFile.Path;

            // output directory for extracted frames
            string outputDir = Path.Combine(AppContext.BaseDirectory, "frames");
            Directory.CreateDirectory(outputDir);

            // ffmpeg command to extract 4 FPS
            string args = $"-i \"{videoPath}\" -vf fps=4 \"{Path.Combine(outputDir, "frame_%04d.jpg")}\"";

            var ffmpegProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = ffmpegPath,
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            ffmpegProcess.Start();
            await ffmpegProcess.WaitForExitAsync();

            // Collect up to 480 frames
            string[] frames = Directory.GetFiles(outputDir, "frame_*.jpg");
            List<string> base64Images = new();

            foreach (var frame in frames.Take(480))
            {
                byte[] imageBytes = await File.ReadAllBytesAsync(frame);
                string base64 = Convert.ToBase64String(imageBytes);
                base64Images.Add(base64);
            }

            // split into batches of 16 and send to Gemini
            List<string> contextResponses = new();
            string apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY")?.Trim();

            for (int i = 0; i < base64Images.Count; i += 16)
            {
                var batch = base64Images.Skip(i).Take(16).ToList();
                var contents = new List<object>
        {
            new {
                role = "user",
                parts = new[] {
                    new { text = "Analyze these video frames and describe any important events or traffic violations. Focus on what you can visually detect." }
                }
            }
        };

                foreach (var img in batch)
                {
                    contents.Add(new
                    {
                        role = "user",
                        parts = new[] {
                    new {
                        inlineData = new {
                            mimeType = "image/jpeg",
                            data = img
                        }
                    }
                }
                    });
                }

                var request = new { contents };
                string json = JsonSerializer.Serialize(request);

                using var client = new HttpClient();
                var response = await client.PostAsync(
                    $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={apiKey}",
                    new StringContent(json, Encoding.UTF8, "application/json")
                );

                string responseJson = await response.Content.ReadAsStringAsync();
                string reply = ExtractGeminiReply(responseJson);
                contextResponses.Add(reply);
            }

            // create a single context
            string fullContext = string.Join("\n", contextResponses);
                        contextStack.Add(("model", fullContext));

            return fullContext;
        }


        // helper functions

        // used to extract the text data and not the other data
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
