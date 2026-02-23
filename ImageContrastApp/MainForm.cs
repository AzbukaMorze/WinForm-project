using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ImageContrastApp;

public sealed class MainForm : Form
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
            Text = "Factor:",
            AutoSize = true,
            Left = 438,
            Top = 21
        };

        numContrastFactor = new NumericUpDown
        {
            DecimalPlaces = 2,
            Increment = 0.10m,
            Minimum = 0.00m,
            Maximum = 4.00m,
            Value = 1.20m,
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

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        ClearImages();
        base.OnFormClosed(e);
    }

    private void btnLoadImage_Click(object? sender, EventArgs e)
    {
        using var openFileDialog = new OpenFileDialog
        {
            Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp",
            Title = "Select an image"
        };

        if (openFileDialog.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        using var loaded = new Bitmap(openFileDialog.FileName);
        var loadedCopy = new Bitmap(loaded);

        originalImage?.Dispose();
        originalImage = loadedCopy;

        SetDisplayedImage(new Bitmap(loadedCopy));
    }

    private void btnApplyContrast_Click(object? sender, EventArgs e)
    {
        if (originalImage is null)
        {
            MessageBox.Show("Load an image first.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        float factor = (float)numContrastFactor.Value;
        var adjusted = AdjustContrast(originalImage, factor);
        SetDisplayedImage(adjusted);
    }

    private void btnSaveImage_Click(object? sender, EventArgs e)
    {
        if (displayedImage is null)
        {
            MessageBox.Show("No image to save.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var saveFileDialog = new SaveFileDialog
        {
            Filter = "PNG Image|*.png|JPEG Image|*.jpg;*.jpeg|BMP Image|*.bmp",
            Title = "Save processed image",
            FileName = "processed-image.png"
        };

        if (saveFileDialog.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        ImageFormat format = GetImageFormat(saveFileDialog.FileName);
        displayedImage.Save(saveFileDialog.FileName, format);
    }

    private void SetDisplayedImage(Bitmap image)
    {
        pictureBox.Image = null;
        displayedImage?.Dispose();
        displayedImage = image;
        pictureBox.Image = displayedImage;
    }

    private void ClearImages()
    {
        pictureBox.Image = null;
        displayedImage?.Dispose();
        originalImage?.Dispose();
        displayedImage = null;
        originalImage = null;
    }

    private static ImageFormat GetImageFormat(string fileName)
    {
        string extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => ImageFormat.Jpeg,
            ".bmp" => ImageFormat.Bmp,
            _ => ImageFormat.Png
        };
    }

    private static Bitmap AdjustContrast(Bitmap image, float contrastFactor)
    {
        Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
        using Bitmap source = image.Clone(rect, PixelFormat.Format32bppArgb);
        Bitmap result = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb);

        BitmapData? sourceData = null;
        BitmapData? resultData = null;

        try
        {
            sourceData = source.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            resultData = result.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            int sourceStride = Math.Abs(sourceData.Stride);
            int resultStride = Math.Abs(resultData.Stride);

            int sourceBytes = sourceStride * source.Height;
            int resultBytes = resultStride * result.Height;

            byte[] sourceBuffer = new byte[sourceBytes];
            byte[] resultBuffer = new byte[resultBytes];

            Marshal.Copy(sourceData.Scan0, sourceBuffer, 0, sourceBytes);

            for (int y = 0; y < source.Height; y++)
            {
                int sourceRow = sourceData.Stride >= 0
                    ? y * sourceStride
                    : (source.Height - 1 - y) * sourceStride;

                int resultRow = resultData.Stride >= 0
                    ? y * resultStride
                    : (result.Height - 1 - y) * resultStride;

                for (int x = 0; x < source.Width; x++)
                {
                    int sIndex = sourceRow + (x * 4);
                    int dIndex = resultRow + (x * 4);

                    float b = sourceBuffer[sIndex] / 255f;
                    float g = sourceBuffer[sIndex + 1] / 255f;
                    float r = sourceBuffer[sIndex + 2] / 255f;

                    r = Clamp01(((r - 0.5f) * contrastFactor) + 0.5f);
                    g = Clamp01(((g - 0.5f) * contrastFactor) + 0.5f);
                    b = Clamp01(((b - 0.5f) * contrastFactor) + 0.5f);

                    resultBuffer[dIndex] = (byte)(b * 255f);
                    resultBuffer[dIndex + 1] = (byte)(g * 255f);
                    resultBuffer[dIndex + 2] = (byte)(r * 255f);
                    resultBuffer[dIndex + 3] = sourceBuffer[sIndex + 3];
                }
            }

            Marshal.Copy(resultBuffer, 0, resultData.Scan0, resultBytes);
        }
        finally
        {
            if (sourceData is not null)
            {
                source.UnlockBits(sourceData);
            }

            if (resultData is not null)
            {
                result.UnlockBits(resultData);
            }
        }

        return result;
    }

    private static float Clamp01(float value)
    {
        if (value < 0f)
        {
            return 0f;
        }

        if (value > 1f)
        {
            return 1f;
        }

        return value;
    }
}
