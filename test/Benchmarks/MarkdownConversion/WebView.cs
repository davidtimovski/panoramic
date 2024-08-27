using System.Text;
using BenchmarkDotNet.Attributes;
using Panoramic.Data.Widgets;

namespace Benchmarks.MarkdownConversion;

[SimpleJob]
[MemoryDiagnoser]
[HtmlExporter]
public class WebView
{
    private const string SampleFileName = "webview.md";

    private string? markdown;
    private WebViewData? model;

    [GlobalSetup]
    public void Setup()
    {
        var mdFilePath = Path.Combine(Directory.GetCurrentDirectory(), Global.SamplesFolderName, SampleFileName);
        markdown = File.ReadAllText(mdFilePath);

        model = WebViewData.FromMarkdown(SampleFileName, markdown!);
    }

    [Benchmark]
    public WebViewData FromMarkdown() => WebViewData.FromMarkdown(SampleFileName, markdown!);

    [Benchmark]
    public void ToMarkdown()
    {
        var builder = new StringBuilder();
        model!.ToMarkdown(builder);
    }
}
