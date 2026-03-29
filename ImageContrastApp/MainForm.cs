using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ImageContrastApp;

public sealed partial class MainForm : Form
{
    private enum ProcessingMode
    {
        GlobalContrast,
        LocalFragment
    }

    private readonly Panel topPanel;
    private readonly Panel topDivider;
    private readonly FlowLayoutPanel actionRow;
    private readonly FlowLayoutPanel paramsRow;
    private readonly Panel imageCanvas;
    private readonly Panel imageFrame;

    private readonly Button btnLoadImage;
    private readonly Button btnApplyContrast;
    private readonly Button btnSaveImage;
    private readonly ComboBox cmbProcessingMode;
    private readonly ComboBox cmbLocalProcessor;
    private readonly NumericUpDown numContrastFactor;
    private readonly NumericUpDown numFragmentWidth;
    private readonly NumericUpDown numFragmentHeight;
    private readonly NumericUpDown numBlendQ;
    private readonly Label lblContrast;
    private readonly Label lblProcessingMode;
    private readonly Label lblLocalProcessor;
    private readonly Label lblFragmentWidth;
    private readonly Label lblFragmentHeight;
    private readonly Label lblBlendQ;
    private readonly CheckBox chkUseMultithreading;
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
            Height = 172,
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
            Text = "Apply",
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

        actionRow.Controls.Add(btnLoadImage);
        actionRow.Controls.Add(btnApplyContrast);
        actionRow.Controls.Add(btnSaveImage);

        paramsRow = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 44,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = Color.Transparent,
            Margin = new Padding(0, 8, 0, 0)
        };

        lblProcessingMode = new Label
        {
            Text = "Mode:",
            AutoSize = true,
            Margin = new Padding(0, 12, 8, 0)
        };

        cmbProcessingMode = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 150,
            Height = 30,
            Margin = new Padding(0, 7, 18, 0),
            FlatStyle = FlatStyle.Flat
        };
        cmbProcessingMode.Items.Add("Global Contrast");
        cmbProcessingMode.Items.Add("Local Fragment");
        cmbProcessingMode.SelectedIndex = 0;
        cmbProcessingMode.SelectedIndexChanged += (_, _) => UpdateParameterAvailability();

        lblContrast = new Label
        {
            Text = "k:",
            AutoSize = true,
            Margin = new Padding(0, 12, 8, 0)
        };

        numContrastFactor = new NumericUpDown
        {
            DecimalPlaces = 0,
            Increment = 5.00m,
            Minimum = 0.00m,
            Maximum = 128.00m,
            Value = 64.00m,
            Width = 80,
            Height = 30,
            Margin = new Padding(0, 8, 18, 0)
        };

        lblLocalProcessor = new Label
        {
            Text = "Local:",
            AutoSize = true,
            Margin = new Padding(0, 12, 8, 0)
        };

        cmbLocalProcessor = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 132,
            Height = 30,
            Margin = new Padding(0, 7, 18, 0),
            FlatStyle = FlatStyle.Flat
        };
        cmbLocalProcessor.Items.Add("Method 1");
        cmbLocalProcessor.Items.Add("Method 2");
        cmbLocalProcessor.Items.Add("Method 3");
        cmbLocalProcessor.SelectedIndex = 0;
        cmbLocalProcessor.SelectedIndexChanged += (_, _) => UpdateParameterAvailability();

        lblFragmentWidth = new Label
        {
            Text = "Frag W:",
            AutoSize = true,
            Margin = new Padding(0, 12, 8, 0)
        };

        numFragmentWidth = new NumericUpDown
        {
            DecimalPlaces = 0,
            Increment = 2m,
            Minimum = 1m,
            Maximum = 128m,
            Value = 9m,
            Width = 64,
            Height = 30,
            Margin = new Padding(0, 8, 18, 0)
        };

        lblFragmentHeight = new Label
        {
            Text = "Frag H:",
            AutoSize = true,
            Margin = new Padding(0, 12, 8, 0)
        };

        numFragmentHeight = new NumericUpDown
        {
            DecimalPlaces = 0,
            Increment = 2m,
            Minimum = 1m,
            Maximum = 128m,
            Value = 9m,
            Width = 64,
            Height = 30,
            Margin = new Padding(0, 8, 18, 0)
        };

        lblBlendQ = new Label
        {
            Text = "q:",
            AutoSize = true,
            Margin = new Padding(0, 12, 8, 0)
        };

        numBlendQ = new NumericUpDown
        {
            DecimalPlaces = 2,
            Increment = 0.10m,
            Minimum = 0.00m,
            Maximum = 1.00m,
            Value = 0.50m,
            Width = 62,
            Height = 30,
            Margin = new Padding(0, 8, 18, 0)
        };

        chkUseMultithreading = new CheckBox
        {
            Text = "Multithread",
            AutoSize = true,
            Margin = new Padding(0, 11, 0, 0)
        };

        paramsRow.Controls.Add(lblProcessingMode);
        paramsRow.Controls.Add(cmbProcessingMode);
        paramsRow.Controls.Add(lblContrast);
        paramsRow.Controls.Add(numContrastFactor);
        paramsRow.Controls.Add(lblLocalProcessor);
        paramsRow.Controls.Add(cmbLocalProcessor);
        paramsRow.Controls.Add(lblFragmentWidth);
        paramsRow.Controls.Add(numFragmentWidth);
        paramsRow.Controls.Add(lblFragmentHeight);
        paramsRow.Controls.Add(numFragmentHeight);
        paramsRow.Controls.Add(lblBlendQ);
        paramsRow.Controls.Add(numBlendQ);
        paramsRow.Controls.Add(chkUseMultithreading);

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
        ApplyTheme();
        UpdateParameterAvailability();
    }
}
