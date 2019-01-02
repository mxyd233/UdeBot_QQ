using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UdeBot.Helper;

namespace UdeBot.Processer.Verify
{
    class EmailReset:Base
    {
        new int verificationCode;
        internal EmailReset(string QQ)
        {
            user = new User(true, QQ);
            verificationCode = Convert.ToInt32(Database.RunQueryOne($"SELECT verify_id FROM osu_email_verify WHERE user_id = '{user.user_id}' AND complete = 0"));
        }

        internal override bool Verify(string inputCode)
        {
            if (int.TryParse(inputCode, out int parsedInt)) {
                if (parsedInt == verificationCode)
                {
                    Database.Exec($"update osu_email_verify set qq_verify_result=1 where user_id={user.user_id}");
                    return true;
                }
            
            }
            DealWithInvalidVerification();
            return false;
            
        }
    }
}
