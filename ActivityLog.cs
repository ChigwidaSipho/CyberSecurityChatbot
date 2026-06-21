using System;
using System.Collections.Generic;
using System.Linq;

namespace CyberSecurityChatbot
{
    /// <summary>
    /// Shared activity log — every feature (chat, tasks, quiz, NLP) writes here.
    /// ActivityLogControl reads from this to display the log tab.
    /// </summary>
    public class ActivityLog
    {
        // ===== STORAGE =====
        private readonly List<LogEntry> _entries = new List<LogEntry>();

        // ===== PUBLIC API =====

        /// <summary>Add a log entry with an optional detail string.</summary>
        public void Add(string action, string detail = null)
        {
            _entries.Add(new LogEntry
            {
                Timestamp = DateTime.Now,
                Action    = action,
                Detail    = detail
            });
        }

        /// <summary>Returns all entries newest-first.</summary>
        public IReadOnlyList<LogEntry> GetAll()
            => _entries.AsEnumerable().Reverse().ToList();

        /// <summary>Returns the most recent N entries newest-first.</summary>
        public IReadOnlyList<LogEntry> GetRecent(int count = 10)
            => _entries.AsEnumerable().Reverse().Take(count).ToList();

        /// <summary>Formats the last N actions as a chat-friendly string.</summary>
        public string FormatRecent(int count = 10)
        {
            var recent = GetRecent(count).ToList();
            if (recent.Count == 0)
                return "📜 No activity recorded yet. Start chatting, add a task, or take the quiz!";

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"📜 Recent Activity (last {recent.Count}):\n");
            for (int i = 0; i < recent.Count; i++)
            {
                var e = recent[i];
                string detail = string.IsNullOrEmpty(e.Detail) ? "" : $" — {e.Detail}";
                sb.AppendLine($"  {i + 1}. [{e.Timestamp:HH:mm:ss}] {e.Action}{detail}");
            }
            return sb.ToString().Trim();
        }

        /// <summary>Total number of entries.</summary>
        public int Count => _entries.Count;
    }

    /// <summary>Single log entry.</summary>
    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string   Action    { get; set; }
        public string   Detail    { get; set; }

        public override string ToString()
        {
            string detail = string.IsNullOrEmpty(Detail) ? "" : $" — {Detail}";
            return $"[{Timestamp:HH:mm:ss}]  {Action}{detail}";
        }
    }
}
