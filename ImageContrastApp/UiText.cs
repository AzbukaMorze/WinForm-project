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
    private static UiLanguage currentLanguage = UiLanguage.Russian;

    private static readonly UiTextSet English = new()
    {
        FormTitle = "Image Contrast Processor",
        LanguageLabel = "Language:",
        RussianLanguage = "Russian",
        EnglishLanguage = "English",
        LoadButton = "Load",
        ApplyButton = "Apply",
        SaveButton = "Save",
        ModeLabel = "Mode:",
        GlobalMode = "Global Contrast",
        LocalMode = "Local Fragment",
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
        NoImageToSave = "No image to save."
    };

    private static readonly UiTextSet Russian = new()
    {
        FormTitle = "\u041E\u0431\u0440\u0430\u0431\u043E\u0442\u043A\u0430 \u043A\u043E\u043D\u0442\u0440\u0430\u0441\u0442\u0430 \u0438\u0437\u043E\u0431\u0440\u0430\u0436\u0435\u043D\u0438\u044F",
        LanguageLabel = "\u042F\u0437\u044B\u043A:",
        RussianLanguage = "\u0420\u0443\u0441\u0441\u043A\u0438\u0439",
        EnglishLanguage = "English",
        LoadButton = "\u0417\u0430\u0433\u0440\u0443\u0437\u0438\u0442\u044C",
        ApplyButton = "\u041F\u0440\u0438\u043C\u0435\u043D\u0438\u0442\u044C",
        SaveButton = "\u0421\u043E\u0445\u0440\u0430\u043D\u0438\u0442\u044C",
        ModeLabel = "\u0420\u0435\u0436\u0438\u043C:",
        GlobalMode = "\u0413\u043B\u043E\u0431\u0430\u043B\u044C\u043D\u044B\u0439",
        LocalMode = "\u041B\u043E\u043A\u0430\u043B\u044C\u043D\u044B\u0439",
        ContrastLabelGlobal = "\u03C3z (\u0433\u043B\u043E\u0431.):",
        ContrastLabelLocal = "\u03C3z (\u043B\u043E\u043A.):",
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
        NoImageToSave = "\u041D\u0435\u0442 \u0438\u0437\u043E\u0431\u0440\u0430\u0436\u0435\u043D\u0438\u044F \u0434\u043B\u044F \u0441\u043E\u0445\u0440\u0430\u043D\u0435\u043D\u0438\u044F."
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
    internal required string RussianLanguage { get; init; }
    internal required string EnglishLanguage { get; init; }
    internal required string LoadButton { get; init; }
    internal required string ApplyButton { get; init; }
    internal required string SaveButton { get; init; }
    internal required string ModeLabel { get; init; }
    internal required string GlobalMode { get; init; }
    internal required string LocalMode { get; init; }
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
}
