using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace StudyDocs
{
    public class MainForm : Form
    {
        ComboBox cboSubject = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 160 };
        ComboBox cboType = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 120 };
        TextBox txtSearch = new TextBox { Width = 220 };
        Button btnAdd = new Button { Text = "Thêm", Width = 80 };
        Button btnEdit = new Button { Text = "Sửa", Width = 80 };
        Button btnDelete = new Button { Text = "Xóa", Width = 80 };
        Button btnOpen = new Button { Text = "Mở", Width = 80 };
        Button btnSubject = new Button { Text = "Môn học", Width = 100 };
        DataGridView dgv = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false };
        StatusStrip status = new StatusStrip();
        ToolStripStatusLabel lblCount = new ToolStripStatusLabel();

        public MainForm()
        {   
            Text = "StudyDocs - Quản lý tài liệu học tập";
            Width = 980; Height = 600;

            var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(8), WrapContents = false, AutoSize = false };
            top.Controls.Add(new Label { Text = "Môn:", AutoSize = true, Padding = new Padding(0, 8, 4, 0) });
            top.Controls.Add(cboSubject);
            top.Controls.Add(new Label { Text = "Loại:", AutoSize = true, Padding = new Padding(8, 8, 4, 0) });
            top.Controls.Add(cboType);
            top.Controls.Add(new Label { Text = "Tìm:", AutoSize = true, Padding = new Padding(8, 8, 4, 0) });
            top.Controls.Add(txtSearch);
            top.Controls.Add(btnAdd);
            top.Controls.Add(btnEdit);
            top.Controls.Add(btnDelete);
            top.Controls.Add(btnOpen);
            top.Controls.Add(btnSubject);

            Controls.Add(dgv);
            Controls.Add(top);
            status.Items.Add(lblCount);
            Controls.Add(status);

            Load += MainForm_Load;
            txtSearch.TextChanged += TxtSearch_TextChanged;
            cboSubject.SelectedIndexChanged += CboSubject_SelectedIndexChanged;
            cboType.SelectedIndexChanged += CboType_SelectedIndexChanged;
            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            btnOpen.Click += BtnOpen_Click;
            btnSubject.Click += BtnSubject_Click;
            dgv.CellDoubleClick += Dgv_CellDoubleClick;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            var s = Db.GetSubjects();
            var row = s.NewRow(); row["SubjectId"] = DBNull.Value; row["Name"] = "(Tất cả)"; s.Rows.InsertAt(row, 0);
            cboSubject.DataSource = s; cboSubject.DisplayMember = "Name"; cboSubject.ValueMember = "SubjectId";

            cboType.Items.Add("(Tất cả)");
            string[] types = new string[] { "pdf", "docx", "ppt", "link", "text" };
            for (int i = 0; i < types.Length; i++) cboType.Items.Add(types[i]);
            cboType.SelectedIndex = 0;

            Reload();
        }

        private void Reload()
        {
            string keyword = txtSearch.Text.Trim();
            int? subjectId = cboSubject.SelectedValue as int?;
            string type = cboType.Text == "(Tất cả)" ? null : cboType.Text;

            var dt = Db.GetDocuments(keyword, subjectId, type);
            dgv.DataSource = dt;

            if (dgv.Columns.Contains("DocumentId")) dgv.Columns["DocumentId"].Visible = false;
            if (dgv.Columns.Contains("Title")) dgv.Columns["Title"].HeaderText = "Tiêu đề";
            if (dgv.Columns.Contains("Subject")) dgv.Columns["Subject"].HeaderText = "Môn";
            if (dgv.Columns.Contains("Type")) dgv.Columns["Type"].HeaderText = "Loại";
            if (dgv.Columns.Contains("FilePath")) dgv.Columns["FilePath"].HeaderText = "Đường dẫn";
            if (dgv.Columns.Contains("Notes"))
            {
                dgv.Columns["Notes"].HeaderText = "Ghi chú";
                dgv.Columns["Notes"].Width = 100;
            }
            if (dgv.Columns.Contains("LastOpened")) dgv.Columns["LastOpened"].HeaderText = "Lần mở gần nhất";
            if (dgv.Columns.Contains("Status")) dgv.Columns["Status"].HeaderText = "Đã học";
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            lblCount.Text = $"Tổng số: {dt.Rows.Count} tài liệu";
        }


        private DocumentItem GetSelectedOrNull()
        {
            if (dgv.CurrentRow == null) return null;
            var r = dgv.CurrentRow;
            var it = new DocumentItem();
            it.DocumentId = (int)r.Cells["DocumentId"].Value;
            it.Title = r.Cells["Title"].Value == null ? string.Empty : r.Cells["Title"].Value.ToString();
            it.Type = r.Cells["Type"].Value == null ? string.Empty : r.Cells["Type"].Value.ToString();
            it.FilePath = r.Cells["FilePath"].Value == null ? string.Empty : r.Cells["FilePath"].Value.ToString();
            it.Status = r.Cells["Status"].Value is bool && (bool)r.Cells["Status"].Value;
            return it;
        }

        private void AddDocument()
        {
            using (var f = new UpsertForm())
            {
                if (f.ShowDialog(this) == DialogResult.OK)
                {
                    Db.InsertDocument(f.Value);
                    Reload();
                }
            }
        }

        private void EditSelected()
        {
            var sel = GetSelectedOrNull();
            if (sel == null) return;
            using (var f = new UpsertForm(sel))
            {
                if (f.ShowDialog(this) == DialogResult.OK)
                {
                    var v = f.Value; v.DocumentId = sel.DocumentId;
                    Db.UpdateDocument(v);
                    Reload();
                }
            }
        }

        private void DeleteSelected()
        {
            var sel = GetSelectedOrNull();
            if (sel == null) return;
            if (MessageBox.Show("Xóa tài liệu: " + sel.Title + "?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                Db.DeleteDocument(sel.DocumentId);
                Reload();
            }
        }

        private void OpenSelected()
        {
            if (dgv.CurrentRow == null) return;
            int id = (int)dgv.CurrentRow.Cells["DocumentId"].Value;
            string path = dgv.CurrentRow.Cells["FilePath"].Value == null ? null : dgv.CurrentRow.Cells["FilePath"].Value.ToString();
            if (string.IsNullOrWhiteSpace(path)) { MessageBox.Show("Mục này không có đường dẫn."); return; }
            try
            {
                if (path.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
                }
                else if (File.Exists(path))
                {
                    Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
                }
                else
                {
                    MessageBox.Show("Không tìm thấy tệp tại đường dẫn đã lưu.");
                    return;
                }
                Db.UpdateLastOpened(id);
                Reload();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không mở được: " + ex.Message);
            }
        }

        private void BtnSubject_Click(object sender, EventArgs e)
        {
            using (var f = new SubjectForm())
            {
                f.ShowDialog(this);
                // Sau khi đóng form môn học -> load lại danh sách
                var s = Db.GetSubjects();
                var row = s.NewRow(); row["SubjectId"] = DBNull.Value; row["Name"] = "(Tất cả)";
                s.Rows.InsertAt(row, 0);
                cboSubject.DataSource = s;
            }
        }



        private void TxtSearch_TextChanged(object sender, EventArgs e) { Reload(); }
        private void CboSubject_SelectedIndexChanged(object sender, EventArgs e) { Reload(); }
        private void CboType_SelectedIndexChanged(object sender, EventArgs e) { Reload(); }
        private void BtnAdd_Click(object sender, EventArgs e) { AddDocument(); }
        private void BtnEdit_Click(object sender, EventArgs e) { EditSelected(); }
        private void BtnDelete_Click(object sender, EventArgs e) { DeleteSelected(); }
        private void BtnOpen_Click(object sender, EventArgs e) { OpenSelected(); }
        private void Dgv_CellDoubleClick(object sender, DataGridViewCellEventArgs e) { OpenSelected(); }
    }
}