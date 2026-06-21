using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CyberSecurityChatbot
{
    /// <summary>
    /// Cybersecurity Quiz tab — 12 questions, multiple-choice and true/false.
    /// Tracks score, gives per-answer feedback, shows final result.
  
    public partial class QuizControl : UserControl
    {
        // ===== DEPENDENCIES =====
        private Chatbot _bot;

        // ===== QUIZ STATE =====
        private List<QuizQuestion> _questions;
        private int  _current = 0;
        private int  _score   = 0;
        private bool _answered = false;
          
        // ===== BRUSHES =====
        private static readonly SolidColorBrush BrushCorrect   = new SolidColorBrush(Color.FromRgb(0x00, 0x3D, 0x1F));
        private static readonly SolidColorBrush BrushWrong     = new SolidColorBrush(Color.FromRgb(0x4A, 0x00, 0x00));
        private static readonly SolidColorBrush BrushCorrectFg = new SolidColorBrush(Color.FromRgb(0x00, 0xFF, 0x88));
        private static readonly SolidColorBrush BrushWrongFg   = new SolidColorBrush(Color.FromRgb(0xFF, 0x66, 0x66));

        public QuizControl()
        {
            InitializeComponent();
            BuildQuestions();
        }

        public void SetBot(Chatbot bot) => _bot = bot;

        // ===== START =====
        public void StartQuiz()
        {
            _current  = 0;
            _score    = 0;
            _answered = false;

            // Shuffle questions for variety
            Shuffle(_questions);

            StartPanel.Visibility    = Visibility.Collapsed;
            ResultsPanel.Visibility  = Visibility.Collapsed;
            QuestionPanel.Visibility = Visibility.Visible;
            FeedbackPanel.Visibility = Visibility.Collapsed;

            _bot?.Log.Add("Quiz started", $"{_questions.Count} questions");
            UpdateScoreDisplay();
            ShowQuestion();
        }

        private void StartQuiz_Click(object sender, RoutedEventArgs e) => StartQuiz();

        // ===== SHOW QUESTION =====
        private void ShowQuestion()
        {
            if (_current >= _questions.Count)
            {
                ShowResults();
                return;
            }

            _answered = false;
            FeedbackPanel.Visibility = Visibility.Collapsed;

            var q = _questions[_current];
            QuestionText.Text = $"Q{_current + 1}. {q.Question}";

            // Progress bar
            double pct = (double)_current / _questions.Count;
            var parentWidth = QuestionPanel.ActualWidth - 80;
            ProgressBar.Width = parentWidth > 0 ? parentWidth * pct : 0;

            // Build option buttons
            OptionsPanel.Children.Clear();
            for (int i = 0; i < q.Options.Count; i++)
            {
                var btn = new Button
                {
                    Style   = (Style)FindResource("AnswerBtn"),
                    Content = q.Options[i],
                    Tag     = i
                };
                btn.Click += AnswerBtn_Click;
                OptionsPanel.Children.Add(btn);
            }

            UpdateScoreDisplay();
        }

        // ===== ANSWER =====
        private void AnswerBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_answered) return;
            _answered = true;

            var btn      = (Button)sender;
            int selected = (int)btn.Tag;
            var q        = _questions[_current];
            bool correct = selected == q.CorrectIndex;

            if (correct) _score++;

            // Colour the buttons
            foreach (Button b in OptionsPanel.Children)
            {
                int idx = (int)b.Tag;
                if (idx == q.CorrectIndex)
                {
                    b.Background = BrushCorrect;
                    b.Foreground = BrushCorrectFg;
                }
                else if (idx == selected && !correct)
                {
                    b.Background = BrushWrong;
                    b.Foreground = BrushWrongFg;
                }
                b.IsEnabled = false;
            }

            // Feedback
            FeedbackResult.Text       = correct ? "✅  Correct!" : "❌  Incorrect";
            FeedbackResult.Foreground = correct ? BrushCorrectFg : BrushWrongFg;
            FeedbackText.Text         = q.Explanation;
            FeedbackPanel.Visibility  = Visibility.Visible;

            _bot?.Log.Add(correct ? "Quiz answer correct" : "Quiz answer wrong",
                          $"Q{_current + 1}: {q.Question.Substring(0, Math.Min(40, q.Question.Length))}...");

            UpdateScoreDisplay();
        }

        // ===== NEXT =====
        private void NextQuestion_Click(object sender, RoutedEventArgs e)
        {
            _current++;
            ShowQuestion();
        }

        // ===== RESULTS =====
        private void ShowResults()
        {
            QuestionPanel.Visibility = Visibility.Collapsed;
            ResultsPanel.Visibility  = Visibility.Visible;

            int total = _questions.Count;
            double pct = (double)_score / total * 100;

            ResultScore.Text = $"{_score} / {total}";

            if (pct >= 80)
            {
                ResultEmoji.Text    = "🏆";
                ResultTitle.Text    = "CYBERSECURITY PRO!";
                ResultTitle.Foreground = new SolidColorBrush(Color.FromRgb(0x00, 0xFF, 0xFF));
                ResultMessage.Text  = "Outstanding! You have a strong understanding of cybersecurity. Keep protecting yourself and others online!";
            }
            else if (pct >= 60)
            {
                ResultEmoji.Text    = "🛡";
                ResultTitle.Text    = "GOOD EFFORT!";
                ResultTitle.Foreground = new SolidColorBrush(Color.FromRgb(0x00, 0xFF, 0x88));
                ResultMessage.Text  = "You're on the right track. Review the topics you missed and keep building your cyber awareness skills!";
            }
            else
            {
                ResultEmoji.Text    = "📚";
                ResultTitle.Text    = "KEEP LEARNING!";
                ResultTitle.Foreground = new SolidColorBrush(Color.FromRgb(0xFF, 0xAA, 0x00));
                ResultMessage.Text  = "Don't give up — cybersecurity takes practice. Head to the Chat tab to learn more about the topics you missed.";
            }

            _bot?.Log.Add("Quiz completed", $"Score: {_score}/{total} ({pct:0}%)");
        }

        // ===== HELPERS =====
        private void UpdateScoreDisplay()
        {
            ScoreText.Text    = $"{_score} / {_questions.Count}";
            ProgressText.Text = $"{_current} / {_questions.Count}";
        }

        private static void Shuffle<T>(List<T> list)
        {
            var rng = new Random();
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                var tmp = list[i]; list[i] = list[j]; list[j] = tmp;
            }
        }

        // ===== QUESTIONS (12) =====
        private void BuildQuestions()
        {
            _questions = new List<QuizQuestion>
            {
                new QuizQuestion
                {
                    Question     = "What should you do if you receive an email asking for your password?",
                    Options      = new List<string> { "A) Reply with your password", "B) Delete the email", "C) Report it as phishing", "D) Ignore it" },
                    CorrectIndex = 2,
                    Explanation  = "Reporting phishing emails helps protect you and others. Legitimate companies never ask for passwords via email."
                },
                new QuizQuestion
                {
                    Question     = "True or False: Using the same password for all accounts is safe if it's a strong password.",
                    Options      = new List<string> { "True", "False" },
                    CorrectIndex = 1,
                    Explanation  = "FALSE. If one account is breached, attackers can access ALL your accounts with the same password. Always use unique passwords."
                },
                new QuizQuestion
                {
                    Question     = "What does HTTPS in a website URL indicate?",
                    Options      = new List<string> { "A) The site is popular", "B) The connection is encrypted", "C) The site is government-owned", "D) The site loads faster" },
                    CorrectIndex = 1,
                    Explanation  = "HTTPS means the connection between your browser and the website is encrypted using TLS/SSL, protecting your data."
                },
                new QuizQuestion
                {
                    Question     = "Which of the following is the strongest password?",
                    Options      = new List<string> { "A) password123", "B) John1990", "C) Tr0ub4dor&3!", "D) qwerty" },
                    CorrectIndex = 2,
                    Explanation  = "Tr0ub4dor&3! uses uppercase, lowercase, numbers, and symbols — making it much harder to crack than dictionary words or personal info."
                },
                new QuizQuestion
                {
                    Question     = "What is two-factor authentication (2FA)?",
                    Options      = new List<string> { "A) Two passwords on one account", "B) A second verification step beyond your password", "C) A type of firewall", "D) Logging in from two devices" },
                    CorrectIndex = 1,
                    Explanation  = "2FA adds a second layer of security (like an app code or SMS) so that even if your password is stolen, attackers can't log in."
                },
                new QuizQuestion
                {
                    Question     = "True or False: Public Wi-Fi is safe to use for online banking.",
                    Options      = new List<string> { "True", "False" },
                    CorrectIndex = 1,
                    Explanation  = "FALSE. Public Wi-Fi can be intercepted by attackers. Always use a VPN or mobile data for sensitive transactions like banking."
                },
                new QuizQuestion
                {
                    Question     = "What is 'social engineering' in cybersecurity?",
                    Options      = new List<string> { "A) Building social media apps", "B) Manipulating people to reveal confidential information", "C) Encrypting social media data", "D) A type of antivirus software" },
                    CorrectIndex = 1,
                    Explanation  = "Social engineering exploits human psychology rather than technical vulnerabilities — tricking people into giving up information or access."
                },
                new QuizQuestion
                {
                    Question     = "Which action best protects you from malware?",
                    Options      = new List<string> { "A) Only using Chrome", "B) Never using email", "C) Keeping your OS and software updated", "D) Using incognito mode" },
                    CorrectIndex = 2,
                    Explanation  = "Software updates patch security vulnerabilities that malware exploits. Keeping everything updated is one of the most effective defences."
                },
                new QuizQuestion
                {
                    Question     = "What does a VPN primarily do?",
                    Options      = new List<string> { "A) Speeds up your internet", "B) Blocks all ads", "C) Encrypts your traffic and hides your IP", "D) Removes viruses" },
                    CorrectIndex = 2,
                    Explanation  = "A VPN (Virtual Private Network) encrypts your internet traffic and masks your IP address, making it much harder to track your activity."
                },
                new QuizQuestion
                {
                    Question     = "True or False: Microsoft and Apple will call you if your computer has a virus.",
                    Options      = new List<string> { "True", "False" },
                    CorrectIndex = 1,
                    Explanation  = "FALSE. This is a tech support scam. Microsoft, Apple and other companies do NOT make unsolicited calls about your computer. Hang up immediately."
                },
                new QuizQuestion
                {
                    Question     = "What is ransomware?",
                    Options      = new List<string> { "A) Software that displays ads", "B) Malware that encrypts your files and demands payment", "C) A tool to recover lost files", "D) A type of phishing email" },
                    CorrectIndex = 1,
                    Explanation  = "Ransomware encrypts your files and demands a ransom to restore them. Regular backups are your best defence against ransomware attacks."
                },
                new QuizQuestion
                {
                    Question     = "Which of the following is a sign of a phishing website?",
                    Options      = new List<string> { "A) A padlock icon in the address bar", "B) The URL is misspelled (e.g. 'paypa1.com')", "C) The site asks you to log in", "D) The site loads quickly" },
                    CorrectIndex = 1,
                    Explanation  = "Phishing sites often use misspelled or lookalike domain names (like paypa1.com instead of paypal.com). Always check the URL carefully."
                }
            };
        }
    }

    /// <summary>Data model for a single quiz question.</summary>
    public class QuizQuestion
    {
        public string       Question     { get; set; }
        public List<string> Options      { get; set; }
        public int          CorrectIndex { get; set; }
        public string       Explanation  { get; set; }
    }
}
