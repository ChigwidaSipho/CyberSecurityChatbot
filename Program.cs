using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyberSecurityChatbot
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //A simple console application to demonstrate a cybersecurity chatbot

            Chatbot bot = new Chatbot();

            bot.GreetUser();

            Console.Write("Enter your name: ");
            string name = Console.ReadLine();

            User user = new User(name);
            Chatbot.PlayGreetingSound("ChatBotGreeting.wav");

            Console.WriteLine($"Hello, {user.Name}! Ask me anything about cybersecurity.");

            while (true)
            {
                Console.Write("> ");
                string input = Console.ReadLine();

                if (input.ToLower() == "exit")
                {
                    Console.WriteLine("Goodbye!");
                    break;
                }

                string response = bot.GetResponse(input);
                Console.WriteLine(response);


            }
        }
    }
}
