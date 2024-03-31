using System.Text;

namespace codecrafters_http_server.src.classes
{
    internal class HTTPResponse : HTTPPayload
    {
        public string StatusCode { get; set; }
        public string ReasonPhrase { get; set; }

        public HTTPResponse()
        {
            Version = "HTTP/1.1";
            StatusCode = "404";
            ReasonPhrase = "Not Found";
        }

        public override string Serialize()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{Version} {StatusCode} {ReasonPhrase}\r\n");
            return sb.Append(base.Serialize()).ToString();
        }
    }
}
