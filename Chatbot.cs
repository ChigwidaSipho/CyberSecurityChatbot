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

            // GREETING
            if (input.Contains("how are you"))
            {
                string[] responses =
                {
            "I'm running smoothly! Ready to help you stay safe online and dodge cyber threats.",
            "All systems operational. I can give tips on passwords, phishing, safe browsing, and more.",
            "Feeling cyber-secure! Ask me anything about keeping your accounts and info protected."
        };
                return GetRandom(responses);
            }

            // PURPOSE
            else if (input.Contains("purpose") || input.Contains("what do you do"))
            {
                return "I’m your personal cybersecurity guide. I explain online threats, give safety tips, and help you make smarter choices online—like avoiding scams, phishing, and weak passwords.";
            }

            // HELP MENU
            else if (input.Contains("help") || input.Contains("what can i ask"))
            {
                return "You can ask me things like:\n- How to create strong passwords\n- How to spot phishing emails or scam messages\n- How to browse safely online\n- How to protect your personal info on websites\n- Any general questions about staying secure online";
            }

            // STRONG PASSWORD
            else if (input.Contains("create strong password") || input.Contains("strong password") || input.Contains("password"))
            {
                string[] responses =
                {
            "To create strong passwords, use uppercase, lowercase, numbers, and symbols. Example: 'Cyber@2026Secure!'. Avoid repeating passwords across sites.",
            "Never reuse passwords across multiple accounts—hackers love that. Make each password unique.",
            "Consider a passphrase: easy to remember but hard to guess, like 'Blue$Coffee*Moon2026'."
        };
                return GetRandom(responses);
            }

            // PHISHING / SCAM
            else if (input.Contains("spot phishing") || input.Contains("phishing") || input.Contains("scam") || input.Contains("scam message") || input.Contains("phishing email"))
            {
                string[] responses =
                {
            "Phishing emails or scam messages often look urgent or too good to be true. Don't click unknown links, verify senders, and double-check URLs before entering info.",
            "Never give personal info in response to suspicious emails or messages.",
            "If it pressures you or promises something amazing, it’s probably a scam."
        };
                return GetRandom(responses);
            }

            // SAFE BROWSING
            else if (input.Contains("browse safely") || input.Contains("safe browsing") || input.Contains("website") || input.Contains("website safety"))
            {
                string[] responses =
                {
            "To browse safely, always check for HTTPS and the padlock icon before entering sensitive info.",
            "Avoid downloading files or software from untrusted websites.",
            "Keep your browser updated, enable pop-up blockers, and consider using a password manager."
        };
                return GetRandom(responses);
            }

            // PERSONAL INFO PROTECTION
            else if (input.Contains("protect personal info") || input.Contains("personal information") || input.Contains("privacy"))
            {
                string[] responses =
                {
            "Protect your personal info by sharing minimally online, using two-factor authentication, strong passwords, and reviewing privacy settings on apps and websites.",
            "Think before you share: only give personal info to trusted sites and contacts.",
            "Regularly check your accounts for suspicious activity and adjust privacy settings."
        };
                return GetRandom(responses);
            }

            // GENERAL CYBERSECURITY / STAY SECURE
            else if (input.Contains("stay secure online") || input.Contains("general cybersecurity") || input.Contains("security tips"))
            {
                string[] responses =
                {
            "Stay secure online by keeping software updated, using strong passwords, spotting phishing attempts, avoiding suspicious links, and reviewing privacy settings.",
            "Think like a hacker: if it looks off, check it twice before clicking or sharing info.",
            "Use multi-factor authentication wherever possible for extra account protection."
        };
                return GetRandom(responses);
            }

            // THANK YOU
            else if (input.Contains("thank you") || input.Contains("thanks"))
            {
                string[] responses =
                {
            "You're welcome! Stay alert and safe online.",
            "No worries! Remember, strong passwords and cautious clicking are your best friends.",
            "Glad I could help! Keep learning and stay protected from cyber threats."
        };
                return GetRandom(responses);
            }

            else
            {
                string[] responses =
                {
            "Hmm… I don’t fully understand that yet. Try asking about:\n- Creating strong passwords\n- Spotting phishing or scam messages\n- Browsing safely online\n- Protecting your personal info\n- General cybersecurity tips",
            "I’m still learning, but I can give advice on passwords, phishing, safe browsing, or protecting personal info if you ask.",
            "Not sure about that one. Ask me something like 'How do I make a strong password?' or 'How do I spot phishing emails?'"
        };
                return GetRandom(responses);
            }
        }
        private static Random rnd = new Random();
        private string GetRandom(string[] responses)
        {
            return responses[rnd.Next(responses.Length)];
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
