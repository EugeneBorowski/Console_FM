using System;
using System.Collections.Generic;
using System.IO;

namespace Console_FM
{
    class Program
    { //5-33
        public static int pagingLevel = 2;//уроень пейджинга по-умолчанию
        public static string input = "";
        public static string curDir = AppDomain.CurrentDomain.BaseDirectory;//текущая папка
        public static readonly string startDir = AppDomain.CurrentDomain.BaseDirectory;//директория откуда запускается приложение
        public static List<string> DirList = new();//список директорий и файлов для постраничного вывода
        public static string logpath = startDir + "errors" + Path.DirectorySeparatorChar + "errors.log";//путь для логов

        static void CommandParcer(string input)
        {
            var command = "";
            var paramStr = "";
            input = input.Trim();
            
            if (!input.Contains(' '))
                command = input;
            else
            {
                for (var e = 0; e < input.IndexOf(' '); e++)
                    command += input[e];
                for (var e = input.IndexOf(' ') + 1; e <= input.Length - 1; e++)
                    paramStr += input[e];
                command = command.ToLower();
                paramStr = paramStr.TrimStart();
            }
           
            switch (command)
            {
                case "list":
                    var dimension = 0;
                    CMD.List(paramStr,dimension);
                    return;
                case "copy":
                    CMD.Copy(paramStr);
                    return;
                case "remove":
                    CMD.Remove(paramStr);
                    return;
                case "info":
                    CMD.Info(paramStr);
                    return;
                case "exit":
                    Environment.Exit(0);
                    return;
                default:
                    CMD.listInfo.Add("Неправильный формат команды");
                    CMD.InfoWriter(CMD.listInfo.ToString());
                    return;
            }
        }

        public static void Logger(string text)
        {
            File.AppendAllText(logpath, DateTime.Now.ToString("dd-MM-yy HH:mm:ss.fff - ") + text + "\n");
        }

        static void Init()
        {
            Console.Title = "Консольный файловый менеджер";
            if (!Directory.Exists(curDir))
                curDir = startDir;
            if (!Directory.Exists("errors"))
                Directory.CreateDirectory("errors");
            Logger("Log started");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Path: " + curDir);
            
            try
            {
                Console.WindowHeight = 40;
            }
            catch (Exception e)
            {
                Logger(e.Message);
                CMD.listInfo.Add(e.Message);
                CMD.InfoWriter(CMD.listInfo.ToString());
            }
            
            for (int i = 1; i <= Console.WindowWidth; i++) //блок директорий
            {
                Console.Write("-");
            }

            Console.SetCursorPosition(0, Console.WindowHeight - 7);//блок информации
            for (int i = 1; i <= Console.WindowWidth; i++)
            {
                Console.Write("-");
            }
            Console.SetCursorPosition((Console.WindowWidth/2)-5, Console.WindowHeight - 7);
            Console.Write("ИНФОРМАЦИЯ");

            Console.SetCursorPosition(0, Console.WindowHeight-2);//блок комманд
            for (int i = 1; i <= Console.WindowWidth; i++)
            {
                Console.Write("-");
            }
        }


        static void Main(string[] args)
        {

            Init();
            Console.SetCursorPosition(0, 0);
            for (var i = 1; i <= Console.WindowHeight; i++)
            {
                Console.WriteLine(i);
            }
            while (true)
            {
                DirList.Clear();
                Console.SetCursorPosition(0, Console.WindowHeight-1);
                CMD.ClearCurrentConsoleLine(Console.WindowHeight - 1, Console.WindowHeight - 1);
                Console.Write(">");
                Console.WriteLine();
                Console.SetCursorPosition(1, Console.WindowHeight - 1);
                input = Console.ReadLine();
                CommandParcer(input);
            }
        }
    }
}