using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OS_kurs.FS
{
    internal class FileNode
    {
        public static byte NameSize = 20;
        public static byte ExpansionSize = 4;

        public string Name;
        public string Expansion;

        public FileNode()
        {
            Name = "rootDir";
            Expansion = "dir";
        }
    }
}
