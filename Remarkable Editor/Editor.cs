using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
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
            string documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string subfolderPath = Path.Combine(documentsFolder, "RemarkableEditor");
            string ScreenSaves = Path.Combine(subfolderPath, "Screen Saves");

            // Ensure the subfolder exists
            if (!Directory.Exists(subfolderPath))
            {
                Directory.CreateDirectory(subfolderPath);
                if (!Directory.Exists(ScreenSaves))
                {
                    Directory.CreateDirectory(ScreenSaves);
                }
            }

            var remoteImagePaths = new[]
            {
        "/usr/share/remarkable/rebooting.png",
        "/usr/share/remarkable/batteryempty.png",
        "/usr/share/remarkable/suspended.png",
        "/usr/share/remarkable/poweroff.png"
            };

            foreach (var remotePath in remoteImagePaths)
            {
                string localPath = Path.Combine(ScreenSaves, Path.GetFileName(remotePath)); // Store in the subfolder

                using (Stream fileStream = File.OpenWrite(localPath))
                {
                    sftpClient.DownloadFile(remotePath, fileStream);
                }
            }

            sftpClient.Disconnect();
        }

        private void LoadImages()
        {
            string documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string subfolderPath = Path.Combine(documentsFolder, "RemarkableEditor");
            string ScreenSaves = Path.Combine(subfolderPath, "Screen Saves");

            string rebootingImage = Path.Combine(ScreenSaves, "rebooting.png");
            string batteryEmptyImage = Path.Combine(ScreenSaves, "batteryempty.png");
            string suspendedImage = Path.Combine(ScreenSaves, "suspended.png");
            string powerOffImage = Path.Combine(ScreenSaves, "poweroff.png");

            if (File.Exists(rebootingImage)) pictureBox1.Image = Image.FromFile(rebootingImage);
            pictureBox1.Tag = rebootingImage;
            if (File.Exists(batteryEmptyImage)) pictureBox2.Image = Image.FromFile(batteryEmptyImage);
            pictureBox2.Tag = batteryEmptyImage;
            if (File.Exists(suspendedImage)) pictureBox3.Image = Image.FromFile(suspendedImage);
            pictureBox3.Tag = suspendedImage;
            if (File.Exists(powerOffImage)) pictureBox4.Image = Image.FromFile(powerOffImage);
            pictureBox4.Tag = powerOffImage;
        }
        
        private void Editor_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            string SavedFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "RemarkableEditor", "Screen Saves");

            if (checkBox.Checked)
            {
                // Show file selection dialog
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // User selected a file, now copy it
                    string selectedFilePath = openFileDialog.FileName;

                    // Define the custom directory path
                    string customFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "RemarkableEditor", "Custom Screens");
                    

                    // Create the directory if it doesn't exist
                    if (!Directory.Exists(customFolderPath))
                    {
                        Directory.CreateDirectory(customFolderPath);
                    }

                    // Copy the selected image to the custom directory
                    string destinationPath = Path.Combine(customFolderPath, Path.GetFileName(selectedFilePath));

                    try
                    {
                        File.Copy(selectedFilePath, destinationPath, true); // Set true to overwrite if file exists
                        MessageBox.Show($"Image saved to {destinationPath}");
                        if (File.Exists(openFileDialog.FileName))
                        {
                            pictureBox1.Image = Image.FromFile(openFileDialog.FileName);
                            pictureBox1.Tag = openFileDialog.FileName;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error copying file: {ex.Message}");
                    }
                }
                else
                {
                    // If no file was selected, uncheck the checkbox
                    checkBox.Checked = false;
                }
            }

            if(!checkBox.Checked) 
            {
                string rebootingImage = Path.Combine(SavedFolderPath, "rebooting.png");
                if (File.Exists(rebootingImage)) pictureBox1.Image = Image.FromFile(rebootingImage);


            }
        }

        private void pictureBox1_DoubleClick(object sender, EventArgs e)
        {
            PictureBox pictureBox = sender as PictureBox;

            if (pictureBox.Image != null && pictureBox.Tag != null)
            {
                string imagePath = pictureBox1.Tag.ToString();

                if (File.Exists(imagePath))
                {
                    // Open the image using the default image viewer
                    try
                    {
                        Process.Start(new ProcessStartInfo(imagePath) { UseShellExecute = true });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error opening image: {ex.Message}");
                    }
                }
                else
                {
                    MessageBox.Show("Image file not found.");
                }
            }
        }

        private void pictureBox2_DoubleClick(object sender, EventArgs e)
        {

        }

        private void pictureBox3_DoubleClick(object sender, EventArgs e)
        {

        }

        private void pictureBox4_DoubleClick(object sender, EventArgs e)
        {
            PictureBox pictureBox = sender as PictureBox;

            if (pictureBox.Image != null && pictureBox.Tag != null)
            {
                string imagePath = pictureBox4.Tag.ToString();

                if (File.Exists(imagePath))
                {
                    // Open the image using the default image viewer
                    try
                    {
                        Process.Start(new ProcessStartInfo(imagePath) { UseShellExecute = true });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error opening image: {ex.Message}");
                    }
                }
                else
                {
                    MessageBox.Show("Image file not found.");
                }
            }
        }
    }
}
