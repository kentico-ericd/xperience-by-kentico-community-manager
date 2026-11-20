using System.Text;

namespace XperienceCommunity.Manager.Helpers;

/// <summary>
/// Quotes values for safe use in the scripts that <see cref="Services.ShellRunner"/> passes to Windows PowerShell.
/// </summary>
public static class PowerShellQuoteHelper
{
    // PowerShell treats the typographic single quotes as quote characters too.
    private static readonly char[] singleQuoteChars = ['\'', '‘', '’', '‚', '‛'];


    /// <summary>
    /// Returns <paramref name="value"/> as a PowerShell single-quoted string literal, so that <c>$</c>, backticks and other
    /// special characters aren't interpreted. Use this for arguments to PowerShell cmdlets.
    /// </summary>
    public static string QuoteLiteral(string value)
    {
        var builder = new StringBuilder(value.Length + 2);
        builder.Append('\'');
        foreach (char c in value)
        {
            if (singleQuoteChars.Contains(c))
            {
                // A quote character is escaped by doubling it
                builder.Append(c);
            }

            builder.Append(c);
        }

        builder.Append('\'');

        return builder.ToString();
    }


    /// <summary>
    /// Returns <paramref name="value"/> quoted so that a native executable (e.g. <c>dotnet</c>) launched from Windows
    /// PowerShell receives it unchanged as a single argument.
    /// </summary>
    /// <remarks>
    /// Windows PowerShell 5.1 passes arguments to native executables without escaping embedded double quotes or trailing
    /// backslashes, so the value is first escaped using the Windows command-line parsing rules, then wrapped as a
    /// PowerShell single-quoted literal.
    /// </remarks>
    public static string QuoteNativeArgument(string value)
    {
        var builder = new StringBuilder(value.Length + 2);
        int backslashes = 0;
        foreach (char c in value)
        {
            if (c == '\\')
            {
                backslashes++;
                continue;
            }

            if (c == '"')
            {
                // Backslashes before a quote must be doubled, and the quote itself escaped
                builder.Append('\\', (backslashes * 2) + 1);
            }
            else
            {
                builder.Append('\\', backslashes);
            }

            builder.Append(c);
            backslashes = 0;
        }

        // PowerShell wraps arguments containing whitespace in double quotes, so trailing backslashes must be doubled to
        // avoid escaping the closing quote
        bool isWrapped = value.Any(char.IsWhiteSpace);
        builder.Append('\\', isWrapped ? backslashes * 2 : backslashes);

        return QuoteLiteral(builder.ToString());
    }
}
