using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MSRDPNatTraverseClient.Client
{
    public class Client
    {
        #region 私有变量
        private string name = "TestPC";
        private int id = 1000;
        private int rdpPort = 3389;
        private string description = "示例：这是一台测试PC。";
        private int peeredId = -1;
        #endregion

        #region 属性
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        public int ID
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }

        public int RDPPort
        {
            get
            {
                return rdpPort;
            }
            set
            {
                rdpPort = value;
            }
        }

        public string Description
        {
            get
            {
                return description;
            }
            set
            {
                description = value;
            }
        }

        public int PeeredId
        {
            get
            {
                return peeredId;
            }
            set
            {
                peeredId = value;
            }
        }
        #endregion

        #region 函数
        public Client() { }
        public Client(string name, int id, int rdpPort, string descrition)
        {
            Name = name;
            ID = id;
            RDPPort = rdpPort;
            Description = description;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
        #endregion
    }
}
