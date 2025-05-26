using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Threading.Tasks;
using System.Text;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;

namespace MindTrack
{
    public partial class AdviceForm : Form
    {
        private RichTextBox adviceBox;
        private Button btnRefresh;
        private Label statusLabel;
        private readonly string connectionString;
        private readonly bool justMood;

        public AdviceForm(bool justMood = false)
        {
            InitializeComponent();
            this.justMood = justMood;
            connectionString = $"Data Source={System.IO.Path.Combine(Application.StartupPath, "Database", "mindtrack.db")};Version=3;";
            InitializeUI();
            LoadAdviceAsync();
        }

        private void InitializeUI()
        {
            this.Text = "Günün Tavsiyeleri";
            this.Size = new Size(600, 400);
            this.MinimumSize = new Size(400, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;

            // Status Label
            statusLabel = new Label
            {
                Text = "Kişiselleştirilmiş tavsiyeleriniz yükleniyor...",
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray
            };

            // Advice Display
            adviceBox = new RichTextBox
            {
                Location = new Point(20, 50),
                Size = new Size(540, 250),
                ReadOnly = true,
                Font = new Font("Segoe UI", 11),
                BackColor = Color.FromArgb(250, 250, 250),
                BorderStyle = BorderStyle.None
            };

            // Refresh Button
            btnRefresh = new Button
            {
                Text = "Yeni Tavsiye Al",
                Location = new Point(20, 320),
                Size = new Size(120, 30),
                BackColor = Color.FromArgb(0, 120, 212),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnRefresh.Click += BtnRefresh_Click;

            this.Controls.AddRange(new Control[] {
                statusLabel, adviceBox, btnRefresh
            });
        }

        private async void BtnRefresh_Click(object sender, EventArgs e)
        {
            await LoadAdviceAsync();
        }

        private async Task LoadAdviceAsync()
        {
            try
            {
                btnRefresh.Enabled = false;
                statusLabel.Text = justMood ? "Ruh haliniz analiz ediliyor..." : "Verileriniz analiz ediliyor...";

                // Get user's recent mood and tasks
                var (mood, tasks) = await GetUserDataAsync();

                // Prepare the task list for the AI function
                var taskList = new List<string>();
                if (!justMood)
                {
                    foreach (var line in tasks.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        // Remove "- " and status in parentheses for a cleaner list
                        var clean = line.TrimStart('-', ' ').Split('(')[0].Trim();
                        if (!string.IsNullOrWhiteSpace(clean))
                            taskList.Add(clean);
                    }
                }

                statusLabel.Text = justMood ? "Ruh hali tabanlı tavsiyeler oluşturuluyor..." : "Kişiselleştirilmiş tavsiyeler oluşturuluyor...";
                string aiResponse = justMood
                    ? await MindTrackAI.GetSupportiveReflectionForMoodAsync(mood)
                    : await MindTrackAI.GetSupportiveReflectionAsync(mood, taskList);

                if (!string.IsNullOrEmpty(aiResponse))
                {
                    adviceBox.Text = FormatAIResponse(aiResponse);
                    statusLabel.Text = "Tavsiyeler güncellendi " + DateTime.Now.ToString("HH:mm:ss");
                }
                else
                {
                    adviceBox.Text = "Şu anda tavsiye oluşturulamadı. Lütfen daha sonra tekrar deneyin.";
                    statusLabel.Text = "Tavsiye oluşturma hatası";
                }
            }
            catch (Exception ex)
            {
                adviceBox.Text = $"Hata: {ex.Message}";
                statusLabel.Text = "Hata oluştu";
            }
            finally
            {
                btnRefresh.Enabled = true;
            }
        }

        private async Task<(string mood, string tasks)> GetUserDataAsync()
        {
            string mood = "nötr";
            var tasks = new StringBuilder();

            using (var conn = new SQLiteConnection(connectionString))
            {
                await conn.OpenAsync();

                // Get latest mood
                using (var cmd = new SQLiteCommand(
                    "SELECT Mood FROM MoodEntries ORDER BY Timestamp DESC LIMIT 1",
                    conn))
                {
                    var result = await cmd.ExecuteScalarAsync();
                    if (result != null && result != DBNull.Value)
                    {
                        mood = result.ToString();
                    }
                }

                // Get today's tasks
                using (var cmd = new SQLiteCommand(
                    "SELECT Title, Status FROM Tasks WHERE Date = @today",
                    conn))
                {
                    cmd.Parameters.AddWithValue("@today", DateTime.Today.ToString("yyyy-MM-dd"));
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            tasks.AppendLine($"- {reader["Title"]} ({reader["Status"]})");
                        }
                    }
                }
            }

            return (mood, tasks.ToString());
        }

        private string FormatAIResponse(string response)
        {
            // Basic formatting to ensure readability
            return response.Replace("\n\n", "\n")
                         .Replace("1.", "\n1.")
                         .Replace("2.", "\n2.")
                         .Replace("3.", "\n3.")
                         .Replace("4.", "\n4.");
        }
    }

    public static class MindTrackAI
    {
        public static async Task<string> GetSupportiveReflectionAsync(string userMood, List<string> tasks, string endpoint = "http://localhost:1234/v1/chat/completions")
        {
            string prompt = $@"
Sen MindTrack adında destekleyici ve anlayışlı bir asistansın.

Görevin, kullanıcının duygusal durumunu ve bugün için planladığı görevleri analiz etmek, ardından bu bağlama dayalı kısa bir yansıma, tavsiye ve cesaret verici sözler sunmak.

Alacağın bilgiler:
1. Kullanıcının mevcut **ruh hali** (kullanıcı tarafından seçilen veya yazılan).
2. Bugün için planlanan **görevler veya hedefler** listesi.

Bu bilgileri kullanarak şunları oluştur:
- Duygusal durumu bağlam içinde yorumlayan kısa bir özet (1-2 cümle).
- Yapıcı destek veya tavsiye sunan 2-3 cümle.
- Kullanıcıyı cesaretlendiren 1 motivasyon cümlesi.

Takip soruları **sorma** veya kullanıcıyı tekrar yönlendirme. Sadece doğal dilde tam yanıtı döndür.

İşte girdi:
- Ruh Hali: {userMood}
- Görevler: {string.Join(", ", tasks)}

Destekleyici yansımanı aşağıda başlat.
";

            var payload = new
            {
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            using (var client = new HttpClient())
            {
                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(endpoint, content);
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();
                dynamic obj = JsonConvert.DeserializeObject(result);
                return obj.choices[0].message.content.ToString();
            }
        }

        public static async Task<string> GetSupportiveReflectionForMoodAsync(string userMood, string endpoint = "http://localhost:1234/v1/chat/completions")
        {
            string prompt = $@"
Sen MindTrack adında destekleyici ve anlayışlı bir asistansın.

Görevin, kullanıcının duygusal durumunu analiz etmek ve bu bağlama dayalı kısa bir yansıma, tavsiye ve cesaret verici sözler sunmak.

Alacağın bilgi:
1. Kullanıcının mevcut **ruh hali** (kullanıcı tarafından seçilen veya yazılan).

Bu bilgiyi kullanarak şunları oluştur:
- Duygusal durumu yorumlayan kısa bir özet (1-2 cümle).
- Yapıcı destek veya tavsiye sunan 2-3 cümle.
- Kullanıcıyı cesaretlendiren 1 motivasyon cümlesi.

Takip soruları **sorma** veya kullanıcıyı tekrar yönlendirme. Sadece doğal dilde tam yanıtı döndür.

İşte girdi:
- Ruh Hali: {userMood}

Destekleyici yansımanı aşağıda başlat.
";

            var payload = new
            {
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            using (var client = new HttpClient())
            {
                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(endpoint, content);
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();
                dynamic obj = JsonConvert.DeserializeObject(result);
                return obj.choices[0].message.content.ToString();
            }
        }
    }
}