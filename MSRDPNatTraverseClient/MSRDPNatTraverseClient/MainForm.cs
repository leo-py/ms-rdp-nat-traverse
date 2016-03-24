using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using MSRDPNatTraverseClient.Config;
using MSRDPNatTraverseClient.Utility;
using MSRDPNatTraverseClient.SSHReverseTunnel;
using Newtonsoft.Json;
using System.Diagnostics;
using MSRDPNatTraverseClient.LocalMachine;

namespace MSRDPNatTraverseClient
{
    public partial class MainForm : Form
    {
        #region global variables or constants
        private LocalMachine.LocalMachine localMachine = null;
        private Config.Config programConfig = null;
        private ProxyServer.ProxyServer server = null;
        #endregion
        public MainForm()
        {
            InitializeComponent();

            // 读取默认的配置信息
            programConfig = FileOperation.ReadConfig();

            if (programConfig != null)
            {
                if (programConfig.Machine != null)
                {
                    localMachine = programConfig.Machine;
                }
                else
                {
                    localMachine = new LocalMachine.LocalMachine();
                    programConfig.Machine = localMachine;
                }
            }
            else
            {
                // 做一些初始化的工作
                localMachine = new LocalMachine.LocalMachine();
                server = new ProxyServer.ProxyServer();
                programConfig = new Config.Config(autoStartupCheckBox.Checked,
                    closeWithoutQuitCheckBox.Checked, localMachine, -1, 9000);
            }

            // 显示这些信息
            ApplyConfig(programConfig);
        }

        #region event_handlers for menus

        private void EditServerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditServer();
        }

        private void ChangeServerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChangeServer();
        }

        private void AboutProgramToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowAboutDialog();
        }

        private void EditLocalMachineToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            EditLocalMachine();
        }

        private void SaveLocalMachineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveLocalMachine();
        }
        #endregion

        #region event_handlers for program control region
        private void autoStartupCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            var cb = sender as CheckBox;
            if (cb != null)
            {
                SetAutoStartup(cb.Checked);
            }
        }

        private void closeWithoutQuitCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            var cb = sender as CheckBox;
            if (cb != null)
            {
                SetCloseWithoutQuit(cb.Checked);
            }
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            Start();
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            Stop();
        }

        private void quitButton_Click(object sender, EventArgs e)
        {
            Quit();
        }

        private void upddateRemteMachineListButton_Click(object sender, EventArgs e)
        {
            UpdateRemoteMachineList();
        }
        private async void controlButton_Click(object sender, EventArgs e)
        {
            var bt = sender as Button;
            if (bt != null)
            {
                if (remoteMachineListBox.SelectedIndex == -1)
                {
                    return;
                }
                int remoteId = int.Parse(remoteMachineListBox.SelectedItem.ToString().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0]);
                Debug.WriteLine(string.Format("连接远程主机：{0}", remoteId));

                if (bt.Text.Equals("连接"))
                {
                    if (await ConnectRemoteMachineAsync(remoteId))
                    {
                        bt.Text = "断开";
                    }
                }
                else
                {
                    if (await DisconnectRemoteMachineAsync(remoteId))
                    {
                        bt.Text = "连接";
                    }
                }
            }
        }

        private void machineInfoTextBox_ReadOnlyChanged(object sender, EventArgs e)
        {
            var tb = sender as TextBox;
            if (tb != null)
            {
                if (tb.ReadOnly)
                {
                    tb.ForeColor = Color.Black;
                }
                else
                {
                    tb.ForeColor = Color.Blue;
                }
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 关闭所有已经打开的线程
            foreach (var thr in customThreadDict)
            {
                thr.Value.Abort();
            }

            // 关闭打开的隧道
            CloseAllSSHReverseTunnels();

            // 保存当前的配置信息
            programConfig.Machine = localMachine;
            FileOperation.SaveConfig(programConfig);
        }
        #endregion

        #region proxy functions: do what the handlers what to
        private void EditLocalMachine()
        {
            // 将编辑窗口改为可写状态
            machineNameTextBox.ReadOnly = false;
            machineDescriptionTextBox.ReadOnly = false;
            RDPPortTextBox.ReadOnly = false;
        }

        private void SaveLocalMachine()
        {
            // 重置为只读状态
            machineNameTextBox.ReadOnly = true;
            machineDescriptionTextBox.ReadOnly = true;
            RDPPortTextBox.ReadOnly = true;

            // 保存所有信息
            localMachine.Name = machineNameTextBox.Text;
            localMachine.Description = machineDescriptionTextBox.Text;
            localMachine.RDPPort = int.Parse(RDPPortTextBox.Text);

            // 告诉配置信息类
            programConfig.Machine = localMachine;
        }

        private void EditServer()
        {
            EditServerForm dialogForm = new EditServerForm();
            dialogForm.ShowDialog();

            // 关闭服务器编辑窗口后，需要从新从磁盘加载最新服务器配置，防止因为没有及时更新导致错误。
            var list = FileOperation.ReadServerList();

            if (list.Count > 0)
            {
                // 加载同名服务器最新信息
                for (var i = 0; i < list.Count; i++)
                {
                    if (list[i].Name == server.Name)
                    {
                        server = list[i];
                        ShowProxyServerInfo(server);

                        // 同时更新配置
                        programConfig.SelectedServerIndex = i;
                        return;
                    }
                }

                // 不存在的话，那么就尝试使用第一个
                server = list[0];
                ShowProxyServerInfo(server);
                // 同时更新配置
                programConfig.SelectedServerIndex = 0;
            }
            else
            {
                server = new ProxyServer.ProxyServer();
                ShowProxyServerInfo(server);
                programConfig.SelectedServerIndex = -1;
            }
        }

        private void ChangeServer()
        {
            ChangeServerForm form = new ChangeServerForm();
            if (form.ShowDialog() == DialogResult.Yes)
            {
                var obj = form.SelectedServer;
                if (obj != null)
                {
                    server = obj;
                    ShowProxyServerInfo(server);
                    // 更新配置信息
                    programConfig.SelectedServerIndex = form.SelectedServerIndex;
                }
            }
        }

        private void ShowProxyServerInfo(ProxyServer.ProxyServer server)
        {
            // 显示代理服务器信息
            serverNameTextBox.Text = server.Name;
            serverIPTextBox.Text = string.Format("{0}:{1}", server.IPAdress, server.LoginPort);
        }

        private void ShowLocalMachineInfo(LocalMachine.LocalMachine machine)
        {
            machineNameTextBox.Text = machine.Name;
            machineIDTextBox.Text = machine.ID.ToString();
            machineDescriptionTextBox.Text = machine.Description;
            RDPPortTextBox.Text = machine.RDPPort.ToString();
        }

        private void ShowAboutDialog()
        {
            (new AboutForm()).ShowDialog();
        }

        private void SetAutoStartup(bool enable)
        {
            programConfig.AutoStartup = enable;
        }

        private void SetCloseWithoutQuit(bool enable)
        {
            programConfig.EnableBackgroundMode = enable;
        }

        /// <summary>
        /// 设置服务器连接状态
        /// </summary>
        /// <param name="status"></param>
        private void SetServerConnectionStatus(bool status)
        {
            machineIsOnline = status;
            if (status)
            {
                serverStatusTextBox.Text = "连接正常";
            }
            else
            {
                serverStatusTextBox.Text = "连接断开";
            }
        }

        /// <summary>
        /// 存放的自定义的线程集合，便于统一管理
        /// </summary>
        private Dictionary<string, Thread> customThreadDict = new Dictionary<string, Thread>();

        /// <summary>
        /// 只有与服务器建立连接后才为true
        /// </summary>
        private bool machineIsOnline = false;
        private async void Start()
        {
            // 根据协议要求，首先需要获取一个ID
            Debug.WriteLine(string.Format("客户端向代理服务器{0}({1}:{2})发送请求：获取一个唯一分配的ID", server.Name, server.IPAdress, remotePort));
            var newId = await RequestMachineIdAsync();
            if (newId == -1)
            {
                // 表明没有获取到合法的ID，因此无法与代理服务器建立连接，故为断开连接状态
                Debug.WriteLine("请求获取ID失败");
                SetServerConnectionStatus(false);
            }
            else
            {
                // 成功获取到ID后，我们将会输出调试信息，查看获得的ID是多少
                Debug.WriteLine(string.Format("客户端成功获取到ID: {0}", newId));

                // 更新本机的ID，并同步显示在窗口中
                localMachine.ID = newId;
                ShowLocalMachineInfo(localMachine);

                // 下面开始向服务器上传当前机器的信息
                Debug.WriteLine("向代理服务器发送机器信息");
                if (await UploadMachineInfoToProxyServerAsync(localMachine))
                {
                    SetServerConnectionStatus(true);
                    Debug.WriteLine("上传本机信息成功");

                    // 启动两个子线程，分别用于查询远程请求状态和发送Keep-Alive消息，保证让服务器知道客户端的存在
                    Debug.WriteLine("启动线程：定时查询远程连接请求");
                    Thread thr1 = new Thread(QueryRemoteControlRequestThread);

                    // 添加到列表中便于管理
                    if (!customThreadDict.ContainsKey("remoteControlRequestThread"))
                    {
                        customThreadDict.Add("remoteControlRequestThread", thr1);
                        thr1.Start();
                    }
                    
                }
            }
            
        }

        private void Stop()
        {
            var tunnel = new MSRDPNatTraverseClient.SSHReverseTunnel.SSHReverseTunnel(localMachine, server, 11812);
            tunnel.Start();

            tunnelList.Add(tunnel);
        }

        private void Quit()
        {
            //QueryTunnelStatus();
            //this.Close();
        }

        /// <summary>
        /// 更新在线用户列表
        /// </summary>
        private async void UpdateRemoteMachineList()
        {
            Dictionary<int, string> list = await GetRemoteMachineList(localMachine.ID);
            if (list == null)
            {
                return;
            }

            remoteMachineListBox.Items.Clear();
            foreach (var item in list)
            {
                remoteMachineListBox.Items.Add(string.Format("{0}    {1}", item.Key, item.Value));
            }
        }

        /// <summary>
        /// 完成和远程主机连接的过程
        /// </summary>
        /// <param name="remoteId"></param>
        /// <returns></returns>
        private async Task<bool> ConnectRemoteMachineAsync(int remoteId)
        {
            // 第一步，向远程主机发送请求，要求连接到主机remoteId上
            if (await RequestToBuildConnectionWithOnlineMachineAsync(localMachine.ID, remoteId))
            {
                Debug.WriteLine("远程服务器已经收到消息，并向远程主机发送邀请");

                // 第二步，等待远程主机建立隧道
                int tryCount = 10;
                while (await QueryTunnelStatusAsync(remoteId))
                {
                    Thread.Sleep(500);
                    if (tryCount-- == 0)
                    {
                        return false;
                    }
                }

                Debug.WriteLine("已经建立了隧道，准备等待连接");
                // 获取远程主机的IP和端口号
                var result = await GetPeeredRemoteMachineAddressAsync(remoteId);

                MessageBox.Show("请打开微软远程控制程序，输入地址：" + result);

                return true;
            }
            else
            {
                Debug.WriteLine("请求连接失败");
                return false;
            }
        }

        /// <summary>
        /// 断开与远程主机的连接
        /// </summary>
        /// <param name="remoteId"></param>
        private async Task<bool> DisconnectRemoteMachineAsync(int remoteId)
        {
            // 只需要告诉远程主机关闭隧道连接即可
            // 具体方法：通过协议，修改远程主机的tunnel_port属性，
            // 远程主机检查到该tunnel_status为false时，无论是否建立连接，都
            // 会强制关闭隧道，关闭连接
            bool result = await ResetTunnelPortRequestAsync(remoteId);
            return result;
        }

        /// <summary>
        /// 标记隧道是否已经打开
        /// </summary>
        private bool tunnelIsOpen = false;
        private async void QueryRemoteControlRequestThread()
        {
            while (true)
            {
                // 只有在线，才会发送查询消息
                if (machineIsOnline)
                {
                    if (tunnelIsOpen)
                    {
                        // 但端口建立后，就轮询隧道状态，但隧道不存在后，就关闭当前的进程
                        if (await QueryTunnelStatusAsync(localMachine.ID))
                        {
                            Debug.WriteLine("隧道仍然正常");
                        }
                        else
                        {
                            Debug.WriteLine("隧道断开");
                            // 关闭隧道进程
                            CloseAllSSHReverseTunnels();
                            tunnelIsOpen = false;
                        }
                    }
                    else
                    {
                        if (await QueryRemoteControlRequestAsync(localMachine.ID))
                        {
                            Debug.WriteLine("收到远程连接请求");

                            // 根据协议要求，需要向服务器申请得到一个可用的代理隧道端口
                            int port = await GetAvailableTunnelPortAsync(localMachine.ID);

                            if (port != -1)
                            {
                                Debug.WriteLine("获得隧道端口：" + port.ToString());

                                //接下来会开始建立隧道
                                SSHReverseTunnel.SSHReverseTunnel tunnel = new SSHReverseTunnel.SSHReverseTunnel(localMachine, server, port);

                                if (tunnel.Start())
                                {
                                    Debug.WriteLine("本地建立隧道进程启动");

                                    // 延时30s查询有没有成功建立隧道。
                                    Thread.Sleep(2500);

                                    // 向服务器询问隧道有没有建立成功
                                    if (await QueryTunnelStatusAsync(localMachine.ID))
                                    {
                                        Debug.WriteLine(string.Format("建立隧道成功，地址：{0}:{1}", server.IPAdress, port));
                                        tunnelIsOpen = true;
                                        tunnelList.Add(tunnel);

                                        // 此时告诉服务器，已经处理了请求的连接
                                        if (await ClearRemoteControlRequestAsync(localMachine.ID))
                                        {
                                            Debug.WriteLine("反向隧道已经成功建立，并且可以从远程登录到本机");
                                        }
                                    }
                                    else
                                    {
                                        tunnel.Stop();
                                    }
                                }
                            }
                        }
                        else
                        {
                            Debug.WriteLine("没有远程连接请求");
                        }
                    }
                }
                // 延时等待一段时间继续查询，目前为5s
                Thread.Sleep(5 * 1000);
            }
        }
        #endregion

        #region core
        private void ApplyConfig(Config.Config conf)
        {
            if (conf != null)
            {
                // 显示本机的有关信息
                ShowLocalMachineInfo(conf.Machine);

                // checkbox状态
                autoStartupCheckBox.Checked = conf.AutoStartup;
                closeWithoutQuitCheckBox.Checked = conf.EnableBackgroundMode;

                // 显示代理服务器信息
                if (conf.SelectedServerIndex != -1)
                {
                    var serverList = FileOperation.ReadServerList();
                    if (serverList.Count > 0)
                    {
                        if (conf.SelectedServerIndex < serverList.Count)
                        {
                            server = serverList[conf.SelectedServerIndex];
                        }
                        else
                        {
                            server = serverList[0];
                        }
                        ShowProxyServerInfo(server);
                    }  
                }
            }
        }
        #endregion

        // tunnel列表，可以存放备用隧道。防止连接失败。
        List<SSHReverseTunnel.SSHReverseTunnel> tunnelList = new List<SSHReverseTunnel.SSHReverseTunnel>();

        private void CloseAllSSHReverseTunnels()
        {
            foreach (var tunnel in tunnelList)
            {
                tunnel.Stop();
            }
        }

        #region 网络服务协议处理有关
        /// <summary>
        /// 发送消息的结构
        /// </summary>
        class RequestMsg
        {
            public string function;
            public object content;
        }

        class RequestMsgWithId : RequestMsg
        {
            public int id;
        }

        class Response
        {
            public bool isResultEffective;   // 表示结果能否使用
            public object Result;            // 返回的结果
        }

        /// <summary>
        /// 执行标准的协议请求，返回固定格式的应答。
        /// </summary>
        /// <param name="function"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        private async Task<object> ExecuteProtocalRequestAsync<T>(string function, object content)
        {
            // 构建消息
            var sendMsg = JsonConvert.SerializeObject(new RequestMsg()
            {
                function = function,
                content = content
            }, Formatting.Indented);

            // 发送消息，等待应答
            var resp = await ClientSendMessageAsync(sendMsg);

            // 判断并返回响应
            var responseDict = JsonConvert.DeserializeObject<Dictionary<string, T>>(resp);

            if (responseDict.Count == 1 && responseDict.ContainsKey("response"))
            {
                return responseDict["response"];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 执行标准的协议请求，返回固定格式的应答。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="function"></param>
        /// <param name="id"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        private async Task<object> ExecuteProtocalRequestAsync<T>(string function, int id, object content)
        {
            // 构建消息
            var sendMsg = JsonConvert.SerializeObject(new RequestMsgWithId()
            {
                function = function,
                id = id,
                content = content
            }, Formatting.Indented);

            // 发送消息，等待应答
            var resp = await ClientSendMessageAsync(sendMsg);

            // 判断并返回响应
            var responseDict = JsonConvert.DeserializeObject<Dictionary<string, T>>(resp);

            if (responseDict != null && responseDict.Count == 1 && responseDict.ContainsKey("response"))
            {
                return responseDict["response"];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 请求获取MachineID，服务器会在收到请求后为该请求机器分配一个唯一的ID
        /// </summary>
        /// <returns></returns>
        private async Task<int> RequestMachineIdAsync()
        {
            var result = await ExecuteProtocalRequestAsync<int>("get", "machine_id");

            if (result != null)
            {
                return(int)(result);
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// 向代理服务器发送本机信息
        /// </summary>
        /// <param name="machine"></param>
        /// <returns></returns>
        private async Task<bool> UploadMachineInfoToProxyServerAsync(LocalMachine.LocalMachine machine)
        {
            var result = await ExecuteProtocalRequestAsync<bool>("upload", machine);

            if (result != null)
            {
                return (bool)(result);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 请求与在线用户建立连接，注意，该函数执行后，不保证隧道建立成功
        /// 因为可能远程主机掉线、或者正在被其他主机连接，因为也会拒绝连接
        /// </summary>
        /// <param name="local"></param>
        /// <param name="remoteMachineId"></param>
        /// <returns></returns>
        private async Task<bool> RequestToBuildConnectionWithOnlineMachineAsync(int localMachineId, int remoteMachineId)
        {
            var result = await ExecuteProtocalRequestAsync<bool>("connect_remote", localMachineId, remoteMachineId);

            if (result != null)
            {
                return (bool)(result);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 该函数用于向服务器查询已经尝试建立成功的配对远程计算机的地址
        /// </summary>
        /// <param name="remoteMachineId"></param>
        /// <returns></returns>
        private async Task<string> GetPeeredRemoteMachineAddressAsync(int remoteMachineId)
        {
            var result = await ExecuteProtocalRequestAsync<string>("get", remoteMachineId, "remote_machine_address");

            if (result != null)
            {
                return (string)(result);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 客户端请求建立一条隧道，但是需要得到一个合法的端口地址在服务器上监听
        /// </summary>
        /// <param name="localMachineId"></param>
        /// <returns></returns>
        private async Task<int> GetAvailableTunnelPortAsync(int localMachineId)
        {
            var result = await ExecuteProtocalRequestAsync<int>("get", localMachineId, "available_port");

            if (result != null)
            {
                return (int)(result);
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// 查询已经尝试建立隧道的状态
        /// </summary>
        /// <param name="localMachineId"></param>
        /// <returns></returns>
        private async Task<bool> QueryTunnelStatusAsync(int localMachineId)
        {
            var result = await ExecuteProtocalRequestAsync<bool>("query", localMachineId, "tunnel_status");

            if (result != null)
            {
                return (bool)(result);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 查询是否有其他机器要求与自身建立连接请求
        /// </summary>
        /// <param name="localMachineId"></param>
        /// <returns></returns>
        private async Task<bool> QueryRemoteControlRequestAsync(int localMachineId)
        {
            var result = await ExecuteProtocalRequestAsync<bool>("query", localMachineId, "remote_control_request");

            if (result != null)
            {
               return (bool)(result);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 告诉代理服务器，本机已经接收到并且处理完远程连接请求，并且此时的隧道也已经建立成功
        /// </summary>
        /// <param name="localMachineId"></param>
        /// <returns></returns>
        private async Task<bool> ClearRemoteControlRequestAsync(int localMachineId)
        {
            var result = await ExecuteProtocalRequestAsync<bool>("clear", localMachineId, "remote_control_request");

            if (result != null)
            {
                return (bool)(result);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 清除主机的标志信息：隧道的状态
        /// </summary>
        /// <param name="localMachineId"></param>
        /// <returns></returns>
        private async Task<bool> ResetTunnelPortRequestAsync(int machineId)
        {
            var result = await ExecuteProtocalRequestAsync<bool>("reset", machineId, "tunnel_status");

            if (result != null)
            {
                return (bool)(result);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 向服务器请求获得当前所有注册并且在线的机器列表
        /// </summary>
        /// <param name="localMachieId"></param>
        /// <returns></returns>
        private async Task<Dictionary<int, string>> GetRemoteMachineList(int localMachineId)
        {
            var result = await ExecuteProtocalRequestAsync<Dictionary<int, string>>("get", localMachineId, "online_machine_list");

            if (result != null)
            {
                return (Dictionary<int, string>)(result);
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region TCP通信相关函数
        /// <summary>
        /// 远程监听端口
        /// </summary>
        private readonly int remotePort = 9000;

        /// <summary>
        /// TCP客户端
        /// 更新：修改为支持异步调用的方法
        /// </summary>
        /// <param name="requestMsg"></param>
        /// <returns></returns>
        private async Task<string> ClientSendMessageAsync(string requestMsg)
        {
            TcpClient client = new TcpClient();
            IPAddress remoteIp = IPAddress.Parse(server.IPAdress);
            string response = "";
            await Task.Run(new Action(() =>
            {
                #region 客户端请求以及等待响应代码
                try
                {
                    client.Connect(remoteIp, programConfig.ProxyServerListenPort);

                    // 获取发送流，然后发送消息
                    var stream = client.GetStream();

                    byte[] outBuffer = Encoding.UTF8.GetBytes(requestMsg.Trim());
                    //Debug.WriteLine(string.Format("\n**************************************************************"));
                    //Debug.WriteLine(string.Format("发送：{0}\n", requestMsg));
                    stream.Write(outBuffer, 0, outBuffer.Length);
                    //Thread sendThread = new Thread(ClientSendThread);
                    //sendThread.Start(client.SendBufferSize);

                    // 延时等待响应结果。
                    Thread.Sleep(200);
                    byte[] inBuffer = new byte[1024];
                    stream.Read(inBuffer, 0, 1024);
                    response = Encoding.UTF8.GetString(inBuffer).Trim();
                    //Debug.WriteLine(string.Format("\n接收：{0}\n", response));
                    //Debug.WriteLine(string.Format("\n**************************************************************\n"));
                    stream.Close();
                    client.Close();
                }
                catch
                { }
                #endregion
            }));
            return response.Replace('\0', ' ').Trim();
        }
        #endregion

    }
}
