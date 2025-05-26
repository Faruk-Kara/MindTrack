using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;

namespace MindTrack
{
    /// <summary>
    /// Odaklanma modu formu
    /// Kullanıcının belirli bir süre boyunca odaklanmasını sağlar
    /// Zamanlayıcı, motivasyon mesajları ve fade efektleri içerir
    /// </summary>
    public partial class FocusForm : Form
    {
        // Form kontrolleri
        private Label timerLabel;      // Kullanılmayan eski timer
        private Label quoteLabel;      // Motivasyon sözleri
        private Button btnStart;       // Başlat butonu
        private Button btnExit;        // Kullanılmayan çıkış butonu
        private NumericUpDown durationInput;  // Süre seçici
        
        // Zamanlayıcılar
        private Timer focusTimer;      // Ana odaklanma zamanlayıcısı
        private Timer quoteTimer;      // Kullanılmayan
        private Timer messageTimer;   // Mesaj değiştirme zamanlayıcısı
        private Timer fadeTimer;       // Soluklaşma efekti zamanlayıcısı
        
        // Zaman değişkenleri
        private int remainingSeconds;  // Kalan saniye
        private DateTime startTime;    // Kullanılmayan
        private int totalSeconds;      // Toplam saniye
        private DateTime sessionStartTime;  // Oturum başlangıç zamanı
        
        // Diğer değişkenler
        private readonly string connectionString;  // Veritabanı bağlantı metni
        private readonly List<string> motivationalQuotes;   // Motivasyon sözleri listesi
        private List<string> motivationalMessages;          // Motivasyon mesajları listesi
        private Random random;         // Rastgele sayı üretici
        private Label lblTimer;        // Ana zamanlayıcı etiketi
        private Label lblMotivation;   // Motivasyon mesajı etiketi
        private Button btnStop;        // Durdur butonu
        private Button btnSave;        // Kaydet ve çık butonu
        private bool isSessionActive;  // Oturum aktif mi?
        
        // Fade efekti değişkenleri
        private bool isFading = false;  // Şu anda fade efekti var mı?
        private int fadeStep = 0;       // Fade adımı
        private string nextMessage = "";  // Sonraki gösterilecek mesaj

        // Windows API fonksiyonları (pencere takibi için)
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder text, int count);

        /// <summary>
        /// Form yapıcı metodu
        /// </summary>
        public FocusForm()
        {
            InitializeComponent();
            connectionString = $"Data Source={System.IO.Path.Combine(Application.StartupPath, "Database", "mindtrack.db")};Version=3;";
            motivationalQuotes = InitializeQuotes();
            random = new Random();
            InitializeUI();
            LoadMotivationalMessages();
            isSessionActive = false;
        }

        /// <summary>
        /// Motivasyon sözlerini yükler
        /// </summary>
        private List<string> InitializeQuotes()
        {
            return new List<string>
            {
                "Meşgul olmak yerine üretken olmaya odaklan.",
                "Başarılı savaşçı, lazer gibi odaklanmış sıradan bir insandır.",
                "Tüm düşüncelerini elindeki işe yoğunlaştır.",
                "Odak nereye giderse, enerji oraya akar.",
                "Odaklanmaya devam et, hayallerinin peşinden git ve hedeflerine doğru ilerlemeye devam et.",
                "Hedefe değil, yolculuğa odaklan.",
                "Zaman yönetimine ne kadar odaklanırsan, yeterli zamanın olduğunu o kadar fark edersin.",
                "Korkuna değil, hedeflerine odaklan.",
                "Odağın gerçekliğini belirler.",
                "Sorun zaman eksikliği değil, yön eksikliğidir. Hepimizin yirmi dört saatlik günleri var."
            };
        }

        /// <summary>
        /// Kullanıcı arayüzünü hazırlar
        /// Tam ekran, karanlık tema ve kontrolleri oluşturur
        /// </summary>
        private void InitializeUI()
        {
            // Form temel ayarları
            this.Text = "Odaklanma Modu";
            this.Size = new Size(800, 600);
            this.FormBorderStyle = FormBorderStyle.None;  // Kenarlık yok
            this.WindowState = FormWindowState.Maximized; // Tam ekran
            this.BackColor = Color.FromArgb(40, 44, 52);  // Koyu gri arka plan
            this.TopMost = true;  // Her zaman üstte

            // Süre ayarlama paneli (üstte)
            var setupPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                BackColor = Color.FromArgb(52, 73, 94),
                Padding = new Padding(20)
            };

            var durationLabel = new Label
            {
                Text = "Odaklanma Süresi (dakika):",
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 30),
                Font = new Font("Segoe UI", 12)
            };

            durationInput = new NumericUpDown
            {
                Location = new Point(250, 30),
                Width = 100,
                Minimum = 1,
                Maximum = 120,
                Value = 25,  // Varsayılan 25 dakika
                Font = new Font("Segoe UI", 12)
            };

            setupPanel.Controls.AddRange(new Control[] { durationLabel, durationInput });

            // Ana zamanlayıcı etiketi - Sabit boyut ve konum
            lblTimer = new Label
            {
                Text = "25:00",
                Font = new Font("Segoe UI", 72, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(400, 100),
                BackColor = Color.Transparent
            };

            // Motivasyon sözleri etiketi - Sabit boyut ve konum
            quoteLabel = new Label
            {
                Text = GetRandomQuote(),
                ForeColor = Color.FromArgb(144, 238, 144),  // Açık yeşil
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 14),
                Size = new Size(800, 60),
                BackColor = Color.Transparent
            };

            // Motivasyon mesajı etiketi - Sabit boyut ve konum
            lblMotivation = new Label
            {
                Text = "Odaklanmaya hazır mısın?",
                Font = new Font("Segoe UI", 20),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(800, 40),
                BackColor = Color.Transparent
            };

            // Kontrol butonları paneli (altta)
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 100,
                BackColor = Color.FromArgb(52, 73, 94)
            };

            // Başlat butonu
            btnStart = new Button
            {
                Text = "Başla",
                Size = new Size(120, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(46, 204, 113),  // Yeşil
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12),
                Anchor = AnchorStyles.None
            };
            btnStart.Click += BtnStart_Click;

            // Durdur butonu
            btnStop = new Button
            {
                Text = "Durdur",
                Size = new Size(120, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(231, 76, 60),  // Kırmızı
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12),
                Enabled = false,
                Visible = false,
                Anchor = AnchorStyles.None
            };
            btnStop.Click += BtnStop_Click;

            // Kaydet ve çık butonu
            btnSave = new Button
            {
                Text = "Kaydet & Çık",
                Size = new Size(120, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(52, 152, 219),  // Mavi
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12),
                Enabled = false,
                Visible = false,
                Anchor = AnchorStyles.None
            };
            btnSave.Click += BtnSave_Click;

            // Form boyutu değiştiğinde kontrolleri yeniden konumlandır
            this.Resize += (s, e) => PositionControls();

            buttonPanel.Controls.AddRange(new Control[] { btnStart, btnStop, btnSave });

            this.Controls.AddRange(new Control[] {
                setupPanel, lblTimer, quoteLabel, lblMotivation, buttonPanel
            });

            this.KeyPreview = true;
            this.KeyDown += FocusForm_KeyDown;

            // Zamanlayıcıları başlat
            focusTimer = new Timer();
            focusTimer.Interval = 1000;  // Her saniye
            focusTimer.Tick += FocusTimer_Tick;

            messageTimer = new Timer();
            messageTimer.Interval = 15000; // 15 saniyede bir yeni mesaj
            messageTimer.Tick += MessageTimer_Tick;

            // Fade efekti zamanlayıcısı
            fadeTimer = new Timer();
            fadeTimer.Interval = 50; // 50ms yumuşak geçiş için
            fadeTimer.Tick += FadeTimer_Tick;

            // Kontrolleri ilk konumlandır
            PositionControls();
        }

        /// <summary>
        /// Kontrolleri ekran ortasında konumlandırır
        /// Yazıların üst üste binmesini önler
        /// </summary>
        private void PositionControls()
        {
            if (this.ClientSize.Width == 0 || this.ClientSize.Height == 0) return;

            int centerX = this.ClientSize.Width / 2;
            int centerY = this.ClientSize.Height / 2;

            // Zamanlayıcıyı üstte konumlandır
            lblTimer.Location = new Point(centerX - lblTimer.Width / 2, centerY - 150);

            // Motivasyon mesajını ortada konumlandır
            lblMotivation.Location = new Point(centerX - lblMotivation.Width / 2, centerY - 20);

            // Motivasyon sözlerini altta konumlandır
            quoteLabel.Location = new Point(centerX - quoteLabel.Width / 2, centerY + 80);

            // Butonları alt panelde konumlandır
            var buttonPanel = this.Controls[this.Controls.Count - 1] as Panel;
            if (buttonPanel != null)
            {
                btnStart.Location = new Point(buttonPanel.Width / 2 - 180, 30);
                btnStop.Location = new Point(buttonPanel.Width / 2 - 60, 30);
                btnSave.Location = new Point(buttonPanel.Width / 2 + 60, 30);
            }
        }

        /// <summary>
        /// Rastgele motivasyon sözü döndürür
        /// </summary>
        private string GetRandomQuote()
        {
            return motivationalQuotes[random.Next(motivationalQuotes.Count)];
        }

        /// <summary>
        /// Motivasyon mesajlarını yükler
        /// </summary>
        private void LoadMotivationalMessages()
        {
            motivationalMessages = new List<string>
            {
                "Odaklanmaya devam et, harika gidiyorsun!",
                "Her dakika hedefinize doğru sayılır.",
                "Momentum kazanıyorsun!",
                "İlerlemeye devam et!",
                "Gelecekteki benin sana teşekkür edecek.",
                "Mükemmelliğe değil, ilerlemeye odaklan.",
                "Başarabilirsin!",
                "Şu anki anda kal.",
                "Küçük adımlar büyük sonuçlara yol açar.",
                "Gözlerini ödülden ayırma!",
                "Konsantrasyonunu koru!",
                "Bu anı yaşa, bu görevi tamamla!",
                "Zamanın değerli, onu iyi kullan!",
                "Her saniye bir adım daha yaklaşıyorsun!",
                "Dikkatini dağıtma, hedefe odaklan!"
            };
        }

        /// <summary>
        /// Başlat butonuna tıklandığında çalışır
        /// </summary>
        private void BtnStart_Click(object sender, EventArgs e)
        {
            if (!isSessionActive)
            {
                StartFocusSession();
            }
        }

        /// <summary>
        /// Odaklanma seansını başlatır
        /// Zamanlayıcıları çalıştırır ve arayüzü günceller
        /// </summary>
        private void StartFocusSession()
        {
            isSessionActive = true;
            sessionStartTime = DateTime.Now;
            totalSeconds = (int)durationInput.Value * 60;  // Dakikayı saniyeye çevir
            remainingSeconds = totalSeconds;
            UpdateTimerDisplay();

            // Ayar panelini gizle, oturum arayüzünü göster
            this.Controls[0].Visible = false;
            lblTimer.Visible = true;
            lblMotivation.Visible = true;
            quoteLabel.Visible = true;

            durationInput.Enabled = false;
            btnStart.Visible = false;
            btnStop.Enabled = true;
            btnStop.Visible = true;
            btnSave.Enabled = false;

            // Zamanlayıcıları başlat
            focusTimer.Start();
            messageTimer.Start();
            ShowRandomMotivationalMessage();
            PositionControls();
        }

        /// <summary>
        /// Ana zamanlayıcı - her saniye çalışır
        /// Kalan süreyi azaltır ve ekranı günceller
        /// </summary>
        private void FocusTimer_Tick(object sender, EventArgs e)
        {
            if (remainingSeconds > 0)
            {
                remainingSeconds--;
                UpdateTimerDisplay();
            }
            else
            {
                StopFocusSession();  // Süre bittiğinde oturumu durdur
            }
        }

        /// <summary>
        /// Mesaj zamanlayıcısı - 15 saniyede bir çalışır
        /// Yeni motivasyon mesajı için fade geçişi başlatır
        /// </summary>
        private void MessageTimer_Tick(object sender, EventArgs e)
        {
            if (!isFading)  // Şu anda fade yoksa yeni fade başlat
            {
                StartFadeTransition();
            }
        }

        /// <summary>
        /// Fade geçişini başlatır
        /// Yeni mesajı seçer ve soluklaşma efektini başlatır
        /// </summary>
        private void StartFadeTransition()
        {
            if (motivationalMessages.Count > 0)
            {
                Random rand = new Random();
                int index = rand.Next(motivationalMessages.Count);
                nextMessage = motivationalMessages[index];
                
                isFading = true;
                fadeStep = 0;
                fadeTimer.Start();
            }
        }

        /// <summary>
        /// Fade efekti zamanlayıcısı - 50ms'de bir çalışır
        /// Mesajın soluklaşarak değişmesini sağlar
        /// </summary>
        private void FadeTimer_Tick(object sender, EventArgs e)
        {
            if (fadeStep < 10) // Fade out - mesaj kaybolur
            {
                int alpha = 255 - (fadeStep * 25);  // Alpha değerini azalt
                lblMotivation.ForeColor = Color.FromArgb(Math.Max(0, alpha), Color.White);
                fadeStep++;
            }
            else if (fadeStep < 20) // Fade in - yeni mesaj belirir
            {
                if (fadeStep == 10)
                {
                    lblMotivation.Text = nextMessage;  // Mesajı değiştir
                }
                int alpha = (fadeStep - 10) * 25;  // Alpha değerini artır
                lblMotivation.ForeColor = Color.FromArgb(Math.Min(255, alpha), Color.White);
                fadeStep++;
            }
            else // Fade tamamlandı
            {
                lblMotivation.ForeColor = Color.White;
                fadeTimer.Stop();
                isFading = false;
                fadeStep = 0;
            }
        }

        /// <summary>
        /// Rastgele motivasyon mesajı gösterir (fade olmadan)
        /// </summary>
        private void ShowRandomMotivationalMessage()
        {
            if (motivationalMessages.Count > 0 && !isFading)
            {
                Random rand = new Random();
                int index = rand.Next(motivationalMessages.Count);
                lblMotivation.Text = motivationalMessages[index];
            }
        }

        /// <summary>
        /// Zamanlayıcı ekranını günceller (MM:SS formatında)
        /// </summary>
        private void UpdateTimerDisplay()
        {
            int minutes = remainingSeconds / 60;
            int seconds = remainingSeconds % 60;
            lblTimer.Text = $"{minutes:D2}:{seconds:D2}";
        }

        /// <summary>
        /// Durdur butonuna tıklandığında çalışır
        /// </summary>
        private void BtnStop_Click(object sender, EventArgs e)
        {
            StopFocusSession();
        }

        /// <summary>
        /// Kaydet ve çık butonuna tıklandığında çalışır
        /// </summary>
        private void BtnSave_Click(object sender, EventArgs e)
        {
            SaveFocusSession();
            this.Close();
        }

        /// <summary>
        /// Odaklanma seansını durdurur
        /// Zamanlayıcıları durdurur ve arayüzü günceller
        /// </summary>
        private void StopFocusSession()
        {
            isSessionActive = false;
            focusTimer.Stop();
            messageTimer.Stop();
            fadeTimer.Stop();

            btnStart.Visible = true;
            btnStart.Enabled = true;
            btnStop.Enabled = false;
            btnStop.Visible = false;
            btnSave.Enabled = true;
            btnSave.Visible = true;

            lblMotivation.Text = "Oturum tamamlandı!";
            lblMotivation.ForeColor = Color.White;
            PositionControls();
        }

        /// <summary>
        /// Odaklanma seansını veritabanına kaydeder
        /// </summary>
        private void SaveFocusSession()
        {
            try
            {
                string dbPath = Path.Combine(Application.StartupPath, "Database", "mindtrack.db");
                string connStr = $"Data Source={dbPath};Version=3;";

                using (var conn = new SQLiteConnection(connStr))
                {
                    conn.Open();
                    string insertQuery = @"
                        INSERT INTO FocusSessions (StartTime, EndTime, DurationMinutes)
                        VALUES (@startTime, @endTime, @duration)";

                    using (var cmd = new SQLiteCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@startTime", sessionStartTime.ToString("yyyy-MM-dd HH:mm:ss"));
                        cmd.Parameters.AddWithValue("@endTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        cmd.Parameters.AddWithValue("@duration", totalSeconds / 60);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Oturum kaydedilirken hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Klavye tuşlarına basıldığında çalışır
        /// ESC tuşu ile oturumu durdurma
        /// </summary>
        private void FocusForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape && focusTimer.Enabled)
            {
                BtnStop_Click(sender, e);
            }
        }

        /// <summary>
        /// Form kapanırken çalışır
        /// Aktif oturum varsa kullanıcıya sorar
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (isSessionActive)
            {
                var result = MessageBox.Show(
                    "Çıkmak istediğinizden emin misiniz? Mevcut oturum kaydedilmeyecek.",
                    "Çıkış Onayı",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }

            // Tüm zamanlayıcıları durdur
            focusTimer?.Stop();
            messageTimer?.Stop();
            fadeTimer?.Stop();
            base.OnFormClosing(e);
        }
    }
}