using System;
namespace OS_kurs.FS
{
    public class INode
    {
        public static byte AccessSize = 8;
        public static byte UserIDSize = 1;
        public static byte GroupIDSize = 1;
        public static byte CreationTimeSize = 8;
        public static byte ModificationTimeSize = 8;
        public static byte SizeInBytesSize = 2;
        public static byte SizeInBlocksSize = 2;
        public static byte BlocksAddressesSize = 20;
        public string Access;
        public byte UserID;
        public byte GroupID;
        public string CreationDate;
        public string ModificationDate;
        public UInt16 SizeInBytes;
        public UInt16 SizeInBlocks;
        public UInt16[] BlocksAddresses = new UInt16[10];
        public INode()
        {
            Access = "TDrwxrwx";
            UserID = 0;
            GroupID = 0;
            CreationDate = DateTime.Now.ToString("ddMMyyyy");
            ModificationDate = DateTime.Now.ToString("ddMMyyyy");
            SizeInBytes = 0;
            SizeInBlocks = 1;
            BlocksAddresses = new UInt16[10];
            BlocksAddresses[0] = 5480;
        }
        public INode(string access, byte userID, byte groupID, UInt16 sizeInBytes, UInt16 sizeInBlocks,
            string creationDate, string modificationDate, UInt16[] blocksAddresses)
        {
            Access = access;
            UserID = userID;
            GroupID = groupID;
            CreationDate = creationDate;
            ModificationDate = modificationDate;
            SizeInBytes = sizeInBytes;
            SizeInBlocks = sizeInBlocks;
            BlocksAddresses = blocksAddresses;
        }
    }
}
