using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Lab3;

public class ProxyServer
{
    private readonly Blocker _blocker;
    private const int Port = 8052;
    private readonly TcpListener _tcpListener;

    public ProxyServer()
    {
        _blocker = new Blocker();
        _tcpListener = new TcpListener(new IPEndPoint(IPAddress.Any, Port));
    }

    public async Task Start()
    {
        Console.WriteLine($"Starting proxy server on {Port}");
        _tcpListener.Start();
        try
        {
            while (true)
            {
                TcpClient tcpClient = await _tcpListener.AcceptTcpClientAsync();
                _ = HandleClient(tcpClient);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        finally
        {
            _tcpListener.Stop();
            _tcpListener.Dispose();
        }
    }

    private async Task HandleClient(TcpClient client)
    {
        
        NetworkStream clientStream = client.GetStream();
        
        // получаем запрос в виде строк
        string[] requestLines = await RequestLinesAsync(clientStream);

        if (requestLines.Length < 1)
            return;
        
        // получаем данные заголовка
        string[] requestParams = requestLines[0].Split(" ");
        
        // проверяем на количество параметров
        if (requestParams.Length < 3) return;
        
        // формируем параметры
        string method = requestParams[0];
        string requestUrl = requestParams[1];
        string httpVersion = requestParams[2];
        
        // создаем полный Uri
        if (!Uri.TryCreate(requestUrl, UriKind.Absolute, out Uri? url)) return;
        
        string host = url.Host;
        if (string.IsNullOrEmpty(host)) return;
        // если в черном списке
        if (_blocker.IsBlocked(host))
        {
            await SendBlockMessageAsync(clientStream);
            // отправляем тело
            Console.WriteLine($"Блокирован доступ к {requestUrl}");
            return;
        }
            
        // получаем остальную информацию запроса
        int port = url.Port;
        string path = url.PathAndQuery;
        TcpClient server = new TcpClient();

        try
        {

            await server.ConnectAsync(host, port);
            NetworkStream serverStream = server.GetStream();
            serverStream.ReadTimeout = 1000;

            // производим замену длинного url на короткий
            requestLines[0] = $"{method} {path} {httpVersion}";
            // отправляем запрос
            await SendRequestAsync(serverStream, requestLines);
            server.Client.Shutdown(SocketShutdown.Send);

            // получаем первую строку 
            byte[] statusLineBytes = await ReceiveStatusLineAsync(serverStream);
            string statusLine = Encoding.UTF8.GetString(statusLineBytes);

            Console.WriteLine($"[{method}] {url} - {statusLine}");

            // отправляем строку пользователю 
            await clientStream.WriteAsync(statusLineBytes, 0, statusLineBytes.Length);
            
            // переводим остальные строки
            await TransmitRequestAsync(serverStream, clientStream);

        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}");
        }
        finally
        {
            server.Client.Shutdown(SocketShutdown.Both);
            server.Close();
            server.Dispose();
            
            client.Close();
            client.Dispose();
        }
    }

    private async Task<string[]> RequestLinesAsync(NetworkStream stream)
    {
        byte[] buffer = new byte[2048];
        int bytes = 0;
        // получаем header от браузера
        bytes = await stream.ReadAsync(buffer, 0, buffer.Length);
            
        // распарсиваем заголовок на куски
        string requestText = Encoding.UTF8.GetString(buffer, 0, bytes);
        string[] requestLines = requestText.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
        return requestLines;
    }

    private async Task SendBlockMessageAsync(NetworkStream stream)
    {
        // получаем длину тела в байтах
        int contentLength = Encoding.UTF8.GetByteCount(_blocker.ResponseBody);
        // формируем ответ
        string response = "HTTP/1.1 403 Forbidden\r\n" +
                          "Content-Type: text/html; charset=utf-8\r\n" +
                          $"Content-Length: {contentLength}\r\n" +
                          "\r\n" +
                          _blocker.ResponseBody;
        
        // кодируем в байты ответ
        byte[] buffer = Encoding.UTF8.GetBytes(response);
        
        // отправляем ответ клиенту
        await stream.WriteAsync(buffer, 0 , buffer.Length);
    }

    private async Task SendRequestAsync(NetworkStream stream, string[] requestLines)
    {
        foreach (var requestLine in requestLines)
        {
            byte[] headerBytes = Encoding.UTF8.GetBytes(requestLine + "\r\n");
            await stream.WriteAsync(headerBytes, 0, headerBytes.Length);
        }
        await stream.WriteAsync(Encoding.UTF8.GetBytes("\r\n"), 0, 2);
    }

    private async Task TransmitRequestAsync(NetworkStream serverStream, NetworkStream clientStream)
    {
        byte[] buffer = new byte[8192];
        int bytesRead;
        
        // Читаем ответ от сервера и отправляем клиенту
        while ((bytesRead = await serverStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            await clientStream.WriteAsync(buffer, 0, bytesRead);
        }
    }


    private async Task<byte[]> ReceiveStatusLineAsync(NetworkStream serverStream)
    {
        byte[] buffer = new byte[2];
        
        List<byte> responseLineBytes = new List<byte>(); // Сюда собираем первую строку
        bool isFirstLineRead = false;

        // Читаем первую строку вручную (до \r\n)
        while (!isFirstLineRead && await serverStream.ReadAsync(buffer, 0, 1) > 0)
        {
            responseLineBytes.Add(buffer[0]);

            // Проверяем конец строки (\r\n)
            if (responseLineBytes.Count >= 2 && 
                responseLineBytes[^2] == '\r' && 
                responseLineBytes[^1] == '\n')
            {
                isFirstLineRead = true;
            }
        }

        // Преобразуем байты первой строки ответа в строку
        return responseLineBytes.ToArray();
    }
}