using Renci.SshNet;
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

namespace Remarkable_Editor
{
    public partial class Editor : Form
    {
        private SshClient sshClient;
        private SftpClient sftpClient;
        public Editor()
        {
            InitializeComponent();
            
        }

        public Editor(SshClient sshClient) : this()
        {
            this.sshClient = sshClient;
            sftpClient = new SftpClient(sshClient.ConnectionInfo);

            // Connect to the SFTP server
            sftpClient.Connect();

            // Download the images from the device
            DownloadImages();
        }

        private void Editor_Load(object sender, EventArgs e)
        {
            LoadImages();
        }

        private void DownloadImages()
        {
            var remoteImagePaths = new[] {
            "/usr/share/remarkable/rebooting.png",
            "/usr/share/remarkable/batteryempty.png",
            "/usr/share/remarkable/suspended.png",
            "/usr/share/remarkable/poweroff.png"
            };

            string subfolderPath = Path.Combine(Application.StartupPath, "Screen Saves");

            foreach (var remotePath in remoteImagePaths)
            {
                string localPath = Path.Combine(subfolderPath, Path.GetFileName(remotePath));
                using (Stream fileStream = File.OpenWrite(localPath))
                {
                    sftpClient.DownloadFile(remotePath, fileStream);
                }
            }

            sftpClient.Disconnect();
        }

        private void LoadImages()
        {
            string subfolderPath = Path.Combine(Application.StartupPath, "Screen Saves");
            // Paths to the images you've downloaded
            string rebootingImage = Path.Combine(subfolderPath, "rebooting.png");
            string batteryEmptyImage = Path.Combine(subfolderPath, "batteryempty.png");
            string suspendedImage = Path.Combine(subfolderPath, "suspended.png");
            string powerOffImage = Path.Combine(subfolderPath, "poweroff.png");

            // Set the images in the PictureBox controls
            if (File.Exists(rebootingImage)) pictureBox1.Image = Image.FromFile(rebootingImage);
            if (File.Exists(batteryEmptyImage)) pictureBox2.Image = Image.FromFile(batteryEmptyImage);
            if (File.Exists(suspendedImage)) pictureBox3.Image = Image.FromFile(suspendedImage);
            if (File.Exists(powerOffImage)) pictureBox4.Image = Image.FromFile(powerOffImage);
        }

        private void Editor_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }
    }
}
