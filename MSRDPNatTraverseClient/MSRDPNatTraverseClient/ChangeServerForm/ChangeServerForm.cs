using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MSRDPNatTraverseClient
{
    public partial class ChangeServerForm : Form
    {
        private List<ProxyServer.ProxyServer> serverList = null;
        public ChangeServerForm()
        {
            InitializeComponent();

            // 添加服务器列表
            serverList = Utility.FileOperation.ReadServerList();

            foreach (var item in serverList)
            {
                serverComboBox.Items.Add(string.Format("{0}({1})", item.Name, item.Hostname));
            }

            if (serverList.Count > 0)
            {
                serverComboBox.SelectedIndex = 0;
            }
        }

        private ProxyServer.ProxyServer server = null;

        public ProxyServer.ProxyServer SelectedServer
        {
            get
            {
                return server;
            }
        }

        public int SelectedServerIndex { get; internal set; }

        private void confirmButton_Click(object sender, EventArgs e)
        {
            // 转换成ProxyServer对象类型 方便返回到主窗口
            int index = GetServerIndex(serverList, 
                serverComboBox.SelectedItem.ToString().Split(new char[] { '(' }, 
                StringSplitOptions.RemoveEmptyEntries)[0]);

            if (index != -1)
            {
                this.server = serverList[index];
                SelectedServerIndex = index;
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private int GetServerIndex(List<ProxyServer.ProxyServer> list, string serverName)
        {
            // 检查是否有重复的名称即可
            for (int i = 0; i < list.Count; i++)
            {
                if (serverName == list[i].Name)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
