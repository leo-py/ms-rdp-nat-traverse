using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSRDPNatTraverseClient.Utility
{
    public class TextEncryption
    {
        #region 核心功能函数

        public static string Encrypt(string content, string password)
        {
            var encContent = content.ToCharArray();
            var key = password.ToCharArray();

            for (int i = 0; i < encContent.Length; i++)
            {
                encContent[i] ^= key[i % key.Length];
            }

            return new string(encContent);
        }

        public static string Decrypt(string content, string password)
        {
            char[] data = content.ToCharArray();
            char[] passwd = password.ToCharArray();

            // 解密运算
            for (int i = 0; i < data.Length; i++)
            {
                data[i] ^= password[i % passwd.Length];
            }

            // 返回解密后的内容
            return new string(data);
        }
        #endregion
    }
}
