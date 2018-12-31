using System;
using System.Collections.Generic;
using System.Linq;
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
            web,
            forgotEmail
        }

        internal static Dictionary<string, Verify> VerificationDictionary = new Dictionary<string, Verify>();
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
            GenCode();
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
                {
                    throw new Exception("无法找到此用户");
                }
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
                })
                
                stmpSev.Send(mail);
            }

            timer = new Timer() { AutoReset = false, Interval = 1000 * 60 * 60 * 30, Enabled = false };
            timer.Elapsed += Timedout;
            timer.Start();
        }
        internal Verify(string QQ)
        {
            this.QQ = QQ;
            this.userid= Convert.ToInt32(Database.RunQueryOne($"(select user_id from phpbb_users where QQ={QQ})"));
            this.verifyFor = VerifyFor.forgotEmail;
        }

        private void Timedout(object sender, ElapsedEventArgs e)
        {
            VerificationDictionary.Remove(QQ);
        }

        private void GenCode()
        {
            Random random = new Random();
            int GenerateChar(int n) => n == 0 ? random.Next(0x30, 0x3a) : (n == 1 ? random.Next(0x41, 0x5b) : random.Next(0x61, 0x7b)); // parameter n corresponds to type of type of chars: 0 being numerals, 1 being upper-case alphabets, and 2 being lower-case alphabets
            var code = "";
            for (int i = 0; i < 9; i++)
            {
                code += (char)GenerateChar(random.Next(3)); // randomly decide whether to generate a numeral, an upper-case alphabet or a lower-case alphabet
            }

            verificationCode = code;
        }

        internal bool VerifyCode(string inputCode)
        {
            switch (verifyFor)
            {
                case VerifyFor.bind:
                    {
                        if (inputCode == verificationCode)
                        {
                            Database.Exec($"update phpbb_users set QQ='{QQ}' where user_id={userid}");
                            VerificationDictionary.Remove(QQ);
                            return true;
                        }
                        else
                        {
                            DealWithInvalidVerification();
                            return false;
                        }
                    }
                case VerifyFor.web:
                    {
                        break;
                    }
                case VerifyFor.forgotEmail:
                    {
                        bool IsVerifyIdMatching()
                        {
                            int verifyIdFromDatabase = Convert.ToInt32(Database.RunQueryOne($"select verify_id from osu_email_verify where user_id={userid}"));
                            //(int)Database.RunQueryOne($"select verify_id from osu_email_verify where user_id=(select user_id from phpbb_users where QQ={QQ})");
                            if (int.TryParse(inputCode, out int parsedInt))
                                return parsedInt == verifyIdFromDatabase;
                            else
                                return false;
                        }

                        var isSuccessful = IsVerifyIdMatching();
                        if (isSuccessful)
                        {
                            Database.Exec($"update osu_email_verify set qq_verify_result=1 where user_id={userid}");
                        }
                        else
                        {
                            DealWithInvalidVerification();
                        }
                        return isSuccessful;
                    }
            }

            void DealWithInvalidVerification()
            {
                if (++failTimes > 2)
                {
                    VerificationDictionary.Remove(QQ);//↓log多次验证失败
                    MahuaApis.Api.api.SendPrivateMessage(cfg.logToQQ, $"{QQ}尝试在 https://osu.zhzi233.cn/u/{userid} 多次验证失败({verifyFor})");
                    throw new Exception("三次验证失败");//抛出异常来返回第三个'bool'
                }
                else
                {   //log验证失败
                    MahuaApis.Api.api.SendPrivateMessage(cfg.logToQQ, $"{QQ}尝试在 https://osu.zhzi233.cn/u/{userid} 验证失败({verifyFor})");
                }
            }
            return false;
        }
    }
}
