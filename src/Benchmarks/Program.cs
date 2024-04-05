using BenchmarkDotNet.Running;
using Benchmarks.MarkdownConversion;

BenchmarkRunner.Run<Note>();
BenchmarkRunner.Run<LinkCollection>();
BenchmarkRunner.Run<RecentLinks>();
