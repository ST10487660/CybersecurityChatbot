using System.Collections.Generic;

namespace CybersecurityChatbot
{
    public class MemoryStore
    {
        private Dictionary<string, string> _memory = new Dictionary<string, string>();

        public string UserName
        {
            get => Recall("name") ?? "there";
            set => Store("name", value);
        }

        public string FavouriteTopic
        {
            get => Recall("favouriteTopic");
            set => Store("favouriteTopic", value);
        }

        public string LastTopic
        {
            get => Recall("lastTopic");
            set => Store("lastTopic", value);
        }

        public void Store(string key, string value)
        {
            if (!string.IsNullOrWhiteSpace(key))
                _memory[key] = value;
        }

        public string Recall(string key)
        {
            return _memory.ContainsKey(key) ? _memory[key] : null;
        }

        public string GetPersonalisedOpener()
        {
            string name = Recall("name");
            string topic = Recall("favouriteTopic");

            if (name != null && topic != null)
                return $"As someone interested in {topic}, {name}, here's something relevant : ";
            if (topic != null)
                return $"As someone interested in {topic}, here is something relevant : ";
            if (name != null)
                return $"Here's something for you, {name}  ";

            return string.Empty;
        }

        public bool HasFavouriteTopic() => Recall("favouriteTopic") != null;
    }
}