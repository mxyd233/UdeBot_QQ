using Newbe.Mahua;
using Newbe.Mahua.MahuaEvents;
using System;

namespace UdeBot.MahuaEvents
{
    /// <summary>
    /// 插件被禁用事件
    /// </summary>
    public class PluginDisabledEvt
        : IPluginDisabledMahuaEvent
    {
        private readonly IMahuaApi _mahuaApi;

        public PluginDisabledEvt(
            IMahuaApi mahuaApi)
        {
            _mahuaApi = mahuaApi;
        }

        public void Disable(PluginDisabledContext context)
        {
            // todo 填充处理逻辑
            Conf.Save();

            // 不要忘记在MahuaModule中注册
        }
    }
}
