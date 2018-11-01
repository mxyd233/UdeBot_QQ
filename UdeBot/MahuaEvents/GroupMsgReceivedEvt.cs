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
                api.BanGroupMember(fromGroup, fromQQ, new TimeSpan(0, 0, new Random().Next(61)));
                reply("{E85F90EE-FC93-44EF-361D-343BD9BCB6BA}.amr");
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
                            if (string.IsNullOrEmpty(arg))
                            {
                                reply("请输入用户名");
                                break;
                            }
                            try
                            {
                                var userid = Convert.ToInt32(Database.RunQueryOne($"SELECT user_id from phpbb_users where username='{arg}'"));//如果没有获得userid将会返回0
                                using (var r = Database.RunQuery("select rank_score,accuracy_new,rank_score_index,level,playcount from osu_user_stats where user_id=" + userid))
                                {
                                    if (r.Read())
                                    {
                                        var pp = r.GetFloat("rank_score");
                                        var acc = Math.Round(r.GetFloat("accuracy_new"), 2);
                                        var rank = r.GetInt32("rank_score_index");
                                        var playcount = r.GetInt32("playcount");
                                        var level = Math.Round(r.GetFloat("level"), 2);
                                        var msg = $"{arg}(#{rank})大佬的水平统计为:\n" +
                                                $"pp:{pp}\n" +
                                                $"acc:{acc}\n" +
                                                $"pc:{playcount}\n" +
                                                $"level:{level}";
                                        reply(msg);
                                        break;
                                    }
                                }
                                reply("用户名不存在");
                                break;
                            }
                            catch
                            {
                                reply("出现未知错误，已转发错误信息给mxr123\n请等待修复");
                                throw;//在这里throw后会被switch块外的catch捕获并发送给cfg.lotToQQ
                            }
                        }
                    case "bind":
                    case "绑定":
                        {
                            if (Verify.VerifyDictionary.ContainsKey(fromQQ))
                            {
                                reply("你仍在一个验证流程中，请将ude绑定邮箱中的验证码通过私聊 !验证 233333 的方式发送给我");
                                break;
                            }
                            if (string.IsNullOrEmpty(arg))
                            {
                                reply("请输入大佬在ude中的游戏id");
                                break;
                            }
                            if (Convert.ToBoolean(Database.RunQueryOne($"SELECT user_id is not null FROM phpbb_users WHERE QQ ='{fromQQ}'")))
                            {
                                reply("你已经绑定过惹");
                                break;
                            }
                            using (var r = Database.RunQuery($"SELECT user_id,(QQ is null) as havQQ FROM phpbb_users WHERE username='{arg}'"))//如果没有获得userid将会返回0
                            {
                                if (r.Read())
                                {
                                    var userid = r.GetInt32("user_id");
                                    var Binded = !r.GetBoolean("havQQ");
                                    if (Binded)
                                    {
                                        reply("此id已经绑定过QQ了");
                                        break;
                                    }
                                    Verify.VerifyDictionary.Add(fromQQ, new Verify(userid, fromQQ, Verify.VerifyFor.bind));
                                    reply("请将ude绑定邮箱中的验证码通过私聊 !验证 233333 的方式发送给我");
                                    break;
                                }
                            }
                            reply("未ude找到此游戏id，请检查输入\n还没有注册的话\n可以在 https://osu.zhzi233.cn/p/register 注册\n(ude同样禁止小号)");
                            break;
                        }
                    case "help":
                        {
                            reply(
                                "------------------Help-------------------\n" +
                                $"当前机器人版本: v.{new PluginInfo().Version}\n" +
                                "Bot更新日志：!updateLog\n" +
                                "查看水平统计：!stat(s) 用户名\n" +
                                "谁是卧底（开发中）:!sswd\n" +
                                "禁言指令：!smoke 目标 时长(秒)\n" +
                                "取消禁言：!unsmoke qq号\n" +
                                "超级管理员列表：!dog\n" +
                                "-----------------------------------------"
                                );
                            break;
                        }
                    case "updatelog":
                        {
                            reply("https://gitee.com/mxr123/UdeBot_QQ/commits/master");
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
                                            if (args[1].Contains("@") && !args[1].Contains("["))
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
                                //reply(new Verify().verificationCode);
                                //reply(arg);
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
                        {//下面api.GetGroupMemberInfo工作似乎不太对 管理员和群主返回了normal 普通群成员返回了mannager 所以判断是否为normal
                            if (api.GetGroupMemberInfo(fromGroup, fromQQ).Authority != GroupMemberAuthority.Normal && !IsSuperAdmin(fromQQ))
                            {
                                reply("权限不足");
                                break;
                            }
                            fromQQ = GetQQThroughAt(fromQQ);
                            //判断是否仅是@名字 ([@xxx]这种真正的@才能获取到qq号码）
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
                    case "op":
                        {
                            if (!IsSuperAdmin(fromQQ))
                            {
                                reply("你不是超级管理员 无法执行此操作");
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
                            reply(msg + "为超级管理");
                            break;
                        }
                    case "removeadmin":
                    case "deop":
                        {
                            if (!IsSuperAdmin(fromQQ))
                            {
                                reply("你不是超级管理员 无法执行此操作");
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
                            reply(msg + "移出超级管理");
                            break;
                        }
                    default:
                        {
                            reply("UdeBot 未知指令");
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
                api.SendPrivateMessage(cfg.logToQQ, e.Message + "\n\n\n" + e.Source);
            }
        }
        private void reply(string Msg)
        {
            api.SendGroupMessage(fromGroup, Msg);
        }

    }
}
