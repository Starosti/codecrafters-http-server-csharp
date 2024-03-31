using System.Text;

namespace codecrafters_http_server.src.classes
{
    internal class HTTPRequest : HTTPPayload
    {
        public string Method { get; set; }
        public string Path { get; set; }

        public HTTPRequest()
        {
            Method = "GET";
            Path = "/";
            Version = "HTTP/1.1";
        }

        public override string Serialize()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{Method} {Path} {Version}\r\n");
            return sb.Append(base.Serialize()).ToString();
        }

        public void ParseStartLine(string startLine)
        {
            string[] parts = startLine.Split(' ');
            if (parts.Length != 3)
            {
                throw new Exception("Invalid start line");
            }
            Method = parts[0];
            Path = parts[1];
            Version = parts[2];
        }
    }
}
