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
        private WasapiLoopbackCapture loopbackCapture; // Capture  audio
        private VoskRecognizer recognizer;

        public MainWindow()
        {
            InitializeComponent();
            InitializeSpeechRecognition();
        }

        private void InitializeSpeechRecognition()
        {
            // for vosk model
            var modelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Models", "vosk-model-small-en-us-0.15");
            if (!Directory.Exists(modelPath))
            {
                throw new DirectoryNotFoundException($"Model path not found: {modelPath}");
            }

            
            var model = new Model(modelPath);

            
            recognizer = new VoskRecognizer(model, 16000.0f);

            
            loopbackCapture = new WasapiLoopbackCapture
            {
                WaveFormat = new WaveFormat(16000, 1) 
            };

           
            loopbackCapture.DataAvailable += LoopbackCapture_DataAvailable;

            
            loopbackCapture.StartRecording();
        }

        private void LoopbackCapture_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (recognizer.AcceptWaveform(e.Buffer, e.BytesRecorded))
            {
                var resultJson = recognizer.Result();
                var resultText = ExtractTextFromJson(resultJson); // Extract 
                Dispatcher.Invoke(() =>
                {
                    ClearAndSetCaptionText(resultText); // Immediately clear 
                });
            }
            else
            {
                var partialJson = recognizer.PartialResult();
                var partialText = ExtractTextFromJson(partialJson); // Extract text
                if (!string.IsNullOrWhiteSpace(partialText))
                {
                    Dispatcher.Invoke(() =>
                    {
                        ClearAndSetCaptionText(partialText); // 
                    });
                }
            }
        }

        private string ExtractTextFromJson(string json)
        {
            try
            {
                // Parse json
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
                // Ignore 
            }
            return string.Empty;
        }

        private void ClearAndSetCaptionText(string newText)
        {
            // Clear 
            CaptionText.Text = string.Empty; // Clear
            CaptionText.Text = newText.Trim(); 
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // for box dragging
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            // Cleanup stuff
            loopbackCapture.StopRecording();
            loopbackCapture.Dispose();
            recognizer.Dispose();
        }
    }
}
