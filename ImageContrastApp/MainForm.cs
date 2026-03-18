using System;
using System.Drawing;
using System.Windows.Forms;

namespace ImageContrastApp;

public sealed partial class MainForm : Form
{
    private readonly Button btnLoadImage;
    private readonly Button btnApplyContrast;
    private readonly Button btnSaveImage;
    private readonly NumericUpDown numContrastFactor;
    private readonly PictureBox pictureBox;

    private Bitmap? originalImage;
    private Bitmap? displayedImage;

    public MainForm()
    {
        Text = "Image Contrast Processor";
        Width = 1000;
        Height = 700;
        StartPosition = FormStartPosition.CenterScreen;

        var topPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 64,
            Padding = new Padding(12)
        };

        btnLoadImage = new Button
        {
            Text = "Load",
            Width = 120,
            Height = 32,
            Left = 12,
            Top = 14
        };
        btnLoadImage.Click += btnLoadImage_Click;

        btnApplyContrast = new Button
        {
            Text = "Apply Contrast",
            Width = 140,
            Height = 32,
            Left = 144,
            Top = 14
        };
        btnApplyContrast.Click += btnApplyContrast_Click;

        btnSaveImage = new Button
        {
            Text = "Save",
            Width = 120,
            Height = 32,
            Left = 296,
            Top = 14
        };
        btnSaveImage.Click += btnSaveImage_Click;

        var lblContrast = new Label
        {
            Text = "k (Global CC):",
            AutoSize = true,
            Left = 438,
            Top = 21
        };

        numContrastFactor = new NumericUpDown
        {
            DecimalPlaces = 2,
            Increment = 0.10m,
            Minimum = -0.99m,
            Maximum = 3.00m,
            Value = 0.20m,
            Width = 80,
            Left = 490,
            Top = 17
        };

        topPanel.Controls.Add(btnLoadImage);
        topPanel.Controls.Add(btnApplyContrast);
        topPanel.Controls.Add(btnSaveImage);
        topPanel.Controls.Add(lblContrast);
        topPanel.Controls.Add(numContrastFactor);

        pictureBox = new PictureBox
        {
            Dock = DockStyle.Fill,
            SizeMode = PictureBoxSizeMode.Zoom,
            BorderStyle = BorderStyle.FixedSingle
        };

        Controls.Add(pictureBox);
        Controls.Add(topPanel);
    }
}

