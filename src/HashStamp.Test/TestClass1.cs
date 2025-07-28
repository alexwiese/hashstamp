namespace HashStamp.Test
{
    internal class TestClass1
    {
        public string TestMethod1(string s = null)
        {
            return "Hello, World!";
        }

        public string TestMethod2(string s= null)
        {
            return "Hello, World!";
        }

        public string TestMethod3()
        {
            return "Hello, World 3!";
        }

        public string TestMethod3(string value)
        {
            return "Hello, World 3!";
        }
    }
}
