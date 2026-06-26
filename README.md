# Cybersecurity Chatbot - Part 3

## Project Overview
This project is a WPF-based Cybersecurity Chatbot built in C#.

Part 3 expands the system with:
- Task management with MySQL database integration
- Cybersecurity quiz system (12 questions)
- NLP-style intent detection
- Activity logging system
- Improved UI with multiple functional tabs

---

## Features

### Task Assistant
- Add, view, and manage tasks
- Stores data in a MySQL database
- Automatically creates database and tables on first run

### Cybersecurity Quiz
- 12 questions (Multiple Choice and True/False)
- Instant scoring system
- Tracks user performance

### NLP Processor
- Simulates natural language processing
- Detects intent using keyword matching
- Improves chatbot flexibility

### Activity Log
- Tracks recent user actions
- Stores timestamps for each action
- Helps monitor system usage

---

## Setup Instructions

### Step 1: Install MySQL NuGet Package
In Visual Studio:
1. Right-click the project in Solution Explorer
2. Select "Manage NuGet Packages"
3. Go to the "Browse" tab
4. Search for "MySql.Data"
5. Install "MySql.Data" by Oracle

Or via Package Manager Console:
```powershell
Install-Package MySql.Data




