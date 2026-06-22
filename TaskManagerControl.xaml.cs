using System;
using System.Windows;
using System.Windows.Controls;

namespace CyberSecurityChatbot
{
    public partial class TaskManagerControl : UserControl
    {
        private Chatbot _bot;

        private int _editingId = -1;
        private bool IsEditing => _editingId != -1;

        public TaskManagerControl()
        {
            InitializeComponent();
        }

        public void SetBot(Chatbot bot)
        {
            _bot = bot;
            InitialiseDb();
            LoadTasks();
        }

        public void Refresh() => LoadTasks();

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

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            if (IsEditing)
                SaveEdit();
            else
                AddNewTask();
        }

        private void AddNewTask()
        {
            string title = TitleBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                ShowStatus("⚠️ Please enter a task title.", isError: true);
                return;
            }

            var task = new CyberTask
            {
                Title = title,
                Description = DescBox.Text.Trim(),
                Reminder = ReminderPicker.SelectedDate
            };

            try
            {
                DatabaseHelper.AddTask(task);
                _bot?.Log.Add("Task added", title);

                string reminderMsg = task.Reminder.HasValue
                    ? $" Reminder set for {task.Reminder.Value:dd MMM yyyy}."
                    : "";

                ShowStatus($"✅ Task '{title}' added.{reminderMsg}");
                ClearForm();
                LoadTasks();
            }
            catch (Exception ex)
            {
                ShowDbError($"⚠️ Could not save task: {ex.Message}");
            }
        }

        private void EditTask_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                var tasks = TaskList.ItemsSource as System.Collections.Generic.List<CyberTask>;
                var task = tasks?.Find(t => t.Id == id);
                if (task == null) return;

                TitleBox.Text = task.Title;
                DescBox.Text = task.Description;
                ReminderPicker.SelectedDate = task.Reminder;

                _editingId = id;
                FormTitle.Text = "✏ EDIT TASK";
                AddTaskBtn.Content = "💾 Save Changes";
                CancelEditBtn.Visibility = Visibility.Visible;

                ShowStatus($"Editing: '{task.Title}' — make changes and click Save.", isError: false);
            }
        }

        private void SaveEdit()
        {
            string title = TitleBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                ShowStatus("⚠️ Please enter a task title.", isError: true);
                return;
            }

            var updated = new CyberTask
            {
                Id = _editingId,
                Title = title,
                Description = DescBox.Text.Trim(),
                Reminder = ReminderPicker.SelectedDate
            };

            try
            {
                DatabaseHelper.UpdateTask(updated);
                _bot?.Log.Add("Task updated", title);
                ShowStatus($"✅ Task '{title}' updated successfully.");
                ExitEditMode();
                LoadTasks();
            }
            catch (Exception ex)
            {
                ShowDbError($"⚠️ Could not update task: {ex.Message}");
            }
        }

        private void CancelEdit_Click(object sender, RoutedEventArgs e)
        {
            ExitEditMode();
            ShowStatus("Edit cancelled.");
        }

        private void ExitEditMode()
        {
            _editingId = -1;
            FormTitle.Text = "📋 ADD TASK";
            AddTaskBtn.Content = "➕ Add Task";
            CancelEditBtn.Visibility = Visibility.Collapsed;
            ClearForm();
        }

        private void SuggestTask_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
                TitleBox.Text = btn.Tag.ToString();
        }

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

        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                if (_editingId == id) ExitEditMode();

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

        private void Refresh_Click(object sender, RoutedEventArgs e) => LoadTasks();

        private void ClearForm()
        {
            TitleBox.Text = "";
            DescBox.Text = "";
            ReminderPicker.SelectedDate = null;
        }

        private void ShowStatus(string msg, bool isError = false)
        {
            StatusText.Text = msg;
            StatusText.Foreground = isError
                ? System.Windows.Media.Brushes.OrangeRed
                : System.Windows.Media.Brushes.LimeGreen;
            StatusText.Visibility = Visibility.Visible;
        }

        private void ShowDbError(string msg)
        {
            DbErrorText.Text = msg;
            DbErrorText.Visibility = Visibility.Visible;
        }
    }
}