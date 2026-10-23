namespace QuadrupleLib.Annotations
{
    [AttributeUsage(
        AttributeTargets.Field | 
        AttributeTargets.Property | 
        AttributeTargets.Method | 
        AttributeTargets.Constructor)]
    internal class MakeNonGenericAttribute : Attribute
    {
    }
}
