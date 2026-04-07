using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace ImageContrastApp;

public sealed partial class MainForm
{
    private void cmbLanguage_SelectedIndexChanged(object? sender, EventArgs e)
    {
        UiLanguage selectedLanguage = cmbLanguage.SelectedIndex == 1
            ? UiLanguage.English
            : UiLanguage.Russian;

        if (UiText.CurrentLanguage == selectedLanguage)
        {
            return;
        }

        UiText.CurrentLanguage = selectedLanguage;
        ApplyLocalizedText();
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
            Filter = uiText.ImageFilesFilter,
            Title = uiText.OpenImageTitle
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
            MessageBox.Show(uiText.LoadImageFirst, uiText.InfoCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            MessageBox.Show(ex.Message, uiText.NotAvailableCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void btnSaveImage_Click(object? sender, EventArgs e)
    {
        if (displayedImage is null)
        {
            MessageBox.Show(uiText.NoImageToSave, uiText.InfoCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var saveFileDialog = new SaveFileDialog
        {
            Filter = uiText.SaveImageFilter,
            Title = uiText.SaveImageTitle,
            FileName = uiText.SaveImageDefaultName
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
            3 => LocalFragmentProcessorKind.Method4,
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
            BlendQ = (float)numBlendQ.Value,
            UseMultithreading = chkUseMultithreading.Checked,
            MaxDegreeOfParallelism = Environment.ProcessorCount,
            ProcessorKind = GetSelectedLocalProcessorKind()
        };
    }

    private void UpdateParameterAvailability()
    {
        bool isLocalMode = GetSelectedProcessingMode() == ProcessingMode.LocalFragment;
        LocalFragmentProcessorKind localKind = GetSelectedLocalProcessorKind();
        bool useManualQ = isLocalMode && localKind == LocalFragmentProcessorKind.Method3;
        bool showAdaptiveQHint = isLocalMode && localKind == LocalFragmentProcessorKind.Method4;
        lblContrast.Text = isLocalMode ? uiText.ContrastLabelLocal : uiText.ContrastLabelGlobal;
        lblBlendQ.Text = showAdaptiveQHint ? uiText.AdaptiveQLabel : uiText.BlendQLabel;

        cmbLocalProcessor.Enabled = isLocalMode;
        numFragmentWidth.Enabled = isLocalMode;
        numFragmentHeight.Enabled = isLocalMode;
        numBlendQ.Visible = useManualQ;
        numBlendQ.Enabled = useManualQ;
        chkUseMultithreading.Enabled = isLocalMode;
        lblLocalProcessor.Enabled = isLocalMode;
        lblFragmentWidth.Enabled = isLocalMode;
        lblFragmentHeight.Enabled = isLocalMode;
        lblBlendQ.Visible = useManualQ || showAdaptiveQHint;
        lblBlendQ.Enabled = useManualQ;
    }

    private void ApplyLocalizedText()
    {
        int selectedModeIndex = cmbProcessingMode.SelectedIndex < 0 ? 0 : cmbProcessingMode.SelectedIndex;
        int selectedMethodIndex = cmbLocalProcessor.SelectedIndex < 0 ? 0 : cmbLocalProcessor.SelectedIndex;

        Text = uiText.FormTitle;
        btnLoadImage.Text = uiText.LoadButton;
        btnApplyContrast.Text = uiText.ApplyButton;
        btnSaveImage.Text = uiText.SaveButton;
        lblLanguage.Text = uiText.LanguageLabel;
        lblProcessingMode.Text = uiText.ModeLabel;
        lblLocalProcessor.Text = uiText.LocalMethodLabel;
        lblFragmentWidth.Text = uiText.FragmentWidthLabel;
        lblFragmentHeight.Text = uiText.FragmentHeightLabel;
        chkUseMultithreading.Text = uiText.Multithreading;

        cmbLanguage.Items.Clear();
        cmbLanguage.Items.Add(uiText.RussianLanguage);
        cmbLanguage.Items.Add(uiText.EnglishLanguage);
        cmbLanguage.SelectedIndex = UiText.CurrentLanguage == UiLanguage.Russian ? 0 : 1;

        cmbProcessingMode.Items.Clear();
        cmbProcessingMode.Items.Add(uiText.GlobalMode);
        cmbProcessingMode.Items.Add(uiText.LocalMode);
        cmbProcessingMode.SelectedIndex = selectedModeIndex;

        cmbLocalProcessor.Items.Clear();
        cmbLocalProcessor.Items.Add(uiText.Method1);
        cmbLocalProcessor.Items.Add(uiText.Method2);
        cmbLocalProcessor.Items.Add(uiText.Method3);
        cmbLocalProcessor.Items.Add(uiText.Method4);
        cmbLocalProcessor.SelectedIndex = selectedMethodIndex;

        UpdateParameterAvailability();
    }
}
