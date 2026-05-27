using System;
using System.Globalization;

namespace ImageContrastApp;

internal enum UiLanguage
{
    Russian,
    English
}

internal static class UiText
{
    private static UiLanguage currentLanguage = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals(
        "ru",
        StringComparison.OrdinalIgnoreCase)
            ? UiLanguage.Russian
            : UiLanguage.English;

    private static readonly UiTextSet English = new()
    {
        FormTitle = "Image Contrast Processor",
        LanguageLabel = "Language:",
        ThemeLabel = "Theme:",
        LightTheme = "Light",
        DarkTheme = "Dark",
        RussianLanguage = "Russian",
        EnglishLanguage = "English",
        LoadButton = "Load",
        ApplyButton = "Apply",
        SaveButton = "Save",
        ModeLabel = "Mode:",
        GlobalMode = "Global Contrast",
        LocalMode = "Local Fragment",
        LocalMeanTvMode = "TV + Local Mean",
        ContrastLabelGlobal = "\u03C3z (Global TV):",
        ContrastLabelLocal = "\u03C3z (Local):",
        LocalMethodLabel = "Method:",
        FragmentWidthLabel = "Frag W:",
        FragmentHeightLabel = "Frag H:",
        BlendQLabel = "q:",
        AdaptiveQLabel = "q = clamp(1 - \u03C3/80):",
        Multithreading = "Multithread",
        Method1 = "Global \u03C3",
        Method2 = "Local \u03C3",
        Method3 = "Manual q",
        Method4 = "Adaptive q",
        ImageFilesFilter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp",
        OpenImageTitle = "Select an image",
        SaveImageFilter = "PNG Image|*.png|JPEG Image|*.jpg;*.jpeg|BMP Image|*.bmp",
        SaveImageTitle = "Save processed image",
        SaveImageDefaultName = "processed-image.png",
        InfoCaption = "Info",
        NotAvailableCaption = "Not available",
        LoadImageFirst = "Load an image first.",
        NoImageToSave = "No processed image to save.",
        SourcePreviewTitle = "Source image",
        PreviousPreviewTitle = "Previous result",
        CurrentPreviewTitle = "Current result",
        NoSourceImage = "No image loaded.",
        NoPreviousImage = "No previous processed image.",
        NoCurrentImage = "No current processed image.",
        SourceImageInfoFormat = "Uploaded source: {0} x {1}px",
        PreviousImageInfoFormat = "{0}, applied at {1}",
        CurrentImageInfoFormat = "{0}, applied at {1}",
        GlobalDetailsFormat = "target sigma z = {0}",
        LocalDetailsFormat = "fragment method: {0}; target sigma z = {1}; window = {2} x {3}; q = {4}",
        LocalMeanTvDetailsFormat = "target sigma z = {0}; window = {1} x {2}",
        ProcessingTimeFormat = "processing time = {0:0.###} ms",
        BrightnessStatsFormat = "brightness avg={0:0.0}; brightness st.dev.={1:0.0}; range={2}-{3}",
        AdaptiveQInfo = "adaptive",
        NotUsedInfo = "not used"
    };

    private static readonly UiTextSet Russian = new()
    {
        FormTitle = "\u041E\u0431\u0440\u0430\u0431\u043E\u0442\u043A\u0430 \u043A\u043E\u043D\u0442\u0440\u0430\u0441\u0442\u0430 \u0438\u0437\u043E\u0431\u0440\u0430\u0436\u0435\u043D\u0438\u044F",
        LanguageLabel = "\u042F\u0437\u044B\u043A:",
        ThemeLabel = "\u0422\u0435\u043C\u0430:",
        LightTheme = "\u0421\u0432\u0435\u0442\u043B\u0430\u044F",
        DarkTheme = "\u0422\u0435\u043C\u043D\u0430\u044F",
        RussianLanguage = "\u0420\u0443\u0441\u0441\u043A\u0438\u0439",
        EnglishLanguage = "English",
        LoadButton = "\u0417\u0430\u0433\u0440\u0443\u0437\u0438\u0442\u044C",
        ApplyButton = "\u041F\u0440\u0438\u043C\u0435\u043D\u0438\u0442\u044C",
        SaveButton = "\u0421\u043E\u0445\u0440\u0430\u043D\u0438\u0442\u044C",
        ModeLabel = "\u0420\u0435\u0436\u0438\u043C:",
        GlobalMode = "\u0422\u0435\u043B\u0435\u0432\u0438\u0437\u0438\u043E\u043D\u043D\u044B\u0439",
        LocalMode = "\u041F\u043E \u0444\u0440\u0430\u0433\u043C\u0435\u043D\u0442\u0430\u043C",
        LocalMeanTvMode = "\u0422\u0435\u043B\u0435\u0432. + \u043B\u043E\u043A. \u0441\u0440\u0435\u0434\u043D.",
        ContrastLabelGlobal = "\u03C3z (\u0422\u0412):",
        ContrastLabelLocal = "\u03C3z (\u0444\u0440\u0430\u0433\u043C.):",
        LocalMethodLabel = "\u041C\u0435\u0442\u043E\u0434:",
        FragmentWidthLabel = "\u0428\u0438\u0440\u0438\u043D\u0430:",
        FragmentHeightLabel = "\u0412\u044B\u0441\u043E\u0442\u0430:",
        BlendQLabel = "q:",
        AdaptiveQLabel = "q = clamp(1 - \u03C3/80):",
        Multithreading = "\u041C\u043D\u043E\u0433\u043E\u043F\u043E\u0442\u043E\u043A",
        Method1 = "\u0413\u043B\u043E\u0431. \u03C3",
        Method2 = "\u041B\u043E\u043A. \u03C3",
        Method3 = "\u0420\u0443\u0447\u043D\u043E\u0439 q",
        Method4 = "\u0410\u0434\u0430\u043F\u0442. q",
        ImageFilesFilter = "\u0424\u0430\u0439\u043B\u044B \u0438\u0437\u043E\u0431\u0440\u0430\u0436\u0435\u043D\u0438\u0439|*.jpg;*.jpeg;*.png;*.bmp",
        OpenImageTitle = "\u0412\u044B\u0431\u0435\u0440\u0438\u0442\u0435 \u0438\u0437\u043E\u0431\u0440\u0430\u0436\u0435\u043D\u0438\u0435",
        SaveImageFilter = "PNG|*.png|JPEG|*.jpg;*.jpeg|BMP|*.bmp",
        SaveImageTitle = "\u0421\u043E\u0445\u0440\u0430\u043D\u0438\u0442\u044C \u043E\u0431\u0440\u0430\u0431\u043E\u0442\u0430\u043D\u043D\u043E\u0435 \u0438\u0437\u043E\u0431\u0440\u0430\u0436\u0435\u043D\u0438\u0435",
        SaveImageDefaultName = "processed-image.png",
        InfoCaption = "\u0418\u043D\u0444\u043E\u0440\u043C\u0430\u0446\u0438\u044F",
        NotAvailableCaption = "\u041D\u0435\u0434\u043E\u0441\u0442\u0443\u043F\u043D\u043E",
        LoadImageFirst = "\u0421\u043D\u0430\u0447\u0430\u043B\u0430 \u0437\u0430\u0433\u0440\u0443\u0437\u0438\u0442\u0435 \u0438\u0437\u043E\u0431\u0440\u0430\u0436\u0435\u043D\u0438\u0435.",
        NoImageToSave = "\u041D\u0435\u0442 \u043E\u0431\u0440\u0430\u0431\u043E\u0442\u0430\u043D\u043D\u043E\u0433\u043E \u0438\u0437\u043E\u0431\u0440\u0430\u0436\u0435\u043D\u0438\u044F \u0434\u043B\u044F \u0441\u043E\u0445\u0440\u0430\u043D\u0435\u043D\u0438\u044F.",
        SourcePreviewTitle = "\u0418\u0441\u0445\u043E\u0434\u043D\u043E\u0435",
        PreviousPreviewTitle = "\u041F\u0440\u0435\u0434\u044B\u0434\u0443\u0449\u0438\u0439 \u0440\u0435\u0437\u0443\u043B\u044C\u0442\u0430\u0442",
        CurrentPreviewTitle = "\u0422\u0435\u043A\u0443\u0449\u0438\u0439 \u0440\u0435\u0437\u0443\u043B\u044C\u0442\u0430\u0442",
        NoSourceImage = "\u0418\u0437\u043E\u0431\u0440\u0430\u0436\u0435\u043D\u0438\u0435 \u043D\u0435 \u0437\u0430\u0433\u0440\u0443\u0436\u0435\u043D\u043E.",
        NoPreviousImage = "\u041D\u0435\u0442 \u043F\u0440\u0435\u0434\u044B\u0434\u0443\u0449\u0435\u0433\u043E \u0440\u0435\u0437\u0443\u043B\u044C\u0442\u0430\u0442\u0430.",
        NoCurrentImage = "\u041D\u0435\u0442 \u0442\u0435\u043A\u0443\u0449\u0435\u0433\u043E \u0440\u0435\u0437\u0443\u043B\u044C\u0442\u0430\u0442\u0430.",
        SourceImageInfoFormat = "\u0418\u0441\u0445\u043E\u0434\u043D\u043E\u0435: {0} x {1}px",
        PreviousImageInfoFormat = "{0}, \u0432\u0440\u0435\u043C\u044F {1}",
        CurrentImageInfoFormat = "{0}, \u0432\u0440\u0435\u043C\u044F {1}",
        GlobalDetailsFormat = "\u0446\u0435\u043B\u0435\u0432\u043E\u0435 sigma z = {0}",
        LocalDetailsFormat = "\u043C\u0435\u0442\u043E\u0434 \u0444\u0440\u0430\u0433\u043C.: {0}; \u0446\u0435\u043B\u0435\u0432\u043E\u0435 sigma z = {1}; \u043E\u043A\u043D\u043E = {2} x {3}; q = {4}",
        LocalMeanTvDetailsFormat = "\u0446\u0435\u043B\u0435\u0432\u043E\u0435 sigma z = {0}; \u043E\u043A\u043D\u043E = {1} x {2}",
        ProcessingTimeFormat = "\u0432\u0440\u0435\u043C\u044F \u043E\u0431\u0440\u0430\u0431\u043E\u0442\u043A\u0438 = {0:0.###} \u043C\u0441",
        BrightnessStatsFormat = "\u044F\u0440\u043A. \u0441\u0440\u0435\u0434.={0:0.0}; \u0421\u041A\u041E \u044F\u0440\u043A.={1:0.0}; \u0434\u0438\u0430\u043F\u0430\u0437\u043E\u043D={2}-{3}",
        AdaptiveQInfo = "\u0430\u0434\u0430\u043F\u0442\u0438\u0432\u043D\u043E",
        NotUsedInfo = "\u043D\u0435 \u0438\u0441\u043F."
    };

    internal static UiLanguage CurrentLanguage
    {
        get => currentLanguage;
        set => currentLanguage = value;
    }

    internal static UiTextSet Current => currentLanguage == UiLanguage.Russian ? Russian : English;
}

internal sealed class UiTextSet
{
    internal required string FormTitle { get; init; }
    internal required string LanguageLabel { get; init; }
    internal required string ThemeLabel { get; init; }
    internal required string LightTheme { get; init; }
    internal required string DarkTheme { get; init; }
    internal required string RussianLanguage { get; init; }
    internal required string EnglishLanguage { get; init; }
    internal required string LoadButton { get; init; }
    internal required string ApplyButton { get; init; }
    internal required string SaveButton { get; init; }
    internal required string ModeLabel { get; init; }
    internal required string GlobalMode { get; init; }
    internal required string LocalMode { get; init; }
    internal required string LocalMeanTvMode { get; init; }
    internal required string ContrastLabelGlobal { get; init; }
    internal required string ContrastLabelLocal { get; init; }
    internal required string LocalMethodLabel { get; init; }
    internal required string FragmentWidthLabel { get; init; }
    internal required string FragmentHeightLabel { get; init; }
    internal required string BlendQLabel { get; init; }
    internal required string AdaptiveQLabel { get; init; }
    internal required string Multithreading { get; init; }
    internal required string Method1 { get; init; }
    internal required string Method2 { get; init; }
    internal required string Method3 { get; init; }
    internal required string Method4 { get; init; }
    internal required string ImageFilesFilter { get; init; }
    internal required string OpenImageTitle { get; init; }
    internal required string SaveImageFilter { get; init; }
    internal required string SaveImageTitle { get; init; }
    internal required string SaveImageDefaultName { get; init; }
    internal required string InfoCaption { get; init; }
    internal required string NotAvailableCaption { get; init; }
    internal required string LoadImageFirst { get; init; }
    internal required string NoImageToSave { get; init; }
    internal required string SourcePreviewTitle { get; init; }
    internal required string PreviousPreviewTitle { get; init; }
    internal required string CurrentPreviewTitle { get; init; }
    internal required string NoSourceImage { get; init; }
    internal required string NoPreviousImage { get; init; }
    internal required string NoCurrentImage { get; init; }
    internal required string SourceImageInfoFormat { get; init; }
    internal required string PreviousImageInfoFormat { get; init; }
    internal required string CurrentImageInfoFormat { get; init; }
    internal required string GlobalDetailsFormat { get; init; }
    internal required string LocalDetailsFormat { get; init; }
    internal required string LocalMeanTvDetailsFormat { get; init; }
    internal required string ProcessingTimeFormat { get; init; }
    internal required string BrightnessStatsFormat { get; init; }
    internal required string AdaptiveQInfo { get; init; }
    internal required string NotUsedInfo { get; init; }
}
