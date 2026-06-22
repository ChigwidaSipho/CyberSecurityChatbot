using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CyberSecurityChatbot
{
    public delegate string ResponseHandler(string input);
    public delegate void NavigationHandler(string destination);

    public class Chatbot
    {
        private readonly User _user = new User();
        private readonly Sentiment _sentiment = new Sentiment();
        private readonly Random _rand = new Random();

        public readonly ActivityLog Log = new ActivityLog();

        private string _lastTopic = "";
        private bool _hasGreeted = false;

        public event NavigationHandler NavigateRequested;

        private readonly ResponseHandler _responseHandler;

        public Chatbot()
        {
            _responseHandler = ProcessResponse;

            _sentiment.OnSentimentDetected += (s, input) =>
                Log.Add("Sentiment", s);
        }

        public string GetResponse(string input) => _responseHandler(input);

        public string UserName => _user.DisplayName;

        // ================= PIPELINE =================
        // Each handler returns null if it doesn't apply. Order matters:
        // TryHandleTopic runs before TryHandleHelp so "help with passwords"
        // resolves to the password tip, not the generic help menu.
        private List<Func<string, string>> BuildPipeline() => new List<Func<string, string>>
        {
            TryHandleName,
            TryHandleGreeting,
            TryHandleNavigation,
            TryHandleFollowUp,
            TryHandleTopic,
            TryHandleSentiment,
            TryHandleHelp
        };

        private string ProcessResponse(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "Type something so I can help.";

            string text = input.Trim().ToLowerInvariant();

            foreach (var handler in BuildPipeline())
            {
                string result = handler(text);
                if (result != null)
                    return result;
            }

            string[] fallback =
            {
                "Not sure I caught that one. I'm good with passwords, phishing, privacy, and malware — or say 'tasks' or 'quiz' if you want to dive in.",
                "Hmm, that didn't land for me. Try asking about passwords, phishing, privacy, or malware?",
                "I'm not following — but I do know my stuff on passwords, phishing, privacy, and malware. Pick one?"
            };

            return fallback[_rand.Next(fallback.Length)];
        }

        // ================= NAME =================
        private string TryHandleName(string text)
        {
            const string marker = "my name is";
            if (!text.Contains(marker)) return null;

            int idx = text.IndexOf(marker, StringComparison.Ordinal);
            string remainder = text.Substring(idx + marker.Length).Trim();

            // Take only the first name-like token, so trailing chatter like
            // "my name is anna, nice to meet you" doesn't get swallowed.
            var match = Regex.Match(remainder, @"^[a-zA-Z]+(?:[-'][a-zA-Z]+)*");
            string name = match.Success ? match.Value : "";

            if (string.IsNullOrWhiteSpace(name))
                return "Hmm, didn't quite catch the name — mind trying 'my name is Alex'?";

            _user.Name = char.ToUpper(name[0]) + name.Substring(1);
            Log.Add("Name set", _user.Name);

            string[] nameReplies =
            {
                $"{_user.Name}! Good to meet you. I'll remember that.",
                $"Got it, {_user.Name} — locked in. What's on your mind?",
                $"Nice, {_user.Name}. I'm bad with a lot of things but not names — you're set.",
                $"{_user.Name}, noted. Let's get into it — what do you want to know?"
            };

            return nameReplies[_rand.Next(nameReplies.Length)];
        }

        // ================= GREETING =================
        private static readonly Regex GreetingPattern =
            new Regex(@"\b(hi|hello|hey|howdy|yo)\b", RegexOptions.Compiled);

        private string TryHandleGreeting(string text)
        {
            if (!GreetingPattern.IsMatch(text)) return null;

            if (!_hasGreeted)
            {
                _hasGreeted = true;
                Log.Add("First greeting");

                return
                    "Hey, what's up 👋 I'm your cybersecurity sidekick — basically here so you don't get owned by a sketchy email or a 12-year-old IT guy on the phone pretending to be your bank.\n\n" +
                    "I can talk through:\n" +
                    "• Passwords (yours are probably bad, no offense)\n" +
                    "• Phishing (the emails, not the fish)\n" +
                    "• Privacy\n• Malware\n\n" +
                    "Or just say: add task | start quiz | show log";
            }

            string[] returnGreetings =
            {
                $"Back again, {_user.DisplayName}? What's up.",
                $"Hey {_user.DisplayName} — what are we dealing with this time?",
                $"Yo {_user.DisplayName}, what do you need?",
                $"{_user.DisplayName}! What's on your mind?"
            };

            return returnGreetings[_rand.Next(returnGreetings.Length)];
        }

        // ================= NAVIGATION =================
        // destination key -> (matcher words, log message, possible replies)
        // Words use whole-word matching (via ContainsAnyWord) so "task" doesn't
        // false-positive inside "multitasking", "test" inside "contest", etc.
        private readonly Dictionary<string, (string[] Words, string Log, string[] Replies)> _nav =
            new Dictionary<string, (string[], string, string[])>
            {
                ["tasks"] = (
                new[] { "task", "tasks", "to-do", "todo" },
                "Tasks opened",
                new[] { "Pulling up your tasks now.", "On it — tasks incoming.", "Let's see what you've got on your plate." }
            ),
                ["quiz"] = (
                new[] { "quiz", "test" },
                "Quiz opened",
                new[] { "Quiz time. Let's see what you've actually retained.", "Alright, let's test you.", "Loading the quiz — no pressure." }
            )
            };

        private string TryHandleNavigation(string text)
        {
            foreach (var kv in _nav)
            {
                if (ContainsAnyWord(text, kv.Value.Words))
                {
                    Log.Add(kv.Value.Log);
                    NavigateRequested?.Invoke(kv.Key);

                    var replies = kv.Value.Replies;
                    return replies[_rand.Next(replies.Length)];
                }
            }

            if (ContainsWordOrPlural(text, "log"))
            {
                Log.Add("Log opened");
                NavigateRequested?.Invoke("log");
                return Log.FormatRecent(10);
            }

            return null;
        }

        // ================= FOLLOW-UP =================
        private string TryHandleFollowUp(string text)
        {
            if (!(text.Contains("tell me more") ||
                  text.Contains("more info") ||
                  text.Contains("explain more") ||
                  text.Contains("go deeper")))
                return null;

            if (string.IsNullOrEmpty(_lastTopic))
                return "Tell me a topic first (passwords, phishing, privacy, malware).";

            Log.Add("Follow-up", _lastTopic);
            return GetMore(_lastTopic);
        }

        // ================= TOPICS =================
        private readonly Dictionary<string, List<string>> _topics =
            new Dictionary<string, List<string>>
            {
                ["password"] = new List<string>
            {
                "Here's the thing about passwords: most get cracked through 'credential stuffing,' not genius hacking. Attackers grab leaked password lists from old breaches and just try them everywhere — so if you reuse one password, one leaked site means every account using it is exposed too. Use a unique password per account (a manager handles the remembering) and you cut that risk to almost zero.",
                "Length matters more than people think. 'P@ssw0rd!' looks complex but it's actually weak — it's a known pattern attackers' tools already check for. A long random phrase like 'horse-battery-staple-lamp' is exponentially harder to brute-force, just because of sheer character count, and it's way easier for you to actually remember.",
                "A password manager isn't overkill, it's the baseline now. It generates a unique, random password for every site and autofills it, so you never have to remember or reuse one. The only password you actually need to memorize is the one master password protecting the vault."
            },
                ["phishing"] = new List<string>
            {
                "Phishing works by manufacturing urgency — 'your account will be suspended in 24 hours,' 'unusual login detected, verify now.' That pressure is the tell: real companies rarely demand instant action by email. When you feel rushed, that's exactly when to slow down and check things properly.",
                "The actual link is what matters, not the link text. An email can display 'yourbank.com' while the real underlying URL goes somewhere completely different. On desktop, hover over the link (don't click) to see the real destination in the corner of your screen before deciding.",
                "Look-alike domains are the most common trick — 'paypa1.com' instead of 'paypal.com,' or a long subdomain trying to bury the real domain ('paypal.com.secure-verify.net' — the real domain there is secure-verify.net, not paypal). Always check what comes right before the final '.com/.net/etc.'"
            },
                ["privacy"] = new List<string>
            {
                "Two-factor authentication (2FA) means even if someone steals your password, they still need a second thing — usually a code from your phone — to get in. It's the single biggest upgrade you can make to any account, and it takes about 30 seconds to turn on in most account settings.",
                "App permissions are worth actually checking. A flashlight app asking for your contacts or location has no functional reason to need that — it's collecting data it doesn't need for the job it does. Go through your phone's app permissions every few months and revoke anything that doesn't make sense.",
                "Public Wi-Fi (cafes, airports) is unencrypted by default, meaning anyone else on that network can potentially see your traffic with the right tools. It's fine for casual browsing, but skip banking or anything sensitive unless you're on a VPN, which encrypts your connection end to end."
            },
                ["malware"] = new List<string>
            {
                "Most malware doesn't 'hack' its way in — it gets invited in. Cracked software, fake 'free' versions of paid apps, and sketchy browser extensions are the most common delivery methods, because they trick you into installing the payload yourself.",
                "Software updates aren't just new features — they patch known security holes. Once a vulnerability is public, attackers actively scan the internet for devices that haven't patched it yet. Staying updated closes that window before it gets exploited.",
                "Ransomware specifically encrypts your files and demands payment to unlock them — and paying doesn't guarantee you get them back. The real defense is backups: if your files are backed up somewhere disconnected (cloud or offline drive), ransomware loses its leverage entirely."
            }
            };

        // Extra trigger words/phrases that should map to a topic even though
        // they're not the topic's literal name or a simple plural of it.
        private readonly Dictionary<string, string[]> _topicAliases = new Dictionary<string, string[]>
        {
            ["password"] = new[] { "pwd", "login info", "passcode" },
            ["phishing"] = new[] { "phished", "phish", "scam email", "fake email" },
            ["privacy"] = new[] { "2fa", "two factor", "two-factor", "mfa" },
            ["malware"] = new[] { "virus", "ransomware", "spyware", "trojan" }
        };

        private string TryHandleTopic(string text)
        {
            foreach (var topic in _topics.Keys)
            {
                bool isDirectMatch = ContainsWordOrPlural(text, topic);
                bool isAliasMatch = _topicAliases.TryGetValue(topic, out var aliases)
                    && aliases.Any(alias => text.Contains(alias));

                if (isDirectMatch || isAliasMatch)
                {
                    _lastTopic = topic;
                    Log.Add("Topic", topic);

                    var list = _topics[topic];
                    return list[_rand.Next(list.Count)];
                }
            }

            return null;
        }

        private string GetMore(string topic)
        {
            switch (topic)
            {
                case "password":
                    return "Concretely: aim for 12+ characters, mix in numbers and symbols, but prioritize length over cleverness. Never reuse a password across two sites — if one site gets breached, that password gets tried everywhere else (this is called credential stuffing, and it's the #1 way accounts get taken over). Use a password manager like Bitwarden (free) or 1Password to generate and store unique ones, and turn on 2FA as a backup in case a password ever does leak.";
                case "phishing":
                    return "Concretely: check the sender's actual email address, not just the display name — 'Amazon Support' can be sent from any address. Hover over links to preview the real URL before clicking. Be suspicious of urgency ('act now,' 'account suspended'), unexpected attachments, and requests for passwords or payment info via email or text — legitimate companies don't ask for that through those channels. When in doubt, go directly to the site by typing the URL yourself instead of clicking the link.";
                case "privacy":
                    return "Concretely: turn on 2FA on every account that offers it — email, banking, social media, all of it. Go through your phone's app permissions every few months and revoke anything unnecessary (does a calculator app really need your location?). On public Wi-Fi, avoid logging into anything sensitive, or use a VPN if you have to. And check your social media privacy settings — a lot of personal info used in scams gets pulled straight from public profiles.";
                case "malware":
                    return "Concretely: only install software from official sources — app stores, or the developer's actual website, not third-party download sites. Keep your OS and apps updated, since most malware exploits known, already-patched vulnerabilities that people just haven't updated yet. Run a reputable antivirus in the background, and keep backups of important files somewhere disconnected from your main device — that's your real safety net against ransomware specifically.";
                default:
                    return "Don't actually have more on that one — try asking about passwords, phishing, privacy, or malware instead.";
            }
        }

        // ================= HELP =================
        // Catches bare "I need help" / "can you help" with no specific topic
        // attached. Runs after TryHandleTopic so "help with passwords" still
        // resolves to the password tip instead of this generic menu.
        // Note: "confused" and "lost" are intentionally NOT included here —
        // Sentiment.cs already treats those as emotional cues and gives a
        // warmer, more specific reply. Catching them here would steal that.
        private static readonly Regex HelpPattern =
            new Regex(@"\b(help|stuck|assist)\b", RegexOptions.Compiled);

        private string TryHandleHelp(string text)
        {
            if (!HelpPattern.IsMatch(text)) return null;

            Log.Add("Help requested");

            string[] helpReplies =
            {
                "Sure thing — I can talk passwords, phishing, privacy, or malware. Or say 'tasks' to manage your security to-dos, or 'quiz' to test yourself. What sounds good?",
                "Happy to help. Pick a lane: passwords, phishing, privacy, malware — or just say 'tasks' or 'quiz'.",
                "I've got you. Ask me about passwords, phishing, privacy, or malware, or say 'tasks'/'quiz' to jump into those."
            };

            return helpReplies[_rand.Next(helpReplies.Length)];
        }

        // ================= SENTIMENT =================
        // Skips sentiment if a known topic word is present, so e.g. "I'm
        // worried about phishing" still routes to the phishing topic rather
        // than getting swallowed by a generic sentiment reply.
        private string TryHandleSentiment(string text)
        {
            if (_topics.Keys.Any(t => ContainsWordOrPlural(text, t)))
                return null;

            string sentiment = _sentiment.GetSentiment(text);
            if (string.IsNullOrEmpty(sentiment))
                return null;

            Log.Add("Sentiment");
            return sentiment;
        }

        // ================= HELPERS =================
        // Whole-word matching avoids false positives like "task" inside
        // "multitasking", "test" inside "contest", "log" inside "catalog".
        private static bool ContainsWord(string text, string word)
        {
            return Regex.IsMatch(text, $@"\b{Regex.Escape(word)}\b");
        }

        private static bool ContainsAnyWord(string text, IEnumerable<string> words)
        {
            return words.Any(w => ContainsWord(text, w));
        }

        // Matches the word itself or a simple plural (password/passwords,
        // log/logs) without opening up false positives elsewhere.
        private static bool ContainsWordOrPlural(string text, string word)
        {
            return Regex.IsMatch(text, $@"\b{Regex.Escape(word)}(e?s)?\b");
        }
    }
}