using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Library
{
    public static class Functions
    {
        public static TimeSpan Max(TimeSpan value1, TimeSpan value2)
        {
            return value1 > value2 ? value1 : value2;
        }
        public static TimeSpan Min(TimeSpan value1, TimeSpan value2)
        {
            return value1 < value2 ? value1 : value2;
        }

        public static string CalcMD5(byte[] bytes)
        {
            MD5 calc = MD5.Create();
            byte[] datas = calc.ComputeHash(bytes);
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < datas.Length; i++)
            {
                sb.Append(datas[i].ToString("x2"));
            }

            return sb.ToString();
        }
        public static string BytesToString(long byteCount)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = Convert.ToDouble(byteCount);
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            string result = String.Format("{0:0.##} {1}", len, sizes[order]);
            return result;
        }

        public static string GetEnumDesc<T>(T e) where T : Enum
        {
            var type = e.GetType();
            var name = Enum.GetName(type, e);
            if (string.IsNullOrEmpty(name)) return null;

            var field = type.GetField(name);
            if (field == null) return null;

            DescriptionAttribute desc = field.GetCustomAttribute<DescriptionAttribute>();
            if (desc == null) return null;

            return desc.Description;
        }

        public static string CalcMD5(string src)
        {
            return CalcMD5(System.Text.Encoding.UTF8.GetBytes(src));
        }

        private static string DeviceCheckSum;
        private static readonly object DeviceCheckSumLock = new object();

        /// <summary>
        /// 取得本机稳定的设备标识（MD5），用于登录等网络包的 CheckSum 字段。
        /// 服务端会对空的 CheckSum 直接返回“禁止登录”，因此本方法保证返回值非空。
        /// 该值同时被服务端用作设备黑名单的键，必须在同一台机器上保持不变。
        /// </summary>
        public static string GetDeviceCheckSum()
        {
            if (!string.IsNullOrEmpty(DeviceCheckSum)) return DeviceCheckSum;

            lock (DeviceCheckSumLock)
            {
                if (!string.IsNullOrEmpty(DeviceCheckSum)) return DeviceCheckSum;

                string source = ReadMachineGuid();

                if (string.IsNullOrEmpty(source))
                    source = $"{Environment.MachineName}|{Environment.UserName}|{Environment.OSVersion.VersionString}|{Environment.ProcessorCount}";

                string sum = CalcMD5(source);

                //极端情况下（MD5 异常）退化为一次性随机值，宁可每次不同也不能为空。
                if (string.IsNullOrEmpty(sum))
                    sum = CalcMD5(Guid.NewGuid().ToString("N"));

                DeviceCheckSum = sum;

                return DeviceCheckSum;
            }
        }

        /// <summary>
        /// 读取系统安装时生成的机器唯一标识；读取失败返回 null，由调用方回退到其他信息。
        /// </summary>
        private static string ReadMachineGuid()
        {
            try
            {
                //显式使用 64 位视图，避免 32 位进程被注册表重定向后取不到值。
                using (Microsoft.Win32.RegistryKey baseKey = Microsoft.Win32.RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, Microsoft.Win32.RegistryView.Registry64))
                using (Microsoft.Win32.RegistryKey key = baseKey.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography"))
                {
                    if (key == null) return null;

                    return key.GetValue("MachineGuid") as string;
                }
            }
            catch
            {
                return null;
            }
        }

        public static string ToString(TimeSpan time, bool details, bool small = false)
        {
            string textD = null;
            string textH = null;
            string textM = null;
            string textS = null;

            if (time.Days >= 1) textD = $"{time.Days} {(small ? "D" : "天")}";

            if (time.Hours >= 1) textH = $"{time.Hours} {(small ? "H" : "小时")}";

            if (time.Minutes >= 1) textM = $"{time.Minutes} {(small ? "M" : "分")}";

            if (time.Seconds >= 1) textS = $"{ time.Seconds} {(small ? "S" : "秒")}";
            else if (time.TotalSeconds > 1 && time.Seconds > 0) textS = "少于一秒";

            if (!details)
                return textD ?? textH ?? textM ?? textS;

            if (textD != null)
                return textD + " " + (textH ?? textM ?? textS);

            if (textH != null)
                return textH + " " + (textM ?? textS);

            if (textM != null)
                return textM + " " + textS;

            return textS ?? string.Empty;
        }

        public static string RandomString(Random Random, int length)
        {
            StringBuilder str = new StringBuilder();

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz";

            for (int i = 0; i < length; i++)
                str.Append(chars[Random.Next(chars.Length)]);

            return str.ToString();
        }
    }
}
