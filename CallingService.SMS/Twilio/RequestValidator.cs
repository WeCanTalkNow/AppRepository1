using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SmsEcho.Twilio
{
    public class TwilioRequestValidator
    {
        public bool IsValid(string pathAndQueryString, string expectedSignature, NameValueCollection post)
        {
            if (string.IsNullOrEmpty(pathAndQueryString))
            {
                return false;
            }

            if (string.IsNullOrEmpty(expectedSignature))
            {
                return false;
            }

            string hash = Sign(pathAndQueryString, post);
            return hash == expectedSignature;
        }

        private static string Sign(string pathAndQueryString, NameValueCollection postParameters)
        {
            StringBuilder toSign = new StringBuilder(pathAndQueryString);

            foreach(var key in postParameters.AllKeys.OrderBy(k => k))
            {
                toSign.AppendFormat("{0}{1}", key, postParameters[key]);
            }

            using (HMACSHA1 sha1 = new HMACSHA1(Encoding.UTF8.GetBytes(ConfigurationManager.AppSettings["TwilioAuthToken"])))
            {
                byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(toSign.ToString()));
                return Convert.ToBase64String(hash);
            }
        }
    }
}
