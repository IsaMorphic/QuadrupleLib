// See https://aka.ms/new-console-template for more information
using QuadrupleLib;

var original = Float128.PI * Float128.PI;
var parsed = Float128.Parse(original.ToString());
Console.WriteLine(original);
Console.WriteLine(parsed);
Console.WriteLine(original == parsed);
