using Microsoft.Data.Sqlite;
using MyHybridApp.Helper;
using MyHybridApp.Models.TaskModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHybridApp.Repository
{
    public static class ResultRepository
    {
        private const string DbPath = "results.db";


        public static void SaveResult(TaskExpression task)
        {
            try
            {
                using var conn = new SqliteConnection($"Data Source={DbPath}");
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
            INSERT OR IGNORE INTO Results (TaskId, Expression, Result, Type, TargetClientId, Timestamp)
            VALUES ($taskId, $expression, $result, $type, $targetClientId, $ts)";

                cmd.Parameters.AddWithValue("$taskId", task.TaskId);
                cmd.Parameters.AddWithValue("$expression", task.Expression);
                cmd.Parameters.AddWithValue("$result", task.Result);
                cmd.Parameters.AddWithValue("$type", task.Type);
                cmd.Parameters.AddWithValue("$targetClientId", task.TargetClientId);
                cmd.Parameters.AddWithValue("$ts", task.Timestamp);
                cmd.ExecuteNonQuery();
                Logger.Log($"✅ Завдання {task.TaskId} збережено");
            }
            catch (Exception ex)
            {
                Logger.Log($"[SQLite ERROR][SaveResult] {ex.Message}\n");
            }
        }

        public static int GetResultCount(string taskId)
        {
            try
            {
                using var conn = new SqliteConnection($"Data Source={DbPath}");
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT COUNT(*) FROM Results WHERE TaskId = $id";
                cmd.Parameters.AddWithValue("$id", taskId);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch (Exception ex)
            {
                Logger.Log($"[SQLite ERROR][GetResultCount] {ex.Message}\n");
                return 0;
            }
        }

        public static int GetAllResultCount()
        {
            try
            {
                using var conn = new SqliteConnection($"Data Source={DbPath}");
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT COUNT(*) FROM Results";
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch (Exception ex)
            {
                Logger.Log($"[SQLite ERROR][GetAllResultCount] {ex.Message}\n");
                return 0;
            }
        }

        public static List<TaskExpression> GetResultsByTaskId(string taskId)
        {
            var results = new List<TaskExpression>();

            try
            {
                using var conn = new SqliteConnection($"Data Source={DbPath}");
                conn.Open();

                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                SELECT TaskId, Expression, Result, Type, TargetClientId, Timestamp
                FROM Results
                WHERE TaskId = $id";
                cmd.Parameters.AddWithValue("$id", taskId);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    results.Add(new TaskExpression
                    {
                        TaskId = reader.GetString(0),
                        Expression = reader.GetString(1),
                        Result = reader.GetString(2),
                        Type = Enum.Parse<MyTaskType>(reader.GetString(3)),
                        TargetClientId = reader.GetString(4),
                        Timestamp = DateTime.Parse(reader.GetString(5))
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"[SQLite ERROR][GetResults] {ex.Message}\n");
            }

            return results;
        }

        public static void DeleteAllResultsByTaskId(string taskId)
        {
            try
            {
                using var conn = new SqliteConnection($"Data Source={DbPath}");
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM Results WHERE TaskId = $id";
                cmd.Parameters.AddWithValue("$id", taskId);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Logger.Log($"[DB ERROR] Clear ByTaskId failed: {ex.Message}\n");
            }
        }


        public static List<TaskExpression> GetResultsByClientId(string clientId)
        {
            var results = new List<TaskExpression>();

            try
            {
                using var conn = new SqliteConnection($"Data Source={DbPath}");
                conn.Open();

                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                SELECT TaskId, Expression, Result, Type, TargetClientId, Timestamp
                FROM Results
                WHERE TargetClientId = $id";
                cmd.Parameters.AddWithValue("$id", clientId);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    results.Add(new TaskExpression
                    {
                        TaskId = reader.GetString(0),
                        Expression = reader.GetString(1),
                        Result = reader.GetString(2),
                        Type = Enum.Parse<MyTaskType>(reader.GetString(3)),
                        TargetClientId = reader.GetString(4),
                        Timestamp = DateTime.Parse(reader.GetString(5))
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"[SQLite ERROR][GetResults] {ex.Message}\n");
            }

            return results;
        }

        public static List<TaskExpression> GetAllResults()
        {
            var results = new List<TaskExpression>();

            try
            {
                using var conn = new SqliteConnection($"Data Source={DbPath}");
                conn.Open();

                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                SELECT TaskId, Expression, Result, Type, TargetClientId, Timestamp
                FROM Results";

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    results.Add(new TaskExpression
                    {
                        TaskId = reader.GetString(0),
                        Expression = reader.GetString(1),
                        Result = reader.GetString(2),
                        Type = Enum.Parse<MyTaskType>(reader.GetString(3)),
                        TargetClientId = reader.GetString(4),
                        Timestamp = DateTime.Parse(reader.GetString(5))
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"[SQLite ERROR][GetResults] {ex.Message}\n");
            }

            return results;
        }

        public static void ClearDatabaseTables()
        {
            try
            {
                using var conn = new SqliteConnection("Data Source=results.db");
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM Tasks; DELETE FROM Results;";
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Logger.Log($"[DB ERROR] Clear failed: {ex.Message}\n");
            }
        }
    }

}
