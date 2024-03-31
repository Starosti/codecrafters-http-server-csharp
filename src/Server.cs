using codecrafters_http_server.src.classes;
using System.Net;
using System.Net.Sockets;
using System.Text;

TcpListener server = new TcpListener(IPAddress.Any, 4221);
server.Start();
Console.WriteLine("Server started");

string directory = "";

if (args.Length > 0)
{
    Console.WriteLine("Parsing arguments");
    if (args.Contains("--directory"))
    {
        int directoryIndex = Array.IndexOf(args, "--directory");
        directory = args[directoryIndex + 1];
        Console.WriteLine("Directory: {0}", directory);
    }
}

while (true)
{
    try
    {
        Socket client = server.AcceptSocket();
        Console.WriteLine("Client connected");

        _ = Task.Run(() => handleConnection(client));
    }
    catch (Exception ex)
    {
        Console.WriteLine("Exception: {0}", ex);
    }
}

async void handleConnection(Socket client)
{
    try
    {
        NetworkStream stream = new NetworkStream(client);
        StreamReader reader = new StreamReader(stream);
        StreamWriter writer = new StreamWriter(stream);

        HTTPRequest request = new HTTPRequest();

        ParseStartLine(reader, request);
        Console.WriteLine("Start line parsed");

        ParseHeaders(reader, request);
        Console.WriteLine("Headers parsed");

        if (request.Method == "POST")
        {
            Console.WriteLine("POST received, parsing body");
            ParseBody(reader, request);
            Console.WriteLine("Body parsed");
        }

        Console.WriteLine("Request parsed, crafting response");

        HTTPResponse response = new HTTPResponse();

        response.Version = "HTTP/1.1";
        response.StatusCode = "200";
        response.ReasonPhrase = "OK";

        switch (request.Path)
        {
            case "/":
                response.AddHeader("Content-Type", "text/html");
                response.Body = Encoding.Unicode.GetBytes("Hello, World!");
                break;

            case "/user-agent":
                string userAgent = request.Headers.Find(h => h.Key == "User-Agent").Value;
                if (string.IsNullOrEmpty(userAgent))
                {
                    response.StatusCode = "400";
                    response.ReasonPhrase = "Bad Request";
                    break;
                }
                response.AddHeader("Content-Type", "text/plain");
                response.AddHeader("Content-Length", userAgent.Length.ToString());
                response.Body = Encoding.Unicode.GetBytes(userAgent);
                break;

            default:
                response.StatusCode = "404";
                response.ReasonPhrase = "Not Found";
                break;
        }

        if (request.Path.StartsWith("/echo/"))
        {
            response.StatusCode = "200";
            response.ReasonPhrase = "OK";
            string body = request.Path.Substring(6);
            response.AddHeader("Content-Type", "text/plain");
            response.AddHeader("Content-Length", body.Length.ToString());
            response.Body = Encoding.Unicode.GetBytes(body);
        }

        if (request.Path.StartsWith("/files/"))
        {
            string filePath = request.Path.Substring(7);
            string fullPath = Path.Combine(directory, filePath);

            if (request.Method == "GET")
            {
                Console.WriteLine("Searching for file {0}", fullPath);

                if (!File.Exists(fullPath))
                {
                    response.StatusCode = "404";
                    response.ReasonPhrase = "Not Found";
                }
                else
                {
                    response = new HTTPFileResponse(fullPath);
                }
            }
            else if (request.Method == "POST")
            {
                Console.WriteLine("Saving file {0}", fullPath);
                File.WriteAllBytes(fullPath, request.Body);
                response.StatusCode = "201";
                response.ReasonPhrase = "Created";
            }
            else
            {
                response.StatusCode = "405";
                response.ReasonPhrase = "Method Not Allowed";
            }
        }
        writer.Write(response);
        writer.Flush();
        if (response is HTTPFileResponse fileResponse)
        {
            await stream.WriteAsync(fileResponse.Body);
        }
        Console.WriteLine("Response sent");
        client.Close();
    }
    catch (Exception ex)
    {
        Console.WriteLine("Exception: {0}", ex);
        client.Close();
    }
}

void ParseBody(StreamReader reader, HTTPRequest request)
{
    int contentLength = int.Parse(request.Headers.Find(h => h.Key == "Content-Length").Value);
    char[] buffer = new char[contentLength];
    reader.Read(buffer, 0, contentLength);
    request.Body = Encoding.UTF8.GetBytes(new string(buffer));
}

void ParseHeaders(StreamReader reader, HTTPRequest request)
{
    while (true)
    {
        string? header = reader.ReadLine();
        if (string.IsNullOrEmpty(header))
        {
            break;
        }
        request.AddHeader(header);
    }
}

void ParseStartLine(StreamReader reader, HTTPRequest request)
{
    while (true)
    {
        string? startLine = reader.ReadLine();
        if (string.IsNullOrEmpty(startLine))
        {
            continue;
        }
        request.ParseStartLine(startLine);
        if (String.IsNullOrEmpty(request.Method) || String.IsNullOrEmpty(request.Path) || String.IsNullOrEmpty(request.Version))
        {
            throw new Exception("Invalid start line");
        }
        else
        {
            break;
        }
    }
}