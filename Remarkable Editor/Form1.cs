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
using System.Drawing.Drawing2D;


namespace Remarkable_Editor
{
    public partial class Form1 : Form
    {
        private SshClient sshClient;

        private bool dragging = false;
        private Point dragCursorPoint;
        private Point dragFormPoint;

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

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Create a GraphicsPath to define the rounded corners
            GraphicsPath path = new GraphicsPath();
            int radius = 20;  // Adjust the radius as per your need
            Rectangle bounds = new Rectangle(0, 0, this.Width, this.Height);
            path.AddArc(bounds.X, bounds.Y, radius, radius, 180, 90);
            path.AddArc(bounds.X + bounds.Width - radius, bounds.Y, radius, radius, 270, 90);
            path.AddArc(bounds.X + bounds.Width - radius, bounds.Y + bounds.Height - radius, radius, radius, 0, 90);
            path.AddArc(bounds.X, bounds.Y + bounds.Height - radius, radius, radius, 90, 90);
            path.CloseAllFigures();

            // Set the form's region to the defined path (rounded corners)
            this.Region = new Region(path);
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

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            dragging = true;
            dragCursorPoint = Cursor.Position;
            dragFormPoint = this.Location;
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                Point diff = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
                this.Location = Point.Add(dragFormPoint, new Size(diff));
            }
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
