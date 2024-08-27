using BenchmarkDotNet.Running;
using Benchmarks.MarkdownConversion;

BenchmarkRunner.Run<Note>();
BenchmarkRunner.Run<LinkCollection>();
BenchmarkRunner.Run<RecentLinks>();
BenchmarkRunner.Run<Checklist>();
BenchmarkRunner.Run<WebView>();
