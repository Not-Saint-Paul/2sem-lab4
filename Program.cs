using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace Laba4
{
    class Memento
    {
        public string _FilePath { get; set; }
        public string _FileName { get; set; }
        public string _FileContent { get; set; }
    }

    public interface IOriginator
    {
        object GetMemento();
        void SetMemento(object memento);
    }

    [Serializable]
    class TextFile : IOriginator
    {
        private string _FilePath { get; set; }
        private string _FileName { get; set; }
        private string _FileContent { get; set; }

        public TextFile(string path, string fileName, string fileContent)
        {
            _FilePath = path; _FileName = fileName; _FileContent = fileContent;
        }

        object IOriginator.GetMemento()
        {
            return new Memento
            {
                _FilePath = this._FilePath,
                _FileName = this._FileName,
                _FileContent = this._FileContent
            };
        }

        void IOriginator.SetMemento(object memento)
        {
            if (memento is Memento)
            {
                var mem = memento as Memento;

                _FilePath = mem._FilePath;
                _FileName = mem._FileName;
                _FileContent = mem._FileContent;
            }
        }

        private static TextFile BinarySerialize(TextFile textFile) //запись в файл 
        {
            string filePath = textFile._FilePath;
            string fileName = textFile._FileName;
            string fileContent = textFile._FileContent;
            
            if (Directory.Exists(filePath))
            {
                using (BinaryWriter writer = new BinaryWriter(File.Open(fileName, FileMode.OpenOrCreate)))
                {
                    writer.Write(fileContent);

                    return textFile;
                }
            }
            else
            {
                Console.WriteLine("Такой каталог не обнаружен. Проверьте правильность пути к каталогу");
            }

            return null;
        }

        private static TextFile BinaryDeserialize(TextFile textFile)
        {
            string filePath = textFile._FilePath;
            string fileName = textFile._FileName;
            string fileContent = textFile._FileContent;

            if (Directory.Exists(filePath))
            {
                using (BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open)))
                {
                    while (reader.PeekChar() > -1)
                    {
                        fileContent = reader.ReadString();
                    }
                    textFile._FileContent = fileContent;

                    return textFile;
                }
            }
            else
            {
                Console.WriteLine("Такой каталог не обнаружен. Проверьте правильность пути к каталогу");
            }
            return null;
        }

        private static TextFile XmlSerialize(TextFile textFile) //запись в xml-файл
        {
            string filePath = textFile._FilePath;
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(TextFile));

            if (Directory.Exists(filePath))
            {
                using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate))
                {
                    xmlSerializer.Serialize(fs, textFile);

                    return textFile;
                }
            }
            else
            {
                Console.WriteLine("Такой каталог не обнаружен. Проверьте правильность пути к каталогу");
            }

            return null;
        }

        private static TextFile XmlDeserialize(TextFile textFile) //чтение из xml-файла
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(TextFile));
            string filePath = textFile._FilePath;
            string fileName = textFile._FileName;

            if (Directory.Exists(filePath))
            {
                using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate))
                {
                    string fileContent = xmlSerializer.Deserialize(fs) as string;
                    textFile._FileContent = fileContent;

                    return textFile;
                }
            }
            else
            {
                Console.WriteLine("Такой каталог не обнаружен. Проверьте правильность пути к каталогу");
            }
            return null;
        }

        public static void OpenOrCreate(string fileName, string filePath, char modifier)
        {
            string fileContent = Console.ReadLine();
            TextFile textFile = new TextFile(filePath, fileName, fileContent);

            switch (modifier)
            {
                case 'b':
                    BinaryDeserialize(textFile);
                    BinarySerialize(textFile);
                    break;
                case 'x':
                    XmlDeserialize(textFile);
                    XmlSerialize(textFile);
                    break;
                default:
                    break;
            }

        }
    }

    

    class FileSearch
    {
        public static void Search(string dirName, List<string> keyWords)
        {
            var directory = new DirectoryInfo(dirName);

            if (directory.Exists)
            {
                FileInfo[] files = directory.GetFiles();
                bool filesInDirIncludesKeyWord = false;

                foreach (FileInfo file in files)
                {
                    string fileName = file.Name;

                    using (FileStream fstream = File.OpenRead(fileName))
                    {
                        byte[] buffer = new byte[fstream.Length];
                        fstream.Read(buffer, 0, buffer.Length);
                        string textFromFile = Encoding.Default.GetString(buffer);
                        string[] splittedText = textFromFile.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string word in splittedText)
                        {
                            foreach (string keyWord in keyWords)
                            {
                                if (keyWord == word)
                                {
                                    Console.WriteLine(fileName + " содержит слово " + keyWord);
                                    filesInDirIncludesKeyWord = true;
                                }
                            }
                        }

                    }
                }

                if(!filesInDirIncludesKeyWord)
                {
                    Console.WriteLine("Файлы из этого каталога не содержат ключевые слова");
                }
            }
            else
            {
                Console.WriteLine("Каталог не обнаружен. Повторите ввод");
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            string command;
            bool workIsEnded = false;

            do
            {
                Console.WriteLine("Введите команду с косой чертой и с заглавной буквы. '/Помощь' - для вывода команд, '/Выход' - выход из приложения");
                command = Console.ReadLine();

                switch(command)
                {
                    case "/Помощь":
                        Console.WriteLine("'/ОткрытьИлиСоздать' - Открыть существующий или создать новый файл\n" 
                            + "'/ПоискПоСловам' - поиск файлов по ключевым словам\n" 
                            + "'/Выход' - выход из программы");
                        break;
                    case "/ОткрытьИлиСоздать":
                        Console.WriteLine("Введите имя файла");
                        string fileName = Console.ReadLine();
                        Console.WriteLine("Введите путь к файлу");
                        string filePath = Console.ReadLine();
                        char modifier = new char();
                        bool isSelected = false;

                        do
                        {
                            Console.WriteLine("Выберите тип сериализации: '/Бин' - бинарная сериализация, '/XML' (латиницей) - XML-сериализация");
                            string type = Console.ReadLine();

                            switch (type)
                            {
                                case "/Бин":
                                    modifier = 'b';
                                    isSelected = true;
                                    break;
                                case "/XML":
                                    modifier = 'x';
                                    isSelected = true;
                                    break;
                                case "/Выход":
                                    break;
                                default:
                                    Console.WriteLine("Введена неверная команда. Повторите ввод.");
                                    break;
                            }
                        } while (!isSelected);
                        
                        TextFile.OpenOrCreate(fileName, filePath, modifier);
                        break;
                    case "/ПоискПоСловам":
                        List<string> keyWords = new List<string>();
                        string newWord;

                        Console.WriteLine("Введите путь к каталогу для поиска в нём файлов");
                        string dirName = Console.ReadLine();

                        Console.WriteLine("Введите ключевые слова (от 1 до 10). '/Останов' - для прекращения ввода");
                        for (int word = 0; word < 10; ++word)
                        {
                            newWord = Console.ReadLine();
                            keyWords.Add(newWord);

                            if (newWord == "/Останов")
                            {
                                break;
                            }
                        }
                        FileSearch.Search(dirName, keyWords);

                        break;
                    case "/Выход":
                        Console.WriteLine("Вы уверены? Да - для выхода");
                        if (Console.ReadLine() == "Да")
                        {
                            workIsEnded = true;
                        }
                        break;
                    default:
                        Console.WriteLine("Введена неверная команда. Повторите ввод.");
                        break;
                }
            } while (!workIsEnded);
        }
    }
}
