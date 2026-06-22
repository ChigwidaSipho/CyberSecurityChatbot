using System.Windows;

namespace CyberSecurityChatbot
{
    public partial class MainWindow : Window
    {
        private readonly Chatbot _bot;

        public MainWindow()
        {
            InitializeComponent();

            _bot = new Chatbot();

            ChatTab.SetBot(_bot);
            TaskTab.SetBot(_bot);
            QuizTab.SetBot(_bot);
            LogTab.SetLog(_bot.Log);

            _bot.NavigateRequested += OnNavigateRequested;
        }

        private void OnNavigateRequested(string destination)
        {
            Dispatcher.Invoke(() =>
            {
                switch (destination)
                {
                    case "tasks":
                        MainTabs.SelectedItem = MainTabs.Items[1];
                        TaskTab.Refresh();
                        break;
                    case "quiz":
                        MainTabs.SelectedItem = MainTabs.Items[2];
                        QuizTab.StartQuiz();
                        break;
                    case "log":
                        MainTabs.SelectedItem = MainTabs.Items[3];
                        LogTab.Refresh();
                        break;
                }
            });
        }
    }
}