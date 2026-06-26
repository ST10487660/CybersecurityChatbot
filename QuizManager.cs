using System;
using System.Collections.Generic;
using System.Text;

namespace CybersecurityChatbot
{
    // Represents a single quiz question
    public class QuizQuestion
    {
        public string Question { get; set; }

        // The answer options shown to the user e.g. "A) ...", "B) ..."
        // For true/false questions this will just have two entries
        public List<string> Options { get; set; }

        // The correct answer key e.g. "C" or "True"
        public string CorrectAnswer { get; set; }

        // Shown after the user answers to reinforce the lesson
        public string Explanation { get; set; }
    }

    // Manages the quiz game from start to finish
    // Tracks which question the user is on and their running score
    public class QuizManager
    {
        private List<QuizQuestion> _questions;
        private int _currentIndex = 0;
        private int _score = 0;
        private bool _isActive = false;

        // Random is used to shuffle questions each time the quiz starts
        private readonly Random _random = new Random();

        public QuizManager()
        {
            _questions = BuildQuestions();
        }

        public bool IsActive => _isActive;

        // Current question/progress, exposed for the Quiz tab UI
        public QuizQuestion CurrentQuestion => _isActive ? _questions[_currentIndex] : null;
        public int CurrentNumber => _currentIndex + 1;
        public int TotalQuestions => Math.Min(10, _questions.Count);
        public int Score => _score;

        // Resets the quiz and returns the first question
        // Starts the quiz without building a formatted string - used by the Quiz tab UI
        public void StartSilent()
        {
            Shuffle(_questions);
            _currentIndex = 0;
            _score = 0;
            _isActive = true;
        }

        public string Start()
        {
            // Shuffle so the order is different each time
            Shuffle(_questions);

            _currentIndex = 0;
            _score = 0;
            _isActive = true;

            return "Cybersecurity Quiz started! You will get 10 questions.\n"
                 + "Type the letter of your answer (A, B, C, D) or True/False.\n\n"
                 + FormatQuestion();
        }

        // Called by ChatBot when the quiz is active and the user sends a message
        // Returns feedback on the answer plus the next question, or the final score
        public class AnswerResult
        {
            public bool Correct;
            public string Explanation;
            public bool QuizEnded;
            public int FinalScore;
            public int FinalTotal;
        }

        // Structured version of SubmitAnswer for the Quiz tab UI
        public AnswerResult SubmitAnswerStructured(string input)
        {
            if (!_isActive) return null;

            QuizQuestion current = _questions[_currentIndex];
            string answer = input.Trim().ToUpper();
            if (answer.Length > 1 && (answer[1] == ')' || answer[1] == '.'))
                answer = answer.Substring(0, 1);

            bool correct = answer == current.CorrectAnswer.ToUpper();
            if (correct) _score++;

            var result = new AnswerResult { Correct = correct, Explanation = current.Explanation };

            _currentIndex++;
            if (_currentIndex >= Math.Min(10, _questions.Count))
            {
                _isActive = false;
                result.QuizEnded = true;
                result.FinalScore = _score;
                result.FinalTotal = Math.Min(10, _questions.Count);
            }

            return result;
        }

        public string SubmitAnswer(string input)
        {
            if (!_isActive)
                return null;

            QuizQuestion current = _questions[_currentIndex];

            // Normalise the input for comparison
            string answer = input.Trim().ToUpper();

            // Strip common prefixes like "A)" or "A." so "A) Reply" also works
            if (answer.Length > 1 && (answer[1] == ')' || answer[1] == '.'))
                answer = answer.Substring(0, 1);

            bool correct = answer == current.CorrectAnswer.ToUpper();

            if (correct)
                _score++;

            var sb = new StringBuilder();

            // Tell the user if they were right or wrong
            sb.AppendLine(correct
                ? "Correct! Well done."
                : "Incorrect. The correct answer was " + current.CorrectAnswer + ".");

            // Always show the explanation to reinforce the lesson
            sb.AppendLine("Explanation: " + current.Explanation);
            sb.AppendLine();

            _currentIndex++;

            // Check if we have reached the end of the quiz (10 questions max)
            if (_currentIndex >= Math.Min(10, _questions.Count))
            {
                _isActive = false;
                sb.AppendLine(BuildFinalScore());
            }
            else
            {
                // Show the next question
                sb.AppendLine(FormatQuestion());
            }

            return sb.ToString();
        }

        // Formats the current question with its options and question number
        private string FormatQuestion()
        {
            QuizQuestion q = _questions[_currentIndex];
            int number = _currentIndex + 1;

            var sb = new StringBuilder();
            sb.AppendLine("Question " + number + " of " + Math.Min(10, _questions.Count) + ":");
            sb.AppendLine(q.Question);
            sb.AppendLine();

            foreach (string option in q.Options)
                sb.AppendLine(option);

            return sb.ToString();
        }

        // Builds the final score message with feedback based on percentage
        private string BuildFinalScore()
        {
            int total = Math.Min(10, _questions.Count);
            int percent = (_score * 100) / total;

            string feedback;
            if (percent >= 80)
                feedback = "Great job! You are a cybersecurity pro!";
            else if (percent >= 50)
                feedback = "Good effort! Keep learning to stay safe online.";
            else
                feedback = "Keep learning to stay safe online. Try the quiz again!";

            return "Quiz complete! Your score: " + _score + "/" + total
                 + " (" + percent + "%)\n" + feedback;
        }

        // All questions are defined here in one place for easy editing
        private List<QuizQuestion> BuildQuestions()
        {
            return new List<QuizQuestion>
            {
                new QuizQuestion
                {
                    Question = "What should you do if you receive an email asking for your password?",
                    Options = new List<string> { "A) Reply with your password", "B) Delete the email", "C) Report the email as phishing", "D) Ignore it" },
                    CorrectAnswer = "C",
                    Explanation = "You should report phishing emails. Legitimate organisations never ask for your password by email."
                },
                new QuizQuestion
                {
                    Question = "How long should a strong password be?",
                    Options = new List<string> { "A) 4 characters", "B) 6 characters", "C) 8 characters", "D) At least 12 characters" },
                    CorrectAnswer = "D",
                    Explanation = "Passwords of at least 12 characters are much harder to crack through brute force attacks."
                },
                new QuizQuestion
                {
                    Question = "What does 2FA stand for?",
                    Options = new List<string> { "A) Two-Factor Authentication", "B) Two-File Access", "C) Twice-Fast Authorization", "D) Two-Form Application" },
                    CorrectAnswer = "A",
                    Explanation = "Two-Factor Authentication adds a second step to login, making it harder for attackers to access your account even with your password."
                },
                new QuizQuestion
                {
                    Question = "Is it safe to use public Wi-Fi for online banking?",
                    Options = new List<string> { "True", "False" },
                    CorrectAnswer = "FALSE",
                    Explanation = "Public Wi-Fi is often unencrypted. Attackers on the same network can intercept your traffic. Use a VPN or mobile data instead."
                },
                new QuizQuestion
                {
                    Question = "What is phishing?",
                    Options = new List<string> { "A) A type of malware", "B) A trick to steal personal information via fake messages", "C) A way to speed up your internet", "D) A firewall setting" },
                    CorrectAnswer = "B",
                    Explanation = "Phishing uses fake emails, messages, or websites to trick users into giving up personal information like passwords or card numbers."
                },
                new QuizQuestion
                {
                    Question = "You should use the same password for all your accounts to make it easier to remember.",
                    Options = new List<string> { "True", "False" },
                    CorrectAnswer = "FALSE",
                    Explanation = "Reusing passwords means that if one account is breached, all your accounts become vulnerable. Use a password manager instead."
                },
                new QuizQuestion
                {
                    Question = "What is ransomware?",
                    Options = new List<string> { "A) Software that speeds up your PC", "B) A firewall tool", "C) Malware that encrypts your files and demands payment", "D) A type of antivirus" },
                    CorrectAnswer = "C",
                    Explanation = "Ransomware locks your files and demands payment to restore access. Regular offline backups are the best defence."
                },
                new QuizQuestion
                {
                    Question = "A VPN helps protect your privacy online.",
                    Options = new List<string> { "True", "False" },
                    CorrectAnswer = "TRUE",
                    Explanation = "A VPN encrypts your internet traffic, hiding your activity from your ISP and others on the same network."
                },
                new QuizQuestion
                {
                    Question = "Which of these is the safest way to store passwords?",
                    Options = new List<string> { "A) Write them in a notebook", "B) Use the same password everywhere", "C) Use a password manager", "D) Save them in a text file on your desktop" },
                    CorrectAnswer = "C",
                    Explanation = "A password manager generates and stores strong unique passwords securely so you only need to remember one master password."
                },
                new QuizQuestion
                {
                    Question = "What should you do before clicking a link in an email?",
                    Options = new List<string> { "A) Click it immediately", "B) Check the sender address and hover over the link to see where it goes", "C) Forward it to a friend", "D) Reply asking if it is real" },
                    CorrectAnswer = "B",
                    Explanation = "Always verify the sender and check the actual URL before clicking. Phishing links often look legitimate but lead to fake websites."
                },
                new QuizQuestion
                {
                    Question = "Keeping your operating system updated is important for security.",
                    Options = new List<string> { "True", "False" },
                    CorrectAnswer = "TRUE",
                    Explanation = "Updates patch known security vulnerabilities that attackers actively exploit. Always install updates promptly."
                },
                new QuizQuestion
                {
                    Question = "What is social engineering?",
                    Options = new List<string> { "A) Building social media apps", "B) Manipulating people into giving up confidential information", "C) A type of encryption", "D) A network protocol" },
                    CorrectAnswer = "B",
                    Explanation = "Social engineering exploits human psychology rather than technical vulnerabilities to gain unauthorised access to systems or data."
                }
            };
        }

        // Fisher-Yates shuffle - gives every question an equal chance of appearing
        private void Shuffle(List<QuizQuestion> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                var temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }
    }
}
