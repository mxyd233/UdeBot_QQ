using Newbe.Mahua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newbe.Mahua.Apis;
using Newbe.Mahua.MahuaEvents;
using static UdeBot.MahuaApis.Api;

namespace UdeBot.game.Spy
{
    class player : IComparable<player>
    {
#if DEBUG
        internal
#endif
        string qq;
        string FromGroup;
        internal bool Described;
        internal bool Voted;
        internal bool Dead;
        internal int Votes;
        internal WhoIsSpy match;
        internal enum playerType
        {
            normal,
            spy
        }
        internal playerType PlayerType { get; private set; }
        internal player(string QQ, string FromGroup, WhoIsSpy Match)
        {
            qq = QQ;
            this.FromGroup = FromGroup;
            match = Match;
            PlayerType = playerType.normal;
            Described = false;
        }
        internal void SendMsg(string msg)
        {
            Helper.Api_sendMsg(Helper.SendType.groupSingle, FromGroup, qq, msg);
            //api.SendPrivateMessage(qq.ToString(), msg);
        }
        internal void SetPlayerSpy()
        {
            match.spy = this;
            PlayerType = playerType.spy;
        }

        public int CompareTo(player other)
        {
            if (Votes > other.Votes)
                return -1;
            if (Votes < other.Votes)
                return 1;
            return 0;
        }
    }
}

