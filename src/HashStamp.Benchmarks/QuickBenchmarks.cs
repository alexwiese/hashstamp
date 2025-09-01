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
        public string[] GetAllHashesFromLargeClass()
        {
            var largeClass = HashStamps.Namespaces["HashStamp.Benchmarks.TestData"]
                .Classes["LargeTestClass"];
            
            return largeClass.Methods.Values.Select(m => m.Hash).ToArray();
        }

        [Benchmark]
        public string CompileTimeAccessLargeClass()
        {
            // Access multiple hashes from compile-time constants
            return HashStamps.HashStamp_Benchmarks_TestData.LargeTestClass.Method1 +
                   HashStamps.HashStamp_Benchmarks_TestData.LargeTestClass.Method10 +
                   HashStamps.HashStamp_Benchmarks_TestData.LargeTestClass.IntMethod5 +
                   HashStamps.HashStamp_Benchmarks_TestData.LargeTestClass.BoolMethod3;
        }

        [Benchmark]
        public string RuntimeAccessLargeClass()
        {
            // Access same hashes via runtime lookup
            var largeClass = HashStamps.Namespaces["HashStamp.Benchmarks.TestData"]
                .Classes["LargeTestClass"];
                
            return largeClass.Methods["Method1"].Hash +
                   largeClass.Methods["Method10"].Hash +
                   largeClass.Methods["IntMethod5"].Hash +
                   largeClass.Methods["BoolMethod3"].Hash;
        }

        [Benchmark]
        public bool NamespaceExists()
        {
            return HashStamps.Namespaces.ContainsKey("HashStamp.Benchmarks.TestData");
        }

        [Benchmark]
        public string[] GetAllNamespaces()
        {
            return HashStamps.Namespaces.Keys.ToArray();
        }
    }
}
