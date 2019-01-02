using System.Collections.Generic;
using System.Linq;

namespace UdeBot.Processer
{
    class Repeat
    {
        private static Dictionary<string, Repeat> RepeationList = new Dictionary<string, Repeat>();

        Queue<string> PreviousMsg = new Queue<string>();//0为上上一条 1为上条
        bool repeated;

        internal static void Add(string QQGroup, string Msg)
        {
            if (!RepeationList.ContainsKey(QQGroup))
                RepeationList.Add(QQGroup, new Repeat(/*QQGroup*/));
            RepeationList[QQGroup].PreviousMsg.Enqueue(Msg);
            if (RepeationList[QQGroup].PreviousMsg.Count == UdeBot.Helper.Common.cfg.reperterTrigger)
            {
                if (RepeationList[QQGroup].PreviousMsg.All(s => s == Msg)&&!RepeationList[QQGroup].repeated)
                {
                    MahuaApis.Api.api.SendGroupMessage(QQGroup, Msg);
                    RepeationList[QQGroup].repeated = true;
                }
                else if(!RepeationList[QQGroup].PreviousMsg.All(s => s == Msg))
                    RepeationList[QQGroup].repeated = false;
                RepeationList[QQGroup].PreviousMsg.Dequeue();
            }
        }
    }
}
