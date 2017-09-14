using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DCNC_Tools.FileSystem.Virtual;
using DCNC_Tools.Formats;

namespace AGTViewer
{
    public sealed partial class Form1 : Form
    {
        public VirtualFolder VirtualFileSystem = new VirtualFolder("ROOT");
        private AGT _openedAgt;

        public Form1()
        {
            InitializeComponent();
            treeView1.OnFillFinished += OnTreeViewFillFinished;

            AllowDrop = true;
            DragEnter += Form1_DragEnter;
            DragDrop += Form1_DragDrop;
        }

        public Form1(string agtFileName)
        {
            InitializeComponent();
            treeView1.OnFillFinished += OnTreeViewFillFinished;

            AllowDrop = true;
            DragEnter += Form1_DragEnter;
            DragDrop += Form1_DragDrop;

            LoadNayaPack(agtFileName);
        }

        private static void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length >= 1)
                LoadNayaPack(files[0]);
        }

        private void OnTreeViewFillFinished(object sender, EventArgs eventArgs)
        {
            Enabled = true;
            toolStripStatusLabel1.Text = $"File (v{_openedAgt.VersionMajor}.{_openedAgt.VersionMinor}) loaded!";
            toolStripProgressBar1.Visible = false;
        }

        public void LoadNayaPack(string fileName)
        {
            toolStripStatusLabel1.Text = $"Loading file {Path.GetFileName(fileName)}";
            toolStripProgressBar1.Visible = true;

            // Wait for Form to be finished loading..
            while (Handle == IntPtr.Zero)
            {
                Thread.Sleep(250);
            }

            Enabled = false;
            Task.Run(() =>
            {
                // TODO: Theoretically we don't even need another copy of AGT.
                _openedAgt = new AGT();
                try
                {
                    _openedAgt.Load(fileName);
                }
                catch (Exception e)
                {
                    _openedAgt = null;
                    MessageBox.Show("Failed to open file.");
                    Invoke((MethodInvoker)(() =>
                    {
                        toolStripStatusLabel1.Text = "File failed!";
                        toolStripProgressBar1.Visible = false;
                        Enabled = true;
                    }));
#if DEBUG
                    throw e;
#endif
                    return;
                }
                
                foreach (var file in _openedAgt.Files)
                {
                    var fileEntryName = file.Key;
                    //treeView1.Nodes.Add(fileEntryName);

                    var path = fileEntryName.Split('\\');

                    var currentFolder = VirtualFileSystem;
                    for (var u = 0; u < path.Length - 1; u++)
                        currentFolder = currentFolder.CreateFolder(path[u]);

                    currentFolder.AddFile(new VirtualFile(fileEntryName, file));
                }

                treeView1.Fill(VirtualFileSystem);
            });
        }

        private void AddSubfolders(TreeNode node, VirtualFolder folder)
        {
            var newnode = new TreeNode(folder.Name) { ImageIndex = 1, SelectedImageIndex = 1, Tag = folder };

            foreach (var subfolder in folder.Folders)
            {
                AddSubfolders(newnode, subfolder);
            }

            foreach (var file in folder.Files)
            {
                var fileNodes = new TreeNode(file.FileName)
                {
                    Tag = file
                };
                newnode.Nodes.Add(fileNodes);
            }

            node.Nodes.Add(newnode);
        }

        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            var node = treeView1.SelectedNode;
            if (node.Tag != null && node.Tag.GetType() == typeof(VirtualFile))
            {
                var tag = node.Tag as VirtualFile;
                var entry = (KeyValuePair<string, byte[]>)tag.InnerFileEntry;
                var viewer = new HexViewer();
                viewer.SetData(entry.Key, entry.Value);
                viewer.Show();
            }
        }

        private void openToolStripButton_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Filter = @"NayaPack (*.agt)|*.agt"
            };
            if (ofd.ShowDialog() != DialogResult.OK) return;
            LoadNayaPack(ofd.FileName);
        }

        private void saveToolStripButton_Click(object sender, EventArgs e)
        {
            // TODO: Still WIP.
            MessageBox.Show("This feature is not yet available!", "Coming Soon!");
        }
    }
}
