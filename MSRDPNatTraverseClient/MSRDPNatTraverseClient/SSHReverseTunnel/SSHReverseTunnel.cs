using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace MSRDPNatTraverseClient.SSHReverseTunnel
{
    /// <summary>
    /// 只是和SSH隧道建立有关的代码整合到此处，不保证隧道一定建立成功。
    /// </summary>
    public class SSHReverseTunnel
    {
        #region 私有字段
        // 这个ID是在一台主机上唯一标识该Tunnel对象的值
        // 该值在没有相关进程启动时为-1；否则为进程的id
        private int _id = -1;

        // 标志tunnel对应的进程是否启动了
        private bool _isTunnelProcessRunning = false;

        // 指定plink所在的路径
        private readonly string PLINK_PROGRAM_PATH = @"util\plink.exe";

        private ProxyServer.ProxyServer _server = null;
        private LocalMachine.LocalMachine _machine = null;

        // 
        private int _tunnelPort = 10000;

        //
        private Process _tunnelProcess = null;
        #endregion

        #region 属性
        public int ID
        {
            get { return _id; }
        }

        public bool IsTunnelProcessRunning
        {
            get { return _isTunnelProcessRunning;  }
        }
        #endregion

        #region 公开的函数接口
        public SSHReverseTunnel(LocalMachine.LocalMachine machine, ProxyServer.ProxyServer server, int tunnelPort)
        {
            _server = server;
            _machine = machine;
            _tunnelPort = tunnelPort;
        }

        public bool Start()
        {
            _tunnelProcess = StartPlinkProcess();
            if (_tunnelProcess != null)
            {
                _id = _tunnelProcess.Id;
                _isTunnelProcessRunning = true;
            }
            else
            {
                _id = -1;
                _isTunnelProcessRunning = false;
            }
            return _isTunnelProcessRunning;
        }

        public bool Stop()
        {
            if (_tunnelProcess != null)
            {
                try
                {
                    _tunnelProcess.Kill();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region 私有的函数接口
        private Process StartPlinkProcess()
        {
            if (ValidatePlinkPath())
            {
                // 命令执行部分
                var cmdProcess = new System.Diagnostics.Process();

                // 相关设置
                cmdProcess.StartInfo.FileName = "cmd.exe";
                cmdProcess.StartInfo.UseShellExecute = false;       // 是否使用操作系统Shell启动
                cmdProcess.StartInfo.RedirectStandardError = true;  // 重定向标准错误
                cmdProcess.StartInfo.RedirectStandardInput = true;  // 接收来自调用程序的输入信息
                cmdProcess.StartInfo.RedirectStandardOutput = true; // 获取输出信息
                cmdProcess.StartInfo.CreateNoWindow = true;         // 不需要窗口显示

                // 启动程序
                cmdProcess.Start();

                // 发送要执行的命令
                string cmdStr = string.Format(@"{0} -pw {1} -P {2} -N -R {3}:{4}:{5}:{6} {7}@{8} &exit",
                    PLINK_PROGRAM_PATH,
                    _server.LoginPassword,
                    _server.LoginPort,
                    "0.0.0.0",
                    _tunnelPort,
                    "127.0.0.1",
                    _machine.RDPPort,
                    _server.LoginName,
                    _server.IPAdress);

                cmdProcess.StandardInput.WriteLine(cmdStr);
                // 延时等待进程启动
                System.Threading.Thread.Sleep(100);

                cmdProcess.StandardInput.WriteLine("y");

                cmdProcess.CloseMainWindow();
                // 返回启动后的plink进程
                return GetLastStartedProcessByName("plink");
            }
            else
            {
                throw new Exception("找不到路径：" + PLINK_PROGRAM_PATH);
            }
        }

        private Process GetLastStartedProcessByName(string name)
        {
            var processesOrderByPid = (from process in System.Diagnostics.Process.GetProcessesByName(name)
                                       orderby process.StartTime
                                       select process).ToList<System.Diagnostics.Process>();

            return (processesOrderByPid.Count > 0 ? processesOrderByPid[processesOrderByPid.Count - 1] : null);
        }

        private bool ValidatePlinkPath()
        {
            return System.IO.File.Exists(PLINK_PROGRAM_PATH);
        }
        #endregion
    }
}
