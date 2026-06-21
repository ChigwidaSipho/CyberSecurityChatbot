using System;
using System.Windows;
using System.Windows.Controls;

namespace CyberSecurityChatbot
{
    /// <summary>
    /// Task Assistant tab — add, view, complete, and delete cybersecurity tasks.
    /// Stores everything in MySQL via DatabaseHelper.
    /// </summary>
    public partial class TaskManagerControl : UserControl  
    {
        private Chatbot _bot;

        public TaskManagerControl()
        {
            InitializeComponent();
        }

        /// <summary>Called by MainWindow to inject the shared bot (for activity logging).</summary>
        public void SetBot(Chatbot bot)
        {
            _bot = bot;
            InitialiseDb();
            LoadTasks();
        }

        // ===== DB INIT =====
        private void InitialiseDb()
        {
            try
            {
                DatabaseHelper.Initialise();
                DbErrorText.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                ShowDbError($"⚠️ DB not connected: {ex.Message}\n\nSee README for MySQL setup.");
            }
        }

        // ===== LOAD TASKS =====
        private void LoadTasks()
        {
            try
            {
                var tasks = DatabaseHelper.GetAllTasks();
                TaskList.ItemsSource = null;
                TaskList.ItemsSource = tasks;
            }  
            catch (Exception ex)
            {
                ShowDbError($"⚠️ Could not load tasks: {ex.Message}");
            }
        }

        // ===== ADD TASK =====
        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            string title = TitleBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                ShowStatus("⚠️ Please enter a task title.", isError: true);
                return;
            }

            var task = new CyberTask
            {
                Title       = title,
                Description = DescBox.Text.Trim(),
                Reminder    = ReminderPicker.SelectedDate
            };

            try
            {
                DatabaseHelper.AddTask(task);
                _bot?.Log.Add("Task added", title);

                string reminderMsg = task.Reminder.HasValue
                    ? $" Reminder set for {task.Reminder.Value:dd MMM yyyy}."
                    : "";

                ShowStatus($"✅ Task '{title}' added.{reminderMsg}");

                // Clear form
                TitleBox.Text = "";
                DescBox.Text  = "";
                ReminderPicker.SelectedDate = null;

                LoadTasks();
            }
            catch (Exception ex)
            {
                ShowDbError($"⚠️ Could not save task: {ex.Message}");
            }
        }

        // ===== SUGGEST TASK =====
        private void SuggestTask_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
                TitleBox.Text = btn.Tag.ToString();
        }

        // ===== MARK DONE =====
        private void MarkDone_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                try
                {
                    DatabaseHelper.MarkComplete(id);
                    _bot?.Log.Add("Task completed", $"ID {id}");
                    LoadTasks();
                }
                catch (Exception ex)
                {
                    ShowDbError($"⚠️ Could not update task: {ex.Message}");
                }
            }
        }

        // ===== DELETE =====
        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                try
                {
                    DatabaseHelper.DeleteTask(id);
                    _bot?.Log.Add("Task deleted", $"ID {id}");
                    LoadTasks();
                }
                catch (Exception ex)
                {
                    ShowDbError($"⚠️ Could not delete task: {ex.Message}");
                }
            }
        }

        // ===== REFRESH =====
        private void Refresh_Click(object sender, RoutedEventArgs e) => LoadTasks();

        // ===== HELPERS =====
        private void ShowStatus(string msg, bool isError = false)
        {
            StatusText.Text       = msg;
            StatusText.Foreground = isError
                ? System.Windows.Media.Brushes.OrangeRed
                : System.Windows.Media.Brushes.LimeGreen;
            StatusText.Visibility = Visibility.Visible;
        }

        private void ShowDbError(string msg)
        {
            DbErrorText.Text       = msg;
            DbErrorText.Visibility = Visibility.Visible;
        }
    }
}
