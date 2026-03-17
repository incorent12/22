using System;
using System.Data;
using System.Windows.Forms;
using Npgsql;

namespace Gazeta
{
    public partial class FormAddAuthor : Form
    {
        private int? authorId = null;
        private ComboBox cmbPosition;
        private TextBox txtFullName, txtPhone, txtEmail;
        private DateTimePicker dtpBirthDate, dtpHireDate;
        private Button btnSave, btnCancel;

        public FormAddAuthor()
        {
            InitializeComponent();
            this.Text = "Добавление автора";
            this.Size = new System.Drawing.Size(400, 350);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            CreateControls();
            LoadPositions();
        }

        public FormAddAuthor(int id)
        {
            InitializeComponent();
            this.Text = "Редактирование автора";
            this.authorId = id;
            this.Size = new System.Drawing.Size(400, 350);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            CreateControls();
            LoadPositions();
            LoadAuthorData();
        }

        private void CreateControls()
        {
            // ФИО
            Label lblFullName = new Label() { Text = "ФИО:*", Left = 20, Top = 20, Width = 100 };
            txtFullName = new TextBox() { Left = 130, Top = 20, Width = 220 };

            // Дата рождения
            Label lblBirthDate = new Label() { Text = "Дата рождения:", Left = 20, Top = 50, Width = 100 };
            dtpBirthDate = new DateTimePicker() { Left = 130, Top = 50, Width = 220 };

            // Телефон
            Label lblPhone = new Label() { Text = "Телефон:", Left = 20, Top = 80, Width = 100 };
            txtPhone = new TextBox() { Left = 130, Top = 80, Width = 220 };

            // Email
            Label lblEmail = new Label() { Text = "Email:", Left = 20, Top = 110, Width = 100 };
            txtEmail = new TextBox() { Left = 130, Top = 110, Width = 220 };

            // Должность
            Label lblPosition = new Label() { Text = "Должность:", Left = 20, Top = 140, Width = 100 };
            cmbPosition = new ComboBox() { Left = 130, Top = 140, Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };

            // Дата найма
            Label lblHireDate = new Label() { Text = "Дата найма:", Left = 20, Top = 170, Width = 100 };
            dtpHireDate = new DateTimePicker() { Left = 130, Top = 170, Width = 220 };

            // Кнопки
            btnSave = new Button() { Text = "Сохранить", Left = 130, Top = 220, Width = 100, Height = 30 };
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button() { Text = "Отмена", Left = 250, Top = 220, Width = 100, Height = 30 };
            btnCancel.Click += (s, e) => this.Close();

            // Добавляем все на форму
            this.Controls.AddRange(new Control[] {
                lblFullName, txtFullName,
                lblBirthDate, dtpBirthDate,
                lblPhone, txtPhone,
                lblEmail, txtEmail,
                lblPosition, cmbPosition,
                lblHireDate, dtpHireDate,
                btnSave, btnCancel
            });
        }

        private void LoadPositions()
        {
            try
            {
                string query = "SELECT id, title FROM positions ORDER BY title";
                DataTable dt = DatabaseHelper.GetDataTable(query);
                cmbPosition.DataSource = dt;
                cmbPosition.DisplayMember = "title";
                cmbPosition.ValueMember = "id";
                cmbPosition.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки должностей: {ex.Message}");
            }
        }

        private void LoadAuthorData()
        {
            try
            {
                string query = "SELECT full_name, birth_date, phone, email, hire_date, position_id FROM employees WHERE id = @id";
                NpgsqlParameter[] parameters = { new NpgsqlParameter("@id", authorId) };

                DataTable dt = DatabaseHelper.GetDataTable(query, parameters);
                if (dt.Rows.Count == 0) return;

                DataRow row = dt.Rows[0];
                txtFullName.Text = row["full_name"].ToString();
                txtPhone.Text = row["phone"].ToString();
                txtEmail.Text = row["email"].ToString();

                // Дата рождения (простое решение)
                if (row["birth_date"] != DBNull.Value)
                    dtpBirthDate.Value = DateTime.Parse(row["birth_date"].ToString());

                // Дата найма (простое решение)
                if (row["hire_date"] != DBNull.Value)
                    dtpHireDate.Value = DateTime.Parse(row["hire_date"].ToString());

                // Должность
                if (row["position_id"] != DBNull.Value)
                    cmbPosition.SelectedValue = Convert.ToInt32(row["position_id"]);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtFullName.Text))
                {
                    MessageBox.Show("Введите ФИО");
                    return;
                }

                string query;
                NpgsqlParameter[] parameters;

                if (authorId == null) // Добавление
                {
                    query = @"INSERT INTO employees (full_name, birth_date, phone, email, hire_date, position_id) 
                              VALUES (@fn, @bd, @ph, @em, @hd, @pid)";

                    parameters = new NpgsqlParameter[]
                    {
                        new NpgsqlParameter("@fn", txtFullName.Text),
                        new NpgsqlParameter("@bd", dtpBirthDate.Checked ? dtpBirthDate.Value : DBNull.Value),
                        new NpgsqlParameter("@ph", string.IsNullOrWhiteSpace(txtPhone.Text) ? DBNull.Value : txtPhone.Text),
                        new NpgsqlParameter("@em", string.IsNullOrWhiteSpace(txtEmail.Text) ? DBNull.Value : txtEmail.Text),
                        new NpgsqlParameter("@hd", dtpHireDate.Checked ? dtpHireDate.Value : DBNull.Value),
                        new NpgsqlParameter("@pid", cmbPosition.SelectedValue ?? DBNull.Value)
                    };
                }
                else // Редактирование
                {
                    query = @"UPDATE employees SET full_name = @fn, birth_date = @bd,
                              phone = @ph, email = @em, hire_date = @hd, position_id = @pid
                              WHERE id = @id";

                    parameters = new NpgsqlParameter[]
                    {
                        new NpgsqlParameter("@fn", txtFullName.Text),
                        new NpgsqlParameter("@bd", dtpBirthDate.Checked ? dtpBirthDate.Value : DBNull.Value),
                        new NpgsqlParameter("@ph", string.IsNullOrWhiteSpace(txtPhone.Text) ? DBNull.Value : txtPhone.Text),
                        new NpgsqlParameter("@em", string.IsNullOrWhiteSpace(txtEmail.Text) ? DBNull.Value : txtEmail.Text),
                        new NpgsqlParameter("@hd", dtpHireDate.Checked ? dtpHireDate.Value : DBNull.Value),
                        new NpgsqlParameter("@pid", cmbPosition.SelectedValue ?? DBNull.Value),
                        new NpgsqlParameter("@id", authorId)
                    };
                }

                if (DatabaseHelper.ExecuteNonQuery(query, parameters))
                {
                    MessageBox.Show(authorId == null ? "Добавлено" : "Сохранено");
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