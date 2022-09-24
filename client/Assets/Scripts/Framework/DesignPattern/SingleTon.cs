using System;

namespace Framework
{
	public class SingleTon<T> where T : class, new()
	{
		private static Lazy<T> lazy = new(() => new T());
		public static T Instance => lazy.Value;

		// ProjectSettings 的设置为 Reload Domain = false
		// 属于开放更多变数以提升开发效率的权衡手段
		// 参考：https://docs.unity3d.com/Manual/DomainReloading.html
		public static void InitSingleTonOnLoad()
		{
			lazy = new(() => new T());
		}
	}
}