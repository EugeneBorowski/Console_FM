using System;
using System.Collections.Generic;
using System.IO;
using System.Configuration;

namespace Console_FM
{
    class Program
    { //4-33
        public static int pagingLevel = 2;//уроень пейджинга по-умолчанию
        public static string input = "";
        public static string curDir = AppDomain.CurrentDomain.BaseDirectory;//текущая папка, заплатка под изменяющуюся директорию в будущем
        public static readonly string startDir = AppDomain.CurrentDomain.BaseDirectory;//директория откуда запускается приложение
        public static List<string> DirList = new();//список директорий и файлов полученный с команды
        public static string logpath = startDir + "errors" + Path.DirectorySeparatorChar + "errors.log";//путь для логов
        private static int maxListarray = 29;//стандартная высота блока выврда папок и файлов
        
        static void CommandParcer(string input)//парсер команд, разделяет команду и параметры
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
                    DirList.Clear();
                    var dimension = 0;
                    CMD.List(paramStr,dimension);
                    ListShow(1);
                    curDir = paramStr;
                    UpdateSettings("directory", curDir);
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

        public static void ListShow(int page)
        {
            var maxPages = (DirList.Count / maxListarray)+1;
            if (DirList.Count % maxListarray == 0)
                maxPages = maxPages - 1;
            if (page <= 0)
                page = 1;
            if (page > maxPages)
                page = maxPages;

            Console.SetCursorPosition(0, 4);
            CMD.ClearCurrentConsoleLine(4,maxListarray);
            Console.SetCursorPosition(0, 4);
            for (var i = (page - 1) * maxListarray; i <= DirList.Count; i++)
            {
                while (i < maxListarray*page && i < DirList.Count)
                {
                    Console.WriteLine(DirList[i]);
                    i++;
                }
            }
        }

        public static void Logger(string text) //логгер
        {
            File.AppendAllText(logpath, DateTime.Now.ToString("dd-MM-yy HH:mm:ss.fff - ") + text + "\n");
        }

        static void ReadSetting(string key)
        {
            try
            {
                var appSettings = ConfigurationManager.AppSettings;
                string result = appSettings[key] ?? $"Parameter {key} not found";
                if (result == "Parameter directory not found")
                {
                    UpdateSettings("directory", curDir);
                    UpdateSettings("paginglevel", pagingLevel.ToString());
                }
                curDir = appSettings["directory"];
                pagingLevel = int.Parse(appSettings["pagingLevel"]);
            }
            catch (Exception e)
            {
                Logger(e.Message);
            }
        }

        static void UpdateSettings(string key, string value)
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;
                if (settings[key] == null)
                {
                    settings.Add(key, value);
                }
                else
                {
                    settings[key].Value = value;
                }
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
            catch (Exception e)
            {
                Logger(e.Message);
            }
        }

        static void Init() //блок инициализации
        {
            ReadSetting("directory");
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
            Console.WriteLine(pagingLevel);
            while (true)
            {
                Console.SetCursorPosition(0, Console.WindowHeight - 1);
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