using HashStamp.Test;
using HashStamp;

Console.WriteLine("Hello, World!");

new TestClass1().TestMethod2();

Console.WriteLine(HashStamps.HashStamp_Test.TestClass1.TestMethod1);
Console.WriteLine(HashStamps.HashStamp_Test.TestClass1.TestMethod2);
Console.WriteLine(HashStamps.HashStamp_Test.TestClass1.TestMethod3_String);
Console.WriteLine(HashStamps.HashStamp_Test.TestClass1.TestMethod3);
Console.WriteLine(HashStamps.HashStamp_Test.TestClass2.TestMethod3);
Console.WriteLine(HashStamps.HashStamp_Test_OtherNamespace.TestClass2.TestMethod3);

var a = HashStamps.Namespaces.ToList();
var x = HashStamps.Namespaces["HashStamp.Test"].Classes["TestClass1"].Methods["TestMethod1"].Hash;
var y = HashStamps.Namespaces["HashStamp.Test"].Classes["TestClass1"].Methods["TestMethod1"].Hash;