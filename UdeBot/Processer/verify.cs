using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Timers;
using UdeBot.Helper;
using static UdeBot.Helper.Common;

namespace UdeBot.Processer
{
    class Verify
    {
        internal enum VerifyFor
        {
            bind,
            web
        }
        internal static Dictionary<string, Verify> VerifyDictionary = new Dictionary<string, Verify>();
        readonly string QQ;
        readonly int userid;
        string verificationCode;
        int failTimes;
        Timer timer;


        private readonly VerifyFor verifyFor;
        internal Verify(int userid, string QQ, VerifyFor verifyFor = VerifyFor.web)
        {
            this.QQ = QQ;
            this.userid = userid;
            this.verifyFor = verifyFor;
            genCode();
            var username = "";
            var userMail = "";
            using (var r = Database.RunQuery($"select user_email,username from phpbb_users where user_id='{this.userid}'"))
            {
                if (r.Read())
                {
                    username = r.GetString("username");
                    userMail = r.GetString("user_email");
                }
                else
                    throw new Exception("无法找到此用户");
            }

            var mailTo = new MailAddress(userMail);
            var mailFrom = new MailAddress(cfg.mailFrom, new PluginInfo().Name);

            using (var mail = new MailMessage(mailFrom, mailTo))
            {
                mail.Subject = "osu!ude账户验证";
                mail.Body =
                $"嗨~ {username}!\n" +
                "有人正在请求你的验证码\n" +
                $"你的验证码是：{verificationCode}\n" +
                "如果你没有进行需要验证码的操作 请注意一下账户安全\n" +
                "osu! | http://osu.zhzi233.cn";

                using (SmtpClient stmpSev = new SmtpClient(cfg.smtpHost)
                {
                    EnableSsl = cfg.stmpSsl,
                    Credentials = new NetworkCredential(cfg.mailFrom, cfg.mailPasswd)
                }
                )

                    stmpSev.Send(mail);
            }
            timer = new Timer() { AutoReset = false, Interval = 1000 * 60 * 60 * 30, Enabled = false };
            timer.Elapsed += timedout;
            timer.Start();
        }

        private void timedout(object sender, ElapsedEventArgs e)
        {
            VerifyDictionary.Remove(QQ);
        }

        private void genCode()
        {
            Random random = new Random();
            var code = "";
            for (int i = 0; i < 9; i++)
            {
                //int determiner(int n) => 
                code += (char)determiner(random);
            }

            verificationCode = code;
        }
        private int determiner(Random random)
        {
            var n = random.Next(3);
            var i = 0;
            switch (n)
            {
                case 0:
                    i = random.Next(0x30, 0x3a);
                    break;
                case 1:
                    i = random.Next(0x41, 0x5b);
                    break;
                default:
                    i = random.Next(0x61, 0x7b);
                    break;
            }

            return i;
        }
        internal bool VerifyCode(string code)
        {
            var success = verificationCode == code;
            if (success)
            {
                switch (verifyFor)
                {
                    case VerifyFor.bind:
                        Database.Exec($"update phpbb_users set QQ='{QQ}' where user_id={userid}");
                        VerifyDictionary.Remove(QQ);
                        break;
                    case VerifyFor.web:
                        break;
                    default:
                        break;
                }
            }
            else if (++failTimes > 2)
            {
                VerifyDictionary.Remove(QQ);//↓log多次验证失败
                MahuaApis.Api.api.SendPrivateMessage(cfg.logToQQ, $"{QQ}尝试在 https://osu.zhzi233.cn/u/{userid} 多次验证失败");
                throw new Exception("三次验证失败");//抛出异常来返回第三个'bool'
            }
            else
            {   //log验证失败
                MahuaApis.Api.api.SendPrivateMessage(cfg.logToQQ, $"{QQ}尝试在 https://osu.zhzi233.cn/u/{userid} 验证失败");
            }
            return success;
        }
    }
}