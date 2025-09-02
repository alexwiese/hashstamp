using BenchmarkDotNet.Running;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Json;
using BenchmarkDotNet.Loggers;
using System;

namespace HashStamp.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            // Configure BenchmarkDotNet
            var config = ManualConfig.Create(DefaultConfig.Instance)
                .AddExporter(JsonExporter.Brief) // Brief JSON export for CI workflows
                .AddLogger(ConsoleLogger.Default);

            if (args.Length > 0 && args[0] == "--quick")
            {
                // Quick mode for CI - reduced iterations
                Console.WriteLine("Running in quick mode for CI...");
                BenchmarkRunner.Run<QuickBenchmarks>(config);
            }
            else
            {
                Console.WriteLine("Running benchmark suite...");
                BenchmarkRunner.Run<QuickBenchmarks>(config);
            }
        }
    }
}
