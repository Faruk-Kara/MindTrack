using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Configuration;

namespace MindTrack
{
    public partial class ChatForm : Form
    {
        private RichTextBox chatBox;
        private TextBox inputBox;
        private Button sendButton;
        private Button clearButton;
        private List<ChatMessage> messageHistory;
        private bool isProcessing;

        public ChatForm()
        {
            InitializeComponent();
            InitializeChatForm();
            messageHistory = new List<ChatMessage>();
        }

        private void InitializeChatForm()
        {
            this.Text = "AI Asistan";
            this.Size = new Size(800, 600);
            this.MinimumSize = new Size(600, 400);
            this.BackColor = Color.FromArgb(240, 240, 240);

            // Chat Display
            chatBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.None,
                Margin = new Padding(10)
            };

            // Input Panel
            var inputPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            // Input Box
            inputBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle,
                Multiline = true
            };
            inputBox.KeyPress += InputBox_KeyPress;

            // Send Button
            sendButton = new Button
            {
                Text = "Gönder",
                Size = new Size(80, 40),
                Dock = DockStyle.Right,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                Margin = new Padding(5, 0, 0, 0)
            };
            sendButton.Click += SendButton_Click;

            // Clear Button
            clearButton = new Button
            {
                Text = "Temizle",
                Size = new Size(80, 40),
                Dock = DockStyle.Right,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                Margin = new Padding(5, 0, 0, 0)
            };
            clearButton.Click += ClearButton_Click;

            inputPanel.Controls.AddRange(new Control[] { inputBox, clearButton, sendButton });

            // Main Container
            var container = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };
            container.Controls.Add(chatBox);

            this.Controls.AddRange(new Control[] { container, inputPanel });
        }

        private async void SendButton_Click(object sender, EventArgs e)
        {
            await SendMessage();
        }

        private async void InputBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter && !ModifierKeys.HasFlag(Keys.Shift))
            {
                e.Handled = true;
                await SendMessage();
            }
        }

        private async Task SendMessage()
        {
            if (isProcessing || string.IsNullOrWhiteSpace(inputBox.Text))
                return;

            isProcessing = true;
            sendButton.Enabled = false;
            inputBox.Enabled = false;

            try
            {
                string userMessage = inputBox.Text.Trim();
                inputBox.Clear();

                AppendMessage("Sen", userMessage, Color.FromArgb(52, 152, 219));

                // Read endpoints from config
                string aiEndpoint = ConfigurationManager.AppSettings["LmStudioEndpoint"] ?? "http://127.0.0.1:1234/v1/chat/completions";

                // Try to translate user message to English, fallback to original if translation fails
                string translatedInput = userMessage;
                try
                {
                    string translated = await TranslateService.TranslateToEnglishAsync(userMessage);
                    if (!translated.StartsWith("[Çeviri Hatası]"))
                    {
                        translatedInput = translated;
                    }
                }
                catch (Exception)
                {
                    // Use original message if translation fails
                    AppendMessage("Sistem", "Çeviri servisi kullanılamıyor, orijinal metin kullanılıyor.", Color.Orange);
                }

                // Get AI response
                string aiResponse = await LmStudioService.SendMessageAsync(translatedInput, aiEndpoint);
                if (aiResponse.StartsWith("[AI Error]"))
                {
                    AppendMessage("Sistem", "AI servis hatası. Lütfen LM Studio'nun localhost:1234'te çalıştığından emin olun", Color.Red);
                    return;
                }

                // Try to translate AI response to Turkish, fallback to original if translation fails
                string translatedResponse = aiResponse;
                try
                {
                    string translated = await TranslateService.TranslateToTurkishAsync(aiResponse);
                    if (!translated.StartsWith("[Çeviri Hatası]"))
                    {
                        translatedResponse = translated;
                    }
                }
                catch (Exception)
                {
                    // Use original response if translation fails
                }

                AppendMessage("AI Asistan", translatedResponse, Color.FromArgb(46, 204, 113));

                messageHistory.Add(new ChatMessage { Sender = "Sen", Content = userMessage, Timestamp = DateTime.Now });
                messageHistory.Add(new ChatMessage { Sender = "AI", Content = translatedResponse, Timestamp = DateTime.Now });
            }
            catch (Exception ex)
            {
                AppendMessage("Sistem", $"Hata: {ex.Message}", Color.Red);
            }
            finally
            {
                isProcessing = false;
                sendButton.Enabled = true;
                inputBox.Enabled = true;
                inputBox.Focus();
            }
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            chatBox.Clear();
            messageHistory.Clear();
        }

        private void AppendMessage(string sender, string message, Color color)
        {
            chatBox.SelectionStart = chatBox.TextLength;
            chatBox.SelectionLength = 0;

            chatBox.SelectionColor = color;
            chatBox.AppendText($"{sender}: ");
            chatBox.SelectionColor = Color.Black;
            chatBox.AppendText($"{message}\n\n");
            chatBox.ScrollToCaret();
        }
    }

    public class ChatMessage
    {
        public string Sender { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
    }
}