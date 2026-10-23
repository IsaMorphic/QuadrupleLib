namespace QuadrupleLib.Annotations
{
    [AttributeUsage(
        AttributeTargets.Property | 
        AttributeTargets.Method | 
        AttributeTargets.Constructor)]
    internal class MakeNonGenericAttribute : Attribute
    {
    }
}
