using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ImageContrastApp;

public sealed partial class MainForm
{
    private const int CornerRadius = 12;

    private void ApplyTheme()
    {
        Color formBack = Color.FromArgb(27, 32, 40);
        Color panelBack = Color.FromArgb(33, 39, 48);
        Color dividerBack = Color.FromArgb(60, 69, 83);
        Color canvasBack = Color.FromArgb(27, 32, 40);
        Color frameBack = Color.FromArgb(42, 48, 58);
        Color textColor = Color.FromArgb(224, 230, 238);
        Color inputBack = Color.FromArgb(48, 56, 67);
        Color inputText = Color.FromArgb(232, 237, 243);

        BackColor = formBack;
        topPanel.BackColor = panelBack;
        topDivider.BackColor = dividerBack;
        imageCanvas.BackColor = canvasBack;
        sourceImageFrame.BackColor = frameBack;
        previousImageFrame.BackColor = frameBack;
        currentImageFrame.BackColor = frameBack;
        previousInfoPanel.BackColor = frameBack;
        currentInfoPanel.BackColor = frameBack;
        sourcePictureBox.BackColor = frameBack;
        previousPictureBox.BackColor = frameBack;
        currentPictureBox.BackColor = frameBack;

        lblProcessingMode.ForeColor = textColor;
        lblContrast.ForeColor = textColor;
        lblLanguage.ForeColor = textColor;
        lblLocalProcessor.ForeColor = textColor;
        lblFragmentWidth.ForeColor = textColor;
        lblFragmentHeight.ForeColor = textColor;
        lblBlendQ.ForeColor = textColor;
        lblSourcePreview.ForeColor = textColor;
        lblPreviousPreview.ForeColor = textColor;
        lblCurrentPreview.ForeColor = textColor;
        lblSourceInfo.ForeColor = textColor;
        lblPreviousInfo.ForeColor = textColor;
        lblPreviousDetails.ForeColor = textColor;
        lblCurrentInfo.ForeColor = textColor;
        lblCurrentDetails.ForeColor = textColor;
        chkUseMultithreading.ForeColor = textColor;
        chkUseMultithreading.BackColor = panelBack;

        StyleComboControl(cmbProcessingMode, inputBack, inputText);
        StyleComboControl(cmbLanguage, inputBack, inputText);
        StyleComboControl(cmbLocalProcessor, inputBack, inputText);
        StyleNumericControl(numContrastFactor, inputBack, inputText);
        StyleNumericControl(numFragmentWidth, inputBack, inputText);
        StyleNumericControl(numFragmentHeight, inputBack, inputText);
        StyleNumericControl(numBlendQ, inputBack, inputText);

        SetButtonBaseColor(btnLoadImage, Color.FromArgb(85, 130, 242));
        SetButtonBaseColor(btnApplyContrast, Color.FromArgb(36, 180, 142));
        SetButtonBaseColor(btnSaveImage, Color.FromArgb(118, 130, 145));
    }

    private void UpdateImageViewportBounds()
    {
        int availableWidth = imageCanvas.ClientSize.Width - imageCanvas.Padding.Horizontal;
        if (availableWidth <= 0)
        {
            return;
        }

        imageGrid.SuspendLayout();
        imageGrid.ColumnStyles.Clear();
        imageGrid.RowStyles.Clear();

        if (availableWidth < 760)
        {
            imageGrid.ColumnCount = 1;
            imageGrid.RowCount = 9;
            imageGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            AddStackedPreviewRows();
        }
        else
        {
            imageGrid.ColumnCount = 3;
            imageGrid.RowCount = 3;
            imageGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333f));
            imageGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333f));
            imageGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.334f));
            imageGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            imageGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            imageGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
            SetPreviewCellPositionsForWideLayout();
        }

        imageGrid.ResumeLayout();
    }

    private void AddStackedPreviewRows()
    {
        imageGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        imageGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 33.333f));
        imageGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
        imageGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        imageGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 33.333f));
        imageGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
        imageGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        imageGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 33.334f));
        imageGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));

        SetCell(lblSourcePreview, 0, 0);
        SetCell(sourceImageFrame, 0, 1);
        SetCell(lblSourceInfo, 0, 2);
        SetCell(lblPreviousPreview, 0, 3);
        SetCell(previousImageFrame, 0, 4);
        SetCell(previousInfoPanel, 0, 5);
        SetCell(lblCurrentPreview, 0, 6);
        SetCell(currentImageFrame, 0, 7);
        SetCell(currentInfoPanel, 0, 8);
    }

    private void SetPreviewCellPositionsForWideLayout()
    {
        SetCell(lblSourcePreview, 0, 0);
        SetCell(lblPreviousPreview, 1, 0);
        SetCell(lblCurrentPreview, 2, 0);
        SetCell(sourceImageFrame, 0, 1);
        SetCell(previousImageFrame, 1, 1);
        SetCell(currentImageFrame, 2, 1);
        SetCell(lblSourceInfo, 0, 2);
        SetCell(previousInfoPanel, 1, 2);
        SetCell(currentInfoPanel, 2, 2);
    }

    private void SetCell(Control control, int column, int row)
    {
        imageGrid.SetColumn(control, column);
        imageGrid.SetRow(control, row);
        imageGrid.SetColumnSpan(control, 1);
        imageGrid.SetRowSpan(control, 1);
    }

    private static Panel CreateImageFrame()
    {
        return new Panel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(6)
        };
    }

    private static PictureBox CreatePreviewPictureBox()
    {
        return new PictureBox
        {
            Dock = DockStyle.Fill,
            SizeMode = PictureBoxSizeMode.Zoom
        };
    }

    private static Label CreatePreviewTitleLabel()
    {
        return new Label
        {
            Dock = DockStyle.Fill,
            AutoEllipsis = true,
            Font = new Font(Control.DefaultFont, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = new Padding(8, 0, 8, 0)
        };
    }

    private static Label CreatePreviewInfoLabel()
    {
        return new Label
        {
            Dock = DockStyle.Fill,
            AutoEllipsis = true,
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = new Padding(8, 4, 8, 4)
        };
    }

    private static void StyleNumericControl(NumericUpDown control, Color backColor, Color textColor)
    {
        control.BackColor = backColor;
        control.ForeColor = textColor;
        control.BorderStyle = BorderStyle.FixedSingle;
    }

    private static void StyleComboControl(ComboBox control, Color backColor, Color textColor)
    {
        control.BackColor = backColor;
        control.ForeColor = textColor;
    }

    private void StyleActionButton(Button button)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.ForeColor = Color.White;
        button.Font = new Font(button.Font, FontStyle.Bold);
        button.Cursor = Cursors.Hand;

        button.MouseEnter += (_, _) =>
        {
            if (!button.Enabled)
            {
                return;
            }

            button.BackColor = ShiftColor(buttonBaseColors[button], 18);
        };

        button.MouseLeave += (_, _) =>
        {
            if (!button.Enabled)
            {
                return;
            }

            button.BackColor = buttonBaseColors[button];
        };

        button.MouseDown += (_, _) =>
        {
            if (!button.Enabled)
            {
                return;
            }

            button.BackColor = ShiftColor(buttonBaseColors[button], -22);
        };

        button.MouseUp += (_, _) =>
        {
            if (!button.Enabled)
            {
                return;
            }

            Point local = button.PointToClient(Cursor.Position);
            bool inside = local.X >= 0 && local.Y >= 0 && local.X < button.Width && local.Y < button.Height;
            button.BackColor = inside ? ShiftColor(buttonBaseColors[button], 18) : buttonBaseColors[button];
        };

        button.EnabledChanged += (_, _) =>
        {
            if (!button.Enabled)
            {
                button.BackColor = ShiftColor(buttonBaseColors[button], -40);
                return;
            }

            button.BackColor = buttonBaseColors[button];
        };

        button.Resize += (_, _) => ApplyRoundedCorners(button, CornerRadius);
        ApplyRoundedCorners(button, CornerRadius);
    }

    private void SetButtonBaseColor(Button button, Color baseColor)
    {
        buttonBaseColors[button] = baseColor;
        if (button.Enabled)
        {
            button.BackColor = baseColor;
        }
    }

    private static Color ShiftColor(Color color, int shift)
    {
        int r = Math.Max(0, Math.Min(255, color.R + shift));
        int g = Math.Max(0, Math.Min(255, color.G + shift));
        int b = Math.Max(0, Math.Min(255, color.B + shift));
        return Color.FromArgb(r, g, b);
    }

    private static void ApplyRoundedCorners(Control control, int radius)
    {
        if (control.Width <= 1 || control.Height <= 1)
        {
            return;
        }

        Rectangle rect = new Rectangle(0, 0, control.Width, control.Height);
        using GraphicsPath path = CreateRoundedPath(rect, radius);
        Region? oldRegion = control.Region;
        control.Region = new Region(path);
        oldRegion?.Dispose();
    }

    private static GraphicsPath CreateRoundedPath(Rectangle rect, int radius)
    {
        int diameter = Math.Max(2, radius * 2);
        GraphicsPath path = new GraphicsPath();

        path.AddArc(rect.Left, rect.Top, diameter, diameter, 180, 90);
        path.AddArc(rect.Right - diameter, rect.Top, diameter, diameter, 270, 90);
        path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(rect.Left, rect.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();

        return path;
    }
}
