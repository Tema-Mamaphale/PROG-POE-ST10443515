# 🛡️ Cybersecurity Awareness Chatbot

A WPF-based desktop chatbot built in C# to help users improve their cybersecurity knowledge. The application offers an engaging chat interface, task manager with reminders, and a dynamic quiz feature — all supported by a lightweight NLP-style response system.

## 🔗 Quick Access

- 🔍 **GitHub Project Repository:** 
- 🎥 **Video Presentation (YouTube):** 

---

## 🚀 Features

### 💬 Chatbot Interface
- Smart responses to cybersecurity-related queries
- Keyword-triggered tips and advice
- Detects user intent for tasks, reminders, and quizzes
- Personalizes conversation with saved user names
- ASCII banner and modern UI styling

### 📝 Task Manager
- Add tasks manually or through the chatbot
- Set reminder dates and priorities
- Weekly recurring options
- Tasks categorized by cybersecurity topics (e.g., Password, Phishing)
- Mark tasks complete or delete them
- Reminder system notifies users via chat interface

### 🧠 Cybersecurity Quiz
- 10 randomized questions each session
- Real-time feedback and explanations
- Tracks quiz scores and displays mistakes summary
- Stores quiz history locally (`quiz_scores.json`)

### 📜 Activity Log
- Automatically logs all user actions and interactions
- Exportable to a timestamped text file

### 🔊 Intro Audio
- Plays a welcome sound upon app launch

---

## 🧱 Tech Stack

- **C#** (.NET WPF)
- **XAML** for UI design
- **Newtonsoft.Json** for file-based storage (tasks and scores)
- **MediaPlayer** for audio support
- **DispatcherTimer** for live reminder checks

---

## 📁 Project Structure
/CyberSecurityChatbot
│
├── MainWindow.xaml # UI layout
├── MainWindow.xaml.cs # Application logic
├── POEChatbot/
│ ├── QuizData.cs # Static quiz content loader
│ ├── TaskItem.cs # Task model (priority, category, etc.)
│ └── QuizQuestion.cs # Quiz structure and score logic
│
├── tasks.json # Local task data store
├── quiz_scores.json # Local quiz score history
└── Luna_Chatbot.wav # Intro audio file
---

## ⚙️ Getting Started

### Prerequisites
- Windows OS
- .NET Desktop Runtime (WPF-compatible)
- Visual Studio (recommended) with WPF template support

### Setup
1. Clone the repository:
   ```bash
   git clone https://github.com/Tema-Mamaphale/cybersecurity-chatbot.git
Open the .sln in Visual Studio.

Ensure Luna_Chatbot.wav is located at the correct file path or update the code accordingly.

Press F5 or select Start Debugging to run the project.
🧪 Sample Interactions
Adding a Task (Chat):

add task Change router password
yes remind me in 3 days

Quiz Mode:

start quiz or let's play a game

Learning Tips:

give me a tip on phishing
what is social engineering?

Other Commands:

show log, help, my name is Alex
📌 Cybersecurity Topics Covered
Password Safety

Two-Factor Authentication (2FA)

Phishing & Social Engineering

Firewalls & Encryption

Malware, Ransomware, Botnets

VPNs & Dark Web

Cyber Hygiene & Zero-Day Threats

Password Managers

DDoS Attacks

📦 Data Storage
Tasks and scores are stored as .json files locally:

tasks.json

quiz_scores.json
👨‍💻 Author
Mamaphale Tema
Cybersecurity POEChatbot — PROG6221
ST10443515

🛡️ License
This project is open-source and free to use for educational and personal purposes.

