// See https://aka.ms/new-console-template for more information
using QuadrupleLib;

using StreamWriter writer = File.CreateText("cordic.csv");
writer.WriteLine("x,sin,cos");
for (int i = -90; i <= 90; i += 5) 
{
    (Float128 sin, Float128 cos) = Float128.SinCos(i * Float128.Pi / 180);
    writer.WriteLine($"{i},{sin.ToString()},{cos.ToString()}");
}
