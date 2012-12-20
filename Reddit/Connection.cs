﻿
namespace Reddit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Net;
    using System.IO;
    using Things.API;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public static class Connection
    {
        #region Properties

        public static string UserAgent { get; set; }

        public static string Cookie { get; set; }

        public static string ModHash { get; set; }

        #endregion

        #region Logged in?

        public static bool LoggedIn ()
        {
            return !string.IsNullOrEmpty(Cookie) && !string.IsNullOrEmpty(ModHash);
        }

        #endregion

        #region Get

        public static string Get (string Url, string Args = "")
        {
            if (!CheckProperties()) throw new NotLoggedInException("you need to: var r = new Reddit(myUserAgent); r.Login(Username, Password);");
            if (!string.IsNullOrEmpty(Args))
            {
                Url = Url + "?" + Args;
            }
            var Request = (HttpWebRequest)WebRequest.Create("http://www.reddit.com" + Url);
            Request.UserAgent = UserAgent;
            Request.CookieContainer = new CookieContainer();
            Request.CookieContainer.Add(Request.RequestUri, new CookieCollection() { new Cookie("reddit_session", Cookie.Replace(",", "%2c")), new Cookie("uh", ModHash) });
            Request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            var Response = Request.GetResponse();
            using (var Reader = new StreamReader(Response.GetResponseStream()))
            {
                return Reader.ReadToEnd().Trim();
            }
        }

        #endregion

        #region Post

        public static string Post (string Url, string Post)
        {
            if (!Url.Equals("/api/login") && !CheckProperties()) throw new NotLoggedInException("you need to: var r = new Reddit(myUserAgent); r.Login(Username, Password);");
            var Request = (HttpWebRequest)WebRequest.Create("http://www.reddit.com" + Url);

            string _Modhash = "";
            if (!string.IsNullOrEmpty(Cookie))
            {
                Request.CookieContainer = new CookieContainer();
                Request.CookieContainer.Add(Request.RequestUri, new CookieCollection() { new Cookie("reddit_session", Cookie.Replace(",", "%2c")) });
                _Modhash = "&uh=" + ModHash;
            }

            var PostData = new ASCIIEncoding().GetBytes(Post + "&uh=" + ModHash + "&api_type=json");
            Request.UserAgent = UserAgent;
            Request.Method = "POST";
            Request.ContentType = "application/x-www-form-urlencoded";
            Request.ContentLength = PostData.Length;
            using (var Stream = Request.GetRequestStream())
            {
                Stream.Write(PostData, 0, PostData.Length);
            }

            var Response = (HttpWebResponse)Request.GetResponse();

            if (Response == null)
            {
                return null;
            }
            using (var Reader = new StreamReader(Response.GetResponseStream()))
            {
                return Reader.ReadToEnd().Trim();
            }
        }

        #endregion

        #region CheckProperties

        private static bool CheckProperties ()
        {
            return !(string.IsNullOrEmpty(UserAgent) && string.IsNullOrEmpty(Cookie) && string.IsNullOrEmpty(ModHash));
        }

        #endregion
    }
}
