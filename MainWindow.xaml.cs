using System;
using System.IO;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace CybersecurityChatbot
{
   
    public partial class MainWindow : Window
    {
        
        private ChatBot _chatBot;

        

        private static readonly SolidColorBrush UserBubbleBg = new SolidColorBrush(Color.FromRgb(0x1A, 0x3A, 0x2A));
        private static readonly SolidColorBrush BotBubbleBg = new SolidColorBrush(Color.FromRgb(0x1C, 0x2A, 0x3A));
        private static readonly SolidColorBrush UserNameFg = new SolidColorBrush(Color.FromRgb(0x39, 0xD3, 0x53));
        private static readonly SolidColorBrush BotNameFg = new SolidColorBrush(Color.FromRgb(0x00, 0xD4, 0xFF));
        private static readonly SolidColorBrush TextColour = new SolidColorBrush(Color.FromRgb(0xE6, 0xED, 0xF3));
        private static readonly SolidColorBrush MutedColour = new SolidColorBrush(Color.FromRgb(0x8B, 0x94, 0x9E));
        private static readonly SolidColorBrush BubbleBorder = new SolidColorBrush(Color.FromRgb(0x30, 0x36, 0x3D));

        public MainWindow()
        {
            InitializeComponent();

            // Create ChatBot and subscribe to its events
            // ChatBot fires OnResponseReady when it has a message for the user
            // ChatBot fires OnSentimentChanged when mood is detected
            _chatBot = new ChatBot();
            _chatBot.OnResponseReady += AppendBotMessage;
            _chatBot.OnSentimentChanged += UpdateSentimentLabel;

            // Load visual assets
            LoadLogo();
            LoadAsciiArt();
            PlayVoiceGreeting();

            // Show the opening greeting message
            AppendBotMessage(_chatBot.GetGreeting());
            AppendBotMessage(_chatBot.GetNamePrompt());
            RefreshTasksPanel();
            UserInput.Focus();
        }

        // Triggered when the Send button is clicked
        private void SendButton_Click(object sender, RoutedEventArgs e) => SendMessage();

        // Triggered when a key is pressed in the input box
        // Enter key sends the message without needing the Send button
        private void UserInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) SendMessage();
        }

        // Triggered when a quick-topic chip button is clicked
        // The Tag property of each button holds the text to send
        private void Chip_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string text)
            {
                UserInput.Text = text;
                SendMessage();
            }
        }

        // Triggered when the Add Task button is clicked on the Tasks tab
        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            string title = TaskTitleInput.Text.Trim();
            string reminder = TaskReminderInput.Text.Trim();

            if (string.IsNullOrWhiteSpace(title))
            {
                StatusBar.Text = "Please enter a task title.";
                return;
            }

            // Build a command string and send it through the chatbot
            // This keeps the task logic in ChatBot/TaskAssistant, not here
            string command = "add task - " + title;
            if (!string.IsNullOrWhiteSpace(reminder))
                command += " remind me in " + reminder;

            TaskTitleInput.Clear();
            TaskReminderInput.Clear();

            AppendUserMessage(command);
            _chatBot.ProcessInput(command);

            // If a reminder was provided, auto-confirm it
            if (!string.IsNullOrWhiteSpace(reminder))
                _chatBot.ProcessInput(reminder);

            // Refresh the task list panel
            RefreshTasksPanel();
            StatusBar.Text = "Task added.";
        }

        // Triggered when the Refresh Tasks button is clicked
        private void RefreshTasks_Click(object sender, RoutedEventArgs e)
        {
            RefreshTasksPanel();
        }

        // Rebuilds the task list display in the Tasks tab
        private void RefreshTasksPanel()
        {
            TaskListPanel.Children.Clear();

            if (_chatBot.DatabaseErrorMessage != null && _chatBot.GetAllTasks().Count == 0)
            {
                TaskListPanel.Children.Add(new TextBlock
                {
                    Text = "Task storage is unavailable.\nDatabase error: " + _chatBot.DatabaseErrorMessage,
                    TextWrapping = TextWrapping.Wrap,
                    FontSize = 13,
                    Foreground = new SolidColorBrush(Color.FromRgb(0xFF, 0x7B, 0x72))
                });
                return;
            }

            var tasks = _chatBot.GetAllTasks();

            if (tasks.Count == 0)
            {
                TaskListPanel.Children.Add(new TextBlock
                {
                    Text = "You have no tasks yet. Add one above to get started.",
                    TextWrapping = TextWrapping.Wrap,
                    FontSize = 13,
                    Foreground = MutedColour
                });
                return;
            }

            var table = new Grid();
            table.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
            table.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90) });
            table.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            table.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });
            table.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) });
            table.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(160) });

            AddHeaderRow(table);

            for (int i = 0; i < tasks.Count; i++)
                AddTaskRow(table, tasks[i], i + 1);

            TaskListPanel.Children.Add(table);
        }

        // Adds the bold column-title row at the top of the task table
        private void AddHeaderRow(Grid table)
        {
            table.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            int row = table.RowDefinitions.Count - 1;

            string[] headers = { "ID", "Status", "Title", "Description", "Reminder", "Actions" };
            for (int col = 0; col < headers.Length; col++)
            {
                var tb = new TextBlock
                {
                    Text = headers[col],
                    FontWeight = FontWeights.Bold,
                    FontSize = 12,
                    Margin = new Thickness(4, 6, 4, 6),
                    Foreground = TextColour
                };
                Grid.SetRow(tb, row);
                Grid.SetColumn(tb, col);
                table.Children.Add(tb);
            }

            var divider = new Border
            {
                BorderBrush = BubbleBorder,
                BorderThickness = new Thickness(0, 0, 0, 1)
            };
            Grid.SetRow(divider, row);
            Grid.SetColumnSpan(divider, headers.Length);
            table.Children.Add(divider);
        }

        // Adds one row to the task table for a single task, with Complete/Delete buttons
        private void AddTaskRow(Grid table, CyberTask task, int displayNumber)
        {
            table.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            int row = table.RowDefinitions.Count - 1;

            var idText = new TextBlock { Text = task.Id.ToString(), Margin = new Thickness(4, 8, 4, 8), Foreground = TextColour };

            var statusText = new TextBlock
            {
                Text = task.IsCompleted ? "Done" : "Pending",
                Margin = new Thickness(4, 8, 4, 8),
                Foreground = task.IsCompleted
                    ? new SolidColorBrush(Color.FromRgb(0x39, 0xD3, 0x53))
                    : new SolidColorBrush(Color.FromRgb(0xF7, 0x81, 0x66))
            };

            var titleText = new TextBlock { Text = task.Title, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(4, 8, 4, 8), Foreground = TextColour };
            var descText = new TextBlock { Text = task.Description, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(4, 8, 4, 8), Foreground = MutedColour, FontSize = 12 };
            var reminderText = new TextBlock { Text = string.IsNullOrWhiteSpace(task.Reminder) ? "-" : task.Reminder, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(4, 8, 4, 8), Foreground = TextColour };

            var actionsPanel = new StackPanel { Orientation = Orientation.Horizontal };
            var completeBtn = new Button { Content = "Complete", Margin = new Thickness(0, 4, 6, 4), Padding = new Thickness(6, 2, 6, 2), Tag = task.Id, IsEnabled = !task.IsCompleted };
            completeBtn.Click += (s, e) => { _chatBot.ProcessInput("complete task " + task.Id); RefreshTasksPanel(); };
            var deleteBtn = new Button { Content = "Delete", Margin = new Thickness(0, 4, 0, 4), Padding = new Thickness(6, 2, 6, 2), Tag = task.Id };
            deleteBtn.Click += (s, e) => { _chatBot.ProcessInput("delete task " + task.Id); RefreshTasksPanel(); };
            actionsPanel.Children.Add(completeBtn);
            actionsPanel.Children.Add(deleteBtn);

            var cells = new UIElement[] { idText, statusText, titleText, descText, reminderText, actionsPanel };
            for (int col = 0; col < cells.Length; col++)
            {
                Grid.SetRow(cells[col], row);
                Grid.SetColumn(cells[col], col);
                table.Children.Add(cells[col]);
            }

            var divider = new Border { BorderBrush = BubbleBorder, BorderThickness = new Thickness(0, 0, 0, 1) };
            Grid.SetRow(divider, row);
            Grid.SetColumnSpan(divider, cells.Length);
            table.Children.Add(divider);
        }

        // Quiz tab handlers

        private void StartQuiz_Click(object sender, RoutedEventArgs e)
        {
            _chatBot.Quiz.StartSilent();
            ShowCurrentQuestion();
            StatusBar.Text = "Quiz started.";
        }

        private void NextQuestion_Click(object sender, RoutedEventArgs e)
        {
            NextQuestionButton.Visibility = Visibility.Collapsed;
            QuizFeedbackText.Text = "";
            ShowCurrentQuestion();
        }

        // Builds the option buttons and question text for the current quiz question
        private void ShowCurrentQuestion()
        {
            var quiz = _chatBot.Quiz;

            if (!quiz.IsActive)
            {
                QuizQuestionText.Text = "";
                QuizOptionsPanel.Children.Clear();
                QuizProgressText.Text = "Click Start Quiz to begin.";
                return;
            }

            var q = quiz.CurrentQuestion;
            QuizProgressText.Text = "Question " + quiz.CurrentNumber + " of " + quiz.TotalQuestions
                                   + "  |  Score: " + quiz.Score;
            QuizQuestionText.Text = q.Question;

            QuizOptionsPanel.Children.Clear();
            foreach (string option in q.Options)
            {
                var btn = new Button
                {
                    Content = option,
                    Tag = option,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(0, 0, 0, 6),
                    Padding = new Thickness(10, 6, 10, 6)
                };
                btn.Click += QuizOption_Click;
                QuizOptionsPanel.Children.Add(btn);
            }
        }

        // Triggered when the user clicks an answer option button
        private void QuizOption_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn) || !(btn.Tag is string optionText)) return;

            // Use the letter prefix (e.g. "A") if present, otherwise the full text (True/False)
            string answer = optionText.Length > 1 && optionText[1] == ')'
                ? optionText.Substring(0, 1)
                : optionText;

            var result = _chatBot.Quiz.SubmitAnswerStructured(answer);
            if (result == null) return;

            QuizOptionsPanel.Children.Clear();

            QuizFeedbackText.Text = (result.Correct ? "Correct! " : "Incorrect. ")
                                   + result.Explanation;
            QuizFeedbackText.Foreground = result.Correct
                ? new SolidColorBrush(Color.FromRgb(0x39, 0xD3, 0x53))
                : new SolidColorBrush(Color.FromRgb(0xFF, 0x7B, 0x72));

            if (result.QuizEnded)
            {
                int percent = (result.FinalScore * 100) / result.FinalTotal;
                QuizProgressText.Text = "Quiz complete! Score: " + result.FinalScore
                                       + "/" + result.FinalTotal + " (" + percent + "%)";
                NextQuestionButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                NextQuestionButton.Visibility = Visibility.Visible;
            }
        }

        private void SendMessage()
        {
            string text = UserInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(text)) return;

            AppendUserMessage(text);
            UserInput.Clear();
            StatusBar.Text = "Thinking...";

            // Pass the input to ChatBot - the response comes back via the event
            _chatBot.ProcessInput(text);

            StatusBar.Text = "Ask me anything about cybersecurity.";
        }

        // Called by ChatBot.OnResponseReady event
        // Dispatcher.Invoke ensures this runs on the UI thread
        private void AppendBotMessage(string message)
        {
            Dispatcher.Invoke(() =>
            {
                var bubble = BuildBubble(message, "CyberBot", BotNameFg, BotBubbleBg, false);
                ChatPanel.Children.Add(bubble);

                // Fade-in animation for a smooth appearance
                bubble.Opacity = 0;
                bubble.BeginAnimation(UIElement.OpacityProperty,
                    new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200)));

                ScrollToBottom();
            });
        }

        // Adds a user message bubble to the chat panel
        private void AppendUserMessage(string message)
        {
            ChatPanel.Children.Add(BuildBubble(message, "You", UserNameFg, UserBubbleBg, true));
            ScrollToBottom();
        }

        // Called by ChatBot.OnSentimentChanged event
        // Updates the mood label in the header
        private void UpdateSentimentLabel(Sentiment sentiment)
        {
            Dispatcher.Invoke(() =>
            {
                string label;
                string hex;

                if (sentiment == Sentiment.Worried) { label = "Worried"; hex = "#F78166"; }
                else if (sentiment == Sentiment.Frustrated) { label = "Frustrated"; hex = "#FF7B72"; }
                else if (sentiment == Sentiment.Curious) { label = "Curious"; hex = "#79C0FF"; }
                else if (sentiment == Sentiment.Happy) { label = "Happy"; hex = "#39D353"; }
                else { label = "Neutral"; hex = "#39D353"; }

                SentimentLabel.Text = label;
                SentimentLabel.Foreground = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString(hex));
            });
        }

        
        private Border BuildBubble(string text, string senderLabel,
            SolidColorBrush senderColour, SolidColorBrush bubbleBg, bool alignRight)
        {
            var panel = new StackPanel();

            // Sender label (e.g. "CyberBot" or "You")
            panel.Children.Add(new TextBlock
            {
                Text = senderLabel,
                Foreground = senderColour,
                FontWeight = FontWeights.SemiBold,
                FontSize = 11,
                Margin = new Thickness(0, 0, 0, 4)
            });

            // Message body - supports **bold** markdown formatting
            var body = new TextBlock
            {
                Foreground = TextColour,
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 20
            };
            RenderBold(body, text);
            panel.Children.Add(body);

            // Timestamp
            panel.Children.Add(new TextBlock
            {
                Text = DateTime.Now.ToString("HH:mm"),
                Foreground = MutedColour,
                FontSize = 10,
                Margin = new Thickness(0, 4, 0, 0),
                HorizontalAlignment = alignRight
                    ? HorizontalAlignment.Right
                    : HorizontalAlignment.Left
            });

            // Outer border that acts as the bubble shape
            return new Border
            {
                Background = bubbleBg,
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(12, 8, 12, 8),
                Margin = new Thickness(alignRight ? 60 : 0, 4, alignRight ? 0 : 60, 4),
                HorizontalAlignment = alignRight
                    ? HorizontalAlignment.Right
                    : HorizontalAlignment.Left,
                MaxWidth = 580,
                BorderBrush = BubbleBorder,
                BorderThickness = new Thickness(1),
                Child = panel
            };
        }

        // Parses **bold** markers and renders bold cyan text inline
        private static void RenderBold(TextBlock block, string text)
        {
            var parts = text.Split(new[] { "**" }, StringSplitOptions.None);
            for (int i = 0; i < parts.Length; i++)
            {
                if (string.IsNullOrEmpty(parts[i])) continue;
                if (i % 2 == 1)
                    block.Inlines.Add(new Bold(new Run(parts[i])
                    {
                        Foreground = new SolidColorBrush(Color.FromRgb(0x00, 0xD4, 0xFF))
                    }));
                else
                    block.Inlines.Add(new Run(parts[i]));
            }
        }

        // Loads the logo image - tries the embedded pack resource first (this is how
        // WPF "Resource" build action images are meant to be loaded), then falls back
        // to loose files on disk in case the build action gets changed later
        private void LoadLogo()
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri("pack://application:,,,/Resources/logo.png", UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.DecodePixelWidth = 36;
                bitmap.EndInit();
                LogoImage.Source = bitmap;
                return;
            }
            catch { }

            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string[] candidates = new[]
                {
                    Path.Combine(baseDir, "Resources", "logo.png"),
                    Path.Combine(baseDir, "logo.png"),
                    Path.Combine(baseDir, "Assets", "logo.png")
                };

                foreach (string path in candidates)
                {
                    if (File.Exists(path))
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(path, UriKind.Absolute);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.DecodePixelWidth = 36;
                        bitmap.EndInit();
                        LogoImage.Source = bitmap;
                        break;
                    }
                }
            }
            catch { } // logo is optional
        }

        // Plays greeting.wav from the application folder on startup
        private void PlayVoiceGreeting()
        {
            try
            {
                string path = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, "Resources", "greeting.wav");

                if (!File.Exists(path))
                    path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "greeting.wav");

                if (File.Exists(path))
                    new SoundPlayer(path).Play();
            }
            catch { }
        }

        // Sets the ASCII art text in the header TextBlock
        private void LoadAsciiArt()
        {
            AsciiArtBlock.Text =
                " ██████╗██╗   ██╗██████╗ ███████╗██████╗ \n" +
                "██╔════╝╚██╗ ██╔╝██╔══██╗██╔════╝██╔══██╗\n" +
                "██║      ╚████╔╝ ██████╔╝█████╗  ██████╔╝\n" +
                "██║       ╚██╔╝  ██╔══██╗██╔══╝  ██╔══██╗\n" +
                "╚██████╗   ██║   ██████╔╝███████╗██║  ██║\n" +
                " ╚═════╝   ╚═╝   ╚═════╝ ╚══════╝╚═╝  ╚═╝";
        }

        // Scrolls the chat area to the latest message
        private void ScrollToBottom()
        {
            ChatScroll.UpdateLayout();
            ChatScroll.ScrollToBottom();
        }
    }
}