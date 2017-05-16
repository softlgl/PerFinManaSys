using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace PerFinManaSys.Web.Tools
{
    public class Des
    {
        /// <summary>
        /// DES加密
        /// </summary>
        /// <param name="inputString">加密字符串</param>
        /// <returns></returns>
        public static string DesEncrypt(string inputString)
        {
            byte[] iv = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF, };
            try
            {
                var byKey = Encoding.UTF8.GetBytes(Key.Substring(0, 8));
                DESCryptoServiceProvider des = new DESCryptoServiceProvider();
                byte[] inputByteArray = Encoding.UTF8.GetBytes(inputString);
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(byKey, iv), CryptoStreamMode.Write);
                cs.Write(inputByteArray, 0, inputByteArray.Length);
                cs.FlushFinalBlock();
                return Convert.ToBase64String(ms.ToArray());
            }
            catch (Exception error)
            {
                LogHelper.Error(error);
                return null;
            }
        }

        /// <summary>
        /// DES解密
        /// </summary>
        /// <param name="inputString">要解密字符串</param>
        /// <returns></returns>
        public static string DesDecrypt(string inputString)
        {
            byte[] iv = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };
            try
            {
                var byKey = Encoding.UTF8.GetBytes(Key.Substring(0, 8));
                DESCryptoServiceProvider des = new DESCryptoServiceProvider();
                var inputByteArray = Convert.FromBase64String(inputString);
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(byKey, iv), CryptoStreamMode.Write);
                cs.Write(inputByteArray, 0, inputByteArray.Length);
                cs.FlushFinalBlock();
                Encoding encoding = new UTF8Encoding();
                return encoding.GetString(ms.ToArray());
            }
            catch (Exception error)
            {
                LogHelper.Error(error);
                return null;
            }  
        }

        /// <summary>
        /// Key
        /// </summary>
        private static string Key
        {
            get
            {
                return "ligliang";
            }
        }  
    }
}
