using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;

namespace Maple.Web.WarmupStarter
{
    public static class WarmupUtility
    {
        /// <summary>
        /// 网络应用程序初始化预热时所需的文件目录 
        /// </summary>
        public static readonly string WarmupFilesPath = "~/App_Data/Warmup/";
        /// <summary>
        /// 返回值为True，那么将请求挂起；如果返回False，则继续Web管道生命周期
        /// </summary>
        /// <param name="httpApplication"></param>
        /// <returns></returns>
        public static bool DoBeginRequest(HttpApplication httpApplication)
        {
            // use the url as it was requested by the client
            // the real url might be different if it has been translated (proxy, load balancing, ...)
            var url = ToUrlString(httpApplication.Request);
            //将一个url字符串转换成另一个只包含数字、字母和下滑线的字符串，使之能够作为友好文件名
            var virtualFileCopy = WarmupUtility.EncodeUrl(url.Trim('/'));
            var localCopy = Path.Combine(HostingEnvironment.MapPath(WarmupFilesPath), virtualFileCopy);

            if (File.Exists(localCopy))
            {
                // 不缓存请求
                httpApplication.Response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));
                httpApplication.Response.Cache.SetValidUntilExpires(false);
                httpApplication.Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
                httpApplication.Response.Cache.SetCacheability(HttpCacheability.NoCache);
                httpApplication.Response.Cache.SetNoStore();

                httpApplication.Response.WriteFile(localCopy);
                httpApplication.Response.End();
                return true;
            }

            if (File.Exists(httpApplication.Request.PhysicalPath))
            {
                return true;
            }

            return false;
        }

        public static string ToUrlString(HttpRequest request)
        {
            return string.Format("{0}://{1}{2}", request.Url.Scheme, request.Headers["Host"], request.RawUrl);
        }

        /// <summary>
        /// 将一个url字符串转换成另一个只包含数字、字母和下滑线的字符串，使之能够作为友好文件名
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string EncodeUrl(string url)
        {
            if (String.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException("url can't be empty");
            }

            var sb = new StringBuilder();
            foreach (var c in url.ToLowerInvariant())
            {
                // only accept alphanumeric chars
                if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
                {
                    sb.Append(c);
                }
                // otherwise encode them in UTF8
                else
                {
                    sb.Append("_");
                    foreach (var b in Encoding.UTF8.GetBytes(new[] { c }))
                    {
                        sb.Append(b.ToString("X"));
                    }
                }
            }

            return sb.ToString();
        }
    }
}
