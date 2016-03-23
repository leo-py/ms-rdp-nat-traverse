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
                    closeWithoutQuitCheckBox.Checked, localMachine, -1);
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

        private void EditLocalMachineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditLocalMachine();
        }
        #endregion

        #region event_handlers for program control region
        private void machienInfoTextBox_DoubleClick(object sender, EventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb != null)
            {
                tb.ReadOnly = !tb.ReadOnly;

                if (tb.ReadOnly == true)
                {
                    // 保存信息
                    switch (tb.Tag.ToString())
                    {
                        case "name":
                            localMachine.Name = tb.Text;
                            break;
                        case "id":
                            localMachine.ID = int.Parse(tb.Text);
                            break;
                        case "rdpPort":
                            localMachine.RDPPort = int.Parse(tb.Text);
                            break;
                        case "desc":
                            localMachine.Description = tb.Text;
                            break;
                        default:
                            break;
                    }
                }
            }
        }

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

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
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
            MessageBox.Show("Good");
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

        private void Start()
        {
            //StartLocalServer();
            //MessageBox.Show("监听端口打开！");
            //BuildConnectionWithProxyServer(server);
        }

        private void Stop()
        {
            //ClientSendRequest("Request: msg from client!");
            //BuildTunnel(server);
        }

        private void Quit()
        {
            //QueryTunnelStatus();
            //this.Close();
        }
        #endregion

        #region core
        private void ApplyConfig(Config.Config conf)
        {
            if (conf != null)
            {
                // 显示本机的有关信息
                machineNameTextBox.Text = conf.Machine.Name;
                machineIDTextBox.Text = conf.Machine.ID.ToString("0000");
                RDPPortTextBox.Text = conf.Machine.RDPPort.ToString();
                machineDescriptionTextBox.Text = conf.Machine.Description;

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
                        serverNameTextBox.Text = server.Name;
                        serverIPTextBox.Text = string.Format("{0}:{1}", server.IPAdress, server.LoginPort);
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
            public int MachineId;
        }

        /// <summary>
        /// 请求获取MachineID，服务器会在收到请求后为该请求机器分配一个唯一的ID
        /// </summary>
        /// <param name="machineId">存储返回的ID</param>
        /// <returns></returns>
        private bool RequestMachineId(out int machineId)
        {
            machineId = 10000;

            // 要发送的请求消息
            var sendMsg = JsonConvert.SerializeObject(new RequestMsg()
            {
                function = "get",
                content = "machine_id"
            }, Formatting.Indented);

            var resp = ClientSendMessage(sendMsg);

            // 返回的消息是一个json格式的字符串
            var obj = JsonConvert.DeserializeObject(resp);

            return false;
        }

        private bool UploadMachineInfoToProxyServer(LocalMachine.LocalMachine machine)
        {
            // 构建要发送的消息
            var sendMsg = JsonConvert.SerializeObject(new RequestMsg()
            {   
                function = "upload",
                content = machine
            }, Formatting.Indented);

            // 向服务器发送消息
            var resp = ClientSendMessage(sendMsg);

            return true;
        }
        #endregion

        #region TCP通信相关函数
        /// <summary>
        /// 本地监听端口
        /// </summary>
        private readonly int localServerListenPort = 19930;
        private readonly string localServerListenIP = "0.0.0.0";

        /// <summary>
        /// 在全局范围引用。
        /// </summary>
        private TcpListener localListener = null;

        /// <summary>
        /// 在本地设置一个监听服务，在特定端口上监听来自远程代理服务器发送过来的请求信息；
        /// 并且根据请求信息，完成相关的工作。
        /// </summary>
        private void StartLocalServer()
        {
            localListener = new TcpListener(IPAddress.Parse(localServerListenIP), localServerListenPort);

            // 新建一个线程专门用于监听
            Thread localListenerThread = new Thread(LocalListenerServerThread);

            // 启动监听和处理线程
            localListenerThread.Start();
        }

        /// <summary>
        /// 本地监听进程。
        /// </summary>
        private void LocalListenerServerThread()
        {
            if (localListener != null)
            {
                localListener.Start();

                // 存放接收的数据buffer
                byte[] recvBuffer = null;

                // 接收消息字节大小
                int msgSize = 0;

                while (true)
                {
                    var client = localListener.AcceptTcpClient();
                    var inStream = client.GetStream();
                    recvBuffer = new byte[client.ReceiveBufferSize];

                    try
                    {
                        lock (inStream)
                        {
                            msgSize = inStream.Read(recvBuffer, 0, recvBuffer.Length);
                        }
                        if (msgSize == 0)
                        {
                            return;
                        }
                        string recvMsg = Encoding.Default.GetString(recvBuffer);

                        this.Invoke(new Action(() =>
                        {
                            machineDescriptionTextBox.Text += recvMsg + "\n";
                        }));
                        client.Close();
                    }
                    catch { }
                }
            }
        }

        /// <summary>
        /// 远程监听端口
        /// </summary>
        private readonly int remotePort = 19934;

        /// <summary>
        /// TCP客户端
        /// </summary>
        /// <param name="requestMsg"></param>
        /// <returns></returns>
        private string ClientSendMessage(string requestMsg)
        {
            TcpClient client = new TcpClient();
            IPAddress remoteIp = IPAddress.Parse(server.IPAdress);
            string response = "";

            #region 客户端请求以及等待响应代码
            try
            {
                client.Connect(remoteIp, remotePort);

                // 获取发送流，然后发送消息
                var stream = client.GetStream();

                byte[] outBuffer = Encoding.UTF8.GetBytes(requestMsg.Trim());
                stream.Write(outBuffer, 0, outBuffer.Length);
                //Thread sendThread = new Thread(ClientSendThread);
                //sendThread.Start(client.SendBufferSize);

                // 延时等待响应结果。
                Thread.Sleep(200);
                byte[] inBuffer = new byte[1024];
                stream.Read(inBuffer, 0, 1024);
                response = Encoding.UTF8.GetString(inBuffer).Trim();

                stream.Close();
                client.Close();
            }
            catch
            { }
            #endregion

            return response;
        }
        #endregion
    }
}
