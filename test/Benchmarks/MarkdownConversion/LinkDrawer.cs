using System.Text;
using BenchmarkDotNet.Attributes;
using Panoramic.Data;

namespace Benchmarks.MarkdownConversion;

[SimpleJob]
[MemoryDiagnoser]
[HtmlExporter]
public class LinkDrawer
{
    private const string SampleFileName = "linkdrawer.md";

    private string? markdown;
    private LinkDrawerData? model;

    [GlobalSetup]
    public void Setup()
    {
        var mdFilePath = Path.Combine(Directory.GetCurrentDirectory(), Global.SamplesFolderName, SampleFileName);
        markdown = File.ReadAllText(mdFilePath);

        model = LinkDrawerData.FromMarkdown(SampleFileName, markdown!);
    }

    [Benchmark]
    public LinkDrawerData FromMarkdown() => LinkDrawerData.FromMarkdown(SampleFileName, markdown!);

    [Benchmark]
    public void ToMarkdown()
    {
        var builder = new StringBuilder();
        model!.ToMarkdown(builder);
    }
}
