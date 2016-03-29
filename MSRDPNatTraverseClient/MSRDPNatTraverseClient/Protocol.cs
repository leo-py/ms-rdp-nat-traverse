using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.Diagnostics;

namespace MSRDPNatTraverseClient.MainFom
{
    /// <summary>
    /// 协议有关的函数放置在该文件下
    /// </summary>
    public class Protocol
    {
        /// <summary>
        /// 用于构建请求消息的类
        /// </summary>
        class Request
        {
            public string key = "id";
            public int id = -1;
            public object value = null;
        }

        #region 公有方法
        /// <summary>
        /// 请求获得id
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public static async Task<int> GetClientIdAsync(string host, int port)
        {
            // 构建要发送的消息
            var content = BuildRequestContent("get", "id", -1, null);

            var result = await SendProtocalRequestAsync<int>(host, port, content);

            if (result != null)
            {
                return (int)result;
            }
            else
            {
                return -1;
            }     
        }

        /// <summary>
        /// 上传客户端信息
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public static async Task<bool> PostClientInformationAsync(string host, int port, Client.Client client)
        {
            // 构建要发送的消息
            var content = BuildRequestContent("post", "client_info", client.ID, client).ToLower();

            var result = await SendProtocalRequestAsync<bool>(host, port, content);

            if (result != null)
            {
                return (bool)result;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 获取有没有远程控制请求
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="clientId"></param>
        /// <returns></returns>
        public static async Task<bool> GetControlRequestAsync(string host, int port, int clientId)
        {
            // 构建要发送的消息
            var content = BuildRequestContent("get", "control_request", clientId, null).ToLower();

            var result = await SendProtocalRequestAsync<bool>(host, port, content);

            if (result != null)
            {
                return (bool)result;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 设置指定id客户端的远程控制请求
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="clientId"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static async Task<bool> PostControlRequestAsync(string host, int port, int clientId, bool value)
        {
            // 构建要发送的消息
            var content = BuildRequestContent("post", "control_request", clientId, value).ToLower();

            var result = await SendProtocalRequestAsync<bool>(host, port, content);

            if (result != null)
            {
                return (bool)result;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 获取一个随机分配的端口号
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="clientId"></param>
        /// <returns></returns>
        public static async Task<int> GetTunnelPortAsync(string host, int port, int clientId, bool generateNewId)
        {
            // 构建要发送的消息
            var content = BuildRequestContent("get", "tunnel_port", clientId, generateNewId).ToLower();

            var result = await SendProtocalRequestAsync<int>(host, port, content);

            if (result != null)
            {
                return (int)result;
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// 设置指定id隧道端口号
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="clientId"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static async Task<bool> PostTunnelPortAsync(string host, int port, int clientId, int value)
        {
            // 构建要发送的消息
            var content = BuildRequestContent("post", "tunnel_port", clientId, value).ToLower();

            var result = await SendProtocalRequestAsync<bool>(host, port, content);

            if (result != null)
            {
                return (bool)result;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 查询指定id的隧道端口号是否正常监听
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="clientId"></param>
        /// <returns></returns>
        public static async Task<bool> GetTunnelStatusAsync(string host, int port, int clientId)
        {
            // 构建要发送的消息
            var content = BuildRequestContent("get", "tunnel_status", clientId, null).ToLower();

            var result = await SendProtocalRequestAsync<bool>(host, port, content);

            if (result != null)
            {
                return (bool)result;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 获取是否被控制的标志
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="clientId"></param>
        /// <returns></returns>
        public static async Task<bool> GetIsUnderControlAsync(string host, int port, int clientId)
        {
            // 构建要发送的消息
            var content = BuildRequestContent("get", "is_under_control", clientId, null).ToLower();

            var result = await SendProtocalRequestAsync<bool>(host, port, content);

            if (result != null)
            {
                return (bool)result;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 设置制定id客户端的被控制状态
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="clientId"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static async Task<bool> PostIsUnderControlAsync(string host, int port, int clientId, bool value)
        {
            // 构建要发送的消息
            var content = BuildRequestContent("post", "is_under_control", clientId, value).ToLower();

            var result = await SendProtocalRequestAsync<bool>(host, port, content);

            if (result != null)
            {
                return (bool)result;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 获取与此客户端远程连接的id
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="clientId"></param>
        /// <returns></returns>
        public static async Task<int> GetPeeredRemoteIdAsync(string host, int port, int clientId)
        {
            // 构建要发送的消息
            var content = BuildRequestContent("get", "peered_remote_id", clientId, null).ToLower();

            var result = await SendProtocalRequestAsync<int>(host, port, content);

            if (result != null)
            {
                return (int)result;
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// 设置指定id的配对客户端的id
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="clientId"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static async Task<bool> PostPeeredRemoteIdAsync(string host, int port, int clientId, int value)
        {
            // 构建要发送的消息
            var content = BuildRequestContent("post", "peered_remote_id", clientId, value).ToLower();
            var result = await SendProtocalRequestAsync<bool>(host, port, content);
            if (result != null)
            {
                return (bool)result;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 获取指定id的存活计数值
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="clientId"></param>
        /// <returns></returns>
        public static async Task<int> GetKeepAliveCountAsync(string host, int port, int clientId)
        {
            // 构建要发送的消息
            var content = BuildRequestContent("get", "keep_alive_count", clientId, null).ToLower();

            var result = await SendProtocalRequestAsync<int>(host, port, content);

            if (result != null)
            {
                return (int)result;
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// 设置存活状态值
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="clientId"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static async Task<bool> PostKeepAliveCountAsync(string host, int port, int clientId, int value)
        {
            // 构建要发送的消息
            var content = BuildRequestContent("post", "keep_alive_count", clientId, value).ToLower();

            var result = await SendProtocalRequestAsync<bool>(host, port, content);

            if (result != null)
            {
                return (bool)result;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 获取指定id的客户端是否在线
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="clientId"></param>
        /// <returns></returns>
        public static async Task<bool> GetIsOnlineAsync(string host, int port, int clientId)
        {
            // 构建要发送的消息
            var content = BuildRequestContent("get", "is_online", clientId, null).ToLower();

            var result = await SendProtocalRequestAsync<bool>(host, port, content);

            if (result != null)
            {
                return (bool)result;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 获取在线的客户端的列表
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public static async Task<Dictionary<int, string>> GetOnlineClientListAsync(string host, int port, int clientId)
        {
            // 构建要发送的消息
            var content = BuildRequestContent("get", "online_list", clientId, null).ToLower();

            var result = await SendProtocalRequestAsync<Dictionary<int, string>>(host, port, content);

            if (result != null)
            {
                return (Dictionary<int, string>)result;
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 发送协议规定格式的请求，并返回响应
        /// </summary>
        /// <typeparam name="T">指定期望的结果类型</typeparam>
        /// <param name="host">主机</param>
        /// <param name="port">端口</param>
        /// <param name="content">内容</param>
        /// <returns>null或者T类型的值</returns>
        private static async Task<object> SendProtocalRequestAsync<T>(string host, int port, string content)
        {
            // 发送并等待响应结果
            string result = await SendAsync(host, port, content);

            // 判断响应结果
            if (string.IsNullOrWhiteSpace(result))
            {
                Debug.WriteLine("请求失败");
                return null;
            }

            // 否则，尝试转换结果
            var dict = JsonConvert.DeserializeObject<Dictionary<string, T>>(result);

            if (dict != null)
            {
                return dict["response"];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 构建符合协议要求格式的字符串
        /// </summary>
        /// <param name="type"></param>
        /// <param name="key"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        private static string BuildRequestContent(string type, string key, int id, object value)
        {
            Dictionary<string, object> request = new Dictionary<string, object>();
            request[type] = new Request() { key = key, id = id, value = value };
            return JsonConvert.SerializeObject(request, Formatting.Indented);
        }

        /// <summary>
        /// 客户端发送消息
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private static async Task<string> SendAsync(string hostname, int port, string content)
        {
            try
            {
                var client = new TcpClient();
                await client.ConnectAsync(hostname, port);
                var stream = client.GetStream();

                // 构建要发送的消息
                Debug.WriteLine("发送：" + content);
                byte[] buffer = Encoding.UTF8.GetBytes(content);
                await stream.WriteAsync(buffer, 0, buffer.Length);

                // 等待响应消息
                byte[] readBuffer = new byte[1024];
                await stream.ReadAsync(readBuffer, 0, readBuffer.Length);

                // 转换成string类型
                string response = Encoding.UTF8.GetString(readBuffer);
                Debug.WriteLine("接收：" + response);
                Debug.WriteLine("\n");

                stream.Close();
                client.Close();
                return response;
            }
            catch (Exception ee)
            {
                Debug.WriteLine(ee.Message);
                return "";
            }
        }
        #endregion
    }
}
