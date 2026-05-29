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

            _chatBot = new ChatBot();
            _chatBot.OnResponseReady += AppendBotMessage;
            _chatBot.OnSentimentChanged += UpdateSentimentLabel;

            LoadLogo();
            LoadAsciiArt();
            PlayVoiceGreeting();

            AppendBotMessage(_chatBot.GetGreeting());
            UserInput.Focus();
        }

        // ── Event handlers ─────────────────────────────────────────

        private void SendButton_Click(object sender, RoutedEventArgs e) => SendMessage();

        private void UserInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) SendMessage();
        }

        private void Chip_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string text)
            {
                UserInput.Text = text;
                SendMessage();
            }
        }

        // ── Send ───────────────────────────────────────────────────

        private void SendMessage()
        {
            string text = UserInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(text)) return;

            AppendUserMessage(text);
            UserInput.Clear();
            StatusBar.Text = "Thinking...";
            _chatBot.ProcessInput(text);
            StatusBar.Text = "Ask me anything about cybersecurity.";
        }

        // ── Delegate callbacks ─────────────────────────────────────

        private void AppendBotMessage(string message)
        {
            Dispatcher.Invoke(() =>
            {
                var bubble = BuildBubble(message, "CyberBot", BotNameFg, BotBubbleBg, false);
                ChatPanel.Children.Add(bubble);

                bubble.Opacity = 0;
                bubble.BeginAnimation(UIElement.OpacityProperty,
                    new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200)));

                ScrollToBottom();
            });
        }

        private void AppendUserMessage(string message)
        {
            ChatPanel.Children.Add(BuildBubble(message, "You", UserNameFg, UserBubbleBg, true));
            ScrollToBottom();
        }

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

        // ── Bubble builder ─────────────────────────────────────────

        private Border BuildBubble(string text, string senderLabel,
            SolidColorBrush senderColour, SolidColorBrush bubbleBg, bool alignRight)
        {
            var panel = new StackPanel();

            panel.Children.Add(new TextBlock
            {
                Text = senderLabel,
                Foreground = senderColour,
                FontWeight = FontWeights.SemiBold,
                FontSize = 11,
                Margin = new Thickness(0, 0, 0, 4)
            });

            var body = new TextBlock
            {
                Foreground = TextColour,
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 20
            };
            RenderBold(body, text);
            panel.Children.Add(body);

            panel.Children.Add(new TextBlock
            {
                Text = DateTime.Now.ToString("HH:mm"),
                Foreground = MutedColour,
                FontSize = 10,
                Margin = new Thickness(0, 4, 0, 0),
                HorizontalAlignment = alignRight ? HorizontalAlignment.Right : HorizontalAlignment.Left
            });

            return new Border
            {
                Background = bubbleBg,
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(12, 8, 12, 8),
                Margin = new Thickness(alignRight ? 60 : 0, 4, alignRight ? 0 : 60, 4),
                HorizontalAlignment = alignRight ? HorizontalAlignment.Right : HorizontalAlignment.Left,
                MaxWidth = 580,
                BorderBrush = BubbleBorder,
                BorderThickness = new Thickness(1),
                Child = panel
            };
        }

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

        // ── Startup helpers ────────────────────────────────────────

        
        
        private void LoadLogo()
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;

                string[] candidates = new[]
                {
                    Path.Combine(baseDir, "logo.png"),
                    Path.Combine(baseDir, "logo.jpg"),
                    Path.Combine(baseDir, "logo.bmp"),
                    Path.Combine(baseDir, "Assets", "logo.png"),
                    Path.Combine(baseDir, "Assets", "logo.jpg")
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
            catch {  }
        }

        /// <summary>
        /// Plays greeting.wav from the application folder if it exists.
        /// </summary>
        private void PlayVoiceGreeting()
        {
            try
            {
                string path = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Resources",
                    "greeting.wav");

                if (File.Exists(path))
                {
                    SoundPlayer player = new SoundPlayer(path);
                    player.Load();
                    player.Play();
                }
                else
                {
                    MessageBox.Show("Audio file not found:\n" + path);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error playing audio:\n" + ex.Message);
            }
        }
        //Ascii art generated with https://patorjk.com/software/taag/#p=display&f=Big&t=CyberBot
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

        private void ScrollToBottom()
        {
            ChatScroll.UpdateLayout();
            ChatScroll.ScrollToBottom();
        }
    }
}