using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static UdeBot.Helper.Common;

namespace UdeBot.Processer.Verify
{
    abstract class Base
    {
        protected User user;
        protected string verificationCode;
        protected int failTimes;
        internal abstract bool Verify(string inputCode);
        protected virtual void DealWithInvalidVerification()
        {
            if (++failTimes > 2)
            {
                //VerificationDictionary.Remove(QQ);//↓log多次验证失败
                MahuaApis.Api.api.SendPrivateMessage(cfg.logToQQ, $"{user.QQ}尝试在 https://osu.zhzi233.cn/u/{user.user_id} 多次验证失败({GetType()})");
                throw new Exception("三次验证失败");//抛出异常来返回第三个'bool'
            }
            else
            {   //log验证失败
                MahuaApis.Api.api.SendPrivateMessage(cfg.logToQQ, $"{user.QQ}尝试在 https://osu.zhzi233.cn/u/{user.user_id} 验证失败({GetType()})");
            }
        }
    }
}
