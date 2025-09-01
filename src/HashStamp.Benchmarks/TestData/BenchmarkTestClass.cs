namespace HashStamp.Benchmarks.TestData
{
    public class BenchmarkTestClass
    {
        public string SimpleMethod()
        {
            return "Hello, World!";
        }

        public string MethodWithLoops()
        {
            var result = "";
            for (int i = 0; i < 10; i++)
            {
                result += $"Iteration {i} ";
            }
            return result;
        }

        public int ComplexMethod()
        {
            var sum = 0;
            var values = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            foreach (var value in values)
            {
                sum += value * value;
            }
            return sum;
        }

        public void VoidMethod()
        {
            // Empty implementation
        }
    }
}
