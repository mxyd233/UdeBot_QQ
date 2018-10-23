using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Newbe.Mahua;
using static UdeBot.MahuaApis.Api;

namespace UdeBot.game.Spy
{
    class WhoIsSpy
    {
        internal enum status
        {
            wating,
            gaming,
            finshed
        }
        internal string QqGroup;
        internal status Status;
        private ushort playerCount;
        internal List<player> PlayerList = new List<player>();
        static string[] Allword = new string[] { "兔子|猫咪", "面包|馒头" };
        internal player spy;
        internal player playerDescribing;
        internal WhoIsSpy(string Group, ushort PlayerCount = 6)
        {
            playerCount = PlayerCount;
            QqGroup = Group;
            Status = status.wating;
        }
        internal void Join(string QQ)
        {
            if (Status == status.wating)
            {

                PlayerList.Add(new player(QQ, QqGroup,this));
                SendMsg($"[@{QQ}] 已加入游戏{PlayerList.Count}/{playerCount}");
                if (PlayerList.Count == playerCount)
                    StartGame();
            }
            else
            {
                SendMsg("游戏已开始 无法加入");
            }

        }
        internal void StartGame()
        {
            var willBeSpy = new Random().Next(playerCount - 1);
            PlayerList[willBeSpy].SetPlayerSpy();
            Status = status.gaming;
            SendMsg("游戏开始");
            var i = new Random().Next((Allword.Length));
            var words = Allword[i].Split('|');
            foreach (var player in PlayerList)
            {
                switch (player.PlayerType)
                {
                    case player.playerType.normal:
                        {
                            // api.
                            player.SendMsg($"from{QqGroup}:\n你是平民\n 你的词语为:{words[0]}");
                            break;
                        }
                    case player.playerType.spy:
                        {
                            player.SendMsg($"from{QqGroup}:\n你是卧底\n 你的词语为:{words[1]}");
                            break;
                        }
                }
            }
            SendMsg("词卡已发放，请各自介绍自己的词语");
            foreach (var player in PlayerList)
            {
                if (player.Dead)
                    continue;
                playerDescribing = player;
                SendMsg($"请[@{player.qq}]解释自己的词语 用[!谁是卧底 解释 ...]解释");
                while (true)
                {
                    Thread.Sleep(500);
                    if (player.Described)
                        break;
                }
            }
            SendMsg("第一回合解释阶段已完成 下面请使用{!投票 目标}进行投票");
            while (true)
            {
                Thread.Sleep(500);
                if (getAllVoted())
                    break;
            }
            PlayerList.Sort();

            PlayerList[0].Dead = true;
            if(spy==PlayerList[0])
            {
                SendMsg("游戏结束:卧底被杀死");
                Close();
            }
#if DEBUG
            foreach (var item in PlayerList)
            {
                SendMsg($"[@{item.qq}] 是{item.PlayerType}");
            }
            SendMsg($"本次的词组为{words[0]}和{words[1]}");
            Close();
#endif
        }
        internal bool Described(string fromQQ)
        {
            var player = FindPlayerByQQ(fromQQ);
            if (player.Dead)
            {
                SendMsg("死人不能说话");
                Helper.Api_mute(QqGroup, fromQQ, 60);
            }
            if (playerDescribing != player)
            {
                SendMsg("cnm别插嘴");
                Helper.Api_mute(QqGroup, fromQQ, 30);
                return false;
            }
            player.Described = true;
            return true;
        }
        internal bool Vote(string fromQQ, string VoteTo)
        {
            if (fromQQ == VoteTo)
            {
                SendMsg("不能够给自己投票");
                return false;
            }
            var fromPlayer = FindPlayerByQQ(fromQQ);
            var toPlayer = FindPlayerByQQ(VoteTo);
            if (fromPlayer.Voted)
            {
                SendMsg($"[@{fromQQ}]你已经投过票惹");
                return false;
            }
            fromPlayer.Voted = true;
            toPlayer.Votes++;
            SendMsg($"[@{VoteTo}]获得{toPlayer.Votes}票");
            return true;
        }
        internal player FindPlayerByQQ(string QQ)
        {
            return PlayerList.Find(player => player.qq == QQ);
        }
        internal void SendMsg(string msg)
        {
            api.SendGroupMessage(QqGroup.ToString(), msg);

        }
        private bool getAllVoted()
        {
            foreach (var player in PlayerList)
            {
                if (!player.Voted)
                    return false;
            }
            return true;
        }
        internal void Close()
        {
            if (GameManager.WhoIsSpyList.Exists(s => s.QqGroup == QqGroup))
            {
                GameManager.WhoIsSpyList.RemoveAll(s => s.QqGroup == QqGroup);
                SendMsg("游戏已经关闭");
            }
        }
    }
}
