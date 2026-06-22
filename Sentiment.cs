using System;

namespace CyberSecurityChatbot
{
    public delegate string SentimentResponseHandler(string sentiment, string input);
    public delegate void SentimentDetectedEventHandler(string sentiment, string input);

    public class Sentiment
    {
        public event SentimentDetectedEventHandler OnSentimentDetected;

        public string GetSentiment(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            input = input.ToLower();

            if (Contains(input, "worried", "scared", "anxious", "nervous", "afraid"))
                return Handle("worried", input,
                    "It sounds like you're worried. Take it step by step — you're safe here. 💙");

            if (Contains(input, "confused", "don't understand", "dont understand", "lost", "unclear"))
                return Handle("confused", input,
                    "No stress — I’ll break it down simply for you. 🧩");

            if (Contains(input, "frustrated", "angry", "annoyed", "upset", "mad"))
                return Handle("frustrated", input,
                    "I get it — this can be frustrating. Let’s fix it together. 💪");

            if (Contains(input, "curious", "interesting", "intrigued", "tell me"))
                return Handle("curious", input,
                    "Good curiosity 😎 That’s how people stay safe online.");

            if (Contains(input, "happy", "great", "awesome", "good", "excited"))
                return Handle("happy", input,
                    "Nice 👍 Staying informed is the best protection online.");

            return string.Empty;
        }

        private string Handle(string sentiment, string input, string response)
        {
            OnSentimentDetected?.Invoke(sentiment, input);
            return response;
        }

        private static bool Contains(string input, params string[] words)
        {
            foreach (var w in words)
                if (input.Contains(w))
                    return true;
            return false;
        }
    }
}