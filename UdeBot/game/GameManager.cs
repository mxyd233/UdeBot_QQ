using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UdeBot.game.Spy;

namespace UdeBot.game
{
    static class GameManager
    {
        internal static List<WhoIsSpy> WhoIsSpyList = new List<WhoIsSpy>();
        internal static WhoIsSpy FindWhoIsSpyByQQGroup(string QQGroup)
        {
            foreach (var Wodi in WhoIsSpyList)
            {
                if (Wodi.QqGroup == QQGroup)
                    return Wodi;
            }
            return null;
        }
    }
}
