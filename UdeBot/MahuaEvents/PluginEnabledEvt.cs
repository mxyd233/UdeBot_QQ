using Newbe.Mahua;
using Newbe.Mahua.MahuaEvents;
using System;
using System.IO;
using System.Xml.Serialization;

namespace UdeBot.MahuaEvents
{
    /// <summary>
    /// 插件被启用事件
    /// </summary>
    public class PluginEnabledEvt
        : IPluginEnabledMahuaEvent
    {

        public PluginEnabledEvt(
            IMahuaApi mahuaApi)
        {
        }

        public void Enabled(PluginEnabledContext context)
        {
            Conf.Load();
            // 不要忘记在MahuaModule中注册
        }
    }
}
