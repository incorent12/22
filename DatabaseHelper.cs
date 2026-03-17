using System;
using System.Data;
using Npgsql;
using System.Windows.Forms;

namespace Gazeta
{
    internal class DatabaseHelper
    {
        // Пароли
        private static string connectionString = "Host=localhost;Port=5432;Database=GazetaDB;Username=postgres;Password=321;";

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
                MessageBox.Show($"Ошибка подключения к PostgreSQL: {ex.Message}\nПроверьте:\n1. Запущен ли сервер PostgreSQL\n2. Правильность логина/пароля\n3. Существует ли база данных",
                    "Ошибка БД", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Общая ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
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
                        if (parameters != null)
                        {
                            cmd.Parameters.AddRange(parameters);
                        }
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка выполнения запроса: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public static DataTable GetDataTable(string query, NpgsqlParameter[] parameters = null)
        {
            DataTable dt = new DataTable(); // Всегда создаем новую таблицу
            try
            {
                using (var conn = GetConnection())
                {
                    if (conn == null) return dt; // Возвращаем пустую таблицу, а не null

                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        if (parameters != null)
                        {
                            cmd.Parameters.AddRange(parameters);
                        }

                        using (var adapter = new NpgsqlDataAdapter(cmd))
                        {
                            adapter.Fill(dt);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка получения данных: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return dt; // Всегда возвращаем таблицу (пустую или с данными)
        }
    }
}