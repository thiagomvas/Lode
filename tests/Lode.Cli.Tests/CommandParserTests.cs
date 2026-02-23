using System.Collections.Generic;
using NUnit.Framework;
using Lode.Cli;
using Lode.Tests.Common;
using NUnit.Framework.Legacy;

namespace Lode.Cli.Tests;

[TestFixture]
[Category(TestCategories.Unit)]
[Category(TestCategories.Cli)]
public class CommandParserTests
{
    [Test]
    public void Parse_EmptyInput_ReturnsNull()
    {
        var result = CommandParser.Parse("");
        Assert.That(result.Command, Is.Null);
        Assert.That(result.Context, Is.Null);
    }

    [Test]
    public void Parse_WhitespaceInput_ReturnsNull()
    {
        var result = CommandParser.Parse("   ");
        Assert.That(result.Command, Is.Null);
        Assert.That(result.Context, Is.Null);
    }

    [Test]
    public void Parse_CommandOnly_ReturnsCommandWithEmptyContext()
    {
        var result = CommandParser.Parse("run");
        Assert.That(result.Command, Is.EqualTo("run"));
        Assert.That(result.Context, Is.Not.Null);
        Assert.That(result.Context.Args, Is.Empty);
        Assert.That(result.Context.Options, Is.Empty);
    }

    [Test]
    public void Parse_CommandWithPositionalArgs_ReturnsArgs()
    {
        var result = CommandParser.Parse("run file1 file2");
        Assert.That(result.Command, Is.EqualTo("run"));
        Assert.That(result.Context.Args, Is.EquivalentTo(new List<string> { "file1", "file2" }));
        Assert.That(result.Context.Options, Is.Empty);
    }

    [Test]
    public void Parse_CommandWithSimpleOptionFlag_ReturnsOptionWithTrue()
    {
        var result = CommandParser.Parse("run -v");
        Assert.That(result.Command, Is.EqualTo("run"));
        Assert.That(result.Context.Options["v"], Is.EqualTo("true"));
        Assert.That(result.Context.Args, Is.Empty);
    }

    [Test]
    public void Parse_CommandWithOptionAndValue_ReturnsOptionWithValue()
    {
        var result = CommandParser.Parse("run -o output.txt");
        Assert.That(result.Command, Is.EqualTo("run"));
        Assert.That(result.Context.Options["o"], Is.EqualTo("output.txt"));
        Assert.That(result.Context.Args, Is.Empty);
    }

    [Test]
    public void Parse_CommandWithEqualsOption_ReturnsOptionWithValue()
    {
        var result = CommandParser.Parse("run --file=input.txt");
        Assert.That(result.Command, Is.EqualTo("run"));
        Assert.That(result.Context.Options["file"], Is.EqualTo("input.txt"));
        Assert.That(result.Context.Args, Is.Empty);
    }

    [Test]
    public void Parse_CommandWithQuotedArg_ReturnsUnquotedArg()
    {
        var result = CommandParser.Parse("run \"file name.txt\"");
        Assert.That(result.Context.Args, Is.EquivalentTo(new List<string> { "file name.txt" }));
        Assert.That(result.Context.Options, Is.Empty);
    }

    [Test]
    public void Parse_CommandWithMixedArgsAndOptions_ReturnsCorrectly()
    {
        var result = CommandParser.Parse("run file1 -v -o \"out file.txt\" file2");
        Assert.That(result.Command, Is.EqualTo("run"));
        Assert.That(result.Context.Args, Is.EquivalentTo(new List<string> { "file1", "file2" }));
        Assert.That(result.Context.Options["v"], Is.EqualTo("true"));
        Assert.That(result.Context.Options["o"], Is.EqualTo("out file.txt"));
    }

    [Test]
    public void Parse_CommandWithEscapedQuoteInArg_ReturnsCorrectArg()
    {
        var result = CommandParser.Parse("run \"file\\\"name.txt\"");
        Assert.That(result.Context.Args, Is.EquivalentTo(new List<string> { "file\"name.txt" }));
    }

    [Test]
    public void Parse_CommandWithMultipleEqualsOptions_ReturnsAllOptions()
    {
        var result = CommandParser.Parse("run --a=1 --b=2");
        Assert.That(result.Context.Options["a"], Is.EqualTo("1"));
        Assert.That(result.Context.Options["b"], Is.EqualTo("2"));
    }
}