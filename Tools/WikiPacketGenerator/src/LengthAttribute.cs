using System;

public class LengthAttribute : Attribute
{
    public int? Length = null;

    public LengthAttribute()
    {
    }

    public LengthAttribute(int length)
    {
        this.Length = length;
    }
}