using System;
using System.Text;

namespace Framework
{
    public static class AssetHelper
    {
        // 将目录路径转化成AssetBundle名称，规则是从第一个Assets目录开始（不包含）
        public static string DirectoryPathToAssetBundleName(string dirPath)
        {
            dirPath = dirPath.ToLower().Replace("\\", "/").TrimStart('/');
            var dirs        = dirPath.Split('/');
            var sb          = new StringBuilder();
            var assetsIndex = Int32.MaxValue;
            for (var i = 0; i < dirs.Length; i++)
            {
                var dir = dirs[i];
                if (i < assetsIndex && dir.Equals("assets"))
                {
                    assetsIndex = i;
                }
                else if (i > assetsIndex)
                {
                    sb.Append(dir).Append("_");
                }
            }

            var assetBundleName = sb.ToString().TrimEnd('_');
            return assetBundleName;
        }
    }
}