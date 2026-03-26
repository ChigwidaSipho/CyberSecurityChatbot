using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CyberSecurityChatbot
{
    internal class Chatbot
    {
        public void GreetUser()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;

            Console.WriteLine("\n╔════════════════════════════════════════════╗");
            Console.WriteLine("║      CYBERSECURITY AWARENESS SYSTEM        ║");
            Console.WriteLine("╚════════════════════════════════════════════╝");

            Console.ResetColor();
        }

        private Random rand = new Random();

        public string GetResponse(string input)
        {
            input = input.ToLower();

            if (input.Contains("how are you"))
            {
                string[] responses =
                {
                    "I'm running smoothly. Ready to keep you safe online!",
                    "All systems operational. What do you need?",
                    "Feeling cyber-secure. Let’s lock things down."
                };
                return GetRandom(responses);
            }

            else if (input.Contains("purpose") || input.Contains("what do you do"))
            {
                return "I help you understand cybersecurity and avoid online threats like scams and hacking.";
            }

            else if (input.Contains("help") || input.Contains("what can i ask"))
            {
                return "Ask me about passwords, phishing, safe browsing, or online scams.";
            }

            else if (input.Contains("password"))
            {
                string[] responses =
                {
                    "Use strong passwords with uppercase, lowercase, numbers, and symbols.",
                    "Never reuse passwords across sites — that’s hacker heaven.",
                    "Try a passphrase like 'Cyber@2026Secure!' instead."
                };
                return GetRandom(responses);
            }

            else if (input.Contains("phishing") || input.Contains("scam"))
            {
                string[] responses =
                {
                    "Phishing = fake messages trying to steal your info.",
                    "Never click suspicious links. Double-check everything.",
                    "If it feels urgent or too good to be true… it’s a scam."
                };
                return GetRandom(responses);
            }

            else if (input.Contains("browsing") || input.Contains("safe browsing") || input.Contains("website"))
            {
                string[] responses =
                {
                    "Always check for HTTPS before entering sensitive info.",
                    "Avoid downloading from unknown websites.",
                    "Keep your browser updated for better security."
                };
                return GetRandom(responses);
            }
            else if (input.Contains("thank you") || input.Contains("thanks"))
            {
                string[] responses =
                {
        "You're welcome! Stay safe online!",
        "No worries! Always here to help.",
        "Glad I could help! Keep your passwords strong."
    };
                return GetRandom(responses);
            }

            else
            {
                string[] responses =
                {
                    "Hmm... I don’t understand that yet.",
                    "Try asking about passwords, phishing, or browsing.",
                    "Still learning. Hit me with cybersecurity stuff."
                };
                return GetRandom(responses);
            }
        }

        private string GetRandom(string[] options)
        {
            return options[rand.Next(options.Length)];
        }

        public void TypeEffect(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;

            foreach (char c in message)
            {
                Console.Write(c);
                Thread.Sleep(20);
            }

            Console.WriteLine();
            Console.ResetColor();
        }

        public static void PlayGreetingSound(string filepath)
        {
            try
            {
                SoundPlayer greetingPlayer = new SoundPlayer(filepath);
                greetingPlayer.PlaySync();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error playing sound: " + ex.Message);
                Console.ResetColor();
            }
        }
    }
}
