using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Media;

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
