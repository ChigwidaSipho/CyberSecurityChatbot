using System;

namespace CyberSecurityChatbot
{
    // Delegate for building a sentiment response string
    public delegate string SentimentResponseHandler(string sentiment, string input);

    // Void delegate used for the event (logging, UI updates, etc.)
    public delegate void SentimentDetectedEventHandler(string sentiment, string input);

    /// <summary>
    /// Detects emotional tone in user input and returns an empathetic response.
    /// Fires OnSentimentDetected so callers can log or react without coupling.
    /// </summary>
    public class Sentiment
    {
        // ===== EVENT =====
        public event SentimentDetectedEventHandler OnSentimentDetected;

        // ===== PUBLIC ENTRY POINT =====
        /// <summary>
        /// Returns a non-empty empathetic response string if a sentiment is found,
        /// otherwise returns an empty string so the caller falls through to topic logic.
        /// </summary>
        public string GetSentiment(string input)
        {
            input = input.ToLower();

            // --- Negative / anxious ---
            if (ContainsAny(input, "worried", "scared", "anxious", "nervous", "afraid"))
                return ProcessSentiment("worried", input,
                    (s, _) => "It sounds like you're feeling worried. Don't stress — I'm here to help you stay safe online, step by step. 💙");

            // --- Confused ---
            if (ContainsAny(input, "confused", "don't understand", "dont understand", "lost", "unclear"))
                return ProcessSentiment("confused", input,
                    (s, _) => "No worries — cybersecurity can feel overwhelming at first. Let me break it down for you. 🧩");

            // --- Frustrated / angry ---
            if (ContainsAny(input, "frustrated", "angry", "annoyed", "upset", "mad"))
                return ProcessSentiment("frustrated", input,
                    (s, _) => "I hear you — it can be really frustrating dealing with online threats. Let's tackle this together. 💪");

            // --- Curious / interested ---
            if (ContainsAny(input, "curious", "interesting", "intrigued", "want to know", "tell me"))
                return ProcessSentiment("curious", input,
                    (s, _) => "Love the curiosity! 😎 Learning about cybersecurity is one of the best things you can do to stay safe.");

            // --- Happy / positive ---
            if (ContainsAny(input, "happy", "great", "awesome", "excited", "good"))
                return ProcessSentiment("happy", input,
                    (s, _) => "That's the spirit! 🎉 Staying positive and informed is the best defence online.");

            return string.Empty;
        }

        // ===== PRIVATE HELPERS =====

        private string ProcessSentiment(string sentiment, string input, SentimentResponseHandler handler)
        {
            // Fire the event for any subscribers (activity log, UI, etc.)
            OnSentimentDetected?.Invoke(sentiment, input);
            return handler(sentiment, input);
        }

        private static bool ContainsAny(string input, params string[] words)
        {
            foreach (var word in words)
                if (input.Contains(word)) return true;
            return false;
        }
    }
}