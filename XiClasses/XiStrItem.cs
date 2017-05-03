using System.Collections.Generic;

namespace TdfReader
{
    public class XiStrItem
    {
        public uint Type;
        public XiStrItemId Id;
        public char[] GroupId; //56
        public char[] Name;
        public uint Grade;
        public uint ReqLevel;
        public uint Value;
        public int Min;
        public int Max;
        public uint Cost;
        public uint Sell;
        public uint Time;
        public int AssistNum;
        public char[] AssistField; // 56
        public XiStrItemId NextId;
        public uint RoundNum;
        public uint Belong;
        public XiStrItemUseInfo UseInfo;
        public XiStrItem NextItemPtr;
        public uint TableIdx;
        public int HeatState;
        public bool Shop;
        public bool Trade;
        public bool Auction;
        public uint SetType;
        public uint SetRate;
        public char[] SetDesc; // 160
        public XiStrAssist SetAssist;
        public uint TransType;
        public uint UpgradeType;
        public uint JewelType;
        public List<AssistGroup> AssistList;
    }
}
