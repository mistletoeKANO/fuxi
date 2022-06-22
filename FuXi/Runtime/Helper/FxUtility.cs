using System;
using System.IO;
using System.Security.Cryptography;

// ReSharper disable once CheckNamespace
namespace FuXi
{
    internal static class FxUtility
    {
        private static readonly double[] byteUnits =
        {
            1073741824.0, 1048576.0, 1024.0, 1
        };

        private static readonly string[] byteUnitsNames =
        {
            "GB", "MB", "KB", "B"
        };
        
        internal static string FormatBytes(long bytes)
        {
            var size = "0 B";
            if (bytes == 0) return size;

            for (var index = 0; index < byteUnits.Length; index++)
            {
                var unit = byteUnits[index];
                if (bytes >= unit)
                {
                    size = $"{bytes / unit:##.##} {byteUnitsNames[index]}";
                    break;
                }
            }
            return size;
        }

        internal static Tuple<string, string> FormatByteTuple(long bytes)
        {
            var size = new Tuple<string, string>("0", "B");
            if (bytes == 0) return size;
            for (var index = 0; index < byteUnits.Length; index++)
            {
                var unit = byteUnits[index];
                if (bytes >= unit)
                {
                    size = new Tuple<string, string>($"{bytes / unit:##.##}", $"{byteUnitsNames[index]}");
                    break;
                }
            }
            return size;
        }

        internal static string FileName(string path)
        {
            path = path.Replace("\\", "/");
            if (path.Contains("/"))
            {
                var lastIndex = path.LastIndexOf('/');
                path = path.Substring(lastIndex + 1, path.Length - lastIndex - 1);
            }
            if (path.Contains("."))
            {
                var lastIndex = path.LastIndexOf('.');
                path = path.Substring(0, lastIndex);
            }
            return path;
        }
        
        public static long FileSize(string file)
        {
            FileInfo fileInfo = new FileInfo(file);
            return fileInfo.Length;
        }
        
        public static string FileMd5(string file)
        {
            try
            {
                using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                    var hashBytes = md5.ComputeHash(fs);
                    return Bytes2String(hashBytes);
                }
            }
            catch (Exception e)
            {
                FxDebug.LogError($"read file:{file} md5 failure with error:{e.Message}");
                return string.Empty;
            }
        }

        public static string FileCrc32(string file)
        {
            try
            {
                using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    Crc32Algorithm crc32 = new Crc32Algorithm();
                    var hashBytes = crc32.ComputeHash(fs);
                    return Bytes2String(hashBytes);
                }
            }
            catch (Exception e)
            {
                FxDebug.LogError($"read file:{file} crc32 failure with error:{e.Message}");
                return string.Empty;
            }
        }

        private static string Bytes2String(byte[] hashBytes)
        {
            string res = BitConverter.ToString(hashBytes);
            res = res.Replace("-", "");
            return res.ToLower();
        }
    }
}