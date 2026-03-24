using System;
using System.Data;
using System.Windows.Forms;
using Npgsql;

namespace Gazeta
{
    public partial class FormAddIssue : Form
    {
        private int? issueId = null;
        private NumericUpDown nudIssueNumber, nudPrintRun;
        private DateTimePicker dtpReleaseDate;
        private Button btnSave, btnCancel;

        public FormAddIssue()
        {
            InitializeComponent();
            this.Text = "Создание выпуска";
            this.Size = new System.Drawing.Size(350, 250);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            CreateControls();
        }

        public FormAddIssue(int id)
        {
            InitializeComponent();
            this.Text = "Редактирование выпуска";
            this.issueId = id;
            this.Size = new System.Drawing.Size(350, 250);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            CreateControls();
            LoadIssueData();
        }

        private void CreateControls()
        {
            Label lblIssueNumber = new Label() { Text = "Номер выпуска:*", Left = 20, Top = 20, Width = 120 };
            nudIssueNumber = new NumericUpDown() { Left = 150, Top = 20, Width = 150, Minimum = 1, Maximum = 9999 };

            Label lblReleaseDate = new Label() { Text = "Дата выхода:*", Left = 20, Top = 50, Width = 120 };
            dtpReleaseDate = new DateTimePicker() { Left = 150, Top = 50, Width = 150, Format = DateTimePickerFormat.Short };

            Label lblPrintRun = new Label() { Text = "Тираж:", Left = 20, Top = 80, Width = 120 };
            nudPrintRun = new NumericUpDown() { Left = 150, Top = 80, Width = 150, Minimum = 1, Maximum = 100000, Value = 1000 };

            btnSave = new Button() { Text = "Сохранить", Left = 80, Top = 130, Width = 100, Height = 30 };
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button() { Text = "Отмена", Left = 190, Top = 130, Width = 100, Height = 30 };
            btnCancel.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] {
                lblIssueNumber, nudIssueNumber,
                lblReleaseDate, dtpReleaseDate,
                lblPrintRun, nudPrintRun,
                btnSave, btnCancel
            });
        }

        private void LoadIssueData()
        {
            string query = "SELECT issue_number, release_date, print_run FROM issues WHERE id = @id";
            NpgsqlParameter[] parameters = { new NpgsqlParameter("@id", issueId.Value) };

            DataTable dt = DatabaseHelper.GetDataTable(query, parameters);
            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                nudIssueNumber.Value = Convert.ToInt32(row["issue_number"]);

                if (row["release_date"] != DBNull.Value)
                    dtpReleaseDate.Value = Convert.ToDateTime(row["release_date"]);

                if (row["print_run"] != DBNull.Value)
                    nudPrintRun.Value = Convert.ToInt32(row["print_run"]);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (nudIssueNumber.Value == 0)
            {
                MessageBox.Show("Введите номер выпуска", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                nudIssueNumber.Focus();
                return;
            }

            string query;
            if (issueId == null)
            {
                query = @"INSERT INTO issues (issue_number, release_date, print_run) 
                          VALUES (@issue_number, @release_date, @print_run)";
            }
            else
            {
                query = @"UPDATE issues SET issue_number = @issue_number, 
                          release_date = @release_date, print_run = @print_run
                          WHERE id = @id";
            }

            NpgsqlParameter[] parameters = {
                new NpgsqlParameter("@issue_number", (int)nudIssueNumber.Value),
                new NpgsqlParameter("@release_date", dtpReleaseDate.Value),
                new NpgsqlParameter("@print_run", (int)nudPrintRun.Value)
            };

            if (issueId != null)
            {
                Array.Resize(ref parameters, parameters.Length + 1);
                parameters[parameters.Length - 1] = new NpgsqlParameter("@id", issueId.Value);
            }

            if (DatabaseHelper.ExecuteNonQuery(query, parameters))
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}