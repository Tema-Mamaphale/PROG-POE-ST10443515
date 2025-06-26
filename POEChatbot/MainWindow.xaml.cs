using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Newtonsoft.Json;
using POEChatbot;

namespace CyberSecurityChatbot
{
    public partial class MainWindow : Window
    {
        private MediaPlayer introPlayer = new MediaPlayer();
        private string userName = "";
        private DispatcherTimer reminderTimer;
        private List<string> activityLog = new List<string>();
        private List<TaskItem> tasks = new List<TaskItem>();
        private List<QuizQuestion> quizQuestions = new List<QuizQuestion>();
        private List<QuizScore> quizScores = new List<QuizScore>();
        private int currentQuizIndex = 0;
        private int quizScore = 0;

        private TaskItem lastTaskPendingReminder;
        private const string TaskSavePath = "tasks.json";
        private const string ScoreSavePath = "quiz_scores.json";

        public MainWindow()
        {
            InitializeComponent();
            PlayIntroAudio();
            LoadQuizQuestions();
            LoadTasksFromFile();
            LoadQuizScoresFromFile();
            StartReminderChecker();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Optional: focus name input or load last used name
            if (!string.IsNullOrEmpty(userName))
            {
                NamePromptPanel.Visibility = Visibility.Collapsed;
                ChatPanel.Visibility = Visibility.Visible;
            }
        }

        private void SaveName(object sender, RoutedEventArgs e)
        {
            userName = NameInputBox.Text.Trim();
            if (!string.IsNullOrWhiteSpace(userName))
            {
                AddChatBubble($"🤖 Hello, {userName}! Ask me anything about cybersecurity.", false);
                NamePromptPanel.Visibility = Visibility.Collapsed;
                ChatPanel.Visibility = Visibility.Visible;
                activityLog.Add($"Name saved: {userName}");
                RefreshActivityLog();
            }
            else
            {
                MessageBox.Show("Please enter your name before continuing.", "Missing Name", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void AddTask(object sender, RoutedEventArgs e)
        {
            var task = new TaskItem
            {
                Title = TaskTitleBox.Text.Trim(),
                Description = TaskDescBox.Text.Trim(),
                ReminderDate = ReminderDatePicker.SelectedDate,
                TaskPriority = (Priority)(PriorityDropdown.SelectedIndex >= 0 ? PriorityDropdown.SelectedIndex : 1),
                TaskCategory = (Category)(CategoryDropdown.SelectedIndex >= 0 ? CategoryDropdown.SelectedIndex : 0),
                IsRecurring = RecurringCheckBox.IsChecked == true
            };

            if (string.IsNullOrWhiteSpace(task.Title))
            {
                MessageBox.Show("Please provide a title for the task.", "Missing Title", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            tasks.Add(task);
            SaveTasksToFile();
            DisplayTaskList();

            AddChatBubble($"📝 Task '{task.Title}' added!", false);
            activityLog.Add("Manual task added: " + task.Title);
            RefreshActivityLog();

            TaskTitleBox.Clear();
            TaskDescBox.Clear();
            ReminderDatePicker.SelectedDate = null;
            PriorityDropdown.SelectedIndex = -1;
            CategoryDropdown.SelectedIndex = -1;
            RecurringCheckBox.IsChecked = false;
        }



        private void PlayIntroAudio()
        {
            try
            {
                string path = @"C:\Users\lab_services_student\OneDrive - ADvTECH Ltd\Desktop\github\PROG-POE-ST10443515\POEChatbot\Luna_Chatbot.wav";
                if (File.Exists(path))
                {
                    introPlayer.Open(new Uri(path));
                    introPlayer.Play();
                }
                else
                {
                    MessageBox.Show("Audio file not found: " + path);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to play audio: " + ex.Message);
            }
        }


        private void StartReminderChecker()
        {
            reminderTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(30)
            };
            reminderTimer.Tick += CheckForReminders;
            reminderTimer.Start();
        }

        private void CheckForReminders(object sender, EventArgs e)
        {
            DateTime today = DateTime.Today;
            foreach (var task in tasks)
            {
                if (task.ReminderDate.HasValue &&
                    task.ReminderDate.Value.Date == today &&
                    !task.ReminderTriggered)
                {
                    AddChatBubble($"🔔 Reminder: '{task.Title}' is due today!", false);
                    task.ReminderTriggered = true;
                    activityLog.Add("Reminder triggered: " + task.Title);
                    RefreshActivityLog();
                    SaveTasksToFile();
                }
            }
        }

        private void UserInputBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter && !Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
            {
                SendMessage(sender, e);
                e.Handled = true;
            }
        }


        private void SendMessage(object sender, RoutedEventArgs e)
        {
            string input = UserInputBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(input)) return;
            AddChatBubble("🧑 " + input, true);
            UserInputBox.Clear();

            string lower = input.ToLower();

            // Chat-triggered add task
            if (lower.StartsWith("add task "))
            {
                string title = input.Substring(9).Trim();
                if (!string.IsNullOrWhiteSpace(title))
                {
                    var task = new TaskItem
                    {
                        Title = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(title),
                        Description = "Added via chat",
                        ReminderDate = null,
                        TaskPriority = Priority.Medium,
                        TaskCategory = Category.General,
                        IsRecurring = false
                    };

                    tasks.Add(task);
                    SaveTasksToFile();
                    DisplayTaskList();

                    AddChatBubble($"🤖 Task added: '{task.Title}'. Would you like a reminder for this task? Type 'yes remind me' or 'no'.", false);
                    activityLog.Add("Task added via chat: " + task.Title);
                    RefreshActivityLog();

                    // Temporarily store last task for potential reminder
                    lastTaskPendingReminder = task;
                }
                else
                {
                    AddChatBubble("🤖 Please provide a task title after 'add task'.", false);
                }

                return;
            }
            if (lastTaskPendingReminder != null && (lower.StartsWith("yes remind me") || lower == "no"))
            {
                if (lower == "no")
                {
                    AddChatBubble("👍 Got it. No reminder set.", false);
                    lastTaskPendingReminder = null;
                }
                else
                {
                    DateTime reminderDate = DateTime.Today.AddDays(1); // default

                    // Try to parse natural language date
                    if (lower.Contains("in "))
                    {
                        var parts = lower.Split(new string[] { "in " }, StringSplitOptions.None)[1].Split(' ');
                        if (int.TryParse(parts[0], out int days))
                        {
                            reminderDate = DateTime.Today.AddDays(days);
                        }
                    }
                    else if (lower.Contains("on "))
                    {
                        string datePart = lower.Split(new string[] { "on " }, StringSplitOptions.None)[1];
                        if (DateTime.TryParse(datePart, out DateTime parsedDate))
                        {
                            reminderDate = parsedDate;
                        }
                    }


                    lastTaskPendingReminder.ReminderDate = reminderDate;
                    AddChatBubble($"🔔 Reminder set for '{lastTaskPendingReminder.Title}' on {reminderDate:MMM dd}.", false);
                    activityLog.Add($"Reminder set via chat: {lastTaskPendingReminder.Title} on {reminderDate:yyyy-MM-dd}");
                    SaveTasksToFile();
                    lastTaskPendingReminder = null;
                }

                RefreshActivityLog();
                return;
            }
            // Chat-triggered quiz start
            if (lower.Contains("quiz") || lower.Contains("game"))
            {
                AddChatBubble("🤖 Starting quiz now! Switching to the Quiz tab...", false);

                Dispatcher.InvokeAsync(() =>
                {
                    foreach (TabItem tab in MainTabControl.Items)
                    {
                        if (tab.Name == "QuizTab")
                        {
                            MainTabControl.SelectedItem = tab;
                            break;
                        }
                    }

                    StartQuiz(null, null);
                }, DispatcherPriority.Background);

                return;
            }

            // Chat-triggered activity log
            if (lower.Contains("show log") || lower.Contains("activity log") || lower.Contains("history"))
            {
                AddChatBubble("🤖 Here are your recent activities:", false);
                foreach (string entry in activityLog)
                    AddChatBubble("🗒️ " + entry, false);
                return;
            }

            // Default chatbot response
            string intent = DetectIntent(input);
            string response = HandleIntent(intent, input);
            AddChatBubble("🤖 " + response, false);
            activityLog.Add(intent.StartsWith("[NLP]") ? intent : "Chat: " + input);
            RefreshActivityLog();
        }

        private string DetectIntent(string input)
        {
            string lower = input.ToLowerInvariant();

            if (Regex.IsMatch(lower, @"\b(add|create)\b.*\btask\b")) return "[INTENT] Add Task";
            if (Regex.IsMatch(lower, @"\b(remind me)( in| on)?\b")) return "[INTENT] Reminder";
            if (Regex.IsMatch(lower, @"\b(quiz|game|test)\b")) return "[INTENT] Quiz";
            if (Regex.IsMatch(lower, @"\b(log|history|activity)\b")) return "[INTENT] Show Log";
            if (Regex.IsMatch(lower, @"\b(what is your purpose|what do you do|who are you)\b")) return "[INTENT] Purpose";

            return input; // fallback
        }


        private string HandleIntent(string intent, string input)
        {
            if (intent.StartsWith("[INTENT]"))
            {
                if (intent.Contains("Add Task")) return "Use 'add task [title]' to create a task.";
                if (intent.Contains("Reminder")) return "I'll remind you! Say something like 'remind me in 2 days'.";
                if (intent.Contains("Quiz")) return "Let’s play a quiz! Just type 'quiz'.";
                if (intent.Contains("Show Log")) return "Here's your recent activity log.";
                if (intent.Contains("Purpose")) return "I'm your cybersecurity assistant! I help you stay safe online, manage tasks, and learn security best practices.";
            }

            return GetChatbotResponse(input);
        }

        private string GetChatbotResponse(string input)
        {
            input = input.ToLower();
            if (string.IsNullOrEmpty(userName))
                return "Please tell me your name first: 'My name is [Your Name]'.";

            if (input.StartsWith("my name is "))
            {
                userName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input.Replace("my name is ", "").Trim());
                return "Nice to meet you, " + userName + "!";
            }

            if (input.Contains("what is your purpose") ||
    input.Contains("what do you do") ||
    input.Contains("who are you"))
            {
                return "I'm Luna, your cybersecurity assistant. I help you learn how to stay safe online, manage your tasks, and quiz yourself on security knowledge!";
            }
            if (input.Contains("hello") || input.Contains("hi"))
                return "Hi " + userName + "!";

            // Tip request handling
            if (input.Contains("give me a tip") || input.Contains("advice"))
            {
                activityLog.Add("Tip requested");
                RefreshActivityLog();

                // Topic-specific tips
                var tipsByTopic = new Dictionary<string, List<string>>
                {
                    ["phishing"] = new List<string>
            {
                "🎣 Always double-check the sender's email before clicking links.",
                "🚫 Don't download attachments from unknown sources.",
                "🔍 Hover over links to preview the real URL before clicking."
            },
                    ["vpn"] = new List<string>
            {
                "🔐 Use a VPN when on public Wi-Fi to protect your data.",
                "🌍 Choose VPNs with no-logs policies to ensure privacy.",
                "📶 Avoid free VPNs—they often come with privacy trade-offs."
            },
                    ["password"] = new List<string>
            {
                "🔑 Use at least 12 characters with a mix of symbols and numbers.",
                "🧠 Don’t reuse passwords across sites.",
                "🛠️ Use a password manager to securely store and generate passwords."
            },
                    ["2fa"] = new List<string>
            {
                "✅ Always enable 2FA when available.",
                "📱 Use an authenticator app instead of SMS codes for better security.",
                "🔐 Backup your 2FA codes in a safe place."
            },
                    ["malware"] = new List<string>
            {
                "🦠 Don't download software from unofficial websites.",
                "🧼 Keep antivirus software up to date.",
                "🚨 Be wary of unexpected popups asking you to install plugins."
            },
                    ["privacy"] = new List<string>
            {
                "🔍 Review your app permissions regularly.",
                "🧱 Limit data shared on social media profiles.",
                "👻 Use private browsing when researching sensitive topics."
            },
                    ["ransomware"] = new List<string>
            {
                "💾 Backup your important files to an offline drive regularly.",
                "🚫 Don't open unexpected email attachments.",
                "🧰 Keep your operating system and apps updated."
            },


                    ["social engineering"] = new List<string>
    {
        "🎭 Be cautious of messages using fear or urgency to get personal info.",
        "📞 Always verify unknown callers claiming to be from IT or support.",
        "🔐 Never share passwords or codes, even if someone says it's 'urgent'."
    },
                    ["firewall"] = new List<string>
    {
        "🧱 Use a firewall to block unauthorized access to your system.",
        "🔍 Monitor firewall logs for unusual activity.",
        "🔒 Don't disable your firewall unless absolutely necessary."
    },
                    ["encryption"] = new List<string>
    {
        "🔐 Always enable encryption for sensitive files and devices.",
        "🧾 Use encrypted messaging apps for private conversations.",
        "📡 Ensure websites use HTTPS to keep your data safe online."
    },
                    ["botnet"] = new List<string>
    {
        "🤖 Keep your system updated to avoid being part of a botnet.",
        "🧪 Use anti-malware tools to scan for infections.",
        "🛑 Never click unknown links that can install botnet malware."
    },
                    ["zero-day"] = new List<string>
    {
        "🧩 Keep all software updated to minimize zero-day vulnerabilities.",
        "🔒 Use reputable antivirus tools with behavior-based detection.",
        "🌐 Avoid sketchy websites that might host exploits."
    },
                    ["ddos"] = new List<string>
    {
        "🌐 Use a CDN to help absorb traffic spikes from DDoS attacks.",
        "🔍 Monitor for unusual traffic patterns.",
        "🛡️ Protect your infrastructure with DDoS mitigation services."
    },
                    ["cyber hygiene"] = new List<string>
    {
        "🧼 Regularly change your passwords and don't reuse them.",
        "🔄 Update your software and apps frequently.",
        "🧽 Remove unused apps and browser extensions."
    },
                    ["dark web"] = new List<string>
    {
        "🌑 Never access the dark web without understanding the risks.",
        "🕵️ Use identity monitoring tools to check if your data is leaked.",
        "🚫 Don't share personal info or interact with illegal content."
    },
                    ["password manager"] = new List<string>
    {
        "🔐 Use a password manager to generate and store secure passwords.",
        "📱 Enable 2FA on your password manager account.",
        "💾 Regularly back up your password database if it's stored locally."
    }
                };

                // Look for topic-specific keywords
                foreach (var topic in tipsByTopic.Keys)
                {
                    if (input.Contains(topic))
                    {
                        var topicTips = tipsByTopic[topic];
                        return topicTips[new Random().Next(topicTips.Count)];
                    }
                }

                // If no topic matched, return a general tip
                string[] generalTips = new string[]
                {
            "🧠 Think before you click: suspicious links can be traps.",
            "🔒 Use Two-Factor Authentication wherever possible.",
            "🧼 Keep your device updated and clean from unused apps.",
            "🔐 Use strong and unique passwords for each account.",
            "📥 Don’t download attachments from unknown sources."
                };

                return generalTips[new Random().Next(generalTips.Length)];
            }

            if (input.Contains("help"))
                return
            @"👋 Hi there! I'm Luna, your cybersecurity assistant. I can help you:

            • 📚 Learn cybersecurity topics  
            • 🧠 Get practical safety tips  
            • 📝 Manage tasks with reminders  
            • 🕹️ Take quizzes to test your knowledge  

            You can ask me about any of these topics:

            🔐 Passwords  
            ✅ Two-Factor Authentication (2FA)  
            🎣 Phishing  
            🎭 Social Engineering  
            🧱 Firewalls  
            🔐 Encryption  
            🦠 Malware & Ransomware  
            🤖 Botnets  
            🕳️ Zero-Day Vulnerabilities  
            🌐 VPNs  
            🌑 Dark Web  
            🧼 Cyber Hygiene  
            🛠️ Password Managers  
            🌪️ DDoS Attacks  

            To get started, try something like:  
            • 'Give me a tip on phishing'  
            • 'What is social engineering?'  
            • 'Add task Change router password'  
            • 'Start a quiz'  

            I'm here to help you stay safe and cyber-smart! 💡";


            if (input.Contains("phishing"))
                return "⚠️ Phishing is when attackers trick you into giving sensitive info. Don't click suspicious links.";

            if (input.Contains("vpn"))
                return "🔐 A VPN encrypts your data and hides your activity—great for public Wi-Fi.";

            if (input.Contains("2fa") || input.Contains("two-factor"))
                return "🔑 2FA adds another layer of login protection using an app or code.";

            if (input.Contains("password"))
                return "🛡️ Use complex, unique passwords for each account.";

            if (input.Contains("malware"))
                return "🦠 Malware can harm or steal your data. Only install trusted apps.";

            if (input.Contains("privacy"))
                return "🕵️ Be cautious about oversharing online. Review app permissions often.";

            if (input.Contains("ransomware"))
                return "🚫 Ransomware locks your data and demands payment. Back up your files often.";

            if (input.Contains("social engineering"))
                return "🎭 Social engineering is a manipulation technique that tricks people into giving up confidential information. Always verify before trusting requests.";

            if (input.Contains("firewall"))
                return "🧱 A firewall is a security system that monitors and controls incoming and outgoing network traffic based on predetermined rules.";

            if (input.Contains("encryption"))
                return "🔐 Encryption protects your data by converting it into unreadable code that only authorized users can decode.";

            if (input.Contains("botnet"))
                return "🤖 A botnet is a network of infected devices controlled by an attacker. They're often used to launch large-scale attacks like DDoS.";

            if (input.Contains("zero-day"))
                return "🕳️ A zero-day vulnerability is a flaw in software that's unknown to the vendor. It's dangerous because it can be exploited before it's patched.";

            if (input.Contains("ddos"))
                return "🌐 A DDoS attack floods a system with traffic, overwhelming it and making it unavailable to users.";

            if (input.Contains("antivirus"))
                return "🛡️ Antivirus software detects and removes malware. Keep it updated to protect against the latest threats.";

            if (input.Contains("dark web"))
                return "🌑 The dark web is a part of the internet that requires special tools to access. It's often used for illegal activities, but not always.";

            if (input.Contains("cyber hygiene"))
                return "🧼 Cyber hygiene means practicing good digital habits—like updating software, using strong passwords, and avoiding suspicious links.";

            if (input.Contains("password manager"))
                return "🔑 A password manager stores your passwords securely and helps you create strong, unique ones for every site.";

            if (input.Contains("worried"))
                return userName + ", it's okay to be concerned—learning is the first step.";

            if (input.Contains("frustrated"))
                return "Cybersecurity can be overwhelming, but you're making progress!";

            if (input.Contains("curious"))
                return "Great! Ask me anything about staying safe online.";

            return "I'm not sure I understood. Try asking about phishing, VPN, passwords, or say 'give me a tip on 2FA'.";
        }

        private void AddChatBubble(string message, bool isUser)
        {
            var border = new Border
            {
                Background = isUser ? Brushes.LightBlue : Brushes.LightGray,
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(8),
                Margin = new Thickness(5),
                HorizontalAlignment = isUser ? HorizontalAlignment.Right : HorizontalAlignment.Left,
                MaxWidth = 400,
                Child = new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap }
            };
            ChatBubblePanel.Children.Add(border);
        }

        private void DisplayTaskList()
        {
            TaskList.Items.Clear();
            foreach (var task in tasks
                  .OrderByDescending(t => !t.IsCompleted) // incomplete first
                  .ThenBy(t => t.ReminderDate ?? DateTime.MaxValue) // soonest due date
                  .ThenBy(t => t.TaskPriority)) // by priority

            {
                var cb = new CheckBox
                {
                    Content = task.Title + (task.ReminderDate.HasValue ? $" (due {task.ReminderDate.Value:MM/dd})" : ""),
                    IsChecked = task.IsCompleted,
                    Width = 300
                };
                cb.Checked += (s, e) => { task.IsCompleted = true; SaveTasksToFile(); };
                cb.Unchecked += (s, e) => { task.IsCompleted = false; SaveTasksToFile(); };

                var btn = new Button { Content = "Delete", Margin = new Thickness(5, 0, 0, 0) };
                btn.Click += (s, e) =>
                {
                    tasks.Remove(task);
                    SaveTasksToFile();
                    DisplayTaskList();
                    activityLog.Add("Task deleted: " + task.Title);
                    RefreshActivityLog();
                };

                var sp = new StackPanel { Orientation = Orientation.Horizontal };
                sp.Children.Add(cb); sp.Children.Add(btn);
                TaskList.Items.Add(sp);
            }
        }

        private void LoadTasksFromFile()
        {
            if (File.Exists(TaskSavePath))
            {
                try { tasks = JsonConvert.DeserializeObject<List<TaskItem>>(File.ReadAllText(TaskSavePath)) ?? new List<TaskItem>(); }
                catch { tasks = new List<TaskItem>(); }
            }
            DisplayTaskList();
        }

        private void SaveTasksToFile()
        {
            File.WriteAllText(TaskSavePath, JsonConvert.SerializeObject(tasks));
        }

        private void LoadQuizQuestions()
        {
            quizQuestions = QuizData.LoadDefaultQuestions();
        }

        private void StartQuiz(object sender, RoutedEventArgs e)
        {
            // Randomly select 10 questions only
            quizMistakes.Clear();
            quizQuestions = QuizData.LoadDefaultQuestions()
                .OrderBy(q => Guid.NewGuid())
                .Take(10)
                .ToList();

            currentQuizIndex = 0;
            quizScore = 0;

            ShowNextQuestion();
            activityLog.Add("Quiz started.");
            RefreshActivityLog();
        }

        private void ShowNextQuestion()
        {
            if (currentQuizIndex >= quizQuestions.Count)
            {
                QuizQuestionText.Text = "🎉 Quiz Completed!";
                QuizAnswers.Items.Clear();
                QuizFeedback.Text = $"Score: {quizScore}/{quizQuestions.Count}";
                SaveQuizScoresToFile();
                activityLog.Add($"Quiz completed: {quizScore}/{quizQuestions.Count}");
                RefreshActivityLog();

                
                // Reset summary text
                QuizSummaryText.Text = "";

                if (quizMistakes.Any())
                {
                    var summaryBuilder = new System.Text.StringBuilder();
                    summaryBuilder.AppendLine("📘 Here's what you got wrong:\n");

                    foreach (var item in quizMistakes)
                    {
                        summaryBuilder.AppendLine($"❌ Question: {item.Question}");
                        summaryBuilder.AppendLine($"   • Your answer: {item.UserAnswer}");
                        summaryBuilder.AppendLine($"   • Correct answer: {item.CorrectAnswer}");
                        summaryBuilder.AppendLine($"   • Explanation: {item.Explanation}\n");
                    }

                    QuizSummaryText.Text = summaryBuilder.ToString();
                }
                else
                {
                    QuizSummaryText.Text = "✅ Perfect score! You answered everything correctly. Well done! 🎉";
                }

                return;
            }
            var q = quizQuestions[currentQuizIndex];
            QuizQuestionText.Text = q.Text;
            QuizAnswers.Items.Clear();
            foreach (var opt in q.Options)
                QuizAnswers.Items.Add(opt);
            QuizFeedback.Text = "";
        }

        private async void SubmitQuizAnswer(object sender, RoutedEventArgs e)
        {
            if (QuizAnswers.SelectedIndex < 0)
            {
                MessageBox.Show("Please select an answer before submitting.", "No Answer Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var q = quizQuestions[currentQuizIndex];
            bool correct = QuizAnswers.SelectedIndex == q.CorrectIndex;
            string selectedAnswer = QuizAnswers.SelectedItem.ToString();
            string correctAnswer = q.Options[q.CorrectIndex];

            string feedback;
            if (correct)
            {
                quizScore++;
                feedback = $"✅ Correct! {q.Explanation}";
            }
            else
            {
                feedback = $"❌ Incorrect. The correct answer was: '{correctAnswer}'. {q.Explanation}";

                quizMistakes.Add(new QuizMistake
                {
                    Question = q.Text,
                    UserAnswer = selectedAnswer,
                    CorrectAnswer = correctAnswer,
                    Explanation = q.Explanation
                });
            }

            QuizFeedback.Foreground = correct ? Brushes.Green : Brushes.Red;
            QuizFeedback.Text = feedback;

            AddChatBubble("🤖 " + feedback, false);
            activityLog.Add($"Quiz Q{currentQuizIndex + 1}: {(correct ? "Correct" : "Incorrect")}");
            RefreshActivityLog();

            currentQuizIndex++;

            await Task.Delay(2000); // Wait 2 seconds before showing next question
            ShowNextQuestion();
        }

        private void LoadQuizScoresFromFile()
        {
            if (File.Exists(ScoreSavePath))
            {
                try { quizScores = JsonConvert.DeserializeObject<List<QuizScore>>(File.ReadAllText(ScoreSavePath)) ?? new List<QuizScore>(); }
                catch { quizScores = new List<QuizScore>(); }
            }
        }

        private void SaveQuizScoresToFile()
        {
            File.WriteAllText(ScoreSavePath, JsonConvert.SerializeObject(quizScores));
        }

        private void RefreshActivityLog()
        {
            ActivityLogBox.Items.Clear();
            foreach (string entry in activityLog)
                ActivityLogBox.Items.Add(new ListBoxItem { Content = entry });
        }

        private void ExportLog(object sender, RoutedEventArgs e)
        {
            string filename = $"ActivityLog_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            File.WriteAllLines(filename, activityLog);
            MessageBox.Show("Exported log to " + filename);
            activityLog.Add("Log exported: " + filename);
            RefreshActivityLog();
        }

        private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private List<QuizMistake> quizMistakes = new List<QuizMistake>();

        private class QuizMistake
        {
            public string Question { get; set; }
            public string UserAnswer { get; set; }
            public string CorrectAnswer { get; set; }
            public string Explanation { get; set; }
        }
    }
}
