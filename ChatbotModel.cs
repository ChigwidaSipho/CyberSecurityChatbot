using System;

namespace CyberSecurityChatbot
{
    /// <summary>
    /// Data transfer object returned by the Chatbot engine.
    /// Keeps topic, message, tips, and metadata together.
    /// </summary>
    public class ChatbotModel
    {
        // ===== AUTO-PROPERTIES =====
        public string Topic { get; set; }
        public string Message { get; set; }
        public string Tips { get; set; }
        public string SentimentLabel { get; set; }
        public DateTime Timestamp { get; set; }
                                            
        // ===== CONSTRUCTOR =====   
        public ChatbotModel()  
        {
            Timestamp = DateTime.Now;
        }

        // ===== FACTORY METHODS =====
        public static ChatbotModel ForTopic(string topic, string message, string tips = null)
            => new ChatbotModel { Topic = topic, Message = message, Tips = tips };

        public static ChatbotModel ForSentiment(string sentimentLabel, string message)
            => new ChatbotModel { SentimentLabel = sentimentLabel, Message = message };

        public static ChatbotModel ForUser(string message)
            => new ChatbotModel { Message = message };

        // ===== DISPLAY =====
        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();
            if (!string.IsNullOrEmpty(Message)) sb.AppendLine(Message);
            if (!string.IsNullOrEmpty(Tips)) sb.AppendLine($"\n💡 Tips:\n{Tips}");
            return sb.ToString().Trim();
        }
    }
}