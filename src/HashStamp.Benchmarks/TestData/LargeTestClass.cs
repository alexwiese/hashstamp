using System;

namespace HashStamp.Benchmarks.TestData
{
    public class LargeTestClass
    {
        public string Method1() => "Method 1 implementation";
        public string Method2() => "Method 2 implementation";
        public string Method3() => "Method 3 implementation";
        public string Method4() => "Method 4 implementation";
        public string Method5() => "Method 5 implementation";
        public string Method6() => "Method 6 implementation";
        public string Method7() => "Method 7 implementation";
        public string Method8() => "Method 8 implementation";
        public string Method9() => "Method 9 implementation";
        public string Method10() => "Method 10 implementation";
        
        public int IntMethod1() { return 1 + 2 + 3; }
        public int IntMethod2() { return 4 + 5 + 6; }
        public int IntMethod3() { return 7 + 8 + 9; }
        public int IntMethod4() { return 10 + 11 + 12; }
        public int IntMethod5() { return 13 + 14 + 15; }
        
        public void VoidMethod1() { Console.WriteLine("Void method 1"); }
        public void VoidMethod2() { Console.WriteLine("Void method 2"); }
        public void VoidMethod3() { Console.WriteLine("Void method 3"); }
        public void VoidMethod4() { Console.WriteLine("Void method 4"); }
        public void VoidMethod5() { Console.WriteLine("Void method 5"); }
        
        public bool BoolMethod1() => true && false;
        public bool BoolMethod2() => false || true;
        public bool BoolMethod3() => !false;
        public bool BoolMethod4() => true == false;
        public bool BoolMethod5() => false != true;
    }
}