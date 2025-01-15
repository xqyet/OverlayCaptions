using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using NAudio.Wave;
using Vosk;

namespace OverlayCaptions
{
    public partial class MainWindow : Window
    {
        private WasapiLoopbackCapture loopbackCapture; // Captures the system audio
        private VoskRecognizer recognizer;

        public MainWindow()
        {
            InitializeComponent();
            InitializeSpeechRecognition();
        }

        private void InitializeSpeechRecognition()
        {
            // Resolve relative path to Vosk model
            var modelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Models", "vosk-model-small-en-us-0.15");
            if (!Directory.Exists(modelPath))
            {
                throw new DirectoryNotFoundException($"Model path not found: {modelPath}");
            }

            // Initialize Vosk model
            var model = new Model(modelPath);

            // Initialize recognizer
            recognizer = new VoskRecognizer(model, 16000.0f);

            // Initialize system audio capture after recognizer initialization
            loopbackCapture = new WasapiLoopbackCapture
            {
                WaveFormat = new WaveFormat(16000, 1) // 16 kHz mono audio
            };

            // Event for data capture
            loopbackCapture.DataAvailable += LoopbackCapture_DataAvailable;

            // Start capturing system audio
            loopbackCapture.StartRecording();
        }

        private void LoopbackCapture_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (recognizer.AcceptWaveform(e.Buffer, e.BytesRecorded))
            {
                var resultJson = recognizer.Result();
                var resultText = ExtractTextFromJson(resultJson); // Extract transcription text
                Dispatcher.Invoke(() =>
                {
                    ClearAndSetCaptionText(resultText); // Immediately clear and update with final text
                });
            }
            else
            {
                var partialJson = recognizer.PartialResult();
                var partialText = ExtractTextFromJson(partialJson); // Extract partial text
                if (!string.IsNullOrWhiteSpace(partialText))
                {
                    Dispatcher.Invoke(() =>
                    {
                        ClearAndSetCaptionText(partialText); // Immediately clear and update with partial text
                    });
                }
            }
        }

        private string ExtractTextFromJson(string json)
        {
            try
            {
                // Parse the JSON and extract the text
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("text", out var textElement))
                {
                    return textElement.GetString();
                }
                if (doc.RootElement.TryGetProperty("partial", out var partialElement))
                {
                    return partialElement.GetString();
                }
            }
            catch
            {
                // Ignore errors and return the raw JSON if parsing fails
            }
            return string.Empty;
        }

        private void ClearAndSetCaptionText(string newText)
        {
            // Clear the current text and set the new transcription
            CaptionText.Text = string.Empty; // Clear old text
            CaptionText.Text = newText.Trim(); // Set the new text
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Allow dragging the window
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            // Cleanup resources when the window is closed
            loopbackCapture.StopRecording();
            loopbackCapture.Dispose();
            recognizer.Dispose();
        }
    }
}
