using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace TdfReader
{
    public partial class Form1 : Form
    {
        private TdfFile _openFile;

        public Form1()
        {
            InitializeComponent();
        }

        public Form1(string filePath)
        {
            InitializeComponent();

            LoadFile(filePath);
        }

        private void LoadFile(string filePath)
        {
            FileInfo fi = new FileInfo(filePath);
            Text = $"TDF Reader ({fi.Name})";

            listView1.Columns.Clear();
            listView1.Items.Clear();

            _openFile = new TdfFile();
            _openFile.Load(filePath);

            listView1.BeginUpdate();
            for (int col = 0; col < _openFile.Header.Col; col++)
            {
                listView1.Columns.Add(_openFile.GetColumnName(col, fi.Name));
            }

            // TODO: Async loading :)?
            using (BinaryReaderExt reader = new BinaryReaderExt(new MemoryStream(_openFile.ResTable)))
            {
                for (int row = 0; row < _openFile.Header.Row; row++)
                {
                    ListViewItem lvi = new ListViewItem();
                    for (int col = 0; col < _openFile.Header.Col; col++)
                    {
                        if (col != 0)
                            lvi.SubItems.Add(reader.ReadUnicode());
                        else
                        {
                            lvi.Text = reader.ReadUnicode();
                        }
                    }
                    listView1.Items.Add(lvi);
                }
            }
            listView1.EndUpdate();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "TDF File (*.tdf)|*.tdf"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                LoadFile(ofd.FileName);
            }
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // TODO: More export options :)
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Comma seperated list (*.csv)|*.csv"
            };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                using (TextWriter writer = File.CreateText(sfd.FileName))
                {
                    StringBuilder sb = new StringBuilder();

                    //Making columns!
                    foreach (ColumnHeader ch in listView1.Columns)
                    {
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
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Text = $"TDF Reader";

            listView1.BeginUpdate();
            listView1.Columns.Clear();
            listView1.Clear();
            listView1.EndUpdate();
            _openFile = null;
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(listView1.SelectedItems[0].Text+",");
            foreach (ListViewItem.ListViewSubItem col in listView1.SelectedItems[0].SubItems)
            {
                sb.Append(col.Text + ",");
            }
            Clipboard.SetText(sb.ToString());
        }

        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (listView1.SelectedItems.Count == 0 || listView1.SelectedItems[0] == null)
                e.Cancel = true;
        }
    }
}
