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

            string remarkableEditorPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "RemarkableEditor");
            string backupPath = Path.Combine(remarkableEditorPath, "Backup");
            if (Directory.Exists(backupPath))
            {
                button3.Enabled = true;
            }

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

        private void UploadImage(PictureBox pictureBox, string remoteFileName, ProgressBar progressBar)
        {
            if (pictureBox.Tag == null)
            {
                MessageBox.Show("No image selected for upload.");
                return;
            }

            string localFilePath = pictureBox.Tag.ToString(); // Path to the selected image
            string remoteFolderPath = "/usr/share/remarkable/"; // Remote directory path
            string remoteFilePath = remoteFolderPath + remoteFileName;

            // Release any image resources in the PictureBox
            if (pictureBox.Image != null)
            {
                pictureBox.Image.Dispose();
                pictureBox.Image = null;
            }

            // Ensure the progress bar is visible and reset it
            progressBar.Visible = true;
            progressBar.Value = 0;
            progressBar.Minimum = 0;
            progressBar.Maximum = 100;

            try
            {
                // Upload the file using SFTP
                using (var fileStream = new FileStream(localFilePath, FileMode.Open, FileAccess.Read))
                {
                    long fileSize = fileStream.Length;
                    long uploadedBytes = 0;

                    using (var sftpClient = new SftpClient(sshClient.ConnectionInfo)) // Use your sshClient instance
                    {
                        sftpClient.Connect();

                        // Upload with progress tracking
                        sftpClient.UploadFile(fileStream, remoteFilePath, (uploaded) =>
                        {
                            uploadedBytes += (long)uploaded;
                            int progressPercentage = (int)((double)uploadedBytes / fileSize * 100);

                            // Ensure the progressPercentage does not exceed 100
                            progressPercentage = Math.Min(progressPercentage, progressBar.Maximum);

                            // Use Invoke to update the ProgressBar from the UI thread
                            progressBar.Invoke((MethodInvoker)(() => progressBar.Value = progressPercentage));
                        });

                        sftpClient.Disconnect();
                    }
                }

                // Restore the image after the upload is complete
                using (var fs = new FileStream(localFilePath, FileMode.Open, FileAccess.Read))
                {
                    pictureBox.Invoke((MethodInvoker)(() => pictureBox.Image = Image.FromStream(fs)));
                }

                MessageBox.Show($"Successfully uploaded {remoteFileName}!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error uploading {remoteFileName}: {ex.Message}");
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
            PictureBox pictureBox = sender as PictureBox;

            if (pictureBox.Image != null && pictureBox.Tag != null)
            {
                string imagePath = pictureBox2.Tag.ToString();

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

        private void pictureBox3_DoubleClick(object sender, EventArgs e)
        {
            PictureBox pictureBox = sender as PictureBox;

            if (pictureBox.Image != null && pictureBox.Tag != null)
            {
                string imagePath = pictureBox3.Tag.ToString();

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

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            string SavedFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "RemarkableEditor", "Screen Saves");

            if (checkBox.Checked)
            {
                // Show file selection dialog
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                openFileDialog.InitialDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "RemarkableEditor"); // Set default directory to Custom Screens


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
                        if (!selectedFilePath.Equals(destinationPath, StringComparison.OrdinalIgnoreCase))
                        {
                            File.Copy(selectedFilePath, destinationPath, true); // Set true to overwrite if file exists
                            MessageBox.Show($"Image saved to {destinationPath}");
                        }
                        
                        if (File.Exists(openFileDialog.FileName))
                        {
                            pictureBox1.Image = Image.FromFile(openFileDialog.FileName);
                            pictureBox1.Tag = openFileDialog.FileName;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error copying file: {ex.Message}");
                        checkBox.Checked = false;
                    }
                }
                else
                {
                    // If no file was selected, uncheck the checkbox
                    checkBox.Checked = false;
                }
            }

            if (!checkBox.Checked)
            {
                string rebootingImage = Path.Combine(SavedFolderPath, "rebooting.png");
                if (File.Exists(rebootingImage)) pictureBox1.Image = Image.FromFile(rebootingImage);
                progressBar1.Visible = false;
                progressBar1.Value = 0;
                
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            string SavedFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "RemarkableEditor", "Screen Saves");

            if (checkBox.Checked)
            {
                // Show file selection dialog
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                openFileDialog.InitialDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "RemarkableEditor"); // Set default directory to Custom Screens

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
                        if (!selectedFilePath.Equals(destinationPath, StringComparison.OrdinalIgnoreCase))
                        {
                            File.Copy(selectedFilePath, destinationPath, true); // Set true to overwrite if file exists
                            MessageBox.Show($"Image saved to {destinationPath}");
                        }

                        if (File.Exists(openFileDialog.FileName))
                        {
                            pictureBox2.Image = Image.FromFile(openFileDialog.FileName);
                            pictureBox2.Tag = openFileDialog.FileName;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error copying file: {ex.Message}");
                        checkBox.Checked = false;
                    }
                }
                else
                {
                    // If no file was selected, uncheck the checkbox
                    checkBox.Checked = false;
                }
            }

            if (!checkBox.Checked)
            {
                string batteryemptyImage = Path.Combine(SavedFolderPath, "batteryempty.png");
                if (File.Exists(batteryemptyImage)) pictureBox2.Image = Image.FromFile(batteryemptyImage);
                progressBar2.Visible = false;
                progressBar2.Value = 0;

            }
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            string SavedFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "RemarkableEditor", "Screen Saves");

            if (checkBox.Checked)
            {
                // Show file selection dialog
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                openFileDialog.InitialDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "RemarkableEditor"); // Set default directory to Custom Screens

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
                        if (!selectedFilePath.Equals(destinationPath, StringComparison.OrdinalIgnoreCase))
                        {
                            File.Copy(selectedFilePath, destinationPath, true); // Set true to overwrite if file exists
                            MessageBox.Show($"Image saved to {destinationPath}");
                        }

                        if (File.Exists(openFileDialog.FileName))
                        {
                            pictureBox3.Image = Image.FromFile(openFileDialog.FileName);
                            pictureBox3.Tag = openFileDialog.FileName;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error copying file: {ex.Message}");
                        checkBox.Checked = false;
                    }
                }
                else
                {
                    // If no file was selected, uncheck the checkbox
                    checkBox.Checked = false;
                }
            }

            if (!checkBox.Checked)
            {
                string suspendImage = Path.Combine(SavedFolderPath, "suspended.png");
                if (File.Exists(suspendImage)) pictureBox3.Image = Image.FromFile(suspendImage);
                progressBar3.Visible = false;
                progressBar3.Value = 0;

            }
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            string SavedFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "RemarkableEditor", "Screen Saves");

            if (checkBox.Checked)
            {
                // Show file selection dialog
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                openFileDialog.InitialDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "RemarkableEditor"); // Set default directory to Custom Screens

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
                        if (!selectedFilePath.Equals(destinationPath, StringComparison.OrdinalIgnoreCase))
                        {
                            File.Copy(selectedFilePath, destinationPath, true); // Set true to overwrite if file exists
                            MessageBox.Show($"Image saved to {destinationPath}");
                        }

                        if (File.Exists(openFileDialog.FileName))
                        {
                            pictureBox4.Image = Image.FromFile(openFileDialog.FileName);
                            pictureBox4.Tag = openFileDialog.FileName;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error copying file: {ex.Message}");
                        checkBox.Checked = false;
                    }
                }
                else
                {
                    // If no file was selected, uncheck the checkbox
                    checkBox.Checked = false;
                }
            }

            if (!checkBox.Checked)
            {
                string poweroffImage = Path.Combine(SavedFolderPath, "poweroff.png");
                if (File.Exists(poweroffImage)) pictureBox4.Image = Image.FromFile(poweroffImage);
                progressBar4.Visible = false;
                progressBar4.Value = 0;

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Check each checkbox and upload the corresponding image if selected
            if (checkBox1.Checked)
            {
                UploadImage(pictureBox1, "rebooting.png", progressBar1);
            }

            if (checkBox2.Checked)
            {
                UploadImage(pictureBox2, "batteryempty.png", progressBar2);
            }

            if (checkBox3.Checked)
            {
                UploadImage(pictureBox3, "suspended.png", progressBar3);
            }

            if (checkBox4.Checked)
            {
                UploadImage(pictureBox4, "poweroff.png", progressBar4);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Define paths
            string remarkableEditorPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "RemarkableEditor");
            string screenSavesPath = Path.Combine(remarkableEditorPath, "Screen Saves");
            string backupPath = Path.Combine(remarkableEditorPath, "Backup");

            // Check if Backup folder exists, if not, create it
            if (!Directory.Exists(backupPath))
            {
                Directory.CreateDirectory(backupPath);
            }

            // Define the names for the images to be backed up
            var imageNames = new Dictionary<string, string>
    {
        { "rebooting.png", Path.Combine(screenSavesPath, "rebooting.png") },
        { "batteryempty.png", Path.Combine(screenSavesPath, "batteryempty.png") },
        { "suspended.png", Path.Combine(screenSavesPath, "suspended.png") },
        { "poweroff.png", Path.Combine(screenSavesPath, "poweroff.png") }
    };

            // Backup images from "Screen Saves" to "Backup" folder
            foreach (var imageName in imageNames)
            {
                string sourcePath = imageName.Value; // Image in "Screen Saves"
                string destinationPath = Path.Combine(backupPath, imageName.Key); // Destination in "Backup"

                if (File.Exists(sourcePath))
                {
                    File.Copy(sourcePath, destinationPath, true); // Overwrite if already exists
                }
            }

            MessageBox.Show("Backup complete!");
            button3.Enabled = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Define paths
            string remarkableEditorPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "RemarkableEditor");
            string backupPath = Path.Combine(remarkableEditorPath, "Backup");

            // Check if Backup folder exists
            if (!Directory.Exists(backupPath))
            {
                MessageBox.Show("No backup folder found");
                return;
            }

            // Temporarily remove event handlers to prevent prompts
            checkBox1.CheckedChanged -= checkBox1_CheckedChanged;
            checkBox2.CheckedChanged -= checkBox2_CheckedChanged;
            checkBox3.CheckedChanged -= checkBox3_CheckedChanged;
            checkBox4.CheckedChanged -= checkBox4_CheckedChanged;

            // Automatically check all the checkboxes without triggering events
            checkBox1.Checked = true;
            checkBox2.Checked = true;
            checkBox3.Checked = true;
            checkBox4.Checked = true;

            // Reattach the event handlers after checking the boxes
            checkBox1.CheckedChanged += checkBox1_CheckedChanged;
            checkBox2.CheckedChanged += checkBox2_CheckedChanged;
            checkBox3.CheckedChanged += checkBox3_CheckedChanged;
            checkBox4.CheckedChanged += checkBox4_CheckedChanged;


            // Define image file names and corresponding PictureBoxes
            var imageNamesToPictureBoxes = new Dictionary<string, PictureBox>
    {
        { "rebooting.png", pictureBox1 },
        { "batteryempty.png", pictureBox2 },
        { "suspended.png", pictureBox3 },
        { "poweroff.png", pictureBox4 }
    };

            // Automatically check all the checkboxes
            checkBox1.Checked = true;
            checkBox2.Checked = true;
            checkBox3.Checked = true;
            checkBox4.Checked = true;

            // Restore images from "Backup" folder to the PictureBoxes
            foreach (var imageEntry in imageNamesToPictureBoxes)
            {
                string imagePath = Path.Combine(backupPath, imageEntry.Key);

                if (File.Exists(imagePath))
                {
                    using (var fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                    {
                        imageEntry.Value.Image = Image.FromStream(fs); // Load the image into the PictureBox
                        imageEntry.Value.Tag = imagePath; // Set the path for future upload
                    }
                }
            }

            MessageBox.Show("Restore complete! You can now upload the restored images.");
        }

        
    }
}
