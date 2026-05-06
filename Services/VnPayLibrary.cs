using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Microsoft.AspNetCore.Http;

namespace Do_an_co_so.Services
{
    public class VnPayLibrary
    {
        private readonly SortedList<string, string> _requestData = new SortedList<string, string>();
        private readonly SortedList<string, string> _responseData = new SortedList<string, string>();

        public void AddRequestData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value)) _requestData.Add(key, value);
        }

        public void AddResponseData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value)) _responseData.Add(key, value);
        }

        public string GetResponseData(string key)
        {
            return _responseData.TryGetValue(key, out var retValue) ? retValue : string.Empty;
        }

        public string CreateRequestUrl(string baseUrl, string vnp_HashSecret)
        {
            StringBuilder data = new StringBuilder();
            foreach (KeyValuePair<string, string> kv in _requestData)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                {
                    data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
                }
            }
            string queryString = data.ToString();
            baseUrl += "?" + queryString;
            string signData = queryString.Remove(queryString.Length - 1);
            string vnp_SecureHash = HmacSHA512(vnp_HashSecret, signData);
            baseUrl += "vnp_SecureHash=" + vnp_SecureHash;
            return baseUrl;
        }

        public bool ValidateSignature(string inputHash, string secretKey)
        {
            StringBuilder data = new StringBuilder();
            foreach (KeyValuePair<string, string> kv in _responseData)
            {
                if (!string.IsNullOrEmpty(kv.Value) && kv.Key != "vnp_SecureHash")
                {
                    data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
                }
            }
            string checkSum = HmacSHA512(secretKey, data.ToString().Remove(data.Length - 1));
            return checkSum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
        }

        private string HmacSHA512(string key, string inputData)
        {
            var hash = new StringBuilder();
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                byte[] hashValue = hmac.ComputeHash(inputBytes);
                foreach (var theByte in hashValue)
                {
                    hash.Append(theByte.ToString("x2"));
                }
            }
            return hash.ToString();
        }
    }
}