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
        private Label lblCharCount;
        private Button btnSave, btnCancel;
        private int maxContentLength = 5000;

        public FormAddArticle()
        {
            InitializeComponent();
            this.Text = "Добавление статьи";
            this.Size = new System.Drawing.Size(550, 550);
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
            this.Size = new System.Drawing.Size(550, 550);
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
            int y = 20;
            int labelWidth = 100;
            int fieldWidth = 380;
            int left = 130;

            Label lblTitle = new Label() { Text = "Заголовок:*", Left = 20, Top = y, Width = labelWidth };
            txtTitle = new TextBox() { Left = left, Top = y, Width = fieldWidth };
            y += 35;

            Label lblAuthor = new Label() { Text = "Автор:*", Left = 20, Top = y, Width = labelWidth };
            cmbAuthor = new ComboBox() { Left = left, Top = y, Width = fieldWidth, DropDownStyle = ComboBoxStyle.DropDownList };
            y += 35;

            Label lblRubric = new Label() { Text = "Рубрика:", Left = 20, Top = y, Width = labelWidth };
            cmbRubric = new ComboBox() { Left = left, Top = y, Width = fieldWidth, DropDownStyle = ComboBoxStyle.DropDownList };
            y += 35;

            Label lblStatus = new Label() { Text = "Статус:", Left = 20, Top = y, Width = labelWidth };
            cmbStatus = new ComboBox() { Left = left, Top = y, Width = fieldWidth, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatus.Items.AddRange(new[] { "Черновик", "Опубликовано", "На редактировании" });
            cmbStatus.SelectedIndex = 0;
            y += 35;

            Label lblCreated = new Label() { Text = "Дата создания:", Left = 20, Top = y, Width = labelWidth };
            dtpCreated = new DateTimePicker() { Left = left, Top = y, Width = fieldWidth };
            y += 35;

            Label lblContent = new Label() { Text = "Текст статьи:", Left = 20, Top = y, Width = labelWidth };
            rtbContent = new RichTextBox() { Left = left, Top = y, Width = fieldWidth, Height = 200 };
            rtbContent.TextChanged += RtbContent_TextChanged;
            y += 210;

            lblCharCount = new Label() { Text = $"Символов: 0 / {maxContentLength}", Left = left, Top = y, Width = fieldWidth, ForeColor = System.Drawing.Color.Gray };
            y += 30;

            btnSave = new Button() { Text = "Сохранить", Left = 130, Top = y, Width = 100, Height = 30 };
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button() { Text = "Отмена", Left = 250, Top = y, Width = 100, Height = 30 };
            btnCancel.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] {
                lblTitle, txtTitle,
                lblAuthor, cmbAuthor,
                lblRubric, cmbRubric,
                lblStatus, cmbStatus,
                lblCreated, dtpCreated,
                lblContent, rtbContent,
                lblCharCount,
                btnSave, btnCancel
            });
        }

        private void RtbContent_TextChanged(object sender, EventArgs e)
        {
            int length = rtbContent.Text.Length;
            lblCharCount.Text = $"Символов: {length} / {maxContentLength}";
            if (length > maxContentLength)
            {
                lblCharCount.ForeColor = System.Drawing.Color.Red;
                btnSave.Enabled = false;
            }
            else
            {
                lblCharCount.ForeColor = System.Drawing.Color.Gray;
                btnSave.Enabled = true;
            }
        }

        private void LoadComboBoxes()
        {
            try
            {
                // Загрузка авторов (исключаем администратора)
                string authorQuery = @"SELECT id, 
                                              last_name || ' ' || first_name || ' ' || COALESCE(middle_name, '') AS full_name 
                                       FROM employees 
                                       WHERE email != 'admin@system.ru'
                                       ORDER BY last_name";
                DataTable authors = DatabaseHelper.GetDataTable(authorQuery);
                cmbAuthor.DataSource = authors;
                cmbAuthor.DisplayMember = "full_name";
                cmbAuthor.ValueMember = "id";
                cmbAuthor.SelectedIndex = -1;

                string rubricQuery = "SELECT id, title FROM rubrics ORDER BY title";
                DataTable rubrics = DatabaseHelper.GetDataTable(rubricQuery);
                cmbRubric.DataSource = rubrics;
                cmbRubric.DisplayMember = "title";
                cmbRubric.ValueMember = "id";
                cmbRubric.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки списков: {ex.Message}");
            }
        }

        private void LoadArticleData()
        {
            try
            {
                string query = @"SELECT title, author_id, rubric_id, content, created_at, status 
                                 FROM articles WHERE id = @id";
                NpgsqlParameter[] parameters = { new NpgsqlParameter("@id", articleId) };

                DataTable dt = DatabaseHelper.GetDataTable(query, parameters);
                if (dt.Rows.Count == 0) return;

                DataRow row = dt.Rows[0];

                txtTitle.Text = row["title"].ToString();

                if (row["author_id"] != DBNull.Value)
                    cmbAuthor.SelectedValue = Convert.ToInt32(row["author_id"]);

                if (row["rubric_id"] != DBNull.Value)
                    cmbRubric.SelectedValue = Convert.ToInt32(row["rubric_id"]);

                rtbContent.Text = row["content"].ToString();

                if (row["created_at"] != DBNull.Value)
                {
                    string dateStr = row["created_at"].ToString();
                    dtpCreated.Value = DateTime.Parse(dateStr);
                }

                cmbStatus.Text = row["status"].ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных статьи: {ex.Message}");
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtTitle.Text))
                {
                    MessageBox.Show("Введите заголовок статьи", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtTitle.Focus();
                    return;
                }

                if (cmbAuthor.SelectedValue == null)
                {
                    MessageBox.Show("Выберите автора", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    cmbAuthor.Focus();
                    return;
                }

                if (rtbContent.Text.Length > maxContentLength)
                {
                    MessageBox.Show($"Текст статьи не должен превышать {maxContentLength} символов",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string query;
                NpgsqlParameter[] parameters;

                if (articleId == null)
                {
                    query = @"INSERT INTO articles (title, author_id, rubric_id, content, created_at, status) 
                              VALUES (@title, @author, @rubric, @content, @date, @status)";

                    parameters = new NpgsqlParameter[]
                    {
                        new NpgsqlParameter("@title", txtTitle.Text.Trim()),
                        new NpgsqlParameter("@author", cmbAuthor.SelectedValue),
                        new NpgsqlParameter("@rubric", cmbRubric.SelectedValue ?? DBNull.Value),
                        new NpgsqlParameter("@content", string.IsNullOrWhiteSpace(rtbContent.Text) ? DBNull.Value : (object)rtbContent.Text),
                        new NpgsqlParameter("@date", dtpCreated.Value),
                        new NpgsqlParameter("@status", cmbStatus.Text)
                    };
                }
                else
                {
                    query = @"UPDATE articles SET 
                              title = @title, 
                              author_id = @author,
                              rubric_id = @rubric, 
                              content = @content, 
                              created_at = @date, 
                              status = @status,
                              reviewed_at = NOW()
                              WHERE id = @id";

                    parameters = new NpgsqlParameter[]
                    {
                        new NpgsqlParameter("@title", txtTitle.Text.Trim()),
                        new NpgsqlParameter("@author", cmbAuthor.SelectedValue),
                        new NpgsqlParameter("@rubric", cmbRubric.SelectedValue ?? DBNull.Value),
                        new NpgsqlParameter("@content", string.IsNullOrWhiteSpace(rtbContent.Text) ? DBNull.Value : (object)rtbContent.Text),
                        new NpgsqlParameter("@date", dtpCreated.Value),
                        new NpgsqlParameter("@status", cmbStatus.Text),
                        new NpgsqlParameter("@id", articleId)
                    };
                }

                if (DatabaseHelper.ExecuteNonQuery(query, parameters))
                {
                    MessageBox.Show(articleId == null ? "Статья успешно добавлена" : "Изменения сохранены",
                        "Успешно", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения статьи: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}