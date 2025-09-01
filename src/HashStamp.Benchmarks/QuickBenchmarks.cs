using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using System.Linq;

namespace HashStamp.Benchmarks
{
    [SimpleJob(RunStrategy.ColdStart)]
    [MemoryDiagnoser]
    [MarkdownExporter]
    public class QuickBenchmarks
    {
        [Benchmark(Baseline = true)]
        public string CompileTimeHashAccess()
        {
            return HashStamps.HashStamp_Benchmarks_TestData.BenchmarkTestClass.SimpleMethod;
        }

        [Benchmark]
        public string RuntimeHashAccess()
        {
            return HashStamps.Namespaces["HashStamp.Benchmarks.TestData"]
                .Classes["BenchmarkTestClass"]
                .Methods["SimpleMethod"]
                .Hash;
        }

        [Benchmark]
        public int CountAllMethods()
        {
            return HashStamps.Namespaces
                .SelectMany(ns => ns.Value.Classes)
                .SelectMany(cls => cls.Value.Methods)
                .Count();
        }
    }
}
