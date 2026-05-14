using System;
using System.Collections.Generic;

namespace CyberSecurityChatbot
{
    internal class ChatbotModel
    {
        // ===== PROPERTIES =====
        public string UserName { get; set; }
        public string UserInterest { get; set; }
        public string Topic { get; set; }
        public string Message { get; set; }
        public string Tips { get; set; }
        public string Sentiment { get; set; }
        public DateTime Timestamp { get; set; }

        // ===== CONSTRUCTOR =====
        public ChatbotModel()
        {
            Timestamp = DateTime.Now;
        }

        // ===== FACTORY METHODS =====
        public static ChatbotModel CreateTopicResponse(string topic, string message, string tips)
        {
            return new ChatbotModel
            {
                Topic = topic,
                Message = message,
                Tips = tips
            };
        }

        public static ChatbotModel CreateSentimentResponse(string sentiment, string message)
        {
            return new ChatbotModel
            {
                Sentiment = sentiment,
                Message = message
            };
        }

        public static ChatbotModel CreateUserMessage(string userName, string message)
        {
            return new ChatbotModel
            {
                UserName = userName,
                Message = message
            };
        }

        // ===== DISPLAY HELPER =====
        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();

            if (!string.IsNullOrEmpty(Message))
                sb.AppendLine(Message);

            if (!string.IsNullOrEmpty(Tips))
                sb.AppendLine($"\n💡 Tips:\n{Tips}");

            return sb.ToString().Trim();
        }
    }
}