using System.Text;
using BenchmarkDotNet.Attributes;
using Panoramic.Data.Widgets;

namespace Benchmarks.MarkdownConversion;

[SimpleJob]
[MemoryDiagnoser]
[HtmlExporter]
public class Note
{
    private const string SampleFileName = "note.md";

    private string? markdown;
    private NoteData? model;

    [GlobalSetup]
    public void Setup()
    {
        var mdFilePath = Path.Combine(Directory.GetCurrentDirectory(), Global.SamplesFolderName, SampleFileName);
        markdown = File.ReadAllText(mdFilePath);

        model = NoteData.FromMarkdown(markdown!);
    }

    [Benchmark]
    public NoteData FromMarkdown() => NoteData.FromMarkdown(markdown!);

    [Benchmark]
    public void ToMarkdown()
    {
        var builder = new StringBuilder();
        model!.ToMarkdown(builder);
    }
}
