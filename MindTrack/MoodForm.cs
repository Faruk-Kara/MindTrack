using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SQLite;
using System.IO;

namespace MindTrack
{
    /// <summary>
    /// Ruh hali kaydetme formu
    /// Kullanıcının günlük ruh halini seçip veritabanına kaydetmesini sağlar
    /// </summary>
    public partial class MoodForm : Form
    {
        // Form kontrolleri
        private Label lblTitle;        // Başlık etiketi
        private Label lblQuestion;     // Soru etiketi
        private FlowLayoutPanel moodPanel;  // Ruh hali butonlarının bulunduğu panel
        private Button btnSave;        // Kaydetme butonu
        private string selectedMood;   // Seçilen ruh hali

        /// <summary>
        /// Form yapıcı metodu
        /// </summary>
        public MoodForm()
        {
            InitializeComponent();
            InitializeMoodForm();
        }

        /// <summary>
        /// Ruh hali formunun arayüzünü hazırlar
        /// </summary>
        private void InitializeMoodForm()
        {
            // Form temel ayarları
            this.Text = "Nasıl hissediyorsun?";
            this.Size = new Size(600, 500);
            this.MinimumSize = new Size(500, 400);
            this.BackColor = Color.FromArgb(240, 240, 240);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            // Başlık etiketi
            lblTitle = new Label
            {
                Text = "Ruh Hali Takipçisi",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 60
            };

            // Soru etiketi
            lblQuestion = new Label
            {
                Text = "Bugün nasıl hissediyorsun?",
                Font = new Font("Segoe UI", 14),
                ForeColor = Color.FromArgb(44, 62, 80),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 40
            };

            // Ruh hali seçim paneli
            moodPanel = new FlowLayoutPanel
            {
                Location = new Point(20, 120),
                Size = new Size(540, 280),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoScroll = true,
                Padding = new Padding(10),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Ruh hali seçenekleri (emoji + açıklama)
            string[] moods = new[]
            {
                "😊 Mutlu",
                "😌 Sakin",
                "😔 Üzgün",
                "😡 Kızgın",
                "😰 Endişeli",
                "😴 Yorgun",
                "😎 Kendinden Emin",
                "😕 Kafası Karışık",
                "😍 Heyecanlı",
                "😶 Nötr"
            };

            // Her ruh hali için buton oluştur
            foreach (string mood in moods)
            {
                var btn = new Button
                {
                    Text = mood,
                    Size = new Size(120, 80),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10),
                    Margin = new Padding(5),
                    BackColor = Color.White,
                    FlatAppearance = { BorderColor = Color.LightGray, BorderSize = 1 }
                };
                btn.Click += MoodButton_Click;  // Tıklama olayını bağla
                moodPanel.Controls.Add(btn);
            }

            // Kaydetme butonu
            btnSave = new Button
            {
                Text = "Ruh Halini Kaydet",
                Size = new Size(200, 40),
                Location = new Point(200, 420),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12),
                Enabled = false  // Başlangıçta pasif
            };
            btnSave.Click += BtnSave_Click;

            // Tüm kontrolleri forma ekle
            this.Controls.AddRange(new Control[] { lblTitle, lblQuestion, moodPanel, btnSave });
        }

        /// <summary>
        /// Ruh hali butonuna tıklandığında çalışır
        /// Seçilen butonu vurgular, diğerlerini normal haline getirir
        /// </summary>
        private void MoodButton_Click(object sender, EventArgs e)
        {
            // Önce tüm butonları normal haline getir
            foreach (Control control in moodPanel.Controls)
            {
                if (control is Button btn)
                {
                    btn.BackColor = Color.White;
                    btn.FlatAppearance.BorderColor = Color.LightGray;
                }
            }

            // Seçilen butonu vurgula
            if (sender is Button selectedBtn)
            {
                selectedBtn.BackColor = Color.FromArgb(52, 152, 219);
                selectedBtn.ForeColor = Color.White;
                selectedBtn.FlatAppearance.BorderColor = Color.FromArgb(41, 128, 185);
                selectedMood = selectedBtn.Text;  // Seçilen ruh halini kaydet
                btnSave.Enabled = true;  // Kaydet butonunu aktif et
            }
        }

        /// <summary>
        /// Kaydet butonuna tıklandığında çalışır
        /// Seçilen ruh halini veritabanına kaydeder
        /// </summary>
        private void BtnSave_Click(object sender, EventArgs e)
        {
            // Ruh hali seçilmemişse işlem yapma
            if (string.IsNullOrEmpty(selectedMood))
                return;

            try
            {
                // Veritabanı bağlantısı kur
                string dbPath = Path.Combine(Application.StartupPath, "Database", "mindtrack.db");
                string connStr = $"Data Source={dbPath};Version=3;";

                using (var conn = new SQLiteConnection(connStr))
                {
                    conn.Open();
                    
                    // Ruh halini veritabanına kaydet
                    string query = @"
                        INSERT INTO MoodEntries (Mood, Timestamp)
                        VALUES (@mood, @timestamp)";

                    using (var cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@mood", selectedMood);
                        cmd.Parameters.AddWithValue("@timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        cmd.ExecuteNonQuery();
                    }
                }

                // Başarı mesajı göster ve formu kapat
                MessageBox.Show("Ruh hali başarıyla kaydedildi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                // Hata durumunda kullanıcıya bilgi ver
                MessageBox.Show($"Ruh hali kaydedilirken hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}