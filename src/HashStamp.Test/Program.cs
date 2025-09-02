using HashStamp.Test;
using HashStamp.Test.LargeScale;
using HashStamp;

Console.WriteLine("Hello, World!");

new TestClass1().TestMethod2();

// Test existing classes
Console.WriteLine(HashStamps.HashStamp_Test.TestClass1.TestMethod1);
Console.WriteLine(HashStamps.HashStamp_Test.TestClass1.TestMethod2);
Console.WriteLine(HashStamps.HashStamp_Test.TestClass1.TestMethod3_String);
Console.WriteLine(HashStamps.HashStamp_Test.TestClass1.TestMethod3);
Console.WriteLine(HashStamps.HashStamp_Test.TestClass2.TestMethod3);
Console.WriteLine(HashStamps.HashStamp_Test_OtherNamespace.TestClass2.TestMethod3);

// Test some large-scale classes (compile-time access)
Console.WriteLine(HashStamps.HashStamp_Test_LargeScale.BusinessLogicClass1.ProcessOrder);
Console.WriteLine(HashStamps.HashStamp_Test_LargeScale.DataAccessClass1.GetCustomerById);
Console.WriteLine(HashStamps.HashStamp_Test_LargeScale_Module2.BusinessLogicClass2.ProcessRefund);
Console.WriteLine(HashStamps.HashStamp_Test_LargeScale_Module3.ServiceClass3.GenerateQRCode);
Console.WriteLine(HashStamps.HashStamp_Test_LargeScale_Module4.ValidationClass4.ValidateJsonSchema);

// Test runtime access
var a = HashStamps.Namespaces.ToList();
var x = HashStamps.Namespaces["HashStamp.Test"].Classes["TestClass1"].Methods["TestMethod1"].Hash;
var y = HashStamps.Namespaces["HashStamp.Test"].Classes["TestClass1"].Methods["TestMethod1"].Hash;

// Test runtime access for large-scale classes
var largeScaleHash = HashStamps.Namespaces["HashStamp.Test.LargeScale"].Classes["BusinessLogicClass1"].Methods["ProcessOrder"].Hash;
var moduleHash = HashStamps.Namespaces["HashStamp.Test.LargeScale.Module2"].Classes["DataAccessClass2"].Methods["GetProductById"].Hash;

Console.WriteLine($"Large scale hash: {largeScaleHash}");
Console.WriteLine($"Module hash: {moduleHash}");

// Display total method count
var totalMethods = HashStamps.Namespaces
    .SelectMany(ns => ns.Value.Classes)
    .SelectMany(cls => cls.Value.Methods)
    .Count();

Console.WriteLine($"Total methods found: {totalMethods}");