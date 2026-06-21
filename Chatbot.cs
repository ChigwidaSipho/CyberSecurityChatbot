using System;
using System.Collections.Generic;

namespace CyberSecurityChatbot
{
    // ===== DELEGATES =====
    public delegate string ResponseHandler(string input);
    public delegate void NavigationHandler(string destination);

    /// <summary>
    /// Core chatbot engine — Part 1 + 2 + 3 logic unified.
    /// Handles keyword matching, random responses, sentiment, memory, conversation flow,
    /// activity logging, and NLP-triggered navigation to Tasks / Quiz / Log tabs.
    /// </summary>
    public class Chatbot
    {
        // ===== DEPENDENCIES =====
        private readonly User _user = new User();
        private readonly Sentiment _sentiment = new Sentiment();
        private readonly Random _rand = new Random();

        /// <summary>Shared activity log — all features write here.</summary>
        public readonly ActivityLog Log = new ActivityLog();

        // Last cybersecurity topic discussed (enables follow-up handling)
        private string _lastTopic = string.Empty;

        // ===== DELEGATES + EVENT =====
        private readonly ResponseHandler _responseHandler;

        /// <summary>
        /// Fired when the chat detects an NLP intent to navigate to another tab.
        /// MainWindow subscribes and switches the selected TabItem.
        /// </summary>
        public event NavigationHandler NavigateRequested;

        // ===== CONSTRUCTOR =====
        public Chatbot()
        {
            _responseHandler = ProcessResponse;

            // Log every detected sentiment via event
            _sentiment.OnSentimentDetected += (s, input) =>
                Log.Add("Sentiment detected", s);
        }

        // ===== PUBLIC API =====
        public string GetResponse(string input) => _responseHandler(input);

        /// <summary>Display name for personalisation across tabs.</summary>
        public string UserName => _user.DisplayName;

        /// <summary>True once the user has provided their name.</summary>
        public bool UserIsIdentified => _user.IsIdentified;

        // ===== MAIN RESPONSE LOGIC =====
        private string ProcessResponse(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "Please type a message — I'm listening! 👂";

            string lower = input.ToLower().Trim();

            // ── NAME ──────────────────────────────────────────────────────────
            if (lower.Contains("my name is"))
            {
                string name = input.ToLower().Replace("my name is", "").Trim();
                if (name.Length > 0)
                    _user.Name = char.ToUpper(name[0]) + name.Substring(1);
                Log.Add("User identified", _user.Name);
                return $"Nice to meet you, {_user.Name}! 👋 Ask me anything about cybersecurity.";
            }

            // ── GREETINGS ─────────────────────────────────────────────────────
            if (ContainsAny(lower, "hello", "hi", "hey", "howzit", "good morning", "good day"))
            {
                Log.Add("Greeting");
                return $"Hey, {_user.DisplayName}! 👋 What can I help you with today?\n\n" +
                       "• Passwords  • Phishing  • Safe Browsing\n" +
                       "• Privacy    • Malware   • VPN  • 2FA\n\n" +
                       "Or try: \"Add task\" / \"Start quiz\" / \"Show activity log\"";
            }

            // ── NLP: NAVIGATION TO TASKS TAB ──────────────────────────────────
            if (ContainsAny(lower, "add task", "new task", "create task", "my tasks", "view tasks",
                                   "open tasks", "go to tasks", "show tasks", "task list",
                                   "remind me", "set reminder", "add a reminder"))
            {
                Log.Add("NLP navigation", "Tasks tab");
                NavigateRequested?.Invoke("tasks");
                return $"Opening the Tasks tab for you, {_user.DisplayName}! 📋\n\n" +
                       "You can add cybersecurity tasks like:\n" +
                       "• Enable two-factor authentication\n" +
                       "• Review privacy settings\n" +
                       "• Update passwords\n\n" +
                       "Set a reminder date so you don't forget! ✅";
            }

            // ── NLP: NAVIGATION TO QUIZ TAB ───────────────────────────────────
            if (ContainsAny(lower, "start quiz", "take quiz", "play quiz", "quiz me",
                                   "test my knowledge", "open quiz", "go to quiz",
                                   "cybersecurity quiz", "trivia"))
            {
                Log.Add("NLP navigation", "Quiz tab");
                NavigateRequested?.Invoke("quiz");
                return $"Let's test your cybersecurity knowledge, {_user.DisplayName}! 🎮\n\n" +
                       "The quiz covers:\n" +
                       "• Phishing awareness\n" +
                       "• Password security\n" +
                       "• Safe browsing\n" +
                       "• Social engineering\n\n" +
                       "Good luck! 🔐";
            }

            // ── ACTIVITY LOG REQUEST ───────────────────────────────────────────
            if (ContainsAny(lower, "activity log", "what have you done", "show log",
                                   "recent actions", "history", "show activity", "open log"))
            {
                Log.Add("Activity log viewed");
                NavigateRequested?.Invoke("log");
                return Log.FormatRecent(10);
            }

            // ── SENTIMENT ─────────────────────────────────────────────────────
            string sentimentResponse = _sentiment.GetSentiment(lower);
            if (!string.IsNullOrEmpty(sentimentResponse))
            {
                if (ContainsAny(lower, "worried", "scared", "anxious", "nervous", "afraid"))
                {
                    string tip = GetTopicResponse("phishing").Message;
                    return $"{sentimentResponse}\n\nHere's something useful:\n\n{tip}";
                }

                if (ContainsAny(lower, "confused", "don't understand", "dont understand", "lost", "unclear"))
                {
                    if (!string.IsNullOrEmpty(_lastTopic))
                        return $"{sentimentResponse}\n\n{GetDetailedInfo(_lastTopic).Tips}";
                    return $"{sentimentResponse}\n\nWhat topic is confusing you?\n• Passwords  • Phishing  • Privacy  • Scams";
                }

                if (ContainsAny(lower, "frustrated", "angry", "annoyed", "upset", "mad"))
                    return $"{sentimentResponse}\n\nWhat topic would you like help with?\n• Passwords  • Phishing  • Privacy  • Scams";

                if (ContainsAny(lower, "curious", "interesting", "intrigued"))
                    return $"{sentimentResponse}\n\nWhat would you like to explore?\n• Passwords  • Phishing  • Privacy  • Malware  • VPN  • 2FA";

                return sentimentResponse;
            }

            // ── INTEREST / MEMORY ─────────────────────────────────────────────
            if (ContainsAny(lower, "interested in", "i like", "i care about"))
            {
                foreach (var key in _topicResponses.Keys)
                {
                    if (lower.Contains(key))
                    {
                        _user.Interest = key;
                        Log.Add("Interest recorded", key);
                        return $"Got it! I'll remember that you're interested in {key}. 🧠\n\n" +
                               GetTopicResponse(key).Message;
                    }
                }
            }

            // ── FOLLOW-UP ─────────────────────────────────────────────────────
            if (ContainsAny(lower, "tell me more", "explain more", "more info",
                                   "another tip", "give me more", "go on", "expand"))
            {
                if (!string.IsNullOrEmpty(_lastTopic))
                {
                    Log.Add("Follow-up requested", _lastTopic);
                    return GetDetailedInfo(_lastTopic).Tips;
                }
                return "Sure! Which topic would you like more on?\n• Passwords  • Phishing  • Safe Browsing  • Privacy";
            }

            // ── HELP ──────────────────────────────────────────────────────────
            if (ContainsAny(lower, "help", "what can you", "menu", "options"))
            {
                Log.Add("Help menu viewed");
                return "Here's everything I can help you with:\n\n" +
                       "🔑 Password safety\n🎣 Phishing & scams\n🌐 Safe browsing\n" +
                       "🔒 Privacy protection\n🦠 Malware awareness\n🌍 VPN\n🔐 2FA\n\n" +
                       "📋 \"Add task\" → go to Tasks tab\n" +
                       "🎮 \"Start quiz\" → go to Quiz tab\n" +
                       "📜 \"Show activity log\" → go to Log tab";
            }

            // ── KEYWORD MATCHING ──────────────────────────────────────────────
            foreach (var key in _topicResponses.Keys)
            {
                if (lower.Contains(key))
                {
                    _lastTopic = key;
                    Log.Add("Topic discussed", key);

                    // Personalise if interest matches
                    var model = GetTopicResponse(key);
                    if (!string.IsNullOrEmpty(_user.Interest) && _user.Interest == key)
                        return $"As someone interested in {key}, here's a useful tip:\n\n{model.Message}";

                    return model.Message;
                }
            }

            // ── SAFE BROWSING (multi-word key) ────────────────────────────────
            if (lower.Contains("safe browsing") || lower.Contains("browsing") || lower.Contains("browser"))
            {
                _lastTopic = "safe browsing";
                Log.Add("Topic discussed", "safe browsing");
                string[] responses =
                {
                    "🌐 Always check for HTTPS and the padlock icon before entering personal info.",
                    "🌐 Avoid downloading software from unknown websites — use official sources only.",
                    "🌐 Keep your browser and extensions updated. Outdated browsers have exploitable vulnerabilities."
                };
                return responses[_rand.Next(responses.Length)];
            }

            // ── THANK YOU ─────────────────────────────────────────────────────
            if (ContainsAny(lower, "thank", "thanks", "appreciate", "cheers"))
            {
                string[] thanks =
                {
                    "You're welcome! Stay safe out there. 🛡",
                    "Happy to help! Remember — cybersecurity is everyone's responsibility.",
                    "Anytime! Keep your accounts locked down tight. 🔒"
                };
                return thanks[_rand.Next(thanks.Length)];
            }

            // ── GOODBYE ───────────────────────────────────────────────────────
            if (ContainsAny(lower, "bye", "goodbye", "exit", "quit", "see you"))
            {
                Log.Add("Session ended");
                return $"Stay safe online, {_user.DisplayName}! 👋 Come back anytime.";
            }

            // ── DEFAULT (NLP fallback — varied so it's less annoying) ─────────
            string[] fallback =
            {
                "I didn't quite catch that. Try asking about:\n• Passwords  • Phishing  • Safe browsing  • Privacy\n\nOr say \"Add task\" / \"Start quiz\"",
                "I specialise in cybersecurity. Ask me about passwords, scams, VPN, or 2FA!",
                "Not sure about that one — try rephrasing, or pick a topic: passwords, phishing, privacy, or safe browsing."
            };
            return fallback[_rand.Next(fallback.Length)];
        }

        // ===== TOPIC DICTIONARY ==============================================
        private readonly Dictionary<string, List<string>> _topicResponses = new Dictionary<string, List<string>>
        {
            {
                "password", new List<string>
                {
                    "🔑 Strong passwords use uppercase, lowercase, numbers, and symbols.\nExample: Cyber@2026!\n\nNever reuse passwords across accounts.",
                    "🔑 Try a passphrase: random words like Blue$Coffee*Moon2026\n\nLong, memorable, and hard to crack.",
                    "🔑 Use a password manager like Bitwarden or 1Password.\n\nNever write passwords on sticky notes or store them in plain text!"
                }
            },
            {
                "phishing", new List<string>
                {
                    "🎣 Phishing is when attackers impersonate trusted companies to steal your info.\n\nAlways check the sender's email address before clicking anything.",
                    "🎣 Watch for urgent language: 'Your account will be closed!'\n\nScammers use panic to bypass your judgement. Legitimate companies don't ask for passwords via email.",
                    "🎣 Hover over links before clicking to see the real URL.\n\nIf the domain looks off (e.g. paypa1.com), don't click — it's a scam."
                }
            },
            {
                "scam", new List<string>
                {
                    "🚨 If it sounds too good to be true, it is.\n\nNever send money or personal info to unverified sources.",
                    "🚨 Romance scams are rising.\n\nBe cautious of online relationships that quickly ask for financial help.",
                    "🚨 Tech support scams use alarming pop-ups.\n\nMicrosoft and Apple will NEVER call you unsolicited about your device."
                }
            },
            {
                "privacy", new List<string>
                {
                    "🔒 Review app permissions regularly. Only allow camera/contacts/location when genuinely needed.",
                    "🔒 Use a VPN on public Wi-Fi to encrypt your traffic.",
                    "🔒 Enable two-factor authentication (2FA) on all important accounts — email, banking, social media."
                }
            },
            {
                "malware", new List<string>
                {
                    "🦠 Never download software from unknown websites.\n\nAlways use official sources or verified app stores.",
                    "🦠 Keep your operating system updated.\n\nUpdates patch vulnerabilities that malware actively exploits.",
                    "🦠 Ransomware can encrypt all your files.\n\nBack up data regularly to offline or cloud storage."
                }
            },
            {
                "vpn", new List<string>
                {
                    "🌍 A VPN encrypts your internet traffic and hides your IP address.\n\nRecommended: ProtonVPN, Mullvad, NordVPN.",
                    "🌍 Not all VPNs are trustworthy.\n\nAvoid free VPNs — they often sell your data to advertisers."
                }
            },
            {
                "2fa", new List<string>
                {
                    "🔐 Two-factor authentication adds a critical second layer of security.\n\nEven if your password leaks, attackers can't get in without your second factor.",
                    "🔐 Use an authenticator app like Google Authenticator or Authy.\n\nSMS-based 2FA can be intercepted via SIM swapping — an app is much safer."
                }
            }
        };

        // ===== HELPERS =======================================================

        private ChatbotModel GetTopicResponse(string topic)
        {
            foreach (var key in _topicResponses.Keys)
            {
                if (topic.Contains(key))
                {
                    _lastTopic = key;
                    var list = _topicResponses[key];
                    return new ChatbotModel { Topic = key, Message = list[_rand.Next(list.Count)] };
                }
            }
            return new ChatbotModel { Message = "I didn't catch that topic. Try asking about passwords, phishing, or privacy." };
        }

        private ChatbotModel GetDetailedInfo(string topic)
        {
            switch (topic)
            {
                case "password":
                    return ChatbotModel.ForTopic(topic, null,
                        "🔑 More on passwords:\n\n• Minimum 12 characters\n• Mix letters, numbers, symbols\n• Use a password manager\n• Enable 2FA everywhere\n• Change passwords after any breach");
                case "phishing":
                    return ChatbotModel.ForTopic(topic, null,
                        "🎣 More on phishing:\n\n• Check sender email addresses carefully\n• Don't click links — go to the site directly\n• Report phishing emails to your provider\n• Use email filters and spam detection");
                case "scam":
                    return ChatbotModel.ForTopic(topic, null,
                        "🚨 More on scams:\n\n• Never send money to unverified contacts\n• Verify identities via official channels\n• Be suspicious of unsolicited contact\n• Report scams to the SAPS Cybercrime Unit");
                case "privacy":
                    return ChatbotModel.ForTopic(topic, null,
                        "🔒 More on privacy:\n\n• Audit app permissions monthly\n• Use encrypted messaging (Signal)\n• Opt out of data tracking where possible\n• Use a VPN on public networks");
                case "malware":
                    return ChatbotModel.ForTopic(topic, null,
                        "🦠 More on malware:\n\n• Keep OS and software updated\n• Use reputable antivirus software\n• Never open attachments from unknown senders\n• Back up data regularly");
                case "safe browsing":
                    return ChatbotModel.ForTopic(topic, null,
                        "🌐 More on safe browsing:\n\n• Use a privacy-focused browser (Firefox, Brave)\n• Install uBlock Origin to block malicious ads\n• Clear cookies and cache regularly\n• Avoid public Wi-Fi without a VPN");
                default:
                    return ChatbotModel.ForTopic(topic, null,
                        $"Let's go deeper into {topic}. Always double-check sources and stay alert online!");
            }
        }

        private static bool ContainsAny(string input, params string[] words)
        {
            foreach (var w in words)
                if (input.Contains(w)) return true;
            return false;
        }
    }
}
