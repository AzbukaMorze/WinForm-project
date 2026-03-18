using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ImageContrastApp;

public sealed partial class MainForm : Form
{
    private readonly Panel topPanel;
    private readonly Panel topDivider;
    private readonly FlowLayoutPanel actionRow;
    private readonly FlowLayoutPanel paramsRow;
    private readonly Panel imageCanvas;
    private readonly Panel imageFrame;

    private readonly Button btnLoadImage;
    private readonly Button btnApplyContrast;
    private readonly Button btnSaveImage;
    private readonly ComboBox cmbTheme;
    private readonly NumericUpDown numContrastFactor;
    private readonly Label lblContrast;
    private readonly Label lblTheme;
    private readonly PictureBox pictureBox;
    private readonly Dictionary<Button, Color> buttonBaseColors;

    private Bitmap? originalImage;
    private Bitmap? displayedImage;

    public MainForm()
    {
        Text = "Image Contrast Processor";
        Width = 1000;
        Height = 700;
        MinimumSize = new Size(900, 620);
        StartPosition = FormStartPosition.CenterScreen;

        topPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 128,
            Padding = new Padding(14)
        };

        topDivider = new Panel
        {
            Dock = DockStyle.Top,
            Height = 1
        };

        actionRow = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 42,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = Color.Transparent,
            Margin = Padding.Empty
        };

        btnLoadImage = new Button
        {
            Text = "Load",
            Width = 118,
            Height = 34,
            Margin = new Padding(0, 0, 10, 0)
        };
        btnLoadImage.Click += btnLoadImage_Click;

        btnApplyContrast = new Button
        {
            Text = "Apply Contrast",
            Width = 148,
            Height = 34,
            Margin = new Padding(0, 0, 10, 0)
        };
        btnApplyContrast.Click += btnApplyContrast_Click;

        btnSaveImage = new Button
        {
            Text = "Save",
            Width = 118,
            Height = 34,
            Margin = new Padding(0, 0, 20, 0)
        };
        btnSaveImage.Click += btnSaveImage_Click;

        lblTheme = new Label
        {
            Text = "Theme:",
            AutoSize = true,
            Margin = new Padding(0, 9, 8, 0)
        };

        cmbTheme = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 110,
            Height = 34,
            Margin = new Padding(0, 4, 0, 0),
            FlatStyle = FlatStyle.Flat
        };
        cmbTheme.Items.Add("Light");
        cmbTheme.Items.Add("Dark");
        cmbTheme.SelectedIndex = 0;
        cmbTheme.SelectedIndexChanged += (_, _) => ApplyTheme((UiTheme)cmbTheme.SelectedIndex);

        actionRow.Controls.Add(btnLoadImage);
        actionRow.Controls.Add(btnApplyContrast);
        actionRow.Controls.Add(btnSaveImage);
        actionRow.Controls.Add(lblTheme);
        actionRow.Controls.Add(cmbTheme);

        paramsRow = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 44,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = Color.Transparent,
            Margin = new Padding(0, 8, 0, 0)
        };

        lblContrast = new Label
        {
            Text = "k (Global CC):",
            AutoSize = true,
            Margin = new Padding(0, 12, 8, 0)
        };

        numContrastFactor = new NumericUpDown
        {
            DecimalPlaces = 2,
            Increment = 0.10m,
            Minimum = -0.99m,
            Maximum = 3.00m,
            Value = 0.20m,
            Width = 80,
            Height = 30,
            Margin = new Padding(0, 8, 18, 0)
        };

        paramsRow.Controls.Add(lblContrast);
        paramsRow.Controls.Add(numContrastFactor);

        topPanel.Controls.Add(paramsRow);
        topPanel.Controls.Add(actionRow);

        imageCanvas = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(10)
        };

        imageFrame = new Panel();
        imageCanvas.Controls.Add(imageFrame);

        pictureBox = new PictureBox
        {
            Dock = DockStyle.Fill,
            SizeMode = PictureBoxSizeMode.Zoom
        };
        imageFrame.Controls.Add(pictureBox);

        Controls.Add(imageCanvas);
        Controls.Add(topDivider);
        Controls.Add(topPanel);

        buttonBaseColors = new Dictionary<Button, Color>(3);

        StyleActionButton(btnLoadImage);
        StyleActionButton(btnApplyContrast);
        StyleActionButton(btnSaveImage);

        imageCanvas.Resize += (_, _) => UpdateImageViewportBounds();
        imageFrame.Resize += (_, _) => ApplyRoundedCorners(imageFrame, 16);

        UpdateImageViewportBounds();
        ApplyRoundedCorners(imageFrame, 16);
        ApplyTheme(UiTheme.Light);
    }
}
