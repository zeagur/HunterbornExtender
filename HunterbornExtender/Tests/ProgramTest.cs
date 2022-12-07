namespace HunterbornExtender.Tests;
using HunterbornExtender;


sealed public class HunterbornProgramTesting
{
    public void TestTokenizeName()
    {
        string?[] testStrings = new string?[]
        {
            "thisIsCamelCase",
            "ThisIsTitleCase",
            "This is a sentence with some TitleCase and camelCase in it.",
            "Let's try ch@racters and _under_lines and -dashes-for-fun- and allM1x3d|_|p!",
            null,
            ""
        };

        HashSet<string>[] actual = testStrings.Select(Tokenizer.Tokenize).ToArray();

        HashSet<string>[] expected = new HashSet<string>[]
        {
            new() { "this", "Is", "Camel", "Case" },
            new() { "This", "Is", "Title", "Case" },
            new() { "This", "is", "a", "sentence", "with", "some", "Title", "Case", "and", "camel", "Case", "in", "it" },
            new() { "Let", "s", "try", "chracters", "and", "under", "lines", "and", "dashes", "for", "fun", "and", "all", "M1x3d", "p" },
            new() { },
            new() { }
        };

        //Assert.AreEqual(expected, actual);
    }

    public void TestTokenizeNames()
    {
        string?[] testStrings = new string?[]
        {
            "hello world",
            "goodbye world",
            "",
            null,
            "hello and goodbye"
        };

        HashSet<string> actual = Tokenizer.Tokenize(testStrings);
        HashSet<string> expected = new() { "hello", "goodbye", "and", "world" };

        //Assert.AreEqual(expected, actual);
    }

}
