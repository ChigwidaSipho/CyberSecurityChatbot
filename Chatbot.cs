using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech.Synthesis;

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

        public string GetResponse(string input)
        {
            input = input.ToLower();

            if (input.Contains("password"))
            {
                return "Make sure your password is strong and not easy to guess!";
            }
            else if (input.Contains("phishing"))
            {
                return "Be careful of suspicious emails asking for personal info.";
            }
            else if (input.Contains("safe browsing"))
            {
                return "Always check if a website is secure (https).";
            }
            else
            {
                return "I’m not sure about that, but stay safe online!";
            }

        }
        // A method to speak a greeting using text-to-speech
        public void SpeakGreeting(string userName)
        {
            SpeechSynthesizer synth = new SpeechSynthesizer();

            synth.Volume = 100; // 0 - 100
            synth.Rate = 0;     // speed (-10 to 10)

            synth.Speak($"Hello {userName}, welcome to the Cybersecurity Awareness Bot.");
        }
    }
}
