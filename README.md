# RealTimeCaptionOverlay

A is a real-time captioning application that just captures audio from your system and then displays the live transcription as text overlays. This project utilizes [Vosk's training model](https://alphacephei.com/vosk/) for speech recognition and [NAudio](https://github.com/naudio/NAudio) for capturing audio input. All required nuget packages are pre-installed so no external dependencies needed! clone-and-play

## Requirements

- **.NET 6.0 or later**
- **Vosk Model**: A Vosk speech recognition model (e.g., `vosk-model-small-en-us-0.15`).
- **NAudio**: For capturing system audio.

## Installation

1. **Clone the Repository**:
   ```bash
   git clone https://github.com/<your-username>/OverlayCaptions.git
   cd OverlayCaptions
   dotnet run

## Demo

[![Watch the video](https://img.youtube.com/vi/72KirniPBrM/0.jpg)](https://www.youtube.com/watch?v=72KirniPBrM&ab_channel=xqyet)

## Run Application
- **The application will start capturing audio from your system immediately upon launch.**
- **The transcriptions will appear on your screen, updated in real time.**
- **To move the caption window, click and drag it.**
