using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using DCNC_Tools.Formats;
using DCNC_Tools.Utils.DDSReader;

namespace NTXViewer
{
    public partial class TextureViewer : UserControl
    {
        public NTX.TextureFile OpenedFile;
        private Dictionary<int, List<NTX.TextureFile>> _textureFiles;
        public Dictionary<int, List<NTX.TextureFile>> LoadedTextureFiles => _textureFiles;

        public TextureViewer()
        {
            InitializeComponent();
        }

        public void LoadTex(NTX ntex)
        {
            listBox1.Items.Clear();
            comboBox1.Items.Clear();
            OpenedFile = null;
            pictureBox1.Image = null;
            _textureFiles = new Dictionary<int, List<NTX.TextureFile>>();

            var categories = new List<object>();
            foreach (var fileTexture in ntex.Textures)
            {
                var cat = fileTexture.TextureName.Remove(0, fileTexture.TextureName.IndexOf("_", StringComparison.Ordinal));
                if (_textureFiles.ContainsKey(categories.IndexOf(cat)))
                {
                    _textureFiles[categories.IndexOf(cat)].Add(fileTexture);
                }
                else
                {
                    categories.Add(cat);
                    var key = categories.Count - 1;
                    //var key = comboBox1.Items.Add(cat);
                    _textureFiles.Add(key, new List<NTX.TextureFile>()
                    {
                        fileTexture
                    });
                }
            }
            comboBox1.Invoke((MethodInvoker) delegate
            {
                comboBox1.Items.AddRange(categories.ToArray());
                if (comboBox1.Items.Count > 0)
                    comboBox1.SelectedIndex = 0;
                comboBox1.Enabled = true;
            });
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBox1.Enabled = false;

            if (comboBox1.SelectedItem == null) return;
            if (!_textureFiles.ContainsKey(comboBox1.SelectedIndex)) return;

            OpenedFile = null;
            pictureBox1.Image = null;

            listBox1.Items.Clear();
            foreach (var textureFile in _textureFiles[comboBox1.SelectedIndex])
            {
                listBox1.Items.Add(textureFile);
            }
            if (listBox1.Items.Count > 0)
                listBox1.SelectedIndex = 0;
            listBox1.Enabled = true;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem.GetType() != typeof(NTX.TextureFile)) return;
            var textureFile = listBox1.SelectedItem as NTX.TextureFile;
            if (textureFile == null) return;

            OpenedFile = textureFile;
            pictureBox1.Image = DDS.LoadImage(textureFile.Texture);
        }

        public EventHandler SaveContextClick;
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveContextClick?.Invoke(sender, e);
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            if (listBox1.SelectedItem == null || listBox1.SelectedItem.GetType() != typeof(NTX.TextureFile))
            {
                e.Cancel = true;
                return;
            }

            var textureFile = listBox1.SelectedItem as NTX.TextureFile;
            if (textureFile != null) return;
            e.Cancel = true;
        }
    }
}
