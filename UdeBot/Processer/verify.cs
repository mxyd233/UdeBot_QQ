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
            web
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
            timer.Elapsed += Timedout;
            timer.Start();
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
            for (int i = 0; i < 8; i++)
            {
                code += (char)GenerateChar(random.Next(3)); // randomly decide whether to generate a numeral, an upper-case alphabet or a lower-case alphabet
            }
            if (code.Any(c => (c >= 0x41 && c < 0x5b) || (c >= 0x61 && c < 0x7b))) // if there are any alphabets (i.e. non-numerals) in code
            {
                code += (char)GenerateChar(random.Next(3));
            }
            else
            {
                code += (char)GenerateChar(random.Next(1, 3)); // force generate a non-numeral char at the last place, in case all 8 leading chars are numerals (in a 0.015% chance)
            }
            verificationCode = code;
        }

        internal bool VerifyCode(string inputCode)
        {
            bool isSuccessful;

            // There are two types of code that inputCode may be, one of which is a random series of chars consisting of both alphabets and numerals, 
            // which is used for binding a qq account with a ude one; the other type consisting of numberals only is in essence the value of user_id 
            // of a user, which is used for rescue measures, say, changing his email address. Unlike the latter one, the former one cannot be parsed
            // as a variable of integral type, so we use this feature to identify which kind of action the user would like to perform. 

            if (int.TryParse(inputCode, out int parsedUserIdInput)) // inputCode is probably user_id for rescue. We may need a larger type than uint if there will be more than 2.1 billion playing ude in the future, which I believe so. 
            {
                isSuccessful = parsedUserIdInput == userid;
                if (isSuccessful)
                {
                    Database.Exec($"update osu_email_verify set qq_verify_result=1 where user_id={userid}");
                }
                else
                {
                    DealWithInvalidVerification();
                }
            }
            else // input is probably generated random code for account binding. GenCode() method is modified to ensure that the verification code generated includes at least one non-numeral char. 
            {
                isSuccessful = verificationCode == inputCode;
                if (isSuccessful)
                {
                    switch (verifyFor)
                    {
                        case VerifyFor.bind:
                            Database.Exec($"update phpbb_users set QQ='{QQ}' where user_id={userid}");
                            VerificationDictionary.Remove(QQ);
                            break;
                        case VerifyFor.web:
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    DealWithInvalidVerification();
                }
            }

            void DealWithInvalidVerification()
            {
                if (++failTimes > 2)
                {
                    VerificationDictionary.Remove(QQ);//↓log多次验证失败
                    MahuaApis.Api.api.SendPrivateMessage(cfg.logToQQ, $"{QQ}尝试在 https://osu.zhzi233.cn/u/{userid} 多次验证失败");
                    throw new Exception("三次验证失败");//抛出异常来返回第三个'bool'
                }
                else
                {   //log验证失败
                    MahuaApis.Api.api.SendPrivateMessage(cfg.logToQQ, $"{QQ}尝试在 https://osu.zhzi233.cn/u/{userid} 验证失败");
                }
            }

            return isSuccessful;
        }
    }
}