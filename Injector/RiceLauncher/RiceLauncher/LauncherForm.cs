using InjectionLibrary;
using JLibrary.PortableExecutable;
using Newtonsoft.Json;
using RiceLauncher.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace RiceLauncher
{
	public class LauncherForm : Form
	{
		private const string configPath = "riceconfig.json";

		private const string activeServerPath = "rice.server";

		private const string driftPath = "DriftCity.exe";

		private IContainer components;

		private StatusStrip statusStrip1;

		private ToolStripStatusLabel statusLabel;

		private ListBox serverListBox;

		private Button launchButton;

		private TextBox ipBox;

		private Label label1;

		private TextBox nameBox;

		private Label label3;

		private Button addButton;

		private Button remButton;

		private ToolStripDropDownButton toolStripDropDownButton1;

		private ToolStripMenuItem perlSavetherobotsToolStripMenuItem;

		private ToolStripMenuItem creditsToolStripMenuItem;

		[DllImport("winmm.dll", EntryPoint = "timeGetTime")]
		public static extern uint MM_GetTime();

		public LauncherForm()
		{
			this.InitializeComponent();
		}

		private void LauncherForm_Load(object sender, EventArgs e)
		{
			if (!File.Exists("DriftCity.exe"))
			{
				MessageBox.Show("Cannot find Drift City.\r\nPlease make sure Rice Launcher is in the Drift City folder.");
				Application.Exit();
			}
			if (File.Exists("riceconfig.json"))
			{
				ServerEntry[] items = JsonConvert.DeserializeObject<ServerEntry[]>(File.ReadAllText("riceconfig.json"));
				this.serverListBox.Items.Clear();
				this.serverListBox.Items.AddRange(items);
			}
			else
			{
				this.serverListBox.Items.Clear();
				this.serverListBox.Items.Add(new ServerEntry
				{
					IP = "127.0.0.1",
					Name = "Local Test"
				});
			}
			if (this.serverListBox.Items.Count > 0)
			{
				this.serverListBox.SelectedIndex = 0;
			}
			this.setStatus("Idle");
		}

		private void setStatus(string msg)
		{
			this.statusLabel.Text = "Status: " + msg;
		}

		private string getRunSw()
		{
			uint num = LauncherForm.MM_GetTime();
			string s = (num - num % 300000u).ToString();
			byte[] value = MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(s));
			return BitConverter.ToString(value).Replace("-", "").ToLower();
		}

		private void launchButton_Click(object sender, EventArgs e)
		{
			if (this.serverListBox.SelectedItem == null)
			{
				MessageBox.Show("Please select a server.");
				return;
			}
			ServerEntry serverEntry = this.serverListBox.SelectedItem as ServerEntry;
			File.WriteAllText("rice.server", serverEntry.IP);
			ProcessStartInfo startInfo = new ProcessStartInfo
			{
				FileName = "DriftCity.exe",
				Arguments = string.Format("/dev /runsw {0}", this.getRunSw())
			};
			this.setStatus("Waiting for game..");
			Process.Start(startInfo);
			this.inject();
		}

		private void inject()
		{
			Process[] processesByName;
			do
			{
				Thread.Sleep(1);
				processesByName = Process.GetProcessesByName("driftcity");
			}
			while (processesByName.Length == 0);
			InjectionMethod injectionMethod = InjectionMethod.Create(InjectionMethodType.ManualMap);
			IntPtr value = IntPtr.Zero;
			using (PortableExecutable portableExecutable = new PortableExecutable(Resources.Rice))
			{
				value = injectionMethod.Inject(portableExecutable, processesByName[0].Id);
			}
			if (value != IntPtr.Zero)
			{
				this.setStatus("Injected Rice");
				return;
			}
			this.setStatus("Failed to inject Rice");
		}

		private void addButton_Click(object sender, EventArgs e)
		{
			if (this.ipBox.Text.Length < 7)
			{
				MessageBox.Show("Please enter a valid IP.");
				return;
			}
			string text = this.ipBox.Text;
			string name = (this.nameBox.Text.Length == 0) ? text : this.nameBox.Text;
			ServerEntry item = new ServerEntry
			{
				IP = text,
				Name = name
			};
			this.serverListBox.Items.Add(item);
			this.saveServers();
		}

		private void remButton_Click(object sender, EventArgs e)
		{
			if (this.serverListBox.SelectedItem == null)
			{
				MessageBox.Show("Please select a server.");
				return;
			}
			this.serverListBox.Items.Remove(this.serverListBox.SelectedItem);
			this.saveServers();
		}

		private void saveServers()
		{
			IEnumerable<ServerEntry> value = this.serverListBox.Items.Cast<ServerEntry>();
			string contents = JsonConvert.SerializeObject(value, Formatting.Indented);
			File.WriteAllText("riceconfig.json", contents);
			this.setStatus("Saved Server List!");
		}

		private void toolStripDropDownButton1_Click(object sender, EventArgs e)
		{
		}

		private void perlSavetherobotsToolStripMenuItem_Click(object sender, EventArgs e)
		{
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(LauncherForm));
			this.statusStrip1 = new StatusStrip();
			this.statusLabel = new ToolStripStatusLabel();
			this.toolStripDropDownButton1 = new ToolStripDropDownButton();
			this.perlSavetherobotsToolStripMenuItem = new ToolStripMenuItem();
			this.serverListBox = new ListBox();
			this.launchButton = new Button();
			this.ipBox = new TextBox();
			this.label1 = new Label();
			this.nameBox = new TextBox();
			this.label3 = new Label();
			this.addButton = new Button();
			this.remButton = new Button();
			this.creditsToolStripMenuItem = new ToolStripMenuItem();
			this.statusStrip1.SuspendLayout();
			base.SuspendLayout();
			this.statusStrip1.Items.AddRange(new ToolStripItem[]
			{
				this.statusLabel,
				this.toolStripDropDownButton1
			});
			this.statusStrip1.Location = new Point(0, 149);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Size = new Size(378, 22);
			this.statusStrip1.TabIndex = 0;
			this.statusStrip1.Text = "statusStrip1";
			this.statusLabel.Name = "statusLabel";
			this.statusLabel.Size = new Size(38, 17);
			this.statusLabel.Text = "status";
			this.toolStripDropDownButton1.DisplayStyle = ToolStripItemDisplayStyle.Text;
			this.toolStripDropDownButton1.DropDownItems.AddRange(new ToolStripItem[]
			{
				this.creditsToolStripMenuItem,
				this.perlSavetherobotsToolStripMenuItem
			});
			this.toolStripDropDownButton1.Image = (Image)componentResourceManager.GetObject("toolStripDropDownButton1.Image");
			this.toolStripDropDownButton1.ImageTransparentColor = Color.Magenta;
			this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
			this.toolStripDropDownButton1.Size = new Size(79, 20);
			this.toolStripDropDownButton1.Text = "// About //";
			this.toolStripDropDownButton1.Click += new EventHandler(this.toolStripDropDownButton1_Click);
			this.perlSavetherobotsToolStripMenuItem.Name = "perlSavetherobotsToolStripMenuItem";
			this.perlSavetherobotsToolStripMenuItem.Size = new Size(228, 22);
			this.perlSavetherobotsToolStripMenuItem.Text = "Perl / Savetherobots - Author";
			this.perlSavetherobotsToolStripMenuItem.Click += new EventHandler(this.perlSavetherobotsToolStripMenuItem_Click);
			this.serverListBox.Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right);
			this.serverListBox.FormattingEnabled = true;
			this.serverListBox.Location = new Point(13, 13);
			this.serverListBox.Name = "serverListBox";
			this.serverListBox.Size = new Size(183, 121);
			this.serverListBox.TabIndex = 1;
			this.launchButton.Anchor = (AnchorStyles.Top | AnchorStyles.Right);
			this.launchButton.Font = new Font("Segoe UI Semibold", 15.75f, FontStyle.Bold, GraphicsUnit.Point, 0);
			this.launchButton.Location = new Point(205, 86);
			this.launchButton.Name = "launchButton";
			this.launchButton.Size = new Size(167, 46);
			this.launchButton.TabIndex = 2;
			this.launchButton.Text = "Start Game";
			this.launchButton.UseVisualStyleBackColor = true;
			this.launchButton.Click += new EventHandler(this.launchButton_Click);
			this.ipBox.Anchor = (AnchorStyles.Top | AnchorStyles.Right);
			this.ipBox.Location = new Point(248, 31);
			this.ipBox.Name = "ipBox";
			this.ipBox.Size = new Size(124, 20);
			this.ipBox.TabIndex = 3;
			this.label1.Anchor = (AnchorStyles.Top | AnchorStyles.Right);
			this.label1.AutoSize = true;
			this.label1.Font = new Font("Segoe UI Semibold", 8.25f, FontStyle.Bold, GraphicsUnit.Point, 0);
			this.label1.Location = new Point(223, 34);
			this.label1.Name = "label1";
			this.label1.Size = new Size(19, 13);
			this.label1.TabIndex = 4;
			this.label1.Text = "IP:";
			this.nameBox.Anchor = (AnchorStyles.Top | AnchorStyles.Right);
			this.nameBox.Location = new Point(248, 10);
			this.nameBox.Name = "nameBox";
			this.nameBox.Size = new Size(124, 20);
			this.nameBox.TabIndex = 7;
			this.label3.Anchor = (AnchorStyles.Top | AnchorStyles.Right);
			this.label3.AutoSize = true;
			this.label3.Font = new Font("Segoe UI Semibold", 8.25f, FontStyle.Bold, GraphicsUnit.Point, 0);
			this.label3.Location = new Point(202, 13);
			this.label3.Name = "label3";
			this.label3.Size = new Size(40, 13);
			this.label3.TabIndex = 8;
			this.label3.Text = "Name:";
			this.addButton.Anchor = (AnchorStyles.Top | AnchorStyles.Right);
			this.addButton.Location = new Point(205, 57);
			this.addButton.Name = "addButton";
			this.addButton.Size = new Size(71, 23);
			this.addButton.TabIndex = 9;
			this.addButton.Text = "Add Server";
			this.addButton.UseVisualStyleBackColor = true;
			this.addButton.Click += new EventHandler(this.addButton_Click);
			this.remButton.Anchor = (AnchorStyles.Top | AnchorStyles.Right);
			this.remButton.Location = new Point(282, 57);
			this.remButton.Name = "remButton";
			this.remButton.Size = new Size(90, 23);
			this.remButton.TabIndex = 10;
			this.remButton.Text = "Remove Server";
			this.remButton.UseVisualStyleBackColor = true;
			this.remButton.Click += new EventHandler(this.remButton_Click);
			this.creditsToolStripMenuItem.Name = "creditsToolStripMenuItem";
			this.creditsToolStripMenuItem.Size = new Size(228, 22);
			this.creditsToolStripMenuItem.Text = "Credits:";
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = AutoScaleMode.Font;
			base.ClientSize = new Size(378, 171);
			base.Controls.Add(this.remButton);
			base.Controls.Add(this.addButton);
			base.Controls.Add(this.label3);
			base.Controls.Add(this.nameBox);
			base.Controls.Add(this.label1);
			base.Controls.Add(this.ipBox);
			base.Controls.Add(this.launchButton);
			base.Controls.Add(this.serverListBox);
			base.Controls.Add(this.statusStrip1);
			base.FormBorderStyle = FormBorderStyle.FixedSingle;
			base.Name = "LauncherForm";
			base.ShowIcon = false;
			base.SizeGripStyle = SizeGripStyle.Hide;
			this.Text = "Rice Launcher";
			base.Load += new EventHandler(this.LauncherForm_Load);
			this.statusStrip1.ResumeLayout(false);
			this.statusStrip1.PerformLayout();
			base.ResumeLayout(false);
			base.PerformLayout();
		}
	}
}
