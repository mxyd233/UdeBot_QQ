using Newbe.Mahua;
using Newbe.Mahua.MahuaEvents;
using System.Collections.Generic;
using UdeBot.Processer;

namespace UdeBot.MahuaEvents
{
    /// <summary>
    /// 来自在线状态的私聊消息接收事件
    /// </summary>
    public class PrivateMsgReceivedEvt : IPrivateMessageReceivedMahuaEvent
    {
        private readonly IMahuaApi _mahuaApi;
        public PrivateMsgReceivedEvt(IMahuaApi mahuaApi) => _mahuaApi = mahuaApi;

        private string fromQQ;
        private string msg;

        public void ProcessPrivateMessage(PrivateMessageReceivedContext context)
        {
            fromQQ = context.FromQq;
            msg = context.Message.Trim(); //去除首尾空字符
            if (!(msg.StartsWith("!") || msg.StartsWith("！")))
            {
                return;
            }
            var cmd = msg.Replace("!", "").Replace("！", "").Trim().ToLower().Split(' ')[0];
            var arg = msg.Remove(0, cmd.Length + 1).Trim();
            List<string> args = new List<string>(arg.Split(' '));
            args.RemoveAll(em => string.IsNullOrWhiteSpace(em) || em == "");
            switch (cmd)
            {
                case "验证":
                case "verify":
                    {
                        if (string.IsNullOrEmpty(arg))
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
                case "bind":
                case "绑定":
                    {
                        Ude.BindUser(fromQQ, arg, Reply);
                        break;
                    }
                case "stats":
                case "stat":
                    {
                        Ude.ReplyStats(arg, Reply);
                        break;
                    }
                case "help":
                    {
                        Ude.ReplyHelp(Reply);
                        break;
                    }
            }
        }
        private void Reply(string msg)
        {
            _mahuaApi.SendPrivateMessage(fromQQ, msg);
        }
    }
}
