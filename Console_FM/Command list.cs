using System;
using System.Collections.Generic;
using System.IO;

namespace Console_FM
{
    class SourceTarget
    {
        public string source { get; set; }
        public string target { get; set; }
        public string parameter { get; set; }
    }

    class CMD
    {
        public static List<string> listInfo = new(); //Список для блока информации 

        public static void ClearCurrentConsoleLine(int from,int to)
        {
            for (int i = from; i <= to; i++)
            {
                Console.SetCursorPosition(0, i);
                Console.Write(new string(' ', Console.WindowWidth));
                Console.SetCursorPosition(0, i);
            }
        }
        static bool CheckForDirectory(string path)
        {
            try
            {
                var result = File.GetAttributes(path).HasFlag(FileAttributes.Directory); //Чек на директорию, если нет, работаем как с файлом
                return result;
            }
            catch (Exception e)
            {
                Program.Logger(e.Message);
                listInfo.Add(e.Message);
                InfoWriter(listInfo.ToString());
                return false;
            }
        }

        private static SourceTarget SourceTargetParcer(string param)
        {
            var output = new SourceTarget();
            var strs = param.Split(' ',StringSplitOptions.RemoveEmptyEntries);
            try
            {
                output.source = strs[0];
                output.target = strs[1];
                if (strs.Rank >= 2)
                    if (strs[2].StartsWith('-'))
                        output.parameter = strs[2];
            }
            catch (Exception e)
            {
                Program.Logger(e.Message);
                listInfo.Add(e.Message);
                listInfo.Add("Неправильно заданы параметры");
                InfoWriter(listInfo.ToString());
            }
            return output;
        }

        private static void DirectoryCopy(string source, string target, int run)
        {
            DirectoryInfo dirS = new(source);
            DirectoryInfo dirT = new(target);
            if (!dirT.Exists && run == 0)
            {
                target = Path.Combine(target, dirS.Name);
            }
            if (!dirT.Exists)
                Directory.CreateDirectory(dirT.FullName);
            DirectoryInfo[] dirs = dirS.GetDirectories();
            FileInfo[] files = dirS.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(target, file.Name);
                file.CopyTo(tempPath, true);
            }
            foreach (DirectoryInfo subdir in dirs)
            {
                string tempPath = Path.Combine(target, subdir.Name);
                DirectoryCopy(subdir.FullName, tempPath, ++run);
                run--;
            }
        }

        public static void InfoWriter(string text)
        {
            ClearCurrentConsoleLine(Console.WindowHeight - 6, Console.WindowHeight - 3);
            Console.SetCursorPosition(0, Console.WindowHeight - 6);
            foreach (var str in listInfo)
            {
                Console.Write(str);
                Console.SetCursorPosition(0, Console.CursorTop+1);
            }
            listInfo.Clear();
        }

        public static void List(string path, int dimension)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                    path = Program.curDir;
                var separator = new string('-', dimension);
                var dirList = new DirectoryInfo(path);
                if (dirList.Exists)
                {
                    DirectoryInfo[] dirs = dirList.GetDirectories();
                    FileInfo[] files = dirList.GetFiles();
                    if (dimension > Program.pagingLevel)
                        return;
                    foreach (var e in dirs)
                    {
                        PrintFiles($"{separator}dir: {e.Name}");
                        var tempDir = path + @"\" + e.Name;
                        List(tempDir, ++dimension);
                        dimension--;
                    }

                    foreach (var e in files)
                    {
                        PrintFiles($"{separator}file: {e.Name}");
                    }

                    static void PrintFiles(string text)
                    {
                        Program.DirList.Add(text);
                    }
                }
                else
                {
                    listInfo.Add("Директория не найдена");
                    InfoWriter(listInfo.ToString());
                }

            }
            catch (Exception e)
            {
                Program.Logger(e.Message);
                listInfo.Add(e.Message);
                InfoWriter(listInfo.ToString());
            }
        }
        
        public static void Copy(string param)
        {
            var input = SourceTargetParcer(param);
            if (CheckForDirectory(input.source))
                try
                {
                    if (!string.IsNullOrEmpty(input.parameter)) //фильтр лишних параметров

                    DirectoryCopy(input.source,input.target,0);
                }
                catch (Exception e)
                {
                    Program.Logger(e.Message);
                    listInfo.Add(e.Message);
                    InfoWriter(listInfo.ToString());
                }
            else
            {
                try
                {
                    File.Copy(input.source, input.target, true);
                }
                catch (Exception e)
                {
                    Program.Logger(e.Message);
                    listInfo.Add(e.Message);
                    InfoWriter(listInfo.ToString());
                }
            }
        }

        public static void Remove(string param)
        {
            if (CheckForDirectory(param))
                try
                {
                    Directory.Delete(param,true);
                }
                catch (Exception e)
                {
                    Program.Logger(e.Message);
                    listInfo.Add(e.Message);
                    InfoWriter(listInfo.ToString());
                }
            else
            {
                try
                {
                    File.Delete(param);
                }
                catch (Exception e)
                {
                    Program.Logger(e.Message);
                    listInfo.Add(e.Message);
                    InfoWriter(listInfo.ToString());
                }
            }
        }

        public static void Info(string param)
        {
            if(CheckForDirectory(param))
                try
                {
                    var dir = new DirectoryInfo(param);
                    listInfo.Add($"name: {dir.Name}");
                    listInfo.Add($"attributes :{dir.Attributes}");
                    InfoWriter(listInfo.ToString());
                }
                catch (Exception e)
                {
                    Program.Logger(e.Message);
                    listInfo.Add(e.Message);
                    InfoWriter(listInfo.ToString());
                }
            else
            {
                try
                {
                    var file = new FileInfo(param);
                    listInfo.Add($"name: {file.Name}");
                    listInfo.Add($"size: {file.Length} bytes");
                    listInfo.Add($"attributes: {file.Attributes}");
                    InfoWriter(listInfo.ToString());
                }
                catch (Exception e)
                {
                    Program.Logger(e.Message);
                    listInfo.Add(e.Message);
                    InfoWriter(listInfo.ToString());
                }
            }
        }
    }
}
