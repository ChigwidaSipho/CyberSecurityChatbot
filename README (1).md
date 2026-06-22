# CyberSecurity Awareness Chatbot

A WPF desktop application built in C# (.NET Framework 4.8) that promotes
cybersecurity awareness through natural language conversation, an interactive
quiz, a task management system with reminders, and an activity log. All
features are accessible through the chatbot interface, which is backed by a
MySQL database for persistent data storage.

Developed as a Part 3 POE submission.

---

## Features

### Chatbot (NLP Engine)

- Intent-based response pipeline with session context memory
- Covers the following cybersecurity topics: passwords, phishing, privacy,
  malware, VPN, two-factor authentication, safe browsing, and scam awareness
- Sentiment detection that responds to emotional cues such as worry,
  confusion, frustration, and curiosity before providing a relevant tip
- Remembers the user's name and most recent topic within a session
- Natural language commands navigate the user to any other tab automatically

| Command | Action |
|---|---|
| add task / show tasks / manage tasks | Opens the Task Assistant tab |
| start quiz | Opens and starts the Quiz |
| show activity log / show log | Opens the Activity Log tab |
| tell me more | Expands on the last cybersecurity topic discussed |
| help | Lists available topics |

### Task Assistant with Reminders

- Add cybersecurity-related tasks with a title, description, and optional reminder date
- Mark tasks as complete or delete them
- All task data is persisted in MySQL and survives application restarts
- Full CRUD operations: Create, Read, Update (mark complete), Delete

### Cybersecurity Quiz

- 12 questions covering multiple choice and true/false formats
- Questions are shuffled on each attempt for variety
- Colour-coded feedback per answer with an explanation shown after each question
- Tiered results based on final score: distinction, pass, or needs review

### Activity Log

- All user actions across every tab are recorded with a precise timestamp
- Displays the 10 most recent entries by default, with a Show More option to
  expand to the full history
- Shared across the entire application via a single ActivityLog instance

### MySQL Database Integration

- Task data is stored in and retrieved from a local MySQL database
- Parameterised queries used throughout to prevent SQL injection
- Database credentials are stored in App.config, not in source code
- The tasks table is created automatically on first run if it does not exist

---

## Requirements

| Requirement | Version |
|---|---|
| Operating System | Windows 10 or 11 |
| .NET Framework | 4.8 |
| Visual Studio | 2019 or 2022 (Desktop Development with C# workload) |
| MySQL Server | 8.0 or later |
| MySQL Workbench | Optional, recommended for verifying database state |

---

## Setup Instructions

### 1. Clone the repository

```
git clone https://github.com/ChigwidaSipho/CyberSecurityChatbot.git
cd CyberSecurityChatbot
```

### 2. Create the database

Open MySQL Workbench or the MySQL command-line client and run the included
schema script:

```
mysql -u root -p < schema.sql
```

Alternatively, open schema.sql in MySQL Workbench and execute it directly.
This creates the cybersecurity_chatbot database and the tasks table.

### 3. Configure database credentials

Open App.config in the project root and update the connection string with
your MySQL username and password:

```xml
<connectionStrings>
    <add name="CyberDb"
         connectionString="Server=localhost;Port=3306;Database=cybersecurity_chatbot;Uid=root;Pwd=YOUR_PASSWORD_HERE;"
         providerName="MySql.Data.MySqlClient" />
</connectionStrings>
```

Do not commit your actual password to a public repository. If forking this
project, add App.config to your .gitignore file.

### 4. Restore NuGet packages and build

Open CyberSecurityChatbot.slnx in Visual Studio, then:

```
Build > Restore NuGet Packages
Build > Build Solution  (Ctrl + Shift + B)
```

The primary dependency is MySql.Data 9.7.0, which is declared in
packages.config and will restore automatically.

### 5. Run the application

Press F5 in Visual Studio, or run the compiled executable directly:

```
bin\Debug\CyberSecurityChatbot.exe
```

---

## Project Structure

```
CyberSecurityChatbot/
|
|-- Chatbot.cs                    NLP engine, intent pipeline, topic responses
|-- ChatbotModel.cs               Data transfer object returned by the chatbot
|-- Sentiment.cs                  Emotional tone detection with event system
|-- User.cs                       User session model (name, interest, start time)
|
|-- ChatbotGUI.xaml/.cs           Chat tab, message bubbles, input, suggestion chips
|-- TaskManagerControl.xaml/.cs   Task Assistant tab, add/complete/delete tasks
|-- QuizControl.xaml/.cs          Quiz tab, 12 questions, scoring, results display
|-- ActivityLogControl.xaml/.cs   Activity Log tab, timestamped history, Show More
|-- MainWindow.xaml/.cs           Shell window, owns shared Chatbot, wires all tabs
|
|-- DatabaseHelper.cs             All MySQL operations for the Task Assistant
|-- ActivityLog.cs                Shared in-memory log written to by all features
|
|-- App.config                    Database credentials, not committed to repository
|-- schema.sql                    SQL script to create the database and tables
|-- packages.config               NuGet package declarations
```

---

## Architecture

The application uses an event-driven navigation pattern so the chatbot
engine remains fully decoupled from the WPF user interface.

MainWindow creates a single shared Chatbot instance and injects it into
every tab, so all tabs write to the same ActivityLog. When the NLP engine
detects a navigation command such as "start quiz", it raises a
NavigateRequested event with a destination string. MainWindow listens for
that event and switches tabs accordingly. The chatbot engine itself has no
knowledge of WPF controls, tabs, or any UI code.

This means Chatbot.cs is fully reusable and testable independently of
the interface.

---

## Security Considerations

- Database credentials are stored in App.config rather than in source code.
- All SQL queries use parameterised inputs, eliminating the risk of SQL
  injection through user input.
- App.config should be excluded from version control on any public fork of
  this project.

---

## Dependencies

| Package | Version | Purpose |
|---|---|---|
| MySql.Data | 9.7.0 | MySQL database connector |
| System.Configuration.ConfigurationManager | 10.0.9 | Reading App.config values |
| BouncyCastle.Cryptography | 2.6.2 | Required internally by MySql.Data |
| System.Memory / System.Buffers | 4.6.x | Required internally by MySql.Data |

All remaining references are standard .NET Framework 4.8 assemblies
included with Visual Studio.

---

## Author

Sipho Chigwida
GitHub: https://github.com/ChigwidaSipho
