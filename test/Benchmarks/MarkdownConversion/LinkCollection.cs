using System.Text;
using BenchmarkDotNet.Attributes;
using Panoramic.Data.Widgets;

namespace Benchmarks.MarkdownConversion;

[SimpleJob]
[MemoryDiagnoser]
[HtmlExporter]
public class LinkCollection
{
    private const string SampleFileName = "linkcollection.md";

    private string? markdown;
    private LinkCollectionData? model;

    [GlobalSetup]
    public void Setup()
    {
        var mdFilePath = Path.Combine(Directory.GetCurrentDirectory(), Global.SamplesFolderName, SampleFileName);
        markdown = File.ReadAllText(mdFilePath);

        model = LinkCollectionData.FromMarkdown(markdown!);
    }

    [Benchmark]
    public LinkCollectionData FromMarkdown() => LinkCollectionData.FromMarkdown(markdown!);

    [Benchmark]
    public void ToMarkdown()
    {
        var builder = new StringBuilder();
        model!.ToMarkdown(builder);
    }
}
