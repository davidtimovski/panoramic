using BenchmarkDotNet.Running;
using Benchmarks.MarkdownConversion;

BenchmarkRunner.Run<LinkCollection>();
BenchmarkRunner.Run<RecentLinks>();
