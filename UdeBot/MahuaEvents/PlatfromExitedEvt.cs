using Newbe.Mahua;
using Newbe.Mahua.MahuaEvents;
using System;

namespace UdeBot.MahuaEvents
{
    /// <summary>
    /// 机器人平台退出事件
    /// </summary>
    public class PlatfromExitedEvt
        : IPlatfromExitedMahuaEvent
    {
        private readonly IMahuaApi _mahuaApi;

        public PlatfromExitedEvt(
            IMahuaApi mahuaApi)
        {
            _mahuaApi = mahuaApi;
        }

        public void Exited(PlatfromExitedContext context)
        {
            // todo 填充处理逻辑
            Conf.Save();

            // 不要忘记在MahuaModule中注册
        }
    }
}
