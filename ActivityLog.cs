using System;
using System.Collections.Generic;
using System.Text;

namespace CybersecurityChatbot
{
    // Records significant actions the chatbot has taken during the session
    // Stores up to 10 recent entries and displays them when the user asks
    public class ActivityLog
    {
        // Each entry is a timestamp + description pair
        private readonly List<string> _entries = new List<string>();

        // Maximum number of entries to keep - rubric says 5-10
        private const int MaxEntries = 10;

        // Adds a new entry to the log with the current timestamp
        // Old entries are removed when the log exceeds MaxEntries
        public void Log(string action)
        {
            string timestamp = DateTime.Now.ToString("HH:mm");
            string entry = "[" + timestamp + "] " + action;

            _entries.Add(entry);

            // Keep only the most recent MaxEntries entries
            if (_entries.Count > MaxEntries)
                _entries.RemoveAt(0); // remove the oldest entry
        }

        // Returns a formatted summary of recent actions
        // Called when the user types "show activity log" or "what have you done for me"
        public string GetSummary()
        {
            if (_entries.Count == 0)
                return "No actions recorded yet. Start chatting to build up an activity log.";

            var sb = new StringBuilder();
            sb.AppendLine("Here is a summary of recent actions:\n");

            // Display entries numbered from most recent (reversed)
            var reversed = new List<string>(_entries);
            reversed.Reverse();

            for (int i = 0; i < reversed.Count; i++)
                sb.AppendLine((i + 1) + ". " + reversed[i]);

            return sb.ToString();
        }

        // Returns how many entries are in the log
        public int Count => _entries.Count;
    }
}
