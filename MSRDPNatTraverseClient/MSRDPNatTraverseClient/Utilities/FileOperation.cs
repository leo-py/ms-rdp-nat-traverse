using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace MSRDPNatTraverseClient.Utility
{
    /// <summary>
    /// 主要涉及到的一些文件操作
    /// 1. 能够正常读取配置文件，并且返回字符串
    /// 2. 能够正常存储制定文件内容到磁盘指定位置
    /// </summary>
    public class FileOperation
    {
        // 默认的存储路径
        private static readonly string DEFAULT_SERVER_LIST_PATH = @"config\servers";
        private static readonly string DEFAULT_CONFIG_PATH = @"config\config.txt";

        private static bool SaveTextFile(string path, string content)
        {
            try
            {
                // 检查配置是否存在，不存在则自动创建。
                var parentDir = Directory.GetParent(path).FullName;
                if (!Directory.Exists(parentDir))
                {
                    Directory.CreateDirectory(parentDir);
                }

                // 写入文件信息
                using (StreamWriter writer = new StreamWriter(path))
                {
                    writer.Write(content);
                }
                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }

        private static bool ReadTextFile(string path, out string content)
        {
            try
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    content = sr.ReadToEnd();
                }

                return true;
            }
            catch (Exception)
            {
                content = "";
                return false;
            }
        }

        public static bool SaveServerList(List<ProxyServer.ProxyServer> serverList)
        {
            var content = "";
            foreach (var server in serverList)
            {
                // 密码需要做特殊处理，即对密码进行加密
                server.LoginPassword = TextEncryption.Encrypt(server.LoginPassword, "password_*12398");

                // 转换为json字符串格式
                var json = JsonConvert.SerializeObject(server, Formatting.Indented);

                content += json + "\n|\n";
            }
            // 保存内容
            return SaveTextFile(DEFAULT_SERVER_LIST_PATH, content);
        }

        public static List<ProxyServer.ProxyServer> ReadServerList()
        {
            List<ProxyServer.ProxyServer> list = new List<ProxyServer.ProxyServer>();

            string content = "";
            if (ReadTextFile(DEFAULT_SERVER_LIST_PATH, out content))
            {
                string[] splitContent = content.Split(new char[] {  '|' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var item in splitContent)
                {
                    if (!string.IsNullOrWhiteSpace(item))
                    {
                        var server = JsonConvert.DeserializeObject<ProxyServer.ProxyServer>(item);
                        
                        // 把加密的秘密解密开
                        server.LoginPassword = TextEncryption.Decrypt(server.LoginPassword, "password_*12398");

                        list.Add(server);
                    }
                }
            }
       
            return list;
        }

        public static bool SaveConfig(Config.Config config)
        {
            // 不可以使用明文保存服务器。
            string json = JsonConvert.SerializeObject(config, Formatting.Indented);

            // 保存配置
            return SaveTextFile(DEFAULT_CONFIG_PATH, json);
        }

        public static Config.Config ReadConfig()
        {
            string content = "";
            if (ReadTextFile(DEFAULT_CONFIG_PATH, out content))
            {
                var conf = JsonConvert.DeserializeObject<Config.Config>(content);

                return conf;
            }

            return null;
        }
    }
}
