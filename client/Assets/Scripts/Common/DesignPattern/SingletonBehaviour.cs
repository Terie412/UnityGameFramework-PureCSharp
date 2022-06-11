using System;
using UnityEngine;
using Object = UnityEngine.Object;

public class SingletonBehaviour<T>: MonoBehaviour where T: SingletonBehaviour<T>
{
	private static readonly Lazy<T> lazy = new Lazy<T>(() =>
	{
		var instance = Object.FindObjectOfType<T>();
		if (instance == null)
		{
			instance = new GameObject().AddComponent<T>();
			instance.name = instance.GetType().Name;
		}

		return instance;
	});
	
	public static T Instance => lazy.Value;
}