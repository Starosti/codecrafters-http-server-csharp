using System.Text;

namespace codecrafters_http_server.src.classes
{
    internal class HTTPPayload
    {
        public string Version { get; set; }
        public List<KeyValuePair<string, string>> Headers { get; set; }
        public byte[] Body { get; set; }

        public HTTPPayload()
        {
            Version = "HTTP/1.1";
            Headers = new List<KeyValuePair<string, string>>();
            Body = new byte[0];
        }

        public virtual string Serialize()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var header in Headers)
            {
                sb.Append($"{header.Key}: {header.Value}\r\n");
            }
            sb.Append("\r\n");
            if (Body != null)
                sb.Append(Encoding.Unicode.GetString(Body));
            return sb.ToString();
        }
        public void AddHeader(string header)
        {
            string[] parts = header.Split(": ");
            AddHeader(parts[0], parts[1]);
        }

        public void AddHeader(string key, string value)
        {
            if (Headers.Exists(h => h.Key == key))
            {
                Headers.RemoveAll(h => h.Key == key);
            }
            Headers.Add(new KeyValuePair<string, string>(key, value));
        }

        public override string ToString()
        {
            return Serialize();
        }
    }
}
