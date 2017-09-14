using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using DCNC_Tools.Formats;
using DCNC_Tools.Utils;

namespace NTXViewer
{
    public sealed partial class Viewer : Form
    {
        public Viewer()
        {
            InitializeComponent();

            AllowDrop = true;
            DragEnter += Form1_DragEnter;
            DragDrop += Form1_DragDrop;

            textureViewer1.SaveContextClick += SaveContextClick;
        }

        public Viewer(string fileName)
        {
            InitializeComponent();

            AllowDrop = true;
            DragEnter += Form1_DragEnter;
            DragDrop += Form1_DragDrop;

            textureViewer1.SaveContextClick += SaveContextClick;

            LoadNtxAsync(fileName);
        }

        private void SaveContextClick(object sender, EventArgs eventArgs)
        {
            if (textureViewer1.OpenedFile == null)
            {
                MessageBox.Show(@"No image selected.");
                return;
            }

            var sfd = new SaveFileDialog
            {
                Filter =
                    @"JPEG (*.jpg)|*.jpg|Portable Network Graphics (*.png)|*.png|DirectDraw Surface (*.dds)|*.dds|All files (*.*)|*.*",
                CheckPathExists = true,
                DefaultExt = "dds",
                FileName = Path.GetFileNameWithoutExtension(textureViewer1.OpenedFile.TextureName)
            };

            if (sfd.ShowDialog() != DialogResult.OK) return;

            textureViewer1.OpenedFile.Save(sfd.FileName);
        }

        private static void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            var files = (string[]) e.Data.GetData(DataFormats.FileDrop);
            if (files.Length == 1)
            {
                LoadNtxAsync(files[0]);
            }
            else if (files.Length > 1)
            {
                MessageBox.Show(@"Multiple files are not supported!", @"GameBryo NTX Viewer", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                LoadMultipleAsync(files);
            }
        }

        private void openToolStripButton_Click(object sender, EventArgs e)
        {
            if (!openToolStripButton.Enabled) return;

            var ofd = new OpenFileDialog
            {
                Multiselect = false,
                CheckFileExists = true,
                Filter = @"GameBryo NTX (*.ntx)|*.ntx|All files (*.*)|*.*"
            };
            if (ofd.ShowDialog() != DialogResult.OK) return;

            LoadNtxAsync(ofd.FileName);
        }

        private void LoadMultipleAsync(string[] fileNames)
        {
            toolStripProgressBar1.Visible = true;
            openToolStripButton.Enabled = false;
            saveToolStripButton.Enabled = false;

            Task.Run(() =>
            {
                var stopwatch = Stopwatch.StartNew();
                var ntex = new NTX();
                var failed = 0;

                foreach (var fileName in fileNames)
                {
                    Invoke((MethodInvoker) delegate
                    {
                        toolStripProgressBar1.Visible = false;
                        toolStripStatusLabel1.Text = $@"Loading {Path.GetFileNameWithoutExtension(fileName)}...";

                        openToolStripButton.Enabled = true;
                        saveToolStripButton.Enabled = true;
                    });

                    try
                    {
                        ntex.Load(fileName);
                    }
                    catch (Exception)
                    {
                        failed++;
                    }
                }

                Invoke((MethodInvoker) delegate
                {
                    textureViewer1.LoadTex(ntex);

                    stopwatch.Stop();

                    toolStripProgressBar1.Visible = false;
                    toolStripStatusLabel1.Text =
                        $@"Loaded {ntex.Textures.Count} out of {fileNames.Length} NTX Files ({
                                failed
                            } failed) Textures in {stopwatch.ElapsedMilliseconds} ms!";

                    openToolStripButton.Enabled = true;
                    saveToolStripButton.Enabled = true;
                });
            });
        }

        private void LoadNtxAsync(string fileName)
        {
            toolStripProgressBar1.Visible = true;
            openToolStripButton.Enabled = false;
            saveToolStripButton.Enabled = false;
            toolStripStatusLabel1.Text = $@"Loading {Path.GetFileNameWithoutExtension(fileName)}...";

            Task.Run(() =>
            {
                var stopwatch = Stopwatch.StartNew();
                var ntex = new NTX();
                try
                {
                    ntex.Load(fileName);
                    textureViewer1.LoadTex(ntex);

                    stopwatch.Stop();

                    Invoke((MethodInvoker) delegate
                    {
                        toolStripProgressBar1.Visible = false;
                        toolStripStatusLabel1.Text =
                            $@"Loaded {ntex.Textures.Count} Textures in {stopwatch.ElapsedMilliseconds} ms!";

                        openToolStripButton.Enabled = true;
                        saveToolStripButton.Enabled = true;
                    });
                }
                catch (Exception)
                {
                    stopwatch.Stop();

                    Invoke((MethodInvoker)delegate
                    {
                        toolStripProgressBar1.Visible = false;
                        toolStripStatusLabel1.Text = $@"Load failed. Invalid file?";
                        openToolStripButton.Enabled = true;
                        MessageBox.Show(@"This is not a valid GameBryo NTX file!", @"GameBryo NTX Viewer",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    });
                }
            });
        }

        private void saveToolStripButton_Click(object sender, EventArgs e)
        {
            if (!saveToolStripButton.Enabled) return;

            var fbd = new FolderBrowserDialog
            {
                ShowNewFolderButton = true,
                SelectedPath = Application.StartupPath
            };
            if (fbd.ShowDialog() != DialogResult.OK) return;

            if (Directory.EnumerateFiles(fbd.SelectedPath, "*.*", SearchOption.AllDirectories).ToArray().Length != 0)
                if (MessageBox.Show(@"Folder is not empty! Proceed anyway?", @"GameBryo NTX Viewer",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    return;

            openToolStripButton.Enabled = false;
            saveToolStripButton.Enabled = false;
            Task.Run(() =>
            {
                Invoke((MethodInvoker) delegate
                {
                    toolStripStatusLabel1.Text = @"Calculating sizes...";
                    toolStripProgressBar1.Visible = true;
                    toolStripProgressBar1.Style = ProgressBarStyle.Blocks;
                    toolStripProgressBar1.Value = 0;
                    toolStripProgressBar1.Maximum =
                        textureViewer1.LoadedTextureFiles.Sum(texturefile => texturefile.Value.Count);
                    toolStripProgressBar1.Step = 1;
                    toolStripProgressBar1.Minimum = 0;
                });

                foreach (var textureFile in textureViewer1.LoadedTextureFiles)
                foreach (var file in textureFile.Value)
                {
                    Invoke((MethodInvoker) delegate
                    {
                        toolStripStatusLabel1.Text = $@"Exporting {file}...";
                        toolStripProgressBar1.PerformStep();
                    });
                    if (File.Exists(Path.Combine(fbd.SelectedPath, file.TextureName)))
                        if (MessageBox.Show($@"Overwrite (Yes) file {file} or skip (No) it?", @"File already exists",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.No)
                            continue;
                    File.WriteAllBytes(Path.Combine(fbd.SelectedPath, file.ToString()), file.Texture);
                    //file.Save(Path.Combine(fbd.SelectedPath, file.TextureName));
                }

                Invoke((MethodInvoker) delegate
                {
                    toolStripProgressBar1.Style = ProgressBarStyle.Marquee;
                    toolStripProgressBar1.Visible = false;
                    toolStripStatusLabel1.Text = @"Export of all textures finished!";

                    openToolStripButton.Enabled = true;
                    saveToolStripButton.Enabled = true;
                });
            });
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Control | Keys.S:
                    SaveContextClick(null, null);
                    return true;
                case Keys.Control | Keys.Shift | Keys.S:
                    saveToolStripButton_Click(saveToolStripButton, null);
                    return true;
                case Keys.Control | Keys.O:
                    openToolStripButton_Click(openToolStripButton, null);
                    return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void helpToolStripButton_Click(object sender, EventArgs e)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);

            var ab = new AboutBox
            {
                AppTitle = "GameBryo NTX Viewer",
                AppDescription = "A Viewer for NTX Files from DriftCity",
                AppVersion = fvi.ProductVersion,
                AppCopyright = "Copyright (c) 2017 GigaToni",
                AppMoreInfo = AboutBox.LicenseInfo + "\n\nhttps://github.com/exmex/DCNC-Tools",
                AppDetailsButton = true
            };
            ab.ShowDialog(this);
        }
    }
}