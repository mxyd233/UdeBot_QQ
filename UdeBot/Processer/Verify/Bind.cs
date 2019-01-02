using System;
using System.Collections.Generic;
using System.Timers;
using UdeBot.Helper;
using static UdeBot.Helper.Common;

namespace UdeBot.Processer.Verify
{
    class Bind : Base
    {
        internal static Dictionary<string, Bind> VerificationDictionary = new Dictionary<string, Bind>();

        Timer timer;
        private int failTimes;

        internal Bind(User User, string QQ)
        {
            user = User;
            verificationCode = GenCode();
            User.SendEmail("osu!ude账户验证", $"嗨~ {User}!\n" +
                "有人正在请求你的验证码\n" +
                $"你的验证码是：{verificationCode}\n" +
                "如果你没有进行需要验证码的操作 请注意一下账户安全\n" +
                "osu! | http://osu.zhzi233.cn");
            TimerSetup();
        }
        private void TimerSetup()
        {
            timer = new Timer() { AutoReset = false, Interval = 1000 * 60 * 60 * 30, Enabled = false };
            timer.Elapsed += Timedout;
            timer.Start();
        }

        private void Timedout(object sender, ElapsedEventArgs e)
        {
            VerificationDictionary.Remove(user.QQ);
        }

        internal bool Verify(string inputCode)
        {
            if (inputCode == verificationCode)
            {
                Database.Exec($"update phpbb_users set QQ='{user.QQ}' where user_id='{user.user_id}'");
                VerificationDictionary.Remove(user.QQ);
                return true;
            }
            else
            {
                DealWithInvalidVerification();
                return false;
            }
        }

        private void DealWithInvalidVerification()
        {
            if (++failTimes > 2)
            {
                VerificationDictionary.Remove(user.QQ);//↓log多次验证失败
                MahuaApis.Api.api.SendPrivateMessage(cfg.logToQQ, $"{user.QQ}尝试在 https://osu.zhzi233.cn/u/{user.user_id} 多次绑定验证失败");
                throw new Exception("三次验证失败");//抛出异常来返回第三个'bool'
            }
            else
            {   //log验证失败
                MahuaApis.Api.api.SendPrivateMessage(cfg.logToQQ, $"{user.QQ}尝试在 https://osu.zhzi233.cn/u/{user.user_id} 绑定验证失败");
            }
        }

        private string GenCode()
        {
            Random random = new Random();
            int GenerateChar(int n) => n == 0 ?
                random.Next(0x30, 0x3a) :
                (n == 1 ? random.Next(0x41, 0x5b) :
                random.Next(0x61, 0x7b));
            // parameter n corresponds to type of type of chars: 0 being numerals, 1 being upper-case alphabets, and 2 being lower-case alphabets
            var code = "";
            for (int i = 0; i < 9; i++)
            {
                code += (char)GenerateChar(random.Next(3));
                // randomly decide whether to generate a numeral, an upper-case alphabet or a lower-case alphabet
            }

            return code;
        }
    }
}
