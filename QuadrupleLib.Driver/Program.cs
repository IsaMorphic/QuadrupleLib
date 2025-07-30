// See https://aka.ms/new-console-template for more information
using QuadrupleLib;

using (StreamWriter writer = File.CreateText("cordic.csv"))
{
    writer.WriteLine("x,sin(x),cos(x)");
    for (int i = -90; i <= 90; i += 5)
    {
        (Float128 sin, Float128 cos) = Float128.SinCos(i * Float128.Pi / 180);
        writer.WriteLine($"{i},{sin},{cos}");
    }
}

using (StreamWriter writer = File.CreateText("logarithms.csv")) 
{
    writer.WriteLine("x,log2(x)");
    for(int i = 1; i <= 64; i++) 
    {
        writer.WriteLine($"{i},{Float128.Log2(i)}");
    }
}

using (StreamWriter writer = File.CreateText("exp.csv"))
{
    writer.WriteLine("x,exp(x)");
    for (int i = 1; i <= 64; i++)
    {
        writer.WriteLine($"{i / 4.0},{Float128.Exp(i / 4.0)}");
    }
}
