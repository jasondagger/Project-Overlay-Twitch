namespace Overlay
{
    using Godot;
    using System.Collections.Generic;
    using HttpRequestCompletedHandler = Godot.HttpRequest.RequestCompletedEventHandler;

    public sealed partial class HttpManager : Node
    {
        public override void _Process(
            double delta
        )
        {
            while (m_httpRequestDatas.Count > 0u)
            {
                var httpRequestData = m_httpRequestDatas.Dequeue();
                var httpRequest = new HttpRequest();
                AddChild(
                    node: httpRequest
                );
                httpRequest.RequestCompleted += httpRequestData.RequestCompletedHandler;
                httpRequest.RequestCompleted += (
                    long result,
                    long responseCode,
                    string[] headers,
                    byte[] body
                ) =>
                {
                    httpRequest.QueueFree();
                };
                httpRequest.Request(
                    url: httpRequestData.Url,
                    customHeaders: httpRequestData.Headers,
                    method: httpRequestData.Method,
                    requestData: httpRequestData.Json
                );
            }
        }

        public void SendHttpRequest(
            string url,
            string[] headers,
            HttpClient.Method method,
            string json,
            HttpRequestCompletedHandler requestCompletedHandler
        )
        {
            m_httpRequestDatas.Enqueue(
                item: new(
                    url: url,
                    headers: headers,
                    method: method,
                    json: json,
                    requestCompletedHandler: requestCompletedHandler
                )
            );
        }

        private struct HttpRequestData
        {
            public string Url = string.Empty;
            public string[] Headers = null;
            public HttpClient.Method Method = HttpClient.Method.Get;
            public string Json = string.Empty;
            public HttpRequestCompletedHandler RequestCompletedHandler = null;

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
    }
}