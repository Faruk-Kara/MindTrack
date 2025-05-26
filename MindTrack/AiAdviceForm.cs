using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Threading.Tasks;
using System.Text;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using System.Linq;

namespace MindTrack
{
    /// <summary>
    /// Gelişmiş AI Tavsiye Formu - Modern tasarım ve çoklu analiz seçenekleri
    /// </summary>
    public partial class AiAdviceForm : Form
    {
        // Ana kontroller
        private Panel headerPanel;
        private Panel contentPanel;
        private Panel buttonPanel;
        private Panel sidePanel;
        
        // Header kontrolleri
        private Label titleLabel;
        private Label subtitleLabel;
        private PictureBox aiIcon;
        
        // Content kontrolleri
        private RichTextBox adviceDisplay;
        private ProgressBar loadingBar;
        private Label statusLabel;
        
        // Button kontrolleri
        private Button btnFullAnalysis;
        private Button btnMoodOnly;
        private Button btnTaskFocus;
        private Button btnMotivation;
        private Button btnSaveAdvice;
        private Button btnClose;
        
        // Side panel kontrolleri
        private GroupBox moodBox;
        private GroupBox taskBox;
        private Label currentMoodLabel;
        private Label taskCountLabel;
        private ListView recentTasksList;
        
        // Veri alanları
        private readonly DateTime selectedDate;
        private readonly string connectionString;
        private string lastAdvice = "";
        private string currentMood = "";
        private List<string> todayTasks = new List<string>();

        public AiAdviceForm(DateTime selectedDate)
        {
            InitializeComponent();
            this.selectedDate = selectedDate;
            connectionString = $"Data Source={System.IO.Path.Combine(Application.StartupPath, "Database", "mindtrack.db")};Version=3;";
            
            InitializeModernUI();
            LoadUserDataAsync();
        }

        /// <summary>
        /// Modern ve kullanıcı dostu arayüz tasarımı
        /// </summary>
        private void InitializeModernUI()
        {
            // Form ayarları
            this.Text = "MindTrack AI Asistan";
            this.Size = new Size(1000, 700);
            this.MinimumSize = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            CreateHeaderPanel();
            CreateSidePanel();
            CreateContentPanel();
            CreateButtonPanel();
            
            // Panelleri forma ekle
            this.Controls.Add(headerPanel);
            this.Controls.Add(sidePanel);
            this.Controls.Add(contentPanel);
            this.Controls.Add(buttonPanel);
        }

        /// <summary>
        /// Üst başlık panelini oluşturur
        /// </summary>
        private void CreateHeaderPanel()
        {
            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.FromArgb(0, 120, 212),
                Padding = new Padding(20, 10, 20, 10)
            };

            // AI ikonu
            aiIcon = new PictureBox
            {
                Size = new Size(50, 50),
                Location = new Point(20, 15),
                BackColor = Color.White,
                SizeMode = PictureBoxSizeMode.CenterImage
            };
            
            // Basit AI ikonu çizimi
            aiIcon.Paint += (s, e) =>
            {
                e.Graphics.FillEllipse(Brushes.LightBlue, 10, 10, 30, 30);
                e.Graphics.DrawString("AI", new Font("Segoe UI", 12, FontStyle.Bold), Brushes.DarkBlue, 17, 20);
            };

            // Ana başlık
            titleLabel = new Label
            {
                Text = "MindTrack AI Asistan",
                Location = new Point(80, 15),
                Size = new Size(300, 30),
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };

            // Alt başlık
            subtitleLabel = new Label
            {
                Text = $"Kişiselleştirilmiş analiz ve tavsiyeler - {selectedDate.ToString("dd MMMM yyyy")}",
                Location = new Point(80, 45),
                Size = new Size(400, 20),
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(220, 220, 220),
                BackColor = Color.Transparent
            };

            headerPanel.Controls.AddRange(new Control[] { aiIcon, titleLabel, subtitleLabel });
        }

        /// <summary>
        /// Yan bilgi panelini oluşturur
        /// </summary>
        private void CreateSidePanel()
        {
            sidePanel = new Panel
            {
                Dock = DockStyle.Right,
                Width = 280,
                BackColor = Color.White,
                Padding = new Padding(15)
            };

            // Ruh hali kutusu
            moodBox = new GroupBox
            {
                Text = "Mevcut Ruh Hali",
                Location = new Point(15, 15),
                Size = new Size(250, 80),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 51, 51)
            };

            currentMoodLabel = new Label
            {
                Text = "Yükleniyor...",
                Location = new Point(10, 25),
                Size = new Size(230, 40),
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.FromArgb(0, 120, 212),
                TextAlign = ContentAlignment.MiddleCenter,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(248, 249, 250)
            };

            moodBox.Controls.Add(currentMoodLabel);

            // Görev kutusu
            taskBox = new GroupBox
            {
                Text = "Bugünün Görevleri",
                Location = new Point(15, 105),
                Size = new Size(250, 300),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 51, 51)
            };

            taskCountLabel = new Label
            {
                Text = "Görevler yükleniyor...",
                Location = new Point(10, 25),
                Size = new Size(230, 25),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleCenter
            };

            recentTasksList = new ListView
            {
                Location = new Point(10, 55),
                Size = new Size(230, 235),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Segoe UI", 9),
                HeaderStyle = ColumnHeaderStyle.None,
                BorderStyle = BorderStyle.FixedSingle
            };
            
            recentTasksList.Columns.Add("Görev", 180);
            recentTasksList.Columns.Add("Durum", 50);

            taskBox.Controls.AddRange(new Control[] { taskCountLabel, recentTasksList });

            sidePanel.Controls.AddRange(new Control[] { moodBox, taskBox });
        }

        /// <summary>
        /// Ana içerik panelini oluşturur
        /// </summary>
        private void CreateContentPanel()
        {
            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(20)
            };

            // Durum etiketi
            statusLabel = new Label
            {
                Text = "AI asistanınız hazırlanıyor...",
                Location = new Point(20, 20),
                Size = new Size(400, 25),
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray
            };

            // Yükleme çubuğu
            loadingBar = new ProgressBar
            {
                Location = new Point(20, 50),
                Size = new Size(400, 10),
                Style = ProgressBarStyle.Marquee,
                MarqueeAnimationSpeed = 30,
                Visible = false
            };

            // Tavsiye görüntüleme alanı
            adviceDisplay = new RichTextBox
            {
                Location = new Point(20, 70),
                Size = new Size(650, 400),
                ReadOnly = true,
                Font = new Font("Segoe UI", 11),
                BackColor = Color.FromArgb(250, 250, 250),
                BorderStyle = BorderStyle.FixedSingle,
                ScrollBars = RichTextBoxScrollBars.Vertical,
                Text = "Merhaba! Ben MindTrack AI asistanınızım. 🤖\n\n" +
                       "Size kişiselleştirilmiş tavsiyeler sunmak için buradayım. " +
                       "Aşağıdaki butonlardan birini seçerek başlayabilirsiniz:\n\n" +
                       "• 🔍 Tam Analiz: Ruh haliniz ve görevlerinizi birlikte analiz eder\n" +
                       "• 😊 Sadece Ruh Hali: Duygusal durumunuza odaklanır\n" +
                       "• 📋 Görev Odaklı: Görevleriniz için stratejik öneriler\n" +
                       "• ⚡ Motivasyon: Hızlı motivasyon ve cesaret verici sözler"
            };

            contentPanel.Controls.AddRange(new Control[] { statusLabel, loadingBar, adviceDisplay });
        }

        /// <summary>
        /// Alt buton panelini oluşturur
        /// </summary>
        private void CreateButtonPanel()
        {
            buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 80,
                BackColor = Color.FromArgb(248, 249, 250),
                Padding = new Padding(20, 15, 20, 15)
            };

            // Analiz butonları
            btnFullAnalysis = CreateStyledButton("🔍 Tam Analiz", new Point(20, 15), Color.FromArgb(0, 120, 212));
            btnMoodOnly = CreateStyledButton("😊 Sadece Ruh Hali", new Point(160, 15), Color.FromArgb(108, 117, 125));
            btnTaskFocus = CreateStyledButton("📋 Görev Odaklı", new Point(300, 15), Color.FromArgb(40, 167, 69));
            btnMotivation = CreateStyledButton("⚡ Motivasyon", new Point(440, 15), Color.FromArgb(255, 193, 7));

            // Yardımcı butonlar
            btnSaveAdvice = CreateStyledButton("💾 Kaydet", new Point(580, 15), Color.FromArgb(108, 117, 125));
            btnClose = CreateStyledButton("❌ Kapat", new Point(720, 15), Color.FromArgb(220, 53, 69));

            // Olay bağlayıcıları
            btnFullAnalysis.Click += async (s, e) => await PerformAnalysis("full");
            btnMoodOnly.Click += async (s, e) => await PerformAnalysis("mood");
            btnTaskFocus.Click += async (s, e) => await PerformAnalysis("tasks");
            btnMotivation.Click += async (s, e) => await PerformAnalysis("motivation");
            btnSaveAdvice.Click += SaveAdvice_Click;
            btnClose.Click += (s, e) => this.Close();

            buttonPanel.Controls.AddRange(new Control[] { 
                btnFullAnalysis, btnMoodOnly, btnTaskFocus, btnMotivation, btnSaveAdvice, btnClose 
            });
        }

        /// <summary>
        /// Stilize edilmiş buton oluşturur
        /// </summary>
        private Button CreateStyledButton(string text, Point location, Color backColor)
        {
            return new Button
            {
                Text = text,
                Location = location,
                Size = new Size(130, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = backColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand
            };
        }

        /// <summary>
        /// Kullanıcı verilerini yükler
        /// </summary>
        private async Task LoadUserDataAsync()
        {
            try
            {
                using (var conn = new SQLiteConnection(connectionString))
                {
                    await conn.OpenAsync();

                    // Ruh hali verisi
                    using (var cmd = new SQLiteCommand(
                        "SELECT Mood FROM MoodEntries WHERE date(Timestamp) = @date ORDER BY Timestamp DESC LIMIT 1",
                        conn))
                    {
                        cmd.Parameters.AddWithValue("@date", selectedDate.ToString("yyyy-MM-dd"));
                        var result = await cmd.ExecuteScalarAsync();
                        currentMood = result?.ToString() ?? "Belirtilmemiş";
                        
                        currentMoodLabel.Text = GetMoodEmoji(currentMood) + " " + currentMood;
                        currentMoodLabel.BackColor = GetMoodColor(currentMood);
                    }

                    // Görev verileri
                    using (var cmd = new SQLiteCommand(
                        "SELECT Title, Status FROM Tasks WHERE Date = @date ORDER BY Id",
                        conn))
                    {
                        cmd.Parameters.AddWithValue("@date", selectedDate.ToString("yyyy-MM-dd"));
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            todayTasks.Clear();
                            recentTasksList.Items.Clear();
                            
                            int completedCount = 0;
                            int totalCount = 0;
                            
                            while (await reader.ReadAsync())
                            {
                                string title = reader["Title"].ToString();
                                string status = reader["Status"].ToString();
                                
                                todayTasks.Add(title);
                                totalCount++;
                                
                                if (status.ToLower() == "completed")
                                    completedCount++;

                                var item = new ListViewItem(title);
                                item.SubItems.Add(GetStatusEmoji(status));
                                
                                // Duruma göre renk
                                if (status.ToLower() == "completed")
                                    item.ForeColor = Color.Green;
                                else if (status.ToLower() == "in progress")
                                    item.ForeColor = Color.Orange;
                                else
                                    item.ForeColor = Color.Black;
                                
                                recentTasksList.Items.Add(item);
                            }
                            
                            taskCountLabel.Text = $"Toplam: {totalCount} | Tamamlanan: {completedCount}";
                        }
                    }
                }

                statusLabel.Text = "Veriler yüklendi. Analiz için bir seçenek seçin.";
            }
            catch (Exception ex)
            {
                statusLabel.Text = "Veri yükleme hatası: " + ex.Message;
                currentMoodLabel.Text = "Hata";
                taskCountLabel.Text = "Görevler yüklenemedi";
            }
        }

        /// <summary>
        /// Seçilen analiz türünü gerçekleştirir
        /// </summary>
        private async Task PerformAnalysis(string analysisType)
        {
            try
            {
                // UI'yi güncelle
                loadingBar.Visible = true;
                loadingBar.Style = ProgressBarStyle.Marquee;
                
                // Butonları devre dışı bırak
                SetButtonsEnabled(false);

                string prompt = "";
                string statusText = "";

                switch (analysisType)
                {
                    case "full":
                        statusText = "Tam analiz gerçekleştiriliyor...";
                        prompt = CreateFullAnalysisPrompt();
                        break;
                    case "mood":
                        statusText = "Ruh hali analizi yapılıyor...";
                        prompt = CreateMoodAnalysisPrompt();
                        break;
                    case "tasks":
                        statusText = "Görev odaklı analiz yapılıyor...";
                        prompt = CreateTaskAnalysisPrompt();
                        break;
                    case "motivation":
                        statusText = "Motivasyon mesajı oluşturuluyor...";
                        prompt = CreateMotivationPrompt();
                        break;
                }

                statusLabel.Text = statusText;

                // AI'dan yanıt al
                string response = await GetAIResponseAsync(prompt);
                
                if (!string.IsNullOrEmpty(response))
                {
                    lastAdvice = response;
                    DisplayFormattedAdvice(response, analysisType);
                    statusLabel.Text = $"Analiz tamamlandı - {DateTime.Now.ToString("HH:mm:ss")}";
                }
                else
                {
                    adviceDisplay.Text = "Üzgünüm, şu anda analiz yapamıyorum. Lütfen daha sonra tekrar deneyin.";
                    statusLabel.Text = "Analiz başarısız";
                }
            }
            catch (Exception ex)
            {
                adviceDisplay.Text = $"Hata oluştu: {ex.Message}";
                statusLabel.Text = "Analiz hatası";
            }
            finally
            {
                loadingBar.Visible = false;
                SetButtonsEnabled(true);
            }
        }

        /// <summary>
        /// Tam analiz prompt'u oluşturur
        /// </summary>
        private string CreateFullAnalysisPrompt()
        {
            return $@"
Sen MindTrack AI asistanısın. Kullanıcının ruh hali ve görevlerini analiz ederek kişiselleştirilmiş tavsiyeler ver.

KULLANICI VERİLERİ:
📅 Tarih: {selectedDate.ToString("dd MMMM yyyy")}
😊 Ruh Hali: {currentMood}
📋 Görevler: {string.Join(", ", todayTasks)}

GÖREV:
1. Ruh hali ve görev uyumunu analiz et
2. Kişiselleştirilmiş öneriler sun
3. Motivasyon ve destek ver
4. Pratik adımlar öner

Yanıtını şu formatta ver:
🔍 GENEL DURUM ANALİZİ
[Kısa analiz]

💡 KİŞİSELLEŞTİRİLMİŞ TAVSİYELER
[3-4 madde halinde]

⚡ BUGÜN İÇİN EYLEM PLANI
[Somut adımlar]

🌟 MOTİVASYON MESAJI
[Cesaret verici sözler]
";
        }

        /// <summary>
        /// Ruh hali analizi prompt'u oluşturur
        /// </summary>
        private string CreateMoodAnalysisPrompt()
        {
            return $@"
Sen MindTrack AI asistanısın. Kullanıcının ruh halini derinlemesine analiz et.

KULLANICI VERİSİ:
📅 Tarih: {selectedDate.ToString("dd MMMM yyyy")}
😊 Ruh Hali: {currentMood}

GÖREV:
1. Ruh halini yorumla
2. Duygusal destek ver
3. İyileştirme önerileri sun
4. Pozitif bakış açısı geliştir

Yanıtını şu formatta ver:
😊 RUH HALİ ANALİZİ
[Duygusal durum yorumu]

💚 DUYGUSAL DESTEK
[Anlayış ve empati]

🌈 İYİLEŞTİRME ÖNERİLERİ
[Pratik öneriler]

✨ POZİTİF MESAJ
[Umut verici sözler]
";
        }

        /// <summary>
        /// Görev analizi prompt'u oluşturur
        /// </summary>
        private string CreateTaskAnalysisPrompt()
        {
            return $@"
Sen MindTrack AI asistanısın. Kullanıcının görevlerini analiz ederek verimlilik önerileri ver.

KULLANICI VERİSİ:
📅 Tarih: {selectedDate.ToString("dd MMMM yyyy")}
📋 Görevler: {string.Join(", ", todayTasks)}

GÖREV:
1. Görev listesini analiz et
2. Önceliklendirme öner
3. Verimlilik stratejileri sun
4. Zaman yönetimi tavsiyeleri ver

Yanıtını şu formatta ver:
📋 GÖREV ANALİZİ
[Görev durumu değerlendirmesi]

🎯 ÖNCELİKLENDİRME
[Hangi görevler önce]

⚡ VERİMLİLİK STRATEJİLERİ
[Pratik yöntemler]

⏰ ZAMAN YÖNETİMİ
[Zaman planlaması önerileri]
";
        }

        /// <summary>
        /// Motivasyon prompt'u oluşturur
        /// </summary>
        private string CreateMotivationPrompt()
        {
            return $@"
Sen MindTrack AI asistanısın. Kullanıcıya güçlü motivasyon ve cesaret ver.

KULLANICI VERİSİ:
📅 Tarih: {selectedDate.ToString("dd MMMM yyyy")}
😊 Ruh Hali: {currentMood}

GÖREV:
1. Güçlü motivasyon ver
2. Özgüven artır
3. Pozitif enerji yay
4. İlham verici ol

Yanıtını şu formatta ver:
⚡ GÜNLÜK MOTİVASYON
[Güçlü motivasyon mesajı]

💪 ÖZGÜVEN ARTIRICI
[Güç verici sözler]

🌟 İLHAM VERİCİ DÜŞÜNCELER
[İlham verici alıntılar]

🚀 BAŞARI MESAJI
[Başarıya odaklı mesaj]
";
        }

        /// <summary>
        /// AI'dan yanıt alır
        /// </summary>
        private async Task<string> GetAIResponseAsync(string prompt)
        {
            try
            {
                var payload = new
                {
                    messages = new[]
                    {
                        new { role = "user", content = prompt }
                    },
                    temperature = 0.7,
                    max_tokens = 1000
                };

                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
                    var json = JsonConvert.SerializeObject(payload);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    
                    var response = await client.PostAsync("http://localhost:1234/v1/chat/completions", content);
                    response.EnsureSuccessStatusCode();
                    
                    var result = await response.Content.ReadAsStringAsync();
                    dynamic obj = JsonConvert.DeserializeObject(result);
                    return obj.choices[0].message.content.ToString();
                }
            }
            catch (Exception ex)
            {
                return $"AI bağlantı hatası: {ex.Message}";
            }
        }

        /// <summary>
        /// Formatlanmış tavsiyeyi görüntüler
        /// </summary>
        private void DisplayFormattedAdvice(string advice, string analysisType)
        {
            adviceDisplay.Clear();
            
            // Başlık ekle
            string title = analysisType switch
            {
                "full" => "🔍 TAM ANALİZ SONUÇLARI",
                "mood" => "😊 RUH HALİ ANALİZİ",
                "tasks" => "📋 GÖREV ANALİZİ",
                "motivation" => "⚡ MOTİVASYON MESAJINIZ",
                _ => "🤖 AI TAVSİYELERİ"
            };

            adviceDisplay.SelectionFont = new Font("Segoe UI", 14, FontStyle.Bold);
            adviceDisplay.SelectionColor = Color.FromArgb(0, 120, 212);
            adviceDisplay.AppendText(title + "\n");
            adviceDisplay.AppendText("═══════════════════════════════════════\n\n");

            // Ana içeriği ekle
            adviceDisplay.SelectionFont = new Font("Segoe UI", 11);
            adviceDisplay.SelectionColor = Color.Black;
            adviceDisplay.AppendText(advice);

            // Alt bilgi ekle
            adviceDisplay.AppendText("\n\n");
            adviceDisplay.SelectionFont = new Font("Segoe UI", 9, FontStyle.Italic);
            adviceDisplay.SelectionColor = Color.Gray;
            adviceDisplay.AppendText($"───────────────────────────────────────\n");
            adviceDisplay.AppendText($"Oluşturulma: {DateTime.Now.ToString("dd MMMM yyyy HH:mm:ss")}\n");
            adviceDisplay.AppendText($"MindTrack AI Asistanı tarafından kişiselleştirildi");
        }

        /// <summary>
        /// Tavsiyeyi kaydetme işlemi
        /// </summary>
        private void SaveAdvice_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(lastAdvice))
            {
                MessageBox.Show("Kaydedilecek tavsiye bulunamadı.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (var saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "Metin Dosyası (*.txt)|*.txt|Tüm Dosyalar (*.*)|*.*";
                    saveDialog.FileName = $"MindTrack_Tavsiye_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.txt";
                    
                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        string content = $"MindTrack AI Tavsiyesi\n";
                        content += $"Tarih: {selectedDate.ToString("dd MMMM yyyy")}\n";
                        content += $"Oluşturulma: {DateTime.Now.ToString("dd MMMM yyyy HH:mm:ss")}\n";
                        content += $"Ruh Hali: {currentMood}\n";
                        content += $"Görev Sayısı: {todayTasks.Count}\n\n";
                        content += "═══════════════════════════════════════\n\n";
                        content += lastAdvice;
                        
                        System.IO.File.WriteAllText(saveDialog.FileName, content, Encoding.UTF8);
                        MessageBox.Show("Tavsiye başarıyla kaydedildi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kaydetme hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Butonları etkinleştir/devre dışı bırak
        /// </summary>
        private void SetButtonsEnabled(bool enabled)
        {
            btnFullAnalysis.Enabled = enabled;
            btnMoodOnly.Enabled = enabled;
            btnTaskFocus.Enabled = enabled;
            btnMotivation.Enabled = enabled;
        }

        /// <summary>
        /// Ruh haline göre emoji döndürür
        /// </summary>
        private string GetMoodEmoji(string mood)
        {
            return mood.ToLower() switch
            {
                "mutlu" or "happy" => "😊",
                "üzgün" or "sad" => "😢",
                "stresli" or "stressed" => "😰",
                "sakin" or "calm" => "😌",
                "enerjik" or "energetic" => "⚡",
                "yorgun" or "tired" => "😴",
                "sinirli" or "angry" => "😠",
                "heyecanlı" or "excited" => "🤩",
                _ => "😐"
            };
        }

        /// <summary>
        /// Ruh haline göre renk döndürür
        /// </summary>
        private Color GetMoodColor(string mood)
        {
            return mood.ToLower() switch
            {
                "mutlu" or "happy" => Color.FromArgb(255, 248, 220),
                "üzgün" or "sad" => Color.FromArgb(230, 240, 255),
                "stresli" or "stressed" => Color.FromArgb(255, 235, 235),
                "sakin" or "calm" => Color.FromArgb(240, 255, 240),
                "enerjik" or "energetic" => Color.FromArgb(255, 250, 205),
                "yorgun" or "tired" => Color.FromArgb(245, 245, 245),
                "sinirli" or "angry" => Color.FromArgb(255, 220, 220),
                "heyecanlı" or "excited" => Color.FromArgb(255, 240, 255),
                _ => Color.FromArgb(248, 249, 250)
            };
        }

        /// <summary>
        /// Görev durumuna göre emoji döndürür
        /// </summary>
        private string GetStatusEmoji(string status)
        {
            return status.ToLower() switch
            {
                "completed" => "✅",
                "in progress" => "🔄",
                "pending" => "⏳",
                _ => "📝"
            };
        }
    }
} 