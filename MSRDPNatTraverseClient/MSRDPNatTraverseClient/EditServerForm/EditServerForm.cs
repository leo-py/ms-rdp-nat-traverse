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
    public partial class EditServerForm : Form
    {
        private List<ProxyServer.ProxyServer> serverList = new List<ProxyServer.ProxyServer>();

        public EditServerForm()
        {
            InitializeComponent();

            // 把磁盘已有的列表读取出来
            serverList = Utility.FileOperation.ReadServerList();

            if (serverList.Count > 0)
            {
                // 添加到列表中显示出来
                UpdateServerListBox(serverList);
                serversListBox.SelectedIndex = 0;
            }
        }

        private void TextBox_Click(object sender, EventArgs e)
        {
            var tb = sender as TextBox;
            if (tb != null)
            {
            }
        }

        private void addServerButton_Click(object sender, EventArgs e)
        {
            AddServer();
        }

        private void removeServerButton_Click(object sender, EventArgs e)
        {
            RemoveServer();
        }

        private void serversListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListBox lb = sender as ListBox;
            if (lb != null)
            {
                // 根据当前的选中的索引值，可以在匹配的serverList中找到对应的信息
                if (lb.SelectedIndex != -1)
                {
                    ShowServerInformation(serverList[lb.SelectedIndex]);
                    old_selected_index = lb.SelectedIndex;
                    groupBoxInfo.Enabled = true;
                }
            }
        }

        private void AddServer()
        {
            // 创建一个新的新的ProxyServer对象
            ProxyServer.ProxyServer server = new ProxyServer.ProxyServer();
            server.Name = "新建服务器";

            serverList.Add(server);

            // 修改后记得刷新列表显示
            UpdateServerListBox(serverList);

            //
            serversListBox.SelectedIndex = serversListBox.Items.Count - 1;
        }

        private bool HasSameServerName(string serverName)
        {
            foreach (var server in serverList)
            {
                if (serverName == server.Name)
                {
                    return true;
                }
            }

            return false;
        }

        private int GetPort(string text)
        {
            try
            {
                return int.Parse(text);
            }
            catch
            {
                return 22;
            }
        }

        private void RemoveServer()
        {
            // 保证必须选中才可以删除
            if (serversListBox.SelectedIndex >= 0)
            {
                serverList.RemoveAt(serversListBox.SelectedIndex);

                // 然后更新列表显示
                UpdateServerListBox(serverList);

                // 同时选中最后一项
                serversListBox.SelectedIndex = serverList.Count > 0 ? serverList.Count - 1 : -1;
                groupBoxInfo.Enabled = serverList.Count > 0 ? true : false;
            }
        }

        private void UpdateServerListBox(List<ProxyServer.ProxyServer> list)
        {
            serversListBox.Items.Clear();
            foreach (var server in list)
            {
                serversListBox.Items.Add(string.Format("{0}({1})", server.Name, server.Hostname));
            }
        }

        private void ShowServerInformation(ProxyServer.ProxyServer server)
        {
            // 显示服务器信息
            serverNameTextBox.Text = server.Name;
            serverPasswordTextBox.Text = server.LoginPassword;
            serverDescriptionTextBox.Text = server.Description;
            serverUserNameTextBox.Text = server.LoginName;
            portTextBox.Text = server.LoginPort.ToString();
            serverIPTextBox.Text = server.Hostname;
        }

        private void EditServerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 此处在关闭窗口时，要把修改后的服务器列表存储到磁盘中
            Utility.FileOperation.SaveServerList(serverList);
        }

        private int old_selected_index = -1;
        private void serverInfoTextBox_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb != null)
            {
                // 确保现在有列表项目被选中，从而判定为对项目的修改。
                if (serversListBox.SelectedIndex == -1 && serversListBox.SelectedIndex != old_selected_index)
                {
                    return;
                }

                if (string.IsNullOrEmpty(tb.Text))
                {
                    return;
                }

                var index = serversListBox.SelectedIndex;
                switch (tb.Tag.ToString())
                {
                    case "name":
                        serverList[index].Name = tb.Text;
                        break;
                    case "ip":
                        serverList[index].Hostname = tb.Text;
                        break;
                    case "port":
                        serverList[index].LoginPort = GetPort(tb.Text);
                        break;
                    case "loginName":
                        serverList[index].LoginName = tb.Text;
                        break;
                    case "password":
                        serverList[index].LoginPassword = tb.Text;
                        break;
                    case "desc":
                        serverList[index].Description = tb.Text;
                        break;
                    default:
                        break;
                }

                // 最后为了确保万一，还是需要更新列表显示
                UpdateServerListBox(serverList);
                serversListBox.SelectedIndex = index;
            }
        }
    }
}
