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
        imageFrame.BackColor = frameBack;
        pictureBox.BackColor = frameBack;

        lblProcessingMode.ForeColor = textColor;
        lblContrast.ForeColor = textColor;
        lblLocalProcessor.ForeColor = textColor;
        lblFragmentWidth.ForeColor = textColor;
        lblFragmentHeight.ForeColor = textColor;
        chkUseMultithreading.ForeColor = textColor;
        chkUseMultithreading.BackColor = panelBack;

        StyleComboControl(cmbProcessingMode, inputBack, inputText);
        StyleComboControl(cmbLocalProcessor, inputBack, inputText);
        StyleNumericControl(numContrastFactor, inputBack, inputText);
        StyleNumericControl(numFragmentWidth, inputBack, inputText);
        StyleNumericControl(numFragmentHeight, inputBack, inputText);

        SetButtonBaseColor(btnLoadImage, Color.FromArgb(85, 130, 242));
        SetButtonBaseColor(btnApplyContrast, Color.FromArgb(36, 180, 142));
        SetButtonBaseColor(btnSaveImage, Color.FromArgb(118, 130, 145));
    }

    private void UpdateImageViewportBounds()
    {
        Rectangle area = imageCanvas.ClientRectangle;
        area.Inflate(-10, -10);

        if (area.Width <= 0 || area.Height <= 0)
        {
            return;
        }

        int targetWidth = (int)(area.Width * 0.90f);
        int targetHeight = (int)(area.Height * 0.90f);

        targetWidth = Math.Max(160, Math.Min(area.Width, targetWidth));
        targetHeight = Math.Max(140, Math.Min(area.Height, targetHeight));

        int x = area.X + ((area.Width - targetWidth) / 2);
        int y = area.Y + ((area.Height - targetHeight) / 2);

        imageFrame.Bounds = new Rectangle(x, y, targetWidth, targetHeight);
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
