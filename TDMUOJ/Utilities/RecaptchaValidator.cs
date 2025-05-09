using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web;
using System.Web.Configuration;
using System.Web.Script.Serialization;

namespace TDMUOJ.Utilities
{
    public class RecaptchaValidator
    {
        private const string RECAPTCHA_VERIFY_URL = "https://www.google.com/recaptcha/api/siteverify";

        public static bool Validate(HttpRequestBase request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            string recaptchaResponse = request.Form["g-recaptcha-response"];

            if (string.IsNullOrEmpty(recaptchaResponse))
                return false;

            try
            {
                string secretKey = WebConfigurationManager.AppSettings["RecaptchaSecretKey"];

                if (string.IsNullOrEmpty(secretKey))
                    throw new Exception("reCAPTCHA Secret Key not found in configuration.");

                using (WebClient client = new WebClient())
                {
                    var postData = new Dictionary<string, string>
                    {
                        { "secret", secretKey },
                        { "response", recaptchaResponse },
                        { "remoteip", request.UserHostAddress }
                    };

                    client.Headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");

                    string response = client.UploadString(
                        RECAPTCHA_VERIFY_URL,
                        "POST",
                        BuildQueryString(postData)
                    );

                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    RecaptchaResponse recaptchaResult = serializer.Deserialize<RecaptchaResponse>(response);

                    return recaptchaResult.Success;
                }
            }
            catch (Exception ex)
            {
                // Log exception
                System.Diagnostics.Debug.WriteLine("reCAPTCHA validation error: " + ex.Message);
                return false;
            }
        }

        private static string BuildQueryString(Dictionary<string, string> parameters)
        {
            List<string> queryParts = new List<string>();

            foreach (var parameter in parameters)
            {
                queryParts.Add(
                    string.Format(
                        "{0}={1}",
                        HttpUtility.UrlEncode(parameter.Key),
                        HttpUtility.UrlEncode(parameter.Value)
                    )
                );
            }

            return string.Join("&", queryParts);
        }

        private class RecaptchaResponse
        {
            public bool Success { get; set; }
            public string[] ErrorCodes { get; set; }
        }
    }
}