using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CyberSecurityChatbot
{
    //A simple chatbot class that provides responses based on user input
    internal class Chatbot
    {
        public void GreetUser()
        {
            Console.WriteLine("====================================");
            Console.WriteLine("  Welcome to the Cybersecurity Bot  ");
            Console.WriteLine("====================================");
        }

        private Random rand = new Random();

        public string GetResponse(string input)
        {
            input = input.ToLower();

            // ===== GREETINGS =====
            if (input.Contains("how are you"))
            {
                string[] responses =
                {
                "I'm running smoothly 😎 Ready to keep you safe online!",
                "All systems operational. How can I help you today?",
                "Feeling cyber-secure 💻 What do you need?"
            };
                return GetRandom(responses);
            }

            // ===== PURPOSE =====
            else if (input.Contains("purpose") || input.Contains("what do you do"))
            {
                return "I help you understand cybersecurity and avoid online threats like scams and hacking.";
            }

            // ===== HELP =====
            else if (input.Contains("help") || input.Contains("what can i ask"))
            {
                return "You can ask about passwords, phishing, safe browsing, and how to avoid online scams.";
            }

            // ===== PASSWORDS =====
            else if (input.Contains("password"))
            {
                string[] responses =
                {
                "Use strong passwords with uppercase, lowercase, numbers, and symbols 🔐",
                "Never reuse passwords across sites—hackers love that mistake.",
                "Tip: Use a passphrase like 'Cyber@2026Secure!' instead of simple words."
            };
                return GetRandom(responses);
            }

            // ===== PHISHING =====
            else if (input.Contains("phishing") || input.Contains("scam"))
            {
                string[] responses =
                {
                "Phishing is when attackers trick you into giving personal info through fake emails or links.",
                "Never click suspicious links. Always double-check the sender 👀",
                "If it feels urgent or too good to be true… it’s probably a scam."
            };
                return GetRandom(responses);
            }

            // ===== SAFE BROWSING =====
            else if (input.Contains("browsing") || input.Contains("safe browsing") || input.Contains("website"))
            {
                string[] responses =
                {
                "Always check for HTTPS 🔒 before entering sensitive info.",
                "Avoid downloading files from unknown websites.",
                "Use trusted browsers and keep them updated for better security."
            };
                return GetRandom(responses);
            }

            // ===== DEFAULT =====
            else
            {
                string[] responses =
                {
                "Hmm 🤔 I don't understand that yet.",
                "Try asking me about passwords, phishing, or safe browsing.",
                "I'm still learning! Ask me something cybersecurity-related 😄"
            };
                return GetRandom(responses);
            }
        }

        // ===== RANDOM RESPONSE HELPER =====
        private string GetRandom(string[] options)
        {
            return options[rand.Next(options.Length)];
        }

        // ===== TYPING EFFECT =====
        public void TypeEffect(string message)
        {
            foreach (char c in message)
            {
                Console.Write(c);
                Thread.Sleep(20); // speed (lower = faster)
            }
            Console.WriteLine();
        }
        public static void PlayGreetingSound(string filepath)
        {
            try
            {
                SoundPlayer greetingPlayer = new SoundPlayer(filepath);
                greetingPlayer.PlaySync(); // Ensures the sound finishes playing before moving on
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error playing sound: " + ex.Message);
            }
        }
    
     }
    
}
