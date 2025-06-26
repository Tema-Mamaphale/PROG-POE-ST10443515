using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POEChatbot
{
    public static class QuizData
    {
        public static List<QuizQuestion> LoadDefaultQuestions()
        {
            return new List<QuizQuestion>
        {
            new QuizQuestion
            {
                Text = "What does 'phishing' refer to?",
                Options = new List<string>
                {
                    "Fishing for data packets",
                    "Tricking users into giving information",
                    "Scanning network ports",
                    "Installing antivirus"
                },
                CorrectIndex = 1,
                Explanation = "Phishing tricks users into revealing personal info like passwords or credit card numbers by pretending to be a trustworthy entity."
            },
            new QuizQuestion
            {
                Text = "What is 2FA used for?",
                Options = new List<string>
                {
                    "Encrypting emails",
                    "Enhancing login security",
                    "Virus scanning",
                    "Data compression"
                },
                CorrectIndex = 1,
                Explanation = "Two-Factor Authentication (2FA) provides an extra layer of login security, combining something you know (password) with something you have (e.g., a code)."
            },
            new QuizQuestion
            {
                Text = "What’s a strong password?",
                Options = new List<string>
                {
                    "123456",
                    "Your birth date",
                    "A mix of characters and symbols",
                    "Your pet’s name"
                },
                CorrectIndex = 2,
                Explanation = "Strong passwords include uppercase, lowercase, numbers, and symbols — making them harder to guess or brute-force."
            },
            new QuizQuestion
            {
                Text = "Why should you avoid public Wi-Fi without a VPN?",
                Options = new List<string>
                {
                    "Because it’s slow",
                    "Because it uses a lot of data",
                    "Because hackers can intercept your data",
                    "Because it blocks streaming services"
                },
                CorrectIndex = 2,
                Explanation = "Public Wi-Fi is often unsecured, allowing attackers to intercept sensitive data unless you use a VPN to encrypt it."
            },
            new QuizQuestion
            {
                Text = "Which of these is an example of malware?",
                Options = new List<string>
                {
                    "Firewall",
                    "Antivirus",
                    "Keylogger",
                    "Encryption software"
                },
                CorrectIndex = 2,
                Explanation = "A keylogger is malware that records your keystrokes to steal personal data like passwords or credit card numbers."
            },
            new QuizQuestion
            {
                Text = "What is ransomware?",
                Options = new List<string>
                {
                    "A free antivirus tool",
                    "Software that locks data and demands payment",
                    "Software that boosts Wi-Fi speed",
                    "A tool for managing passwords"
                },
                CorrectIndex = 1,
                Explanation = "Ransomware locks your files and demands payment (a ransom) to restore access — often with no guarantee of return."
            },
            new QuizQuestion
            {
                Text = "What should you check before clicking a link in an email?",
                Options = new List<string>
                {
                    "If the link is short",
                    "The sender’s name",
                    "If it says 'free'",
                    "The actual URL behind the link"
                },
                CorrectIndex = 3,
                Explanation = "Always hover over links to see the actual URL — phishing emails often hide malicious links behind trusted-looking text."
            },
            new QuizQuestion
            {
                Text = "What does a firewall do?",
                Options = new List<string>
                {
                    "Cleans your hard drive",
                    "Protects against phishing",
                    "Blocks unauthorized network traffic",
                    "Speeds up internet"
                },
                CorrectIndex = 2,
                Explanation = "A firewall monitors and filters incoming and outgoing network traffic, acting as a barrier to malicious traffic."
            },
            new QuizQuestion
            {
                Text = "Why should you use a password manager?",
                Options = new List<string>
                {
                    "To store all your passwords securely",
                    "To create and remember one easy password",
                    "To automatically change passwords every day",
                    "To block popups"
                },
                CorrectIndex = 0,
                Explanation = "Password managers store strong, unique passwords for each site and help prevent password reuse — improving security."
            },
            new QuizQuestion
            {
                Text = "What’s the main danger of reusing the same password across multiple sites?",
                Options = new List<string>
                {
                    "It makes login easier",
                    "It can cause syncing issues",
                    "If one site is hacked, all accounts are at risk",
                    "It uses more storage"
                },
                CorrectIndex = 2,
                Explanation = "If one site is compromised, attackers can try the same password on other services (credential stuffing)."
            }
        };
        }
    }
}
