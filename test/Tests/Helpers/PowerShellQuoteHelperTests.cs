using NUnit.Framework;

using XperienceCommunity.Manager.Helpers;

namespace XperienceCommunity.Manager.Tests.Tests.Helpers;

/// <summary>
/// Tests for <see cref="PowerShellQuoteHelper"/>. Expected values were verified by running the quoted values through
/// Windows PowerShell 5.1 and checking the arguments received by a native executable.
/// </summary>
public class PowerShellQuoteHelperTests
{
    [TestCase("Ab3$xyzQ!1", "'Ab3$xyzQ!1'")]
    [TestCase("p$?q", "'p$?q'")]
    [TestCase("x`y", "'x`y'")]
    [TestCase("$(Get-Date)", "'$(Get-Date)'")]
    [TestCase("a'b", "'a''b'")]
    [TestCase("a\u2019b", "'a\u2019\u2019b'")]
    [TestCase("a\"b", "'a\"b'")]
    [TestCase(@"C:\my dir\", @"'C:\my dir\'")]
    public void QuoteLiteral_EscapesValue(string value, string expected) =>
        Assert.That(PowerShellQuoteHelper.QuoteLiteral(value), Is.EqualTo(expected));


    [TestCase("Ab3$xyzQ!1", "'Ab3$xyzQ!1'")]
    [TestCase("a b", "'a b'")]
    [TestCase("a'b", "'a''b'")]
    [TestCase("a\"b", "'a\\\"b'")]
    [TestCase("a b\"c", "'a b\\\"c'")]
    [TestCase("end\\\"x", "'end\\\\\\\"x'")]
    [TestCase(@"C:\nodir\", @"'C:\nodir\'")]
    [TestCase(@"C:\my dir\", @"'C:\my dir\\'")]
    [TestCase(@"\\server\share", @"'\\server\share'")]
    public void QuoteNativeArgument_EscapesValue(string value, string expected) =>
        Assert.That(PowerShellQuoteHelper.QuoteNativeArgument(value), Is.EqualTo(expected));
}
