using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using Npgsql;

namespace Gazeta
{
    public partial class FormIssueComposition : Form
    {
        private int issueId;
        private ListBox lbAvailable, lbSelected;
        private Button btnAdd, btnRemove, btnSave, btnCancel;

        public FormIssueComposition(int issueId)
        {
            InitializeComponent();
            this.issueId = issueId;
            this.Text = "Состав выпуска";
            this.Size = new System.Drawing.Size(500, 400);
            this.StartPosition = FormStartPosition.CenterParent;

            CreateControls();
            LoadData();
        }

        private void CreateControls()
        {
            Label lblAvailable = new Label()
            {
                Text = "Доступные статьи:",
                Left = 20,
                Top = 20,
                Width = 200
            };
            lbAvailable = new ListBox()
            {
                Left = 20,
                Top = 40,
                Width = 200,
                Height = 250,
                SelectionMode = SelectionMode.MultiExtended
            };

            Label lblSelected = new Label()
            {
                Text = "Статьи в выпуске:",
                Left = 280,
                Top = 20,
                Width = 200
            };
            lbSelected = new ListBox()
            {
                Left = 280,
                Top = 40,
                Width = 200,
                Height = 250,
                SelectionMode = SelectionMode.MultiExtended
            };

            btnAdd = new Button()
            {
                Text = ">",
                Left = 230,
                Top = 100,
                Width = 40,
                Height = 30
            };
            btnAdd.Click += (s, e) => MoveItems(lbAvailable, lbSelected);

            btnRemove = new Button()
            {
                Text = "<",
                Left = 230,
                Top = 140,
                Width = 40,
                Height = 30
            };
            btnRemove.Click += (s, e) => MoveItems(lbSelected, lbAvailable);

            btnSave = new Button()
            {
                Text = "Сохранить",
                Left = 150,
                Top = 310,
                Width = 100,
                Height = 30
            };
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button()
            {
                Text = "Закрыть",
                Left = 260,
                Top = 310,
                Width = 100,
                Height = 30
            };
            btnCancel.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] {
                lblAvailable, lbAvailable,
                lblSelected, lbSelected,
                btnAdd, btnRemove, btnSave, btnCancel
            });
        }

        private void LoadData()
        {
            try
            {
                // Загружаем все статьи (доступные для добавления)
                string allQuery = @"SELECT a.id, a.title, 
                                           e.last_name || ' ' || e.first_name || ' ' || COALESCE(e.middle_name, '') as author
                                   FROM articles a
                                   LEFT JOIN employees e ON a.author_id = e.id
                                   WHERE a.status = 'Опубликовано'
                                   ORDER BY a.created_at DESC";

                DataTable allArticles = DatabaseHelper.GetDataTable(allQuery);

                // Настройка ListBox для доступных статей
                lbAvailable.DataSource = null;
                lbAvailable.DisplayMember = "title";
                lbAvailable.ValueMember = "id";
                lbAvailable.DataSource = allArticles;

                // Загружаем статьи уже в выпуске
                string selectedQuery = @"SELECT a.id, a.title, 
                                                e.last_name || ' ' || e.first_name || ' ' || COALESCE(e.middle_name, '') as author
                                        FROM articles a
                                        JOIN issue_articles ia ON a.id = ia.article_id
                                        LEFT JOIN employees e ON a.author_id = e.id
                                        WHERE ia.issue_id = @issueId
                                        ORDER BY a.title";

                NpgsqlParameter[] parameters = { new NpgsqlParameter("@issueId", issueId) };
                DataTable selectedArticles = DatabaseHelper.GetDataTable(selectedQuery, parameters);

                // Настройка ListBox для статей в выпуске
                lbSelected.DataSource = null;
                lbSelected.DisplayMember = "title";
                lbSelected.ValueMember = "id";
                lbSelected.DataSource = selectedArticles;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private void MoveItems(ListBox source, ListBox destination)
        {
            if (source.SelectedItems.Count == 0) return;

            try
            {
                DataTable sourceTable = (DataTable)source.DataSource;
                DataTable destTable;

                if (destination.DataSource == null)
                {
                    destTable = sourceTable.Clone();
                }
                else
                {
                    destTable = (DataTable)destination.DataSource;
                }

                // Собираем выделенные элементы
                List<object> selectedIds = new List<object>();

                foreach (DataRowView rowView in source.SelectedItems)
                {
                    selectedIds.Add(rowView["id"]);

                    // Добавляем в destination
                    DataRow newRow = destTable.NewRow();
                    newRow["id"] = rowView["id"];
                    newRow["title"] = rowView["title"];
                    newRow["author"] = rowView["author"];
                    destTable.Rows.Add(newRow);
                }

                // Удаляем из source (итерация с конца)
                for (int i = sourceTable.Rows.Count - 1; i >= 0; i--)
                {
                    if (selectedIds.Contains(sourceTable.Rows[i]["id"]))
                    {
                        sourceTable.Rows.RemoveAt(i);
                    }
                }

                sourceTable.AcceptChanges();
                destTable.AcceptChanges();

                // Обновляем источник source
                source.DataSource = null;
                source.DataSource = sourceTable;
                source.DisplayMember = "title";
                source.ValueMember = "id";

                // Обновляем источник destination
                destination.DataSource = null;
                destination.DataSource = destTable;
                destination.DisplayMember = "title";
                destination.ValueMember = "id";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка перемещения: {ex.Message}");
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Удаляем все старые связи
                string deleteQuery = "DELETE FROM issue_articles WHERE issue_id = @issueId";
                NpgsqlParameter[] deleteParams = { new NpgsqlParameter("@issueId", issueId) };
                DatabaseHelper.ExecuteNonQuery(deleteQuery, deleteParams);

                // Добавляем новые связи
                DataTable selectedTable = (DataTable)lbSelected.DataSource;
                if (selectedTable != null && selectedTable.Rows.Count > 0)
                {
                    foreach (DataRow row in selectedTable.Rows)
                    {
                        string insertQuery = "INSERT INTO issue_articles (issue_id, article_id) VALUES (@issueId, @articleId)";
                        NpgsqlParameter[] insertParams = {
                            new NpgsqlParameter("@issueId", issueId),
                            new NpgsqlParameter("@articleId", row["id"])
                        };
                        DatabaseHelper.ExecuteNonQuery(insertQuery, insertParams);
                    }
                }

                MessageBox.Show("Состав выпуска сохранен", "Успешно",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }
        }
    }
}