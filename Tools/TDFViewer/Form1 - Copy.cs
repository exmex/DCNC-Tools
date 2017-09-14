using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using DCNC_Tools.Formats;
using DCNC_Tools.IO;
using TDFViewer.UI;

namespace TDFViewer
{
    public sealed partial class Form1 : Form
    {
        private FileInfo _openFileInfo;
        private TDF _tdfFile;

        public Form1()
        {
            InitializeComponent();

            AllowDrop = true;
            DragEnter += Form1_DragEnter;
            DragDrop += Form1_DragDrop;
        }

        public Form1(string filePath)
        {
            InitializeComponent();

            var aProp =
                typeof(ListView).GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance);
            if (aProp != null) aProp.SetValue(listView1, true, null);

            LoadFile(filePath);
        }

        private static void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            var files = (string[]) e.Data.GetData(DataFormats.FileDrop);
            if (files.Length >= 1)
            {
                LoadFile(files[0]);
            }
        }

        private void LoadFile(string filePath)
        {
            saveToolStripMenuItem.Enabled = true;
            saveAsToolStripMenuItem.Enabled = true;
            closeToolStripMenuItem.Enabled = true;

            _openFileInfo = new FileInfo(filePath);
            Text = $@"TDF Viewer ({_openFileInfo.Name})";

            listView1.Columns.Clear();
            listView1.Columns.Add("Row");
            listView1.Items.Clear();

            statusStripLabel1.Text = $@"Loading file {_openFileInfo.Name}";
            statusStripProgressBar1.Visible = true;

            _tdfFile = new TDF();
            _tdfFile.Load(filePath);

            listView1.BeginUpdate();
            var columnDefFile = "ColumnDefinitions/ColumnDef_" + Path.GetFileNameWithoutExtension(_openFileInfo.Name) +
                                ".txt";
            if (File.Exists(columnDefFile))
            {
                var colNames = File.ReadLines(columnDefFile).ToList();
                if (colNames.Count != _tdfFile.Header.Col)
                {
                    Debug.WriteLine("Definition column size mismatch");
                    columnDefFile = "ColumnDefinitions/ColumnDef_" +
                                    Path.GetFileNameWithoutExtension(_openFileInfo.Name) + "KR.txt";
                    if (File.Exists(columnDefFile))
                    {
                        colNames = File.ReadLines(columnDefFile).ToList();
                        if (colNames.Count != _tdfFile.Header.Col)
                        {
                            if (colNames.Count > _tdfFile.Header.Col)
                            {
                                MessageBox.Show(@"Column Definition file contains more columns than file itself!",
                                    @"Column Definition size mismatch", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                            }
                            Debug.WriteLine("Definition (KR) column size mismatch");
                        }
                        else
                        {
                            Text = $@"TDF Viewer (KR: {_openFileInfo.Name})";
                        }
                    }
                    else
                    {
                        if (colNames.Count > _tdfFile.Header.Col)
                        {
                            MessageBox.Show(@"Column Definition file contains more columns than file itself!",
                                @"Column Definition size mismatch", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        }
                    }
                }
                else
                {
                    Text = $@"TDF Viewer (US: {_openFileInfo.Name})";
                }

                for (var col = 0; col < _tdfFile.Header.Col; col++)
                {
                    listView1.Columns.Add(col < colNames.Count ? colNames[col] : col.ToString());
                }
            }
            else
            {
                for (var col = 0; col < _tdfFile.Header.Col; col++)
                {
                    listView1.Columns.Add(col.ToString());
                }
            }

            // TODO: Async loading :)?
            using (var reader = new BinaryReaderExt(new MemoryStream(_tdfFile.ResTable)))
            {
                for (var row = 0; row < _tdfFile.Header.Row; row++)
                {
                    var lvi = new ListViewItem
                    {
                        Text = row.ToString()
                    };

                    for (var col = 0; col < _tdfFile.Header.Col; col++)
                    {
                        lvi.SubItems.Add(reader.ReadUnicode());
                    }
                    listView1.Items.Add(lvi);
                }
            }
            listView1.EndUpdate();

            statusStripLabel1.Text =
                $@"File (v{_tdfFile.Version.Major}.{_tdfFile.Version.Minor} - {(int) _tdfFile.Header.Date.Month}/{
                        (int) _tdfFile.Header.Date.Day
                    }/{_tdfFile.Header.Date.Year}) loaded with {_tdfFile.Header.Row} rows and {
                        _tdfFile.Header.Col
                    } cols!";
            statusStripProgressBar1.Visible = false;
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var assembly = Assembly.GetExecutingAssembly();

            var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);

            var ab = new AboutBox
            {
                AppTitle = "TDF Viewer",
                AppDescription = "An editor for TDF files from DriftCity",
                AppVersion = fvi.ProductVersion,
                AppCopyright = "Copyright (c) 2017 GigaToni",
                AppMoreInfo = AboutBox.LicenseInfo + "\n\nhttps://github.com/exmex/DCNC-Tools",
                AppDetailsButton = true
            };
            ab.ShowDialog(this);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_openFileInfo != null)
            {
                if (MessageBox.Show(@"Close already open file and procceed with opening a new one?",
                        @"A file is already open", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    return;
                closeToolStripMenuItem_Click(null, null);
            }

            var ofd = new OpenFileDialog
            {
                Filter = @"TDF File (*.tdf)|*.tdf"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                LoadFile(ofd.FileName);
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_tdfFile == null)
                return;

            var sfd = new SaveFileDialog
            {
                Filter = @"Table Data Format (*.tdf)|*.tdf",
                FileName = Path.GetFileNameWithoutExtension(_openFileInfo.Name),
            };
            if (sfd.ShowDialog() != DialogResult.OK) return;

            SaveAndWriteTdf(sfd.FileName);
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_tdfFile == null)
                return;

            if (MessageBox.Show(@"Are you sure you want to write your changes to the file?", @"Overwrite file?",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;

            SaveAndWriteTdf(_openFileInfo.FullName);
        }

        private void SaveAndWriteTdf(string fileName)
        {
            /*_tdfFile.Header.Col = (uint) listView1.Columns.Count;
            _tdfFile.Header.Row = (uint) listView1.Items.Count;
            //tdfFile.Header.Offset = tdfFile.Header.Col * tdfFile.Header.Row;*/

            //_tdfFile.DataTable = new int[_tdfFile.Header.Col * _tdfFile.Header.Row];
            if (changeDateToolStripMenuItem.Checked)
            {
                _tdfFile.Header.Date.Day = (char) DateTime.Now.Day;
                _tdfFile.Header.Date.Month = (char) DateTime.Now.Month;
                _tdfFile.Header.Date.Year = (ushort) DateTime.Now.Year;
            }

            if (increaseVersionToolStripMenuItem.Checked)
            {
                if (_tdfFile.Version.Minor < 9)
                    _tdfFile.Version.Minor++;
                else
                {
                    _tdfFile.Version.Major++;
                    _tdfFile.Version.Minor = 0;
                }
            }

            var columns = (int) _tdfFile.Header.Col;
            var rows = (int) _tdfFile.Header.Row;
            var offset = (int) _tdfFile.Header.Offset;
            //var size = offset + (columns * 4 * rows + 24);
            var size = offset - (columns * 4 * rows + 24);
            _tdfFile.ResTable = new byte[size];

            // Each column has only 4 bytes?

            using (var writer = new BinaryWriterExt(new MemoryStream(_tdfFile.ResTable)))
            {
                for (var row = 0; row < _tdfFile.Header.Row; row++)
                {
                    for (var col = 1; col < _tdfFile.Header.Col + 1; col++)
                    {
                        var colText = listView1.Items[row].SubItems[col].Text;
                        foreach (var c in colText)
                        {
                            Debug.WriteLineIf(row == 0, "Writing char: " + c);
                            writer.Write(Encoding.ASCII.GetBytes(new[] {c}));
                            writer.Write((byte) 0);
                        }
                        writer.Write(new byte[2]);
                    }
                }
            }

            _tdfFile.Save(fileName);
        }

        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_openFileInfo != null)
            {
                if (MessageBox.Show(@"Close already open file and procceed with importing a new one?",
                        @"A file is already open", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    return;

                closeToolStripMenuItem_Click(null, null);
            }

            var ofd = new OpenFileDialog
            {
                Filter = @"Comma seperated list (*.csv)|*.csv"
            };
            if (ofd.ShowDialog() != DialogResult.OK) return;

            saveToolStripMenuItem.Enabled = true;
            saveAsToolStripMenuItem.Enabled = true;
            closeToolStripMenuItem.Enabled = true;

            listView1.Columns.Clear();
            listView1.Columns.Add(@"Row");

            listView1.Items.Clear();

            _openFileInfo = new FileInfo(ofd.FileName);
            Text = $@"TDF Viewer {_openFileInfo.Name}";

            using (TextReader reader = File.OpenText(ofd.FileName))
            {
                listView1.BeginUpdate();
                string line;
                var row = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    var columns = line.Split(',');
                    var lvi = new ListViewItem(row.ToString());
                    for (var i = 0; i < columns.Length; i++)
                    {
                        if (columns[i] == "")
                            continue;

                        if (listView1.Columns.Count <= i)
                            listView1.Columns.Add(i.ToString());

                        lvi.SubItems.Add(columns[i]);
                    }
                    listView1.Items.Add(lvi);
                    row++;
                }
                listView1.EndUpdate();
            }
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_openFileInfo == null)
            {
                MessageBox.Show(@"No file is open!", @"No file opened!");
                return;
            }

            // TODO: More export options :)
            var sfd = new SaveFileDialog
            {
                Filter = @"Comma seperated list (*.csv)|*.csv|XML (*.xml)|*.xml",
                FileName = Path.GetFileNameWithoutExtension(_openFileInfo.Name),
            };
            if (sfd.ShowDialog() != DialogResult.OK) return;

            var fi = new FileInfo(sfd.FileName);

            if (fi.Extension == ".csv")
            {
                using (TextWriter writer = File.CreateText(sfd.FileName))
                {
                    var sb = new StringBuilder();

                    //Making columns!
                    foreach (ColumnHeader ch in listView1.Columns)
                    {
                        if (ch.Text == @"Row")
                            continue;
                        sb.Append(ch.Text + ",");
                    }

                    sb.AppendLine();

                    //Looping through items and subitems
                    foreach (ListViewItem lvi in listView1.Items)
                    {
                        foreach (ListViewItem.ListViewSubItem lvs in lvi.SubItems)
                        {
                            if (lvs.Text.Trim() == string.Empty)
                                sb.Append(" ,");
                            else
                                sb.Append(lvs.Text + ",");
                        }
                        sb.AppendLine();
                    }
                    writer.Write(sb.ToString());
                }
            }
            else if (fi.Extension == ".xml")
            {
                MessageBox.Show(@"Export to XML is coming soon!", @"Coming soon", MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
                // TODO: Export to XML.
            }
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveToolStripMenuItem.Enabled = false;
            saveAsToolStripMenuItem.Enabled = false;
            closeToolStripMenuItem.Enabled = false;

            Text = @"TDF Viewer";

            listView1.BeginUpdate();
            listView1.Columns.Clear();
            listView1.Clear();
            listView1.EndUpdate();

            _openFileInfo = null;
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var sb = new StringBuilder();
            sb.Append(listView1.SelectedItems[0].Text + ",");
            foreach (ListViewItem.ListViewSubItem col in listView1.SelectedItems[0].SubItems)
            {
                sb.Append(col.Text + ",");
            }
            Clipboard.SetText(sb.ToString());

            statusStripLabel1.Text = @"Copied to clipboard!";
        }

        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (listView1.SelectedItems.Count == 0 || listView1.SelectedItems[0] == null)
                e.Cancel = true;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData != (Keys.Control | Keys.C) || listView1.SelectedItems.Count == 0 ||
                listView1.SelectedItems[0] == null) return base.ProcessCmdKey(ref msg, keyData);

            var sb = new StringBuilder();
            sb.Append(listView1.SelectedItems[0].Text + ",");
            foreach (ListViewItem.ListViewSubItem col in listView1.SelectedItems[0].SubItems)
            {
                sb.Append(col.Text + ",");
            }
            Clipboard.SetText(sb.ToString());
            statusStripLabel1.Text = @"Copied to clipboard!";
            return true;
        }

        private void changeDateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            changeDateToolStripMenuItem.Checked = !changeDateToolStripMenuItem.Checked;
        }

        private void increaseVersionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            increaseVersionToolStripMenuItem.Checked = !increaseVersionToolStripMenuItem.Checked;
        }
    }
}