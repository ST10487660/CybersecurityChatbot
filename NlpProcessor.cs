using System.Collections.Generic;

namespace CybersecurityChatbot
{
    // Simulates Natural Language Processing using dictionaries of phrase variations
    // The goal is to understand the user's intent even when they phrase things differently
    // e.g. "Can you remind me to update my password?" should trigger the task assistant
    public class NlpProcessor
    {
        // Maps a recognised intent to a list of phrases that express that intent
        // string.Contains() checks are used to match partial phrases in the user's input
        private Dictionary<string, List<string>> _intentPhrases;

        public NlpProcessor()
        {
            _intentPhrases = new Dictionary<string, List<string>>
            {
                // Task-related intents - various ways a user might ask to add a task
                {
                    "add_task", new List<string>
                    {
                        "add task", "add a task", "create task", "new task",
                        "i need to", "i want to", "can you remind me to",
                        "remind me to", "set a reminder", "make a note",
                        "set up", "i should", "don't let me forget"
                    }
                },

                // View tasks - various ways a user might ask to see their task list
                {
                    "view_tasks", new List<string>
                    {
                        "show tasks", "view tasks", "list tasks", "my tasks",
                        "what tasks", "show my tasks", "what do i need to do",
                        "what have i got", "pending tasks", "task list"
                    }
                },

                // Quiz intents - ways a user might ask to start the quiz
                {
                    "start_quiz", new List<string>
                    {
                        "start quiz", "quiz me", "take the quiz", "play quiz",
                        "test me", "test my knowledge", "i want a quiz",
                        "let's play", "cybersecurity quiz", "begin quiz",
                        "start the game", "mini game"
                    }
                },

                // Activity log - ways a user might ask to see the log
                {
                    "show_log", new List<string>
                    {
                        "show log", "activity log", "show activity",
                        "what have you done", "what have you done for me",
                        "recent actions", "history", "show history",
                        "what did you do", "log"
                    }
                },

                // Help intent - ways a user might ask what the bot can do
                {
                    "help", new List<string>
                    {
                        "help", "what can you do", "what can i ask",
                        "how do i use", "commands", "options", "menu",
                        "what are my options", "capabilities"
                    }
                }
            };
        }

        // Returns the matched intent string or null if nothing matched
        // The intent string matches the keys in _intentPhrases above
        public string DetectIntent(string lower)
        {
            foreach (var kvp in _intentPhrases)
            {
                foreach (string phrase in kvp.Value)
                {
                    if (lower.Contains(phrase))
                        return kvp.Key; // return the intent name
                }
            }

            return null; // no intent matched
        }

        // Extracts cybersecurity-relevant keywords from a sentence
        // Used to improve task titles and descriptions when the user types naturally
        // e.g. "I need to update my password and enable 2FA" -> ["password", "2fa"]
        public List<string> ExtractKeywords(string lower)
        {
            var found = new List<string>();

            // Cybersecurity keyword list - same topics as KeywordResponder
            string[] keywords = {
                "password", "phishing", "2fa", "two-factor", "authentication",
                "malware", "ransomware", "vpn", "privacy", "backup",
                "antivirus", "firewall", "encryption", "hacking", "scam",
                "wifi", "update", "patch", "social engineering"
            };

            foreach (string keyword in keywords)
                if (lower.Contains(keyword))
                    found.Add(keyword);

            return found;
        }

        // Takes the detected intent and builds a natural-sounding confirmation
        // so the bot feels responsive rather than mechanical
        public string BuildIntentConfirmation(string intent, string userInput)
        {
            switch (intent)
            {
                case "add_task":
                    return null; // TaskAssistant handles the full response

                case "view_tasks":
                    return null; // TaskAssistant handles the full response

                case "start_quiz":
                    return null; // QuizManager handles the full response

                case "show_log":
                    return null; // ActivityLog handles the full response

                case "help":
                    return "Here is what I can help you with:\n\n"
                         + "  - Cybersecurity topics: password, phishing, malware, vpn, 2fa, wifi, privacy, scam, hacking, ransomware\n"
                         + "  - Add tasks: \"add task - enable 2FA\" or \"remind me to update my password\"\n"
                         + "  - View tasks: \"show tasks\" or \"my tasks\"\n"
                         + "  - Complete/delete tasks: \"complete task 1\" or \"delete task 2\"\n"
                         + "  - Quiz: \"start quiz\" or \"test me\"\n"
                         + "  - Activity log: \"show activity log\" or \"what have you done for me\"\n"
                         + "  - Follow-up: \"tell me more\" or \"give me another tip\"";

                default:
                    return null;
            }
        }
    }
}
