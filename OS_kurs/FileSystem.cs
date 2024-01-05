using OS_kurs.FS;
using System;
using System.IO;
using System.Text;

namespace OS_kurs
{
    class FileSystem
    {
        string path = "Drive";
        public byte UserID = 0;
        public byte GroupID = 0;
        public UInt16 Directory = 60;

        public INode[] IList = new INode[SuperBlock.IListSize];
        
        public FileSystem()
        {
            CreateDrive();
        }
        public bool ChangeDir(string name)
        {
            if(name != ".")
            {
                byte[] files = ReadFileBlock(Directory);
                using (FileStream fs = new FileStream(path, FileMode.Open))
                {
                    byte[] two = new byte[2];
                    byte[] eight = new byte[8];
                    byte[] one = new byte[1];
                    byte[] twenty = new byte[20];
                    byte[] four = new byte[4];
                    byte[] thirty = new byte[30];

                    fs.Seek(Directory + 10, SeekOrigin.Begin); // Считаем размер директории
                    fs.Read(two, 0, 2);
                    UInt16 count = (UInt16)BitConverter.ToInt16(two, 0);

                    for (int i = 0; i < count; i += 2)
                    {
                        string res = "";
                        two[0] = files[i];
                        two[1] = files[i + 1];

                        UInt16 addr = (UInt16)BitConverter.ToInt16(two, 0);

                        fs.Seek(addr, SeekOrigin.Begin); // Получаем права доступа
                        fs.Read(eight, 0, 8);
                        string rights = GetValidString(eight);

                        fs.Seek(addr + 8, SeekOrigin.Begin); // Получаем UserID
                        fs.Read(one, 0, 1);
                        UInt16 temp = (UInt16)one[0];

                        fs.Seek(addr + 9, SeekOrigin.Begin); // Получаем UserID
                        fs.Read(one, 0, 1);
                        UInt16 gid = (UInt16)one[0];

                        if (!(rights[2] == 'r' && temp == UserID) && !(rights[5] == 'r' && gid == GroupID)) // Проверяет права доступа
                            continue;


                        fs.Seek(addr + 30, SeekOrigin.Begin); // Получаем BlockAddress
                        fs.Read(two, 0, 2);
                        temp = (UInt16)BitConverter.ToInt16(two, 0);

                        fs.Seek(temp, SeekOrigin.Begin); // Считываем название и расширение
                        fs.Read(twenty, 0, 20);
                        res += GetValidString(twenty);

                        if (res == name)
                        {
                            Directory = addr;
                            return true;
                        }
                    }
                }
            }
            else
            {
                using (FileStream fs = new FileStream(path, FileMode.Open))
                {
                    byte[] buffer = new byte[2];
                    fs.Seek(Directory + 30, SeekOrigin.Begin); // Получаем BlockAddress
                    fs.Read(buffer, 0, 2);
                    UInt16 temp = (UInt16)BitConverter.ToInt16(buffer, 0);

                    fs.Seek(temp + 22, SeekOrigin.Begin); // Считываем предыдую директорию
                    fs.Read(buffer, 0, 2);
                    Directory = (UInt16)BitConverter.ToInt16(buffer, 0);
                    return true;
                }
                    
            }
            return false;
        }
        public void CreateDir(string name)
        {
            if (Directory != 60)
            {
                byte[] eight = new byte[8];
                byte[] one = new byte[1];
                using (FileStream fs = new FileStream(path, FileMode.Open))
                {
                    fs.Seek(Directory, SeekOrigin.Begin); // Получаем права доступа
                    fs.Read(eight, 0, 8);
                    string rights = GetValidString(eight);

                    fs.Seek(Directory + 8, SeekOrigin.Begin); // Получаем UserID
                    fs.Read(one, 0, 1);
                    UInt16 temp = (UInt16)one[0];

                    fs.Seek(Directory + 9, SeekOrigin.Begin); // Получаем UserID
                    fs.Read(one, 0, 1);
                    UInt16 gid = (UInt16)one[0];

                    if (!(rights[3] == 'w' && temp == UserID) && !(rights[6] == 'w' && gid == GroupID)) // Проверяет права доступа
                        return;
                }
            }

            UInt16 addr = WriteNewINode("TDrw----");
            WriteNewFullName(addr, name, "dir");
            WriteDataInBlock(Directory, BitConverter.GetBytes(addr));
        }
        public bool Remove(string fullname)
        {
            byte[] files = ReadFileBlock(Directory);
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                byte[] two = new byte[2];
                byte[] eight = new byte[8];
                byte[] one = new byte[1];
                byte[] twenty = new byte[20];
                byte[] four = new byte[4];
                byte[] thirty = new byte[30];

                fs.Seek(Directory + 10, SeekOrigin.Begin); // Считаем размер директории
                fs.Read(two, 0, 2);
                UInt16 count = (UInt16)BitConverter.ToInt16(two, 0);

                for (int i = 0; i < count; i += 2)
                {
                    string res = "";
                    two[0] = files[i];
                    two[1] = files[i + 1];

                    UInt16 addr = (UInt16)BitConverter.ToInt16(two, 0);

                    fs.Seek(addr + 8, SeekOrigin.Begin); // Получаем UserID
                    fs.Read(one, 0, 1);
                    UInt16 temp = (UInt16)one[0];
                    if (temp == UserID || 0 == UserID)
                    {
                        fs.Seek(addr + 30, SeekOrigin.Begin); // Получаем BlockAddress
                        fs.Read(two, 0, 2);
                        temp = (UInt16)BitConverter.ToInt16(two, 0);

                        fs.Seek(temp, SeekOrigin.Begin); // Считываем название и расширение
                        fs.Read(twenty, 0, 20);
                        res += GetValidString(twenty);
                        fs.Read(four, 0, 4);
                        res += "." + GetValidString(four);

                        if (res == fullname)
                        {
                            fs.Seek(addr, SeekOrigin.Begin); // Затираем INode
                            fs.Write(BitConverter.GetBytes((UInt16)0), 0, 2);

                            fs.Seek(14, SeekOrigin.Begin); // Увеличваем FreeINodeCount
                            fs.Read(two, 0, 2);
                            UInt16 freeINodeCount = (UInt16)BitConverter.ToInt16(two, 0);
                            freeINodeCount += 1;
                            fs.Seek(-2, SeekOrigin.Current);
                            fs.Write(BitConverter.GetBytes(freeINodeCount), 0, 2);

                            fs.Seek(Directory + 10, SeekOrigin.Begin); // Считаем размер директории
                            count -= 2;
                            fs.Write(BitConverter.GetBytes(count), 0, 2);

                            fs.Seek(Directory + 30, SeekOrigin.Begin);
                            fs.Read(two, 0, 2);
                            UInt16 blockOffset = (UInt16)BitConverter.ToInt16(two, 0);

                            if(count - i > 0)
                            {
                                fs.Seek(blockOffset + 26 + i, SeekOrigin.Begin); // Переходим на последующие адреса
                                byte[] buffer = new byte[count - i];
                                fs.Read(buffer, 0, count - i);
                                fs.Seek(-2, SeekOrigin.Current);
                                fs.Write(BitConverter.GetBytes((UInt16)0), 0, 2);

                                fs.Seek(blockOffset + 24 + i, SeekOrigin.Begin); // Переходим на место адреса удаляемого файла
                                fs.Write(buffer, 0, count - i);
                            }
                            else
                            {
                                fs.Seek(blockOffset + 24 + i, SeekOrigin.Begin); // Переходим на место адреса удаляемого файла
                                fs.Write(BitConverter.GetBytes((UInt16)0), 0, 2);
                            }
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public bool Rename(string name, string newName)
        {
            byte[] files = ReadFileBlock(Directory);
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                byte[] two = new byte[2];
                byte[] eight = new byte[8];
                byte[] one = new byte[1];
                byte[] twenty = new byte[20];
                byte[] four = new byte[4];
                byte[] thirty = new byte[30];

                fs.Seek(Directory + 10, SeekOrigin.Begin); // Считаем размер директории
                fs.Read(two, 0, 2);
                UInt16 count = (UInt16)BitConverter.ToInt16(two, 0);

                for (int i = 0; i < count; i += 2)
                {
                    string res = "";
                    two[0] = files[i];
                    two[1] = files[i + 1];

                    UInt16 inode = (UInt16)BitConverter.ToInt16(two, 0);

                    fs.Seek(inode + 8, SeekOrigin.Begin); // Получаем UserID
                    fs.Read(one, 0, 1);
                    UInt16 temp = (UInt16)one[0];
                    if (temp == UserID || 0 == UserID)
                    {
                        fs.Seek(inode + 30, SeekOrigin.Begin); // Получаем BlockAddress
                        fs.Read(two, 0, 2);
                        temp = (UInt16)BitConverter.ToInt16(two, 0);

                        fs.Seek(temp, SeekOrigin.Begin); // Считываем название и расширение
                        fs.Read(twenty, 0, 20);
                        res += GetValidString(twenty);
                        fs.Read(four, 0, 4);
                        res += "." + GetValidString(four);

                        if (res == name)
                        {
                            fs.Seek(temp, SeekOrigin.Begin);
                            for (int j = 0; j < 24; j += 2)
                                fs.Write(BitConverter.GetBytes((UInt16)0), 0, 2);
                            string nname = Path.GetFileNameWithoutExtension(newName);
                            string expansion = Path.GetExtension(newName).Split('.')[1];
                            fs.Seek(temp, SeekOrigin.Begin);
                            fs.Write(Encoding.UTF8.GetBytes(nname), 0, nname.Length);
                            fs.Seek(temp + 20, SeekOrigin.Begin);
                            fs.Write(Encoding.UTF8.GetBytes(expansion), 0, expansion.Length);
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public bool RenameDir(string name, string newName)
        {
            byte[] files = ReadFileBlock(Directory);
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                byte[] two = new byte[2];
                byte[] eight = new byte[8];
                byte[] one = new byte[1];
                byte[] twenty = new byte[20];
                byte[] four = new byte[4];
                byte[] thirty = new byte[30];

                fs.Seek(Directory + 10, SeekOrigin.Begin); // Считаем размер директории
                fs.Read(two, 0, 2);
                UInt16 count = (UInt16)BitConverter.ToInt16(two, 0);

                for (int i = 0; i < count; i += 2)
                {
                    string res = "";
                    two[0] = files[i];
                    two[1] = files[i + 1];

                    UInt16 inode = (UInt16)BitConverter.ToInt16(two, 0);

                    fs.Seek(inode + 8, SeekOrigin.Begin); // Получаем UserID
                    fs.Read(one, 0, 1);
                    UInt16 temp = (UInt16)one[0];
                    if (temp == UserID || 0 == UserID)
                    {
                        fs.Seek(inode + 30, SeekOrigin.Begin); // Получаем BlockAddress
                        fs.Read(two, 0, 2);
                        temp = (UInt16)BitConverter.ToInt16(two, 0);

                        fs.Seek(temp, SeekOrigin.Begin); // Считываем название и расширение
                        fs.Read(twenty, 0, 20);
                        res += GetValidString(twenty);
                        fs.Seek(temp, SeekOrigin.Begin); // Считываем название и расширение
                        fs.Read(twenty, 0, 20);
                        res = GetValidString(twenty);
                        fs.Read(two, 0, 2);
                        UInt16 expansiton = (UInt16)BitConverter.ToInt16(two, 0);

                        if (res == name && expansiton == 0)
                        {
                            fs.Seek(temp, SeekOrigin.Begin);
                            for (int j = 0; j < 20; j += 2)
                                fs.Write(BitConverter.GetBytes((UInt16)0), 0, 2);
                            fs.Seek(temp, SeekOrigin.Begin);
                            fs.Write(Encoding.UTF8.GetBytes(newName), 0, newName.Length);
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public bool MoveFile(string name, string dir)
        {
            byte[] files = ReadFileBlock(Directory);
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                byte[] two = new byte[2];
                byte[] eight = new byte[8];
                byte[] one = new byte[1];
                byte[] twenty = new byte[20];
                byte[] four = new byte[4];
                byte[] thirty = new byte[30];

                fs.Seek(Directory + 10, SeekOrigin.Begin); // Считаем размер директории
                fs.Read(two, 0, 2);
                UInt16 count = (UInt16)BitConverter.ToInt16(two, 0);

                for (int i = 0; i < count; i += 2)
                {
                    string res = "";
                    two[0] = files[i];
                    two[1] = files[i + 1];

                    UInt16 inode = (UInt16)BitConverter.ToInt16(two, 0);

                    fs.Seek(inode + 8, SeekOrigin.Begin); // Получаем UserID
                    fs.Read(one, 0, 1);
                    UInt16 temp = (UInt16)one[0];
                    if (temp == UserID || 0 == UserID)
                    {
                        fs.Seek(inode + 30, SeekOrigin.Begin); // Получаем BlockAddress
                        fs.Read(two, 0, 2);
                        temp = (UInt16)BitConverter.ToInt16(two, 0);

                        fs.Seek(temp, SeekOrigin.Begin); // Считываем название и расширение
                        fs.Read(twenty, 0, 20);
                        res += GetValidString(twenty);
                        fs.Read(four, 0, 4);
                        res += "." + GetValidString(four);

                        if (res == name)
                        {
                            fs.Seek(Directory + 10, SeekOrigin.Begin); // Считаем размер директории
                            count -= 2;
                            fs.Write(BitConverter.GetBytes(count), 0, 2);

                            fs.Seek(Directory + 30, SeekOrigin.Begin);
                            fs.Read(two, 0, 2);
                            UInt16 blockOffset = (UInt16)BitConverter.ToInt16(two, 0);

                            if (count - i > 0)
                            {
                                fs.Seek(blockOffset + 26 + i, SeekOrigin.Begin); // Переходим на последующие адреса
                                byte[] buffer = new byte[count - i];
                                fs.Read(buffer, 0, count - i);
                                fs.Seek(-2, SeekOrigin.Current);
                                fs.Write(BitConverter.GetBytes((UInt16)0), 0, 2);

                                fs.Seek(blockOffset + 24 + i, SeekOrigin.Begin); // Переходим на место адреса удаляемого файла
                                fs.Write(buffer, 0, count - i);
                            }
                            else
                            {
                                fs.Seek(blockOffset + 24 + i, SeekOrigin.Begin); // Переходим на место адреса удаляемого файла
                                fs.Write(BitConverter.GetBytes((UInt16)0), 0, 2);
                            }

                            fs.Close();
                            ChangeDir(dir);
                            WriteDataInBlock(Directory, BitConverter.GetBytes(inode));
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public bool RemoveDir(string fullname)
        {
            byte[] files = ReadFileBlock(Directory);
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                byte[] two = new byte[2];
                byte[] eight = new byte[8];
                byte[] one = new byte[1];
                byte[] twenty = new byte[20];
                byte[] four = new byte[4];
                byte[] thirty = new byte[30];

                fs.Seek(Directory + 10, SeekOrigin.Begin); // Считаем размер директории
                fs.Read(two, 0, 2);
                UInt16 count = (UInt16)BitConverter.ToInt16(two, 0);

                for (int i = 0; i < count; i += 2)
                {
                    two[0] = files[i];
                    two[1] = files[i + 1];

                    UInt16 addr = (UInt16)BitConverter.ToInt16(two, 0);

                    fs.Seek(addr + 8, SeekOrigin.Begin); // Получаем UserID
                    fs.Read(one, 0, 1);
                    UInt16 temp = (UInt16)one[0];
                    if (temp == UserID || 0 == UserID)
                    {
                        fs.Seek(addr + 30, SeekOrigin.Begin); // Получаем BlockAddress
                        fs.Read(two, 0, 2);
                        temp = (UInt16)BitConverter.ToInt16(two, 0);

                        fs.Seek(temp, SeekOrigin.Begin); // Считываем название и расширение
                        fs.Read(twenty, 0, 20);
                        string name = GetValidString(twenty);
                        fs.Read(two, 0, 2);
                        UInt16 expansiton = (UInt16)BitConverter.ToInt16(two, 0); ;

                        if (name == fullname && expansiton == 0)
                        {
                            fs.Seek(addr, SeekOrigin.Begin); // Затираем INode
                            fs.Write(BitConverter.GetBytes((UInt16)0), 0, 2);

                            fs.Seek(14, SeekOrigin.Begin); // Увеличваем FreeINodeCount
                            fs.Read(two, 0, 2);
                            UInt16 freeINodeCount = (UInt16)BitConverter.ToInt16(two, 0);
                            freeINodeCount += 1;
                            fs.Seek(-2, SeekOrigin.Current);
                            fs.Write(BitConverter.GetBytes(freeINodeCount), 0, 2);

                            fs.Seek(Directory + 10, SeekOrigin.Begin); // Считаем размер директории
                            count -= 2;
                            fs.Write(BitConverter.GetBytes(count), 0, 2);

                            fs.Seek(Directory + 30, SeekOrigin.Begin);
                            fs.Read(two, 0, 2);
                            UInt16 blockOffset = (UInt16)BitConverter.ToInt16(two, 0);

                            if (count - i > 0)
                            {
                                fs.Seek(blockOffset + 26 + i, SeekOrigin.Begin); // Переходим на последующие адреса
                                byte[] buffer = new byte[count - i];
                                fs.Read(buffer, 0, count - i);
                                fs.Seek(-2, SeekOrigin.Current);
                                fs.Write(BitConverter.GetBytes((UInt16)0), 0, 2);

                                fs.Seek(blockOffset + 24 + i, SeekOrigin.Begin); // Переходим на место адреса удаляемого файла
                                fs.Write(buffer, 0, count - i);
                            }
                            else
                            {
                                fs.Seek(blockOffset + 24 + i, SeekOrigin.Begin); // Переходим на место адреса удаляемого файла
                                fs.Write(BitConverter.GetBytes((UInt16)0), 0, 2);
                            }
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public bool CopyFile(string name1, string name2)
        {
            byte[] files = ReadFileBlock(Directory);
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                byte[] two = new byte[2];
                byte[] eight = new byte[8];
                byte[] one = new byte[1];
                byte[] twenty = new byte[20];
                byte[] four = new byte[4];
                byte[] thirty = new byte[30];

                fs.Seek(Directory + 10, SeekOrigin.Begin); // Считаем размер директории
                fs.Read(two, 0, 2);
                UInt16 count = (UInt16)BitConverter.ToInt16(two, 0);

                for (int i = 0; i < count; i += 2)
                {
                    string res = "";
                    two[0] = files[i];
                    two[1] = files[i + 1];

                    UInt16 addr = (UInt16)BitConverter.ToInt16(two, 0);

                    fs.Seek(Directory, SeekOrigin.Begin); // Получаем права доступа
                    fs.Read(eight, 0, 8);
                    string rights = GetValidString(eight);

                    fs.Seek(Directory + 8, SeekOrigin.Begin); // Получаем UserID
                    fs.Read(one, 0, 1);
                    UInt16 temp = (UInt16)one[0];

                    fs.Seek(Directory + 9, SeekOrigin.Begin); // Получаем UserID
                    fs.Read(one, 0, 1);
                    UInt16 gid = (UInt16)one[0];

                    if (!(rights[3] == 'w' && temp == UserID) && !(rights[6] == 'w' && gid == GroupID)) // Проверяет права доступа
                        continue;

                    fs.Seek(addr + 8, SeekOrigin.Begin); // Получаем UserID
                    fs.Read(one, 0, 1);
                    temp = (UInt16)one[0];
                    
                    fs.Seek(addr + 30, SeekOrigin.Begin); // Получаем BlockAddress
                    fs.Read(two, 0, 2);
                    temp = (UInt16)BitConverter.ToInt16(two, 0);

                    fs.Seek(temp, SeekOrigin.Begin); // Считываем название и расширение
                    fs.Read(twenty, 0, 20);
                    res = GetValidString(twenty);
                    fs.Read(four, 0, 4);
                    res += "." + GetValidString(four);

                    if (res == name1)
                    {
                        fs.Seek(addr, SeekOrigin.Begin); // Получаем данные из копируемого INode
                        fs.Read(thirty, 0, 30);

                        int j = 0;
                        UInt16 addrNew = 0;
                        while (j < 20 && addrNew == 0)
                        {
                            fs.Seek(18 + j * 2, SeekOrigin.Begin);
                            fs.Read(two, 0, 2);
                            addrNew = (UInt16)BitConverter.ToInt16(two, 0);
                            j++;
                        }
                        if (addrNew != 0)
                        {
                            // Superblock
                            fs.Seek(-2, SeekOrigin.Current); // Затираем адрес в ListINode
                            fs.Write(BitConverter.GetBytes((UInt16)0), 0, 2);

                            fs.Seek(14, SeekOrigin.Begin); // Уменьшаем FreeINodeCount
                            fs.Read(two, 0, 2);
                            UInt16 freeINodeCount = (UInt16)BitConverter.ToInt16(two, 0);
                            freeINodeCount -= 1;
                            fs.Seek(-2, SeekOrigin.Current);
                            fs.Write(BitConverter.GetBytes(freeINodeCount), 0, 2);

                            //INode
                            fs.Seek(addrNew, SeekOrigin.Begin);
                            fs.Write(thirty, 0, 30);

                            string name = Path.GetFileNameWithoutExtension(name2);
                            string expansion = Path.GetExtension(name2).Split('.')[1];
                            fs.Close();
                            WriteNewFullName(addrNew, name, expansion);
                            WriteDataInBlock(Directory, BitConverter.GetBytes(addrNew));
                            string data = ReadFile(name1);
                            WriteInFile(name2, data);
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public string ReadFile(string fullname)
        {
            byte[] files = ReadFileBlock(Directory);
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                byte[] two = new byte[2];
                byte[] eight = new byte[8];
                byte[] one = new byte[1];
                byte[] twenty = new byte[20];
                byte[] four = new byte[4];
                byte[] thirty = new byte[30];

                fs.Seek(Directory + 10, SeekOrigin.Begin); // Считаем размер директории
                fs.Read(two, 0, 2);
                UInt16 count = (UInt16)BitConverter.ToInt16(two, 0);

                for (int i = 0; i < count; i += 2)
                {
                    string res = "";
                    two[0] = files[i];
                    two[1] = files[i + 1];

                    UInt16 addr = (UInt16)BitConverter.ToInt16(two, 0);

                    fs.Seek(addr, SeekOrigin.Begin); // Получаем права доступа
                    fs.Read(eight, 0, 8);
                    string rights = GetValidString(eight);

                    fs.Seek(addr + 8, SeekOrigin.Begin); // Получаем UserID
                    fs.Read(one, 0, 1);
                    UInt16 temp = (UInt16)one[0];

                    fs.Seek(addr + 9, SeekOrigin.Begin); // Получаем UserID
                    fs.Read(one, 0, 1);
                    UInt16 gid = (UInt16)one[0];

                    if (!(rights[2] == 'r' && temp == UserID) && !(rights[5] == 'r' && gid == GroupID)) // Проверяет права доступа
                        continue;

                    fs.Seek(addr + 8, SeekOrigin.Begin); // Получаем UserID
                    fs.Read(one, 0, 1);
                    temp = (UInt16)one[0];
                    
                    fs.Seek(addr + 30, SeekOrigin.Begin); // Получаем BlockAddress
                    fs.Read(two, 0, 2);
                    temp = (UInt16)BitConverter.ToInt16(two, 0);

                    fs.Seek(temp, SeekOrigin.Begin); // Считываем название и расширение
                    fs.Read(twenty, 0, 20);
                    res += GetValidString(twenty);
                    fs.Read(four, 0, 4);
                    res += "." + GetValidString(four);

                    if (res == fullname)
                    {
                        fs.Close();
                        byte[] value = ReadFileBlock(addr);
                        return GetValidString(value);
                    }
                }
            }
            return "";
        }
        public bool WriteInFile(string fullname, string value)
        {
            byte[] files = ReadFileBlock(Directory);
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                byte[] two = new byte[2];
                byte[] eight = new byte[8];
                byte[] one = new byte[1];
                byte[] twenty = new byte[20];
                byte[] four = new byte[4];
                byte[] thirty = new byte[30];

                fs.Seek(Directory + 10, SeekOrigin.Begin); // Считаем размер директории
                fs.Read(two, 0, 2);
                UInt16 count = (UInt16)BitConverter.ToInt16(two, 0);

                for (int i = 0; i < count; i += 2)
                {
                    string res = "";
                    two[0] = files[i];
                    two[1] = files[i + 1];

                    UInt16 addr = (UInt16)BitConverter.ToInt16(two, 0);

                    fs.Seek(addr, SeekOrigin.Begin); // Получаем права доступа
                    fs.Read(eight, 0, 8);
                    string rights = GetValidString(eight);

                    fs.Seek(addr + 8, SeekOrigin.Begin); // Получаем UserID
                    fs.Read(one, 0, 1);
                    UInt16 temp = (UInt16)one[0];

                    fs.Seek(addr + 9, SeekOrigin.Begin); // Получаем UserID
                    fs.Read(one, 0, 1);
                    UInt16 gid = (UInt16)one[0];

                    if (!(rights[3] == 'w' && temp == UserID) && !(rights[6] == 'w' && gid == GroupID)) // Проверяет права доступа
                        continue;

                    fs.Seek(addr + 30, SeekOrigin.Begin); // Получаем BlockAddress
                    fs.Read(two, 0, 2);
                    temp = (UInt16)BitConverter.ToInt16(two, 0);

                    fs.Seek(temp, SeekOrigin.Begin); // Считываем название и расширение
                    fs.Read(twenty, 0, 20);
                    res += GetValidString(twenty);
                    fs.Read(four, 0, 4);
                    res += "." + GetValidString(four);

                    if (res == fullname)
                    {
                        fs.Seek(addr + 10, SeekOrigin.Begin); // Считываем SizeInBytes
                        fs.Read(two, 0, 2);
                        UInt16 sizeInBytes = (UInt16)BitConverter.ToInt16(two, 0);

                        fs.Read(two, 0, 2); // Считываем sizeInBlocks TODO
                        UInt16 sizeInBlocks = (UInt16)BitConverter.ToInt16(two, 0);

                        if (512 - sizeInBytes - 24 > value.Length)
                        {
                            fs.Seek(temp + 24 + sizeInBytes, SeekOrigin.Begin);
                            fs.Write(Encoding.UTF8.GetBytes(value), 0, value.Length);

                            fs.Seek(addr + 10, SeekOrigin.Begin); // Увеличиваем SizeInBytes
                            fs.Write(BitConverter.GetBytes((UInt16)(sizeInBytes + value.Length)), 0, 2);
                        }
                        return true;
                    }
                }
            }
            return false;
        }
        public bool CopyDir(string name1, string name2)
        {
            byte[] files = ReadFileBlock(Directory);
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                byte[] two = new byte[2];
                byte[] eight = new byte[8];
                byte[] one = new byte[1];
                byte[] twenty = new byte[20];
                byte[] four = new byte[4];
                byte[] thirty = new byte[30];

                fs.Seek(Directory + 10, SeekOrigin.Begin); // Считаем размер директории
                fs.Read(two, 0, 2);
                UInt16 count = (UInt16)BitConverter.ToInt16(two, 0);

                for (int i = 0; i < count; i += 2)
                {
                    string res = "";
                    two[0] = files[i];
                    two[1] = files[i + 1];

                    UInt16 addr = (UInt16)BitConverter.ToInt16(two, 0);

                    fs.Seek(Directory, SeekOrigin.Begin); // Получаем права доступа
                    fs.Read(eight, 0, 8);
                    string rights = GetValidString(eight);

                    fs.Seek(Directory + 8, SeekOrigin.Begin); // Получаем UserID
                    fs.Read(one, 0, 1);
                    UInt16 temp = (UInt16)one[0];

                    fs.Seek(Directory + 9, SeekOrigin.Begin); // Получаем UserID
                    fs.Read(one, 0, 1);
                    UInt16 gid = (UInt16)one[0];

                    if (!(rights[3] == 'w' && temp == UserID) && !(rights[6] == 'w' && gid == GroupID)) // Проверяет права доступа
                        continue;

                    fs.Seek(addr + 8, SeekOrigin.Begin); // Получаем UserID
                    fs.Read(one, 0, 1);
                    temp = (UInt16)one[0];
                    
                fs.Seek(addr + 30, SeekOrigin.Begin); // Получаем BlockAddress
                    fs.Read(two, 0, 2);
                    temp = (UInt16)BitConverter.ToInt16(two, 0);

                    fs.Seek(temp, SeekOrigin.Begin); // Считываем название и расширение
                    fs.Read(twenty, 0, 20);
                    res += GetValidString(twenty);

                    if (res == name1)
                    {
                        fs.Seek(addr, SeekOrigin.Begin); // Получаем данные из копируемого INode
                        fs.Read(thirty, 0, 30);

                        int j = 0;
                        UInt16 addrNew = 0;
                        while (j < 20 && addrNew == 0)
                        {
                            fs.Seek(18 + j * 2, SeekOrigin.Begin);
                            fs.Read(two, 0, 2);
                            addrNew = (UInt16)BitConverter.ToInt16(two, 0);
                            j++;
                        }
                        if (addrNew != 0)
                        {
                            // Superblock
                            fs.Seek(-2, SeekOrigin.Current); // Затираем адрес в ListINode
                            fs.Write(BitConverter.GetBytes((UInt16)0), 0, 2);

                            fs.Seek(14, SeekOrigin.Begin); // Уменьшаем FreeINodeCount
                            fs.Read(two, 0, 2);
                            UInt16 freeINodeCount = (UInt16)BitConverter.ToInt16(two, 0);
                            freeINodeCount -= 1;
                            fs.Seek(-2, SeekOrigin.Current);
                            fs.Write(BitConverter.GetBytes(freeINodeCount), 0, 2);

                            //INode
                            fs.Seek(addrNew, SeekOrigin.Begin);
                            fs.Write(thirty, 0, 30);

                            fs.Close();
                            WriteNewFullName(addrNew, name2, "dir");
                            WriteDataInBlock(Directory, BitConverter.GetBytes(addrNew));
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public bool ChangeRights(string rights, string fullname)
        {
            byte[] files = ReadFileBlock(Directory);
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                byte[] two = new byte[2];
                byte[] eight = new byte[8];
                byte[] one = new byte[1];
                byte[] twenty = new byte[20];
                byte[] four = new byte[4];

                fs.Seek(Directory + 10, SeekOrigin.Begin); // Считаем размер директории
                fs.Read(two, 0, 2);
                UInt16 count = (UInt16)BitConverter.ToInt16(two, 0);

                for (int i = 0; i < count; i += 2)
                {
                    string res = "";
                    two[0] = files[i];
                    two[1] = files[i + 1];

                    UInt16 addr = (UInt16)BitConverter.ToInt16(two, 0);

                    fs.Seek(addr + 8, SeekOrigin.Begin); // Получаем UserID
                    fs.Read(one, 0, 1);
                    UInt16 temp = (UInt16)one[0];
                    if (temp == UserID || 0 == UserID)
                    {
                        fs.Seek(addr + 30, SeekOrigin.Begin); // Получаем BlockAddress
                        fs.Read(two, 0, 2);
                        temp = (UInt16)BitConverter.ToInt16(two, 0);

                        fs.Seek(temp, SeekOrigin.Begin); // Считываем название и расширение
                        fs.Read(twenty, 0, 20);
                        res += GetValidString(twenty);
                        fs.Read(four, 0, 4);
                        res += "." + GetValidString(four);

                        if(res == fullname)
                        {
                            fs.Seek(addr + 2, SeekOrigin.Begin);
                            fs.Write(Encoding.UTF8.GetBytes(rights), 0, INode.AccessSize - 2);
                            return true;
                        }
                    }
                }
                return false;
            }
        }
        public bool ChangeRightsDir(string rights, string fullname)
        {
            byte[] files = ReadFileBlock(Directory);
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                byte[] two = new byte[2];
                byte[] eight = new byte[8];
                byte[] one = new byte[1];
                byte[] twenty = new byte[20];
                byte[] four = new byte[4];

                fs.Seek(Directory + 10, SeekOrigin.Begin); // Считаем размер директории
                fs.Read(two, 0, 2);
                UInt16 count = (UInt16)BitConverter.ToInt16(two, 0);

                for (int i = 0; i < count; i += 2)
                {
                    string res = "";
                    two[0] = files[i];
                    two[1] = files[i + 1];

                    UInt16 addr = (UInt16)BitConverter.ToInt16(two, 0);

                    fs.Seek(addr + 8, SeekOrigin.Begin); // Получаем UserID
                    fs.Read(one, 0, 1);
                    UInt16 temp = (UInt16)one[0];
                    if (temp == UserID || 0 == UserID)
                    {
                        fs.Seek(addr + 30, SeekOrigin.Begin); // Получаем BlockAddress
                        fs.Read(two, 0, 2);
                        temp = (UInt16)BitConverter.ToInt16(two, 0);

                        fs.Seek(temp, SeekOrigin.Begin); // Считываем название и расширение
                        fs.Read(twenty, 0, 20);
                        res += GetValidString(twenty);

                        if (res == fullname)
                        {
                            fs.Seek(addr + 2, SeekOrigin.Begin);
                            fs.Write(Encoding.UTF8.GetBytes(rights), 0, INode.AccessSize - 2);
                            return true;
                        }
                    }
                }
                return false;
            }
        }
        public string ReadDirectory()
        {
            byte[] files = ReadFileBlock(Directory);
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                string res = "";
                byte[] two = new byte[2];
                byte[] eight = new byte[8];
                byte[] one = new byte[1];
                byte[] twenty = new byte[20];
                byte[] four = new byte[4];

                fs.Seek(Directory + 10, SeekOrigin.Begin); // Считаем размер директории
                fs.Read(two, 0, 2);
                UInt16 count = (UInt16)BitConverter.ToInt16(two, 0);

                for (int i = 0; i < count; i += 2)
                {
                    two[0] = files[i];
                    two[1] = files[i + 1];

                    UInt16 addr = (UInt16)BitConverter.ToInt16(two, 0);

                    fs.Seek(addr, SeekOrigin.Begin); // Считываем права
                    fs.Read(eight, 0, 8);
                    res += Encoding.UTF8.GetString(eight).Substring(2) + "\t";

                    fs.Read(one, 0, 1); // Получаем UserID
                    UInt16 temp = (UInt16)one[0];

                    fs.Seek(5062 + temp * 42, SeekOrigin.Begin); // Считываем имя пользователя
                    fs.Read(twenty, 0, 20);
                    res += GetValidString(twenty) + "\t";

                    fs.Seek(addr + 10, SeekOrigin.Begin); // Считываем размер в байтах
                    fs.Read(two, 0, 2);
                    temp = (UInt16)BitConverter.ToInt16(two, 0);
                    res += temp + "\t";

                    fs.Seek(2, SeekOrigin.Current); // Считываем CreationTime
                    fs.Read(eight, 0, 8);
                    string date = Encoding.UTF8.GetString(eight);
                    date = date.Insert(2, ".");
                    date = date.Insert(5, ".");
                    res += date + "\t";

                    fs.Read(eight, 0, 8); // Считываем ModificationTime
                    date = Encoding.UTF8.GetString(eight);
                    date = date.Insert(2, ".");
                    date = date.Insert(5, ".");
                    res += date + "\t";

                    fs.Read(two, 0, 2); // Получаем BlockAddress
                    temp = (UInt16)BitConverter.ToInt16(two, 0);

                    fs.Seek(temp, SeekOrigin.Begin);
                    fs.Read(twenty, 0, 20);
                    res += GetValidString(twenty);
                    fs.Read(four, 0, 4);
                    if (four[0] != 0)
                        res += "." + GetValidString(four);
                    res += "\n";
                }

                return res;
            }
        }
        public byte[] ReadFileBlock(UInt16 iNodeOffset)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                byte[] buffer = new byte[2];
                fs.Seek(iNodeOffset + 10, SeekOrigin.Begin);
                fs.Read(buffer, 0, 2);
                UInt16 size = (UInt16)BitConverter.ToInt16(buffer, 0);
                fs.Seek(iNodeOffset + 30, SeekOrigin.Begin);
                fs.Read(buffer, 0, 2);
                UInt16 blockOffset = (UInt16)BitConverter.ToInt16(buffer, 0);

                buffer = new byte[size];
                fs.Seek(blockOffset + 24, SeekOrigin.Begin);
                fs.Read(buffer, 0, size);

                return buffer;
            }
        }
        public void CreateFile(string name, string expansion)
        {
            if (Directory != 60)
            {
                byte[] eight = new byte[8];
                byte[] one = new byte[1];
                using (FileStream fs = new FileStream(path, FileMode.Open))
                {
                    fs.Seek(Directory, SeekOrigin.Begin); // Получаем права доступа
                    fs.Read(eight, 0, 8);
                    string rights = GetValidString(eight);

                    fs.Seek(Directory + 8, SeekOrigin.Begin); // Получаем UserID
                    fs.Read(one, 0, 1);
                    UInt16 temp = (UInt16)one[0];

                    fs.Seek(Directory + 9, SeekOrigin.Begin); // Получаем UserID
                    fs.Read(one, 0, 1);
                    UInt16 gid = (UInt16)one[0];

                    if (!(rights[3] == 'w' && temp == UserID) && !(rights[6] == 'w' && gid == GroupID)) // Проверяет права доступа
                        return;
                }
            }

            UInt16 addr = WriteNewINode("TFrw----");
            WriteNewFullName(addr, name, expansion);
            WriteDataInBlock(Directory, BitConverter.GetBytes(addr));
        }
        public UInt16 WriteNewINode(string access)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                UInt16 address = 0;
                byte[] buffer = new byte[2];
                int i = 0;
                while(i < 20 && address == 0)
                {
                    fs.Seek(18 + i*2, SeekOrigin.Begin);
                    fs.Read(buffer, 0, 2);
                    address = (UInt16)BitConverter.ToInt16(buffer, 0);
                    i++;
                }
                if (address != 0)
                {
                    // Superblock
                    fs.Seek(-2, SeekOrigin.Current); // Затираем адрес в ListINode
                    fs.Write(BitConverter.GetBytes((UInt16)0), 0, 2);

                    fs.Seek(14, SeekOrigin.Begin); // Уменьшаем FreeINodeCount
                    fs.Read(buffer, 0, 2);
                    UInt16 freeINodeCount = (UInt16)BitConverter.ToInt16(buffer, 0);
                    freeINodeCount -= 1;
                    fs.Seek(-2, SeekOrigin.Current);
                    fs.Write(BitConverter.GetBytes(freeINodeCount), 0, 2);

                    // IList
                    fs.Seek(address, SeekOrigin.Begin); // Записываем атрибуты INode
                    fs.Write(Encoding.UTF8.GetBytes(access), 0, INode.AccessSize);
                    fs.Write(BitConverter.GetBytes(UserID), 0, INode.UserIDSize);
                    fs.Write(BitConverter.GetBytes(GroupID), 0, INode.GroupIDSize);
                    fs.Write(BitConverter.GetBytes((UInt16)0), 0, INode.SizeInBytesSize);
                    fs.Write(BitConverter.GetBytes((UInt16)0), 0, INode.SizeInBlocksSize);
                    fs.Write(Encoding.UTF8.GetBytes(DateTime.Now.ToString("ddMMyyyy")), 0, INode.CreationTimeSize);
                    fs.Write(Encoding.UTF8.GetBytes(DateTime.Now.ToString("ddMMyyyy")), 0, INode.ModificationTimeSize);

                    return address;
                }
                return 0;
            }
        }
        public void WriteNewFullName(UInt16 address, string name, string expansion)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                byte[] buffer = new byte[2];
                //ListBlock
                fs.Seek(12, SeekOrigin.Begin); // Уменьшаем FreeBlockCount
                fs.Read(buffer, 0, 2);
                UInt16 freeBlockCount = (UInt16)BitConverter.ToInt16(buffer, 0);
                freeBlockCount -= 1;
                fs.Seek(-2, SeekOrigin.Current);
                fs.Write(BitConverter.GetBytes(freeBlockCount), 0, 2);

                fs.Seek(58, SeekOrigin.Begin); // Находим адрес списка в памяти
                fs.Read(buffer, 0, 2);
                UInt16 bladdress = (UInt16)BitConverter.ToInt16(buffer, 0);
                fs.Seek(bladdress, SeekOrigin.Begin);

                fs.Read(buffer, 0, 2); // Проверяем продолжение списка
                UInt16 newBladdress = (UInt16)BitConverter.ToInt16(buffer, 0);

                if (newBladdress == 0) // Затираем адрес в ListBlock
                {
                    fs.Seek(bladdress + (freeBlockCount + 1) * 2, SeekOrigin.Begin);
                    fs.Read(buffer, 0, 2);
                    newBladdress = (UInt16)BitConverter.ToInt16(buffer, 0);

                    fs.Seek(-2, SeekOrigin.Current);
                    fs.Write(BitConverter.GetBytes((UInt16)0), 0, 2);
                } // TODO

                fs.Seek(newBladdress, SeekOrigin.Begin); // Записываем имя и расширение
                fs.Write(Encoding.UTF8.GetBytes(name), 0, name.Length);
                fs.Seek(newBladdress + 20, SeekOrigin.Begin);
                if (expansion != "dir")
                    fs.Write(Encoding.UTF8.GetBytes(expansion), 0, expansion.Length);
                else
                {
                    fs.Write(BitConverter.GetBytes((UInt16)0), 0, 2);
                    fs.Write(BitConverter.GetBytes(Directory), 0, 2);
                }

                fs.Seek(newBladdress + 510, SeekOrigin.Begin); // Заполняем нулями до конца блока TODO DELETE
                fs.Write(BitConverter.GetBytes((UInt16)0), 0, 2);

                fs.Seek(address + 10, SeekOrigin.Begin); // Записываем атрибуты INode
                fs.Write(BitConverter.GetBytes((UInt16)0), 0, INode.SizeInBytesSize);
                fs.Write(BitConverter.GetBytes((UInt16)1), 0, INode.SizeInBlocksSize);
                fs.Seek(address + 22, SeekOrigin.Begin);
                fs.Write(Encoding.UTF8.GetBytes(DateTime.Now.ToString("ddMMyyyy")), 0, INode.ModificationTimeSize);
                fs.Write(BitConverter.GetBytes(newBladdress), 0, 2);
            }
        }
        public void WriteDataInBlock(UInt16 address, byte[] data)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                byte[] buffer = new byte[2];
                fs.Seek(address + 30, SeekOrigin.Begin); // Выбираем первый блок
                fs.Read(buffer, 0, 2);

                UInt16 bladdress = (UInt16)BitConverter.ToInt16(buffer, 0);

                fs.Seek(Directory + 10, SeekOrigin.Begin); // Считаем размер директории
                fs.Read(buffer, 0, 2);
                UInt16 count = (UInt16)BitConverter.ToInt16(buffer, 0);

                fs.Seek(bladdress + 24 + count, SeekOrigin.Begin); // Записываем данные
                fs.Write(data, 0, data.Length);

                if (data.Length < 488)
                {
                    fs.Seek(bladdress + 510, SeekOrigin.Begin); // Заполняем нулями до конца блока TODO DELETE
                    fs.Write(BitConverter.GetBytes((UInt16)0), 0, 2);
                }

                fs.Seek(address + 10, SeekOrigin.Begin); // Записываем атрибуты INode
                fs.Write(BitConverter.GetBytes(count + data.Length), 0, INode.SizeInBytesSize);
                fs.Write(BitConverter.GetBytes((count + data.Length + 24) / 512 + 1), 0, INode.SizeInBlocksSize);
                fs.Seek(address + 22, SeekOrigin.Begin);
                fs.Write(Encoding.UTF8.GetBytes(DateTime.Now.ToString("ddMMyyyy")), 0, INode.ModificationTimeSize);
            }
        }
        public bool IsLogin(string login, string password) {
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                if (login == "")
                    return false;

                for(int i = 5062; i < 5480; i += 42)
                {
                    byte[] buffer = new byte[20];
                    fs.Seek(i, SeekOrigin.Begin);
                    fs.Read(buffer, 0, 20);

                    string exLogin = GetValidString(buffer);

                    if (exLogin == login)
                    {
                        fs.Seek(i + 20, SeekOrigin.Begin);
                        fs.Read(buffer, 0, 20);

                        string exPassword = GetValidString(buffer);

                        if(exPassword == password)
                        {
                            byte[] id = new byte[1];
                            fs.Seek(i-2, SeekOrigin.Begin);
                            fs.Read(id, 0, 1);

                            UserID = id[0];
                            fs.Read(id, 0, 1);

                            GroupID = id[0];
                            return true;
                        }
                    }
                }
            }
            
            return false;
        }
        public string GetAllUsers()
        {
            string res = "";
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                for (int i = 5062; i < 5480; i += 42)
                {
                    byte[] buffer = new byte[20];
                    fs.Seek(i, SeekOrigin.Begin);
                    fs.Read(buffer, 0, 20);

                    if (GetValidString(buffer) != "")
                        res += GetValidString(buffer) + "\n";
                }
            }
            return res;
        }
        public bool AddUser(string login, string password)
        {
            if (UserID != 0)
                return false;
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                for (int i = 5062; i < 5480; i += 42)
                {
                    byte[] buffer = new byte[20];
                    fs.Seek(i, SeekOrigin.Begin);
                    fs.Read(buffer, 0, 20);

                    if (GetValidString(buffer) == "") // Находим с пустым именем
                    {
                        fs.Seek(i - 2, SeekOrigin.Begin);
                        fs.Write(BitConverter.GetBytes((byte)((i - 5062) / 42)), 0, 1);
                        fs.Write(BitConverter.GetBytes((byte)0), 0, 1);
                        fs.Write(Encoding.UTF8.GetBytes(login), 0, login.Length);
                        fs.Seek(i + 20, SeekOrigin.Begin);
                        fs.Write(Encoding.UTF8.GetBytes(password), 0, password.Length);
                        return true;
                    }
                }
            }
            return false;
        }
        public bool ChangeGroup(string login, string group)
        {
            if (UserID != 0)
                return false;
            if (Convert.ToInt32(group) > 255 || Convert.ToInt32(group) < 0)
                return false;
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                for (int i = 5062; i < 5480; i += 42)
                {
                    byte[] buffer = new byte[20];
                    fs.Seek(i, SeekOrigin.Begin);
                    fs.Read(buffer, 0, 20);

                    if (GetValidString(buffer) == login) // Находим пользователя
                    {
                        fs.Seek(i - 1, SeekOrigin.Begin);
                        byte[] gid = new byte[1];
                        gid[0] = Convert.ToByte(group);
                        fs.Write(gid, 0, 1);
                        return true;
                    }
                }
            }
            return false;
        }
        public string GetValidString(byte[] buffer) { return Encoding.UTF8.GetString(buffer).Split('\0')[0]; }
        public void CreateDrive()
        {
            if (true/*!File.Exists(path)*/) // Create FS
            {
                WriteSuperBlock();

                WriteRootINode();

                WriteRootUserInfo();

                CreateRootDir();

                CreateListBlock();

                Directory = 60;
            }
        }
        public void WriteSuperBlock()
        {
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                fs.Seek(0, SeekOrigin.Begin);
                fs.Write(Encoding.UTF8.GetBytes(SuperBlock.Type), 0, SuperBlock.TypeSize);
                fs.Write(BitConverter.GetBytes(SuperBlock.SizeInBlocks), 0, SuperBlock.SizeInBlocksSize);
                fs.Write(BitConverter.GetBytes(SuperBlock.IListSize), 0, SuperBlock.IListSizeSize);
                fs.Write(BitConverter.GetBytes(SuperBlock.FreeBlockCount), 0, SuperBlock.FreeBlockCountSize);
                fs.Write(BitConverter.GetBytes(SuperBlock.FreeINodeCount), 0, SuperBlock.FreeINodeCountSize);
                fs.Write(BitConverter.GetBytes(SuperBlock.BlockSize), 0, SuperBlock.BlockSizeSize);
                for (int i = 0; i < 20; i++) // 20 = SuperBlock.ListINodeSize / sizeof(UInt16)
                {
                    SuperBlock.ListINode[i] = (UInt16)(110 + i * 50); //110 = 60 + 50
                    fs.Write(BitConverter.GetBytes(SuperBlock.ListINode[i]), 0, 2); // 2 = sizeof(UInt16)
                }
                fs.Write(BitConverter.GetBytes(SuperBlock.ListBlock), 0, SuperBlock.ListBlockSize);
            }
        }
        public void WriteRootINode()
        {
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                IList[0] = new INode();
                fs.Seek(60, SeekOrigin.Begin);
                fs.Write(Encoding.UTF8.GetBytes(IList[0].Access), 0, INode.AccessSize);
                fs.Write(BitConverter.GetBytes(IList[0].UserID), 0, INode.UserIDSize);
                fs.Write(BitConverter.GetBytes(IList[0].GroupID), 0, INode.GroupIDSize);
                fs.Write(BitConverter.GetBytes(IList[0].SizeInBytes), 0, INode.SizeInBytesSize);
                fs.Write(BitConverter.GetBytes(IList[0].SizeInBlocks), 0, INode.SizeInBlocksSize);
                fs.Write(Encoding.UTF8.GetBytes(IList[0].CreationDate), 0, INode.CreationTimeSize);
                fs.Write(Encoding.UTF8.GetBytes(IList[0].ModificationDate), 0, INode.ModificationTimeSize);
                fs.Write(BitConverter.GetBytes(IList[0].BlocksAddresses[0]), 0, 2);
                fs.Seek(5060, SeekOrigin.Begin);
                fs.Write(BitConverter.GetBytes(0), 0, 1);
            }
        }
        public void WriteRootUserInfo()
        {
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                fs.Seek(5060, SeekOrigin.Begin);
                fs.Write(BitConverter.GetBytes((byte)0), 0, 1);
                fs.Write(BitConverter.GetBytes((byte)0), 0, 1);
                fs.Write(Encoding.UTF8.GetBytes("admin"), 0, 5);
                fs.Seek(14, SeekOrigin.Current);
                fs.Write(BitConverter.GetBytes(0), 0, 1);
                fs.Write(Encoding.UTF8.GetBytes("0000"), 0, 4);
                fs.Seek(5479, SeekOrigin.Begin);
                fs.Write(BitConverter.GetBytes(0), 0, 1);
            }
        }
        public void CreateRootDir()
        {
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                fs.Seek(5480, SeekOrigin.Begin);
                fs.Write(Encoding.UTF8.GetBytes("rootdir"), 0, 7);
                fs.Seek(12, SeekOrigin.Current);
                fs.Write(BitConverter.GetBytes(0), 0, 1);
                fs.Write(BitConverter.GetBytes((UInt16)0), 0, 2);
                fs.Write(BitConverter.GetBytes((UInt16)60), 0, 2);
                fs.Seek(5992, SeekOrigin.Begin);
                fs.Write(BitConverter.GetBytes(0), 0, 1);
            }
        }
        public void CreateListBlock()
        {
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                fs.Seek(5992, SeekOrigin.Begin);
                fs.Write(BitConverter.GetBytes(0), 0, 2);
                for (UInt16 i = 6504; i < 56680; i += 512)
                    fs.Write(BitConverter.GetBytes(i), 0, 2);

                fs.Seek(56831, SeekOrigin.Begin);
                fs.Write(BitConverter.GetBytes(0), 0, 1);
            }
        }
    }
}
