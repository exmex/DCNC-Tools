using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DCNC_Tools.Formats;
using TDFViewer.UI;
using TDFViewer.Utils;

namespace TDFViewer
{
    public sealed partial class Form1 : Form
    {
        private FileInfo _openFileInfo;
        private TDFDictionaryConverter _tdfConverter;

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

            AllowDrop = true;
            DragEnter += Form1_DragEnter;
            DragDrop += Form1_DragDrop;

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
                LoadFile(files[0]);
        }

        public void OnFileLoaded()
        {
            saveToolStripMenuItem.Enabled = true;
            saveAsToolStripMenuItem.Enabled = true;
            closeToolStripMenuItem.Enabled = true;

            /*statusStripLabel1.Text =
                $@"File (v{tdf.Version.Major}.{tdf.Version.Minor} - {(int)tdf.Header.Date.Month}/{
                        (int)tdf.Header.Date.Day
                    }/{tdf.Header.Date.Year}) loaded with {tdf.Header.Row} rows and {
                        tdf.Header.Col
                    } cols!";*/
            statusStripProgressBar1.Visible = false;
        }

        private void LoadFile(string filePath)
        {
            var tdf = new TDF();
            tdf.Load(filePath);
            _tdfConverter = new TDFDictionaryConverter(tdf);
            try
            {
                _tdfConverter.GetColumnDefinitions(filePath);
            }
            catch (Exception)
            {
                /* ignored */
            }
            _openFileInfo = new FileInfo(filePath);
            Text = $@"{_openFileInfo.Name} - TDF Viewer";

            LoadFile();
        }

        private void LoadFile()
        {
            listView1.Columns.Clear();
            listView1.Items.Clear();

            Task.Run(() =>
            {
                try
                {
                    var columns = new List<ColumnHeader>();
                    var rows = new List<ListViewItem>();
                    foreach (var row in _tdfConverter.Rows)
                    {
                        var lvi = new ListViewItem(row.Key.ToString(CultureInfo.InvariantCulture));
                        foreach (var column in row.Value)
                            lvi.SubItems.Add(column);
                        rows.Add(lvi);

                        if (row.Value.Count <= columns.Count) continue;

                        for (var i = columns.Count; i < row.Value.Count; i++)
                            if (_tdfConverter.ColumnDefinitions.Count > i)
                                columns.Add(new ColumnHeader {Text = _tdfConverter.ColumnDefinitions[i]});
                            else
                                columns.Add(new ColumnHeader {Text = i.ToString(CultureInfo.InvariantCulture)});
                    }

                    Invoke((MethodInvoker) (() =>
                    {
                        listView1.BeginUpdate();
                        listView1.Columns.AddRange(columns.ToArray());
                        listView1.Items.AddRange(rows.ToArray());
                        listView1.EndUpdate();

                        OnFileLoaded();
                    }));
                }
                catch (Exception e)
                {
#if DEBUG
                    MessageBox.Show(e.Message);
#endif
                }
            });
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
            if (listView1.Items.Count != 0 && _tdfConverter != null)
            {
                if (MessageBox.Show("Close already open file and procceed with opening a new one?",
                        "A file is already open", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    return;
                closeToolStripMenuItem_Click(null, null);
            }

            var ofd = new OpenFileDialog
            {
                Filter = @"TDF File (*.tdf)|*.tdf"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
                LoadFile(ofd.FileName);
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("This feature is not yet available!", "Coming Soon!");
            /*if (_tdfFile == null)
                return;

            var sfd = new SaveFileDialog
            {
                Filter = @"Table Data Format (*.tdf)|*.tdf",
                FileName = Path.GetFileNameWithoutExtension(_openFileInfo.Name),
            };
            if (sfd.ShowDialog() != DialogResult.OK) return;

            SaveAndWriteTdf(sfd.FileName);*/
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("This feature is not yet available!", "Coming Soon!");
            /*if (_tdfFile == null)
                return;

            if (MessageBox.Show(@"Are you sure you want to write your changes to the file?", @"Overwrite file?",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;

            SaveAndWriteTdf(_openFileInfo.FullName);*/
        }

        private void SaveAndWriteTdf(string fileName)
        {
            /*_tdfFile.Header.Col = (uint) listView1.Columns.Count;
            _tdfFile.Header.Row = (uint) listView1.Items.Count;
            //tdfFile.Header.Offset = tdfFile.Header.Col * tdfFile.Header.Row;*/

            //_tdfFile.DataTable = new int[_tdfFile.Header.Col * _tdfFile.Header.Row];
            /*
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

            _tdfFile.Save(fileName);*/
        }

        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.Items.Count != 0 && _tdfConverter != null)
            {
                if (MessageBox.Show(@"Close already open file and procceed with opening a new one?",
                        @"A file is already open", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    return;
                closeToolStripMenuItem_Click(null, null);
            }

            var ofd = new OpenFileDialog
            {
                Filter = @"Comma seperated list (*.csv)|*.csv|XML (*.xml)|*.xml|All files|*.*"
            };
            if (ofd.ShowDialog() != DialogResult.OK) return;

            listView1.Columns.Clear();
            listView1.Items.Clear();

            _openFileInfo = new FileInfo(ofd.FileName);
            Text = $@"{_openFileInfo.Name} - TDF Viewer";

            _tdfConverter = new TDFDictionaryConverter();
            _tdfConverter.Import(ofd.FileName);
            LoadFile();
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.Items.Count == 0 || _tdfConverter == null || _openFileInfo == null) return;
            var sfd = new SaveFileDialog
            {
                Filter = @"Comma seperated list (*.csv)|*.csv|XML (*.xml)|*.xml|All files|*.*",
                FileName = Path.GetFileNameWithoutExtension(_openFileInfo.Name)
            };
            if (sfd.ShowDialog() != DialogResult.OK) return;
            _tdfConverter.Export(sfd.FileName);
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
                sb.Append(col.Text + ",");
            Clipboard.SetText(sb.ToString());
            statusStripLabel1.Text = $"Copied row {listView1.SelectedItems[0].Text} to clipboard!";
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            if (listView1.SelectedItems.Count == 0 || listView1.SelectedItems[0] == null)
                e.Cancel = true;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData != (Keys.Control | Keys.C) || listView1.SelectedItems.Count == 0 ||
                listView1.SelectedItems[0] == null) return base.ProcessCmdKey(ref msg, keyData);

            copyToolStripMenuItem_Click(null, null);
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