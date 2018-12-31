using Newbe.Mahua;
using Newbe.Mahua.MahuaEvents;
using System;
using System.Collections.Generic;
using System.IO;
using UdeBot.game.Spy;
using UdeBot.Helper;
using UdeBot.Processer;
using static UdeBot.game.GameManager;
using static UdeBot.Helper.Common;
using static UdeBot.MahuaApis.Api;

namespace UdeBot.MahuaEvents
{
    /// <summary>
    /// 群消息接收事件
    /// </summary>
    public class GroupMsgReceivedEvt : IGroupMessageReceivedMahuaEvent
    {
        public GroupMsgReceivedEvt(
            IMahuaApi mahuaApi)
        {
        }

        private string fromGroup;
        private string fromQQ;
        private string msg;
        public void ProcessGroupMessage(GroupMessageReceivedContext context)
        {
            fromGroup = context.FromGroup;
            fromQQ = context.FromQq;
            msg = context.Message;
            //<发烟>
            if (msg.ToLower().Contains(@"\uf09f9aac"))
            {
                api.BanGroupMember(fromGroup, fromQQ, new TimeSpan(0, 0, 10));
                api.SendGroupMessage(fromGroup, GetVoiceGuid("大哥抽烟.mp3"));
                //var guid = Api_UploadVoice(BytesToIntptr(buffer));
                //Reply(guid);
                //api_UploadVoice()
                //Reply("{E85F90EE-FC93-44EF-361D-343BD9BCB6BA}.amr");
            }//</发烟>

            try
            {
                msg = msg.Trim();//去除首尾空字符
                if (!(msg.StartsWith("!") || msg.StartsWith("！")))
                {
                    Repeat.Add(fromGroup, msg);
                    return;
                }
                var cmd = msg.Replace("!", "").Replace("！", "").Trim().ToLower().Split(' ')[0];
                var arg = msg.Remove(0, cmd.Length + 1).Trim();
                var argsarr = arg.Split(' ');
                List<string> args = new List<string>(argsarr);
                args.RemoveAll(em => string.IsNullOrWhiteSpace(em) || em == "");
                switch (cmd)
                {
                    case "stats":
                    case "stat":
                        {
                            Ude.ReplyStats(arg,Reply);
                            break;
                        }
                    case "bind":
                    case "绑定":
                        {
                            Ude.BindUser(fromQQ, arg,Reply);
                            break;
                        }
                    case "验证":
                    case "verify":
                        {
                            if(string.IsNullOrEmpty(arg))
                            {
                                Reply("请输入验证码");
                                break;
                            }
                            Ude.VerifyUser(fromQQ, arg, Reply);
                            break;
                        }
                    case "找回邮箱":
                    case "resetemail":
                        {
                            if (string.IsNullOrEmpty(arg))
                            {
                                Reply("请输入验证码");
                                break;
                            }
                            Ude.ForgotEmailVerify(fromQQ, arg, Reply);
                            break;
                        }
                    case "help":
                        {
                            Ude.ReplyHelp(Reply);
                            break;
                        }
                    case "sswd":
                    case "谁是卧底":
                        {
                            if (args.Count < 1)
                            {
                                Reply("使用方法：谁是卧底 {开始|加入|解释|投票|结束}");
                                break;
                            }
                            WhoIsSpy spy = null;
                            if (WhoIsSpyList.Exists(s => s.QqGroup == fromGroup))
                                spy = FindWhoIsSpyByQQGroup(fromGroup);
                            else if (args[0] != "start" && args[0] != "开始")
                            {
                                Reply("此群没有已开启的谁是卧底游戏");
                                break;
                            }
                            switch (args[0])
                            {
                                case "start":
                                case "开始":
                                    {
                                        if (WhoIsSpyList.Exists(s => s.QqGroup == fromGroup))
                                        {
                                            Reply("Error本群已经有游戏进行中");
                                            break;
                                        }
                                        else
                                        {
                                            ushort playerCount = 6;
                                            if (!(args.Count < 2) && !ushort.TryParse(args[1], out playerCount))
                                            {
                                                Reply("游戏人数有误");
                                                break;
                                            }
                                            WhoIsSpyList.Add(new WhoIsSpy(fromGroup, playerCount));
                                            Reply("谁是卧底游戏开始 等待加入 0/" + playerCount);
                                        }
                                        break;
                                    }
                                case "join":
                                case "加入":
                                    {
                                        if (WhoIsSpyList.Exists(s => s.QqGroup == fromGroup))
                                            spy.Join(fromQQ);
                                        else
                                            Reply("此群没有已开启的谁是卧底游戏 使用!sswd start来开启一场游戏");
                                        break;
                                    }
                                case "explain":
                                case "解释":
                                    {
                                        spy.Described(fromQQ);
                                        break;
                                    }
                                case "vote":
                                case "投票":
                                    {
                                        if (args.Count >= 2)
                                        {
                                            if (args[1].Contains("@") && !args[1].Contains("["))
                                            {
                                                Reply("目标有误");
                                                break;
                                            }
                                            spy.Vote(fromQQ, GetQQThroughAt(args[1]));
                                        }
                                        else
                                            Reply($"[@{fromQQ}]请输入投票目标");
                                        break;
                                    }
                                case "close":
                                case "结束":
                                case "关闭":
                                    {
                                        if (IsSuperAdmin(fromQQ))
                                        {
                                            spy.Close();
                                        }
                                        else
                                        {
                                            Reply("你没有权限进行此操作");
                                        }
                                        break;
                                    }
                            }
                            break;
                        }
                    case "dog":
                    case "sa":
                        {
                            string Smsg = "超级管理员:\n";
                            foreach (var item in cfg.op)
                            {
                                Smsg += "[@" + item + ']' + '\n';
                            }
                            Smsg = Smsg.TrimEnd('\n');
                            Reply(Smsg);
                            break;
                        }
                    case "test":
                        {
                            try
                            {
                                api.SendGroupMessage(fromGroup).Image(@"C:\osu!\Songs\846995 Drop - Granat\anime_bg.png").Done();
                                //Reply(api.GetLoginQq());
                            }
                            catch (Exception e)
                            {
                                Reply(e.Message + "\n" + e.StackTrace);
                            }
                            break;
                        }
                    case "smoke":
                    case "mute":
                    case "禁言":
                        {//下面api.GetGroupMemberInfo工作似乎不太对 管理员和群主返回了normal 普通群成员返回了mannager 所以判断是否为normal
                            if (api.GetGroupMemberInfo(fromGroup, fromQQ).Authority != GroupMemberAuthority.Normal && !IsSuperAdmin(fromQQ))
                            {
                                Reply("权限不足");
                                break;
                            }
                            fromQQ = GetQQThroughAt(fromQQ);
                            //判断是否仅是@名字 ([@xxx]这种真正的@才能获取到qq号码）
                            if (fromQQ.Contains("@") && !fromQQ.Contains("["))
                            {
                                Reply("目标有误");
                                break;
                            }
                            if (args.Count < 2 || !int.TryParse(args[1], out int sec))
                            {
                                Reply("时长输入有误");
                                break;
                            }
                            Api_mute(fromGroup, GetQQThroughAt(args[0]), sec);
                            break;
                        }
                    case "悬念1":
                        {
                            FileStream fs = new FileStream(@"F:\silk\xn1.silk", FileMode.Open);
                            byte[] buffer = new byte[fs.Length];
                            fs.Read(buffer, 0, buffer.Length);
                            //var guid = Api_UploadVoice(BytesToIntptr(buffer));
                            //Reply(guid);
                            fs.Close();
                            break;
                        }
                    case "随机选择群员":
                    case "rollmember":
                        {
                            var list = new List<GroupMemberInfo>(api.GetGroupMemebersWithModel(fromGroup).Model);
                            Reply("抽中了[@" + list[new Random().Next(list.Count)].Qq + ']');
                            break;
                        }
                    case "全员禁言":
                    case "layingdownsmoke":
                    case "铃铛smoke":
                    case @"\uf09f9494smoke":
                        {
                            if (!IsSuperAdmin(fromQQ))
                            {
                                api.SendGroupMessage("你不是超级管理员 无法执行此操作");
                                break;
                            }
                            foreach (var item in api.GetGroupMemebersWithModel(fromGroup).Model)
                            {
                                api.BanGroupMember(fromGroup, item.Qq, new TimeSpan(0, 0, 10));
                            }
                            Reply("Laying down smoke");
                            break;
                        }
                    case "addadmin":
                    case "op":
                        {
                            if (!IsSuperAdmin(fromQQ))
                            {
                                Reply("你不是超级管理员 无法执行此操作");
                                break;
                            }
                            var msg = "已添加\n";
                            foreach (var item in args)
                            {
                                if (string.IsNullOrEmpty(item) | string.IsNullOrWhiteSpace(item))
                                    continue;
                                string qq = GetQQThroughAt(item);
                                if (IsSuperAdmin(qq))
                                    continue;
                                cfg.op.Add(qq);
                                msg += At(qq) + '\n';
                            }
                            Reply(msg + "为超级管理");
                            break;
                        }
                    case "removeadmin":
                    case "deop":
                        {
                            if (!IsSuperAdmin(fromQQ))
                            {
                                Reply("你不是超级管理员 无法执行此操作");
                                break;
                            }
                            var msg = "已将\n";
                            foreach (var item in args)
                            {
                                if (string.IsNullOrEmpty(item) | string.IsNullOrWhiteSpace(item))
                                    continue;
                                string qq = GetQQThroughAt(item);
                                cfg.op.RemoveAll(op => op == qq);
                                msg += At(qq) + '\n';
                            }
                            Reply(msg + "移出超级管理");
                            break;
                        }
                    default:
                        {
                            Reply("UdeBot 未知指令");
                            goto case "help";
                        }
                }
            }
            catch (IndexOutOfRangeException)
            {
                Reply("参数有误");
            }
            catch (Exception e)
            {
                api.SendPrivateMessage(cfg.logToQQ, e.Message + "\n\n\n" + e.Source);
            }
        }
        private void Reply(string Msg)
        {
            api.SendGroupMessage(fromGroup, Msg);
        }

    }
}
