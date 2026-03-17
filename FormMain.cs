using System;
using System.Data;
using System.Windows.Forms;
using Npgsql;

namespace Gazeta  
{
    public partial class FormMain : Form
    {
        private string currentTable = "employees";
        private TabControl tabControl;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel statusLabel;
        private ToolStripStatusLabel recordsLabel;

        public FormMain()
        {
            InitializeComponent();
            this.Text = "Редакция газеты - Управление базой данных";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            CreateMainFormControls();
            LoadData("employees");
        }

        private void CreateMainFormControls()
        {
            // Создаем MenuStrip
            MenuStrip menuStrip = new MenuStrip();

            ToolStripMenuItem fileMenu = new ToolStripMenuItem("Файл");
            ToolStripMenuItem exitItem = new ToolStripMenuItem("Выход");
            exitItem.Click += (s, arg) => Application.Exit();
            fileMenu.DropDownItems.Add(exitItem);

            ToolStripMenuItem helpMenu = new ToolStripMenuItem("Справка");
            ToolStripMenuItem aboutItem = new ToolStripMenuItem("О программе");
            aboutItem.Click += (s, arg) => MessageBox.Show("Редакция газеты\nВерсия 1.0\nКурсовая работа", "О программе");
            helpMenu.DropDownItems.Add(aboutItem);

            menuStrip.Items.Add(fileMenu);
            menuStrip.Items.Add(helpMenu);

            // Создаем StatusStrip
            statusStrip = new StatusStrip();
            statusLabel = new ToolStripStatusLabel("Статус: Подключение...");
            recordsLabel = new ToolStripStatusLabel("Записей: 0");
            statusStrip.Items.Add(statusLabel);
            statusStrip.Items.Add(new ToolStripStatusLabel(" "));
            statusStrip.Items.Add(recordsLabel);

            // Создаем TabControl
            tabControl = new TabControl();
            tabControl.Dock = DockStyle.Fill;
            tabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;

            // Вкладка Авторы
            TabPage tabAuthors = new TabPage("Авторы");
            CreateAuthorsTab(tabAuthors);
            tabControl.TabPages.Add(tabAuthors);

            // Вкладка Статьи
            TabPage tabArticles = new TabPage("Статьи");
            CreateArticlesTab(tabArticles);
            tabControl.TabPages.Add(tabArticles);

            // Вкладка Выпуски
            TabPage tabIssues = new TabPage("Выпуски");
            CreateIssuesTab(tabIssues);
            tabControl.TabPages.Add(tabIssues);

            // Добавляем контролы на форму
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
            dgv.CellDoubleClick += Dgv_CellDoubleClick;

            Panel panel = new Panel();
            panel.Dock = DockStyle.Bottom;
            panel.Height = 50;
            panel.BackColor = System.Drawing.Color.LightGray;

            Button btnAdd = new Button();
            btnAdd.Text = "Добавить автора";
            btnAdd.Location = new System.Drawing.Point(10, 10);
            btnAdd.Size = new System.Drawing.Size(120, 30);
            btnAdd.Click += BtnAddAuthor_Click;

            Button btnEdit = new Button();
            btnEdit.Text = "Редактировать";
            btnEdit.Location = new System.Drawing.Point(140, 10);
            btnEdit.Size = new System.Drawing.Size(120, 30);
            btnEdit.Click += BtnEditAuthor_Click;

            Button btnDelete = new Button();
            btnDelete.Text = "Удалить";
            btnDelete.Location = new System.Drawing.Point(270, 10);
            btnDelete.Size = new System.Drawing.Size(120, 30);
            btnDelete.Click += BtnDeleteAuthor_Click;

            Button btnRefresh = new Button();
            btnRefresh.Text = "Обновить";
            btnRefresh.Location = new System.Drawing.Point(400, 10);
            btnRefresh.Size = new System.Drawing.Size(120, 30);
            btnRefresh.Click += (s, arg) => LoadData("employees");

            panel.Controls.Add(btnAdd);
            panel.Controls.Add(btnEdit);
            panel.Controls.Add(btnDelete);
            panel.Controls.Add(btnRefresh);

            tab.Controls.Add(dgv);
            tab.Controls.Add(panel);
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
            dgv.CellDoubleClick += Dgv_CellDoubleClick;

            Panel panel = new Panel();
            panel.Dock = DockStyle.Bottom;
            panel.Height = 50;
            panel.BackColor = System.Drawing.Color.LightGray;

            Button btnAdd = new Button();
            btnAdd.Text = "Добавить статью";
            btnAdd.Location = new System.Drawing.Point(10, 10);
            btnAdd.Size = new System.Drawing.Size(120, 30);
            btnAdd.Click += BtnAddArticle_Click;

            Button btnEdit = new Button();
            btnEdit.Text = "Редактировать";
            btnEdit.Location = new System.Drawing.Point(140, 10);
            btnEdit.Size = new System.Drawing.Size(120, 30);
            btnEdit.Click += BtnEditArticle_Click;

            Button btnDelete = new Button();
            btnDelete.Text = "Удалить";
            btnDelete.Location = new System.Drawing.Point(270, 10);
            btnDelete.Size = new System.Drawing.Size(120, 30);
            btnDelete.Click += BtnDeleteArticle_Click;

            Button btnRefresh = new Button();
            btnRefresh.Text = "Обновить";
            btnRefresh.Location = new System.Drawing.Point(400, 10);
            btnRefresh.Size = new System.Drawing.Size(120, 30);
            btnRefresh.Click += (s, arg) => LoadData("articles");

            panel.Controls.Add(btnAdd);
            panel.Controls.Add(btnEdit);
            panel.Controls.Add(btnDelete);
            panel.Controls.Add(btnRefresh);

            tab.Controls.Add(dgv);
            tab.Controls.Add(panel);
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

            Panel panel = new Panel();
            panel.Dock = DockStyle.Bottom;
            panel.Height = 50;
            panel.BackColor = System.Drawing.Color.LightGray;

            Button btnAdd = new Button();
            btnAdd.Text = "Создать выпуск";
            btnAdd.Location = new System.Drawing.Point(10, 10);
            btnAdd.Size = new System.Drawing.Size(120, 30);
            btnAdd.Click += BtnAddIssue_Click;

            Button btnComposition = new Button();
            btnComposition.Text = "Состав выпуска";
            btnComposition.Location = new System.Drawing.Point(140, 10);
            btnComposition.Size = new System.Drawing.Size(120, 30);
            btnComposition.Click += BtnIssueComposition_Click;

            Button btnDelete = new Button();
            btnDelete.Text = "Удалить";
            btnDelete.Location = new System.Drawing.Point(270, 10);
            btnDelete.Size = new System.Drawing.Size(120, 30);
            btnDelete.Click += BtnDeleteIssue_Click;

            Button btnRefresh = new Button();
            btnRefresh.Text = "Обновить";
            btnRefresh.Location = new System.Drawing.Point(400, 10);
            btnRefresh.Size = new System.Drawing.Size(120, 30);
            btnRefresh.Click += (s, arg) => LoadData("issues");

            panel.Controls.Add(btnAdd);
            panel.Controls.Add(btnComposition);
            panel.Controls.Add(btnDelete);
            panel.Controls.Add(btnRefresh);

            tab.Controls.Add(dgv);
            tab.Controls.Add(panel);
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
                    query = @"SELECT e.id, e.full_name AS ""ФИО"", e.phone AS ""Телефон"", 
                                     e.email AS ""Email"", p.title AS ""Должность"", 
                                     e.hire_date AS ""Дата найма""
                              FROM employees e
                              LEFT JOIN positions p ON e.position_id = p.id
                              ORDER BY e.full_name";
                    break;

                case "articles":
                    query = @"SELECT a.id, a.title AS ""Заголовок"", 
                                     e.full_name AS ""Автор"", 
                                     r.title AS ""Рубрика"",
                                     a.created_at AS ""Дата создания"",
                                     a.status AS ""Статус""
                              FROM articles a
                              LEFT JOIN employees e ON a.author_id = e.id
                              LEFT JOIN rubrics r ON a.rubric_id = r.id
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

                if (recordsLabel != null)
                    recordsLabel.Text = $"Записей: {dt.Rows.Count}";
                if (statusLabel != null)
                    statusLabel.Text = "Статус: Подключено";
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

                if (currentTable == "employees")
                {
                    FormAddAuthor form = new FormAddAuthor(id);
                    if (form.ShowDialog() == DialogResult.OK)
                        LoadData("employees");
                }
                else if (currentTable == "articles")
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
                MessageBox.Show("Выберите автора для редактирования", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnDeleteAuthor_Click(object sender, EventArgs e)
        {
            DataGridView dgv = GetCurrentDataGridView();
            if (dgv != null && dgv.SelectedRows.Count > 0)
            {
                if (MessageBox.Show("Удалить выбранного автора?", "Подтверждение",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    int id = Convert.ToInt32(dgv.SelectedRows[0].Cells["id"].Value);
                    string query = "DELETE FROM employees WHERE id = @id";
                    NpgsqlParameter[] parameters = { new NpgsqlParameter("@id", id) };

                    if (DatabaseHelper.ExecuteNonQuery(query, parameters))
                    {
                        MessageBox.Show("Автор удален", "Успешно",
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
                LoadData("issues"); // Обновляем данные после изменения состава
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