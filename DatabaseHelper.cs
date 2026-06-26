using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace CybersecurityChatbot
{
    // Represents a single cybersecurity task stored in the database
    public class CyberTask
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Reminder { get; set; }      // e.g. "3 days" or a date string
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Handles all communication with the MySQL database
    // All SQL is kept here so no other class touches the database directly
    public class DatabaseHelper
    {
        // Connection string - update the password to match your MySQL setup
        private const string ConnectionString =
            "Server=localhost;Database=cybersecurity_chatbot;Uid=root;Pwd=Rakgoale@1;";

        // Creates the database and tasks table if they do not already exist
        // Called once when the app starts so the user never has to set up MySQL manually
        public void Initialise()
        {
            // First connect without specifying a database so we can create it
            string setupConnection = "Server=localhost;Uid=root;Pwd=Rakgoale@1;";

            using (var conn = new MySqlConnection(setupConnection))
            {
                conn.Open();

                // Create the database if it does not exist
                string createDb = "CREATE DATABASE IF NOT EXISTS cybersecurity_chatbot;";
                new MySqlCommand(createDb, conn).ExecuteNonQuery();
            }

            // Now connect to the database and create the tasks table
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();

                string createTable = @"
                    CREATE TABLE IF NOT EXISTS tasks (
                        id          INT AUTO_INCREMENT PRIMARY KEY,
                        title       VARCHAR(200)  NOT NULL,
                        description VARCHAR(500)  NOT NULL,
                        reminder    VARCHAR(100),
                        is_completed TINYINT(1)   NOT NULL DEFAULT 0,
                        created_at  DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP
                    );";

                new MySqlCommand(createTable, conn).ExecuteNonQuery();
            }
        }

        // Inserts a new task into the database and returns the generated ID
        public int AddTask(string title, string description, string reminder)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();

                string insertSql = @"
                    INSERT INTO tasks (title, description, reminder)
                    VALUES (@title, @desc, @reminder);";

                using (var cmd = new MySqlCommand(insertSql, conn))
                {
                    cmd.Parameters.AddWithValue("@title", title);
                    cmd.Parameters.AddWithValue("@desc", description);
                    cmd.Parameters.AddWithValue("@reminder", reminder ?? "");
                    cmd.ExecuteNonQuery();
                }

                using (var idCmd = new MySqlCommand("SELECT LAST_INSERT_ID();", conn))
                {
                    return Convert.ToInt32(idCmd.ExecuteScalar());
                }
            }
        }

        // Returns all tasks from the database, newest first
        public List<CyberTask> GetAllTasks()
        {
            var tasks = new List<CyberTask>();

            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();

                string sql = "SELECT * FROM tasks ORDER BY created_at DESC;";

                using (var reader = new MySqlCommand(sql, conn).ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tasks.Add(new CyberTask
                        {
                            Id          = reader.GetInt32("id"),
                            Title       = reader.GetString("title"),
                            Description = reader.GetString("description"),
                            Reminder    = reader.IsDBNull(reader.GetOrdinal("reminder"))
                                            ? "" : reader.GetString("reminder"),
                            IsCompleted = reader.GetBoolean("is_completed"),
                            CreatedAt   = reader.GetDateTime("created_at")
                        });
                    }
                }
            }

            return tasks;
        }

        // Marks a task as completed using its ID
        public void MarkCompleted(int taskId)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();

                string sql = "UPDATE tasks SET is_completed = 1 WHERE id = @id;";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", taskId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Updates just the reminder field for an existing task
        public void UpdateReminder(int taskId, string reminder)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();

                string sql = "UPDATE tasks SET reminder = @reminder WHERE id = @id;";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@reminder", reminder);
                    cmd.Parameters.AddWithValue("@id", taskId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Permanently removes a task from the database using its ID
        public void DeleteTask(int taskId)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();

                string sql = "DELETE FROM tasks WHERE id = @id;";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", taskId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
