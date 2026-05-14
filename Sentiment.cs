using System;

namespace CyberSecurityChatbot
{
    // Delegate for sentiment responses (returns string)
    public delegate string SentimentResponseHandler(string sentiment, string input);

    // Separate delegate for the event (must return void)
    public delegate void SentimentDetectedEventHandler(string sentiment, string input);

    internal class Sentiment
    {
        // ===== EVENT (void delegate) =====
        public event SentimentDetectedEventHandler OnSentimentDetected;

        // ===== DETECT SENTIMENT =====
        public string GetSentiment(string input)
        {
            input = input.ToLower();

            if (input.Contains("worried") || input.Contains("scared"))
            {
                return ProcessSentiment(
                    "worried",
                    input,
                    (sentiment, userInput) =>
                    $"It sounds like you're {sentiment}. Don't worry — I'll help you stay safe online step by step."
                );
            }
            else if (input.Contains("confused"))
            {
                return ProcessSentiment(
                    "confused",
                    input,
                    (sentiment, userInput) =>
                    $"I can see you're {sentiment}. Let me explain it more simply for you."
                );
            }
            else if (input.Contains("curious"))
            {
                return ProcessSentiment(
                    "curious",
                    input,
                    (sentiment, userInput) =>
                    $"I like your curiosity 😎 Learning about cybersecurity is a smart move."
                );
            }

            return "";
        }

        // ===== PROCESS SENTIMENT =====
        private string ProcessSentiment(
            string sentiment,
            string input,
            SentimentResponseHandler handler)
        {
            // Fire the event (for logging, UI updates, etc.)
            OnSentimentDetected?.Invoke(sentiment, input);

            // Return the delegate's response string
            return handler(sentiment, input);
        }
    }
}