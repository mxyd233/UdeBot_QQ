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
    class Verify1
    {
        internal enum VerifyFor
        {
            Bind,
            Web,
            ForgotEmail
        }

        internal static Dictionary<string, Verify1> VerificationDictionary = new Dictionary<string, Verify1>();
        readonly string QQ;
        readonly int userid;
        readonly string verificationCode;
        int failTimes;
        Timer timer;
        private readonly VerifyFor verifyFor;

        internal Verify1(int userid, string QQ, VerifyFor verifyFor = VerifyFor.Web)
        {
            this.QQ = QQ;
            this.userid = userid;
            this.verifyFor = verifyFor;
            //GenCode();
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


            timer = new Timer() { AutoReset = false, Interval = 1000 * 60 * 60 * 30, Enabled = false };
            timer.Elapsed += Timedout;
            timer.Start();
        }

        internal Verify1(string QQ)
        {
            this.QQ = QQ;
            this.userid= Convert.ToInt32(Database.RunQueryOne($"(select user_id from phpbb_users where QQ={QQ})"));
            this.verifyFor = VerifyFor.ForgotEmail;
        }

        private void Timedout(object sender, ElapsedEventArgs e)
        {
            VerificationDictionary.Remove(QQ);
        }



        internal bool VerifyCode(string inputCode)
        {
            switch (verifyFor)
            {
                case VerifyFor.Bind:
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

                case VerifyFor.Web:
                    break;

                case VerifyFor.ForgotEmail:
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
