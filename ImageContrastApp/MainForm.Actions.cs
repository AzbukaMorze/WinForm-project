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

        SetSourceImage(loadedCopy);
        ClearProcessedImages();
        UpdateProcessingInfo();
    }

    private void btnApplyContrast_Click(object? sender, EventArgs e)
    {
        if (sourceImage is null)
        {
            MessageBox.Show(uiText.LoadImageFirst, uiText.InfoCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            Bitmap adjusted = GetSelectedProcessingMode() switch
            {
                ProcessingMode.GlobalContrast => ImageContrastProcessor.AdjustGlobalContrast(sourceImage, (float)numContrastFactor.Value),
                ProcessingMode.LocalFragment => LocalFragmentEngine.Process(sourceImage, BuildLocalFragmentSettings()),
                ProcessingMode.LocalMeanTvContrast => LocalMeanTvProcessor.AdjustContrast(
                    sourceImage,
                    (float)numContrastFactor.Value,
                    (int)numFragmentWidth.Value,
                    (int)numFragmentHeight.Value),
                _ => throw new InvalidOperationException("Unknown processing mode.")
            };

            SetCurrentProcessedImage(adjusted, BuildProcessingInfo());
        }
        catch (NotImplementedException ex)
        {
            MessageBox.Show(ex.Message, uiText.NotAvailableCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void btnSaveImage_Click(object? sender, EventArgs e)
    {
        if (currentProcessedImage is null)
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
        currentProcessedImage.Save(saveFileDialog.FileName, format);
    }

    private void SetSourceImage(Bitmap image)
    {
        sourcePictureBox.Image = null;
        sourceImage?.Dispose();
        sourceImage = image;
        sourcePictureBox.Image = sourceImage;
    }

    private void SetCurrentProcessedImage(Bitmap image, ProcessingInfo processingInfo)
    {
        previousPictureBox.Image = null;
        previousProcessedImage?.Dispose();
        previousProcessedImage = currentProcessedImage is null ? null : new Bitmap(currentProcessedImage);
        previousPictureBox.Image = previousProcessedImage;
        previousProcessingInfo = currentProcessingInfo;

        currentPictureBox.Image = null;
        currentProcessedImage?.Dispose();
        currentProcessedImage = image;
        currentPictureBox.Image = currentProcessedImage;
        currentProcessingInfo = processingInfo;
        UpdateProcessingInfo();
    }

    private void ClearProcessedImages()
    {
        previousPictureBox.Image = null;
        currentPictureBox.Image = null;
        previousProcessedImage?.Dispose();
        currentProcessedImage?.Dispose();
        previousProcessedImage = null;
        currentProcessedImage = null;
        previousProcessingInfo = null;
        currentProcessingInfo = null;
    }

    private void ClearImages()
    {
        sourcePictureBox.Image = null;
        ClearProcessedImages();
        sourceImage?.Dispose();
        sourceImage = null;
        UpdateProcessingInfo();
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
        return cmbProcessingMode.SelectedIndex switch
        {
            1 => ProcessingMode.LocalFragment,
            2 => ProcessingMode.LocalMeanTvContrast,
            _ => ProcessingMode.GlobalContrast
        };
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

    private ProcessingInfo BuildProcessingInfo()
    {
        ProcessingMode selectedMode = GetSelectedProcessingMode();
        string methodName = cmbProcessingMode.Text;
        string details = selectedMode switch
        {
            ProcessingMode.GlobalContrast => string.Format(
                uiText.GlobalDetailsFormat,
                numContrastFactor.Value),
            ProcessingMode.LocalFragment => string.Format(
                uiText.LocalDetailsFormat,
                cmbLocalProcessor.Text,
                numContrastFactor.Value,
                numFragmentWidth.Value,
                numFragmentHeight.Value,
                GetLocalQDescription()),
            ProcessingMode.LocalMeanTvContrast => string.Format(
                uiText.LocalMeanTvDetailsFormat,
                numContrastFactor.Value,
                numFragmentWidth.Value,
                numFragmentHeight.Value),
            _ => string.Empty
        };

        return new ProcessingInfo(methodName, details, DateTime.Now);
    }

    private string GetLocalQDescription()
    {
        LocalFragmentProcessorKind selectedKind = GetSelectedLocalProcessorKind();
        return selectedKind switch
        {
            LocalFragmentProcessorKind.Method3 => numBlendQ.Value.ToString("0.00"),
            LocalFragmentProcessorKind.Method4 => uiText.AdaptiveQInfo,
            _ => uiText.NotUsedInfo
        };
    }

    private void UpdateProcessingInfo()
    {
        lblSourceInfo.Text = sourceImage is null
            ? uiText.NoSourceImage
            : string.Format(uiText.SourceImageInfoFormat, sourceImage.Width, sourceImage.Height);

        if (previousProcessedImage is null || previousProcessingInfo is null)
        {
            lblPreviousInfo.Text = uiText.NoPreviousImage;
            lblPreviousDetails.Text = string.Empty;
        }
        else
        {
            lblPreviousInfo.Text = string.Format(
                uiText.PreviousImageInfoFormat,
                previousProcessingInfo.MethodName,
                previousProcessingInfo.AppliedAt.ToString("HH:mm:ss"));
            lblPreviousDetails.Text = previousProcessingInfo.Details;
        }

        if (currentProcessedImage is null || currentProcessingInfo is null)
        {
            lblCurrentInfo.Text = uiText.NoCurrentImage;
            lblCurrentDetails.Text = string.Empty;
            return;
        }

        lblCurrentInfo.Text = string.Format(
            uiText.CurrentImageInfoFormat,
            currentProcessingInfo.MethodName,
            currentProcessingInfo.AppliedAt.ToString("HH:mm:ss"));
        lblCurrentDetails.Text = currentProcessingInfo.Details;
    }

    private void UpdateParameterAvailability()
    {
        ProcessingMode selectedMode = GetSelectedProcessingMode();
        bool isLocalMode = selectedMode == ProcessingMode.LocalFragment;
        bool usesWindow = isLocalMode || selectedMode == ProcessingMode.LocalMeanTvContrast;
        LocalFragmentProcessorKind localKind = GetSelectedLocalProcessorKind();
        bool useManualQ = isLocalMode && localKind == LocalFragmentProcessorKind.Method3;
        bool showAdaptiveQHint = isLocalMode && localKind == LocalFragmentProcessorKind.Method4;
        lblContrast.Text = isLocalMode ? uiText.ContrastLabelLocal : uiText.ContrastLabelGlobal;
        lblBlendQ.Text = showAdaptiveQHint ? uiText.AdaptiveQLabel : uiText.BlendQLabel;

        cmbLocalProcessor.Enabled = isLocalMode;
        numFragmentWidth.Enabled = usesWindow;
        numFragmentHeight.Enabled = usesWindow;
        numBlendQ.Visible = useManualQ;
        numBlendQ.Enabled = useManualQ;
        chkUseMultithreading.Enabled = isLocalMode;
        lblLocalProcessor.Enabled = isLocalMode;
        lblFragmentWidth.Enabled = usesWindow;
        lblFragmentHeight.Enabled = usesWindow;
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
        lblSourcePreview.Text = uiText.SourcePreviewTitle;
        lblPreviousPreview.Text = uiText.PreviousPreviewTitle;
        lblCurrentPreview.Text = uiText.CurrentPreviewTitle;
        chkUseMultithreading.Text = uiText.Multithreading;

        cmbLanguage.Items.Clear();
        cmbLanguage.Items.Add(uiText.RussianLanguage);
        cmbLanguage.Items.Add(uiText.EnglishLanguage);
        cmbLanguage.SelectedIndex = UiText.CurrentLanguage == UiLanguage.Russian ? 0 : 1;

        cmbProcessingMode.Items.Clear();
        cmbProcessingMode.Items.Add(uiText.GlobalMode);
        cmbProcessingMode.Items.Add(uiText.LocalMode);
        cmbProcessingMode.Items.Add(uiText.LocalMeanTvMode);
        cmbProcessingMode.SelectedIndex = selectedModeIndex;

        cmbLocalProcessor.Items.Clear();
        cmbLocalProcessor.Items.Add(uiText.Method1);
        cmbLocalProcessor.Items.Add(uiText.Method2);
        cmbLocalProcessor.Items.Add(uiText.Method3);
        cmbLocalProcessor.Items.Add(uiText.Method4);
        cmbLocalProcessor.SelectedIndex = selectedMethodIndex;

        UpdateParameterAvailability();
        UpdateProcessingInfo();
    }
}
