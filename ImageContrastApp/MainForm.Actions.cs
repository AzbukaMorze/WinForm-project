using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace ImageContrastApp;

public sealed partial class MainForm
{
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

        float globalContrastK = (float)numContrastFactor.Value;
        Bitmap adjusted = ImageContrastProcessor.AdjustGlobalContrast(originalImage, globalContrastK);
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
}

