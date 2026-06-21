using System;

namespace CyberSecurityChatbot
{
    /// <summary>
    /// Represents the current chatbot user.
    /// Stores identity and preference data used across all features.
    /// </summary>
    public class User
    {
        public string Name { get; set; }
        public string Interest { get; set; }   // e.g. "privacy", "phishing"
        public DateTime SessionStart { get; set; }

        public User()
        {
            SessionStart = DateTime.Now;
        }

        public User(string name) : this()
        {
            Name = name;
        }

        /// <summary>Returns a display-friendly greeting token.</summary>
        public string DisplayName => string.IsNullOrWhiteSpace(Name) ? "there" : Name;

        /// <summary>True once the user has provided their name.</summary>
        public bool IsIdentified => !string.IsNullOrWhiteSpace(Name);
    }
}