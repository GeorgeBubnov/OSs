using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OS_kurs
{
    internal static class Program
    {
        static FileSystem sys;
        static OperatingSystem os;
        static string login = "";
        static string password = "";
        static void Main()
        {
            /*Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());*/

            Console.WriteLine("Hello World! It's GeorgeOS");
            sys = new FileSystem();
            os = new OperatingSystem();

            Login();

            while (true)
            {
                Console.Write($"{login}>");

                switch (Console.ReadLine())
                {
                    case "ls":
                        Console.Write(sys.ReadDirectory());
                        break;

                    case string s when Regex.IsMatch(s, @"^touch [a-zA-Z0-9]+\.[a-z]+$"):
                        string fullName = Regex.Replace(s, @"^touch ", "");

                        string name = Path.GetFileNameWithoutExtension(fullName);
                        string expansion = Path.GetExtension(fullName).Split('.')[1];

                        if (name.Length <= 20 && expansion.Length <= 4)
                            sys.CreateFile(name, expansion);
                        break;

                    case string s when Regex.IsMatch(s, @"^mkdir [a-zA-Z0-9]+$"):
                        string dirName = Regex.Replace(s, @"^mkdir ", "");

                        if (dirName.Length <= 20)
                            sys.CreateDir(dirName);
                        break;

                    case string s when Regex.IsMatch(s, @"^rmdir [a-zA-Z0-9]+$"):
                        string ddirName = Regex.Replace(s, @"^rmdir ", "");

                        if (ddirName.Length <= 20)
                            sys.RemoveDir(ddirName);
                        break;

                    case string s when Regex.IsMatch(s, @"^cd ([a-zA-Z0-9]+|\.)$"):
                        string cdirName = Regex.Replace(s, @"^cd ", "");

                        if (cdirName.Length <= 20)
                            if(sys.ChangeDir(cdirName))
                                Console.WriteLine("Переход прошел успешно!");
                        break;

                    case string s when Regex.IsMatch(s, @"^chmod [r\-][w\-][x\-][r\-][w\-][x\-] [a-zA-Z0-9]+\.[a-z]+$"):
                        string[] chmodv = Regex.Replace(s, @"^chmod ", "").Split(' ');
                        if (sys.ChangeRights(chmodv[0], chmodv[1]))
                            Console.WriteLine("Права изменены успешно!");
                        break;

                    case string s when Regex.IsMatch(s, @"^chmod [r\-][w\-][x\-][r\-][w\-][x\-] [a-zA-Z0-9]+$"):
                        string[] chcmodv = Regex.Replace(s, @"^chmod ", "").Split(' ');
                        if (sys.ChangeRightsDir(chcmodv[0], chcmodv[1]))
                            Console.WriteLine("Права изменены успешно!");
                        break;

                    case string s when Regex.IsMatch(s, @"^cp [a-zA-Z0-9]+\.[a-z]+ [a-zA-Z0-9]+\.[a-z]+$"):
                        string[] cpv = Regex.Replace(s, @"^cp ", "").Split(' ');
                        sys.CopyFile(cpv[0], cpv[1]);
                        break;

                    case string s when Regex.IsMatch(s, @"^move [a-zA-Z0-9]+\.[a-z]+ ([a-zA-Z0-9]+|\.)$"):
                        string[] mv = Regex.Replace(s, @"^move ", "").Split(' ');
                        sys.MoveFile(mv[0], mv[1]);
                        break;

                    case string s when Regex.IsMatch(s, @"^cpdir [a-zA-Z0-9]+ [a-zA-Z0-9]+$"):
                        string[] dcpv = Regex.Replace(s, @"^cpdir ", "").Split(' ');
                        sys.CopyDir(dcpv[0], dcpv[1]);
                        break;

                    case string s when Regex.IsMatch(s, @"^rm [a-zA-Z0-9]+\.[a-z]+$"):
                        sys.Remove(Regex.Replace(s, @"^rm ", ""));
                        break;

                    case string s when Regex.IsMatch(s, @"^echo .+ >> [a-zA-Z0-9]+\.[a-z]+$"):
                        string echoVal = Regex.Replace(s, @"^echo ", "");
                        string values = Regex.Replace(echoVal, @" >> [a-zA-Z0-9]+\.[a-z]+$", "");
                        string ename = Regex.Replace(echoVal, @".+ >> ", "");
                        sys.WriteInFile(ename, values);
                        break;

                    case string s when Regex.IsMatch(s, @"^cat [a-zA-Z0-9]+\.[a-z]+$"):
                        Console.WriteLine(sys.ReadFile(Regex.Replace(s, @"^cat ", "")));
                        break;

                    case string s when Regex.IsMatch(s, @"^rename [a-zA-Z0-9]+\.[a-z]+ [a-zA-Z0-9]+\.[a-z]+$"):
                        string[] rv = Regex.Replace(s, @"^rename ", "").Split(' ');
                        sys.Rename(rv[0], rv[1]);
                        break;

                    case string s when Regex.IsMatch(s, @"^renamedir [a-zA-Z0-9]+ [a-zA-Z0-9]+$"):
                        string[] rdv = Regex.Replace(s, @"^renamedir ", "").Split(' ');
                        sys.RenameDir(rdv[0], rdv[1]);
                        break;

                    case "users":
                        Console.Write(sys.GetAllUsers());
                        break;

                    case string s when Regex.IsMatch(s, @"^adduser [a-zA-Z0-9]+ [a-zA-Z0-9]+$"):
                        string[] auv = Regex.Replace(s, @"^adduser ", "").Split(' ');
                        sys.AddUser(auv[0], auv[1]);
                        break;

                    case "login":
                        Login();
                        break;

                    case string s when Regex.IsMatch(s, @"^chgroup [a-zA-Z0-9]+ [0-9]+$"):
                        string[] gv = Regex.Replace(s, @"^chgroup ", "").Split(' ');
                        sys.ChangeGroup(gv[0], gv[1]);
                        break;

                    case "exit":
                        return;

                    case "help":
// TODO Пресматривать список INode
                        Console.Write(
                            " touch\tСоздает новый файл с именем и расширением указанным в file\n" +
                            " ls\tОтображает содержимое текущей директории\n" +
                            " chmod\tИзменяет права доступа файла file на rights\n" + ///and for dir
                            " cp\tКопирует файл oldfile в newfile\n" +
                            " rm\tУдаляет файл file\n" +
                            " mkdir\tСоздает директорию dir\n" +
                            " cd\tПереходит в директорию dir\n" +
                            " rmdir\tУдаляет директорию dir\n" +
                            " cpdir\tКопирует директорию olddir в newdir\n" + // TODO WITH DATA
                            " echo\tДописывает text в конец файла file\n" + // TODO BIGDATA
                            " cat\tОтображает содержимое файла file\n" + // TODO BIGDATA
                            " move\tПеремещает файл file в директорию dir\n" + // MB TODO with slash / || TODO Directory
                            " rename\tИзменяет название файла oldfile на newfile\n" +
                            " renamedir\tИзменяет название директории olddir на newdir\n" +
                            " users\tОтображает информацию о пользователях\n" +
                            " adduser\tДобавляет пользователя с логином login и паролем password\n" +
                            " login\tЗапрашивает логин и пароль для входа в систему\n" +
                            " chgroup\tИзменяет группу пользователя login на gid\n" +
                            "\texit\tВыход из системы\n" +
                            " ps\tОтображает информацию о процессах\n" +
                            " kill\tУничтожает процесс с идентификатором pid\n" +
                            " mp\tСоздает новый процесс с заданным временем работы time и приоритетом pri\n" +
                            " chpt\tИзменяет время работы процесса с идентификатором pid на time\n" +
                            " chpp\tИзменяет приоритет процесса с идентификатором pid на pri\n" +
                            " gen\tГенерирует count случайных процессов\n" +
                            " top\tОтображает информацию о процессах в реальном времени\n"
                            );
                        break;

                    case "ps":
                        Console.Write(os.GetProcess());
                        break;

                    case string s when Regex.IsMatch(s, @"^kill \d+$"):
                        os.Remove(Convert.ToInt32(Regex.Replace(s, @"^kill ", "")));
                        break;

                    case string s when Regex.IsMatch(s, @"^mp \d+( \d+)?$"):
                        string makep = Regex.Replace(s, @"^mp ", "");
                        if (Regex.IsMatch(s, @"^mp \d+ \d+$"))
                        {
                            int time = int.Parse(makep.Split(' ')[0]);
                            sbyte pri = sbyte.Parse(makep.Split(' ')[1]);
                            os.AddNewProcess(time, pri);
                        }
                        else
                        {
                            int time = int.Parse(makep);
                            os.AddNewProcess(time);
                        }
                        break;

                    case string s when Regex.IsMatch(s, @"^chpt \d+ \d+$"):
                        string cht = Regex.Replace(s, @"^chpt ", "");
                        {
                            int id = int.Parse(cht.Split(' ')[0]);
                            int time = int.Parse(cht.Split(' ')[1]);
                            os.ChangeTime(id, time);
                        }
                        break;

                    case string s when Regex.IsMatch(s, @"^chpp \d+ ( |-)\d+$"):
                        string chp = Regex.Replace(s, @"^chpp ", "");
                        {
                            var id = int.Parse(chp.Split(' ')[0]);
                            var pri = sbyte.Parse(chp.Split(' ')[1]);
                            os.ChangePriorety(id, pri);
                        }
                        break;

                    case string s when Regex.IsMatch(s, @"^gen \d+$"):
                        int count = int.Parse(Regex.Replace(s, @"^gen ", ""));
                        {
                            Random random = new Random();
                            while (count-- > 0)
                                os.AddNewProcess(random.Next(0, 1000), (sbyte)random.Next(-20, 19));
                        }
                        break;
                    
                    case "top":
                        WriteTopProcess();
                        while (os.IsNotEmpty())
                            Console.ReadKey(true);
                        break;

                    default:
                        Console.WriteLine("Команда не найдена");
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
                    Console.WriteLine("Ошибка! Неверное значение\n");
            } while (!sys.IsLogin(login, password));
            sys.Directory = 60;
        }
        static async void WriteTopProcess()
        {
            Console.Clear();
            await Task.Run(() =>
            {
                while (os.IsNotEmpty())
                {
                    Console.Write(os.GetProcess());
                    Thread.Sleep(500);
                    Console.Clear();
                }
            });
        }
    }
}
