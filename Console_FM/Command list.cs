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

        public static void ClearCurrentConsoleLine(int from,int to) //чистильщик консоли
        {
            for (int i = from; i <= to; i++)
            {
                Console.SetCursorPosition(0, i);
                Console.Write(new string(' ', Console.WindowWidth));
                Console.SetCursorPosition(0, i);
            }
        }
        static bool CheckForDirectory(string path) //Проверка, файл или директория в запрашиваемом пути
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
                InfoWriter();
                return false;
            }
        }

        private static SourceTarget SourceTargetParcer(string param) //парсим путь источника и назначния
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
                InfoWriter();
            }
            return output;
        }

        private static void DirectoryCopy(string source, string target, int run) //блок копирования директорий
        {
            DirectoryInfo dirS = new(source);
            DirectoryInfo dirT = new(target);
            if (!dirT.Exists && run == 0)
                target = Path.Combine(target, dirS.Name);
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

        public static void InfoWriter()//Переписывает информационный блок
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

        public static void List(string path, int dimension)//команда вывода директорий
        {
            try
            {
                var separator = new string('-', dimension);
                var dirList = new DirectoryInfo(path);
                if (dimension == 0)//замена выхода на уровень выше
                    if (path == "..")
                        path = dirList.Parent.FullName;
                if (dirList.Exists)
                {
                    DirectoryInfo[] dirs = dirList.GetDirectories();
                    FileInfo[] files = dirList.GetFiles();
                    if (dimension > Program.settings.paginglevel)
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
                    InfoWriter();
                }
            }
            catch (Exception e)
            {
                Program.Logger(e.Message);
                listInfo.Add(e.Message);
                InfoWriter();
            }
        }
        
        public static void Copy(string param) //блок копирования папок и файлов
        {
            var input = SourceTargetParcer(param);
            if (CheckForDirectory(input.source))
                try
                {
                    if (string.IsNullOrEmpty(input.parameter)) //фильтр лишних параметров
                        DirectoryCopy(input.source,input.target,0);//если папка, работаем как с папкой
                }
                catch (Exception e)
                {
                    Program.Logger(e.Message);
                    listInfo.Add(e.Message);
                    InfoWriter();
                }
            else//иначе работает как с файлом
            {
                try
                {
                    var fileIn = new FileInfo(input.source);
                    var fileOut = new FileInfo(input.target);
                    if (!Directory.Exists(fileOut.FullName))
                        Directory.CreateDirectory(fileOut.FullName);//если целевой папки нет, создаем ее
                    File.Copy(input.source, input.target+ Path.DirectorySeparatorChar+ fileIn.Name, true);
                }
                catch (Exception e)
                {
                    Program.Logger(e.Message);
                    listInfo.Add(e.Message);
                    InfoWriter();
                }
            }
        }

        public static void Remove(string param)//блок удаления
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
                    InfoWriter();
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
                    InfoWriter();
                }
            }
        }

        public static void Info(string param) //блок вывода информации
        {
            if(CheckForDirectory(param))
                try
                {
                    var dir = new DirectoryInfo(param);
                    listInfo.Add($"name: {dir.Name}");
                    listInfo.Add($"attributes :{dir.Attributes}");
                    InfoWriter();
                }
                catch (Exception e)
                {
                    Program.Logger(e.Message);
                    listInfo.Add(e.Message);
                    InfoWriter();
                }
            else
            {
                try
                {
                    var file = new FileInfo(param);
                    listInfo.Add($"name: {file.Name}");
                    listInfo.Add($"size: {file.Length} bytes");
                    listInfo.Add($"attributes: {file.Attributes}");
                    InfoWriter();
                }
                catch (Exception e)
                {
                    Program.Logger(e.Message);
                    listInfo.Add(e.Message);
                    InfoWriter();
                }
            }
        }
    }
}
