using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using UdeBot;

namespace UdeBot
{
    public class Conf
    {
        public List<string> op = new List<string>();
        internal static void Load()
        {
            using (FileStream fs = new FileStream($"{new PluginInfo().Id}/conf.xml", FileMode.OpenOrCreate))
            {
                Helper.cfg = new Conf();
                try
                {
                    var xmlSerializer = new XmlSerializer(Helper.cfg.GetType());
                    Helper.cfg = xmlSerializer.Deserialize(fs) as Conf;
                }
                catch (InvalidOperationException)
                {
                    Helper.cfg = new Conf();
                }

                //添加必须的超级管理
                if (!Helper.IsSuperAdmin("1543502875"))
                    Helper.cfg.op.Add("1543502875");
                if (!Helper.IsSuperAdmin("2362016620"))
                    Helper.cfg.op.Add("2362016620");
            }
        }
        internal static void Save()
        {
            using (FileStream fs = new FileStream($"{new PluginInfo().Id}/conf.xml", FileMode.OpenOrCreate))
            {
                var xmlSerializer = new XmlSerializer(Helper.cfg.GetType());
                xmlSerializer.Serialize(fs, Helper.cfg);
            }
        }
    }
}
