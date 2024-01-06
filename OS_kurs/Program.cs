using System;
using System.IO;
using System.Text.RegularExpressions;

namespace OS_kurs
{
    internal static class Program
    {
        static FileSystem sys;
        static string login = "";
        static string password = "";
        static void Main()
        {
            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.White;
            Console.Clear();
            sys = new FileSystem();

            Console.Write("Создать новый диск? Y/N ");
            if (Console.ReadKey().Key == ConsoleKey.Y)
                sys.CreateDrive();
            Console.WriteLine();

            Login();

            while (true)
            {
                Console.Write($"{login}>");

                switch (Console.ReadLine())
                {
                    case string s when Regex.IsMatch(s, @"^touch [a-zA-Z0-9]+\.[a-z]+$"):
                        string fullName = Regex.Replace(s, @"^touch ", "");

                        string name = Path.GetFileNameWithoutExtension(fullName);
                        string expansion = Path.GetExtension(fullName).Split('.')[1];

                        if (name.Length <= 20 && expansion.Length <= 4)
                        {
                            sys.CreateFile(name, expansion);
                            Console.WriteLine($"\tФайл \"{fullName}\" успешно создан!");
                        }
                        break;

                    case string s when Regex.IsMatch(s, @"^mkdir [a-zA-Z0-9]+$"):
                        string dirName = Regex.Replace(s, @"^mkdir ", "");

                        if (dirName.Length <= 20)
                        {
                            sys.CreateDir(dirName);
                            Console.WriteLine($"\tДиректория \"{dirName}\" успешно создана!");
                        }
                        break;

                    case "ls":
                        Console.WriteLine("\tСодержимое текущей директории:");
                        Console.Write(sys.ReadDirectory());
                        break;

                    case string s when Regex.IsMatch(s, @"^rm [a-zA-Z0-9]+\.[a-z]+$"):
                        string rmF = Regex.Replace(s, @"^rm ", "");
                        if (rmF.Length <= 25)
                        {
                            if (sys.Remove(rmF))
                                Console.WriteLine($"\tФайл \"{rmF}\" успешно удален!");
                        }
                        break;

                    case string s when Regex.IsMatch(s, @"^rm [a-zA-Z0-9]+$"):
                        string ddirName = Regex.Replace(s, @"^rm ", "");

                        if (ddirName.Length <= 20)
                        {
                            if (sys.RemoveDir(ddirName))
                                Console.WriteLine($"\tДиректория \"{ddirName}\" успешно удалена!");
                        }
                        break;

                    case string s when Regex.IsMatch(s, @"^cd ([a-zA-Z0-9]+|\.)$"):
                        string cdirName = Regex.Replace(s, @"^cd ", "");

                        if (cdirName.Length <= 20)
                            if (sys.ChangeDir(cdirName))
                                Console.WriteLine("\tПереход успешно выполнен!");
                        break;

                    case string s when Regex.IsMatch(s, @"^chmod [r\-][w\-][x\-][r\-][w\-][x\-] [a-zA-Z0-9]+\.[a-z]+$"):
                        string[] chmodv = Regex.Replace(s, @"^chmod ", "").Split(' ');
                        if (sys.ChangeRights(chmodv[0], chmodv[1]))
                            Console.WriteLine("\tПрава успешно изменены!");
                        break;

                    case string s when Regex.IsMatch(s, @"^chmod [r\-][w\-][x\-][r\-][w\-][x\-] [a-zA-Z0-9]+$"):
                        string[] chcmodv = Regex.Replace(s, @"^chmod ", "").Split(' ');
                        if (sys.ChangeRightsDir(chcmodv[0], chcmodv[1]))
                            Console.WriteLine("\tПрава успешно изменены!");
                        break;

                    case string s when Regex.IsMatch(s, @"^cp [a-zA-Z0-9]+\.[a-z]+ [a-zA-Z0-9]+\.[a-z]+$"):
                        string[] cpv = Regex.Replace(s, @"^cp ", "").Split(' ');
                        if (sys.CopyFile(cpv[0], cpv[1]))
                            Console.WriteLine("\tФайл успешно скопирован!");
                        break;

                    case string s when Regex.IsMatch(s, @"^cp [a-zA-Z0-9]+ [a-zA-Z0-9]+$"):
                        string[] dcpv = Regex.Replace(s, @"^cp ", "").Split(' ');
                        if (sys.CopyDir(dcpv[0], dcpv[1]))
                            Console.WriteLine("\tДиректория успешно скопирована!");
                        break;

                    case string s when Regex.IsMatch(s, @"^move [a-zA-Z0-9]+\.[a-z]+ ([a-zA-Z0-9]+|\.)$"):
                        string[] mv = Regex.Replace(s, @"^move ", "").Split(' ');
                        if (sys.MoveFile(mv[0], mv[1]))
                            Console.WriteLine("\tФайл успешно перемещен! Текущая директория изменена!");
                        break;

                    case string s when Regex.IsMatch(s, @"^rename [a-zA-Z0-9]+\.[a-z]+ [a-zA-Z0-9]+\.[a-z]+$"):
                        string[] rv = Regex.Replace(s, @"^rename ", "").Split(' ');
                        if (sys.Rename(rv[0], rv[1]))
                            Console.WriteLine("\tФайл успешно переименован!");
                        break;

                    case string s when Regex.IsMatch(s, @"^rename [a-zA-Z0-9]+ [a-zA-Z0-9]+$"):
                        string[] rdv = Regex.Replace(s, @"^rename ", "").Split(' ');
                        if (sys.RenameDir(rdv[0], rdv[1]))
                            Console.WriteLine("\tДиректория успешно переименована!");
                        break;

                    case string s when Regex.IsMatch(s, @"^echo .+ >> [a-zA-Z0-9]+\.[a-z]+$"):
                        string echoVal = Regex.Replace(s, @"^echo ", "");
                        string values = Regex.Replace(echoVal, @" >> [a-zA-Z0-9]+\.[a-z]+$", "");
                        string ename = Regex.Replace(echoVal, @".+ >> ", "");
                        if (sys.WriteInFile(ename, values))
                            Console.WriteLine("\tДанные успешно записаны!");
                        break;

                    case string s when Regex.IsMatch(s, @"^cat [a-zA-Z0-9]+\.[a-z]+$"):
                        string ca = "";
                        ca = sys.ReadFile(Regex.Replace(s, @"^cat ", ""));
                        if (ca.Length != 0)
                        {
                            Console.WriteLine("\tДанные успешно считаны! Содержимое:");
                            Console.WriteLine(ca);
                        }
                        break;
                    
                    case "lu":
                        Console.Write(sys.GetAllUsers());
                        break;

                    case string s when Regex.IsMatch(s, @"^addu [a-zA-Z0-9]+ [a-zA-Z0-9]+$"):
                        string[] auv = Regex.Replace(s, @"^addu ", "").Split(' ');
                        if (sys.AddUser(auv[0], auv[1]))
                            Console.WriteLine("\tПользователь успешно создан!");
                        break;

                    case string s when Regex.IsMatch(s, @"^chgu [a-zA-Z0-9]+ [0-9]+$"):
                        string[] gv = Regex.Replace(s, @"^chgu ", "").Split(' ');
                        if(sys.ChangeGroup(gv[0], gv[1]))
                            Console.WriteLine("\tГруппа пользователя успешно изменена!");
                        break;
                    
                    case "user":
                        Login();
                        break;

                    case "quit":
                        return;

                    case "help":
// TODO Пресматривать список INode
                        Console.Write(
                            "\ttouch\tСоздает новый файл с именем и расширением указанным в file\n" +
                            "\tmkdir\tСоздает директорию dir\n" +
                            "\tls\tОтображает содержимое текущей директории\n" +
                            "\trm\tУдаляет файл/директорию file\n" +
                            "\tcd\tПереходит в директорию dir\n" +
                            "\tchmod\tИзменяет права доступа файла/директории file на rights\n" + ///and for dir
                            "\tcp\tКопирует файл/директорию oldfile в newfile\n" +
                            "\tmove\tПеремещает файл file в директорию dir\n" + // MB TODO with slash / || TODO Directory
                            "\trename\tИзменяет название файла/директории oldfile на newfile\n" +
                            "\techo\tДописывает text в конец файла file\n" + // TODO BIGDATA
                            "\tcat\tОтображает содержимое файла file\n" + // TODO BIGDATA
                            "\tlu\tОтображает логины пользователей\n" +
                            "\taddu\tДобавляет пользователя с логином login и паролем password\n" +
                            "\tchgu\tИзменяет группу пользователя login на gid\n" +
                            "\tuser\tЗапрашивает логин и пароль для входа в систему\n" +
                            "\tquit\tВыход из системы\n"
                            );
                        break;

                    default:
                        Console.WriteLine("\tНеверная команда!");
                        break;
                }
            }
        }
        static void Login()
        {
            do
            {
                Console.Write("Введите логин: ");
                login = Console.ReadLine();
                Console.Write("Введите пароль: ");
                password = Console.ReadLine();
                if (sys.IsLogin(login, password) == false)
                    Console.WriteLine("Ошибка!");
            } while (!sys.IsLogin(login, password));
            sys.Directory = 60;
        }
    }
}
