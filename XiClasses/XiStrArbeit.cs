namespace TdfReader
{
    public class XiStrArbeit
    {
        public char[] ArbeitId; // 56
        public uint NeedLevel;
        public char[] GivePost; // 56
        public char[] Title; // 128
        public char[][] Place; // 20, 56
        public XiStrArbeitResult Exp;
        public XiStrArbeitResult Money;
        public XiStrArbeitResult Time;
        public ItemGroup Item_Gold;
        public ItemGroup Item_Silver;
        public ItemGroup Item_Bronze;
        public ItemGroup Item_Bottom;
        public uint Idx;
        public XiStrIcon GivePostPtr;
    }
}