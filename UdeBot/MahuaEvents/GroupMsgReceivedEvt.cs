using Newbe.Mahua;
using Newbe.Mahua.MahuaEvents;
using Newbe.Mahua.Apis;
using System;
using System.Collections.Generic;
using UdeBot.game.Spy;
using static UdeBot.MahuaApis.Api;
using System.Runtime.InteropServices;
using System.IO;
using static UdeBot.game.GameManager;
using static UdeBot.Helper;

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
            if (msg.ToLower().Contains(@"\uf09f9aac"))
            {
                api.BanGroupMember(fromGroup, fromQQ, new TimeSpan(0, 0, new Random().Next(61)));
                reply("{E85F90EE-FC93-44EF-361D-343BD9BCB6BA}.amr");
            }
            try
            {
                msg = msg.Trim();//去除首位空字符
                if (!(msg.StartsWith("!") || msg.StartsWith("！")))
                    return;
                var cmd = msg.Replace("!", "").Replace("！", "").Trim().ToLower().Split(' ')[0];
                var arg = msg.Remove(0, cmd.Length + 1).Trim();
                var argsarr = arg.Split(' ');
                List<string> args = new List<string>(argsarr);
                args.RemoveAll(em => string.IsNullOrWhiteSpace(em) || em == "");
                switch (cmd)
                {
                    case "help":
                        {
                            reply("------------------Help-------------------\n" +
                                                           "   当前机器人版本: v." + new PluginInfo().Version
                                                         + "\n Bot更新日志：!updateLog "
                                                         + " \n 查看可用群：!group" + "\n谁是卧底（开发中）:!sswd\n禁言指令：!smoke 被禁言者qq 禁言时间\n取消禁言：!unsmoke qq号\n关闭谁是卧底" +
                                                         "!closesswd\n超级管理员列表：!dog");
                            break;
                        }
                    case "updatelog":
                        {
                            reply("MiaoBot UpdataLog\n v0.0.0.1 群消息接受正常\nv0.0.0.2 游戏 谁是卧底开发ING\n V0.0.0.3 谁是卧底大部分开发完成");
                            break;
                        }
                    case "sswd":
                    case "谁是卧底":
                        {
                            if (args.Count < 1)
                            {
                                reply("使用方法：谁是卧底 {开始|加入|解释|投票|结束}");
                                break;
                            }
                            WhoIsSpy spy = null;
                            if (WhoIsSpyList.Exists(s => s.QqGroup == fromGroup))
                                spy = FindWhoIsSpyByQQGroup(fromGroup);
                            else if (args[0] != "start" && args[0] != "开始")
                            {
                                reply("此群没有已开启的谁是卧底游戏");
                                break;
                            }
                            switch (args[0])
                            {
                                case "start":
                                case "开始":
                                    {
                                        if (WhoIsSpyList.Exists(s => s.QqGroup == fromGroup))
                                        {
                                            reply("Error本群已经有游戏进行中");
                                            break;
                                        }
                                        else
                                        {
                                            ushort playerCount = 6;
                                            if (!(args.Count < 2) && !ushort.TryParse(args[1], out playerCount))
                                            {
                                                reply("游戏人数有误");
                                                break;
                                            }
                                            WhoIsSpyList.Add(new WhoIsSpy(fromGroup, playerCount));
                                            reply("谁是卧底游戏开始 等待加入 0/" + playerCount);
                                        }
                                        break;
                                    }
                                case "join":
                                case "加入":
                                    {
                                        if (WhoIsSpyList.Exists(s => s.QqGroup == fromGroup))
                                            spy.Join(fromQQ);
                                        else
                                            reply("此群没有已开启的谁是卧底游戏 使用!sswd start来开启一场游戏");
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
                                            if(args[1].Contains("@")&&!args[1].Contains("["))
                                            {
                                                reply("目标有误");
                                                break;
                                            }
                                            spy.Vote(fromQQ, GetQQThroughAt(args[1]));
                                        }
                                        else
                                            reply($"[@{fromQQ}]请输入投票目标");
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
                                            reply("你没有权限进行此操作");
                                        }
                                        break;
                                    }
                            }
                            break;
                        }
                    case "dog":
                    case "op":
                    case "sa":
                        {
                            string Smsg = "超级管理员:\n";
                            foreach (var item in cfg.op)
                            {
                                Smsg += "[@" + item + ']' + '\n';
                            }
                            Smsg = Smsg.TrimEnd('\n');
                            reply(Smsg);
                            break;
                        }
                    case "test":
                        {
                            try
                            {
                                reply(arg);
                                //var filename = arg;
                                //if (Helper.ConvertToPcm(ref filename))
                                //    reply("成功,文件名为" + filename);
                                //else
                                //    reply("失败");
                                //if (Helper.ConvertPcmToSlik(ref filename))
                                //{
                                //    reply("成功,文件名为" + filename);
                                //    FileStream fs = new FileStream(filename, FileMode.Open);
                                //    byte[] buffer = new byte[fs.Length];
                                //    fs.Read(buffer, 0, buffer.Length);
                                //    var guid = Api_UploadVoice(api.GetLoginQq(), BytesToIntptr(buffer));
                                //    fs.Close();
                                //    reply(guid);
                                //}
                                //else
                                //    reply("失败");
                            }
                            catch (Exception e)
                            {
                                reply(e.Message + "\n" + e.StackTrace);
                            }
                            break;
                        }
                    case "smoke":
                    case "mute":
                    case "禁言":
                        {
                            if (!IsSuperAdmin(fromQQ))
                            {
                                reply("你没有权限");
                                break;
                            }
                            fromQQ = GetQQThroughAt(fromQQ);
                            if (fromQQ.Contains("@") && !fromQQ.Contains("["))
                            {
                                reply("目标有误");
                                break;
                            }
                            int sec;
                            if (args.Count < 2 || !int.TryParse(args[1], out sec))
                            {
                                reply("时长输入有误");
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
                            var guid = api_UploadVoice(BytesToIntptr(buffer));
                            reply(guid);
                            fs.Close();
                            break;
                        }
                    case "随机选择群员":
                    case "rollmember":
                        {
                            var list = new List<GroupMemberInfo>(api.GetGroupMemebersWithModel(fromGroup).Model);
                            reply("抽中了[@" + list[new Random().Next(list.Count)].Qq + ']');
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
                            reply("Laying down smoke");
                            break;
                        }
                    case "addadmin":
                        {
                            if (!IsSuperAdmin(fromQQ))
                            {
                                reply("你不是超级管理员 无法执行此操作");
                                break;
                            }
                            foreach (var item in args)
                            {
                                if (string.IsNullOrEmpty(item) | string.IsNullOrWhiteSpace(item))
                                    continue;
                                string qq;
                                if (item.StartsWith("[@"))
                                    qq = item.Remove(0, 2).Remove(item.Length - 3);
                                else
                                    qq = item;
                                cfg.op.Add(qq);
                            }
                            break;
                        }
                    default:
                        {
                            reply("Miao Bot 未知指令");
                            goto case "help";
                        }
                }
            }
            catch (IndexOutOfRangeException)
            {
                reply("参数有误");
            }
            catch (Exception e)
            {
                api.SendPrivateMessage("2362016620", e.Message + "\n\n\n" + e.Source);
            }
        }
        private void reply(string Msg)
        {
            api.SendGroupMessage(fromGroup, Msg);
        }

    }
}
