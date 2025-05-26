using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Threading.Tasks;
using System.Net.Http;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace MindTrack
{
    /// <summary>
    /// Ana form sınıfı - Programın ana ekranını yönetir
    /// Kullanıcının görevlerini, ruh halini ve odaklanma seanslarını takip eder
    /// </summary>
    public partial class Form1 : Form
    {
        // Sol panel kontrolleri - Hava durumu, saat ve ana butonlar
        private Panel leftPanel;
        private Panel centerPanel;
        private Panel rightPanel;
        
        // Sol paneldeki kontroller
        private Label lblWeather;  // Hava durumu gösterimi
        private Label lblClock;    // Dijital saat
        private Button btnFocus;   // Odaklanma modu butonu
        private Button btnStats;   // İstatistikler butonu
        private Button btnMood;    // Ruh hali kaydetme butonu
        
        // Orta paneldeki kontroller - Görev listesi
        private ListView taskListView;  // Görevleri gösteren liste
        private Button btnEditTasks;    // Görev düzenleme butonu
        
        // Sağ paneldeki kontroller - Takvim ve AI özellikleri
        private MonthCalendar calendar;     // Tarih seçici
        private Button btnChat;             // AI sohbet butonu
        private Button btnGetAdvice;        // Hızlı tavsiye butonu
        private Button btnFeelingAdvice;    // Ruh hali analizi butonu
        
        // Diğer formlar ve zamanlayıcılar
        private ChatForm chatForm;    // Sohbet formu referansı
        private Timer clockTimer;     // Saat güncellemesi için
        private Timer weatherTimer;   // Hava durumu güncellemesi için

        /// <summary>
        /// Form yapıcı metodu - Program başladığında çalışır
        /// </summary>
        public Form1()
        {
            InitializeComponent();  // Windows Forms tasarımcısı tarafından oluşturulan kod
            InitUI();              // Kullanıcı arayüzünü hazırla
            DatabaseHelper.EnsureDatabaseInitialized();  // Veritabanını kontrol et ve oluştur
            InitializeTimers();    // Zamanlayıcıları başlat

            // Bugünün görevlerini yükle
            LoadTasksForDate(DateTime.Today);

            // Program açılışında ruh hali sorgusu
            if (!IsMoodLoggedToday())
            {
                var moodForm = new MoodForm();
                moodForm.ShowDialog();
            }
        }

        /// <summary>
        /// Kullanıcı arayüzünü başlatan metod
        /// Form boyutu, renkleri ve panelleri ayarlar
        /// </summary>
        private void InitUI()
        {
            this.Text = "MindTrack - Zihin Takip";
            this.Size = new Size(1400, 900);
            this.MinimumSize = new Size(1200, 700);
            this.BackColor = Color.FromArgb(240, 240, 240);
            this.StartPosition = FormStartPosition.CenterScreen;
            
            // Panelleri oluştur
            InitializePanels();
            
            // Her paneli ayrı ayrı ayarla
            SetupLeftPanel();
            SetupCenterPanel();
            SetupRightPanel();
        }

        /// <summary>
        /// Ana panelleri oluşturur (Sol, Orta, Sağ)
        /// </summary>
        private void InitializePanels()
        {
            // Sol panel - Hava durumu, saat, ana butonlar
            leftPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 280,
                BackColor = Color.White,
                Padding = new Padding(15)
            };

            // Orta panel - Görev listesi
            centerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(15),
                Margin = new Padding(5, 0, 5, 0)
            };

            // Sağ panel - Takvim ve AI özellikleri
            rightPanel = new Panel
            {
                Dock = DockStyle.Right,
                Width = 350,
                BackColor = Color.White,
                Padding = new Padding(15)
            };

            // Panelleri forma ekle (sıra önemli!)
            this.Controls.Add(rightPanel);
            this.Controls.Add(leftPanel);
            this.Controls.Add(centerPanel);
        }

        /// <summary>
        /// Sol paneli ayarlar - Hava durumu, saat ve ana butonlar
        /// </summary>
        private void SetupLeftPanel()
        {
            // Hava durumu etiketi
            lblWeather = new Label
            {
                Text = "Hava durumu yükleniyor...",
                Size = new Size(250, 80),
                Font = new Font("Segoe UI", 11),
                Location = new Point(15, 20),
                TextAlign = ContentAlignment.TopCenter,
                ForeColor = Color.FromArgb(51, 51, 51),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(248, 249, 250)
            };

            // Dijital saat
            lblClock = new Label
            {
                Size = new Size(250, 50),
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                Location = new Point(15, 110),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(0, 120, 212),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(248, 249, 250)
            };

            // Odaklanma modu butonu
            btnFocus = new Button
            {
                Text = "🎯 Odaklanma Modu",
                Size = new Size(250, 50),
                Location = new Point(15, 180),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(100, 149, 237),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            btnFocus.Click += BtnFocus_Click;  // Tıklama olayını bağla

            // İstatistikler butonu
            btnStats = new Button
            {
                Text = "📊 İstatistikler",
                Size = new Size(250, 50),
                Location = new Point(15, 240),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            btnStats.Click += BtnStats_Click;

            // Ruh hali kaydetme butonu
            btnMood = new Button
            {
                Text = "😊 Ruh Hali Kaydet",
                Size = new Size(250, 50),
                Location = new Point(15, 300),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(241, 196, 15),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            btnMood.Click += BtnMood_Click;

            // Tüm kontrolleri sol panele ekle
            leftPanel.Controls.AddRange(new Control[] { lblWeather, lblClock, btnFocus, btnStats, btnMood });
        }

        /// <summary>
        /// Orta paneli ayarlar - Görev listesi ve düzenleme butonu
        /// </summary>
        private void SetupCenterPanel()
        {
            // Başlık etiketi
            var titleLabel = new Label
            {
                Text = "Bugünün Görevleri",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 51, 51),
                Dock = DockStyle.Top,
                Height = 50,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(0, 10, 0, 0)
            };

            // Görev listesi
            taskListView = new ListView
            {
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 11),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                HeaderStyle = ColumnHeaderStyle.Nonclickable
            };
            
            // Liste sütunlarını ayarla
            taskListView.Columns.Add("Görev", 350);
            taskListView.Columns.Add("Durum", 120);
            
            // Liste görünümünü iyileştir
            taskListView.UseCompatibleStateImageBehavior = false;
            taskListView.View = View.Details;

            // Görev düzenleme butonu
            btnEditTasks = new Button
            {
                Text = "Görevleri Düzenle",
                Size = new Size(150, 45),
                Dock = DockStyle.Bottom,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(72, 161, 77),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };
            btnEditTasks.Click += BtnEditTasks_Click;

            // Kontrolleri orta panele ekle
            centerPanel.Controls.Add(taskListView);
            centerPanel.Controls.Add(btnEditTasks);
            centerPanel.Controls.Add(titleLabel);
        }

        /// <summary>
        /// Sağ paneli ayarlar - Takvim ve AI özellikleri
        /// </summary>
        private void SetupRightPanel()
        {
            // Takvim kontrolü
            calendar = new MonthCalendar
            {
                Location = new Point(15, 15),
                MaxSelectionCount = 1,
                Font = new Font("Segoe UI", 10)
            };
            calendar.DateSelected += Calendar_DateSelected;  // Tarih seçildiğinde çalışacak metod

            // AI Asistan başlığı
            var lblAIAdvice = new Label
            {
                Text = "AI Asistan",
                Location = new Point(15, calendar.Bottom + 30),
                Size = new Size(320, 30),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 51, 51),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // AI Tavsiye butonu
            var btnOpenAIAdvice = new Button
            {
                Text = "🤖 AI Tavsiye Aç",
                Size = new Size(320, 45),
                Location = new Point(15, lblAIAdvice.Bottom + 15),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 120, 212),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            btnOpenAIAdvice.Click += BtnOpenAIAdvice_Click;

            // AI Sohbet butonu
            btnChat = new Button
            {
                Text = "💬 AI Sohbet Aç",
                Size = new Size(320, 45),
                Location = new Point(15, btnOpenAIAdvice.Bottom + 15),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(147, 112, 219),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            btnChat.Click += BtnChat_Click;

            // Hızlı işlemler başlığı
            var lblQuickActions = new Label
            {
                Text = "Hızlı İşlemler",
                Location = new Point(15, btnChat.Bottom + 30),
                Size = new Size(320, 25),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 51, 51),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Hızlı tavsiye butonu
            btnGetAdvice = new Button
            {
                Text = "⚡ Hızlı Tavsiye",
                Size = new Size(320, 40),
                Location = new Point(15, lblQuickActions.Bottom + 10),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            btnGetAdvice.Click += BtnGetAdvice_Click;

            // Sadece ruh hali analizi butonu
            btnFeelingAdvice = new Button
            {
                Text = "😊 Sadece Ruh Hali Analizi",
                Size = new Size(320, 40),
                Location = new Point(15, btnGetAdvice.Bottom + 10),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            btnFeelingAdvice.Click += BtnFeelingAdvice_Click;

            // Tüm kontrolleri sağ panele ekle
            rightPanel.Controls.AddRange(new Control[] { 
                calendar, lblAIAdvice, btnOpenAIAdvice, btnChat, lblQuickActions, btnGetAdvice, btnFeelingAdvice 
            });
        }

        /// <summary>
        /// Odaklanma modu butonuna tıklandığında çalışır
        /// </summary>
        private void BtnFocus_Click(object sender, EventArgs e)
        {
            var focusForm = new FocusForm();
            focusForm.ShowDialog();  // Modal olarak aç
        }

        /// <summary>
        /// Takvimde tarih seçildiğinde çalışır
        /// Seçilen tarihin görevlerini yükler
        /// </summary>
        private void Calendar_DateSelected(object sender, DateRangeEventArgs e)
        {
            LoadTasksForDate(e.Start);
        }

        /// <summary>
        /// Belirtilen tarihin görevlerini veritabanından yükler ve listede gösterir
        /// </summary>
        /// <param name="date">Görevleri yüklenecek tarih</param>
        private async void LoadTasksForDate(DateTime date)
        {
            taskListView.Items.Clear();  // Önce listeyi temizle
            
            // Başlığı güncelle
            var titleLabel = centerPanel.Controls.OfType<Label>().FirstOrDefault();
            if (titleLabel != null)
            {
                if (date.Date == DateTime.Today)
                    titleLabel.Text = "Bugünün Görevleri";
                else
                    titleLabel.Text = $"{date.ToString("dd MMMM yyyy")} Görevleri";
            }
            
            try
            {
                // Veritabanından görevleri getir
                string query = "SELECT Id, Title, Status FROM Tasks WHERE Date = @date ORDER BY Id";
                using (var reader = DatabaseHelper.ExecuteReader(query,
                    new SQLiteParameter("@date", date.ToString("yyyy-MM-dd"))))
                {
                    int taskCount = 0;
                    while (reader.Read())
                    {
                        var item = new ListViewItem(reader["Title"].ToString());
                        string status = reader["Status"].ToString();
                        
                        // İngilizce durumları Türkçe'ye çevir
                        string turkishStatus = status;
                        switch (status.ToLower())
                        {
                            case "completed":
                                turkishStatus = "Tamamlandı";
                                break;
                            case "in progress":
                                turkishStatus = "Devam Ediyor";
                                break;
                            case "pending":
                                turkishStatus = "Bekliyor";
                                break;
                        }
                        
                        item.SubItems.Add(turkishStatus);
                        item.Tag = reader["Id"];
                        
                        // Duruma göre renk ver
                        if (status.ToLower() == "completed")
                        {
                            item.ForeColor = Color.Green;
                            item.Font = new Font(taskListView.Font, FontStyle.Strikeout);
                        }
                        else if (status.ToLower() == "in progress")
                        {
                            item.ForeColor = Color.Orange;
                        }
                        else
                        {
                            item.ForeColor = Color.Black;
                        }
                        
                        taskListView.Items.Add(item);
                        taskCount++;
                    }
                    
                    // Eğer görev yoksa bilgi mesajı göster
                    if (taskCount == 0)
                    {
                        var noTasksItem = new ListViewItem("Bu tarih için görev yok");
                        noTasksItem.SubItems.Add("");
                        noTasksItem.ForeColor = Color.Gray;
                        noTasksItem.Font = new Font(taskListView.Font, FontStyle.Italic);
                        taskListView.Items.Add(noTasksItem);
                    }
                }
            }
            catch (Exception ex)
            {
                // Hata durumunda kullanıcıya bilgi ver
                MessageBox.Show("Görevler yüklenirken hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                var errorItem = new ListViewItem("Görevler yüklenirken hata");
                errorItem.SubItems.Add("Hata");
                errorItem.ForeColor = Color.Red;
                taskListView.Items.Add(errorItem);
            }
        }

        /// <summary>
        /// Görev düzenleme butonuna tıklandığında çalışır
        /// </summary>
        private void BtnEditTasks_Click(object sender, EventArgs e)
        {
            var taskForm = new TaskForm(calendar.SelectionStart);
            if (taskForm.ShowDialog() == DialogResult.OK)
            {
                LoadTasksForDate(calendar.SelectionStart);  // Görevleri yeniden yükle
                
                // Görev düzenledikten sonra ruh hali sor
                var moodForm = new MoodForm();
                moodForm.ShowDialog();
            }
        }

        /// <summary>
        /// İstatistikler butonuna tıklandığında çalışır
        /// </summary>
        private void BtnStats_Click(object sender, EventArgs e)
        {
            var statsForm = new StatsForm();
            statsForm.ShowDialog();
        }

        /// <summary>
        /// Ruh hali kaydetme butonuna tıklandığında çalışır
        /// </summary>
        private void BtnMood_Click(object sender, EventArgs e)
        {
            var moodForm = new MoodForm();
            moodForm.ShowDialog();
        }

        /// <summary>
        /// AI sohbet butonuna tıklandığında çalışır
        /// </summary>
        private void BtnChat_Click(object sender, EventArgs e)
        {
            if (chatForm == null || chatForm.IsDisposed)
                chatForm = new ChatForm();
            chatForm.Show();
            chatForm.BringToFront();
        }

        /// <summary>
        /// AI tavsiye butonuna tıklandığında çalışır
        /// </summary>
        private void BtnOpenAIAdvice_Click(object sender, EventArgs e)
        {
            var aiAdviceForm = new AiAdviceForm(calendar.SelectionStart);
            aiAdviceForm.ShowDialog(this);
        }

        /// <summary>
        /// Sadece ruh hali analizi butonuna tıklandığında çalışır
        /// </summary>
        private void BtnFeelingAdvice_Click(object sender, EventArgs e)
        {
            using (var adviceForm = new AdviceForm(true)) // true = sadece ruh hali modu
            {
                adviceForm.ShowDialog(this);
            }
        }

        /// <summary>
        /// Hızlı tavsiye butonuna tıklandığında çalışır
        /// </summary>
        private void BtnGetAdvice_Click(object sender, EventArgs e)
        {
            using (var adviceForm = new AdviceForm(false)) // false = normal mod
            {
                adviceForm.ShowDialog(this);
            }
        }

        /// <summary>
        /// Bugün ruh hali kaydedilip kaydedilmediğini kontrol eder
        /// </summary>
        /// <returns>Bugün ruh hali kaydedildiyse true, yoksa false</returns>
        private bool IsMoodLoggedToday()
        {
            try
            {
                string query = "SELECT COUNT(*) FROM MoodEntries WHERE date(Timestamp) = @today";
                var count = Convert.ToInt32(DatabaseHelper.ExecuteScalar(query,
                    new SQLiteParameter("@today", DateTime.Today.ToString("yyyy-MM-dd"))));
                return count > 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Zamanlayıcıları başlatır (saat ve hava durumu)
        /// </summary>
        private void InitializeTimers()
        {
            // Saat zamanlayıcısı - her saniye günceller
            clockTimer = new Timer();
            clockTimer.Interval = 1000;
            clockTimer.Tick += ClockTimer_Tick;
            clockTimer.Start();

            // Hava durumu zamanlayıcısı - 30 dakikada bir günceller
            weatherTimer = new Timer();
            weatherTimer.Interval = 1800000;
            weatherTimer.Tick += WeatherTimer_Tick;
            weatherTimer.Start();

            // İlk hava durumu güncellemesi
            UpdateWeatherAsync();
        }

        /// <summary>
        /// Saat zamanlayıcısı - her saniye çalışır
        /// </summary>
        private void ClockTimer_Tick(object sender, EventArgs e)
        {
            lblClock.Text = DateTime.Now.ToString("HH:mm:ss");
        }

        /// <summary>
        /// Hava durumu zamanlayıcısı - 30 dakikada bir çalışır
        /// </summary>
        private async void WeatherTimer_Tick(object sender, EventArgs e)
        {
            await UpdateWeatherAsync();
        }

        /// <summary>
        /// Hava durumu bilgilerini günceller
        /// </summary>
        private async Task UpdateWeatherAsync()
        {
            try
            {
                var weatherData = await WeatherService.GetWeatherAsync("Istanbul");
                lblWeather.Text = weatherData.GetFormattedString();
            }
            catch (Exception ex)
            {
                lblWeather.Text = "Hava durumu bilgisi mevcut değil";
            }
        }

        /// <summary>
        /// Form kapanırken çalışır
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
        }
    }
}
