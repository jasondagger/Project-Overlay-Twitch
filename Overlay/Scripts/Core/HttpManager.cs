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
            HttpRequestData httpRequestData = m_httpRequestDatas.Dequeue();

            HttpRequest httpRequest = new();
            AddChild(
                httpRequest    
            );
            httpRequest.RequestCompleted += httpRequestData.requestCompletedHandler;
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
                httpRequestData.url,
                httpRequestData.headers,
                httpRequestData.method,
                httpRequestData.json
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
            new HttpRequestData(
                url,
                headers,
                method,
                json,
                requestCompletedHandler
            )
        );
    }

    private struct HttpRequestData
    {
        public string url;
        public string[] headers;
        public HttpClient.Method method;
        public string json;
        public HttpRequestCompletedHandler requestCompletedHandler;

        public HttpRequestData(
            string url,
            string[] headers,
            HttpClient.Method method,
            string json,
            HttpRequestCompletedHandler requestCompletedHandler
        )
        {
            this.url = url;
            this.headers = headers;
            this.method = method;
            this.json = json;
            this.requestCompletedHandler = requestCompletedHandler;
        }
    }

    private Queue<HttpRequestData> m_httpRequestDatas = new();
}