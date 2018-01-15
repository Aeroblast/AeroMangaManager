using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using SharpCompress;
using SharpCompress.Archives;
using SharpCompress.Readers;
using SharpCompress.Common;

namespace AeroMangaManager
{
    class ArchiveManager
    {
        static string _tempDirPath = "Temp";
        public static string tempDirPath
        {
            get{ return _tempDirPath;}
        }
        readonly static string configPath = "Config.txt";
        static List<string> mangaDirPaths;
        static List<string> passwords;
        static List<UnarchiveInfo> tasks;
        static List<string> mangaPaths;
        static void Unarchive(object unarchiveInfo)
        {
            UnarchiveInfo info=(UnarchiveInfo)unarchiveInfo;
            Logger.Log(info.archivePath);
            ReaderOptions readerOptions = new ReaderOptions();
            foreach (string pw in passwords)
            {
                readerOptions.Password = pw;
                try
                {
                    using (var archive = ArchiveFactory.Open(info.archivePath, readerOptions))
                    {
                        using (var reader = archive.ExtractAllEntries())
                        {
                            reader.WriteAllToDirectory(info.tempDirPath);
                        }
                    }
                    info.success=true;
                    break;
                }
                catch (SharpCompress.Compressors.LZMA.DataErrorException e)
                {
                    if (e.Message == "Data Error")
                    {
                        continue;
                    }
                }
            }

        }
        public static UnarchiveInfo StartUnarchive(int mangaId,int sessionId)
        {
            if(mangaId<0||mangaId>=mangaPaths.Count)
            {
                return null;
            }
            string path=Path.Combine(tempDirPath,sessionId.ToString());
            if(!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            else
            {
                ClearDir(path);
            }
            //并发风险注意……嘛反正预想场景不会发生啦
            int index=-1;
            for(int i=0;i<tasks.Count;i++)
            {
                 if(!tasks[i].thread.IsAlive)
                 {
                     tasks[i].success=false;
                     index=i;
                     break;
                 }
            }
            if(index<0)
            {
                index=tasks.Count;
                tasks.Add(new UnarchiveInfo());
            }
            tasks[index].archivePath=mangaPaths[mangaId];
            tasks[index].thread=new Thread(Unarchive);
            tasks[index].tempDirPath=path;
            tasks[index].sessionId=sessionId;
            tasks[index].thread.Start(tasks[index]);
            return tasks[index];
        }
        public static string GetList(int start,int count)
        {
            count=Math.Min(count,mangaPaths.Count-start);
            if(count<=0)return null;

            string result="";
            for(int i=start;i<start+count;i++)
            {
                result+=(i+":"+Path.GetFileName(mangaPaths[i])+"\n");
            }
            return result;
        }
        public static bool isArchive(string path)
        {
            string ext = Path.GetExtension(path);
            return ".7z".Equals(ext, StringComparison.OrdinalIgnoreCase)
            || ".zip".Equals(ext, StringComparison.OrdinalIgnoreCase)
            || ".rar".Equals(ext, StringComparison.OrdinalIgnoreCase);
        }
        public static bool isImage(string path)
        {
            string ext = Path.GetExtension(path);
            return ".jpg".Equals(ext, StringComparison.OrdinalIgnoreCase)
            || ".gif".Equals(ext, StringComparison.OrdinalIgnoreCase)
            || ".jpeg".Equals(ext, StringComparison.OrdinalIgnoreCase)
            || ".png".Equals(ext, StringComparison.OrdinalIgnoreCase);
        }
        public static void ClearDir(string path)
        {
            foreach(string dir in Directory.GetDirectories(path))
            {ClearDir(dir);Directory.Delete(dir); }
           foreach(string file in Directory.GetFiles(path))
           {
               File.Delete(file);
           }
        }
        public static void Init()
        {
            mangaDirPaths = new List<string>();
            passwords = new List<string>();
            tasks=new List<UnarchiveInfo>();
            if (File.Exists(configPath))
            {
                int i = 0;
                foreach (string str in File.ReadAllLines(configPath))
                {
                    if (i == 0)
                    {
                        if (str.IndexOf("[MangaDir]") == 0) i = 1;
                        if (str.IndexOf("[TempDir]") == 0) i = 2;
                        if (str.IndexOf("[Passwords]") == 0) i = 3;
                        continue;
                    }
                    switch (i)
                    {
                        case 1:
                            {
                                if (str.IndexOf("[/MangaDir]") == 0) { i = 0; break; }
                                if (Directory.Exists(str))
                                {
                                    mangaDirPaths.Add(str);
                                    Logger.Log("MangaDir:" + str);
                                }
                            }
                            break;
                        case 2:
                            {
                                if (str.IndexOf("[/TempDir]") == 0) { i = 0; break; }
                                _tempDirPath = str;
                                Logger.Log("TempDir:" + str);

                            }
                            break;
                        case 3:
                            {
                                if (str.IndexOf("[/Passwords]") == 0) { i = 0; break; }
                                passwords.Add(str);
                            }
                            break;
                    }

                }
            }
            else
            {
                Logger.Log("Warning: Config file not found.");
            }

            if (!Directory.Exists(tempDirPath))
            {
                Directory.CreateDirectory(tempDirPath);
            }
            ClearDir(tempDirPath);
            
            mangaPaths=new List<string>();
            foreach (string path in mangaDirPaths)
            {
                string[] files = Directory.GetFiles(path);
                foreach (string file in files)
                {
                    if(isArchive(file))
                    {
                        mangaPaths.Add(file);
                    }
                }
            }

        }
    }
    public class UnarchiveInfo
    {
        public string archivePath;
        public string tempDirPath;
        public int sessionId;
        public Thread thread;

        public bool success=false;
    }
}