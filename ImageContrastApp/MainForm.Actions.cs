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

        try
        {
            Bitmap adjusted = GetSelectedProcessingMode() switch
            {
                ProcessingMode.GlobalContrast => ImageContrastProcessor.AdjustGlobalContrast(originalImage, (float)numContrastFactor.Value),
                ProcessingMode.LocalFragment => LocalFragmentEngine.Process(originalImage, BuildLocalFragmentSettings()),
                _ => throw new InvalidOperationException("Unknown processing mode.")
            };

            SetDisplayedImage(adjusted);
        }
        catch (NotImplementedException ex)
        {
            MessageBox.Show(ex.Message, "Not available", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
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

    private ProcessingMode GetSelectedProcessingMode()
    {
        return cmbProcessingMode.SelectedIndex == 1
            ? ProcessingMode.LocalFragment
            : ProcessingMode.GlobalContrast;
    }

    private LocalFragmentProcessorKind GetSelectedLocalProcessorKind()
    {
        return cmbLocalProcessor.SelectedIndex switch
        {
            0 => LocalFragmentProcessorKind.Method1,
            1 => LocalFragmentProcessorKind.Method2,
            2 => LocalFragmentProcessorKind.Method3,
            _ => LocalFragmentProcessorKind.Method1
        };
    }

    private LocalFragmentSettings BuildLocalFragmentSettings()
    {
        return new LocalFragmentSettings
        {
            FragmentWidth = (int)numFragmentWidth.Value,
            FragmentHeight = (int)numFragmentHeight.Value,
            TargetStandardDeviation = (float)numContrastFactor.Value,
            UseMultithreading = chkUseMultithreading.Checked,
            MaxDegreeOfParallelism = Environment.ProcessorCount,
            ProcessorKind = GetSelectedLocalProcessorKind()
        };
    }

    private void UpdateParameterAvailability()
    {
        bool isLocalMode = GetSelectedProcessingMode() == ProcessingMode.LocalFragment;
        bool showAdaptiveQ = isLocalMode && GetSelectedLocalProcessorKind() == LocalFragmentProcessorKind.Method3;
        lblContrast.Text = isLocalMode ? "σz (Local):" : "σz (Global TV):";
        lblBlendQ.Text = "q = clamp(1 - σ/80):";

        cmbLocalProcessor.Enabled = isLocalMode;
        numFragmentWidth.Enabled = isLocalMode;
        numFragmentHeight.Enabled = isLocalMode;
        numBlendQ.Visible = false;
        chkUseMultithreading.Enabled = isLocalMode;
        lblLocalProcessor.Enabled = isLocalMode;
        lblFragmentWidth.Enabled = isLocalMode;
        lblFragmentHeight.Enabled = isLocalMode;
        lblBlendQ.Visible = showAdaptiveQ;
    }
}
