using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Renci.SshNet;


namespace Remarkable_Editor
{
    public partial class Form1 : Form
    {
        private SshClient sshClient;
        public Form1()
        {
            InitializeComponent();
            button1.Focus();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadSettings();
            button1.Select();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string ipAddress = textBox1.Text;
            string username = textBox2.Text;
            string password = textBox3.Text;

            try
            {
                sshClient = new SshClient(ipAddress, username, password);
                sshClient.Connect();

                // Attempt to run a basic command that requires authentication
                var command = sshClient.RunCommand("echo test");
                if (command.ExitStatus == 0)
                {
                    this.Hide();
                    MessageBox.Show("Connection Successful!");
                    SaveSettings();
                    // Move to the next window after connection
                    var nextWindow = new Editor(sshClient);
                    nextWindow.Show();
                    
                }
                else
                {
                    MessageBox.Show("Authentication failed. Please check your password.");
                    sshClient.Disconnect();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Connection failed: " + ex.Message);
            }
        }

        private void SaveSettings()
        {
            if (checkBox1.Checked)
            {
                Properties.Settings.Default.SaveLogin = true;
                Properties.Settings.Default.Username = textBox1.Text;
                Properties.Settings.Default.Password = textBox3.Text;  
            }
            else
            {
                Properties.Settings.Default.SaveLogin = false;
                Properties.Settings.Default.Username = string.Empty;
                Properties.Settings.Default.Password = string.Empty;
            }

            // Save the settings to disk
            Properties.Settings.Default.Save();
        }

        private void Form1_Closing(object sender, FormClosingEventArgs e)
        {
            SaveSettings();
        }

        private void LoadSettings()
        {
            if (Properties.Settings.Default.SaveLogin)
            {
                textBox1.Text = Properties.Settings.Default.Username;
                textBox3.Text = Properties.Settings.Default.Password;
                checkBox1.Checked = true;
            }
            else
            {
                checkBox1.Checked = false;
            }
        }


    }
}
