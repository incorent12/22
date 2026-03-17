using System;
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
            Label lblAvailable = new Label() { Text = "Доступные статьи:", Left = 20, Top = 20, Width = 200 };
            lbAvailable = new ListBox() { Left = 20, Top = 40, Width = 200, Height = 250, SelectionMode = SelectionMode.MultiExtended };

            Label lblSelected = new Label() { Text = "Статьи в выпуске:", Left = 280, Top = 20, Width = 200 };
            lbSelected = new ListBox() { Left = 280, Top = 40, Width = 200, Height = 250, SelectionMode = SelectionMode.MultiExtended };

            btnAdd = new Button() { Text = ">", Left = 230, Top = 100, Width = 40, Height = 30 };
            btnAdd.Click += (s, e) => MoveItems(lbAvailable, lbSelected);

            btnRemove = new Button() { Text = "<", Left = 230, Top = 140, Width = 40, Height = 30 };
            btnRemove.Click += (s, e) => MoveItems(lbSelected, lbAvailable);

            btnSave = new Button() { Text = "Сохранить", Left = 150, Top = 310, Width = 100, Height = 30 };
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button() { Text = "Закрыть", Left = 260, Top = 310, Width = 100, Height = 30 };
            btnCancel.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] {
                lblAvailable, lbAvailable,
                lblSelected, lbSelected,
                btnAdd, btnRemove, btnSave, btnCancel
            });
        }

        private void LoadData()
        {
            // Загружаем все статьи
            string allQuery = @"SELECT a.id, a.title, e.full_name 
                               FROM articles a
                               LEFT JOIN employees e ON a.author_id = e.id
                               WHERE a.status = 'Опубликовано'
                               ORDER BY a.created_at DESC";
            DataTable allArticles = DatabaseHelper.GetDataTable(allQuery);

            lbAvailable.DisplayMember = "title";
            lbAvailable.ValueMember = "id";
            lbAvailable.DataSource = allArticles;

            // Загружаем статьи в выпуске
            string selectedQuery = @"SELECT a.id, a.title, e.full_name 
                                    FROM articles a
                                    JOIN issue_articles ia ON a.id = ia.article_id
                                    LEFT JOIN employees e ON a.author_id = e.id
                                    WHERE ia.issue_id = @issueId
                                    ORDER BY a.title";
            NpgsqlParameter[] parameters = { new NpgsqlParameter("@issueId", issueId) };
            DataTable selectedArticles = DatabaseHelper.GetDataTable(selectedQuery, parameters);

            lbSelected.DisplayMember = "title";
            lbSelected.ValueMember = "id";
            lbSelected.DataSource = selectedArticles;
        }

        private void MoveItems(ListBox source, ListBox destination)
        {
            if (source.SelectedItems.Count == 0) return;

            // Получаем выделенные элементы
            List<DataRowView> selectedRows = new List<DataRowView>();
            foreach (DataRowView item in source.SelectedItems)
            {
                selectedRows.Add(item);
            }

            DataTable sourceTable = (DataTable)source.DataSource;
            DataTable destTable;

            if (destination.DataSource == null)
            {
                destTable = sourceTable.Clone(); // Создаем копию структуры
            }
            else
            {
                destTable = (DataTable)destination.DataSource;
            }

            // Копируем выделенные элементы в destination
            foreach (DataRowView rowView in selectedRows)
            {
                // Добавляем в destination
                DataRow newRow = destTable.NewRow();
                newRow["id"] = rowView["id"];
                newRow["title"] = rowView["title"];
                newRow["full_name"] = rowView["full_name"];
                destTable.Rows.Add(newRow);

                // Удаляем из source (ищем по id вручную)
                for (int i = sourceTable.Rows.Count - 1; i >= 0; i--)
                {
                    if (sourceTable.Rows[i]["id"].ToString() == rowView["id"].ToString())
                    {
                        sourceTable.Rows.RemoveAt(i);
                        break;
                    }
                }
            }

            // Применяем изменения
            sourceTable.AcceptChanges();
            destTable.AcceptChanges();

            // Обновляем источник данных для source
            source.DataSource = null;
            source.DataSource = sourceTable;
            source.DisplayMember = "title";
            source.ValueMember = "id";

            // Обновляем источник данных для destination
            destination.DataSource = null;
            destination.DataSource = destTable;
            destination.DisplayMember = "title";
            destination.ValueMember = "id";
        }

        private void BtnSave_Click(object sender, EventArgs e)
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
    }
}
