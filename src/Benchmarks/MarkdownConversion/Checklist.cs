using System.Text;
using BenchmarkDotNet.Attributes;
using Panoramic.Data.Widgets;

namespace Benchmarks.MarkdownConversion;

[SimpleJob]
[MemoryDiagnoser]
[HtmlExporter]
public class Checklist
{
    private const string SampleFileName = "checklist.md";

    private string? markdown;
    private ChecklistData? model;

    [GlobalSetup]
    public void Setup()
    {
        var mdFilePath = Path.Combine(Directory.GetCurrentDirectory(), Global.SamplesFolderName, SampleFileName);
        markdown = File.ReadAllText(mdFilePath);

        model = ChecklistData.FromMarkdown(markdown!);
    }

    [Benchmark]
    public ChecklistData FromMarkdown() => ChecklistData.FromMarkdown(markdown!);

    [Benchmark]
    public void ToMarkdown()
    {
        var builder = new StringBuilder();
        model!.ToMarkdown(builder);
    }
}
