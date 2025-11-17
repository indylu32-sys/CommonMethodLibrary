using System.Text;

namespace CommonMethodLibrary.Core.Helpers;

/// <summary>
/// 文件处理工具类
/// </summary>
public static class FileHelper
{
    /// <summary>
    /// 读取文件内容
    /// </summary>
    public static async Task<string> ReadAllTextAsync(string filePath, Encoding? encoding = null)
    {
        encoding ??= Encoding.UTF8;
        return await File.ReadAllTextAsync(filePath, encoding);
    }

    /// <summary>
    /// 写入文件内容
    /// </summary>
    public static async Task WriteAllTextAsync(string filePath, string content, Encoding? encoding = null)
    {
        encoding ??= Encoding.UTF8;
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(filePath, content, encoding);
    }

    /// <summary>
    /// 追加内容到文件
    /// </summary>
    public static async Task AppendAllTextAsync(string filePath, string content, Encoding? encoding = null)
    {
        encoding ??= Encoding.UTF8;
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.AppendAllTextAsync(filePath, content, encoding);
    }

    /// <summary>
    /// 复制文件
    /// </summary>
    public static void CopyFile(string sourceFile, string destFile, bool overwrite = true)
    {
        var directory = Path.GetDirectoryName(destFile);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.Copy(sourceFile, destFile, overwrite);
    }

    /// <summary>
    /// 移动文件
    /// </summary>
    public static void MoveFile(string sourceFile, string destFile, bool overwrite = true)
    {
        var directory = Path.GetDirectoryName(destFile);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (overwrite && File.Exists(destFile))
        {
            File.Delete(destFile);
        }

        File.Move(sourceFile, destFile);
    }

    /// <summary>
    /// 删除文件
    /// </summary>
    public static void DeleteFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    /// <summary>
    /// 获取文件大小（字节）
    /// </summary>
    public static long GetFileSize(string filePath)
    {
        if (!File.Exists(filePath))
            return 0;

        var fileInfo = new FileInfo(filePath);
        return fileInfo.Length;
    }

    /// <summary>
    /// 获取友好的文件大小描述
    /// </summary>
    public static string GetFriendlyFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }

    /// <summary>
    /// 获取文件扩展名
    /// </summary>
    public static string GetFileExtension(string filePath)
    {
        return Path.GetExtension(filePath);
    }

    /// <summary>
    /// 获取不带扩展名的文件名
    /// </summary>
    public static string GetFileNameWithoutExtension(string filePath)
    {
        return Path.GetFileNameWithoutExtension(filePath);
    }

    /// <summary>
    /// 确保目录存在
    /// </summary>
    public static void EnsureDirectoryExists(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
    }

    /// <summary>
    /// 删除目录及其内容
    /// </summary>
    public static void DeleteDirectory(string directoryPath, bool recursive = true)
    {
        if (Directory.Exists(directoryPath))
        {
            Directory.Delete(directoryPath, recursive);
        }
    }

    /// <summary>
    /// 获取目录中的所有文件
    /// </summary>
    public static string[] GetFiles(string directoryPath, string searchPattern = "*.*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {
        if (!Directory.Exists(directoryPath))
            return Array.Empty<string>();

        return Directory.GetFiles(directoryPath, searchPattern, searchOption);
    }

    /// <summary>
    /// 判断文件是否存在
    /// </summary>
    public static bool FileExists(string filePath)
    {
        return File.Exists(filePath);
    }

    /// <summary>
    /// 判断目录是否存在
    /// </summary>
    public static bool DirectoryExists(string directoryPath)
    {
        return Directory.Exists(directoryPath);
    }
}
