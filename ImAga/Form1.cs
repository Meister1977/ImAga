using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ImAga
{
    public partial class Form1 : Form
    {
        private readonly Timer _timer1 = new Timer();
        public TimeSpan TimeoutToHide { get; }
        public DateTime LastMouseMove { get; private set; }
        public bool IsHidden { get; private set; }
        private FileSystemWatcher _watcher = new FileSystemWatcher();

        public Form1()
        {
            InitializeComponent();
            TimeoutToHide = TimeSpan.FromSeconds(3);
            MouseMove += Form1_MouseMove;
            _timer1.Interval = 500;
            _timer1.Tick += _timer1_Tick;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            TopMost = true;
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            //this.Bounds = Screen.PrimaryScreen.Bounds;
            

            string dir = RegistryHelper.Read("Path");
            if (string.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir))
                dir = SelectDir(true);

            if (string.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir))
                return;
            GetFirstImage(dir);
            StartWatching(dir);
            _timer1.Start();
        }
        private void GetFirstImage(string dir)
        {
            var directory = new DirectoryInfo(dir);
            var imageFiles = directory.GetFiles("*.jpg")
                .OrderByDescending(f => f.LastWriteTime);

            if (imageFiles.Any())
                pictureBox1.Image = ImageHandler.Rotate(imageFiles.First().FullName);
        }

        private void StartWatching(string dir)
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Dispose();
            // Create a new FileSystemWatcher and set its properties.
            // Watch for changes in LastAccess and LastWrite times, and the renaming of files or directories.
            // Only watch jpg
            _watcher = new FileSystemWatcher
                       {
                           Path = dir,
                           NotifyFilter = NotifyFilters.LastWrite,
                           Filter = "*.jpg"
                       };

            // Add event handlers.
            _watcher.Changed += OnChanged;
            // Begin watching.
            _watcher.EnableRaisingEvents = true;
        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            pictureBox1.Image = ImageHandler.Rotate(e.FullPath);
        }

        private void _toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            Form1_MouseMove(null, null);
            Close();
        }

        private void _toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Form1_MouseMove(null, null);
            SelectDir(false);
        }
        private string SelectDir(bool isStart)
        {
            _timer1.Stop();
            if (IsHidden)
            {
                Cursor.Show();
                IsHidden = false;
            }
            string dir;
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                dir = RegistryHelper.Read("Path");
                if (!string.IsNullOrWhiteSpace(dir) && Directory.Exists(dir))
                    fbd.SelectedPath = dir;
            
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    dir = fbd.SelectedPath;
                    RegistryHelper.Write("Path", dir);
                }
            }
            if (isStart)
                return dir;

            GetFirstImage(dir);
            StartWatching(dir);
            _timer1.Start();
            return dir;
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            LastMouseMove = DateTime.Now;

            if (!IsHidden)
                return;
            Cursor.Show();
            IsHidden = false;
        } 

        private void _timer1_Tick(object sender, EventArgs e)
        {
            TimeSpan elaped = DateTime.Now - LastMouseMove;

            if (elaped < TimeoutToHide || IsHidden)
                return;
            Cursor.Hide();
            IsHidden = true;
        }

        private void _pictureBox1_Click(object sender, EventArgs e)
        {
            Form1_MouseMove(null, null);
            contextMenuStrip1.Show();
            contextMenuStrip1.Top = Cursor.Position.Y;
            contextMenuStrip1.Left = Cursor.Position.X;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Escape)
                return;
            Form1_MouseMove(null, null);
            Close();
        }

        private void _toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            Form1_MouseMove(null, null);
            if (toolStripMenuItem3.Text == "Ablakba")
            {
                TopMost = false;
                FormBorderStyle = FormBorderStyle.Sizable;
                WindowState = FormWindowState.Normal;
                toolStripMenuItem3.Text = "Teljes képernyőre";
            }
            else
            {
                TopMost = true;
                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Maximized;
                toolStripMenuItem3.Text = "Ablakba";
            }
        }

        private void _pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            Form1_MouseMove(null, null);
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_watcher != null)
            {
                _watcher.EnableRaisingEvents = false;
                _watcher.Dispose();
            }
            if (_timer1 != null)
            {
                _timer1.Stop();
                _timer1.Dispose();
            }
        }
    }
}
