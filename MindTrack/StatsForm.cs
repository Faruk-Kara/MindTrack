using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SQLite;
using System.IO;
using System.Windows.Forms.DataVisualization.Charting;
using System.Collections.Generic;
using System.Linq;

namespace MindTrack
{
    public partial class StatsForm : Form
    {
        private TabControl tabControl;
        private Chart focusChart;
        private Chart moodChart;
        private Chart taskChart;
        private DateTimePicker startDatePicker;
        private DateTimePicker endDatePicker;
        private Button btnUpdate;

        public StatsForm()
        {
            InitializeComponent();
            InitializeStatsForm();
            LoadData();
        }

        private void InitializeStatsForm()
        {
            this.Text = "Verimlilik İstatistikleri";
            this.Size = new Size(1000, 700);
            this.MinimumSize = new Size(800, 600);
            this.BackColor = Color.FromArgb(240, 240, 240);

            // Date Range Panel
            var datePanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            var lblStart = new Label
            {
                Text = "Başlangıç Tarihi:",
                AutoSize = true,
                Location = new Point(10, 15),
                Font = new Font("Segoe UI", 10)
            };

            startDatePicker = new DateTimePicker
            {
                Location = new Point(130, 12),
                Width = 150,
                Format = DateTimePickerFormat.Short
            };

            var lblEnd = new Label
            {
                Text = "Bitiş Tarihi:",
                AutoSize = true,
                Location = new Point(300, 15),
                Font = new Font("Segoe UI", 10)
            };

            endDatePicker = new DateTimePicker
            {
                Location = new Point(390, 12),
                Width = 150,
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today
            };

            btnUpdate = new Button
            {
                Text = "Güncelle",
                Location = new Point(560, 10),
                Size = new Size(100, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10)
            };
            btnUpdate.Click += BtnUpdate_Click;

            datePanel.Controls.AddRange(new Control[] { lblStart, startDatePicker, lblEnd, endDatePicker, btnUpdate });

            // Tab Control
            tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Padding = new Point(10, 10)
            };

            // Focus Sessions Tab
            var focusTab = new TabPage("Odaklanma Oturumları");
            focusChart = new Chart
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };
            focusChart.ChartAreas.Add(new ChartArea("FocusArea"));
            focusChart.Legends.Add(new Legend("FocusLegend"));
            focusTab.Controls.Add(focusChart);

            // Mood Tracking Tab
            var moodTab = new TabPage("Ruh Hali Takibi");
            moodChart = new Chart
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };
            moodChart.ChartAreas.Add(new ChartArea("MoodArea"));
            moodChart.Legends.Add(new Legend("MoodLegend"));
            moodTab.Controls.Add(moodChart);

            // Task Completion Tab
            var taskTab = new TabPage("Görev Tamamlama");
            taskChart = new Chart
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };
            taskChart.ChartAreas.Add(new ChartArea("TaskArea"));
            taskChart.Legends.Add(new Legend("TaskLegend"));
            taskTab.Controls.Add(taskChart);

            tabControl.TabPages.AddRange(new TabPage[] { focusTab, moodTab, taskTab });

            this.Controls.AddRange(new Control[] { datePanel, tabControl });
        }

        private void LoadData()
        {
            try
            {
                string dbPath = Path.Combine(Application.StartupPath, "Database", "mindtrack.db");
                string connStr = $"Data Source={dbPath};Version=3;";

                using (var conn = new SQLiteConnection(connStr))
                {
                    conn.Open();
                    LoadFocusData(conn);
                    LoadMoodData(conn);
                    LoadTaskData(conn);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"İstatistikler yüklenirken hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadFocusData(SQLiteConnection conn)
        {
            focusChart.Series.Clear();
            var series = new Series("Odaklanma Süresi")
            {
                ChartType = SeriesChartType.Column,
                Color = Color.FromArgb(52, 152, 219)
            };

            string query = @"
                SELECT date(StartTime) as Date, SUM(DurationMinutes) as TotalMinutes
                FROM FocusSessions
                WHERE date(StartTime) BETWEEN @start AND @end
                GROUP BY date(StartTime)
                ORDER BY date(StartTime)";

            using (var cmd = new SQLiteCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@start", startDatePicker.Value.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@end", endDatePicker.Value.ToString("yyyy-MM-dd"));

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var date = DateTime.Parse(reader["Date"].ToString());
                        var minutes = Convert.ToDouble(reader["TotalMinutes"]);
                        series.Points.AddXY(date.ToShortDateString(), minutes);
                    }
                }
            }

            focusChart.Series.Add(series);
            focusChart.ChartAreas[0].AxisX.Title = "Tarih";
            focusChart.ChartAreas[0].AxisY.Title = "Dakika";
            focusChart.ChartAreas[0].AxisX.Interval = 1;
        }

        private void LoadMoodData(SQLiteConnection conn)
        {
            moodChart.Series.Clear();
            var series = new Series("Ruh Hali Dağılımı")
            {
                ChartType = SeriesChartType.Pie,
                IsValueShownAsLabel = true
            };

            string query = @"
                SELECT Mood, COUNT(*) as Count
                FROM MoodEntries
                WHERE date(Timestamp) BETWEEN @start AND @end
                GROUP BY Mood
                ORDER BY Count DESC";

            using (var cmd = new SQLiteCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@start", startDatePicker.Value.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@end", endDatePicker.Value.ToString("yyyy-MM-dd"));

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var mood = reader["Mood"].ToString();
                        var count = Convert.ToInt32(reader["Count"]);
                        series.Points.AddXY(mood, count);
                    }
                }
            }

            moodChart.Series.Add(series);
            moodChart.ChartAreas[0].AxisX.Title = "Ruh Hali";
            moodChart.ChartAreas[0].AxisY.Title = "Sayı";
        }

        private void LoadTaskData(SQLiteConnection conn)
        {
            taskChart.Series.Clear();
            var series = new Series("Görev Tamamlama")
            {
                ChartType = SeriesChartType.StackedColumn,
                Color = Color.FromArgb(46, 204, 113)
            };

            string query = @"
                SELECT date(Date) as Date,
                       SUM(CASE WHEN Status = 'Completed' THEN 1 ELSE 0 END) as Completed,
                       SUM(CASE WHEN Status = 'Pending' THEN 1 ELSE 0 END) as Pending
                FROM Tasks
                WHERE date(Date) BETWEEN @start AND @end
                GROUP BY date(Date)
                ORDER BY date(Date)";

            using (var cmd = new SQLiteCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@start", startDatePicker.Value.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@end", endDatePicker.Value.ToString("yyyy-MM-dd"));

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var date = DateTime.Parse(reader["Date"].ToString());
                        var completed = Convert.ToInt32(reader["Completed"]);
                        var pending = Convert.ToInt32(reader["Pending"]);
                        series.Points.AddXY(date.ToShortDateString(), completed);
                    }
                }
            }

            taskChart.Series.Add(series);
            taskChart.ChartAreas[0].AxisX.Title = "Tarih";
            taskChart.ChartAreas[0].AxisY.Title = "Görevler";
            taskChart.ChartAreas[0].AxisX.Interval = 1;
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            LoadData();
        }
    }
}