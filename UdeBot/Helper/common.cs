using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using static UdeBot.MahuaApis.Api;

namespace UdeBot.Helper
{
    internal static class Common
    {
        internal enum SendType
        {
            friend = 1,
            group = 2,
            discuss = 3,
            groupSingle = 4,
            discussSingle = 5
        }
        internal static string LogonQQ;
        internal static Conf cfg;
        #region naiveApi
        [DllImport("Message.dll")]
        private static extern string Api_UploadVoice(string usingQQ, byte[] Data);
        [DllImport("Message.dll")]
        private static extern bool Api_IsFriend(string usingQQ, string QQ);
        [DllImport("Message.dll")]
        private static extern bool Api_SendMsg(string usingQQ, int sendType, int subType, string groupDst, string QQDst, string Msg);
        [DllImport("Message.dll")]
        private static extern int Api_OutPut(string msg);

        #endregion
        internal static bool IsSuperAdmin(string QQ)
        {
            return cfg.op.Contains(QQ);
        }
        internal static string GetVoiceGuid(string filename)
        {
            if (!File.Exists(filename + ".pcm"))
                ConvertToPcm(ref filename);
            else
                filename+=".pcm";
            if (!File.Exists(filename + ".pcm.silk"))
                ConvertPcmToSlik(ref filename);
            else
                filename += ".silk"; ;

            using (var fs = File.OpenRead(filename))
            {
                byte[] b = new byte[fs.Length];
                fs.Read(b, 0, b.Length);
                return Api_UploadVoice(b);

            }
        }
        private static string Api_UploadVoice(byte[] Data)
        {
            return Api_UploadVoice(LogonQQ, Data);
        }
        internal static bool Api_isFriend(string QQ)
        {
            return Api_IsFriend(LogonQQ, QQ);
        }
        internal static bool Api_sendMsg(SendType sendType, string groupDst, string QQDst, string Msg, int subType = 0)
        {
            return Api_SendMsg(LogonQQ, (int)sendType, subType, groupDst, QQDst, Msg);
        }
        internal static void Api_mute(string fromGroup, string dstQQ, int sec)
        {
            api.BanGroupMember(fromGroup, dstQQ, new TimeSpan(0, 0, Convert.ToInt32(sec)));
            api.SendGroupMessage(fromGroup, $"已将[@{dstQQ}]禁言{sec}秒钟");
            api.SendGroupMessage(fromGroup, GetVoiceGuid("大哥抽烟.mp3"));
        }
        internal static int Log(string msg)
        {
            return Api_OutPut(msg);
        }
        internal static string GetQQThroughAt(string At)
        {
            if (At.StartsWith("[@"))
                return At.Remove(0, 2).Remove(At.Length - 3);
            else
                return At;
        }
        internal static string At(string QQ) { return $"[@{QQ}]"; }
        private static bool ConvertToPcm(ref string filename)
        {
            using (var process = new Process())
            {
                var psi = new ProcessStartInfo("ffmpeg.exe", $"-i \"{filename}\" -y -f s16le -ar 24000 -ac 1 -acodec pcm_s16le \"{filename}.pcm\"")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                process.StartInfo = psi;
                process.Start();
                process.WaitForExit();
                if (process.ExitCode == 0)
                {
                    filename += ".pcm";
                    return true;
                }
                filename = "";
                return false;
            }
        }
        private static bool ConvertPcmToSlik(ref string filename)
        {
            using (var process = new Process())
            {
                var psi = new ProcessStartInfo("silk_v3_encoder.exe", $"\"{filename}\" \"{filename}.silk\" -tencent -rate 24000")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                process.StartInfo = psi;
                process.Start();
                process.WaitForExit();
                if (process.ExitCode == 0)
                {

                    filename += ".silk";
                    return true;
                }
                filename = "";
                return false;
            }
        }
    }
}
