using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Checksum;
using System;
using System.IO;

namespace SharpZipLib
{
    public class Zipper
    {
        static void de_compress(Stream fs, string filePath)
        {
            if (filePath[filePath.Length - 1] != System.IO.Path.DirectorySeparatorChar)
                filePath += System.IO.Path.DirectorySeparatorChar;
            using (ZipInputStream stream = new ZipInputStream(fs))
            {
                ZipEntry entry;
                while ((entry = stream.GetNextEntry()) != null)
                {
                    string directoryName = null;
                    string entryName = entry.Name;
                    if (string.IsNullOrWhiteSpace(entryName) == false)
                    {
                        directoryName = Path.GetDirectoryName(entryName) + Path.DirectorySeparatorChar;
                    }
                    string name = Path.GetFileName(entryName);
                    ///文件的父级目录的绝对路径
                    string newDirectory = filePath + directoryName;
                    if (Directory.Exists(newDirectory) == false)
                    {
                        Directory.CreateDirectory(newDirectory);
                    }
                    if (string.IsNullOrWhiteSpace(name) == false)
                    {
                        string fileName = filePath + directoryName + name;
                        using (FileStream fsWrite = File.Create(fileName))
                        {
                            byte[] buffer = new byte[4096]; // max buffer 4k
                            int sizeRead = 0;
                            do
                            {
                                sizeRead = stream.Read(buffer, 0, buffer.Length);
                                fsWrite.Write(buffer, 0, sizeRead);
                            } while (sizeRead > 0);
                        }
                    }
                }
            }
        }
        static public void DeCompress(byte[] bytestream,string filePath)
        {
            using (var fs = new MemoryStream(bytestream))
            {
                de_compress(fs, filePath);
            }
        }
        static public void DeCompress(string filePath, string zipFilePath)
        {
            if (filePath[filePath.Length - 1] != System.IO.Path.DirectorySeparatorChar)
                filePath += System.IO.Path.DirectorySeparatorChar;
            using (FileStream fs = File.OpenRead(zipFilePath))
            {
                de_compress(fs, zipFilePath);
            }
        }
        static public void Compress(string filePath, string zipFilePath)
        {
            if (filePath[filePath.Length - 1] != System.IO.Path.DirectorySeparatorChar)
                filePath += System.IO.Path.DirectorySeparatorChar;
            using (FileStream fs = File.Create(zipFilePath))
            {
                using (ZipOutputStream stream = new ZipOutputStream(fs))
                {
                    stream.SetLevel(0);  // 压缩级别 0-9
                    CreateZipFiles(filePath, stream, filePath.Length);
                    stream.Finish();
                }
            }
        }

        /// 递归压缩文件
        static void CreateZipFiles(string sourceFilePath, ZipOutputStream stream, int subIndex)
        {
            Crc32 crc = new Crc32();
            string[] filesArray = Directory.GetFileSystemEntries(sourceFilePath);
            foreach (string file in filesArray)
            {
                if (Directory.Exists(file))                     //如果当前是文件夹，递归
                {
                    CreateZipFiles(file, stream, subIndex);
                }
                else                                            //如果是文件，开始压缩
                {
                    using (FileStream fs = File.OpenRead(file))
                    {
                        byte[] buffer = new byte[fs.Length];
                        fs.Read(buffer, 0, buffer.Length);
                        string tempFile = file.Substring(subIndex);
                        ZipEntry entry = new ZipEntry(tempFile);
                        entry.DateTime = DateTime.Now;
                        entry.Size = fs.Length;
                        crc.Reset();
                        crc.Update(buffer);
                        entry.Crc = crc.Value;
                        stream.PutNextEntry(entry);
                        stream.Write(buffer, 0, buffer.Length);
                    }
                }
            }
        }
    }
}