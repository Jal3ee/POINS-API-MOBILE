using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace Point_Internal_API.Models
{
    public class Point_IntervalModel
    {
    }
    public class CorsHandler : DelegatingHandler
    {
        const string Origin = "*";
        const string AccessControlRequestMethod = "Access-Control-Request-Method";
        const string AccessControlRequestHeaders = "Access-Control-Request-Headers";
        const string AccessControlAllowOrigin = "Access-Control-Allow-Origin";
        const string AccessControlAllowMethods = "Access-Control-Allow-Methods";
        const string AccessControlAllowHeaders = "Access-Control-Allow-Headers";
        protected string AllowOrigin;

        public CorsHandler()
        {
            AllowOrigin = "*";
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(AllowOrigin))
            {
                return base.SendAsync(request, cancellationToken);
            }
            bool isCorsRequest = request.Headers.Contains(Origin);
            bool isPreflightRequest = request.Method == HttpMethod.Options;
            if (!isCorsRequest)
            {
                return base.SendAsync(request, cancellationToken);
            }
            if (isPreflightRequest)
            {
                return Task.Factory.StartNew(() =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Headers.Add(AccessControlAllowOrigin, request.Headers.GetValues(Origin).First());
                    //response.Headers.Add(AccessControlAllowOrigin, AllowOrigin);

                    string accessControlRequestMethod = request.Headers.GetValues(AccessControlRequestMethod).FirstOrDefault();
                    if (accessControlRequestMethod != null)
                    {
                        response.Headers.Add(AccessControlAllowMethods, accessControlRequestMethod);
                    }

                    string requestedHeaders = string.Join(", ", request.Headers.GetValues(AccessControlRequestHeaders));
                    if (!string.IsNullOrEmpty(requestedHeaders))
                    {
                        response.Headers.Add(AccessControlAllowHeaders, requestedHeaders);
                    }

                    return response;
                }, cancellationToken);
            }
            //!isPreflightRequest
            return base.SendAsync(request, cancellationToken).ContinueWith(t =>
            {
                var resp = t.Result;
                //resp.Headers.Add(AccessControlAllowOrigin, request.Headers.GetValues(Origin).First());
                resp.Headers.Add(AccessControlAllowOrigin, AllowOrigin);
                return resp;
            });
        }
    }
}