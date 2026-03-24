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
        private TextBox txtLastName, txtFirstName, txtMiddleName, txtPhone, txtEmail;
        private DateTimePicker dtpBirthDate, dtpHireDate;
        private Button btnSave, btnCancel;

        public FormAddAuthor()
        {
            InitializeComponent();
            this.Text = "Добавление сотрудника";
            this.Size = new System.Drawing.Size(450, 450);
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
            this.Text = "Редактирование сотрудника";
            this.authorId = id;
            this.Size = new System.Drawing.Size(450, 450);
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
            int y = 20;
            int labelWidth = 100;
            int fieldWidth = 280;
            int left = 130;

            Label lblLastName = new Label() { Text = "Фамилия:*", Left = 20, Top = y, Width = labelWidth };
            txtLastName = new TextBox() { Left = left, Top = y, Width = fieldWidth };
            y += 35;

            Label lblFirstName = new Label() { Text = "Имя:*", Left = 20, Top = y, Width = labelWidth };
            txtFirstName = new TextBox() { Left = left, Top = y, Width = fieldWidth };
            y += 35;

            Label lblMiddleName = new Label() { Text = "Отчество:", Left = 20, Top = y, Width = labelWidth };
            txtMiddleName = new TextBox() { Left = left, Top = y, Width = fieldWidth };
            y += 35;

            Label lblBirthDate = new Label() { Text = "Дата рождения:", Left = 20, Top = y, Width = labelWidth };
            dtpBirthDate = new DateTimePicker() { Left = left, Top = y, Width = fieldWidth };
            y += 35;

            Label lblPhone = new Label() { Text = "Телефон:", Left = 20, Top = y, Width = labelWidth };
            txtPhone = new TextBox() { Left = left, Top = y, Width = fieldWidth };
            y += 35;

            Label lblEmail = new Label() { Text = "Email:", Left = 20, Top = y, Width = labelWidth };
            txtEmail = new TextBox() { Left = left, Top = y, Width = fieldWidth };
            y += 35;

            Label lblPosition = new Label() { Text = "Должность:", Left = 20, Top = y, Width = labelWidth };
            cmbPosition = new ComboBox() { Left = left, Top = y, Width = fieldWidth, DropDownStyle = ComboBoxStyle.DropDownList };
            y += 35;

            Label lblHireDate = new Label() { Text = "Дата найма:", Left = 20, Top = y, Width = labelWidth };
            dtpHireDate = new DateTimePicker() { Left = left, Top = y, Width = fieldWidth };
            y += 45;

            btnSave = new Button() { Text = "Сохранить", Left = 130, Top = y, Width = 100, Height = 30 };
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button() { Text = "Отмена", Left = 250, Top = y, Width = 100, Height = 30 };
            btnCancel.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] {
                lblLastName, txtLastName,
                lblFirstName, txtFirstName,
                lblMiddleName, txtMiddleName,
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
            string query = "SELECT id, title FROM positions ORDER BY title";
            DataTable dt = DatabaseHelper.GetDataTable(query);
            cmbPosition.DataSource = dt;
            cmbPosition.DisplayMember = "title";
            cmbPosition.ValueMember = "id";
            cmbPosition.SelectedIndex = -1;
        }

        private void LoadAuthorData()
        {
            try
            {
                string query = @"SELECT last_name, first_name, middle_name, birth_date, 
                                        phone, email, hire_date, position_id 
                                 FROM employees WHERE id = @id";
                NpgsqlParameter[] parameters = { new NpgsqlParameter("@id", authorId) };

                DataTable dt = DatabaseHelper.GetDataTable(query, parameters);
                if (dt.Rows.Count == 0) return;

                DataRow row = dt.Rows[0];
                txtLastName.Text = row["last_name"].ToString();
                txtFirstName.Text = row["first_name"].ToString();
                txtMiddleName.Text = row["middle_name"].ToString();
                txtPhone.Text = row["phone"].ToString();
                txtEmail.Text = row["email"].ToString();

                if (row["birth_date"] != DBNull.Value)
                    dtpBirthDate.Value = DateTime.Parse(row["birth_date"].ToString());

                if (row["hire_date"] != DBNull.Value)
                    dtpHireDate.Value = DateTime.Parse(row["hire_date"].ToString());

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
                if (string.IsNullOrWhiteSpace(txtLastName.Text) || string.IsNullOrWhiteSpace(txtFirstName.Text))
                {
                    MessageBox.Show("Введите фамилию и имя");
                    return;
                }

                string query;
                NpgsqlParameter[] parameters;

                if (authorId == null)
                {
                    query = @"INSERT INTO employees (last_name, first_name, middle_name, birth_date, 
                              phone, email, hire_date, position_id) 
                              VALUES (@ln, @fn, @mn, @bd, @ph, @em, @hd, @pid)";

                    parameters = new NpgsqlParameter[]
                    {
                        new NpgsqlParameter("@ln", txtLastName.Text),
                        new NpgsqlParameter("@fn", txtFirstName.Text),
                        new NpgsqlParameter("@mn", string.IsNullOrWhiteSpace(txtMiddleName.Text) ? DBNull.Value : txtMiddleName.Text),
                        new NpgsqlParameter("@bd", dtpBirthDate.Checked ? dtpBirthDate.Value : DBNull.Value),
                        new NpgsqlParameter("@ph", string.IsNullOrWhiteSpace(txtPhone.Text) ? DBNull.Value : txtPhone.Text),
                        new NpgsqlParameter("@em", string.IsNullOrWhiteSpace(txtEmail.Text) ? DBNull.Value : txtEmail.Text),
                        new NpgsqlParameter("@hd", dtpHireDate.Checked ? dtpHireDate.Value : DBNull.Value),
                        new NpgsqlParameter("@pid", cmbPosition.SelectedValue ?? DBNull.Value)
                    };
                }
                else
                {
                    query = @"UPDATE employees SET last_name = @ln, first_name = @fn, middle_name = @mn,
                              birth_date = @bd, phone = @ph, email = @em, 
                              hire_date = @hd, position_id = @pid
                              WHERE id = @id";

                    parameters = new NpgsqlParameter[]
                    {
                        new NpgsqlParameter("@ln", txtLastName.Text),
                        new NpgsqlParameter("@fn", txtFirstName.Text),
                        new NpgsqlParameter("@mn", string.IsNullOrWhiteSpace(txtMiddleName.Text) ? DBNull.Value : txtMiddleName.Text),
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
                    MessageBox.Show(authorId == null ? "Сотрудник добавлен" : "Изменения сохранены");
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