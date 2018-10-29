using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UdeBot.Helper;
using static UdeBot.Helper.Common;

namespace UdeBot
{
    public class Conf
    {
        public List<string> op = new List<string>();
        public string logToQQ = "2362016620";
        public string dbConnectionString="Server=127.0.0.1;Database=osu;User=root;Password=passwd;";
        public string mailFrom = "";
        public string mailPasswd = "";
        public string smtpHost = "";
        public bool stmpSsl = false;
        
        internal static void Load()
        {
            using (FileStream fs = new FileStream("conf.xml", FileMode.OpenOrCreate))
            {
                cfg = new Conf();
                try
                {
                    var xmlSerializer = new XmlSerializer(cfg.GetType());
                    cfg = xmlSerializer.Deserialize(fs) as Conf;
                }
                catch (InvalidOperationException)
                {
                    Save(true);
                    cfg = new Conf();

                }

                //添加必须的超级管理
                if (!IsSuperAdmin("1543502875"))
                    cfg.op.Add("1543502875");
                if (!IsSuperAdmin("2362016620"))
                    cfg.op.Add("2362016620");
            }
        }
        internal static void Save(bool saveToBak = false)
        {
            if (saveToBak)
            {
                var bakName = "conf.xml.bak.{DateTime.Now.Ticks}";
                if (File.Exists("conf.xml"))
                    File.Copy("conf.xml", bakName);
                for (int i = 0; i < 4; i++)//重要的事情说三遍
                {
                    Log("配置文件损坏 请重新配置 原配置文件为" + bakName);
                }
                return;
            }
            using (FileStream fs = new FileStream("conf.xml", FileMode.OpenOrCreate))
            {
                var xmlSerializer = new XmlSerializer(cfg.GetType());
                xmlSerializer.Serialize(fs, cfg);
            }
        }
    }
}
