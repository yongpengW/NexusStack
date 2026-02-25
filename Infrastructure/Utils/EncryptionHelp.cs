using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace NexusStack.Infrastructure.Utils
{
    public class EncryptionHelp
    {
        /// <summary>
        /// MD5 Encryption
        /// </summary>
        /// <param name="strInput"></param>
        /// <returns></returns>
        public static string GetMD5(string strInput)
        {
            using var md5 = MD5.Create();
            byte[] byteResult = md5.ComputeHash(Encoding.UTF8.GetBytes(strInput));
            string strResult = BitConverter.ToString(byteResult);
            string realResult = strResult.Replace("-", null);
            return realResult;
        }

        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="password"></param>
        /// <param name="bit"></param>
        /// <returns></returns>
        public static string MD5Encrypt(string password, byte bit = 32)
        {
            var md5Hasher = new MD5CryptoServiceProvider();
            byte[] hashedDataBytes;
            hashedDataBytes = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(password));
            var tmp = new StringBuilder();
            foreach (byte i in hashedDataBytes)
            {
                tmp.Append(i.ToString("x2"));
            }
            if (bit == 16) return tmp.ToString().Substring(8, 16);
            else if (bit == 32) return tmp.ToString();//默认情况
            else return string.Empty;
        }

        /// <summary>
        /// 公钥格式的转换
        /// </summary>
        /// <param name="publicKey"></param>
        /// <returns></returns>
        public static string RsaPublicKeyToXml(string publicKey)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(publicKey))
                    return "";
                if (publicKey.Contains("<RSAKeyValue>"))
                    return publicKey;
                RsaKeyParameters publicKeyParam;
                //尝试进行java格式的密钥读取
                try
                {
                    publicKeyParam = (RsaKeyParameters)PublicKeyFactory.CreateKey(Convert.FromBase64String(publicKey));
                }
                catch
                {
                    publicKeyParam = null;
                }
                //非java格式密钥进行pem格式的密钥读取
                if (publicKeyParam == null)
                {
                    try
                    {
                        var pemKey = publicKey;
                        if (!pemKey.Contains("BEGIN RSA PRIVATE KEY"))
                        {
                            pemKey = @"-----BEGIN RSA PRIVATE KEY-----
                           " + publicKey + @"
                           -----END RSA PRIVATE KEY-----";
                        }
                        var array = Encoding.ASCII.GetBytes(pemKey);
                        var stream = new MemoryStream(array);
                        var reader = new StreamReader(stream);
                        var pemReader = new Org.BouncyCastle.OpenSsl.PemReader(reader);
                        publicKeyParam = (RsaKeyParameters)pemReader.ReadObject();
                    }
                    catch
                    {
                        publicKeyParam = null;
                    }
                }
                //如果都解析失败，则返回原串
                if (publicKeyParam == null)
                    return publicKey;
                //输出XML格式密钥
                return string.Format(
                    "<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent></RSAKeyValue>",
                    Convert.ToBase64String(publicKeyParam.Modulus.ToByteArrayUnsigned()),
                    Convert.ToBase64String(publicKeyParam.Exponent.ToByteArrayUnsigned())
                );
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// RSA加密
        /// </summary>
        /// <param name="publicKey"></param>
        /// <param name="rawInput"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static string RsaEncrypt(string publicKey, string rawInput)
        {
            if (string.IsNullOrEmpty(rawInput))
            {
                return string.Empty;
            }

            if (string.IsNullOrWhiteSpace(publicKey))
            {
                throw new ArgumentException("Invalid Public Key");
            }

            using (var rsaProvider = new RSACryptoServiceProvider())
            {

                var inputBytes = Encoding.UTF8.GetBytes(rawInput);
                var key = RsaPublicKeyToXml(publicKey);
                rsaProvider.FromXmlString(key);
                int bufferSize = (rsaProvider.KeySize / 8) - 11;
                var buffer = new byte[bufferSize];
                using (MemoryStream inputStream = new MemoryStream(inputBytes),
                     outputStream = new MemoryStream())
                {
                    while (true)
                    {
                        int readSize = inputStream.Read(buffer, 0, bufferSize);
                        if (readSize <= 0)
                        {
                            break;
                        }

                        var temp = new byte[readSize];
                        Array.Copy(buffer, 0, temp, 0, readSize);
                        var encryptedBytes = rsaProvider.Encrypt(temp, false);
                        outputStream.Write(encryptedBytes, 0, encryptedBytes.Length);
                    }
                    return Convert.ToBase64String(outputStream.ToArray());
                }
            }
        }

        /// <summary>
        /// 生成随机批次号
        /// </summary>
        /// <returns></returns>
        public static string GenerateBatchNumber()
        {
            var utcNow = DateTime.UtcNow.AddDays(-5);
            var randomNumber = new Random().Next(0, 999);
            var batchNumber = $"{utcNow:yyyyMMdd}{randomNumber:D3}";
            return batchNumber;
        }

        /// <summary>
        /// MD5签名
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="secret"></param>
        /// <returns></returns>
        public static string SignMD5Request(IDictionary<string, string> parameters, string secret)
        {
            // 第一步：把字典按Key的字母顺序排序
            var sortedParams = new SortedDictionary<string, string>(parameters, StringComparer.Ordinal);
            var dem = sortedParams.GetEnumerator();
            // 第二步：把所有参数名和参数值串在一起
            var query = new StringBuilder();
            query.Append(secret);
            while (dem.MoveNext())
            {
                string key = dem.Current.Key;
                string value = dem.Current.Value;
                if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                {
                    query.Append(key).Append(value);
                }
            }
            // 第三步：使用MD5加密
            byte[] bytes;
            query.Append(secret);
            var md5 = MD5.Create();
            //先转换成UTF8
            var utf8 = Encoding.UTF8.GetBytes(query.ToString());
            bytes = md5.ComputeHash(utf8);
            // 第四步：把二进制转化为大写的十六进制
            var result = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                result.Append(bytes[i].ToString("X2"));
            }
            return result.ToString();
        }

        /// <summary>
        /// 菜鸟开放平台签名生成
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="secret"></param>
        /// <returns></returns>
        public static string GenerateCainiaoSha256Signature(IDictionary<string, string> parameters, string secret)
        {
            // 按 key 字典序升序排序
            var sorted = parameters.OrderBy(p => p.Key, StringComparer.Ordinal).ToList();

            // 拼接成 key1value1key2value2... 形式（注意：不是 query string）
            var sb = new StringBuilder();
            foreach (var (key, value) in sorted)
            {
                sb.Append(key).Append(value);
            }

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
            return Convert.ToBase64String(hash);
        }
    }
}
