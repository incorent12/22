using System;
using System.Windows.Forms;

namespace Gazeta
{
    public partial class FormLogin : Form
    {
        private TextBox txtEmail, txtPassword;
        private Button btnLogin, btnCancel;
        private CheckBox chkShowPassword;
        private Label lblTitle;
        private Panel headerPanel;
        private Label lblIcon;

        public FormLogin()
        {
            InitializeComponent();
            this.Text = "Вход в систему";
            this.Size = new System.Drawing.Size(420, 380);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = System.Drawing.Color.White;

            CreateControls();
        }

        private void CreateControls()
        {
            // Верхняя панель
            headerPanel = new Panel()
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = System.Drawing.Color.FromArgb(0, 120, 215)
            };

            // Иконка на панели (слева)
            lblIcon = new Label()
            {
                Text = "📰",
                Font = new System.Drawing.Font("Segoe UI", 24, System.Drawing.FontStyle.Regular),
                ForeColor = System.Drawing.Color.White,
                Left = 20,
                Top = 20,
                Width = 50,
                Height = 40,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            };
            headerPanel.Controls.Add(lblIcon);

            // Заголовок на панели (рядом с иконкой)
            lblTitle = new Label()
            {
                Text = "РЕДАКЦИЯ ГАЗЕТЫ",
                Font = new System.Drawing.Font("Segoe UI", 16, System.Drawing.FontStyle.Bold),
                ForeColor = System.Drawing.Color.White,
                Left = 80,
                Top = 22,
                Width = 280,
                Height = 35,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
            headerPanel.Controls.Add(lblTitle);
            this.Controls.Add(headerPanel);

            // Подзаголовок
            Label lblSubtitle = new Label()
            {
                Text = "Авторизация в системе",
                Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Regular),
                ForeColor = System.Drawing.Color.FromArgb(100, 100, 100),
                Left = 130,
                Top = 95,
                Width = 180,
                Height = 25,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblSubtitle);

            // Email (изменено с "Электронная почта:" на "Email:")
            Label lblEmail = new Label()
            {
                Text = "Email:",
                Left = 50,
                Top = 140,
                Width = 130,
                Height = 25,
                Font = new System.Drawing.Font("Segoe UI", 10),
                ForeColor = System.Drawing.Color.FromArgb(64, 64, 64)
            };
            txtEmail = new TextBox()
            {
                Left = 190,
                Top = 140,
                Width = 180,
                Height = 25,
                Font = new System.Drawing.Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(lblEmail);
            this.Controls.Add(txtEmail);

            // Пароль
            Label lblPassword = new Label()
            {
                Text = "Пароль:",
                Left = 50,
                Top = 180,
                Width = 130,
                Height = 25,
                Font = new System.Drawing.Font("Segoe UI", 10),
                ForeColor = System.Drawing.Color.FromArgb(64, 64, 64)
            };
            txtPassword = new TextBox()
            {
                Left = 190,
                Top = 180,
                Width = 180,
                Height = 25,
                PasswordChar = '●',
                Font = new System.Drawing.Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(lblPassword);
            this.Controls.Add(txtPassword);

            // Показать пароль
            chkShowPassword = new CheckBox()
            {
                Text = "Показать пароль",
                Left = 190,
                Top = 215,
                Width = 120,
                Height = 25,
                Font = new System.Drawing.Font("Segoe UI", 9),
                BackColor = System.Drawing.Color.White,
                ForeColor = System.Drawing.Color.FromArgb(100, 100, 100)
            };
            chkShowPassword.CheckedChanged += (s, e) =>
                txtPassword.PasswordChar = chkShowPassword.Checked ? '\0' : '●';
            this.Controls.Add(chkShowPassword);

            // Кнопка Войти
            btnLogin = new Button()
            {
                Text = "ВОЙТИ",
                Left = 90,
                Top = 260,
                Width = 110,
                Height = 40,
                BackColor = System.Drawing.Color.FromArgb(0, 120, 215),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;
            this.Controls.Add(btnLogin);

            // Кнопка Отмена
            btnCancel = new Button()
            {
                Text = "ОТМЕНА",
                Left = 220,
                Top = 260,
                Width = 110,
                Height = 40,
                BackColor = System.Drawing.Color.FromArgb(240, 240, 240),
                ForeColor = System.Drawing.Color.FromArgb(64, 64, 64),
                FlatStyle = FlatStyle.Flat,
                Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => Application.Exit();
            this.Controls.Add(btnCancel);

            // Разделитель
            Panel separator = new Panel()
            {
                Left = 40,
                Top = 250,
                Width = 340,
                Height = 1,
                BackColor = System.Drawing.Color.FromArgb(220, 220, 220)
            };
            this.Controls.Add(separator);

            // Подсказка
            Label lblHint = new Label()
            {
                Text = "Тестовый вход: admin@system.ru  |  пароль: admin",
                Left = 70,
                Top = 315,
                Width = 280,
                Height = 20,
                Font = new System.Drawing.Font("Segoe UI", 8),
                ForeColor = System.Drawing.Color.FromArgb(150, 150, 150),
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblHint);
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                string email = txtEmail.Text.Trim();
                string password = txtPassword.Text;

                if (string.IsNullOrWhiteSpace(email))
                {
                    MessageBox.Show("Введите Email", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtEmail.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show("Введите пароль", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPassword.Focus();
                    return;
                }

                btnLogin.Enabled = false;
                btnLogin.Text = "ВХОД...";
                Application.DoEvents();

                if (DatabaseHelper.Login(email, password))
                {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Неверный Email или пароль", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtPassword.Clear();
                    txtPassword.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnLogin.Enabled = true;
                btnLogin.Text = "ВОЙТИ";
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter)
            {
                BtnLogin_Click(this, EventArgs.Empty);
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}