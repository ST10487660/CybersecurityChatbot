using System;
using System.Collections.Generic;
using System.Text;

namespace CybersecurityChatbot
{
    // Handles all task-related chatbot logic
    // Sits between ChatBot.cs and DatabaseHelper.cs
    // ChatBot calls methods here
    public class TaskAssistant
    {
        private readonly DatabaseHelper _db;

        // Tracks whether we are waiting for the user to confirm a reminder
        // after adding a task
        private bool _awaitingReminderResponse = false;
        private string _pendingTaskTitle = null;
        private int _pendingTaskId = -1;

        public TaskAssistant(DatabaseHelper db)
        {
            _db = db;
        }

        // Called by ChatBot.ProcessInput() when task-related keywords are detected
        // Returns a response string or null if the input was not task-related
        public string HandleInput(string lower, string original)
        {
            // If we are waiting for a reminder response after adding a task
            if (_awaitingReminderResponse)
                return HandleReminderResponse(lower, original);

            // Adding a task
            if (lower.Contains("add task") || lower.Contains("add a task")
                || lower.Contains("create task") || lower.Contains("new task")
                || lower.Contains("remind me to") || lower.Contains("set a reminder"))
                return AddTaskFromInput(original);

            // Viewing tasks
            if (lower.Contains("show tasks") || lower.Contains("view tasks")
                || lower.Contains("list tasks") || lower.Contains("my tasks")
                || lower.Contains("what tasks"))
                return ShowTasks();

            // Completing a task
            if (lower.Contains("complete task") || lower.Contains("mark task")
                || lower.Contains("done task") || lower.Contains("finished task"))
                return CompleteTaskFromInput(lower);

            // Deleting a task
            if (lower.Contains("delete task") || lower.Contains("remove task"))
                return DeleteTaskFromInput(lower);

            return null; // input was not task-related
        }

        //  Add task 

        private string AddTaskFromInput(string original)
        {
            // Extract the task title from the user's message
            // Strip common command phrases to get the actual task name
            string title = original.ToLower()
       .Replace("add task", "")
       .Replace("add a task", "")
       .Replace("create task", "")
       .Replace("new task", "")
       .Replace("remind me to", "")
       .Replace("set a reminder to", "")
       .Replace("-", "")
       .Trim();

            // If nothing meaningful was left after stripping, use a default
            if (string.IsNullOrWhiteSpace(title))
                title = "Cybersecurity task";

            // Build a description based on known cybersecurity keywords in the title
            string description = BuildDescription(title);

            // Add to DB without a reminder for now
            _pendingTaskId = _db.AddTask(title, description, "");
            _pendingTaskTitle = title;

            // Set flag so the next input is treated as a reminder response
            _awaitingReminderResponse = true;

            return "Task added: \"" + title + "\"\n"
                 + "Description: " + description + "\n\n"
                 + "Would you like to set a reminder? If yes, say something like "
                 + "\"remind me in 3 days\" or \"remind me on Monday\". "
                 + "Otherwise type \"no\".";
        }

        // Builds an auto-generated description based on keywords in the task title
        private string BuildDescription(string title)
        {
            string lower = title.ToLower();

            if (lower.Contains("password"))
                return "Update your passwords to strong, unique ones using a password manager.";
            if (lower.Contains("2fa") || lower.Contains("two-factor") || lower.Contains("authentication"))
                return "Enable two-factor authentication to add a second layer of security to your accounts.";
            if (lower.Contains("privacy"))
                return "Review your account privacy settings to control who can see your information.";
            if (lower.Contains("backup"))
                return "Back up your important data to a secure, offline or cloud location.";
            if (lower.Contains("update") || lower.Contains("patch"))
                return "Keep your software and operating system updated to protect against known vulnerabilities.";
            if (lower.Contains("antivirus") || lower.Contains("scan"))
                return "Run a full antivirus scan to check for malware or suspicious activity.";
            if (lower.Contains("vpn"))
                return "Set up a VPN to encrypt your internet connection, especially on public networks.";

            // Generic fallback description
            return "Complete this cybersecurity task to improve your online safety.";
        }

        
        private string HandleReminderResponse(string lower, string original)
        {
            _awaitingReminderResponse = false;

            // User said no
            if (lower.Contains("no") || lower == "n")
                return "Got it. No reminder set for \"" + _pendingTaskTitle + "\". "
                     + "Type \"show tasks\" to see all your tasks.";

            // Extract reminder timeframe from the input
            string reminder = ExtractReminder(original);

            // Update the task in the database with the reminder
            UpdateReminder(_pendingTaskId, reminder);

            return "Reminder set for \"" + _pendingTaskTitle + "\": " + reminder + ".\n"
                 + "Type \"show tasks\" to see all your tasks.";
        }

        // Tries to extract a timeframe like "3 days" or "Monday" from the input
        private string ExtractReminder(string input)
        {
            string lower = input.ToLower();

            // Look for "in X days/weeks" pattern
            if (lower.Contains("day"))
            {
                foreach (string word in input.Split(' '))
                {
                    if (int.TryParse(word, out int n))
                        return "in " + n + " day" + (n == 1 ? "" : "s");
                }
                return "in a few days";
            }

            if (lower.Contains("week"))
            {
                foreach (string word in input.Split(' '))
                {
                    if (int.TryParse(word, out int n))
                        return "in " + n + " week" + (n == 1 ? "" : "s");
                }
                return "in a week";
            }

            // Named days
            string[] days = { "monday", "tuesday", "wednesday", "thursday", "friday", "saturday", "sunday" };
            foreach (string day in days)
                if (lower.Contains(day))
                    return "on " + char.ToUpper(day[0]) + day.Substring(1);

            if (lower.Contains("tomorrow"))
                return "tomorrow";

            // If nothing matched, store the raw input as the reminder
            return input.Trim();
        }

        // Updates just the reminder field for an existing task
        private void UpdateReminder(int taskId, string reminder)
        {
            _db.UpdateReminder(taskId, reminder);
        }

        public List<CyberTask> GetAllTasks()
        {
            return _db.GetAllTasks();
        }

        public string ShowTasks()
        {
            var tasks = _db.GetAllTasks();

            if (tasks.Count == 0)
                return "You have no tasks yet. Type \"add task - enable 2FA\" to add one.";

            var sb = new StringBuilder();
            sb.AppendLine("Here are your tasks:\n");

            foreach (var t in tasks)
            {
                // Show a tick for completed tasks, a dash for pending ones
                string status = t.IsCompleted ? "[Done]" : "[Pending]";
                sb.AppendLine(t.Id + ". " + status + " " + t.Title);
                sb.AppendLine("   " + t.Description);

                if (!string.IsNullOrWhiteSpace(t.Reminder))
                    sb.AppendLine("   Reminder: " + t.Reminder);

                sb.AppendLine();
            }

            sb.AppendLine("Type \"complete task 1\" or \"delete task 1\" to manage tasks.");
            return sb.ToString();
        }

        private string CompleteTaskFromInput(string lower)
        {
            int id = ExtractId(lower);

            if (id == -1)
                return "Please specify the task number. Example: \"complete task 2\"";

            _db.MarkCompleted(id);
            return "Task " + id + " marked as completed. Well done for staying cyber-safe!";
        }

        private string DeleteTaskFromInput(string lower)
        {
            int id = ExtractId(lower);

            if (id == -1)
                return "Please specify the task number. Example: \"delete task 2\"";

            _db.DeleteTask(id);
            return "Task " + id + " has been deleted.";
        }

        // Extracts the first number found in a string
        // Used to get the task ID from inputs like "delete task 3"
        private int ExtractId(string input)
        {
            foreach (string word in input.Split(' '))
                if (int.TryParse(word, out int id))
                    return id;
            return -1;
        }

        // Returns true if the input contains any task-related keyword
        // Used by ChatBot.cs to decide whether to route to this class
        public bool IsTaskInput(string lower)
        {
            return lower.Contains("add task") || lower.Contains("add a task")
                || lower.Contains("create task") || lower.Contains("new task")
                || lower.Contains("show tasks") || lower.Contains("view tasks")
                || lower.Contains("list tasks") || lower.Contains("my tasks")
                || lower.Contains("complete task") || lower.Contains("mark task")
                || lower.Contains("delete task") || lower.Contains("remove task")
                || lower.Contains("remind me to") || lower.Contains("set a reminder")
                || _awaitingReminderResponse; // always handle if we are mid-conversation
        }
    }
}
