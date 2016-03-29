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
using MSRDPNatTraverseClient.Client;
using MSRDPNatTraverseClient.MainFom;

namespace MSRDPNatTraverseClient
{
    public partial class MainForm : Form
    {
        #region 一些使用到的全局变量
        /// <summary>
        /// 本地客户端的对象实例
        /// </summary>
        private Client.Client client = null;

        /// <summary>
        /// 配置对象实例
        /// </summary>
        private Config.Config programConfig = null;

        /// <summary>
        /// 代理服务器对象实例
        /// </summary>
        private ProxyServer.ProxyServer proxyServer = null;

        /// <summary>
        /// 两个特殊线程
        /// </summary>
        private Thread keepAliveThread = null;
        private Thread queryStatusThread = null;

        /// <summary>
        /// 取消线程的执行
        /// </summary>
        CancellationTokenSource cts = null;

        /// <summary>
        /// 定时器，用于检测要不要尝试重新连接代理服务器，重新启动服务
        /// </summary>
        private System.Windows.Forms.Timer autoRestartServiceTimer = new System.Windows.Forms.Timer();

        /// <summary>
        /// 在线客户端列表
        /// </summary>
        private List<int> onlineClientList = new List<int>();

        // tunnel列表，可以存放备用隧道。防止连接失败。
        List<SSHReverseTunnel.SSHReverseTunnel> tunnelList = new List<SSHReverseTunnel.SSHReverseTunnel>();

        /// <summary>
        /// 标记能不能真的退出应用
        /// </summary>
        private bool canQuit = false;

        #endregion
        public MainForm()
        {
            InitializeComponent();
        }

        #region 菜单项及按钮等事件处理函数集合
        /// <summary>
        /// 编辑菜单点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditServerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditServer();
        }

        /// <summary>
        /// 更换服务器菜单点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeServerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChangeServer();
        }

        /// <summary>
        /// 关于菜单项点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AboutProgramToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowAboutDialog();
        }

        /// <summary>
        /// 编辑本地客户端菜单点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditLocalMachineToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            EditClient();
        }

        /// <summary>
        /// 保存本地客户端菜单项点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveLocalMachineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveClient();
        }

        /// <summary>
        /// 开机启动选择事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void autoStartupCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            var cb = sender as CheckBox;
            if (cb != null)
            {
                SetAutoStartup(cb.Checked);
                
                // 但这个选中后，必须要保证选择另外一个隐藏而不退出
                if (cb.Checked)
                {
                    closeWithoutQuitCheckBox.Checked = true;
                }
            }
        }

        /// <summary>
        /// 关闭窗口在后台运行点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void closeWithoutQuitCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            var cb = sender as CheckBox;
            if (cb != null)
            {
                SetCloseWithoutQuit(cb.Checked);

                // 当这一个项目取消后，保证不能自动启动服务
                if (!cb.Checked)
                {
                    autoStartCheckBox.Checked = false;
                }
            }
        }


        private async void acceptControlRequestCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            var cb = sender as CheckBox;
            if (cb != null)
            {
                // 考虑没有启动服务的情况下，是可以随意修改的
               if (startButton.Enabled == false)
                {
                    if (cb.Checked)
                    {
                        if (await AcceptControlRequest(true))
                        {
                            Debug.WriteLine("修改状态成功");
                        }
                        else
                        {
                            cb.Checked = !cb.Checked;
                        }
                    }
                    else
                    {
                        if (await AcceptControlRequest(false))
                        {
                            Debug.WriteLine("修改状态成功");
                        }
                        else
                        {
                            cb.Checked = !cb.Checked;
                        }
                    }
                }

                programConfig.AcceptControlRequest = cb.Checked;
            }
        }

        /// <summary>
        /// 启动按钮单击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void startButton_Click(object sender, EventArgs e)
        {
            Start();
        }

        /// <summary>
        /// 停止按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void stopButton_Click(object sender, EventArgs e)
        {
            Stop();
        }

        /// <summary>
        /// 退出按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void quitButton_Click(object sender, EventArgs e)
        {
            Quit();
        }

        /// <summary>
        /// 更新客户端列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void upddateRemteMachineListButton_Click(object sender, EventArgs e)
        {
            UpdateRemoteMachineList();
        }

        /// <summary>
        /// 控制按钮单击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void controlButton_Click(object sender, EventArgs e)
        {
            var bt = sender as Button;
            if (bt != null)
            {
                if (bt.Text == "连接")
                {
                    if (remoteClientListBox.SelectedIndex == -1)
                    {
                        return;
                    }

                    var remoteId = onlineClientList[remoteClientListBox.SelectedIndex];
                    client.PeeredId = remoteId;

                    ShowStatusStrip(string.Format("正在向远程客户端发送({0})控制请求，请稍等...", remoteId));

                    if (await PrepareToControlRemoteClientAsync(remoteId))
                    {
                        // 获取隧道端口
                        int tunnelPort = await MainFom.Protocol.GetTunnelPortAsync(proxyServer.Hostname, programConfig.ProxyServerListenPort, remoteId, false);
                        if (tunnelPort != -1)
                        {
                            Debug.WriteLine("获取到远程客户端的隧道：" + tunnelPort.ToString());
                            bt.Text = "断开";
                            remoteClientListBox.Enabled = false;
                            ShowStatusStrip(string.Format("与远程客户端({0})已经建立连接。地址：{1}:{2}", remoteId, proxyServer.Hostname, tunnelPort));
                            //MessageBox.Show("请打开远程控制桌面程序，输入: " + string.Format("{0}:{1}", proxyServer.Hostname, tunnelPort));
                            OpenRDPProgram(proxyServer.Hostname, tunnelPort);
                        }  
                    }
                    else
                    {
                        HideStatusStrip();
                    }
                }
                else
                {
                    if (await DisconnectToRemoteClient(client.PeeredId))
                    {
                        Debug.WriteLine("断开连接");
                        bt.Text = "连接";
                        remoteClientListBox.Enabled = true;
                    }
                }
            }
        }

        /// <summary>
        /// 客户端信息属性变化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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


        /// <summary>
        /// 通知图标收到鼠标左键单击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void notifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ControlWindowVisibilityToolStripMenuItem_Click(null, null);
            }
        }

        /// <summary>
        /// 控制窗口隐藏、显示的菜单项
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ControlWindowVisibilityToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Visible = !this.Visible;
            if (this.Visible)
            {
                ControlWindowVisibilityToolStripMenuItem.Text = "隐藏窗口";
            }
            else
            {
                ControlWindowVisibilityToolStripMenuItem.Text = "显示窗口";
            }
        }

        /// <summary>
        /// 控制启动服务的菜单项
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Start();
        }

        /// <summary>
        /// 控制停止服务的菜单项
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Stop();
        }

        /// <summary>
        /// 控制退出的菜单项
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Quit();
        }

        /// <summary>
        /// 窗体显示的时候触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_Shown(object sender, EventArgs e)
        {
            // 加载配置
            LoadConfig();
        }

        /// <summary>
        /// 主窗口关闭时的处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 检查是否真的要退出
            if (canQuit)
            {
                // 保存当前的配置信息
                programConfig.Client = client;
                FileOperation.SaveConfig(programConfig);

                // 停止定时器
                autoRestartServiceTimer.Enabled = false;
                autoRestartServiceTimer.Dispose();
            }
            else
            {
                // 检查要不要阻止退出而只是隐藏
                if (closeWithoutQuitCheckBox.Checked)
                {
                    e.Cancel = true;
                    ControlWindowVisibilityToolStripMenuItem_Click(null, null);
                }
                else
                {
                    Stop();
                    // 保存当前的配置信息
                    programConfig.Client = client;
                    FileOperation.SaveConfig(programConfig);

                    // 停止定时器
                    autoRestartServiceTimer.Enabled = false;
                    autoRestartServiceTimer.Dispose();
                }
            }
        }

        #endregion

        #region 核心函数
        /// <summary>
        /// 编辑当前客户端信息
        /// </summary>
        private void EditClient()
        {
            // 将编辑窗口改为可写状态
            machineNameTextBox.ReadOnly = false;
            machineDescriptionTextBox.ReadOnly = false;
            RDPPortTextBox.ReadOnly = false;
        }

        /// <summary>
        /// 保存当前客户端信息
        /// </summary>
        private void SaveClient()
        {
            // 重置为只读状态
            machineNameTextBox.ReadOnly = true;
            machineDescriptionTextBox.ReadOnly = true;
            RDPPortTextBox.ReadOnly = true;

            // 保存所有信息
            client.Name = machineNameTextBox.Text;
            client.Description = machineDescriptionTextBox.Text;
            client.RDPPort = int.Parse(RDPPortTextBox.Text);

            // 告诉配置信息类
            programConfig.Client = client;
        }

        /// <summary>
        /// 编辑代理服务器
        /// </summary>
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
                    if (list[i].Name == proxyServer.Name)
                    {
                        proxyServer = list[i];
                        ShowProxyServerInfo(proxyServer);

                        // 同时更新配置
                        programConfig.SelectedServerIndex = i;
                        return;
                    }
                }

                // 不存在的话，那么就尝试使用第一个
                proxyServer = list[0];
                ShowProxyServerInfo(proxyServer);
                // 同时更新配置
                programConfig.SelectedServerIndex = 0;
            }
            else
            {
                proxyServer = new ProxyServer.ProxyServer();
                ShowProxyServerInfo(proxyServer);
                programConfig.SelectedServerIndex = -1;
            }
        }

        /// <summary>
        /// 更换代理服务器
        /// </summary>
        private void ChangeServer()
        {
            ChangeServerForm form = new ChangeServerForm();
            if (form.ShowDialog() == DialogResult.Yes)
            {
                var obj = form.SelectedServer;
                if (obj != null)
                {
                    proxyServer = obj;
                    ShowProxyServerInfo(proxyServer);
                    // 更新配置信息
                    programConfig.SelectedServerIndex = form.SelectedServerIndex;
                }
            }
        }

        /// <summary>
        /// 显示代理服务器信息
        /// </summary>
        /// <param name="server"></param>
        private void ShowProxyServerInfo(ProxyServer.ProxyServer server)
        {
            // 显示代理服务器信息
            serverNameTextBox.Text = server.Name;
            serverIPTextBox.Text = string.Format("{0}:{1}", server.Hostname, server.LoginPort);
        }

        /// <summary>
        /// 显示客户端信息
        /// </summary>
        /// <param name="machine"></param>
        private void ShowClientInfo(Client.Client machine)
        {
            machineNameTextBox.Text = machine.Name;
            machineIDTextBox.Text = machine.ID.ToString();
            machineDescriptionTextBox.Text = machine.Description;
            RDPPortTextBox.Text = machine.RDPPort.ToString();
        }

        /// <summary>
        /// 显示关于窗口
        /// </summary>
        private void ShowAboutDialog()
        {
            (new AboutForm()).ShowDialog();
        }

        /// <summary>
        /// 自启动设置
        /// </summary>
        /// <param name="enable"></param>
        private void SetAutoStartup(bool enable)
        {
            programConfig.AutoStartup = enable;
        }

        /// <summary>
        /// 关闭后依然在后台运行
        /// </summary>
        /// <param name="enable"></param>
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
        /// 客户端启动相关服务
        /// </summary>
        private async void Start(bool enableInteractive = true)
        {
            //
            tryRestart = false;

            // 更新按钮的状态
            startButton.Enabled = false;
            stopButton.Enabled = true;
            updateOnlineListButton.Enabled = true;
            controlButton.Enabled = true;

            // 更新菜单的状态
            StartToolStripMenuItem.Enabled = false;
            StopToolStripMenuItem.Enabled = true;

            ShowStatusStrip("正在启动服务，请稍等...");

            // 关闭可能不小心打开的隧道进程
            CloseAllSSHReverseTunnels();

            // 根据协议要求，首先获取一个id，标记在线注册
            client.ID = await MainFom.Protocol.GetClientIdAsync(proxyServer.Hostname, programConfig.ProxyServerListenPort);

            if (client.ID != -1)
            {
                Debug.WriteLine("成功获取到id: " + client.ID.ToString());

                SetServerConnectionStatus(true);
                // 更新显示
                ShowClientInfo(client);

                // 上传本机信息
                if (await MainFom.Protocol.PostClientInformationAsync(proxyServer.Hostname, programConfig.ProxyServerListenPort, client))
                {
                    Debug.WriteLine("成功上传本机信息到代理服务器中");

                    await AcceptControlRequest(programConfig.AcceptControlRequest);

                    // 创建两个线程分别负责检查在线状态和检查请求状态信息
                    keepAliveThread = new Thread(this.KeepAliveThread);
                    queryStatusThread = new Thread(this.QueryStatusThread);

                    // 检查计时器有没有启动
                    if (autoRestartServiceTimer.Enabled == false)
                    {
                        autoRestartServiceTimer.Interval = 30 * 1000;
                        autoRestartServiceTimer.Enabled = true;
                        autoRestartServiceTimer.Start();
                        autoRestartServiceTimer.Tick += AutoRestartServiceTimer_Tick;
                    }

                    cts = new CancellationTokenSource();

                    // 启动线程
                    Debug.WriteLine("启动保持在线状态的线程");
                    keepAliveThread.Start();

                    Debug.WriteLine("启动查询本机状态请求的线程");
                    queryStatusThread.Start();

                    // 自动更新在线的客户端列表
                    UpdateRemoteMachineList();
                }
                else
                {
                    Debug.WriteLine("没能上传本机信息到代理服务器中");
                }

                // 此后，隐藏主窗口
                if (autoStartCheckBox.Checked)
                {
                    this.Close();
                }
            }
            else
            {
                Debug.WriteLine("获取id失败");
                // 如果是监视线程尝试自动启动服务，就不需要对话框提示
                if (enableInteractive)
                {
                    MessageBox.Show(string.Format("启动服务失败，请确保网络状态正常，并且代理服务器: {0} 工作正常后再次尝试！", proxyServer.Name), "警告消息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                SetServerConnectionStatus(false);
                Stop();
                tryRestart = true;
            }
            HideStatusStrip();
        }

        /// <summary>
        /// 客户端停止相关服务
        /// </summary>
        private void Stop()
        {
            // 停止两个特殊线程
            if (cts != null)
            {
                cts.Cancel();
            }

            SetServerConnectionStatus(false);

            CloseAllSSHReverseTunnels();

            // 更新控件状态
            startButton.Enabled = true;
            stopButton.Enabled = false;
            updateOnlineListButton.Enabled = false;
            controlButton.Enabled = false;

            // 更新菜单的状态
            StartToolStripMenuItem.Enabled = true;
            StopToolStripMenuItem.Enabled = false;
        }

        /// <summary>
        /// 客户端退出，并停止所有服务
        /// </summary>
        private void Quit()
        {
            Stop();
            canQuit = true;
            this.Close();
        }

        /// <summary>
        /// 更新在线用户列表
        /// </summary>
        private async void UpdateRemoteMachineList()
        {
            // 向服务器请求获取列表
            var dict = await MainFom.Protocol.GetOnlineClientListAsync(proxyServer.Hostname, programConfig.ProxyServerListenPort, client.ID);

            if (dict != null)
            {
                this.Invoke(new Action(() =>
                {
                    // 看看有没有被选中的项目，如果没有的话，那就什么都不用管；否则要对选中的项目作记录
                    int id = (remoteClientListBox.SelectedIndex != -1) ? onlineClientList[remoteClientListBox.SelectedIndex] : -1;

                    remoteClientListBox.Items.Clear();
                    onlineClientList.Clear();
                    for (int i = 0; i < dict.Count; i++)
                    {
                        remoteClientListBox.Items.Add(string.Format("{0}. {1}    {2}",
                            i + 1, dict.ElementAt(i).Key, dict.ElementAt(i).Value));
                        onlineClientList.Add(dict.ElementAt(i).Key);
                    }

                    // 查看之前选中的项目，如果有，还要继续保持选中
                    for (int i = 0; i < onlineClientList.Count; i++)
                    {
                        if (onlineClientList[i] == id)
                        {
                            remoteClientListBox.SelectedIndex = i;
                            return;
                        }
                    }
                })); 
            }
        }

        /// <summary>
        /// 关闭所有正在启动的plink进程，停用隧道。
        /// </summary>
        private void CloseAllSSHReverseTunnels()
        {
            //foreach (var tunnel in tunnelList)
            //{
            //    tunnel.Stop();
            //}

            Debug.WriteLine("关闭所有隧道连接");
            foreach (var item in tunnelList)
            {
                item.Stop();
            }
        }

        #endregion

        #region 处理远程控制连接和断开等相关函数
        private SSHReverseTunnel.SSHReverseTunnel tunnel = null;

        /// <summary>
        /// 被控端会要求自己准备建立连接被控制
        /// </summary>
        /// <returns></returns>
        private async Task<bool> PrepareToBeUnderControlAsync()
        {
            // 存在控制请求后，会选择开始进行隧道建立
            // 首先，获得一个准许的隧道端口号
            var tunnelPort = await MainFom.Protocol.GetTunnelPortAsync(proxyServer.Hostname, programConfig.ProxyServerListenPort, client.ID, true);

            CloseAllSSHReverseTunnels();

            // 确保端口号ok
            if (tunnelPort != -1)
            {
                // 启动本地隧道进程，尝试和远程代理服务器建立隧道
                tunnel = new SSHReverseTunnel.SSHReverseTunnel(client, proxyServer, tunnelPort);
                if (tunnel.Start())
                {
                    // 实验表明，此处必须要延时等待一下，否则可能建立隧道会失败
                    Thread.Sleep(1000);
                    tunnelList.Add(tunnel);

                    // 最多尝试查询次数
                    int tryCount = 5;
                    bool status = false;
                    while (tryCount != 0)
                    {
                        // 隧道进程成功启动后，我们需要向远程服务器查询有没有成功建立隧道端口
                        status = await MainFom.Protocol.GetTunnelStatusAsync(proxyServer.Hostname, programConfig.ProxyServerListenPort, client.ID);

                        if (status)
                        {
                            Debug.WriteLine("隧道成功建立，端口号为：" + tunnelPort.ToString());
                        }
                        Thread.Sleep(500);
                        tryCount--;
                    }

                    if (status)
                    {
                        // 隧道成功建立，此时应当清除掉控制请求
                        if (await MainFom.Protocol.PostControlRequestAsync(proxyServer.Hostname, programConfig.ProxyServerListenPort, client.ID, false))
                        {
                            Debug.WriteLine("远程控制请求已经得到处理");
                            if (await MainFom.Protocol.PostIsUnderControlAsync(proxyServer.Hostname, programConfig.ProxyServerListenPort, client.ID, true))
                            {
                                Debug.WriteLine("已经标记为正在被控制状态");
                                return true;
                            }
                            else
                            {
                                CloseAllSSHReverseTunnels();
                                return false;
                            }
                        }
                        else
                        {
                            CloseAllSSHReverseTunnels();
                            return false;
                        }
                    }
                    else
                    {
                        CloseAllSSHReverseTunnels();
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 请求准备与远程客户端建立连接
        /// </summary>
        /// <param name="remoteId"></param>
        /// <returns></returns>
        private async Task<bool> PrepareToControlRemoteClientAsync(int remoteId)
        {
            bool oldState = programConfig.AcceptControlRequest;
            programConfig.AcceptControlRequest = false;
            // 看看对方是否允许被控制
            if (await MainFom.Protocol.GetPeeredRemoteIdAsync(proxyServer.Hostname, programConfig.ProxyServerListenPort, remoteId) == remoteId)
            {
                MessageBox.Show(string.Format("客户端({0})拒绝接收控制请求，请确保对方允许被控制！", remoteId), "警告信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                programConfig.AcceptControlRequest = oldState;
                return false;
            }

            // 要检查远程客户端是否正在被控制中
            if (await MainFom.Protocol.GetIsUnderControlAsync(proxyServer.Hostname, programConfig.ProxyServerListenPort, remoteId))
            {
                MessageBox.Show(string.Format("客户端(id: {0})正在被其他客户端远程控制中，拒绝请求！", remoteId), "错误提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                programConfig.AcceptControlRequest = oldState;
                return false;
            }
            else
            {
                // 把需要控制的客户端的控制请求设置为true
                if (await MainFom.Protocol.PostControlRequestAsync(proxyServer.Hostname, programConfig.ProxyServerListenPort, remoteId, true))
                {
                    // 接下来，把自身的id告诉远程目标客户端
                    if (await MainFom.Protocol.PostPeeredRemoteIdAsync(proxyServer.Hostname, programConfig.ProxyServerListenPort, remoteId, client.ID))
                    {
                        Debug.WriteLine("设置目标客户端配对id成功");
                        // 等待对方响应请求
                        int tryCount = 60;
                        while (true)
                        {
                            if (await MainFom.Protocol.GetIsUnderControlAsync(proxyServer.Hostname, programConfig.ProxyServerListenPort, remoteId))
                            {
                                Debug.WriteLine("远程客户端的隧道已经成功建立，可以被远程访问。");
                                programConfig.AcceptControlRequest = oldState;
                                return true;
                            }
                            else
                            {
                                tryCount--;
                                if (tryCount == 0)
                                {
                                    MessageBox.Show(string.Format("申请远程控制客户端(id: {0})失败，请确保对方网络正常！", remoteId), "错误提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    programConfig.AcceptControlRequest = oldState;
                                    return false;
                                }
                            }
                            Thread.Sleep(1000);
                        }
                    }
                    else
                    {
                        programConfig.AcceptControlRequest = oldState;
                        return false;
                    }
                }
                else
                {
                    // 可能是网络断开等原因，没有成功邀请
                    MessageBox.Show(string.Format("远程客户端(id: {0})没有成功收到邀请，请确保双方网络正常！", remoteId), "错误提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    programConfig.AcceptControlRequest = oldState;
                    return false;
                }
            }
        }

        /// <summary>
        /// 关闭与远程客户端的配对
        /// </summary>
        /// <param name="remoteId"></param>
        /// <returns></returns>
        private async Task<bool> DisconnectToRemoteClient(int remoteId)
        {
            if (remoteId != -1)
            {
                // 要判断对方是否下线，不要无谓地发送取消消息
                if (await MainFom.Protocol.GetIsOnlineAsync(proxyServer.Hostname, programConfig.ProxyServerListenPort, remoteId))
                {
                    // 直接向对方发送释放控制请求即可
                    ShowStatusStrip(string.Format("正在向客户端({0})发送断开连接请求...", remoteId));
                    if (await MainFom.Protocol.PostIsUnderControlAsync(proxyServer.Hostname, programConfig.ProxyServerListenPort, remoteId, false))
                    {
                        Debug.WriteLine("已经发送了断开请求，等待对方断开连接。");
                        MessageBox.Show(string.Format("已经断开与远程客户端({0})的连接!", client.PeeredId), "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        client.PeeredId = -1;
                        HideStatusStrip();
                        return true;
                    }
                    else
                    {
                        HideStatusStrip();
                        return false;
                    }
                }
                else
                {
                    Debug.WriteLine("对方已经离线，无需发送断开连接消息");
                    HideStatusStrip();
                    return true;
                }
            }
            else
            {
                HideStatusStrip();
                return false;
            }
        }

        /// <summary>
        /// 接收远程控制与否
        /// </summary>
        /// <param name="flag"></param>
        /// <returns></returns>
        private async Task<bool> AcceptControlRequest(bool flag)
        {
            // 检查是不是已经被控制了，如果被控制了的话，不可修改
            if (!flag)
            {
                if (await MainFom.Protocol.GetIsUnderControlAsync(proxyServer.Hostname, programConfig.ProxyServerListenPort, client.ID))
                {
                    MessageBox.Show("当前客户端正在被控制中，无法修改此选项！请在断开连接后操作！", "警告信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
                else
                {
                    if (await MainFom.Protocol.PostPeeredRemoteIdAsync(proxyServer.Hostname, programConfig.ProxyServerListenPort, client.ID, client.ID))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (await MainFom.Protocol.PostPeeredRemoteIdAsync(proxyServer.Hostname, programConfig.ProxyServerListenPort, client.ID, -1))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        #endregion

        #region 后台轮询和保持在线状态线程
        /// <summary>
        /// 该后台线程处理函数会定时查询客户端有没有控制请求等状态，供客户端使用
        /// </summary>
        /// <param name=""></param>
        private async void QueryStatusThread()
        {
            while (true)
            {
                if (cts.Token.IsCancellationRequested)
                {
                    Debug.WriteLine("关闭线程：QueryStatusThread " + queryStatusThread.ManagedThreadId);
                    HideStatusStrip();
                    break;
                }

                if (programConfig.AcceptControlRequest)
                {
                    bool flag = true;
                    // 查询有没有正在被控制
                    // 如果客户端一直被控制中，将会被锁定，而无法被其他客户端连接
                    if (await MainFom.Protocol.GetIsUnderControlAsync(proxyServer.Hostname, programConfig.ProxyServerListenPort, client.ID))
                    {
                        // 查询隧道状态
                        if (await MainFom.Protocol.GetTunnelStatusAsync(proxyServer.Hostname, programConfig.ProxyServerListenPort, client.ID))
                        {
                            Debug.WriteLine("隧道状态正常，本机正在被控制中");
                            ShowStatusStrip(string.Format("本机与远程客户端({0})建立联系，可被远程登录...", client.PeeredId));
                        }
                        else
                        {
                            // 重新启动隧道
                            CloseAllSSHReverseTunnels();
                            if (tunnel.Start())
                            {
                                tunnelList.Add(tunnel);
                            }

                            HideStatusStrip();
                        }

                        // 查询配对的客户端是否在线
                        if (await MainFom.Protocol.GetIsOnlineAsync(proxyServer.Hostname, programConfig.ProxyServerListenPort, client.PeeredId))
                        {
                            Debug.WriteLine(string.Format("远程客户端{0}依然在线", client.PeeredId));
                        }
                        else
                        {
                            // 解除锁定，从而保证本机可以被其他客户端控制
                            if (await MainFom.Protocol.PostIsUnderControlAsync(proxyServer.Hostname, programConfig.ProxyServerListenPort, client.ID, false))
                            {
                                HideStatusStrip();
                                Debug.WriteLine("解除本机锁定成功，可以接收其他客户端的请求");
                            }
                        }
                    }
                    else
                    {
                        // 查询有没有控制请求
                        if (await MainFom.Protocol.GetControlRequestAsync(proxyServer.Hostname, programConfig.ProxyServerListenPort, client.ID))
                        {
                            // 获取匹配的客户端id
                            client.PeeredId = await MainFom.Protocol.GetPeeredRemoteIdAsync(proxyServer.Hostname, programConfig.ProxyServerListenPort, client.ID);

                            if (client.PeeredId != -1)
                            {
                                Debug.WriteLine("请求远程控制的客户端id: " + client.PeeredId.ToString());
                                ShowStatusStrip(string.Format("收到来自客户端({0})的远程控制请求，正在处理...", client.PeeredId));
                                // 准备连接
                                if (await PrepareToBeUnderControlAsync())
                                {
                                    Debug.WriteLine("准备建立连接工作已经完毕，等待对方远程登录。");
                                    ShowStatusStrip(string.Format("来自远程客户端({0})的控制请求已经处理完毕，对方可以正常远程登录", client.PeeredId));
                                    flag = false;
                                }
                                else
                                {
                                    Debug.WriteLine("准备建立连接工作失败！");
                                    HideStatusStrip();
                                }
                            }
                            else
                            {
                                Debug.WriteLine("获取远程配对客户端id失败");
                                HideStatusStrip();
                            }
                        }

                        if (flag)
                        {
                            HideStatusStrip();
                            CloseAllSSHReverseTunnels();
                        }
                    }
                }
                //
                // 每隔一段时间查询一次状态
                //
                Thread.Sleep(programConfig.QueryStatusInterval * 1000);
            }
        }

        /// <summary>
        /// 该线程会定时更新服务器上的keep_alive_count值，防止因为被服务器递减为零后，认为超时并下线本机
        /// </summary>
        private async void KeepAliveThread()
        {
            while (true)
            {
                if (cts.Token.IsCancellationRequested)
                {
                    Debug.WriteLine("关闭线程：KeepAliveThread " + keepAliveThread.ManagedThreadId);
                    break;
                }

                if (client.ID != -1)
                {
                    if (await MainFom.Protocol.PostKeepAliveCountAsync(proxyServer.Hostname, programConfig.ProxyServerListenPort, client.ID, 10))
                    {
                        UpdateRemoteMachineList();
                    }
                    else
                    {
                        // 表示连接失败，无法和服务器建立连接
                        this.Invoke(new Action(() =>
                        {
                            SetServerConnectionStatus(false);
                            ShowStatusStrip("与代理服务器的连接已经断开，程序将会在一段时间后尝试重新启动服务...");
                            Stop();
                            tryRestart = true;
                        }));
                    }
                }
                // 每1s更新一次
                // 服务器会在一段时间后收不到新值，自动判断为下线
                Thread.Sleep(programConfig.KeepAliveInterval * 1000);
            }
        }

        private bool tryRestart = false;
        /// <summary>
        /// 检查是否需要重启服务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AutoRestartServiceTimer_Tick(object sender, EventArgs e)
        {
            if (tryRestart)
            {
                this.Invoke(new Action(() =>
                {
                    Start(false);
                }));
            }
        }

        /// <summary>
        /// 专门用来显示进度的线程
        /// </summary>
        private void ShowProgressFormThread(object obj)
        {
            Dictionary<string, string> dict = (Dictionary<string, string>)obj;

            if (dict != null)
            {
                var form = new ProgressForm(dict["title"], dict["content"]);
                form.ShowDialog();
            }
        }
        #endregion

        #region 其他函数
        private void LoadConfig()
        {
            // 读取默认的配置信息
            programConfig = FileOperation.ReadConfig();

            if (programConfig != null)
            {
                if (programConfig.Client != null)
                {
                    client = programConfig.Client;
                }
                else
                {
                    client = new Client.Client();
                    programConfig.Client = client;
                }
            }
            else
            {
                // 做一些初始化的工作
                client = new Client.Client();
                proxyServer = new ProxyServer.ProxyServer();
                programConfig = new Config.Config(autoStartCheckBox.Checked,
                    closeWithoutQuitCheckBox.Checked, client, -1, 9000);
            }
            // 显示本机的有关信息
            ShowClientInfo(programConfig.Client);

            // checkbox状态
            autoStartCheckBox.Checked = programConfig.AutoStartup;
            closeWithoutQuitCheckBox.Checked = programConfig.EnableBackgroundMode;
            acceptControlRequestCheckBox.Checked = programConfig.AcceptControlRequest;

            // 显示代理服务器信息
            if (programConfig.SelectedServerIndex != -1)
            {
                var serverList = FileOperation.ReadServerList();
                if (serverList.Count > 0)
                {
                    if (programConfig.SelectedServerIndex < serverList.Count)
                    {
                        proxyServer = serverList[programConfig.SelectedServerIndex];
                    }
                    else
                    {
                        proxyServer = serverList[0];
                    }
                    ShowProxyServerInfo(proxyServer);
                }

                // 检查是否需要自动启动
                if (autoStartCheckBox.Checked)
                {
                    AutoStartService();
                }
            }
            else
            {
                // 打开编辑窗口，提示用户添加服务器
                MessageBox.Show("没有可以选择的代理服务器，请自行添加代理服务器后更换。", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                EditServer();
                ChangeServer();
            }
        }

        /// <summary>
        /// 自动启动服务
        /// </summary>
        private void AutoStartService()
        {
            Start();
        }

        /// <summary>
        /// 设置状态条显示
        /// </summary>
        /// <param name="content">显示的文字内容</param>
        /// <param name="isProgressBarVisible">是否需要显示进度条</param>
        private void ShowStatusStrip(string content)
        {
            toolStripStatusLabel.Text = content;
        }

        /// <summary>
        /// 隐藏状态栏
        /// </summary>
        private void HideStatusStrip()
        {
            try
            {
                toolStripStatusLabel.Text = "";
            }
            catch { }
        }

        /// <summary>
        /// 自动打开远程桌面应用
        /// 使用帮助：
        /// /v 指定要连接的远程客户端
        /// /span 将远程桌面的高度和宽度和本地客户端桌面进行匹配
        /// /control 运行控制会话
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        private void OpenRDPProgram(string host, int port)
        {
            Process proc = new Process();
            proc.StartInfo.FileName = "mstsc";
            proc.StartInfo.Arguments = string.Format("/v {0}:{1} /span", host, port);
            proc.StartInfo.CreateNoWindow = true;

            bool oldState = closeWithoutQuitCheckBox.Checked;

            closeWithoutQuitCheckBox.Checked = true;
            // 启动前隐藏主窗口，等待关闭后再弹出主窗口，防止误操作！
            ControlWindowVisibilityToolStripMenuItem_Click(null, null);

            proc.Start();
            proc.WaitForExit();

            // 自动断开连接，不再需要手动断开连接
            controlButton_Click(controlButton, null);
            ControlWindowVisibilityToolStripMenuItem_Click(null, null);
            closeWithoutQuitCheckBox.Checked = oldState;
        }

        #endregion

    }
}
