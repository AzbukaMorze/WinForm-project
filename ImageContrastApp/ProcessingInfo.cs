using System;

namespace ImageContrastApp;

internal sealed record ProcessingInfo(string MethodName, string Details, DateTime AppliedAt, TimeSpan ElapsedTime);
