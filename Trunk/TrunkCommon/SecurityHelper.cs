using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace TrunkCommon
{
    /// <summary>
    /// 加密工具类
    /// </summary>
    public class SecurityHelper
    {
        /// <summary>
        /// MD5单向加密字符串(16进制32个字符形式，注意默认是大写)。标准签名法
        /// </summary>
        public static string MD5Encrypt(string s)
        {
            if (s == null)
                return null;
            byte[] bs = MD5Encrypt(Encoding.UTF8.GetBytes(s));
            char[] cs = new char[32];
            for (int i = 0; i < bs.Length; i++)
            {
                cs[i * 2] = bs[i].ToString("X2")[0];
                cs[i * 2 + 1] = bs[i].ToString("X2")[1];
            }
            return new string(cs);
        }

        /// <summary>
        /// md5算法加密数据
        /// </summary>
        public static byte[] MD5Encrypt(byte[] b)
        {
            if (b == null)
                return null;
            MD5 md5 = new MD5CryptoServiceProvider();
            b = md5.ComputeHash(b);
            md5.Clear();
            return b;
        }

        /// <summary>
        /// AES加密算法
        /// </summary>
        public static byte[] AESEncrypt(byte[] plainTextBytes, string key)
        {
            //分组加密算法
            SymmetricAlgorithm des = Rijndael.Create();
            //设置密钥及密钥向量
            des.Key = Encoding.UTF8.GetBytes(key);
            des.IV = _IV;
            byte[] cipherBytes = null;
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(plainTextBytes, 0, plainTextBytes.Length);
                    cs.FlushFinalBlock();
                    cipherBytes = ms.ToArray();//得到加密后的字节数组
                    cs.Close();
                    ms.Close();
                }
            }
            return cipherBytes;
        }
        /// <summary>
        /// AES加密算法（密钥为16个字符）
        /// </summary>
        public static string AESEncrypt(string plainTextBytes, string key)
        {
            return Convert.ToBase64String(AESEncrypt(Encoding.UTF8.GetBytes(plainTextBytes), key));
        }

        /// <summary>
        /// AES解密
        /// </summary>
        public static byte[] AESDecrypt(byte[] showTextBytes, string key)
        {
            SymmetricAlgorithm des = Rijndael.Create();
            des.Key = Encoding.UTF8.GetBytes(key);
            des.IV = _IV;
            byte[] decryptBytes = new byte[showTextBytes.Length];
            using (MemoryStream ms = new MemoryStream(showTextBytes))
            {
                using (CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    try
                    {
                        cs.Read(decryptBytes, 0, decryptBytes.Length);
                    }
                    catch
                    {
                        return null;
                    }
                }
            }
            return decryptBytes;
        }

        /// <summary>
        /// AES解密
        /// </summary>
        public static byte[] AESDecrypt(byte[] showTextBytes, string key, byte[] iv)
        {
            SymmetricAlgorithm des = Rijndael.Create();
            des.Key = Encoding.UTF8.GetBytes(key);
            des.IV = iv;
            byte[] decryptBytes = new byte[showTextBytes.Length];
            using (MemoryStream ms = new MemoryStream(showTextBytes))
            {
                using (CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    try
                    {
                        cs.Read(decryptBytes, 0, decryptBytes.Length);
                    }
                    catch
                    {
                        return null;
                    }
                }
            }
            return decryptBytes;
        }

        /// <summary>
        /// AES解密
        /// </summary>
        public static string AESDecrypt(string showTextBytes, string key)
        {
            byte[] b = AESDecrypt(Convert.FromBase64String(showTextBytes), key);
            if (b == null)
                return null;
            return Encoding.UTF8.GetString(b).Replace("\0", string.Empty);
        }


        /// <summary>
        /// AES解密
        /// </summary>
        public static string AESDecrypt(string showTextBytes, string key, byte[] iv)
        {
            byte[] b = AESDecrypt(Convert.FromBase64String(showTextBytes), key, iv);
            if (b == null)
                return null;
            return Encoding.UTF8.GetString(b).Replace("\0", string.Empty);
        }



        //默认密钥向量 
        private static byte[] _IV = { 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 };

        /// <summary>
        /// SHA1单向加密数据
        /// </summary>
        public static byte[] SHA1Encrypt(byte[] b)
        {
            if (b == null)
                return null;
            SHA1 sha1 = new SHA1CryptoServiceProvider();
            b = sha1.ComputeHash(b);
            sha1.Clear();
            return b;
        }

        /// <summary>
        /// SHA1单向加密字符串
        /// </summary>
        public static string SHA1Encrypt(string s)
        {
            if (s == null)
                return null;
            return Convert.ToBase64String(SHA1Encrypt(Encoding.UTF8.GetBytes(s)));
        }

        /// <summary>
        /// SHA256单向加密数据
        /// </summary>
        public static byte[] SHA256Encrypt(byte[] b)
        {
            if (b == null)
                return null;
            SHA256 sha256 = new SHA256Managed();
            b = sha256.ComputeHash(b);
            sha256.Clear();
            return b;
        }

        /// <summary>
        /// SHA256单向加密字符串
        /// </summary>
        public static string SHA256Encrypt(string s)
        {
            if (s == null)
                return null;
            return Convert.ToBase64String(SHA256Encrypt(Encoding.UTF8.GetBytes(s)));
        }


        /// <summary>
        /// SHA384单向加密数据
        /// </summary>
        public static byte[] SHA384Encrypt(byte[] b)
        {
            if (b == null)
                return null;
            SHA384 sha384 = new SHA384Managed();
            b = sha384.ComputeHash(b);
            sha384.Clear();
            return b;
        }

        /// <summary>
        /// SHA384单向加密字符串
        /// </summary>
        public static string SHA384Encrypt(string s)
        {
            if (s == null)
                return null;
            return Convert.ToBase64String(SHA384Encrypt(Encoding.UTF8.GetBytes(s)));
        }

        /// <summary>
        /// SHA512单向加密数据
        /// </summary>
        public static byte[] SHA512Encrypt(byte[] b)
        {
            if (b == null)
                return null;
            SHA512 sha512 = new SHA512Managed();
            b = sha512.ComputeHash(b);
            sha512.Clear();
            return b;
        }

        /// <summary>
        /// SHA512单向加密字符串
        /// </summary>
        public static string SHA512Encrypt(string s)
        {
            if (s == null)
                return null;
            return Convert.ToBase64String(SHA512Encrypt(Encoding.UTF8.GetBytes(s)));
        }

        /// <summary>
        /// DES快速加密数据
        /// </summary>
        public static byte[] DESEncrypt(byte[] b, string key)
        {
            if (b == null)
                return null;
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            byte[] k = null;
            if (key == null)
            {
                des.Key = des.IV = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            }
            else
            {
                k = Encoding.UTF8.GetBytes(key);
                if (k.Length >= 8)
                {
                    des.Key = new byte[] { k[0], k[1], k[2], k[3], k[4], k[5], k[6], k[7] };
                    int tl = k.Length;
                    des.IV = new byte[] { k[tl - 1], k[tl - 2], k[tl - 3], k[tl - 4], k[tl - 5], k[tl - 6], k[tl - 7], k[tl - 8] };
                }
                else
                {
                    byte[] tk = new byte[8];
                    byte[] tv = new byte[8];
                    for (int i = 0, j = k.Length - 1; i < k.Length; i++, j--)
                    {
                        tk[i] = k[i];
                        tv[i] = k[j];
                    }
                    for (int i = k.Length; i < 8; i++)
                    {
                        tk[i] = tv[i] = 0;
                    }
                    des.Key = tk;
                    des.IV = tv;
                }
            }
            ICryptoTransform ct = des.CreateEncryptor();
            using (MemoryStream ms = new MemoryStream())
            {
                CryptoStream cs = new CryptoStream(ms, ct, CryptoStreamMode.Write);
                cs.Write(b, 0, b.Length);
                cs.FlushFinalBlock();
                cs.Close();
                des.Clear();
                return ms.ToArray();
            }
        }

        /// <summary>
        /// DES快速加密字符串
        /// </summary>
        public static string DESEncrypt(string s, string key)
        {
            if (s == null)
                return null;
            return Convert.ToBase64String(DESEncrypt(Encoding.UTF8.GetBytes(s), key));
        }

        /// <summary>
        /// DES快速解密数据
        /// </summary>
        public static byte[] DESDecrypt(byte[] b, string key)
        {
            if (b == null)
                return null;
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            byte[] k = null;
            if (key == null)
            {
                des.Key = des.IV = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            }
            else
            {
                k = Encoding.UTF8.GetBytes(key);
                if (k.Length >= 8)
                {
                    des.Key = new byte[] { k[0], k[1], k[2], k[3], k[4], k[5], k[6], k[7] };
                    int tl = k.Length;
                    des.IV = new byte[] { k[tl - 1], k[tl - 2], k[tl - 3], k[tl - 4], k[tl - 5], k[tl - 6], k[tl - 7], k[tl - 8] };
                }
                else
                {
                    byte[] tk = new byte[8];
                    byte[] tv = new byte[8];
                    for (int i = 0, j = k.Length - 1; i < k.Length; i++, j--)
                    {
                        tk[i] = k[i];
                        tv[i] = k[j];
                    }
                    for (int i = k.Length; i < 8; i++)
                    {
                        tk[i] = tv[i] = 0;
                    }
                    des.Key = tk;
                    des.IV = tv;
                }
            }
            ICryptoTransform ct = des.CreateDecryptor();
            using (MemoryStream ms = new MemoryStream())
            {
                CryptoStream cs = new CryptoStream(ms, ct, CryptoStreamMode.Write);
                cs.Write(b, 0, b.Length);
                cs.FlushFinalBlock();
                cs.Close();
                des.Clear();
                return ms.ToArray();
            }
        }

        /// <summary>
        /// DES快速解密字符串
        /// </summary>
        public static string DESDecrypt(string s, string key)
        {
            if (s == null)
                return null;
            return Encoding.UTF8.GetString(DESDecrypt(Convert.FromBase64String(s), key));
        }

        /// <summary>
        /// XOR加密
        /// 特征：速度快，安全性较低，可解密，密钥无效返回错误文本
        /// </summary>
        public static string XOREncrypt(string s, string key)
        {
            if (s == null) s = string.Empty;
            if (key == null) key = string.Empty;
            byte[] ks = Encoding.UTF8.GetBytes(key);
            byte[] tx = Encoding.UTF8.GetBytes(s);
            for (int i = 0; i < tx.Length; i++)
                tx[i] ^= ks[i % ks.Length];
            return Convert.ToBase64String(tx);
        }

        /// <summary>
        /// XOR解密
        /// 特征：速度快，安全性较低，可解密，密钥无效返回错误文本
        /// </summary>
        public static string XORDecrypt(string s, string key)
        {
            if (s == null) return null;
            if (key == null) key = string.Empty;
            byte[] ks = Encoding.UTF8.GetBytes(key);
            byte[] tx = Convert.FromBase64String(s);
            for (int i = 0; i < tx.Length; i++)
                tx[i] ^= ks[i % ks.Length];
            return Encoding.UTF8.GetString(tx);
        }

        /// <summary>
        /// 不对称加密法(1024)
        /// </summary>
        /// <param name="s">要加秘的原本</param>
        /// <param name="publicKey">公钥</param>
        /// <returns>密文(加密失败返回null)</returns>
        public static byte[] RSAEncrypt(byte[] b, string publicKey)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(1024);
            rsa.FromXmlString(publicKey);
            b = rsa.Encrypt(b, false);
            rsa.Clear();
            return b;
        }

        /// <summary>
        /// 不对称加密法(1024)
        /// </summary>
        /// <param name="s">要加秘的原本</param>
        /// <param name="publicKey">公钥</param>
        /// <returns>密文(加密失败返回null)</returns>
        public static string RSAEncrypt(string s, string publicKey)
        {
            if (s == null)
                return null;
            return Convert.ToBase64String(RSAEncrypt(Encoding.UTF8.GetBytes(s), publicKey));
        }


        /// <summary>
        /// 不对称解密法(1024)
        /// </summary>
        /// <param name="s">密文</param>
        /// <param name="privateKey">私钥</param>
        /// <returns>原文(解密失败返回null)</returns>
        public static byte[] RSADectypt(byte[] b, string privateKey)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(1024);
            rsa.FromXmlString(privateKey);
            b = rsa.Decrypt(b, false);
            rsa.Clear();
            return b;
        }

        /// <summary>
        /// 不对称解密法(1024)
        /// </summary>
        /// <param name="s">密文</param>
        /// <param name="privateKey">私钥</param>
        /// <returns>原文(解密失败返回null)</returns>
        public static string RSADectypt(string s, string privateKey)
        {
            if (s == null)
                return null;
            return Encoding.UTF8.GetString(RSADectypt(Convert.FromBase64String(s), privateKey));
        }

        /// <summary>
        /// 创建不对称加密(1024)密钥对（[0]-公钥；[1]-私钥）
        /// </summary>
        /// <returns>公钥和私钥（[0]-公钥；[1]-私钥）</returns>
        public static string[] RSACreateKey()
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(1024);
            return new string[] { rsa.ToXmlString(false), rsa.ToXmlString(true) };
        }

        private static uint[] crcTable = { 0x0, 0x77073096, 0xee0e612c, 0x990951ba, 0x76dc419, 0x706af48f, 0xe963a535, 0x9e6495a3, 0xedb8832, 0x79dcb8a4, 0xe0d5e91e, 0x97d2d988, 0x9b64c2b, 0x7eb17cbd, 0xe7b82d07, 0x90bf1d91, 0x1db71064, 0x6ab020f2, 0xf3b97148, 0x84be41de, 0x1adad47d, 0x6ddde4eb, 0xf4d4b551, 0x83d385c7, 0x136c9856, 0x646ba8c0, 0xfd62f97a, 0x8a65c9ec, 0x14015c4f, 0x63066cd9, 0xfa0f3d63, 0x8d080df5, 0x3b6e20c8, 0x4c69105e, 0xd56041e4, 0xa2677172, 0x3c03e4d1, 0x4b04d447, 0xd20d85fd, 0xa50ab56b, 0x35b5a8fa, 0x42b2986c, 0xdbbbc9d6, 0xacbcf940, 0x32d86ce3, 0x45df5c75, 0xdcd60dcf, 0xabd13d59, 0x26d930ac, 0x51de003a, 0xc8d75180, 0xbfd06116, 0x21b4f4b5, 0x56b3c423, 0xcfba9599, 0xb8bda50f, 0x2802b89e, 0x5f058808, 0xc60cd9b2, 0xb10be924, 0x2f6f7c87, 0x58684c11, 0xc1611dab, 0xb6662d3d, 0x76dc4190, 0x1db7106, 0x98d220bc, 0xefd5102a, 0x71b18589, 0x6b6b51f, 0x9fbfe4a5, 0xe8b8d433, 0x7807c9a2, 0xf00f934, 0x9609a88e, 0xe10e9818, 0x7f6a0dbb, 0x86d3d2d, 0x91646c97, 0xe6635c01, 0x6b6b51f4, 0x1c6c6162, 0x856530d8, 0xf262004e, 0x6c0695ed, 0x1b01a57b, 0x8208f4c1, 0xf50fc457, 0x65b0d9c6, 0x12b7e950, 0x8bbeb8ea, 0xfcb9887c, 0x62dd1ddf, 0x15da2d49, 0x8cd37cf3, 0xfbd44c65, 0x4db26158, 0x3ab551ce, 0xa3bc0074, 0xd4bb30e2, 0x4adfa541, 0x3dd895d7, 0xa4d1c46d, 0xd3d6f4fb, 0x4369e96a, 0x346ed9fc, 0xad678846, 0xda60b8d0, 0x44042d73, 0x33031de5, 0xaa0a4c5f, 0xdd0d7cc9, 0x5005713c, 0x270241aa, 0xbe0b1010, 0xc90c2086, 0x5768b525, 0x206f85b3, 0xb966d409, 0xce61e49f, 0x5edef90e, 0x29d9c998, 0xb0d09822, 0xc7d7a8b4, 0x59b33d17, 0x2eb40d81, 0xb7bd5c3b, 0xc0ba6cad, 0xedb88320, 0x9abfb3b6, 0x3b6e20c, 0x74b1d29a, 0xead54739, 0x9dd277af, 0x4db2615, 0x73dc1683, 0xe3630b12, 0x94643b84, 0xd6d6a3e, 0x7a6a5aa8, 0xe40ecf0b, 0x9309ff9d, 0xa00ae27, 0x7d079eb1, 0xf00f9344, 0x8708a3d2, 0x1e01f268, 0x6906c2fe, 0xf762575d, 0x806567cb, 0x196c3671, 0x6e6b06e7, 0xfed41b76, 0x89d32be0, 0x10da7a5a, 0x67dd4acc, 0xf9b9df6f, 0x8ebeeff9, 0x17b7be43, 0x60b08ed5, 0xd6d6a3e8, 0xa1d1937e, 0x38d8c2c4, 0x4fdff252, 0xd1bb67f1, 0xa6bc5767, 0x3fb506dd, 0x48b2364b, 0xd80d2bda, 0xaf0a1b4c, 0x36034af6, 0x41047a60, 0xdf60efc3, 0xa867df55, 0x316e8eef, 0x4669be79, 0xcb61b38c, 0xbc66831a, 0x256fd2a0, 0x5268e236, 0xcc0c7795, 0xbb0b4703, 0x220216b9, 0x5505262f, 0xc5ba3bbe, 0xb2bd0b28, 0x2bb45a92, 0x5cb36a04, 0xc2d7ffa7, 0xb5d0cf31, 0x2cd99e8b, 0x5bdeae1d, 0x9b64c2b0, 0xec63f226, 0x756aa39c, 0x26d930a, 0x9c0906a9, 0xeb0e363f, 0x72076785, 0x5005713, 0x95bf4a82, 0xe2b87a14, 0x7bb12bae, 0xcb61b38, 0x92d28e9b, 0xe5d5be0d, 0x7cdcefb7, 0xbdbdf21, 0x86d3d2d4, 0xf1d4e242, 0x68ddb3f8, 0x1fda836e, 0x81be16cd, 0xf6b9265b, 0x6fb077e1, 0x18b74777, 0x88085ae6, 0xff0f6a70, 0x66063bca, 0x11010b5c, 0x8f659eff, 0xf862ae69, 0x616bffd3, 0x166ccf45, 0xa00ae278, 0xd70dd2ee, 0x4e048354, 0x3903b3c2, 0xa7672661, 0xd06016f7, 0x4969474d, 0x3e6e77db, 0xaed16a4a, 0xd9d65adc, 0x40df0b66, 0x37d83bf0, 0xa9bcae53, 0xdebb9ec5, 0x47b2cf7f, 0x30b5ffe9, 0xbdbdf21c, 0xcabac28a, 0x53b39330, 0x24b4a3a6, 0xbad03605, 0xcdd70693, 0x54de5729, 0x23d967bf, 0xb3667a2e, 0xc4614ab8, 0x5d681b02, 0x2a6f2b94, 0xb40bbe37, 0xc30c8ea1, 0x5a05df1b, 0x2d02ef8d, };
        /// <summary>
        /// 生成CRC32效验码
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static int CRC32(byte[] bytes)
        {
            uint crc = 0xFFFFFFFF;
            for (int i = 0; i < bytes.Length; i++)
            {
                crc = ((crc >> 8) & 0x00FFFFFF) ^ crcTable[(crc ^ bytes[i]) & 0xFF];
            }
            return (int)(crc ^ 0xFFFFFFFF);
        }

        /// <summary>
        /// 获取加密参数的url地址
        /// </summary>
        /// <param name="requestUrl">要请求的页面地址（不含参数）</param>
        /// <param name="parameters">参数（键只能是字母或数字组成，字母必须开头）</param>
        /// <param name="addSignature">是否自动加MD5签名（防止被篡改）</param>
        /// <param name="addTimestamp">是否自动加时间戳（DateTime.Now.Ticks）</param>
        /// <param name="encryptPassword">16个字母或数字组成的密钥（AES），如果为null或空字符串则不对参数进行加密</param>
        /// <param name="signatureValue">签名字串（输出参数）</param>
        /// <param name="timestampValue">时间戳（输出参数）</param>
        /// <returns>加密参数的url地址</returns>
        public static string GetSecureUrl(string requestUrl, IEnumerable<KeyValuePair<string, string>> parameters, string signatureExt, string encryptPassword, out string signatureValue, out DateTime timestampValue)
        {
            if (requestUrl == null)
                requestUrl = string.Empty;

            signatureValue = null;
            timestampValue = DateTime.MinValue;

            if (parameters == null)
                parameters = new Dictionary<string, string>();

            List<KeyValuePair<string, string>> orderedParameters = new List<KeyValuePair<string, string>>(parameters);
            timestampValue = DateTime.Now;
            orderedParameters.Add(new KeyValuePair<string, string>(SECUREURL_TIMESTAMP_NAME, timestampValue.Ticks.ToString()));

            orderedParameters.Sort(delegate(KeyValuePair<string, string> a, KeyValuePair<string, string> b)
            {
                return a.Key.CompareTo(b.Key);
            });

            string paramNVP = orderedParameters.GetNvpString(false);

            signatureValue = SecurityHelper.MD5Encrypt(paramNVP + signatureExt).ToUpper();
            paramNVP = string.Concat(paramNVP, "&", SECUREURL_SIGNATURE_NAME, "=", signatureValue);
            paramNVP = string.Concat(SECUREURL_PARAMETER_NAME, "=", HttpServerUtility.UrlTokenEncode(SecurityHelper.AESEncrypt(Encoding.UTF8.GetBytes(paramNVP), encryptPassword)));

            requestUrl = requestUrl.TrimEnd('?', '#');
            return string.Concat(requestUrl, (requestUrl.IndexOf('?') == -1) ? '?' : '&', paramNVP);
        }
        /// <summary>
        /// 获取加密参数的url地址
        /// </summary>
        /// <param name="requestUrl">要请求的页面地址（含参数）</param>
        /// <param name="addSignature">是否自动加MD5签名（防止被篡改）</param>
        /// <param name="addTimestamp">是否自动加时间戳（DateTime.Now.Ticks）</param>
        /// <param name="encryptPassword">16个字母或数字组成的密钥（AES），如果为null或空字符串则不对参数进行加密</param>
        /// <param name="signatureValue">签名字串（输出参数）</param>
        /// <param name="timestampValue">时间戳（输出参数）</param>
        /// <returns>加密参数的url地址</returns>
        public static string GetSecureUrl(string requestUrlWithParameters, string signatureExt, string encryptPassword, out string signatureValue, out DateTime timestampValue)
        {
            if (requestUrlWithParameters == null)
                requestUrlWithParameters = string.Empty;

            Dictionary<string, string> parameters = null;
            string[] query = requestUrlWithParameters.Split('?');
            if (query.Length > 1)
                parameters = WebUrlHelper.ParseQueryString(query[1]).ToDictionary();

            return GetSecureUrl(query[0], parameters, signatureExt, encryptPassword, out signatureValue, out timestampValue);
        }
        public const string SECUREURL_PARAMETER_NAME = "_SUP";
        public const string SECUREURL_SIGNATURE_NAME = "_SUS";
        public const string SECUREURL_TIMESTAMP_NAME = "_SUT";
        /// <summary>
        /// 通过加密参数的url地址获取其中的参数
        /// </summary>
        /// <param name="secureUrl">加密参数的url地址或querystring部分</param>
        /// <param name="encryptPassword">16位字母或数字组成的解密密钥（AES）</param>
        /// <param name="errorCode">错误代码：0-成功，1-无效url地址，2-密钥为空，3-解密失败，4-签名无效，5-时间已过期</param>
        /// <returns>参数</returns>
        public static Dictionary<string, string> GetSecureParameters(string secureUrl, string signatureExt, string encryptPassword, int expireSeconds, out int errorCode)
        {
            string signatureValue = null;
            DateTime timestampValue = DateTime.MinValue;
            errorCode = 0;

            Dictionary<string, string> parameters = new Dictionary<string, string>();

            if (string.IsNullOrEmpty(secureUrl))
            {
                errorCode = 1;//无效地址
                return null;
            }

            if (secureUrl.IndexOf('?') != -1)
            {
                secureUrl = secureUrl.Split('?')[1];
            }

            if (secureUrl.IndexOf('=') == -1)
            {
                secureUrl = string.Empty;
            }

            if (secureUrl.IndexOf(SECUREURL_PARAMETER_NAME + "=", StringComparison.CurrentCultureIgnoreCase) != -1)
            {
                if (string.IsNullOrEmpty(encryptPassword))
                {
                    errorCode = 2;//密码无效
                    return null;
                }
                try
                {
                    string[] tmp1 = secureUrl.Split('&');
                    string[] tmp2 = null;
                    foreach (string item in tmp1)
                    {
                        tmp2 = item.Split('=');
                        if (tmp2.Length == 2 && tmp2[0] == SECUREURL_PARAMETER_NAME)
                        {
                            secureUrl = tmp2[1];
                            break;
                        }
                    }
                    secureUrl = Encoding.UTF8.GetString(
                        SecurityHelper.AESDecrypt(HttpServerUtility.UrlTokenDecode(secureUrl), encryptPassword)
                    ).Replace("\0", string.Empty);
                }
                catch
                {
                    errorCode = 3;//解密失败
                    return null;
                }
            }

            NameValueCollection nvp = WebUrlHelper.ParseQueryString(secureUrl);
            string tmp = null;
            for (int i = 0; i < nvp.Count; i++)
            {
                tmp = nvp.GetKey(i);
                if ("aspxautodetectcookiesupport".Equals(tmp, StringComparison.CurrentCultureIgnoreCase))
                    continue;
                else if (SECUREURL_SIGNATURE_NAME.Equals(tmp, StringComparison.CurrentCultureIgnoreCase))
                {
                    signatureValue = nvp.Get(i).ToUpper();
                    continue;
                }
                else if (SECUREURL_TIMESTAMP_NAME.Equals(tmp, StringComparison.CurrentCultureIgnoreCase))
                {
                    timestampValue = new DateTime(nvp.Get(i).ToLong());
                    if (Math.Abs((timestampValue - DateTime.Now).TotalSeconds) > expireSeconds)
                    {
                        errorCode = 4;//时间已过期
                        return null;
                    }
                }
                parameters.Add(nvp.GetKey(i), nvp.Get(i));
            }


            List<KeyValuePair<string, string>> orderedParameters = new List<KeyValuePair<string, string>>(parameters);
            orderedParameters.Sort(delegate(KeyValuePair<string, string> a, KeyValuePair<string, string> b)
            {
                return a.Key.CompareTo(b.Key);
            });

            string paramNVP = orderedParameters.GetNvpString(false);

            string newSignature = null;
            if (paramNVP.Length > 0)
            {
                newSignature = SecurityHelper.MD5Encrypt(paramNVP + signatureExt).ToUpper();
            }
            if (signatureValue != null && signatureValue != newSignature)
            {
                errorCode = 5;//签名无效
            }

            return parameters;
        }

        /// <summary>
        /// HMacSHA1签名算法
        /// </summary>
        /// <param name="text"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string HMacSHA1(string text, string key)
        {
            HMACSHA1 hmacsha1 = new HMACSHA1(Encoding.UTF8.GetBytes(key));
            byte[] hashBytes = hmacsha1.ComputeHash(Encoding.UTF8.GetBytes(text));
            return Convert.ToBase64String(hashBytes);
        }

    }//end class
}
