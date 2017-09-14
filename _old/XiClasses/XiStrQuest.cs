using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TdfReader
{
    public class XiStrQuest
    {
        public char[] QuestId; // 56
        public uint QuestIdN;
        public uint PrevQuestIdN;
        public uint Event;
        public uint NeedLevel;
        public uint NeedLevelPercent;
        public char[] GivePost; // 56
        public char[] Title; // 128
        public char[] EndPost; // 56
        public char[][] Place; // 5,56
        public int[] CrashTime; // 5
        public int[] TimeLimit; // 5
        public int[] MinSpeed; // 5
        public int[] MaxSpeed; // 5
        public int[] MinLadisu; // 5
        public int[] MaxLadius; // 5
        public uint QuestPath_01;
        public uint QuestPath_02;
        public uint Car_01;
        public uint Car_02;
        public uint ClearQuestIdN;
        public int Count;
        public int RewardExp;
        public int RewardMoney;
        public char[] Item01; // 56
        public char[] Item02; // 56
        public char[] Item03; // 56
        public uint Idx;
        public XiStrQuest PrevQuestPtr;
        public XiStrQuest NextQuestPtr;
        public XiStrIcon GivePostPtr;
        public XiStrIcon EndPostPtr;
        public uint RewardItemNum;
        public XiStrItem Item01Ptr;
        public XiStrItem Item02Ptr;
        public XiStrItem Item03Ptr;
        public XIVISUALITEM_INFO VSItemPtr;
    }
}
