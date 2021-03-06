﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ac_predictor.Util;

namespace ac_predictor.Handlers
{
    public class OPTIONSHandler : IHttpHandler
    {
        public bool IsReusable => true;

        public void ProcessRequest(HttpContext context)
        {
            HttpRequest request = context.Request;
            HttpResponse responce = context.Response;
            if (!request.Headers.AllKeys.Contains("Origin")) return;
            var requestUrl = request.Url;
            var originUrl = new Uri(request.Headers["Origin"]);
            if (!CrossOriginSettings.IsAllowed(requestUrl, originUrl)) return;
            AddHeader("Access-Control-Allow-Origin", originUrl.GetLeftPart(UriPartial.Authority),false);
            AddHeader("Access-Control-Allow-Methods", "GET");
            AddHeader("Access-Control-Allow-Headers", "X-PINGOTHER, Content-Type, Origin");
            AddHeader("Access-Control-Max-Age", "86400", false);

            void AddHeader(string key,string value,bool AllowDuplicate = true)
            {
                if (!AllowDuplicate && responce.Headers.AllKeys.Contains(key)) responce.Headers.Remove(key);
                responce.Headers.Add(key, value);
            }
        }
    }
}