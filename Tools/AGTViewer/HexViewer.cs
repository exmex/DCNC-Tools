using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AGTViewer
{
    public partial class HexViewer : Form
    {
        private string _fileName;

        public HexViewer()
        {
            InitializeComponent();
        }

        public void SetData(string name, byte[] data)
        {
            _fileName = name;
            Text = $@"HexViewer ({_fileName})";
            byteViewer1.SetBytes(data);
        }

        private void ChangeByteMode(object sender, EventArgs e)
        {
            if (sender == null || sender.GetType() != typeof(ToolStripMenuItem)) return;

            var menuItem = (ToolStripMenuItem)sender;

            hexToolStripMenuItem.Checked = false;
            textToolStripMenuItem.Checked = false;
            unicodeToolStripMenuItem.Checked = false;
            autoToolStripMenuItem.Checked = false;

            DisplayMode mode;
            switch (menuItem.Text)
            {
                case "ANSI":
                    mode = DisplayMode.Ansi;
                    menuItem.Checked = true;
                    break;
                case "Hex":
                    mode = DisplayMode.Hexdump;
                    menuItem.Checked = true;
                    break;
                case "Unicode":
                    mode = DisplayMode.Unicode;
                    menuItem.Checked = true;
                    break;
                default:
                    mode = DisplayMode.Auto;
                    menuItem.Checked = true;
                    break;
            }
            byteViewer1.SetDisplayMode(mode);
        }


        private void saveToolStripButton_Click(object sender, EventArgs e)
        {
            var sfd = new SaveFileDialog();
            sfd.FileName = _fileName;
            if (sfd.ShowDialog() != DialogResult.OK) return;
            File.WriteAllBytes(sfd.FileName, byteViewer1.GetBytes());
        }
    }
}
