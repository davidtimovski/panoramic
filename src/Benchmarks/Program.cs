using BenchmarkDotNet.Running;
using Benchmarks.MarkdownConversion;

var summary = BenchmarkRunner.Run<RecentLinks>();
