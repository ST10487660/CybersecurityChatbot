# CybersecurityChatbot
 # Cybersecurity Awareness Chatbot

A WPF desktop chatbot that teaches users about cybersecurity topics including passwords, phishing, malware, privacy, and more.

**Student name:** Matlou Shaun Rakgoale]  
**Student number:** [ST10487660]  
**Module:** PROG6221 - Programming 2A  

---

## Features implemented (Part 2)

- WPF GUI with dark colour scheme, ASCII art header, and image logo
- Voice greeting played on launch (`greeting.wav`)
- User name captured on first message and used throughout the conversation
- Favourite topic stored and referenced in personalised responses
- Keyword recognition for 10 cybersecurity topics with 4 responses each
- Responses selected randomly from a list — never the same reply twice in a row
- Sentiment detection for worried, curious, frustrated, and happy moods
- Empathetic opener prepended automatically when sentiment is detected
- Follow-up handling — typing "more" or "tell me more" continues the current topic
- Fallback response for unrecognised input
- Delegate and event-driven architecture (no logic in MainWindow.xaml.cs)

---

## Prerequisites

- Windows 10 or 11
- Visual Studio 2022
- .NET 8.0 SDK

---

## How to clone and run

```bash
git clone https://github.com/YourUsername/CybersecurityChatbot.git
cd CybersecurityChatbot
```

1. Open `CybersecurityChatbot.sln` in Visual Studio 2022
2. Press **F5** or click **Start** to build and run

---

## Asset files

Both files must be in the same folder as the compiled `.exe` (e.g. `bin/Debug/net8.0-windows/`).

| File | Purpose | How to set up |
|------|---------|---------------|
| `greeting.wav` | Voice greeting played on launch | In Solution Explorer, right-click the file > Properties > Copy to Output Directory: **Copy always** |
| `logo.png` | Image displayed in the header | Same setting as above |

The app starts and runs without either file — they are loaded silently if present.

---

## Project structure

```
CybersecurityChatbot/
├── CybersecurityChatbot.sln
├── README.md
├── .gitignore
├── .github/
│   └── workflows/
│       └── build.yml
└── CybersecurityChatbot/
    ├── App.xaml
    ├── App.xaml.cs
    ├── MainWindow.xaml
    ├── MainWindow.xaml.cs
    ├── ChatBot.cs
    ├── KeywordResponder.cs
    ├── SentimentDetector.cs
    ├── MemoryStore.cs
    ├── greeting.wav
    └── logo.png
```

---




