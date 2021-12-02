using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace Console_FM
{
    public class Settings
    {
        public string directory { get; set; }
        public int paginglevel { get; set; }
    }

    class Program
    {
        public static Settings settings = new();
        private static readonly string configFileName = "appsettings.xml";
        public static string input = "";
        public static string curDir = AppDomain.CurrentDomain.BaseDirectory;//текущая папка
        public static readonly string startDir = AppDomain.CurrentDomain.BaseDirectory;//директория откуда запускается приложение
        public static List<string> DirList = new();//список директорий и файлов полученный с команды
        public static string logpath = startDir + "errors" + Path.DirectorySeparatorChar + "errors.log";//путь для логов
        private static int maxListarray = Console.WindowHeight-11;//стандартная высота блока вывода папок и файлов для стандартного окна минус высота блока информации и пути
        
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
                var dir = new DirectoryInfo(curDir);
                if (paramStr == "..")
                    paramStr = curDir = dir.Parent.FullName;
            }

            switch (command)
            {
                case "list":
                    if (string.IsNullOrEmpty(paramStr))
                        paramStr = Program.curDir;
                    DirList.Clear();
                    var dimension = 0;
                    CMD.List(paramStr,dimension);
                    var dirList = new DirectoryInfo(paramStr);
                    if(dirList.Exists)
                    {
                        settings.directory = curDir = paramStr;
                        ListShow(1);
                        UpdateSettings();
                    }
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
                    CMD.InfoWriter();
                    return;
            }
        }

        public static void ListShow(int page)//блок постраничного отображения
        {
            Console.SetCursorPosition(5, 2);
            CMD.ClearCurrentConsoleLine(2, 2);
            Console.Write("Path: " + $"{curDir}");
            CMD.listInfo.Add("Для пролистывания страниц используйте клавиши вверхи и вниз");
            CMD.listInfo.Add("Для выхода из режима нажмите клавишу ESC");
            CMD.InfoWriter();
            do
            {
                var maxPages = (DirList.Count / maxListarray) + 1;
                if (DirList.Count % maxListarray == 0)
                    maxPages--;
                if (page <= 0)
                    page = 1;
                if (page > maxPages)
                    page = maxPages;

                Console.SetCursorPosition(0, 4);
                CMD.ClearCurrentConsoleLine(4, maxListarray+3);//высота отображения 
                Console.SetCursorPosition(0, 4);
                for (var i = (page - 1) * maxListarray; i <= DirList.Count; i++)
                {
                    while (i < maxListarray * page && i < DirList.Count)
                    {
                        Console.WriteLine(DirList[i]);
                        i++;
                    }
                    if(i > maxListarray * page)
                        break;
                }
                Console.SetCursorPosition(1, Console.WindowHeight - 1);
                var key = Console.ReadKey(true);//перехват кнопок управления
                if (key.Key == ConsoleKey.UpArrow)
                    page--;
                if (key.Key == ConsoleKey.DownArrow)
                    page++;
                if (key.Key == ConsoleKey.Escape)
                    break;
            } while (true);
            CMD.ClearCurrentConsoleLine(Console.WindowHeight - 6, Console.WindowHeight - 3);
        }

        public static void Logger(string text) //логгер
        {
            File.AppendAllText(logpath, DateTime.Now.ToString("dd-MM-yy HH:mm:ss.fff - ") + text + "\n");
        }

        static void ReadSetting()//чтения файла настроек
        {
            try
            {
                string xmlText = File.ReadAllText(configFileName);
                StringReader stringReader = new (xmlText);
                XmlSerializer serializer = new (typeof(Settings));
                Settings tempSettings = (Settings) serializer.Deserialize(stringReader);
                if (!Directory.Exists(tempSettings.directory) || tempSettings.paginglevel <= 0)
                {
                    throw new Exception("Не валидный файл настроек");
                }
                settings.directory = tempSettings.directory;
                settings.paginglevel = tempSettings.paginglevel;
            }
            catch (Exception e)
            {
                Logger(e.Message);
                settings.directory = curDir;
                settings.paginglevel = 2;
                UpdateSettings();
            }
        }

        static void UpdateSettings()//запись файла настроек
        {
            try
            {
                StringWriter stringWriter = new ();
                XmlSerializer xmlSerializer = new (typeof(Settings));
                xmlSerializer.Serialize(stringWriter, settings);
                string xml = stringWriter.ToString();
                File.WriteAllText(configFileName, xml, Encoding.UTF8);
            }
            catch (Exception e)
            {
                Logger(e.Message);
            }
        }

        static void Init() //блок инициализации
        {
            Console.Title = "Консольный файловый менеджер";
            if (!Directory.Exists(curDir))
                curDir = startDir;
            if (!Directory.Exists("errors"))
                Directory.CreateDirectory("errors");
            Logger("Log started");
            ReadSetting();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Path: " + settings.directory);
            
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

        static void Main()
        {
            Init();
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