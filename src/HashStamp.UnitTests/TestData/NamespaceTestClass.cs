namespace HashStamp.UnitTests.TestData.AlternateNamespace
{
    public class NamespaceTestClass
    {
        public string NamespaceMethod()
        {
            return "Different namespace";
        }

        public void AnotherNamespaceMethod()
        {
            var result = "Testing namespaces";
            System.Console.WriteLine(result);
        }
    }
}
