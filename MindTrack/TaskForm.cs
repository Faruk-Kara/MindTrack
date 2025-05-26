using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Collections.Generic;

namespace MindTrack
{
    public partial class TaskForm : Form
    {
        private ListView taskListView;
        private TextBox taskInput;
        private Button addButton;
        private Button deleteButton;
        private Button saveButton;
        private DateTime selectedDate;
        private List<TaskItem> tasks;

        public TaskForm(DateTime date)
        {
            InitializeComponent();
            selectedDate = date;
            tasks = new List<TaskItem>();
            InitializeTaskForm();
            LoadTasks();
        }

        private void InitializeTaskForm()
        {
            this.Text = $"{selectedDate.ToShortDateString()} Görevleri";
            this.Size = new Size(600, 500);
            this.MinimumSize = new Size(500, 400);
            this.BackColor = Color.FromArgb(240, 240, 240);

            // Task List
            taskListView = new ListView
            {
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10)
            };
            taskListView.Columns.Add("Görev", 400);
            taskListView.Columns.Add("Durum", 100);

            // Input Panel
            var inputPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            // Task Input
            taskInput = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };
            taskInput.KeyPress += TaskInput_KeyPress;

            // Add Button
            addButton = new Button
            {
                Text = "Ekle",
                Size = new Size(80, 40),
                Dock = DockStyle.Right,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                Margin = new Padding(5, 0, 0, 0)
            };
            addButton.Click += AddButton_Click;

            // Delete Button
            deleteButton = new Button
            {
                Text = "Sil",
                Size = new Size(80, 40),
                Dock = DockStyle.Right,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                Margin = new Padding(5, 0, 0, 0)
            };
            deleteButton.Click += DeleteButton_Click;

            // Save Button
            saveButton = new Button
            {
                Text = "Kaydet",
                Size = new Size(80, 40),
                Dock = DockStyle.Right,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                Margin = new Padding(5, 0, 0, 0)
            };
            saveButton.Click += SaveButton_Click;

            inputPanel.Controls.AddRange(new Control[] { taskInput, saveButton, deleteButton, addButton });

            // Main Container
            var container = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };
            container.Controls.Add(taskListView);

            this.Controls.AddRange(new Control[] { container, inputPanel });
        }

        private void LoadTasks()
        {
            try
            {
                string query = @"
                    SELECT Id, Title, Status, CreatedAt 
                    FROM Tasks 
                    WHERE Date = @date 
                    ORDER BY CreatedAt DESC";

                using (var reader = DatabaseHelper.ExecuteReader(query, 
                    new SQLiteParameter("@date", selectedDate.ToString("yyyy-MM-dd"))))
                {
                    tasks.Clear();
                    taskListView.Items.Clear();

                    while (reader.Read())
                    {
                        var task = new TaskItem
                        {
                            Id = reader.GetInt32(0),
                            Title = reader.GetString(1),
                            Status = reader.GetString(2),
                            CreatedAt = DateTime.Parse(reader.GetString(3))
                        };
                        tasks.Add(task);

                        var item = new ListViewItem(task.Title);
                        
                        // Türkçe durum çevirisi
                        string turkishStatus = task.Status;
                        switch (task.Status.ToLower())
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
                        taskListView.Items.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Görevler yüklenirken hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveTasks()
        {
            try
            {
                DatabaseHelper.ExecuteInTransaction(conn =>
                {
                    // Delete existing tasks for the date
                    string deleteQuery = "DELETE FROM Tasks WHERE Date = @date";
                    using (var cmd = new SQLiteCommand(deleteQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@date", selectedDate.ToString("yyyy-MM-dd"));
                        cmd.ExecuteNonQuery();
                    }

                    // Insert updated tasks
                    string insertQuery = @"
                        INSERT INTO Tasks (Title, Date, Status, CreatedAt)
                        VALUES (@title, @date, @status, @createdAt)";

                    foreach (var task in tasks)
                    {
                        using (var cmd = new SQLiteCommand(insertQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@title", task.Title);
                            cmd.Parameters.AddWithValue("@date", selectedDate.ToString("yyyy-MM-dd"));
                            cmd.Parameters.AddWithValue("@status", task.Status);
                            cmd.Parameters.AddWithValue("@createdAt", task.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
                            cmd.ExecuteNonQuery();
                        }
                    }
                });

                MessageBox.Show("Görevler başarıyla kaydedildi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Görevler kaydedilirken hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            AddTask();
        }

        private void TaskInput_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                AddTask();
            }
        }

        private void AddTask()
        {
            if (string.IsNullOrWhiteSpace(taskInput.Text))
                return;

            var task = new TaskItem
            {
                Title = taskInput.Text.Trim(),
                Status = "Pending",
                CreatedAt = DateTime.Now
            };
            tasks.Add(task);

            var item = new ListViewItem(task.Title);
            item.SubItems.Add("Bekliyor"); // Türkçe durum
            taskListView.Items.Add(item);

            // Save immediately to DB
            try
            {
                string insertQuery = @"
                    INSERT INTO Tasks (Title, Date, Status, CreatedAt) 
                    VALUES (@title, @date, @status, @createdAt)";

                DatabaseHelper.ExecuteNonQuery(insertQuery,
                    new SQLiteParameter("@title", task.Title),
                    new SQLiteParameter("@date", selectedDate.ToString("yyyy-MM-dd")),
                    new SQLiteParameter("@status", task.Status),
                    new SQLiteParameter("@createdAt", task.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"))
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Görev eklenirken hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            taskInput.Clear();
            taskInput.Focus();
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            if (taskListView.SelectedItems.Count == 0)
                return;

            var selectedIndex = taskListView.SelectedIndices[0];
            tasks.RemoveAt(selectedIndex);
            taskListView.Items.RemoveAt(selectedIndex);
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            SaveTasks();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (tasks.Count > 0)
            {
                var result = MessageBox.Show(
                    "Değişikliklerinizi kaydetmek istiyor musunuz?",
                    "Değişiklikleri Kaydet",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    SaveTasks();
                }
                else if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
            }

            base.OnFormClosing(e);
        }
    }

    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}