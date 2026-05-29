using System;
using System.Collections.Generic;

namespace CybersecurityChatbot
{
    public class ChatBot
    {
        private KeywordResponder _keywords;
        private SentimentDetector _sentiment;
        private MemoryStore _memory;

        public delegate void ResponseReadyHandler(string response);
        public delegate void SentimentChangedHandler(Sentiment sentiment);

        public event ResponseReadyHandler OnResponseReady;
        public event SentimentChangedHandler OnSentimentChanged;

        private bool _awaitingName = true;
        private string _lastTopic = null;

        public ChatBot()
        {
            _keywords = new KeywordResponder();
            _sentiment = new SentimentDetector();
            _memory = new MemoryStore();
        }

        public string GetGreeting()
        {
            return "Welcome to the Cybersecurity Awareness Chatbot!\n\nWhat is your name?";
        }

        public void ProcessInput(string userInput)
        {
            if (string.IsNullOrWhiteSpace(userInput))
            {
                Fire("Please type something - I'm here to help!");
                return;
            }

            string input = userInput.Trim();
            string lower = input.ToLower();

            //  Capture name
            if (_awaitingName)
            {
                string name = Capitalise(input.Split(' ')[0]);
                _memory.UserName = name;
                _awaitingName = false;

                Fire($"Great to meet you, {name}!\n\nI'm your Cybersecurity Awareness Bot. You can ask me about:\n" + BuildTopicList());
                return;
            }

            //  Follow-up phrases
            if (IsFollowUp(lower))
            {
                Fire(HandleFollowUp());
                return;
            }

            //  Sentiment detection
            Sentiment detected = _sentiment.Detect(lower);
            string sentimentOpener = _sentiment.GetSentimentResponse(detected);
            OnSentimentChanged?.Invoke(detected);

            //  Keyword matching
            string matchedKeyword;
            string keywordResponse = _keywords.GetResponse(lower, out matchedKeyword);

            if (keywordResponse != null)
            {
                _lastTopic = matchedKeyword;
                Fire(sentimentOpener + FormatTopicResponse(matchedKeyword, keywordResponse));
                return;
            }

            //  Special phrases
            if (lower.Contains("how are you"))
            {
                Fire($"Running perfectly and ready to help, {_memory.UserName}! What would you like to know?");
                return;
            }

            if (lower.Contains("what can you") || lower.Contains("help") || lower.Contains("topic"))
            {
                Fire($"Here's what I can help you with, {_memory.UserName}:\n\n" + BuildTopicList());
                return;
            }

            if (lower.Contains("hello") || lower.Contains("hi") || lower.Contains("hey"))
            {
                Fire($"Hey {_memory.UserName}! Ask me anything about staying safe online.");
                return;
            }

            if (lower == "exit" || lower == "bye" || lower == "quit")
            {
                Fire($"Goodbye, {_memory.UserName}! Stay safe online.");
                return;
            }

            //  Fallback
            Fire(sentimentOpener + "I'm not sure I understand that - could you try rephrasing?\n\nYou can ask me about: " + string.Join(", ", _keywords.GetAllKeywords()));
        }

        private void Fire(string message) => OnResponseReady?.Invoke(message);

        private bool IsFollowUp(string lower)
        {
            return lower.Contains("another") || lower.Contains("more") || lower.Contains("tell me more") ||
                   lower.Contains("explain more") || lower.Contains("go on") || lower.Contains("continue") ||
                   lower.Contains("next tip") || lower.Contains("again") || lower.Contains("elaborate");
        }

        private string HandleFollowUp()
        {
            if (_lastTopic == null)
                return "What topic would you like to explore? Try asking about passwords, phishing, scams, or privacy.";

            string tip = _keywords.GetAnotherResponse(_lastTopic);
            return tip == null ? "I don't have more on that topic right now. Try asking something else!" :
                                 $"Here's another tip on {_lastTopic}:\n\n- {tip}\n\nSay 'more' for another!";
        }

        private string FormatTopicResponse(string topic, string tip)
        {
            return $"{Capitalise(topic)} Tip\n\n{tip}\n\nSay 'more' or 'give me another tip' for more advice!";
        }

        private string BuildTopicList()
        {
            string list = "";
            foreach (string k in _keywords.GetAllKeywords())
                list += $"  - {Capitalise(k)}\n";
            return list;
        }

        private string Capitalise(string word)
        {
            if (string.IsNullOrEmpty(word)) return word;
            return char.ToUpper(word[0]) + word.Substring(1).ToLower();
        }
    }
}