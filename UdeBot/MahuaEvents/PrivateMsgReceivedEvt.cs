using Newbe.Mahua;
using Newbe.Mahua.MahuaEvents;
using System.Collections.Generic;
using UdeBot.Processer;

namespace UdeBot.MahuaEvents
{
    /// <summary>
    /// 来自在线状态的私聊消息接收事件
    /// </summary>
    public class PrivateMsgReceivedEvt
        : IPrivateMessageReceivedMahuaEvent
    {
        private readonly IMahuaApi _mahuaApi;

        public PrivateMsgReceivedEvt(
            IMahuaApi mahuaApi)
        {
            _mahuaApi = mahuaApi;
        }

        private string fromQQ;
        private string msg;

        public void ProcessPrivateMessage(PrivateMessageReceivedContext context)
        {
            fromQQ = context.FromQq;
            msg = context.Message;

            msg = msg.Trim();//去除首尾空字符
            if (!(msg.StartsWith("!") || msg.StartsWith("！")))
                return;
            var cmd = msg.Replace("!", "").Replace("！", "").Trim().ToLower().Split(' ')[0];
            var arg = msg.Remove(0, cmd.Length + 1).Trim();
            List<string> args = new List<string>(arg.Split(' '));
            args.RemoveAll(em => string.IsNullOrWhiteSpace(em) || em == "");
            switch (cmd)
            {
                case "验证":
                case "verify":
                    {
                        if (!Verify.VerifyDictionary.ContainsKey(fromQQ))
                        {
                            reply("你并没有要验证的操作，或者你的验证流程已经超时了呢");
                            break;
                        }

                        try
                        {
                            if (Verify.VerifyDictionary[fromQQ].VerifyCode(arg))
                            {
                                reply("绑定成功~");
                            }
                            else
                            {
                                reply("验证码错误 请重新输入");
                            }
                        }
                        catch//三次均验证失败后会抛出异常
                        {
                            reply("已达错误次数上限，请重新开始进行验证");
                        }
                        break;
                    }
            }
        }
        private void reply(string msg)
        {
            _mahuaApi.SendPrivateMessage(fromQQ, msg);
        }
    }
}
