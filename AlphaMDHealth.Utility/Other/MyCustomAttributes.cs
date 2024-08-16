namespace AlphaMDHealth.Utility;

[AttributeUsage(AttributeTargets.Property)]
public class MyCustomAttributes : Attribute
{
    public string TheResourceKey;

    public MyCustomAttributes(string resourceKey)
    {
        this.TheResourceKey = resourceKey;
    }
}