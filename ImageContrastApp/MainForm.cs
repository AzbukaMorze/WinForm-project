using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ImageContrastApp;

public sealed partial class MainForm : Form
{
    private UiTextSet uiText => UiText.Current;

    private enum ProcessingMode
    {
        GlobalContrast,
        LocalFragment,
        LocalMeanTvContrast
    }

    private readonly Panel topPanel;
    private readonly Panel topDivider;
    private readonly FlowLayoutPanel actionRow;
    private readonly FlowLayoutPanel paramsRow;
    private readonly Panel imageCanvas;
    private readonly TableLayoutPanel imageGrid;
    private readonly Panel sourceImageFrame;
    private readonly Panel previousImageFrame;
    private readonly Panel currentImageFrame;
    private readonly Panel previousInfoPanel;
    private readonly Panel currentInfoPanel;

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
    private readonly Label lblLanguage;
    private readonly Label lblProcessingMode;
    private readonly Label lblLocalProcessor;
    private readonly Label lblFragmentWidth;
    private readonly Label lblFragmentHeight;
    private readonly Label lblBlendQ;
    private readonly CheckBox chkUseMultithreading;
    private readonly ComboBox cmbLanguage;
    private readonly PictureBox sourcePictureBox;
    private readonly PictureBox previousPictureBox;
    private readonly PictureBox currentPictureBox;
    private readonly Label lblSourcePreview;
    private readonly Label lblPreviousPreview;
    private readonly Label lblCurrentPreview;
    private readonly Label lblSourceInfo;
    private readonly Label lblPreviousInfo;
    private readonly Label lblPreviousDetails;
    private readonly Label lblCurrentInfo;
    private readonly Label lblCurrentDetails;
    private readonly Dictionary<Button, Color> buttonBaseColors;

    private Bitmap? sourceImage;
    private Bitmap? previousProcessedImage;
    private Bitmap? currentProcessedImage;
    private ImageBrightnessStats? sourceStats;
    private ImageBrightnessStats? previousStats;
    private ImageBrightnessStats? currentStats;
    private ProcessingInfo? previousProcessingInfo;
    private ProcessingInfo? currentProcessingInfo;

    public MainForm()
    {
        Text = uiText.FormTitle;
        Width = 1800;
        Height = 1080;
        MinimumSize = new Size(Width, Height);
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
            Text = uiText.LoadButton,
            Width = 128,
            Height = 34,
            Margin = new Padding(0, 0, 10, 0)
        };
        btnLoadImage.Click += btnLoadImage_Click;

        btnApplyContrast = new Button
        {
            Text = uiText.ApplyButton,
            Width = 148,
            Height = 34,
            Margin = new Padding(0, 0, 10, 0)
        };
        btnApplyContrast.Click += btnApplyContrast_Click;

        btnSaveImage = new Button
        {
            Text = uiText.SaveButton,
            Width = 128,
            Height = 34,
            Margin = new Padding(0, 0, 20, 0)
        };
        btnSaveImage.Click += btnSaveImage_Click;

        actionRow.Controls.Add(btnLoadImage);
        actionRow.Controls.Add(btnApplyContrast);
        actionRow.Controls.Add(btnSaveImage);

        lblLanguage = new Label
        {
            Text = uiText.LanguageLabel,
            AutoSize = true,
            Margin = new Padding(0, 12, 8, 0)
        };

        cmbLanguage = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 116,
            Height = 30,
            Margin = new Padding(0, 7, 0, 0),
            FlatStyle = FlatStyle.Flat
        };
        cmbLanguage.SelectedIndexChanged += cmbLanguage_SelectedIndexChanged;

        actionRow.Controls.Add(lblLanguage);
        actionRow.Controls.Add(cmbLanguage);

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
            Text = uiText.ModeLabel,
            AutoSize = true,
            Margin = new Padding(0, 12, 8, 0)
        };

        cmbProcessingMode = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 190,
            Height = 30,
            Margin = new Padding(0, 7, 18, 0),
            FlatStyle = FlatStyle.Flat
        };
        cmbProcessingMode.Items.Add(uiText.GlobalMode);
        cmbProcessingMode.Items.Add(uiText.LocalMode);
        cmbProcessingMode.Items.Add(uiText.LocalMeanTvMode);
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
            Text = uiText.LocalMethodLabel,
            AutoSize = true,
            Margin = new Padding(0, 12, 8, 0)
        };

        cmbLocalProcessor = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 140,
            Height = 30,
            Margin = new Padding(0, 7, 18, 0),
            FlatStyle = FlatStyle.Flat
        };
        cmbLocalProcessor.Items.Add(uiText.Method1);
        cmbLocalProcessor.Items.Add(uiText.Method2);
        cmbLocalProcessor.Items.Add(uiText.Method3);
        cmbLocalProcessor.Items.Add(uiText.Method4);
        cmbLocalProcessor.SelectedIndex = 0;
        cmbLocalProcessor.SelectedIndexChanged += (_, _) => UpdateParameterAvailability();

        lblFragmentWidth = new Label
        {
            Text = uiText.FragmentWidthLabel,
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
            Text = uiText.FragmentHeightLabel,
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
            Text = uiText.BlendQLabel,
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
            Text = uiText.Multithreading,
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

        imageGrid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 3,
            BackColor = Color.Transparent
        };
        imageGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333f));
        imageGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333f));
        imageGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.334f));
        imageGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        imageGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        imageGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 92));

        lblSourcePreview = CreatePreviewTitleLabel();
        lblPreviousPreview = CreatePreviewTitleLabel();
        lblCurrentPreview = CreatePreviewTitleLabel();

        sourceImageFrame = CreateImageFrame();
        previousImageFrame = CreateImageFrame();
        currentImageFrame = CreateImageFrame();

        sourcePictureBox = CreatePreviewPictureBox();
        previousPictureBox = CreatePreviewPictureBox();
        currentPictureBox = CreatePreviewPictureBox();

        sourceImageFrame.Controls.Add(sourcePictureBox);
        previousImageFrame.Controls.Add(previousPictureBox);
        currentImageFrame.Controls.Add(currentPictureBox);

        lblSourceInfo = CreatePreviewInfoLabel();
        previousInfoPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(8, 5, 8, 5),
            Margin = new Padding(6)
        };

        lblPreviousInfo = CreatePreviewInfoLabel();
        lblPreviousInfo.Dock = DockStyle.Top;
        lblPreviousInfo.Height = 22;
        lblPreviousInfo.Margin = Padding.Empty;

        lblPreviousDetails = CreatePreviewInfoLabel();
        lblPreviousDetails.Dock = DockStyle.Fill;
        lblPreviousDetails.Margin = Padding.Empty;

        previousInfoPanel.Controls.Add(lblPreviousDetails);
        previousInfoPanel.Controls.Add(lblPreviousInfo);

        currentInfoPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(8, 5, 8, 5),
            Margin = new Padding(6)
        };

        lblCurrentInfo = CreatePreviewInfoLabel();
        lblCurrentInfo.Dock = DockStyle.Top;
        lblCurrentInfo.Height = 22;
        lblCurrentInfo.Margin = Padding.Empty;

        lblCurrentDetails = CreatePreviewInfoLabel();
        lblCurrentDetails.Dock = DockStyle.Fill;
        lblCurrentDetails.Margin = Padding.Empty;

        currentInfoPanel.Controls.Add(lblCurrentDetails);
        currentInfoPanel.Controls.Add(lblCurrentInfo);

        imageGrid.Controls.Add(lblSourcePreview, 0, 0);
        imageGrid.Controls.Add(lblPreviousPreview, 1, 0);
        imageGrid.Controls.Add(lblCurrentPreview, 2, 0);
        imageGrid.Controls.Add(sourceImageFrame, 0, 1);
        imageGrid.Controls.Add(previousImageFrame, 1, 1);
        imageGrid.Controls.Add(currentImageFrame, 2, 1);
        imageGrid.Controls.Add(lblSourceInfo, 0, 2);
        imageGrid.Controls.Add(previousInfoPanel, 1, 2);
        imageGrid.Controls.Add(currentInfoPanel, 2, 2);

        imageCanvas.Controls.Add(imageGrid);

        Controls.Add(imageCanvas);
        Controls.Add(topDivider);
        Controls.Add(topPanel);

        buttonBaseColors = new Dictionary<Button, Color>(3);

        StyleActionButton(btnLoadImage);
        StyleActionButton(btnApplyContrast);
        StyleActionButton(btnSaveImage);

        imageCanvas.Resize += (_, _) => UpdateImageViewportBounds();
        sourceImageFrame.Resize += (_, _) => ApplyRoundedCorners(sourceImageFrame, 16);
        previousImageFrame.Resize += (_, _) => ApplyRoundedCorners(previousImageFrame, 16);
        currentImageFrame.Resize += (_, _) => ApplyRoundedCorners(currentImageFrame, 16);
        previousInfoPanel.Resize += (_, _) => ApplyRoundedCorners(previousInfoPanel, 10);
        currentInfoPanel.Resize += (_, _) => ApplyRoundedCorners(currentInfoPanel, 10);

        UpdateImageViewportBounds();
        ApplyRoundedCorners(sourceImageFrame, 16);
        ApplyRoundedCorners(previousImageFrame, 16);
        ApplyRoundedCorners(currentImageFrame, 16);
        ApplyRoundedCorners(previousInfoPanel, 10);
        ApplyRoundedCorners(currentInfoPanel, 10);
        ApplyTheme();
        ApplyLocalizedText();
        UpdateParameterAvailability();
    }
}
