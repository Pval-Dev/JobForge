namespace JobForge.Common;

using System.Diagnostics.CodeAnalysis;

public enum ArgumentError
{
    Null,
    Empty,
    WhiteSpace,
    TooLong,
    ControlCharacters,
    LeadingOrTrailingWhitespace
}

public static class StringValidator
{
    private const int DefaultMaxLength = 256;

    [DoesNotReturn]
    public static void SetException(ArgumentError error)
    {
        string message = error switch
        {
            ArgumentError.Null => "Value cannot be null.",
            ArgumentError.Empty => "Value cannot be empty.",
            ArgumentError.WhiteSpace => "Value cannot be whitespace.",
            ArgumentError.TooLong =>
                $"Value cannot exceed {DefaultMaxLength} characters.",
            ArgumentError.ControlCharacters =>
                "Value cannot contain control characters.",
            ArgumentError.LeadingOrTrailingWhitespace =>
                "Value cannot start or end with whitespace.",
            _ => "Invalid value."
        };

        throw new ArgumentException(message);
    }

    public static void Validate(string value)
    {
        if (value is null)
            SetException(ArgumentError.Null);

        if (value == string.Empty)
            SetException(ArgumentError.Empty);

        if (string.IsNullOrWhiteSpace(value))
            SetException(ArgumentError.WhiteSpace);

        if (value.Length > DefaultMaxLength)
            SetException(ArgumentError.TooLong);

        foreach (char character in value)
        {
            if (char.IsControl(character))
                SetException(ArgumentError.ControlCharacters);
        }

        if (value != value.Trim())
            SetException(ArgumentError.LeadingOrTrailingWhitespace);
    }
}