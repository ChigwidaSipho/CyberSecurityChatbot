using System;
using System.Threading;

namespace CyberSecurityChatbot
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // ===== ASCII HEADER (CLEAN + SAFE COLORS) =====
            string[] ascii = {
                "  $$$$$$\\  $$\\                  $$\\     $$\\                  $$\\     ",
                " $$  __$$\\ $$ |                 $$ |    $$ |                 $$ |    ",
                " $$ /  \\__|$$$$$$$\\   $$$$$$\\ $$$$$$\\   $$$$$$$\\   $$$$$$\\ $$$$$$\\   ",
                " $$ |      $$  __$$\\  \\____$$\\\\_$$  _|  $$  __$$\\ $$  __$$\\\\_$$  _|  ",
                " $$ |      $$ |  $$ | $$$$$$$ | $$ |    $$ |  $$ |$$ /  $$ | $$ |    ",
                " $$ |  $$\\ $$ |  $$ |$$  __$$ | $$ |$$\\ $$ |  $$ |$$ |  $$ | $$ |$$\\ ",
                " \\$$$$$$  |$$ |  $$ |\\$$$$$$$ | \\$$$$  |$$$$$$$  |\\$$$$$$  | \\$$$$  |",
                "  \\______/ \\__|  \\__| \\_______|  \\____/ \\_______/  \\______/   \\____/ "
            };

            // SAFE COLORS ONLY (no black, no dark nonsense)
            ConsoleColor[] colors = {
                ConsoleColor.Cyan,
                ConsoleColor.Green,
                ConsoleColor.Yellow,
                ConsoleColor.Magenta,
                ConsoleColor.Blue,
                ConsoleColor.White
            };

            Random rand = new Random();
            ConsoleColor chosenColor = colors[rand.Next(colors.Length)];

            foreach (string line in ascii)
            {
                Console.ForegroundColor = chosenColor;
                Console.WriteLine(line);
                Thread.Sleep(50);
            }

            Console.ResetColor();

            // ===== SOUND =====
            Chatbot.PlayGreetingSound("ChatBotGreeting.wav");

            // ===== BOT INIT =====
            Chatbot bot = new Chatbot();
            bot.GreetUser();

            // ===== USER NAME =====
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("\nEnter your name: ");
            Console.ResetColor();

            string name = Console.ReadLine();
            User user = new User(name);

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"\nWelcome, {user.Name}. System ready.\n");
            Console.ResetColor();

            // ===== CHAT LOOP =====
            while (true)
            {
                // User input
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("> ");
                Console.ResetColor();

                string input = Console.ReadLine();

                // Exit
                if (input.ToLower() == "exit")
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\nThank you for chatting! Stay safe online, " + user.Name + "!");

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Session terminated.");
                    Console.ResetColor();
                    break;
                }

                // Typing effect
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("Bot is typing");
                for (int i = 0; i < 3; i++)
                {
                    Thread.Sleep(400);
                    Console.Write(".");
                }
                Console.WriteLine();
                Console.ResetColor();

                // Bot response
                string response = bot.GetResponse(input);
                bot.TypeEffect(response);

                Console.WriteLine();
            }
        }
    }
}
