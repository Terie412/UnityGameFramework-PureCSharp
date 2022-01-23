using System;

public class SingleTon<T> where T : class, new()
{
    private static readonly Lazy<T> lazy = new Lazy<T>(() =>  new T() );
    public static T Instance
    {
        get
        {
            return lazy.Value;
        }
    }
}