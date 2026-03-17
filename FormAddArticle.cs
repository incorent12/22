using System;
using System.Data;
using System.Windows.Forms;
using Npgsql;

namespace Gazeta
{
    public partial class FormAddArticle : Form
    {
        private int? articleId = null;
        private TextBox txtTitle;
        private ComboBox cmbAuthor, cmbRubric, cmbStatus;
        private RichTextBox rtbContent;
        private DateTimePicker dtpCreated;
        private Button btnSave, btnCancel;

        public FormAddArticle()
        {
            InitializeComponent();
            this.Text = "Добавление статьи";
            this.Size = new System.Drawing.Size(500, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            CreateControls();
            LoadComboBoxes();
        }

        public FormAddArticle(int id)
        {
            InitializeComponent();
            this.Text = "Редактирование статьи";
            this.articleId = id;
            this.Size = new System.Drawing.Size(500, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            CreateControls();
            LoadComboBoxes();
            LoadArticleData();
        }

        private void CreateControls()
        {
            // Заголовок
            Label lblTitle = new Label() { Text = "Заголовок:*", Left = 20, Top = 20, Width = 100 };
            txtTitle = new TextBox() { Left = 130, Top = 20, Width = 330 };

            // Автор
            Label lblAuthor = new Label() { Text = "Автор:*", Left = 20, Top = 50, Width = 100 };
            cmbAuthor = new ComboBox() { Left = 130, Top = 50, Width = 330, DropDownStyle = ComboBoxStyle.DropDownList };

            // Рубрика
            Label lblRubric = new Label() { Text = "Рубрика:", Left = 20, Top = 80, Width = 100 };
            cmbRubric = new ComboBox() { Left = 130, Top = 80, Width = 330, DropDownStyle = ComboBoxStyle.DropDownList };

            // Статус
            Label lblStatus = new Label() { Text = "Статус:", Left = 20, Top = 110, Width = 100 };
            cmbStatus = new ComboBox() { Left = 130, Top = 110, Width = 330, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatus.Items.AddRange(new[] { "Черновик", "Опубликовано", "На редактировании" });
            cmbStatus.SelectedIndex = 0;

            // Дата создания
            Label lblCreated = new Label() { Text = "Дата создания:", Left = 20, Top = 140, Width = 100 };
            dtpCreated = new DateTimePicker() { Left = 130, Top = 140, Width = 330 };

            // Текст статьи
            Label lblContent = new Label() { Text = "Текст статьи:", Left = 20, Top = 170, Width = 100 };
            rtbContent = new RichTextBox() { Left = 130, Top = 170, Width = 330, Height = 200 };

            // Кнопки
            btnSave = new Button() { Text = "Сохранить", Left = 130, Top = 390, Width = 100, Height = 30 };
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button() { Text = "Отмена", Left = 250, Top = 390, Width = 100, Height = 30 };
            btnCancel.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] {
                lblTitle, txtTitle,
                lblAuthor, cmbAuthor,
                lblRubric, cmbRubric,
                lblStatus, cmbStatus,
                lblCreated, dtpCreated,
                lblContent, rtbContent,
                btnSave, btnCancel
            });
        }

        private void LoadComboBoxes()
        {
            // Авторы
            string authorQuery = "SELECT id, full_name FROM employees ORDER BY full_name";
            DataTable authors = DatabaseHelper.GetDataTable(authorQuery);
            cmbAuthor.DataSource = authors;
            cmbAuthor.DisplayMember = "full_name";
            cmbAuthor.ValueMember = "id";
            cmbAuthor.SelectedIndex = -1;

            // Рубрики
            string rubricQuery = "SELECT id, title FROM rubrics ORDER BY title";
            DataTable rubrics = DatabaseHelper.GetDataTable(rubricQuery);
            cmbRubric.DataSource = rubrics;
            cmbRubric.DisplayMember = "title";
            cmbRubric.ValueMember = "id";
            cmbRubric.SelectedIndex = -1;
        }

        private void LoadArticleData()
        {
            try
            {
                string query = "SELECT title, author_id, rubric_id, content, created_at, status FROM articles WHERE id = @id";
                NpgsqlParameter[] parameters = { new NpgsqlParameter("@id", articleId) };

                DataTable dt = DatabaseHelper.GetDataTable(query, parameters);
                if (dt.Rows.Count == 0) return;

                DataRow row = dt.Rows[0];

                // Заголовок
                txtTitle.Text = row["title"].ToString();

                // Автор
                if (row["author_id"] != DBNull.Value)
                    cmbAuthor.SelectedValue = Convert.ToInt32(row["author_id"]);

                // Рубрика
                if (row["rubric_id"] != DBNull.Value)
                    cmbRubric.SelectedValue = Convert.ToInt32(row["rubric_id"]);

                // Текст
                rtbContent.Text = row["content"].ToString();

                // Дата создания - ПРОСТОЕ РЕШЕНИЕ
                if (row["created_at"] != DBNull.Value)
                {
                    // Преобразуем в строку, потом в дату - работает всегда
                    string dateStr = row["created_at"].ToString();
                    dtpCreated.Value = DateTime.Parse(dateStr);
                }

                // Статус
                cmbStatus.Text = row["status"].ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtTitle.Text))
                {
                    MessageBox.Show("Введите заголовок");
                    return;
                }

                if (cmbAuthor.SelectedValue == null)
                {
                    MessageBox.Show("Выберите автора");
                    return;
                }

                string query;
                NpgsqlParameter[] parameters;

                if (articleId == null) // Добавление
                {
                    query = @"INSERT INTO articles (title, author_id, rubric_id, content, created_at, status) 
                              VALUES (@title, @author, @rubric, @content, @date, @status)";

                    parameters = new NpgsqlParameter[]
                    {
                        new NpgsqlParameter("@title", txtTitle.Text),
                        new NpgsqlParameter("@author", cmbAuthor.SelectedValue),
                        new NpgsqlParameter("@rubric", cmbRubric.SelectedValue ?? DBNull.Value),
                        new NpgsqlParameter("@content", string.IsNullOrWhiteSpace(rtbContent.Text) ? DBNull.Value : rtbContent.Text),
                        new NpgsqlParameter("@date", dtpCreated.Value),
                        new NpgsqlParameter("@status", cmbStatus.Text)
                    };
                }
                else // Редактирование
                {
                    query = @"UPDATE articles SET title = @title, author_id = @author,
                              rubric_id = @rubric, content = @content, created_at = @date, status = @status
                              WHERE id = @id";

                    parameters = new NpgsqlParameter[]
                    {
                        new NpgsqlParameter("@title", txtTitle.Text),
                        new NpgsqlParameter("@author", cmbAuthor.SelectedValue),
                        new NpgsqlParameter("@rubric", cmbRubric.SelectedValue ?? DBNull.Value),
                        new NpgsqlParameter("@content", string.IsNullOrWhiteSpace(rtbContent.Text) ? DBNull.Value : rtbContent.Text),
                        new NpgsqlParameter("@date", dtpCreated.Value),
                        new NpgsqlParameter("@status", cmbStatus.Text),
                        new NpgsqlParameter("@id", articleId)
                    };
                }

                if (DatabaseHelper.ExecuteNonQuery(query, parameters))
                {
                    MessageBox.Show(articleId == null ? "Статья добавлена" : "Сохранено");
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }
    }
}