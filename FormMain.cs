using System;
using System.Data;
using System.Windows.Forms;
using Npgsql;
using System.Collections.Generic;

namespace Gazeta
{
    public partial class FormMain : Form
    {
        private string currentTable = "employees";
        private TabControl tabControl;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel statusLabel;
        private ToolStripStatusLabel userLabel;
        private ToolStripStatusLabel roleLabel;

        public FormMain()
        {
            InitializeComponent();
            this.Text = "Редакция газеты - Управление базой данных";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = System.Drawing.Color.White;
            this.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            CreateMainFormControls();
            LoadData("employees");
            UpdateStatusBar();
        }

        private void UpdateStatusBar()
        {
            if (userLabel != null)
            {
                userLabel.Text = $"👤 Пользователь: {DatabaseHelper.CurrentUser.FullName}";
                roleLabel.Text = $"🔑 Роль: {DatabaseHelper.CurrentUser.Role}";
            }
        }

        private void CreateMainFormControls()
        {
            MenuStrip menuStrip = new MenuStrip();
            menuStrip.BackColor = System.Drawing.Color.FromArgb(0, 120, 215);
            menuStrip.ForeColor = System.Drawing.Color.White;
            menuStrip.Padding = new Padding(5, 2, 0, 2);

            ToolStripMenuItem fileMenu = new ToolStripMenuItem("📁 Файл");
            fileMenu.ForeColor = System.Drawing.Color.White;
            ToolStripMenuItem logoutItem = new ToolStripMenuItem("🔄 Сменить пользователя");
            logoutItem.Click += (s, arg) => Application.Restart();
            fileMenu.DropDownItems.Add(logoutItem);

            ToolStripMenuItem exitItem = new ToolStripMenuItem("❌ Выход");
            exitItem.Click += (s, arg) => Application.Exit();
            fileMenu.DropDownItems.Add(exitItem);

            ToolStripMenuItem reportsMenu = new ToolStripMenuItem("📊 Отчеты");
            reportsMenu.ForeColor = System.Drawing.Color.White;
            ToolStripMenuItem exportArticles = new ToolStripMenuItem("📎 Экспорт статей в Excel");
            exportArticles.Click += ExportArticlesToExcel;
            reportsMenu.DropDownItems.Add(exportArticles);

            ToolStripMenuItem helpMenu = new ToolStripMenuItem("❓ Справка");
            helpMenu.ForeColor = System.Drawing.Color.White;
            ToolStripMenuItem aboutItem = new ToolStripMenuItem("ℹ️ О программе");
            aboutItem.Click += (s, arg) => MessageBox.Show($"Редакция газеты\nВерсия 2.0\nПользователь: {DatabaseHelper.CurrentUser.FullName}\nРоль: {DatabaseHelper.CurrentUser.Role}", "О программе");
            helpMenu.DropDownItems.Add(aboutItem);

            menuStrip.Items.Add(fileMenu);
            menuStrip.Items.Add(reportsMenu);
            menuStrip.Items.Add(helpMenu);

            statusStrip = new StatusStrip();
            statusStrip.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
            statusStrip.Padding = new Padding(5, 0, 5, 0);
            statusLabel = new ToolStripStatusLabel("✅ Статус: Подключено");
            userLabel = new ToolStripStatusLabel($"👤 Пользователь: {DatabaseHelper.CurrentUser.FullName}");
            roleLabel = new ToolStripStatusLabel($"🔑 Роль: {DatabaseHelper.CurrentUser.Role}");
            ToolStripStatusLabel recordsLabel = new ToolStripStatusLabel("📋 Записей: 0");

            statusStrip.Items.Add(statusLabel);
            statusStrip.Items.Add(new ToolStripStatusLabel(" | "));
            statusStrip.Items.Add(userLabel);
            statusStrip.Items.Add(new ToolStripStatusLabel(" | "));
            statusStrip.Items.Add(roleLabel);
            statusStrip.Items.Add(new ToolStripStatusLabel(" | "));
            statusStrip.Items.Add(recordsLabel);

            tabControl = new TabControl();
            tabControl.Dock = DockStyle.Fill;
            tabControl.Font = new System.Drawing.Font("Segoe UI", 10);
            tabControl.Padding = new Point(12, 8);
            tabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;

            TabPage tabAuthors = new TabPage("👥 Сотрудники");
            CreateAuthorsTab(tabAuthors);
            tabControl.TabPages.Add(tabAuthors);

            TabPage tabArticles = new TabPage("📝 Статьи");
            CreateArticlesTab(tabArticles);
            tabControl.TabPages.Add(tabArticles);

            TabPage tabIssues = new TabPage("📰 Выпуски");
            CreateIssuesTab(tabIssues);
            tabControl.TabPages.Add(tabIssues);

            this.Controls.Add(tabControl);
            this.Controls.Add(menuStrip);
            this.Controls.Add(statusStrip);
            this.MainMenuStrip = menuStrip;
        }

        private void CreateAuthorsTab(TabPage tab)
        {
            DataGridView dgv = new DataGridView();
            dgv.Name = "dgvAuthors";
            dgv.Dock = DockStyle.Fill;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;
            dgv.ReadOnly = true;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.BackgroundColor = System.Drawing.Color.White;
            dgv.BorderStyle = BorderStyle.None;
            dgv.GridColor = System.Drawing.Color.FromArgb(230, 230, 230);
            dgv.RowHeadersVisible = false;
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.FromArgb(64, 64, 64);
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.ColumnHeadersHeight = 40;
            dgv.RowTemplate.Height = 35;
            dgv.RowTemplate.MinimumHeight = 35;
            dgv.CellDoubleClick += Dgv_CellDoubleClick;

            // Панель поиска
            Panel searchPanel = new Panel();
            searchPanel.Dock = DockStyle.Top;
            searchPanel.Height = 50;
            searchPanel.BackColor = System.Drawing.Color.FromArgb(250, 250, 250);
            searchPanel.Padding = new Padding(10, 8, 10, 8);

            Label lblSearch = new Label() { Text = "🔍 Поиск:", Left = 10, Top = 10, Width = 60 };
            TextBox txtSearch = new TextBox() { Left = 70, Top = 8, Width = 200, Height = 28 };
            Button btnSearch = new Button()
            {
                Text = "Найти",
                Left = 280,
                Top = 8,
                Width = 80,
                Height = 28,
                BackColor = System.Drawing.Color.FromArgb(0, 120, 215),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSearch.FlatAppearance.BorderSize = 0;
            btnSearch.Click += (s, e) => LoadFilteredEmployees(txtSearch.Text);

            Button btnReset = new Button()
            {
                Text = "Сброс",
                Left = 370,
                Top = 8,
                Width = 80,
                Height = 28,
                BackColor = System.Drawing.Color.FromArgb(108, 117, 125),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnReset.FlatAppearance.BorderSize = 0;
            btnReset.Click += (s, e) => {
                txtSearch.Text = "";
                LoadData("employees");
            };

            searchPanel.Controls.Add(lblSearch);
            searchPanel.Controls.Add(txtSearch);
            searchPanel.Controls.Add(btnSearch);
            searchPanel.Controls.Add(btnReset);

            // Панель с кнопками
            Panel buttonPanel = new Panel();
            buttonPanel.Dock = DockStyle.Bottom;
            buttonPanel.Height = 65;
            buttonPanel.BackColor = System.Drawing.Color.FromArgb(248, 248, 248);
            buttonPanel.Padding = new Padding(10);

            Button btnAdd = new Button();
            btnAdd.Text = "➕ Добавить сотрудника";
            btnAdd.Location = new System.Drawing.Point(10, 15);
            btnAdd.Size = new System.Drawing.Size(150, 38);
            btnAdd.BackColor = System.Drawing.Color.FromArgb(0, 120, 215);
            btnAdd.ForeColor = System.Drawing.Color.White;
            btnAdd.FlatStyle = FlatStyle.Flat;
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
            btnAdd.Cursor = Cursors.Hand;
            btnAdd.Click += BtnAddAuthor_Click;
            btnAdd.Enabled = DatabaseHelper.CurrentUser.IsAdmin;

            Button btnEdit = new Button();
            btnEdit.Text = "✏️ Редактировать";
            btnEdit.Location = new System.Drawing.Point(170, 15);
            btnEdit.Size = new System.Drawing.Size(130, 38);
            btnEdit.BackColor = System.Drawing.Color.FromArgb(255, 193, 7);
            btnEdit.ForeColor = System.Drawing.Color.White;
            btnEdit.FlatStyle = FlatStyle.Flat;
            btnEdit.FlatAppearance.BorderSize = 0;
            btnEdit.Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
            btnEdit.Cursor = Cursors.Hand;
            btnEdit.Click += BtnEditAuthor_Click;
            btnEdit.Enabled = DatabaseHelper.CurrentUser.IsAdmin;

            Button btnDelete = new Button();
            btnDelete.Text = "🗑️ Удалить";
            btnDelete.Location = new System.Drawing.Point(310, 15);
            btnDelete.Size = new System.Drawing.Size(110, 38);
            btnDelete.BackColor = System.Drawing.Color.FromArgb(220, 53, 69);
            btnDelete.ForeColor = System.Drawing.Color.White;
            btnDelete.FlatStyle = FlatStyle.Flat;
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
            btnDelete.Cursor = Cursors.Hand;
            btnDelete.Click += BtnDeleteAuthor_Click;
            btnDelete.Enabled = DatabaseHelper.CurrentUser.IsAdmin;

            Button btnRefresh = new Button();
            btnRefresh.Text = "🔄 Обновить";
            btnRefresh.Location = new System.Drawing.Point(430, 15);
            btnRefresh.Size = new System.Drawing.Size(110, 38);
            btnRefresh.BackColor = System.Drawing.Color.FromArgb(108, 117, 125);
            btnRefresh.ForeColor = System.Drawing.Color.White;
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
            btnRefresh.Cursor = Cursors.Hand;
            btnRefresh.Click += (s, arg) => LoadData("employees");

            buttonPanel.Controls.Add(btnAdd);
            buttonPanel.Controls.Add(btnEdit);
            buttonPanel.Controls.Add(btnDelete);
            buttonPanel.Controls.Add(btnRefresh);

            tab.Controls.Add(dgv);
            tab.Controls.Add(buttonPanel);
            tab.Controls.Add(searchPanel);
        }

        private void CreateArticlesTab(TabPage tab)
        {
            DataGridView dgv = new DataGridView();
            dgv.Name = "dgvArticles";
            dgv.Dock = DockStyle.Fill;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;
            dgv.ReadOnly = true;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.BackgroundColor = System.Drawing.Color.White;
            dgv.BorderStyle = BorderStyle.None;
            dgv.GridColor = System.Drawing.Color.FromArgb(230, 230, 230);
            dgv.RowHeadersVisible = false;
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.FromArgb(64, 64, 64);
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.ColumnHeadersHeight = 40;
            dgv.RowTemplate.Height = 35;
            dgv.RowTemplate.MinimumHeight = 35;
            dgv.CellDoubleClick += Dgv_CellDoubleClick;

            // Панель поиска и фильтров
            Panel searchPanel = new Panel();
            searchPanel.Dock = DockStyle.Top;
            searchPanel.Height = 50;
            searchPanel.BackColor = System.Drawing.Color.FromArgb(250, 250, 250);
            searchPanel.Padding = new Padding(10, 8, 10, 8);

            Label lblSearch = new Label() { Text = "🔍 Поиск:", Left = 10, Top = 10, Width = 60 };
            TextBox txtSearch = new TextBox() { Left = 70, Top = 8, Width = 180, Height = 28 };

            Label lblStatus = new Label() { Text = "Статус:", Left = 270, Top = 10, Width = 50 };
            ComboBox cmbStatus = new ComboBox()
            {
                Left = 320,
                Top = 8,
                Width = 120,
                Height = 28,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbStatus.Items.AddRange(new[] { "Все", "Черновик", "Опубликовано", "На редактировании" });
            cmbStatus.SelectedIndex = 0;

            Button btnSearch = new Button()
            {
                Text = "Найти",
                Left = 460,
                Top = 8,
                Width = 80,
                Height = 28,
                BackColor = System.Drawing.Color.FromArgb(0, 120, 215),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSearch.FlatAppearance.BorderSize = 0;
            btnSearch.Click += (s, e) => LoadFilteredArticles(txtSearch.Text, cmbStatus.SelectedItem?.ToString());

            Button btnReset = new Button()
            {
                Text = "Сброс",
                Left = 550,
                Top = 8,
                Width = 80,
                Height = 28,
                BackColor = System.Drawing.Color.FromArgb(108, 117, 125),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnReset.FlatAppearance.BorderSize = 0;
            btnReset.Click += (s, e) => {
                txtSearch.Text = "";
                cmbStatus.SelectedIndex = 0;
                LoadData("articles");
            };

            searchPanel.Controls.Add(lblSearch);
            searchPanel.Controls.Add(txtSearch);
            searchPanel.Controls.Add(lblStatus);
            searchPanel.Controls.Add(cmbStatus);
            searchPanel.Controls.Add(btnSearch);
            searchPanel.Controls.Add(btnReset);

            // Панель с кнопками
            Panel buttonPanel = new Panel();
            buttonPanel.Dock = DockStyle.Bottom;
            buttonPanel.Height = 65;
            buttonPanel.BackColor = System.Drawing.Color.FromArgb(248, 248, 248);
            buttonPanel.Padding = new Padding(10);

            Button btnAdd = new Button();
            btnAdd.Text = "➕ Добавить статью";
            btnAdd.Location = new System.Drawing.Point(10, 15);
            btnAdd.Size = new System.Drawing.Size(140, 38);
            btnAdd.BackColor = System.Drawing.Color.FromArgb(0, 120, 215);
            btnAdd.ForeColor = System.Drawing.Color.White;
            btnAdd.FlatStyle = FlatStyle.Flat;
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
            btnAdd.Cursor = Cursors.Hand;
            btnAdd.Click += BtnAddArticle_Click;
            btnAdd.Enabled = DatabaseHelper.CurrentUser.CanEdit;

            Button btnEdit = new Button();
            btnEdit.Text = "✏️ Редактировать";
            btnEdit.Location = new System.Drawing.Point(160, 15);
            btnEdit.Size = new System.Drawing.Size(130, 38);
            btnEdit.BackColor = System.Drawing.Color.FromArgb(255, 193, 7);
            btnEdit.ForeColor = System.Drawing.Color.White;
            btnEdit.FlatStyle = FlatStyle.Flat;
            btnEdit.FlatAppearance.BorderSize = 0;
            btnEdit.Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
            btnEdit.Cursor = Cursors.Hand;
            btnEdit.Click += BtnEditArticle_Click;
            btnEdit.Enabled = DatabaseHelper.CurrentUser.CanEdit;

            Button btnDelete = new Button();
            btnDelete.Text = "🗑️ Удалить";
            btnDelete.Location = new System.Drawing.Point(300, 15);
            btnDelete.Size = new System.Drawing.Size(110, 38);
            btnDelete.BackColor = System.Drawing.Color.FromArgb(220, 53, 69);
            btnDelete.ForeColor = System.Drawing.Color.White;
            btnDelete.FlatStyle = FlatStyle.Flat;
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
            btnDelete.Cursor = Cursors.Hand;
            btnDelete.Click += BtnDeleteArticle_Click;
            btnDelete.Enabled = DatabaseHelper.CurrentUser.IsAdmin;

            Button btnRefresh = new Button();
            btnRefresh.Text = "🔄 Обновить";
            btnRefresh.Location = new System.Drawing.Point(420, 15);
            btnRefresh.Size = new System.Drawing.Size(110, 38);
            btnRefresh.BackColor = System.Drawing.Color.FromArgb(108, 117, 125);
            btnRefresh.ForeColor = System.Drawing.Color.White;
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
            btnRefresh.Cursor = Cursors.Hand;
            btnRefresh.Click += (s, arg) => LoadData("articles");

            buttonPanel.Controls.Add(btnAdd);
            buttonPanel.Controls.Add(btnEdit);
            buttonPanel.Controls.Add(btnDelete);
            buttonPanel.Controls.Add(btnRefresh);

            tab.Controls.Add(dgv);
            tab.Controls.Add(buttonPanel);
            tab.Controls.Add(searchPanel);
        }

        private void CreateIssuesTab(TabPage tab)
        {
            DataGridView dgv = new DataGridView();
            dgv.Name = "dgvIssues";
            dgv.Dock = DockStyle.Fill;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;
            dgv.ReadOnly = true;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.BackgroundColor = System.Drawing.Color.White;
            dgv.BorderStyle = BorderStyle.None;
            dgv.GridColor = System.Drawing.Color.FromArgb(230, 230, 230);
            dgv.RowHeadersVisible = false;
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.FromArgb(64, 64, 64);
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.ColumnHeadersHeight = 40;
            dgv.RowTemplate.Height = 35;
            dgv.RowTemplate.MinimumHeight = 35;

            Panel panel = new Panel();
            panel.Dock = DockStyle.Bottom;
            panel.Height = 65;
            panel.BackColor = System.Drawing.Color.FromArgb(248, 248, 248);
            panel.Padding = new Padding(10);

            Button btnAdd = new Button();
            btnAdd.Text = "➕ Создать выпуск";
            btnAdd.Location = new System.Drawing.Point(10, 15);
            btnAdd.Size = new System.Drawing.Size(140, 38);
            btnAdd.BackColor = System.Drawing.Color.FromArgb(0, 120, 215);
            btnAdd.ForeColor = System.Drawing.Color.White;
            btnAdd.FlatStyle = FlatStyle.Flat;
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
            btnAdd.Cursor = Cursors.Hand;
            btnAdd.Click += BtnAddIssue_Click;
            btnAdd.Enabled = DatabaseHelper.CurrentUser.IsAdmin;

            Button btnComposition = new Button();
            btnComposition.Text = "📄 Состав выпуска";
            btnComposition.Location = new System.Drawing.Point(160, 15);
            btnComposition.Size = new System.Drawing.Size(140, 38);
            btnComposition.BackColor = System.Drawing.Color.FromArgb(23, 162, 184);
            btnComposition.ForeColor = System.Drawing.Color.White;
            btnComposition.FlatStyle = FlatStyle.Flat;
            btnComposition.FlatAppearance.BorderSize = 0;
            btnComposition.Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
            btnComposition.Cursor = Cursors.Hand;
            btnComposition.Click += BtnIssueComposition_Click;
            btnComposition.Enabled = DatabaseHelper.CurrentUser.CanEdit;

            Button btnDelete = new Button();
            btnDelete.Text = "🗑️ Удалить";
            btnDelete.Location = new System.Drawing.Point(310, 15);
            btnDelete.Size = new System.Drawing.Size(110, 38);
            btnDelete.BackColor = System.Drawing.Color.FromArgb(220, 53, 69);
            btnDelete.ForeColor = System.Drawing.Color.White;
            btnDelete.FlatStyle = FlatStyle.Flat;
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
            btnDelete.Cursor = Cursors.Hand;
            btnDelete.Click += BtnDeleteIssue_Click;
            btnDelete.Enabled = DatabaseHelper.CurrentUser.IsAdmin;

            Button btnRefresh = new Button();
            btnRefresh.Text = "🔄 Обновить";
            btnRefresh.Location = new System.Drawing.Point(430, 15);
            btnRefresh.Size = new System.Drawing.Size(110, 38);
            btnRefresh.BackColor = System.Drawing.Color.FromArgb(108, 117, 125);
            btnRefresh.ForeColor = System.Drawing.Color.White;
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
            btnRefresh.Cursor = Cursors.Hand;
            btnRefresh.Click += (s, arg) => LoadData("issues");

            panel.Controls.Add(btnAdd);
            panel.Controls.Add(btnComposition);
            panel.Controls.Add(btnDelete);
            panel.Controls.Add(btnRefresh);

            tab.Controls.Add(dgv);
            tab.Controls.Add(panel);
        }

        private void LoadFilteredEmployees(string searchText)
        {
            string query = @"SELECT e.id, e.last_name AS ""Фамилия"", e.first_name AS ""Имя"", 
                                    COALESCE(e.middle_name, '') AS ""Отчество"", e.phone AS ""Телефон"", 
                                    e.email AS ""Email"", p.title AS ""Должность"", 
                                    e.hire_date AS ""Дата найма""
                             FROM employees e
                             LEFT JOIN positions p ON e.position_id = p.id
                             WHERE e.email != 'admin@system.ru'";

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                query += @" AND (e.last_name ILIKE @search OR 
                                 e.first_name ILIKE @search OR 
                                 e.email ILIKE @search)";
                parameters.Add(new NpgsqlParameter("@search", $"%{searchText}%"));
            }

            query += " ORDER BY e.last_name";

            DataTable dt = DatabaseHelper.GetDataTable(query, parameters.ToArray());
            DataGridView dgv = GetCurrentDataGridView();
            if (dgv != null)
            {
                dgv.DataSource = dt;
                if (dgv.Columns["id"] != null)
                    dgv.Columns["id"].Visible = false;

                if (statusStrip != null && statusStrip.Items.Count > 6)
                    statusStrip.Items[6].Text = $"📋 Записей: {dt.Rows.Count}";
            }
        }

        private void LoadFilteredArticles(string searchText, string status)
        {
            string query = @"SELECT a.id, a.title AS ""Заголовок"", 
                                    e.last_name || ' ' || e.first_name AS ""Автор"", 
                                    r.title AS ""Рубрика"",
                                    a.created_at AS ""Дата создания"",
                                    a.status AS ""Статус"",
                                    COALESCE(rev.last_name || ' ' || rev.first_name, '---') AS ""Редактор"",
                                    a.reviewed_at AS ""Дата редактирования""
                             FROM articles a
                             LEFT JOIN employees e ON a.author_id = e.id
                             LEFT JOIN rubrics r ON a.rubric_id = r.id
                             LEFT JOIN employees rev ON a.reviewed_by = rev.id
                             WHERE (e.email IS NULL OR e.email != 'admin@system.ru')";

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                query += @" AND (a.title ILIKE @search OR 
                                 a.content ILIKE @search OR 
                                 e.last_name ILIKE @search OR 
                                 e.first_name ILIKE @search)";
                parameters.Add(new NpgsqlParameter("@search", $"%{searchText}%"));
            }

            if (status != "Все")
            {
                query += " AND a.status = @status";
                parameters.Add(new NpgsqlParameter("@status", status));
            }

            query += " ORDER BY a.created_at DESC";

            DataTable dt = DatabaseHelper.GetDataTable(query, parameters.ToArray());
            DataGridView dgv = GetCurrentDataGridView();
            if (dgv != null)
            {
                dgv.DataSource = dt;
                if (dgv.Columns["id"] != null)
                    dgv.Columns["id"].Visible = false;

                if (statusStrip != null && statusStrip.Items.Count > 6)
                    statusStrip.Items[6].Text = $"📋 Записей: {dt.Rows.Count}";
            }
        }

        private void ExportArticlesToExcel(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "CSV файл (*.csv)|*.csv";
            sfd.FileName = $"articles_export_{DateTime.Now:yyyyMMdd_HHmmss}";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                string query = @"SELECT a.id, a.title AS Заголовок, 
                                       e.last_name || ' ' || e.first_name AS Автор,
                                       r.title AS Рубрика,
                                       a.created_at AS Дата_создания,
                                       a.status AS Статус,
                                       COALESCE(rev.last_name || ' ' || rev.first_name, '---') AS Редактор
                                FROM articles a
                                LEFT JOIN employees e ON a.author_id = e.id
                                LEFT JOIN rubrics r ON a.rubric_id = r.id
                                LEFT JOIN employees rev ON a.reviewed_by = rev.id
                                WHERE e.email IS NULL OR e.email != 'admin@system.ru'
                                ORDER BY a.created_at DESC";

                DataTable dt = DatabaseHelper.GetDataTable(query);
                DatabaseHelper.ExportToExcel(dt, sfd.FileName);
            }
        }

        private void TabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl == null) return;

            switch (tabControl.SelectedIndex)
            {
                case 0:
                    currentTable = "employees";
                    LoadData("employees");
                    break;
                case 1:
                    currentTable = "articles";
                    LoadData("articles");
                    break;
                case 2:
                    currentTable = "issues";
                    LoadData("issues");
                    break;
            }
        }

        private void LoadData(string tableName)
        {
            string query = "";
            DataGridView dgv = GetCurrentDataGridView();

            switch (tableName)
            {
                case "employees":
                    query = @"SELECT e.id, e.last_name AS ""Фамилия"", e.first_name AS ""Имя"", 
                                     COALESCE(e.middle_name, '') AS ""Отчество"", e.phone AS ""Телефон"", 
                                     e.email AS ""Email"", p.title AS ""Должность"", 
                                     e.hire_date AS ""Дата найма""
                              FROM employees e
                              LEFT JOIN positions p ON e.position_id = p.id
                              WHERE e.email != 'admin@system.ru'
                              ORDER BY e.last_name";
                    break;

                case "articles":
                    query = @"SELECT a.id, a.title AS ""Заголовок"", 
                                     e.last_name || ' ' || e.first_name AS ""Автор"", 
                                     r.title AS ""Рубрика"",
                                     a.created_at AS ""Дата создания"",
                                     a.status AS ""Статус"",
                                     COALESCE(rev.last_name || ' ' || rev.first_name, '---') AS ""Редактор"",
                                     a.reviewed_at AS ""Дата редактирования""
                              FROM articles a
                              LEFT JOIN employees e ON a.author_id = e.id
                              LEFT JOIN rubrics r ON a.rubric_id = r.id
                              LEFT JOIN employees rev ON a.reviewed_by = rev.id
                              WHERE e.email IS NULL OR e.email != 'admin@system.ru'
                              ORDER BY a.created_at DESC";
                    break;

                case "issues":
                    query = @"SELECT i.id, i.issue_number AS ""Номер выпуска"",
                                     i.release_date AS ""Дата выхода"",
                                     i.print_run AS ""Тираж"",
                                     (SELECT COUNT(*) FROM issue_articles ia WHERE ia.issue_id = i.id) AS ""Кол-во статей""
                              FROM issues i
                              ORDER BY i.release_date DESC";
                    break;
            }

            DataTable dt = DatabaseHelper.GetDataTable(query);
            if (dgv != null)
            {
                dgv.DataSource = dt;
                if (dgv.Columns["id"] != null)
                    dgv.Columns["id"].Visible = false;

                if (statusStrip != null && statusStrip.Items.Count > 6)
                    statusStrip.Items[6].Text = $"📋 Записей: {dt.Rows.Count}";
            }
        }

        private DataGridView GetCurrentDataGridView()
        {
            if (tabControl == null || tabControl.SelectedTab == null) return null;

            foreach (Control ctrl in tabControl.SelectedTab.Controls)
            {
                if (ctrl is DataGridView dgv)
                    return dgv;
            }
            return null;
        }

        private void Dgv_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridView dgv = sender as DataGridView;
                if (dgv == null) return;

                int id = Convert.ToInt32(dgv.Rows[e.RowIndex].Cells["id"].Value);

                if (currentTable == "employees" && DatabaseHelper.CurrentUser.IsAdmin)
                {
                    FormAddAuthor form = new FormAddAuthor(id);
                    if (form.ShowDialog() == DialogResult.OK)
                        LoadData("employees");
                }
                else if (currentTable == "articles" && DatabaseHelper.CurrentUser.CanEdit)
                {
                    FormAddArticle form = new FormAddArticle(id);
                    if (form.ShowDialog() == DialogResult.OK)
                        LoadData("articles");
                }
            }
        }

        private void BtnAddAuthor_Click(object sender, EventArgs e)
        {
            FormAddAuthor form = new FormAddAuthor();
            if (form.ShowDialog() == DialogResult.OK)
                LoadData("employees");
        }

        private void BtnEditAuthor_Click(object sender, EventArgs e)
        {
            DataGridView dgv = GetCurrentDataGridView();
            if (dgv != null && dgv.SelectedRows.Count > 0)
            {
                int id = Convert.ToInt32(dgv.SelectedRows[0].Cells["id"].Value);
                FormAddAuthor form = new FormAddAuthor(id);
                if (form.ShowDialog() == DialogResult.OK)
                    LoadData("employees");
            }
            else
            {
                MessageBox.Show("Выберите сотрудника для редактирования", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnDeleteAuthor_Click(object sender, EventArgs e)
        {
            DataGridView dgv = GetCurrentDataGridView();
            if (dgv != null && dgv.SelectedRows.Count > 0)
            {
                if (MessageBox.Show("Удалить выбранного сотрудника?", "Подтверждение",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    int id = Convert.ToInt32(dgv.SelectedRows[0].Cells["id"].Value);
                    string query = "DELETE FROM employees WHERE id = @id";
                    NpgsqlParameter[] parameters = { new NpgsqlParameter("@id", id) };

                    if (DatabaseHelper.ExecuteNonQuery(query, parameters))
                    {
                        MessageBox.Show("Сотрудник удален", "Успешно",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadData("employees");
                    }
                }
            }
        }

        private void BtnAddArticle_Click(object sender, EventArgs e)
        {
            FormAddArticle form = new FormAddArticle();
            if (form.ShowDialog() == DialogResult.OK)
                LoadData("articles");
        }

        private void BtnEditArticle_Click(object sender, EventArgs e)
        {
            DataGridView dgv = GetCurrentDataGridView();
            if (dgv != null && dgv.SelectedRows.Count > 0)
            {
                int id = Convert.ToInt32(dgv.SelectedRows[0].Cells["id"].Value);
                FormAddArticle form = new FormAddArticle(id);
                if (form.ShowDialog() == DialogResult.OK)
                    LoadData("articles");
            }
        }

        private void BtnDeleteArticle_Click(object sender, EventArgs e)
        {
            DataGridView dgv = GetCurrentDataGridView();
            if (dgv != null && dgv.SelectedRows.Count > 0)
            {
                if (MessageBox.Show("Удалить выбранную статью?", "Подтверждение",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    int id = Convert.ToInt32(dgv.SelectedRows[0].Cells["id"].Value);
                    string query = "DELETE FROM articles WHERE id = @id";
                    NpgsqlParameter[] parameters = { new NpgsqlParameter("@id", id) };

                    if (DatabaseHelper.ExecuteNonQuery(query, parameters))
                    {
                        MessageBox.Show("Статья удалена", "Успешно",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadData("articles");
                    }
                }
            }
        }

        private void BtnAddIssue_Click(object sender, EventArgs e)
        {
            FormAddIssue form = new FormAddIssue();
            if (form.ShowDialog() == DialogResult.OK)
                LoadData("issues");
        }

        private void BtnIssueComposition_Click(object sender, EventArgs e)
        {
            DataGridView dgv = GetCurrentDataGridView();
            if (dgv != null && dgv.SelectedRows.Count > 0)
            {
                int issueId = Convert.ToInt32(dgv.SelectedRows[0].Cells["id"].Value);
                FormIssueComposition form = new FormIssueComposition(issueId);
                form.ShowDialog();
                LoadData("issues");
            }
        }

        private void BtnDeleteIssue_Click(object sender, EventArgs e)
        {
            DataGridView dgv = GetCurrentDataGridView();
            if (dgv != null && dgv.SelectedRows.Count > 0)
            {
                if (MessageBox.Show("Удалить выбранный выпуск?", "Подтверждение",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    int id = Convert.ToInt32(dgv.SelectedRows[0].Cells["id"].Value);
                    string query = "DELETE FROM issues WHERE id = @id";
                    NpgsqlParameter[] parameters = { new NpgsqlParameter("@id", id) };

                    if (DatabaseHelper.ExecuteNonQuery(query, parameters))
                    {
                        MessageBox.Show("Выпуск удален", "Успешно",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadData("issues");
                    }
                }
            }
        }
    }
}