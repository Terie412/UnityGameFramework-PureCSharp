using System;

public class SingleTon<T> where T : class, new()
{
	private static readonly Lazy<T> lazy = new(() =>  new T());
	public static T Instance => lazy.Value;
}