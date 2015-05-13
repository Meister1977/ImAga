using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImAga
{
    public partial class Form1 : Form
    {
        private Timer timer1 = new Timer();
        public TimeSpan TimeoutToHide { get; private set; }
        public DateTime LastMouseMove { get; private set; }
        public bool IsHidden { get; private set; }
        private FileSystemWatcher watcher = new FileSystemWatcher();

        public Form1()
        {
            InitializeComponent();
            TimeoutToHide = TimeSpan.FromSeconds(3);
            this.MouseMove += new MouseEventHandler(Form1_MouseMove);
            timer1.Interval = 500;
            timer1.Tick += timer1_Tick;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.TopMost = true;
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            //this.Bounds = Screen.PrimaryScreen.Bounds;
            

            string dir = RegistryHelper.Read("Path");
            if (String.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir))
                dir = SelectDir(true);

            if (!String.IsNullOrWhiteSpace(dir) && Directory.Exists(dir))
            {
                GetFirstImage(dir);
                StartWatching(dir);
                timer1.Start();
            }
        }
        private void GetFirstImage(string dir)
        {
            var directory = new DirectoryInfo(dir);
            var imageFiles = directory.GetFiles("*.jpg")
                .OrderByDescending(f => f.LastWriteTime);

            if (imageFiles.Any())
                pictureBox1.ImageLocation = imageFiles.First().FullName;
        }

        private void StartWatching(string dir)
        {
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
            watcher = new FileSystemWatcher();

            // Create a new FileSystemWatcher and set its properties.
            watcher.Path = dir;
            /* Watch for changes in LastAccess and LastWrite times, and
               the renaming of files or directories. */
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            // Only watch text files.
            watcher.Filter = "*.jpg";

            // Add event handlers.
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            // Begin watching.
            watcher.EnableRaisingEvents = true;
        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            bool loaded = false;
            do
            {
                try
                {
                    pictureBox1.ImageLocation = e.FullPath;
                    loaded = true;
                }
                catch (System.IO.IOException)
                {
                    ;
                }
            } while (!loaded);

        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            Form1_MouseMove(null, null);
            this.Close();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Form1_MouseMove(null, null);
            SelectDir(false);
        }
        private string SelectDir(bool isStart)
        {
            timer1.Stop();
            if (IsHidden)
            {
                Cursor.Show();
                IsHidden = false;
            }
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            
            string dir = RegistryHelper.Read("Path");
            if (!String.IsNullOrWhiteSpace(dir) && Directory.Exists(dir))
                fbd.SelectedPath = dir;
            
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                dir = fbd.SelectedPath;
                RegistryHelper.Write("Path", dir);
            }
            if (!isStart)
            {
                GetFirstImage(dir);
                StartWatching(dir);
                timer1.Start();
            }
            return dir;
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            LastMouseMove = DateTime.Now;

            if (IsHidden)
            {
                Cursor.Show();
                IsHidden = false;
            }
        } 

        private void timer1_Tick(object sender, EventArgs e)
        {
            TimeSpan elaped = DateTime.Now - LastMouseMove;
            if (elaped >= TimeoutToHide && !IsHidden)
            {
                Cursor.Hide();
                IsHidden = true;
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Form1_MouseMove(null, null);
            contextMenuStrip1.Show();
            contextMenuStrip1.Top = Cursor.Position.Y;
            contextMenuStrip1.Left = Cursor.Position.X;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Form1_MouseMove(null, null);
                this.Close();
            }
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            if (toolStripMenuItem3.Text == "Ablakba")
            {
                this.TopMost = false;
                this.FormBorderStyle = FormBorderStyle.Sizable;
                this.WindowState = FormWindowState.Normal;
                toolStripMenuItem3.Text = "Teljes képernyőre";
            }
            else
            {
                this.TopMost = true;
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Maximized;
                toolStripMenuItem3.Text = "Ablakba";
            }
        }
    }
}
