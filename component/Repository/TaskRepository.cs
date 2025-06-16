using Microsoft.Data.Sqlite;
using MyHybridApp.Helper;
using MyHybridApp.Models.TaskModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;


namespace MyHybridApp.Repository
{
    public static class TaskRepository
    {
        private const string DbPath = "results.db";

        public static void CreateTaskTable()
        {
            try
            {
                if (!File.Exists(DbPath))
                {
                    using var conn = new SqliteConnection($"Data Source={DbPath}");
                    conn.Open();
                    var cmd = conn.CreateCommand();

                    cmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Tasks (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        TaskId TEXT KEY,
                        Expression TEXT NOT NULL,
                        CountOfSubTasks INTEGER,
                        Result TEXT,
                        Status TEXT NOT NULL,
                        Type TEXT NOT NULL,
                        Timestamp TEXT NOT NULL,
                        UNIQUE(TaskId)
                    );";
                    cmd.ExecuteNonQuery();

                    Logger.Log("Tasks table created");

                    cmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Results (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        TaskId TEXT NOT NULL,
                        Expression TEXT NOT NULL,
                        Type TEXT NOT NULL,
                        Result TEXT NOT NULL,
                        TargetClientId TEXT NOT NULL,
                        Timestamp TEXT NOT NULL,
                        UNIQUE(Id)
                    );";
                    cmd.ExecuteNonQuery();

                    Logger.Log("Results table created");

                }
            }
            catch (Exception ex)
            {
                Logger.Log($"[SQLite ERROR][TaskRepository Init] {ex.Message}\n");
            }
        }

        public static async Task AddOrUpdateTask(MyTask task)
        {
            try
            {
                using var conn = new SqliteConnection($"Data Source={DbPath}");
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
            INSERT INTO Tasks (TaskId, Expression, CountOfSubTasks, Result, Status, Type, Timestamp)
            VALUES ($id, $expr, $count, $result, $status, $type, $ts)
            ON CONFLICT(TaskId) DO UPDATE SET
                Status = $status,
                Timestamp = $ts;
            ";
                cmd.Parameters.AddWithValue("$id", task.TaskId);
                cmd.Parameters.AddWithValue("$expr", task.Expression);
                cmd.Parameters.AddWithValue("$count", task.CountOfSubTasks);
                cmd.Parameters.AddWithValue("$result", task.Result);
                cmd.Parameters.AddWithValue("$status", task.Status);
                cmd.Parameters.AddWithValue("$type", task.Type);
                cmd.Parameters.AddWithValue("$ts", task.Timestamp);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Logger.Log($"[SQLite ERROR][AddOrUpdateTask] {ex.Message}\n");
            }
        }

        public static void UpdateTask(MyTask task)
        {
            try
            {
                using var conn = new SqliteConnection($"Data Source=results.db");
                conn.Open();

                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
            UPDATE Tasks
            SET Expression = $expr,
                Result = $result,
                CountOfSubTasks = $count,
                Type = $type,
                Status = $status,
                Timestamp = $ts
            WHERE TaskId = $id";

                cmd.Parameters.AddWithValue("$id", task.TaskId);
                cmd.Parameters.AddWithValue("$expr", task.Expression);
                cmd.Parameters.AddWithValue("$count", task.CountOfSubTasks);
                cmd.Parameters.AddWithValue("$result", task.Result);
                cmd.Parameters.AddWithValue("$status", task.Status);
                cmd.Parameters.AddWithValue("$type", task.Type);
                cmd.Parameters.AddWithValue("$ts", task.Timestamp);

                int affected = cmd.ExecuteNonQuery();
                Logger.Log($"[SQLite] UPDATED Tasks → {affected} rows");
            }
            catch (Exception ex)
            {
                Logger.Log($"[SQLite ERROR][UpdateTask] {ex.Message}\n");
            }
        }


        public static MyTask? GetTaskById(string taskId)
        {
            try
            {
                using var conn = new SqliteConnection($"Data Source={DbPath}");
                conn.Open();

                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                SELECT TaskId, Expression, CountOfSubTasks, Result, Status, Type, Timestamp
                FROM Tasks
                WHERE TaskId = $id";
                cmd.Parameters.AddWithValue("$id", taskId);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return new MyTask
                    {
                        TaskId = reader.GetString(0),
                        Expression = reader.GetString(1),
                        CountOfSubTasks = reader.GetInt32(2),
                        Result = reader.GetString(3),
                        Status = Enum.Parse<MyTaskStatus>(reader.GetString(4)),
                        Type = Enum.Parse<MyTaskType>(reader.GetString(5)),
                        Timestamp = DateTime.Parse(reader.GetString(6))
                    };
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"[SQLite ERROR][GetTaskById] {ex.Message}\n");
            }

            return null;
        }

        public static int GetAllTaskCount()
        {
            try
            {
                using var conn = new SqliteConnection($"Data Source={DbPath}");
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT COUNT(*) FROM Tasks";
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch (Exception ex)
            {
                Logger.Log($"[SQLite ERROR][GetAllTaskCount] {ex.Message}\n");
                return 0;
            }
        }

        public static void UpdateStatus(string taskId, MyTaskStatus newStatus)
        {
            try
            {
                using var conn = new SqliteConnection($"Data Source={DbPath}");
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "UPDATE Tasks SET Status = $status WHERE TaskId = $id";
                cmd.Parameters.AddWithValue("$status", newStatus);
                cmd.Parameters.AddWithValue("$id", taskId);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Logger.Log($"[SQLite ERROR][UpdateStatus] {ex.Message}\n");
            }
        }

        public static void UpdateStatusAndResult(string taskId, MyTaskStatus newStatus, string result)
        {
            try
            {
                using var conn = new SqliteConnection($"Data Source={DbPath}");
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "UPDATE Tasks SET Status = $status, Result = $result WHERE TaskId = $id";
                cmd.Parameters.AddWithValue("$status", newStatus);
                cmd.Parameters.AddWithValue("$result", result);
                cmd.Parameters.AddWithValue("$id", taskId);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Logger.Log($"[SQLite ERROR][UpdateStatus] {ex.Message}\n");
            }
        }


        public static List<MyTask> GetAllNotReadyTasks()
        {
            var list = new List<MyTask>();

            try
            {
                using var conn = new SqliteConnection($"Data Source=results.db");
                conn.Open();

                var cmd = conn.CreateCommand();
                cmd.CommandText =
                    "SELECT TaskId, Expression, Result, CountOfSubTasks, Status, Type, Timestamp FROM Tasks WHERE Status != 'Success'";
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new MyTask
                    {
                        TaskId = reader.GetString(0),
                        Expression = reader.GetString(1),
                        Result = reader.GetString(2),
                        CountOfSubTasks = reader.GetInt32(3),
                        Status = Enum.Parse<MyTaskStatus>(reader.GetString(4)),
                        Type = Enum.Parse<MyTaskType>(reader.GetString(5)),
                        Timestamp = DateTime.Parse(reader.GetString(6))
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"[SQLite ERROR][GetAllNotReadyTasks] {ex.Message}\n");
            }

            return list;
        }


        public static List<MyTask> GetAllTasks()
        {
            var list = new List<MyTask>();

            try
            {
                using var conn = new SqliteConnection($"Data Source=results.db");
                conn.Open();

                var cmd = conn.CreateCommand();
                cmd.CommandText =
                    "SELECT TaskId, Expression, Result, CountOfSubTasks, Status, Type, Timestamp FROM Tasks";
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new MyTask
                    {
                        TaskId = reader.GetString(0),
                        Expression = reader.GetString(1),
                        Result = reader.GetString(2),
                        CountOfSubTasks = reader.GetInt32(3),
                        Status = Enum.Parse<MyTaskStatus>(reader.GetString(4)),
                        Type = Enum.Parse<MyTaskType>(reader.GetString(5)),
                        Timestamp = DateTime.Parse(reader.GetString(6))
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"[SQLite ERROR][GetAllTasks] {ex.Message}\n");
            }

            return list;
        }


    }

}
