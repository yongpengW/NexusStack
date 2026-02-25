using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace NexusStack.Infrastructure.Utils
{
    /// <summary>
    /// 字符串扩展方法
    /// </summary>
    public static class StringExtensions
    {

        /// <summary>
        /// 字符串格式化，String.Format() 方法的语法糖
        /// </summary>
        /// <param name="source"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string Format(this string source, params object[] args)
        {
            return string.Format(source, args);
        }

        /// <summary>
        /// 将字符串转换为long类型
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static long Long(this string value)
        {
            long.TryParse(value, out long result);
            return result;
        }

        /// <summary>
        /// 判断字符串是否为手机号码
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsMobile(this string source)
        {
            return Regex.IsMatch(source, @"^1\d{10}$");
        }

        /// <summary>
        /// 根据盐值加密密码
        /// </summary>
        /// <param name="password"></param>
        /// <param name="salt"></param>
        /// <returns></returns>
        public static string EncodePassword(this string password, string salt)
        {
            var bytes = Encoding.UTF8.GetBytes(password);
            var src = Convert.FromBase64String(salt);
            var dst = new byte[src.Length + bytes.Length];

            Buffer.BlockCopy(src, 0, dst, 0, src.Length);
            Buffer.BlockCopy(bytes, 0, dst, src.Length, bytes.Length);

            //byte[] encodeBytes = HashAlgorithm.Create("SHA256").ComputeHash(dst);
            byte[] encodeBytes = SHA256.HashData(dst);

            return Convert.ToBase64String(encodeBytes);
        }

        /// <summary>
        /// MD5 混淆
        /// </summary>
        /// <param name="source"></param>
        /// <returns>MD5 Base64 值</returns>
        public static string EncodeMD5(string source)
        {
            var bytes = Encoding.UTF8.GetBytes(source);
            return EncodeMD5(bytes);
        }

        /// <summary>
        /// MD5 混淆
        /// </summary>
        /// <param name="data"></param>
        /// <returns>MD5 Base64 值</returns>
        public static string EncodeMD5(byte[] data)
        {
            using var hasher = HashAlgorithm.Create("MD5");
            var encodeBytes = hasher.ComputeHash(data);
            return Convert.ToBase64String(encodeBytes);
        }

        /// <summary>
        /// Base64 解密
        /// </summary>
        /// <param name="base64String"></param>
        /// <returns></returns>
        public static string Base64ToString(this string base64String)
        {
            byte[] base64Bytes = Convert.FromBase64String(base64String);
            return Encoding.UTF8.GetString(base64Bytes);
        }

        /// <summary>
        /// 生成 Token
        /// </summary>
        /// <param name="username"></param>
        /// <param name="expirationDate"></param>
        /// <returns></returns>
        public static string GenerateToken(string userName, DateTimeOffset expirationDate)
        {
            var data = new byte[64];
            RandomNumberGenerator.Create().GetBytes(data);
            return Convert.ToBase64String(data);
        }

        /// <summary>
        /// 生成 PasswordSalt
        /// </summary>
        /// <returns></returns>
        public static string GeneratePassworldSalt()
        {
            var data = new byte[32];
            RandomNumberGenerator.Create().GetBytes(data);
            return Convert.ToBase64String(data);
        }

        /// <summary>
        /// 判断字符串是否为空
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrWhiteSpace(value) || value.Equals("null", StringComparison.CurrentCultureIgnoreCase);
        }

        /// <summary>
        /// 判断字符串是否不为空
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsNotNullOrEmpty(this string value)
        {
            return !string.IsNullOrWhiteSpace(value) && !value.Equals("null", StringComparison.CurrentCultureIgnoreCase);
        }

        public static List<long> toBigIntList(this string? value)
        {
            if (string.IsNullOrEmpty(value))
                return new List<long>();
            return value.Split(".", StringSplitOptions.RemoveEmptyEntries).Select(a => Convert.ToInt64(a)).ToList();
        }
        public static string GenerateUniqueFileName(this string value)
        {
            DateTime now = DateTime.Now;
            string timestamp = now.ToString("yyyyMMddHHmmssffff");
            string extension = Path.GetExtension(value);
            string newFileName = $"{Path.GetFileNameWithoutExtension(value)}_{timestamp}{extension}";
            return newFileName;
        }

        /// <summary>
        /// 隐藏敏感信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToMaskSensitiveInfo(this string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "*";
            }

            if (value.Length <= 1)
            {
                return "*";
            }

            return value[0] + new string('*', value.Length - 1);
        }

        public static string ToShopIdString(this long[] values)
        {
            if (values == null || values.Length == 0) { return ""; }

            string result = string.Join(',', values);

            return ',' + result + ',';
        }

        public static string ToShopIdString(this long value)
        {
            return ',' + value.ToString() + ',';
        }

        public static long[] ToShopIdArray(this string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return new long[0];
            var valueTrim = value.Trim(',');
            var shopidList = valueTrim.Split(',');
            var longList = new List<long>();
            foreach (var shopid in shopidList)
            {
                try
                {
                    longList.Add(long.Parse(shopid));
                }
                catch (Exception ex) { }
            }
            return longList.ToArray();
        }
    }
}
