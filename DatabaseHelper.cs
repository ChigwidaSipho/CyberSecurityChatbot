using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace CyberSecurityChatbot
{
    /// <summary>
    /// Handles all MySQL operations for the Task Assistant.
    /// </summary>
    public static class DatabaseHelper
    {
        // 🔧 UPDATE ONLY THIS LINE (password + DB name if needed)
        private const string ConnStr =
            "Server=localhost;Port=3306;Database=cybersecurity_chatbot;Uid=root;Pwd=SiphoChigwida@2007;";

        // ===== INIT =====  
        public static void Initialise() 
        {
            const string sql = @"
                CREATE TABLE IF NOT EXISTS tasks (
                    id          INT AUTO_INCREMENT PRIMARY KEY,
                    title       VARCHAR(200)  NOT NULL,
                    description TEXT,        
                    reminder    DATE,
                    is_complete TINYINT(1)    NOT NULL DEFAULT 0,
                    created_at  DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP
                );";

            Execute(sql);
        }

        // ===== CREATE =====
        public static void AddTask(CyberTask task)
        {
            const string sql = @"
                INSERT INTO tasks (title, description, reminder, is_complete)
                VALUES (@title, @desc, @reminder, 0);";

            using (var conn = Open())
            using (var cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@title", task.Title);
                cmd.Parameters.AddWithValue("@desc", task.Description ?? "");
                cmd.Parameters.AddWithValue("@reminder",
                    task.Reminder.HasValue ? (object)task.Reminder.Value : DBNull.Value);

                cmd.ExecuteNonQuery();
            }
        }

        // ===== READ =====
        public static List<CyberTask> GetAllTasks()
        {
            const string sql = @"
                SELECT id, title, description, reminder, is_complete, created_at
                FROM tasks
                ORDER BY created_at DESC;";

            var list = new List<CyberTask>();

            using (var conn = Open())
            using (var cmd = new MySqlCommand(sql, conn))
            using (var rdr = cmd.ExecuteReader())
            {
                while (rdr.Read())
                {
                    list.Add(new CyberTask
                    {
                        Id = rdr.GetInt32("id"),
                        Title = rdr.GetString("title"),
                        Description = rdr.IsDBNull(rdr.GetOrdinal("description")) ? "" : rdr.GetString("description"),
                        Reminder = rdr.IsDBNull(rdr.GetOrdinal("reminder")) ? (DateTime?)null : rdr.GetDateTime("reminder"),
                        IsComplete = rdr.GetInt32("is_complete") == 1,
                        CreatedAt = rdr.GetDateTime("created_at")
                    });
                }
            }

            return list;
        }

        // ===== UPDATE =====
        public static void MarkComplete(int id)
        {
            const string sql = "UPDATE tasks SET is_complete = 1 WHERE id = @id";

            using (var conn = Open())
            using (var cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        // ===== DELETE =====
        public static void DeleteTask(int id)
        {
            const string sql = "DELETE FROM tasks WHERE id = @id";

            using (var conn = Open())
            using (var cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        // ===== HELPERS =====
        private static void Execute(string sql)
        {
            using (var conn = Open())
            using (var cmd = new MySqlCommand(sql, conn))
            {
                cmd.ExecuteNonQuery();
            }
        }

        private static MySqlConnection Open()
        {
            var conn = new MySqlConnection(ConnStr);
            conn.Open();
            return conn;
        }
    }

    // ===== MODEL =====
    public class CyberTask
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? Reminder { get; set; }
        public bool IsComplete { get; set; }
        public DateTime CreatedAt { get; set; }

        public string ReminderDisplay =>
            Reminder.HasValue ? Reminder.Value.ToString("dd MMM yyyy") : "None";

        public string StatusDisplay =>
            IsComplete ? "✅ Done" : "⏳ Pending";
    }
}