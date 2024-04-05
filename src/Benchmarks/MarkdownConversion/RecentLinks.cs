using System.Text;
using BenchmarkDotNet.Attributes;
using Panoramic.Data.Widgets;

namespace Benchmarks.MarkdownConversion;

[SimpleJob]
[MemoryDiagnoser]
[HtmlExporter]
public class RecentLinks
{
    private const string SampleFileName = "recentlinks.md";

    private string? markdown;
    private RecentLinksData? model;

    [GlobalSetup]
    public void Setup()
    {
        var mdFilePath = Path.Combine(Directory.GetCurrentDirectory(), Global.SamplesFolderName, SampleFileName);
        markdown = File.ReadAllText(mdFilePath);

        model = RecentLinksData.FromMarkdown(markdown!);
    }

    [Benchmark]
    public RecentLinksData FromMarkdown() => RecentLinksData.FromMarkdown(markdown!);

    [Benchmark]
    public void ToMarkdown()
    {
        var builder = new StringBuilder();
        model!.ToMarkdown(builder);
    }
}
