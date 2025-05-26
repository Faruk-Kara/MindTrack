# MindTrack 🧠

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET Framework](https://img.shields.io/badge/.NET%20Framework-4.7.2-blue.svg)](https://dotnet.microsoft.com/download/dotnet-framework)
[![Platform](https://img.shields.io/badge/platform-Windows-lightgrey.svg)](https://www.microsoft.com/windows)

MindTrack is a comprehensive productivity and well-being application designed to help users manage their daily tasks, track their mood, maintain focus, and interact with an AI assistant. Built with C# Windows Forms, it provides an intuitive interface for personal productivity management.

## ✨ Features

- **🌤️ Weather Display**: Real-time weather conditions for your location
- **📋 Task Management**: Create, edit, and track daily tasks with completion status
- **⏱️ Focus Mode**: Timer-based focus sessions with motivational messages and session tracking
- **😊 Mood Tracking**: Daily mood logging with visual feedback and historical data
- **🤖 AI Assistant**: Chat with an AI assistant powered by LM Studio for personalized advice
- **🌐 Translation Support**: Built-in translation between Turkish and English
- **📊 Statistics**: Visual representation of productivity metrics and mood trends
- **💾 Local Database**: SQLite database for secure local data storage

## 🖼️ Screenshots

*Screenshots will be added here soon*

## 🛠️ Prerequisites

Before running MindTrack, ensure you have the following installed:

- **Windows OS** (Windows 10 or later recommended)
- **.NET Framework 4.7.2** or later
- **Visual Studio 2019/2022** (for development)
- **SQLite** (automatically handled by NuGet packages)

### External Services (Optional)

- **LM Studio** - For AI chat functionality
- **Translation Service** - Custom translation service running on localhost:5000
- **OpenWeatherMap API** - For weather data

## 🚀 Installation & Setup

### 1. Clone the Repository

```bash
git clone https://github.com/Faruk-Kara/MindTrack.git
cd MindTrack
```

### 2. Open in Visual Studio

1. Open `MindTrack.sln` in Visual Studio
2. Restore NuGet packages (should happen automatically)
3. Build the solution (`Ctrl+Shift+B`)

### 3. Configuration

Update the following settings in `MindTrack/App.config`:

```xml
<appSettings>
    <!-- Weather API Configuration -->
    <add key="WeatherApiKey" value="YOUR_OPENWEATHERMAP_API_KEY" />
    <add key="DefaultCity" value="Your City Name" />
    
    <!-- AI Service Configuration -->
    <add key="LmStudioEndpoint" value="http://127.0.0.1:1234/v1/chat/completions" />
    
    <!-- Translation Service Configuration -->
    <add key="TranslateEndpoint" value="http://127.0.0.1:5000/translate" />
</appSettings>
```

### 4. API Keys Setup

#### OpenWeatherMap API
1. Visit [OpenWeatherMap](https://openweathermap.org/api)
2. Sign up for a free account
3. Generate an API key
4. Add the key to your `App.config`

#### LM Studio (Optional)
1. Download and install [LM Studio](https://lmstudio.ai/)
2. Load your preferred language model
3. Start the local server (default: http://127.0.0.1:1234)

### 5. Run the Application

Press `F5` in Visual Studio or run the built executable from `bin/Debug/` or `bin/Release/`

## 📖 Usage Guide

### Main Dashboard
- **Left Panel**: Weather information and current time
- **Center Panel**: Task management and AI chat interface
- **Right Panel**: Quick access to Focus Mode and Mood Tracking

### 📋 Task Management
1. Click **"Add Task"** to create a new task
2. Enter task description and press `Enter`
3. Use checkboxes to mark tasks as complete
4. Click **"Save"** to persist changes to the database

### ⏱️ Focus Mode
1. Click **"Focus Mode"** to open the focus timer
2. Set your desired session duration
3. Click **"Start"** to begin the focus session
4. Receive motivational messages during the session
5. Click **"Stop"** to end early or let it complete naturally
6. Click **"Save"** to record the session in your statistics

### 😊 Mood Tracking
1. Click **"Mood"** to open mood tracking
2. Select your current mood from the available options
3. Click **"Save"** to record your mood entry
4. View mood trends in the Statistics section

### 🤖 AI Chat
1. Click **"AI Chat"** to open the chat interface
2. Type your message and press `Enter`
3. The AI responds in the same language as your input
4. Use the **"Clear"** button to start a new conversation

### 📊 Statistics
1. Click **"Stats"** to view your productivity dashboard
2. Select date ranges to analyze specific periods
3. Review focus session durations, mood patterns, and task completion rates

## 🗄️ Database Structure

MindTrack uses SQLite with the following schema:

```sql
-- Mood tracking table
CREATE TABLE mood_entries (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    date TEXT NOT NULL,
    mood TEXT NOT NULL,
    timestamp DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- Task management table
CREATE TABLE tasks (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    description TEXT NOT NULL,
    completed BOOLEAN DEFAULT 0,
    created_date DATETIME DEFAULT CURRENT_TIMESTAMP,
    completed_date DATETIME
);

-- Focus session tracking table
CREATE TABLE focus_sessions (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    duration INTEGER NOT NULL,
    notes TEXT,
    session_date DATETIME DEFAULT CURRENT_TIMESTAMP
);
```

## 🏗️ Architecture

MindTrack follows a modular architecture:

- **`Form1.cs`** - Main application window and dashboard
- **`TaskForm.cs`** - Task management interface
- **`FocusForm.cs`** - Focus timer and session management
- **`MoodForm.cs`** - Mood tracking interface
- **`ChatForm.cs`** - AI chat interface
- **`StatsForm.cs`** - Statistics and analytics dashboard
- **`DatabaseHelper.cs`** - SQLite database operations
- **`WeatherService.cs`** - Weather API integration
- **`LmStudioService.cs`** - AI service integration
- **`TranslateService.cs`** - Translation service integration

## 🔧 Development

### Building from Source

```bash
# Clone the repository
git clone https://github.com/yourusername/MindTrack.git

# Open in Visual Studio
# Build -> Build Solution (Ctrl+Shift+B)

# Or use MSBuild from command line
msbuild MindTrack.sln /p:Configuration=Release
```

### Dependencies

The project uses the following NuGet packages:

- **System.Data.SQLite** - SQLite database support
- **Newtonsoft.Json** - JSON serialization
- **RestSharp** - HTTP client for API calls
- **EntityFramework** - ORM for database operations
- **System.Text.Json** - Modern JSON handling

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request. For major changes, please open an issue first to discuss what you would like to change.

### Development Guidelines

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Weather data provided by [OpenWeatherMap](https://openweathermap.org/)
- AI functionality powered by [LM Studio](https://lmstudio.ai/)
- Icons and UI inspiration from modern productivity applications

## 📞 Support

If you encounter any issues or have questions:

1. Check the [Issues](https://github.com/yourusername/MindTrack/issues) page
2. Create a new issue if your problem isn't already reported
3. Provide detailed information about your environment and the issue

---

**Made with ❤️ for productivity enthusiasts** 
