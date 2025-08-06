# Cherry-Labs

Cherry-Labs is a Windows desktop application built using WinUI 3 and .NET 8. It integrates Google's Gemini API to analyze video content by extracting and analyzing image frames. The application uses FFmpeg to process video input and supports context-aware multi-turn conversations with the Gemini model.

## Features

- AI-powered video analysis using Gemini 2.0 Flash
- Extracts frames from videos at 4 FPS using FFmpeg
- Processes up to 480 frames per video
- Sends images in batches to Gemini for scene understanding
- Maintains conversation context across multiple requests
- Secure API key loading via system environment variable
- Self-contained executable; no .NET installation required

## Getting Started

### Requirements

- Windows 10 version 1809 (build 17763) or later
- .NET 8 SDK (for building from source)
- Visual Studio 2022 or later (for development)
- FFmpeg executable (included in distribution or download manually)

### Installation

1. Download the published folder containing `Cherry-Labs.exe` and related files.

2. Set the `GEMINI_API_KEY` environment variable:
   - Open the Run dialog (`Win + R`), type `SystemPropertiesAdvanced`, and press Enter.
   - Click "Environment Variables..."
   - Under "User variables", click "New" and add:
     - Name: `GEMINI_API_KEY`
     - Value: your Gemini API key
3. install FFMPEG using winget `winget install ffmpeg`

4. Run `Cherry-Labs.exe` to start the application.

## Building from Source

### Prerequisites

- Windows 10 1809 or later
- Visual Studio 2022 with .NET 8 and Windows App SDK (v1.7+) workloads installed
- FFmpeg installed

### Clone and Build

```bash
git clone https://github.com/your-username/Cherry-Labs.git
cd Cherry-Labs
