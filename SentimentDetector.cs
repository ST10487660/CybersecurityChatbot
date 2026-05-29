using System.Collections.Generic;

namespace CybersecurityChatbot
{
    public enum Sentiment
    {
        Neutral,
        Worried,
        Curious,
        Frustrated,
        Happy
    }

    public class SentimentDetector
    {
        private Dictionary<Sentiment, List<string>> _triggerWords;

        public SentimentDetector()
        {
            _triggerWords = new Dictionary<Sentiment, List<string>>
            {
                {
                    Sentiment.Worried, new List<string>
                    {
                        "worried", "scared", "afraid", "anxious",
                        "nervous", "unsafe", "concerned", "frightened", "panic"
                    }
                },
                {
                    Sentiment.Curious, new List<string>
                    {
                        "curious", "wondering", "interested", "want to know",
                        "how does", "tell me", "what is", "explain", "learn"
                    }
                },
                {
                    Sentiment.Frustrated, new List<string>
                    {
                        "frustrated", "annoyed", "confused", "don't understand",
                        "angry", "irritated", "fed up", "lost", "stuck"
                    }
                },
                {
                    Sentiment.Happy, new List<string>
                    {
                        "great", "thanks", "helpful", "awesome", "love it",
                        "amazing", "perfect", "excellent", "happy", "glad", "cool"
                    }
                }
            };
        }

        public Sentiment Detect(string input)
        {
            string lower = input.ToLower();

            foreach (var kvp in _triggerWords)
                foreach (string trigger in kvp.Value)
                    if (lower.Contains(trigger))
                        return kvp.Key;

            return Sentiment.Neutral;
        }

        public string GetSentimentResponse(Sentiment sentiment)
        {
            switch (sentiment)
            {
                case Sentiment.Worried:
                    return "It's completely understandable to feel that way — you're already taking a great step by learning about it. Here's something that can help:\n\n";
                case Sentiment.Curious:
                    return "Great question! Staying curious is one of the best ways to stay cyber-safe. Here's what you should know:\n\n";
                case Sentiment.Frustrated:
                    return "I hear you — this can be confusing. Let me break it down as simply as possible:\n\n";
                case Sentiment.Happy:
                    return "Glad to hear it! Keep that positive energy — here's another tip to keep you safe:\n\n";
                default:
                    return string.Empty;
            }
        }
    }
}
