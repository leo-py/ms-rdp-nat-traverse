using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSRDPNatTraverseClient.ProxyServer
{
    public class ProxyServer
    {
        #region 代理服务器的一些私有变量
        private string _name = "代理服务器";
        private string _hostname = "example.com";
        private int _loginPort = 22;
        private string _loginName = "root";
        private string _loginPassword = "password";
        private string _description = "示例：这是一台用于远程控制的代理服务器。";
        #endregion

        #region 代理服务器公开的属性
        public string Name
        {
            get
            {
                return this._name;
            }
            set
            {
                this._name = ValidateValue(value, this._name);
            }
        }

        public string Hostname
        {
            get
            {
                return this._hostname;
            }
            set
            {
                this._hostname = ValidateValue(value, this._hostname);
            }
        }

        public int LoginPort
        {
            get
            {
                return this._loginPort;
            }
            set
            {
                this._loginPort = value;
            }
        }

        public string LoginName
        {
            get
            {
                return this._loginName;
            }
            set
            {
                this._loginName = ValidateValue(value, this._loginName);
            }
        }

        public string LoginPassword
        {
            get
            {
                return this._loginPassword;
            }
            set
            {
                this._loginPassword = ValidateValue(value, this._loginPassword);
            }
        }

        public string Description
        {
            get
            {
                return this._description;
            }
            set
            {
                this._description = ValidateValue(value, this._description);
            }
        }
        #endregion

        #region 主要函数
        public ProxyServer() { }
        public ProxyServer(string name, 
            string ipAddress, 
            int loginPort, string 
            loginName, 
            string loginPassword, 
            string description)
        {
            Name = name;
            Hostname = ipAddress;
            LoginName = loginName;
            LoginPassword = loginPassword ;
            Description = description;
            LoginPort = loginPort;
        }
        #endregion

        #region 工具(utilities)
        private string ValidateValue(string value, string defaultValue)
        {
            if (!string.IsNullOrEmpty(value))
            {
                return value;
            }
            else
            {
                return defaultValue;
            }
        }
        #endregion

    }
}
