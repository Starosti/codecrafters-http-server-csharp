using System.Text;

namespace codecrafters_http_server.src.classes
{
    internal class HTTPFileResponse : HTTPResponse
    {
        public HTTPFileResponse(string path)
        {
            Body = File.ReadAllBytes(path);
            Version = "HTTP/1.1";
            StatusCode = "200";
            ReasonPhrase = "OK";
            Headers.Add(new KeyValuePair<string, string>("Content-Type", "application/octet-stream"));
            Headers.Add(new KeyValuePair<string, string>("Content-Length", Body.Length.ToString()));
        }

        public override string Serialize()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{Version} {StatusCode} {ReasonPhrase}\r\n");
            foreach (var header in Headers)
            {
                sb.Append($"{header.Key}: {header.Value}\r\n");
            }
            sb.Append("\r\n");
            return sb.ToString();
        }
    }
}
