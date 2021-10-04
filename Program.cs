using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

using System.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace test
{
    /// <summary>
    /// 发送http请求，结果在变量中自取
    /// </summary>
    class Program
    {
        //public static string url;
        public static int statusCode;
        public static bool Exception;
        public static string title;
        public static string htmlCode;
        public static string statusCodeString;


        static void Main(string[] args)
        {
            postRequest("168.138.47.113/file");
            Console.WriteLine("statusCode: " + statusCodeString);
            Console.WriteLine("title: " + title);
            Console.WriteLine("htmlcode: "+htmlCode);
        }

        private static String postRequest(String url)
        {
            HttpWebRequest request = null;
            HttpWebResponse resp;
            try
            {
                request = (HttpWebRequest)WebRequest.Create("http://" + url);
            }
            catch (Exception)
            {
                Exception = true;
                return "unknown request exception";
            }


            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                //Parse valid information returned by error 401, etc
                response = (HttpWebResponse)ex.Response;
            }
            catch (Exception)
            {
                Exception = true;
                return "unknown response exception";
            }


            //Parse the response
            if (response != null)
            {
                statusCode = (int)response.StatusCode;
                Console.WriteLine(statusCodeString);
                Stream stream = response.GetResponseStream();
                StreamReader reader = new StreamReader(stream);
                string responseText = reader.ReadToEnd();
                reader.Close();
                stream.Close();
                htmlCode = responseText;
            }


            switch (statusCode)
            {
                case 200:
                    statusCodeString = "200 OK";
                    break;
                case 301:
                    statusCodeString = "301 Moved Permanently";
                    break;
                case 400:
                    statusCodeString = "400 Bad Request";
                    break;
                case 403:
                    statusCodeString = "403 Forbidden";
                    break;
                case 404:
                    statusCodeString = "404 Not Found";
                    break;
                case 500:
                    statusCodeString = "500 Internal Server Error";
                    break;
                case 501:
                    statusCodeString = "501 Not Implemented";
                    break;
                case 502:
                    statusCodeString = "502 Bad Gateway";
                    break;
                case 503:
                    statusCodeString = "503 Service Unavailable";
                    break;
                case 504:
                    statusCodeString = "504 Gateway Timeout";
                    break;
                default:
                    statusCodeString = "" + statusCode;
                    break;
            }


            if (htmlCode != null)
            {
                Match m = Regex.Match(htmlCode, @"<title>\s*(.+?)\s*</title>");
                if (m.Success)
                {
                    title = m.Groups[1].Value;
                }
                else
                {
                    title = "";
                }
            }
            //else
            //    htmlCode = statusCodeString;
                //return "null response exception";

            return htmlCode;

        }

    }

}
