using System;
using System.Net;
using System.Net.Mail;
using UdeBot.Helper;
using static UdeBot.Helper.Common;

namespace UdeBot.Processer
{
    public class User
    {
        public int user_id;
        public string username;
        public string user_email;
        public string QQ;
        public bool haveQQ;
        #region init
        public User(int User_id)
        {
            using (var r = Database.RunQuery($"SELECT username,user_email,IFNULL(QQ,'') as QQ FROM phpbb_users WHERE user_id ='{User_id}'"))
            {
                if (!r.Read())//查询不到任何字段
                    throw new Exception("User not found");
                this.user_id = User_id;
                this.username = r.GetString("username");
                this.user_email = r.GetString("user_email");
                this.QQ = r.GetString("QQ");
                haveQQ = !string.IsNullOrEmpty(QQ);
            }
        }
        public User(string Username)
        {
            using (var r = Database.RunQuery($"SELECT user_id,user_email,IFNULL(QQ,'') as QQ FROM phpbb_users WHERE username ='{Username}'"))
            {
                if (!r.Read())//查询不到任何字段
                    throw new Exception("User not found");
                this.username = Username;
                this.user_id = r.GetInt32("user_id");
                this.user_email = r.GetString("user_email");
                this.QQ = r.GetString("QQ");
                haveQQ = !string.IsNullOrEmpty(QQ);
            }
        }
        public User(bool ByQQ,string QQ)
        {
            if (!ByQQ)
                throw new Exception("not ByQQ with a QQ value");
            using (var r = Database.RunQuery($"SELECT user_id,user_email,username FROM phpbb_users WHERE QQ ='{QQ}'"))
            {
                if (!r.Read())//查询不到任何字段
                    throw new Exception("User not found");
                this.QQ = QQ;
                this.user_id = r.GetInt32("user_id");
                this.user_email = r.GetString("user_email");
                this.username = r.GetString("username");
                haveQQ = !string.IsNullOrEmpty(QQ);
            }
        }
        #endregion
        public void SendEmail(string Title,string Msg)
        {
            var mailTo = new MailAddress(user_email);
            var mailFrom = new MailAddress(cfg.mailFrom, new PluginInfo().Name);

            using (var mail = new MailMessage(mailFrom, mailTo))
            {
                mail.Subject = Title;
                mail.Body = Msg;
                using (SmtpClient stmpSev = new SmtpClient(cfg.smtpHost)
                {
                    EnableSsl = cfg.stmpSsl,
                    Credentials = new NetworkCredential(cfg.mailFrom, cfg.mailPasswd)
                })

                    stmpSev.Send(mail);
            }
        }
        public void SendQQ(string Msg)
        {
            if (!haveQQ)
                return;
            MahuaApis.Api.api.SendPrivateMessage(QQ, Msg);
        }

        internal void BindQQ(string QQ)
        {
            if (haveQQ)
                return;
            this.QQ = QQ;
            Verify.Bind.VerificationDictionary.Add(QQ, new Verify.Bind(this, QQ));
        }

        public override string ToString()
        {
            return username;
        }
    }
}
