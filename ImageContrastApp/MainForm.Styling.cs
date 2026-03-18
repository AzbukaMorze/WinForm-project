using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ImageContrastApp;

public sealed partial class MainForm
{
    private enum UiTheme
    {
        Light,
        Dark
    }

    private const int CornerRadius = 12;

    private void ApplyTheme(UiTheme theme)
    {
        bool isDark = theme == UiTheme.Dark;

        Color formBack = isDark ? Color.FromArgb(27, 32, 40) : Color.FromArgb(236, 240, 246);
        Color panelBack = isDark ? Color.FromArgb(33, 39, 48) : Color.FromArgb(248, 250, 252);
        Color dividerBack = isDark ? Color.FromArgb(60, 69, 83) : Color.FromArgb(218, 224, 232);
        Color canvasBack = isDark ? Color.FromArgb(27, 32, 40) : Color.FromArgb(236, 240, 246);
        Color frameBack = isDark ? Color.FromArgb(42, 48, 58) : Color.FromArgb(249, 251, 253);
        Color textColor = isDark ? Color.FromArgb(224, 230, 238) : Color.FromArgb(58, 70, 84);
        Color inputBack = isDark ? Color.FromArgb(48, 56, 67) : Color.White;
        Color inputText = isDark ? Color.FromArgb(232, 237, 243) : Color.FromArgb(33, 43, 54);

        BackColor = formBack;
        topPanel.BackColor = panelBack;
        topDivider.BackColor = dividerBack;
        imageCanvas.BackColor = canvasBack;
        imageFrame.BackColor = frameBack;
        pictureBox.BackColor = frameBack;

        lblTheme.ForeColor = textColor;
        lblContrast.ForeColor = textColor;

        StyleComboControl(cmbTheme, inputBack, inputText);
        StyleNumericControl(numContrastFactor, inputBack, inputText);

        SetButtonBaseColor(btnLoadImage, isDark ? Color.FromArgb(85, 130, 242) : Color.FromArgb(62, 110, 241));
        SetButtonBaseColor(btnApplyContrast, isDark ? Color.FromArgb(36, 180, 142) : Color.FromArgb(28, 157, 124));
        SetButtonBaseColor(btnSaveImage, isDark ? Color.FromArgb(118, 130, 145) : Color.FromArgb(90, 103, 120));
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
