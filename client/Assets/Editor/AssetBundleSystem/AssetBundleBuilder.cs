using System.Collections.Generic;
using System.IO;
using System.Text;
using Framework;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class FileInfo
{
    public string fileName;
    public string fileFullName;
    public string filePath;
    public string assetBundleName;
}

public class AssetBundleBuilder : UnityEditor.Editor
{
    [MenuItem("Build/Build AssetBundles")]
    private static void BuildAssetBundles()
    {
        // 构造 <目录，目录下所有的文件> 映射，<文件名, 目录名> 映射。前者用于打包，后者用于运行时查询。
        Dictionary<string, List<string>> filePath_fileFullNames = new Dictionary<string, List<string>>();
        // Dictionary<string, string> fileName_filePath = new Dictionary<string, string>();
        Dictionary<string, FileInfo> fileName_fileInfo = new Dictionary<string, FileInfo>();
        foreach (var assetDir in AssetManager.Instance.assetDirs)
        {
            AddFiles(assetDir, ref filePath_fileFullNames, ref fileName_fileInfo);
        }

        foreach (var assetDir in AssetManager.Instance.singleAssetDirs)
        {
            AddSingleBundleFiles(assetDir, assetDir, ref filePath_fileFullNames, ref fileName_fileInfo);
        }

        Debug.Log($"filePath_fileFullName = {JsonConvert.SerializeObject(filePath_fileFullNames, Formatting.Indented)}");
        Debug.Log($"fileName_filePath = {JsonConvert.SerializeObject(fileName_fileInfo, Formatting.Indented)}");

        // 通过上面映射构造 AssetBundleBuild 列表，其中每个目录对应一个AB，AB 里面包含目录下的所有文件
        List<AssetBundleBuild> builds = new List<AssetBundleBuild>();
        foreach (var keyValuePair in filePath_fileFullNames)
        {
            if (keyValuePair.Value.Count == 0)
                continue;

            AssetBundleBuild build = new AssetBundleBuild();
            string assetBundleName = AssetHelper.DirectoryPathToAssetBundleName(keyValuePair.Key);
            build.assetBundleName = assetBundleName;

            var assetNames = new List<string>();
            foreach (var s in keyValuePair.Value)
            {
                assetNames.Add(s);
                var fileName = Path.GetFileName(s);
                var fileInfo = fileName_fileInfo[fileName];
                fileInfo.assetBundleName = assetBundleName;
            }

            build.assetNames = assetNames.ToArray();
            builds.Add(build);
        }

        // 开始构建 AB
        if (Directory.Exists(AssetManager.Instance.ASSETBUNDLE_DIR))
        {
            Directory.Delete(AssetManager.Instance.ASSETBUNDLE_DIR, true);
        }

        Directory.CreateDirectory(AssetManager.Instance.ASSETBUNDLE_DIR);

        BuildPipeline.BuildAssetBundles(AssetManager.Instance.ASSETBUNDLE_DIR, builds.ToArray(), BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.Android);

        StringBuilder sb = new StringBuilder();
        foreach (var keyValuePair in fileName_fileInfo)
        {
            sb.Append(keyValuePair.Key).Append(",").Append(keyValuePair.Value.filePath).Append(",").Append(keyValuePair.Value.assetBundleName).Append("\n");
        }

        File.WriteAllText(AssetManager.Instance.ASSETBUNDLE_DIR + "/fileName_dirName_assetBundleName.csv", sb.ToString());

        AssetDatabase.Refresh();
    }

    private static void AddSingleBundleFiles(string rootDir, string dir, ref Dictionary<string, List<string>> filePath_fileFullName, ref Dictionary<string, FileInfo> fileName_fileInfo)
    {
        var files = Directory.GetFiles(dir);
        var validFiles = new List<string>();
        foreach (var file in files)
        {
            var ext = Path.GetExtension(file);
            var fileName = Path.GetFileName(file);
            if (ext.Equals(".meta"))
            {
                continue;
            }

            validFiles.Add(file.Replace("\\", "/"));
            string filePath = Path.GetDirectoryName(file).Replace("\\", "/").TrimStart('/');
            fileName_fileInfo[fileName] = new FileInfo {fileName = fileName, filePath = filePath};
        }

        if (validFiles.Count > 0)
        {
            if (!filePath_fileFullName.ContainsKey(rootDir))
            {
                filePath_fileFullName[rootDir] = new List<string>();
            }

            filePath_fileFullName[rootDir].AddRange(validFiles);
        }

        var dirs = Directory.GetDirectories(dir);
        foreach (var s in dirs)
        {
            AddSingleBundleFiles(rootDir, s, ref filePath_fileFullName, ref fileName_fileInfo);
        }
    }

    private static void AddFiles(string dir, ref Dictionary<string, List<string>> filePath_fileFullName, ref Dictionary<string, FileInfo> fileName_fileInfo)
    {
        var files = Directory.GetFiles(dir);
        var validFiles = new List<string>();
        foreach (var file in files)
        {
            var ext = Path.GetExtension(file);
            var fileName = Path.GetFileName(file);
            if (ext.Equals(".meta"))
            {
                continue;
            }

            validFiles.Add(file.Replace("\\", "/"));
            string filePath = Path.GetDirectoryName(file).Replace("\\", "/").TrimStart('/');
            fileName_fileInfo[fileName] = new FileInfo {fileName = fileName, filePath = filePath};
        }

        if (validFiles.Count > 0)
            filePath_fileFullName[dir] = validFiles;

        var dirs = Directory.GetDirectories(dir);
        foreach (var s in dirs)
        {
            AddFiles(s, ref filePath_fileFullName, ref fileName_fileInfo);
        }
    }
}