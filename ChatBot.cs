using System;
using System.Collections.Generic;

namespace CybersecurityChatbot
{
    // Central chatbot class - the only class MainWindow.xaml.cs talks to
    // Routes user input through all features in the correct priority order
    public class ChatBot
    {

        private KeywordResponder _keywords;      // Part 2: keyword recognition
        private SentimentDetector _sentiment;    // Part 2: sentiment detection
        private MemoryStore _memory;             // Part 2: remembers name and topic

        private DatabaseHelper _database;        // Part 3: MySQL connection
        private TaskAssistant _taskAssistant;    // Part 3: task management
        private QuizManager _quiz;               // Part 3: quiz mini-game
        private NlpProcessor _nlp;              // Part 3: NLP simulation
        private ActivityLog _activityLog;        // Part 3: action history

        // Delegates define the method signature that event subscribers must match
        public delegate void ResponseReadyHandler(string response);
        public delegate void SentimentChangedHandler(Sentiment sentiment);

        // Events that MainWindow subscribes to
        // OnResponseReady fires whenever the bot has a message to display
        // OnSentimentChanged fires when a new sentiment is detected
        public event ResponseReadyHandler OnResponseReady;
        public event SentimentChangedHandler OnSentimentChanged;

        private bool _awaitingName = true;   // true until the user provides their name
        private string _lastTopic = null;    // the last cybersecurity topic discussed
        private string _lastDatabaseError = null;

        public ChatBot()
        {
            // Part 2 classes
            _keywords = new KeywordResponder();
            _sentiment = new SentimentDetector();
            _memory = new MemoryStore();

            // Part 3 classes
            _database = new DatabaseHelper();
            _quiz = new QuizManager();
            _nlp = new NlpProcessor();
            _activityLog = new ActivityLog();

            // Set up the database table on startup
            // This creates the database and table if they do not exist yet
            try
            {
                _database.Initialise();
                _taskAssistant = new TaskAssistant(_database);
            }
            catch (Exception ex)
            {
                _taskAssistant = null;
                _lastDatabaseError = ex.Message;
                Console.WriteLine("Database error: " + ex.Message);
            }
        }

        // Returns the opening message shown when the app first loads
        public string GetGreeting()
        {
            return "Hello! Welcome to the Cybersecurity Awareness Chatbot.\n\n"
                 + "I'm here to help you learn about staying safe online, manage cybersecurity tasks, and test your knowledge with a quiz.";
        }

        // Returns the follow-up message asking for the user's name
        public string GetNamePrompt()
        {
            return "Before we get started, what is your name?";
        }

        // Every message the user sends goes through this method
        // It checks each feature in priority order and fires the first match
        public void ProcessInput(string userInput)
        {
            // Guard: ignore empty input
            if (string.IsNullOrWhiteSpace(userInput))
            {
                Fire("Please type something.");
                return;
            }

            string input = userInput.Trim();
            string lower = input.ToLower();

            if (_awaitingName)
            {
                string name = Capitalise(input.Split(' ')[0]);
                _memory.UserName = name;
                _awaitingName = false;

                _activityLog.Log("User introduced themselves as " + name);

                Fire("Nice to meet you, " + name + ".\n\n"
                   + "I can help you with cybersecurity topics, tasks, a quiz, and more.\n"
                   + "Type \"help\" to see all options, or just ask a question.");
                return;
            }

            // Once the quiz has started, all input is treated as quiz answers
            // until the quiz ends
            if (_quiz.IsActive)
            {
                string quizResponse = _quiz.SubmitAnswer(input);
                _activityLog.Log("Quiz answer submitted: " + input);
                Fire(quizResponse);
                return;
            }

            // TaskAssistant tracks whether it is waiting for a reminder response
            // If it is, it must handle the input before anything else
            if (_taskAssistant != null && _taskAssistant.IsTaskInput(lower))
            {
                string taskResponse = _taskAssistant.HandleInput(lower, input);
                if (taskResponse != null)
                {
                    _activityLog.Log("Task action: " + input);
                    Fire(taskResponse);
                    return;
                }
            }

            // Check if the user's input maps to a known intent
            // even if they phrased it in an unusual way
            string intent = _nlp.DetectIntent(lower);

            if (intent != null)
            {
                // Some intents are handled by other classes
                // NlpProcessor returns null for those, signalling to route them
                string nlpResponse = _nlp.BuildIntentConfirmation(intent, input);

                if (nlpResponse != null)
                {
                    // NLP handled it directly (e.g. "help")
                    _activityLog.Log("NLP intent: " + intent);
                    Fire(nlpResponse);
                    return;
                }

                // Route to the correct handler based on intent
                switch (intent)
                {
                    case "add_task":
                    case "view_tasks":
                        if (_taskAssistant != null)
                        {
                            string tr = _taskAssistant.HandleInput(lower, input);
                            if (tr != null)
                            {
                                _activityLog.Log("Task action via NLP: " + intent);
                                Fire(tr);
                                return;
                            }
                        }
                        break;

                    case "start_quiz":
                        _activityLog.Log("Quiz started");
                        Fire(_quiz.Start());
                        return;

                    case "show_log":
                        Fire(_activityLog.GetSummary());
                        return;
                }
            }

            // "tell me more", "another tip" etc. continue the last topic
            if (IsFollowUp(lower))
            {
                Fire(HandleFollowUp());
                return;
            }

            // Detect the emotional tone and get an empathetic opener
            Sentiment detected = _sentiment.Detect(lower);
            string sentimentOpener = _sentiment.GetSentimentResponse(detected);
            OnSentimentChanged?.Invoke(detected);

            if (detected != Sentiment.Neutral)
                _activityLog.Log("Sentiment detected: " + detected.ToString());

            // Check if the input contains a known cybersecurity keyword
            string matchedKeyword;
            string keywordResponse = _keywords.GetResponse(lower, out matchedKeyword);

            if (keywordResponse != null)
            {
                _lastTopic = matchedKeyword;
                string memoryNote = BuildMemoryNote(lower, matchedKeyword);
                _activityLog.Log("Keyword matched: " + matchedKeyword);
                Fire(sentimentOpener + memoryNote + FormatTopicResponse(matchedKeyword, keywordResponse));
                return;
            }

            if (lower.Contains("how are you"))
            {
                Fire("Running fine and ready to help, " + _memory.UserName + ". What would you like to know?");
                return;
            }

            if (lower.Contains("what can you") || lower.Contains("help") || lower.Contains("topic"))
            {
                Fire(_nlp.BuildIntentConfirmation("help", input));
                return;
            }

            if (lower.Contains("hello") || lower.Contains("hi") || lower.Contains("hey"))
            {
                Fire("Hello, " + _memory.UserName + ". Ask me anything about staying safe online.");
                return;
            }

            if (lower == "exit" || lower == "bye" || lower == "quit")
            {
                Fire("Goodbye, " + _memory.UserName + ". Stay safe online.");
                return;
            }

            // Nothing matched - give a helpful fallback rather than just "I don't know"
            Fire(sentimentOpener
               + "I did not quite understand that. Could you rephrase?\n\n"
               + "You can ask about: " + string.Join(", ", _keywords.GetAllKeywords()) + ".\n"
               + "Or type \"help\" to see all options.");
        }

        // Returns the task list as plain text, bypassing the conversational
        // routing in ProcessInput. Used by MainWindow's Tasks tab so refreshing
        // the panel doesn't get tangled up with quiz state, name prompts, etc.
        public string GetTaskListText()
        {
            if (_taskAssistant == null)
            {
                return "Task storage is unavailable right now.\nDatabase error: "
                     + (_lastDatabaseError ?? "unknown error");
            }

            return _taskAssistant.ShowTasks();
        }

        public List<CyberTask> GetAllTasks()
        {
            return _taskAssistant?.GetAllTasks() ?? new List<CyberTask>();
        }

        public string DatabaseErrorMessage => _lastDatabaseError;

        // Exposes the quiz manager so the Quiz tab UI can drive it directly
        public QuizManager Quiz => _quiz;

        // Fires the OnResponseReady event to send a message to the GUI
        private void Fire(string message) => OnResponseReady?.Invoke(message);

        // Returns true if the input is asking for more on the current topic
        private bool IsFollowUp(string lower)
        {
            return lower.Contains("another") || lower.Contains("more")
                || lower.Contains("tell me more") || lower.Contains("explain more")
                || lower.Contains("go on") || lower.Contains("continue")
                || lower.Contains("next tip") || lower.Contains("again")
                || lower.Contains("elaborate");
        }

        // Handles follow-up requests by fetching another tip on the last topic
        private string HandleFollowUp()
        {
            if (_lastTopic == null)
                return "What topic would you like to explore? Try asking about passwords, phishing, scams, or privacy.";

            string tip = _keywords.GetAnotherResponse(_lastTopic);
            return tip == null
                ? "I do not have more on that topic right now. Try asking something else."
                : "Here is another tip on " + _lastTopic + ":\n\n" + tip + "\n\nType \"more\" for another tip.";
        }

        // Checks if the user is expressing interest in a topic and stores it in memory
        private string BuildMemoryNote(string lower, string matchedKeyword)
        {
            if (lower.Contains("interest") || lower.Contains("want to learn")
                || lower.Contains("tell me about"))
            {
                _memory.FavouriteTopic = matchedKeyword;
                return "Noted - I will remember you are interested in " + matchedKeyword + ". ";
            }

            if (_memory.HasFavouriteTopic() && _memory.FavouriteTopic == matchedKeyword)
                return _memory.GetPersonalisedOpener();

            return string.Empty;
        }

        // Formats a topic response with a clear label
        private string FormatTopicResponse(string topic, string tip)
        {
            return "[" + Capitalise(topic) + "]\n\n" + tip
                 + "\n\nType \"more\" for another tip.";
        }

        // Capitalises the first letter and lowercases the rest
        private string Capitalise(string word)
        {
            if (string.IsNullOrEmpty(word)) return word;
            return char.ToUpper(word[0]) + word.Substring(1).ToLower();
        }
    }
}