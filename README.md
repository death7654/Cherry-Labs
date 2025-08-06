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

## Visual Representation 
<img width="1024" height="1536" alt="image" src="https://github.com/user-attachments/assets/604cba48-a9fd-4c25-adba-44f868bfec06" />


## Tech Stack Justification

### Backend

- **.NET 8**: Provides modern, high-performance APIs with full support for Windows 10+ desktop development.
- **WinUI 3**: Microsoft's latest UI framework for building fluent Windows desktop apps.
- **FFmpeg**: A fast and reliable tool for frame extraction without video decoding overhead.

### AI Model

- **Gemini 2.0 Flash (via Google Generative Language API)**:
  - Chosen for its multi-modal capabilities and high performance with image understanding.
  - Supports multi-turn chat with visual context.
  - Allows integration using REST API with structured payloads.


## Getting Started

### Installation

1. Download the latest release from releases [releases](https://github.com/death7654/Cherry-Labs/releases)

2. Generate a Gemini API Key
   - Go to [Google's AI Studio](https://aistudio.google.com/apikey)
   - Click on Create API Key, and copy its value
   - Open the Run dialog (`Win + R`), type `SystemPropertiesAdvanced`, and press Enter.
   - Click "Environment Variables..."
   - Under "User variables", click "New" and add:
     - Name: `GEMINI_API_KEY`
     - Value: your Gemini API key
4. install FFMPEG using winget `winget install ffmpeg`

5. Run `Cherry-Labs.exe` to start the application.

### Requirements

- Windows 10 version 1809 (build 17763) or later
- .NET 8 SDK (for building from source)
- Visual Studio 2022 or later (for development)
- FFmpeg executable (included in distribution or download manually)


## Building from Source

### Prerequisites

- Windows 10 1809 or later
- Visual Studio 2022 with .NET 8 and Windows App SDK (v1.7+) workloads installed
- FFmpeg installed

### Clone and Build

```bash
git clone https://github.com/your-username/Cherry-Labs.git
cd Cherry-Labs
