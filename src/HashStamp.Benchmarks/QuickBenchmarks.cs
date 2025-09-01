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

        [Benchmark]
        public string[] GetAllHashesFromSingleClass()
        {
            var testClass = HashStamps.Namespaces["HashStamp.Benchmarks.TestData"]
                .Classes["BenchmarkTestClass"];

            return testClass.Methods.Values.Select(m => m.Hash).ToArray();
        }

        [Benchmark]
        public bool ContainsSpecificMethod()
        {
            return HashStamps.Namespaces["HashStamp.Benchmarks.TestData"]
                .Classes["BenchmarkTestClass"]
                .Methods.ContainsKey("SimpleMethod");
        }

        [Benchmark]
        public string GetMultipleHashes()
        {
            var benchmarkClass = HashStamps.Namespaces["HashStamp.Benchmarks.TestData"]
                .Classes["BenchmarkTestClass"];

            return benchmarkClass.Methods["SimpleMethod"].Hash +
                   benchmarkClass.Methods["MethodWithLoops"].Hash +
                   benchmarkClass.Methods["ComplexMethod"].Hash;
        }
    }
}
