using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;

namespace D2SLib_Benchmark;

internal class BenchmarkConfig : ManualConfig
{
    public BenchmarkConfig()
    {
        AddJob(Job.Default
            .WithRuntime(CoreRuntime.Core10_0)
            .WithPlatform(Platform.X64)
            .WithJit(Jit.RyuJit));
        AddDiagnoser(MemoryDiagnoser.Default);
        //AddExporter(CsvMeasurementsExporter.Default);
        //AddExporter(HtmlExporter.Default);
        AddExporter(MarkdownExporter.GitHub);
        AddLogger(ConsoleLogger.Default);
    }
}