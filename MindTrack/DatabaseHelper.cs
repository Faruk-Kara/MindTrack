using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;

namespace MindTrack
{
    /// <summary>
    /// Veritabanı yardımcı sınıfı
    /// SQLite veritabanı işlemlerini yönetir
    /// Veritabanı oluşturma, tablo oluşturma ve temel CRUD işlemleri
    /// </summary>
    public static class DatabaseHelper
    {
        // Veritabanı klasör ve dosya adları
        private static readonly string DB_FOLDER = "Database";
        private static readonly string DB_FILE = "mindtrack.db";
        private static readonly string DB_VERSION = "1.0";
        
        public static string GetDatabasePath()
        {
            string folderPath = Path.Combine(Application.StartupPath, DB_FOLDER);
            if (!Directory.Exists(folderPath))
            {
                try
                {
                    Directory.CreateDirectory(folderPath);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to create database directory: {ex.Message}", ex);
                }
            }
            return Path.Combine(folderPath, DB_FILE);
        }

        public static string GetConnectionString()
        {
            return $"Data Source={GetDatabasePath()};Version=3;";
        }

        public static void EnsureDatabaseInitialized()
        {
            string folderPath = Path.Combine(Application.StartupPath, "Database");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string dbPath = Path.Combine(folderPath, "mindtrack.db");
            string connStr = $"Data Source={dbPath};Version=3;";

            using (var conn = new SQLiteConnection(connStr))
            {
                conn.Open();

                // Always drop and recreate the Tasks table
                using (var cmd = new SQLiteCommand("DROP TABLE IF EXISTS Tasks;", conn)) cmd.ExecuteNonQuery();

                string createTaskTable = @"
                    CREATE TABLE Tasks (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Title TEXT NOT NULL,
                        Date TEXT NOT NULL,
                        Status TEXT NOT NULL,
                        CreatedAt TEXT NOT NULL
                    );";
                using (var cmd = new SQLiteCommand(createTaskTable, conn)) cmd.ExecuteNonQuery();

                // Repeat for other tables as needed...
                using (var cmd = new SQLiteCommand("DROP TABLE IF EXISTS MoodEntries;", conn)) cmd.ExecuteNonQuery();
                string createMoodTable = @"
                    CREATE TABLE MoodEntries (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Mood TEXT NOT NULL,
                        Timestamp TEXT NOT NULL
                    );";
                using (var cmd = new SQLiteCommand(createMoodTable, conn)) cmd.ExecuteNonQuery();

                using (var cmd = new SQLiteCommand("DROP TABLE IF EXISTS FocusSessions;", conn)) cmd.ExecuteNonQuery();
                string createFocusTable = @"
                    CREATE TABLE FocusSessions (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        StartTime TEXT NOT NULL,
                        EndTime TEXT NOT NULL,
                        DurationMinutes INTEGER NOT NULL
                    );";
                using (var cmd = new SQLiteCommand(createFocusTable, conn)) cmd.ExecuteNonQuery();
            }
        }

        private static void CreateVersionTable(SQLiteConnection conn)
        {
            string createVersionTable = @"
                CREATE TABLE IF NOT EXISTS DbVersion (
                    Version TEXT NOT NULL,
                    LastUpdated TEXT NOT NULL
                );";

            using (var cmd = new SQLiteCommand(createVersionTable, conn))
            {
                cmd.ExecuteNonQuery();
            }
        }

        private static bool ShouldRecreateTables(SQLiteConnection conn)
        {
            try
            {
                string query = "SELECT Version FROM DbVersion ORDER BY LastUpdated DESC LIMIT 1";
                using (var cmd = new SQLiteCommand(query, conn))
                {
                    var result = cmd.ExecuteScalar();
                    return result == null || result.ToString() != DB_VERSION;
                }
            }
            catch
            {
                return true;
            }
        }

        private static void UpdateDatabaseVersion(SQLiteConnection conn)
        {
            string updateVersion = @"
                INSERT INTO DbVersion (Version, LastUpdated)
                VALUES (@version, @timestamp);";

            using (var cmd = new SQLiteCommand(updateVersion, conn))
            {
                cmd.Parameters.AddWithValue("@version", DB_VERSION);
                cmd.Parameters.AddWithValue("@timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.ExecuteNonQuery();
            }
        }

        private static void DropAllTables(SQLiteConnection conn)
        {
            string[] tables = { "Tasks", "MoodEntries", "FocusSessions", "DbVersion" };
            
            foreach (string table in tables)
            {
                using (var cmd = new SQLiteCommand($"DROP TABLE IF EXISTS {table}", conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void ExecuteNonQuery(string query, params SQLiteParameter[] parameters)
        {
            using (var conn = new SQLiteConnection(GetConnectionString()))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand(query, conn))
                {
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static SQLiteDataReader ExecuteReader(string query, params SQLiteParameter[] parameters)
        {
            var conn = new SQLiteConnection(GetConnectionString());
            conn.Open();
            var cmd = new SQLiteCommand(query, conn);
            
            if (parameters != null)
            {
                cmd.Parameters.AddRange(parameters);
            }
            
            return cmd.ExecuteReader(CommandBehavior.CloseConnection);
        }

        public static object ExecuteScalar(string query, params SQLiteParameter[] parameters)
        {
            using (var conn = new SQLiteConnection(GetConnectionString()))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand(query, conn))
                {
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }
                    return cmd.ExecuteScalar();
                }
            }
        }

        public static void ExecuteInTransaction(Action<SQLiteConnection> action)
        {
            using (var conn = new SQLiteConnection(GetConnectionString()))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        action(conn);
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}
