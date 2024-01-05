using System;

namespace OS_kurs.FS
{
    internal class SuperBlock
    {
        public const byte TypeSize = 8;
        public const byte SizeInBlocksSize = sizeof(UInt16);
        public const byte IListSizeSize = sizeof(UInt16);
        public const byte FreeBlockCountSize = sizeof(UInt16);
        public const byte FreeINodeCountSize = sizeof(UInt16);
        public const byte BlockSizeSize = sizeof(UInt16);
        public const byte ListINodeSize = 20 * sizeof(UInt16);
        public const byte ListBlockSize = sizeof(UInt16);

        public const string Type = "FMANAGER";
        public const UInt16 SizeInBlocks = 111;
        public const UInt16 IListSize = 100;
        public static UInt16 FreeBlockCount = 98;
        public static UInt16 FreeINodeCount = 99;
        public const UInt16 BlockSize = 512;
        public static UInt16[] ListINode = new UInt16[20];
        public static UInt16 ListBlock = 5992;
    }
}
