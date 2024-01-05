using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OS_kurs.FS
{
    internal class UserNode
    {
        public static byte IDSize = 1;
        public static byte GroupIDSize = 1;
        public static byte LoginSize = 20;
        public static byte PasswordSize = 20;

        public byte ID;
        public byte GroupID;
        public string Login;
        public string Password;

        public UserNode()
        {
            ID = 0;
            GroupID = 0;
            Login = "root";
            Password = "123";
        }
    }
}
