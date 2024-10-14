using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public class RoundedGroupBox : GroupBox
{
    protected override void OnPaint(PaintEventArgs e)
    {
        // Create a GraphicsPath object for the rounded rectangle
        GraphicsPath path = new GraphicsPath();
        int radius = 20; // Adjust the radius as needed
        Rectangle bounds = this.ClientRectangle;

        path.AddArc(bounds.X, bounds.Y, radius, radius, 180, 90); // Top-left corner
        path.AddArc(bounds.X + bounds.Width - radius, bounds.Y, radius, radius, 270, 90); // Top-right corner
        path.AddArc(bounds.X + bounds.Width - radius, bounds.Y + bounds.Height - radius, radius, radius, 0, 90); // Bottom-right corner
        path.AddArc(bounds.X, bounds.Y + bounds.Height - radius, radius, radius, 90, 90); // Bottom-left corner
        path.CloseAllFigures();

        // Set smoothing mode for better appearance
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        // Draw the background
        using (Brush brush = new SolidBrush(this.BackColor))
        {
            e.Graphics.FillPath(brush, path);
        }

        // Draw the border
        using (Pen pen = new Pen(this.ForeColor, 1))
        {
            e.Graphics.DrawPath(pen, path);
        }

        // Draw the text
        TextRenderer.DrawText(e.Graphics, this.Text, this.Font, bounds, this.ForeColor, TextFormatFlags.Top | TextFormatFlags.Left);

        // Skip the default painting behavior to prevent it from overwriting
    }
}
