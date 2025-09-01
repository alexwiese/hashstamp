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
        public string LargeScaleCompileTimeAccess()
        {
            return HashStamps.HashStamp_Benchmarks_TestData_LargeScale.BenchmarkBusinessClass1.ProcessTransaction;
        }

        [Benchmark]
        public string LargeScaleRuntimeAccess()
        {
            return HashStamps.Namespaces["HashStamp.Benchmarks.TestData.LargeScale"]
                .Classes["BenchmarkBusinessClass1"]
                .Methods["ProcessTransaction"]
                .Hash;
        }

        [Benchmark]
        public int CountLargeScaleMethods()
        {
            return HashStamps.Namespaces
                .Where(ns => ns.Key.Contains("LargeScale"))
                .SelectMany(ns => ns.Value.Classes)
                .SelectMany(cls => cls.Value.Methods)
                .Count();
        }

        [Benchmark]
        public int CountNamespaces()
        {
            return HashStamps.Namespaces.Count();
        }

        [Benchmark]
        public int CountClasses()
        {
            return HashStamps.Namespaces
                .SelectMany(ns => ns.Value.Classes)
                .Count();
        }
    }
}
