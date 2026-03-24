using System;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using Npgsql;
using System.Windows.Forms;

namespace Gazeta
{
    internal class DatabaseHelper
    {
        private static string connectionString = "Host=localhost;Port=5432;Database=GazetaDB;Username=postgres;Password=321;";

        public static class CurrentUser
        {
            public static int Id { get; set; }
            public static string Username { get; set; }
            public static string Role { get; set; }
            public static int? EmployeeId { get; set; }
            public static string FullName { get; set; }
            public static string Email { get; set; }

            public static bool IsAdmin => Role == "admin";
            public static bool IsEditor => Role == "admin" || Role == "editor";
            public static bool CanEdit => Role == "admin" || Role == "editor";
        }

        public static NpgsqlConnection GetConnection()
        {
            try
            {
                NpgsqlConnection connection = new NpgsqlConnection(connectionString);
                connection.Open();
                return connection;
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show($"Ошибка подключения: {ex.Message}", "Ошибка БД",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        public static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        // Вход по email
        public static bool Login(string email, string password)
        {
            try
            {
                string hash = HashPassword(password);

                string query = @"SELECT u.id, u.username, u.role, u.employee_id,
                                       e.last_name, e.first_name, e.middle_name, e.email
                                FROM users u
                                LEFT JOIN employees e ON u.employee_id = e.id
                                WHERE u.username = @email AND u.password_hash = @hash";

                NpgsqlParameter[] parameters = {
                    new NpgsqlParameter("@email", email),
                    new NpgsqlParameter("@hash", hash)
                };

                DataTable dt = GetDataTable(query, parameters);

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    CurrentUser.Id = Convert.ToInt32(row["id"]);
                    CurrentUser.Username = row["username"].ToString();
                    CurrentUser.Role = row["role"].ToString();
                    CurrentUser.Email = row["email"].ToString();

                    if (row["employee_id"] != DBNull.Value)
                        CurrentUser.EmployeeId = Convert.ToInt32(row["employee_id"]);

                    // Формируем ФИО
                    string lastName = row["last_name"]?.ToString() ?? "";
                    string firstName = row["first_name"]?.ToString() ?? "";
                    string middleName = row["middle_name"]?.ToString() ?? "";

                    if (!string.IsNullOrWhiteSpace(lastName) && !string.IsNullOrWhiteSpace(firstName))
                        CurrentUser.FullName = $"{lastName} {firstName} {middleName}".Trim();
                    else
                        CurrentUser.FullName = CurrentUser.Username;

                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка авторизации: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public static bool ExecuteNonQuery(string query, NpgsqlParameter[] parameters = null)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn == null) return false;
                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        if (parameters != null) cmd.Parameters.AddRange(parameters);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public static DataTable GetDataTable(string query, NpgsqlParameter[] parameters = null)
        {
            DataTable dt = new DataTable();
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn == null) return dt;
                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        if (parameters != null) cmd.Parameters.AddRange(parameters);
                        using (var adapter = new NpgsqlDataAdapter(cmd))
                        {
                            adapter.Fill(dt);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return dt;
        }

        public static void ExportToExcel(DataTable dataTable, string filePath)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < dataTable.Columns.Count; i++)
                {
                    sb.Append(dataTable.Columns[i].ColumnName);
                    if (i < dataTable.Columns.Count - 1) sb.Append(";");
                }
                sb.AppendLine();
                foreach (DataRow row in dataTable.Rows)
                {
                    for (int i = 0; i < dataTable.Columns.Count; i++)
                    {
                        string value = row[i].ToString();
                        if (value.Contains(";")) value = $"\"{value}\"";
                        sb.Append(value);
                        if (i < dataTable.Columns.Count - 1) sb.Append(";");
                    }
                    sb.AppendLine();
                }
                System.IO.File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
                MessageBox.Show($"Экспорт завершен!\nФайл: {filePath}", "Успешно",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}