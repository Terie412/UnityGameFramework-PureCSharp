using System;
using UnityEngine;

namespace Framework
{
	public class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
	{
		private static readonly Lazy<T> lazy = new(() =>
		{
			var instance = FindObjectOfType<T>();
			if (instance != null)
				return instance;

			instance = new GameObject().AddComponent<T>();
			instance.name = instance.GetType().Name;
			return instance;
		});

		public static T Instance => lazy.Value;
	}
}