
namespace Overlay
{
    using Godot;
    using System.Collections.Generic;
    using HttpRequestCompletedHandler = Godot.HttpRequest.RequestCompletedEventHandler;
    using Method = Godot.HttpClient.Method;

    public sealed partial class HttpManager : Node
    {
        #region GODOT_INTRINSICS
        public override void _Process(
            double delta
        )
        {
            ProcessHttpRequests();
        }
        #endregion

        public static bool IsResponseCodeSuccessful(
            long responseCode
        )
        {
            return 
                responseCode >= 200u && 
                responseCode < 300u;
        }

        public void SendHttpRequest(
            string url,
            List<string> headers,
            Method method,
            string json,
            HttpRequestCompletedHandler requestCompletedHandler
        )
        {
            var requiresContentLengthHeader =
                method is Method.Post ||
                method is Method.Put;
            if (
                requiresContentLengthHeader is true && 
                json.Length is 0
            )
            {
                headers.Add(
                    item: "Content-Length: 0"
                );
            }

            var httpRequestData = new HttpRequestData(
                url: url,
                headers: headers.ToArray(),
                method: method,
                json: json,
                requestCompletedHandler: requestCompletedHandler
            );
            lock (m_lock)
            {
                m_httpRequestDatas.Enqueue(
                    item: httpRequestData
                );
            }
        }

        #region INTERNAL_VARIABLES_&_STRUCTURES
        private struct HttpRequestData
        {
            public string Url { get; set; } = string.Empty;
            public string[] Headers { get; set; } = null;
            public HttpClient.Method Method { get; set; } = HttpClient.Method.Get;
            public string Json { get; set; } = string.Empty;
            public HttpRequestCompletedHandler RequestCompletedHandler { get; set; } = null;

            public HttpRequestData(
                string url,
                string[] headers,
                HttpClient.Method method,
                string json,
                HttpRequestCompletedHandler requestCompletedHandler
            )
            {
                this.Url = url;
                this.Headers = headers;
                this.Method = method;
                this.Json = json;
                this.RequestCompletedHandler = requestCompletedHandler;
            }
        }

        private readonly Queue<HttpRequestData> m_httpRequestDatas = new();
        private readonly object m_lock = new();
        #endregion

        #region INTERNAL_PROCESSORS
        private void ProcessHttpRequests()
        {
            while (true)
            {
                HttpRequestData httpRequestData;
                lock (m_lock)
                {
                    if (m_httpRequestDatas.Count > 0u)
                    {
                        httpRequestData = m_httpRequestDatas.Dequeue();
                    }
                    else
                    {
                        break;
                    }
                }

                var httpRequest = new HttpRequest();
                AddChild(
                    node: httpRequest
                );
                httpRequest.RequestCompleted += httpRequestData.RequestCompletedHandler;
                httpRequest.RequestCompleted +=
                (
                    long result,
                    long responseCode,
                    string[] headers,
                    byte[] body
                ) =>
                {
                    httpRequest.QueueFree();
                };

                _ = httpRequest.Request(
                    url: httpRequestData.Url,
                    customHeaders: httpRequestData.Headers,
                    method: httpRequestData.Method,
                    requestData: httpRequestData.Json
                );
            }
        }
        #endregion
    }
}