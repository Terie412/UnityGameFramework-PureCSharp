using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

// 资产导入的规则：
// 1. Content 下资源，除了少数目录以外，一律小写，包括扩展名。原因是很多版本管理软件对大小写支持不好，比如GitHub，P4，同时Windows系统也不区分大小写
// 这会造成很多问题，一个问题是，你可以无法在windows下创建两个除了大小之外完全同名的文件，但是却可以想办法把他们都上传到仓库上去
// 然后拉取的时候你会发现始终有一个文件无法拉取下来，而你并不能保证你拉取到的文件是你想要的
// 另外也是减少美术同学在命名规则上的关注度

// 资产导入的后处理
public class MyAssetPostProcessor : AssetPostprocessor
{
    private static readonly List<string> lowerCaseCheckExcludeFiles = new List<string>
    {
        "Assets/Content/Prefabs/UI", // UI 的prefab 目录基本都是由程序首先创建，所以犯错的可能性比较小，而且我们需要有UI命名跟相应的全局Lua对象同名的需求
    };

    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        for (var i = 0; i < importedAssets.Length; i++)
        {
            CheckLowerCase(importedAssets[i]);
        }

        for (int i = 0; i < movedAssets.Length; i++)
        {
            CheckLowerCase(movedAssets[i]);
        }
    }

    static void CheckLowerCase(string assetPath)
    {
        // 不对目录的命名做限制，因为我们假定目录都是大驼峰命名，同时目录被创建的机会比较少，不容易犯错
        if (Directory.Exists(assetPath))
        {
            return;
        }
        
        foreach (var file in lowerCaseCheckExcludeFiles)
        {
            if (assetPath.Contains(file)) return;
        }

        var fileName = Path.GetFileName(assetPath);
        if (assetPath.Contains("Assets/Content") && !string.Equals(fileName, fileName.ToLower()))
        {
            Debug.LogError($"Content目录（美术向资源）都要小写：{assetPath}");

            var fileDir = Path.GetDirectoryName(assetPath);
            File.Move(assetPath, fileDir + $"/{fileName.ToLower()}");
            AssetDatabase.Refresh();
        }
    }
}