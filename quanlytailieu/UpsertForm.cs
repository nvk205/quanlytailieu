using System;
using System.Data;
using System.Windows.Forms;

namespace StudyDocs
{
    public class UpsertForm : Form
    {
        TextBox txtTitle = new TextBox { Width = 320 };
        ComboBox cboSubject = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 200 };
        ComboBox cboType = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 120 };
        TextBox txtPath = new TextBox { Width = 320 };
        Button btnBrowse = new Button { Text = "...", Width = 32 };
        TextBox txtNotes = new TextBox { Width = 320 };
        CheckBox chkDone = new CheckBox { Text = "Đã học" };
        Button btnOk = new Button { Text = "Lưu", Width = 90 };
        Button btnCancel = new Button { Text = "Hủy", Width = 90 };
        ErrorProvider ep = new ErrorProvider();

        public DocumentItem Value { get; private set; }

        public UpsertForm() : this(null) { }

        public UpsertForm(DocumentItem existing)
        {
            Value = new DocumentItem();

            Text = existing == null ? "Thêm tài liệu" : "Sửa tài liệu";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false; MinimizeBox = false; AutoSize = true; AutoSizeMode = AutoSizeMode.GrowAndShrink;

            var layout = new TableLayoutPanel { ColumnCount = 3, RowCount = 6, Padding = new Padding(12), AutoSize = true };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            layout.Controls.Add(new Label { Text = "Tiêu đề:", AutoSize = true }, 0, 0);
            layout.Controls.Add(txtTitle, 1, 0);

            layout.Controls.Add(new Label { Text = "Môn học:", AutoSize = true }, 0, 1);
            layout.Controls.Add(cboSubject, 1, 1);

            layout.Controls.Add(new Label { Text = "Loại:", AutoSize = true }, 0, 2);
            layout.Controls.Add(cboType, 1, 2);

            layout.Controls.Add(new Label { Text = "Đường dẫn:", AutoSize = true }, 0, 3);
            layout.Controls.Add(txtPath, 1, 3);
            layout.Controls.Add(btnBrowse, 2, 3);

            layout.Controls.Add(new Label { Text = "Ghi chú:", AutoSize = true }, 0, 4);
            layout.Controls.Add(txtNotes, 1, 4);

            layout.Controls.Add(chkDone, 1, 5);

            var bottom = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill, AutoSize = true };
            bottom.Controls.Add(btnCancel);
            bottom.Controls.Add(btnOk);
            layout.Controls.Add(bottom, 1, 6);

            Controls.Add(layout);

            Load += UpsertForm_Load;
            btnBrowse.Click += BtnBrowse_Click;
            btnOk.Click += BtnOk_Click;
            btnCancel.Click += BtnCancel_Click;

            if (existing != null)
            {
                txtTitle.Text = existing.Title;
                txtPath.Text = existing.FilePath;
                chkDone.Checked = existing.Status;
                Tag = existing; // store for later use
            }
        }

        private void UpsertForm_Load(object sender, EventArgs e)
        {
            var s = Db.GetSubjects();
            var row = s.NewRow(); row["SubjectId"] = DBNull.Value; row["Name"] = "(Không)"; s.Rows.InsertAt(row, 0);
            cboSubject.DataSource = s; cboSubject.DisplayMember = "Name"; cboSubject.ValueMember = "SubjectId";

            string[] types = new string[] { "pdf", "docx", "ppt", "link", "text" };
            for (int i = 0; i < types.Length; i++) cboType.Items.Add(types[i]);
            cboType.SelectedIndex = 0;

            if (Tag is DocumentItem)
            {
                var ex = (DocumentItem)Tag;
                int idx = cboType.FindStringExact(ex.Type);
                if (idx >= 0) cboType.SelectedIndex = idx;
                if (ex.SubjectId.HasValue)
                {
                    foreach (object o in cboSubject.Items)
                    {
                        var it = (DataRowView)o;
                        int? sid = it.Row.Field<int?>("SubjectId");
                        if (sid.HasValue && sid.Value == ex.SubjectId.Value) { cboSubject.SelectedItem = o; break; }
                    }
                }
            }
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            if (cboType.SelectedItem != null && cboType.SelectedItem.ToString() == "link")
            {
                MessageBox.Show("Loại 'link' không cần duyệt file. Hãy điền URL vào ô Đường dẫn.");
                return;
            }
            using (var ofd = new OpenFileDialog { Title = "Chọn tệp tài liệu" })
            {
                if (ofd.ShowDialog() == DialogResult.OK) txtPath.Text = ofd.FileName;
            }
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            ep.Clear();
            bool ok = true;
            if (string.IsNullOrWhiteSpace(txtTitle.Text)) { ep.SetError(txtTitle, "Nhập tiêu đề"); ok = false; }
            if (cboType.SelectedItem == null) { ep.SetError(cboType, "Chọn loại"); ok = false; }
            if (!ok) return;

            Value = new DocumentItem();
            Value.Title = txtTitle.Text.Trim();
            Value.SubjectId = (cboSubject.SelectedValue is int) ? (int?)((int)cboSubject.SelectedValue) : null;
            Value.Type = cboType.SelectedItem.ToString();
            Value.FilePath = txtPath.Text.Trim();
            Value.Notes = txtNotes.Text.Trim();
            Value.Status = chkDone.Checked;
            DialogResult = DialogResult.OK;
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}