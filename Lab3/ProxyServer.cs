using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Lab3;

public class ProxyServer
{
    private readonly Blocker _blocker;
    private const int Port = 8080;
    private readonly TcpListener _tcpListener;

    public ProxyServer()
    {
        _blocker = new Blocker();
        _blocker.Add("blocked.com");
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
        using (client)
        using (NetworkStream clientStream = client.GetStream())
        using (StreamReader clientReader = new StreamReader(clientStream))
        using (StreamWriter clientWriter = new StreamWriter(clientStream))
        {
            clientWriter.AutoFlush = true;
            
            // получаем header от браузера
            string? header = await clientReader.ReadLineAsync();
            
            // проверка на пустоту
            if (string.IsNullOrEmpty(header)) return;
            
            // распарсиваем заголовок на куски
            string[] requestParams = header.Split(' ');
            
            // проверяем на количество параметров
            if (requestParams.Length < 3) return;
            
            // формируем параметры
            string method = requestParams[0];
            string requestUrl = requestParams[1];
            string httpVersion = requestParams[2];
            
            // создаем полный Uri
            if (!Uri.TryCreate(requestUrl, UriKind.Absolute, out Uri uri)) return;
            
            string host = uri.Host;
            if (string.IsNullOrEmpty(host)) return;
            // если в черном списке
            if (_blocker.IsBlocked(host))
            {
                // узнаем длину в байтах
                int contentLength = Encoding.UTF8.GetByteCount(_blocker.ResponseBody);
                // отправляем заголовок
                await clientWriter.WriteLineAsync("HTTP/1.1 403 Forbidden");
                await clientWriter.WriteLineAsync("Content-Type: text/html; charset=utf-8");
                await clientWriter.WriteLineAsync($"Content-Length: {contentLength}");
                await clientWriter.WriteLineAsync();
                await clientWriter.WriteLineAsync(_blocker.ResponseBody);
                // отправляем тело
                Console.WriteLine($"Блокирован доступ к {requestUrl}");
                return;
            }
                
            // получаем остальную информацию 
            int port = uri.Port;
            string path = uri.PathAndQuery;
            
            try
            {
                using (TcpClient server = new TcpClient())
                {
                    // подключаемся к удаленном серверу
                    await server.ConnectAsync(host, port);

                    using (NetworkStream serverStream = server.GetStream())
                    using (StreamReader serverReader = new StreamReader(serverStream))
                    using (StreamWriter serverWriter = new StreamWriter(serverStream))
                    {
                        serverWriter.AutoFlush = true;
                        // отправляем заголовок запроса
                        await serverWriter.WriteLineAsync($"{method} {path} {httpVersion}");
                        
                        // получаем остальную часть от клиента и передаем сразу серверу
                        string? line = "";
                        while (!string.IsNullOrEmpty(line = await clientReader.ReadLineAsync()))
                        {
                            await serverWriter.WriteLineAsync(line);
                        }
                        // записываем пустую строчку для конца запроса
                        await serverWriter.WriteLineAsync();
                        
                        // получаем заголовок от сервера
                        string? responseLine = await serverReader.ReadLineAsync();
                        // отправляем клиенту
                        await clientWriter.WriteLineAsync(responseLine);
                        
                        Console.WriteLine($"[{method}] {uri} {responseLine}");
                        
                        if (string.IsNullOrEmpty(responseLine)) return;
                        
                        while (!string.IsNullOrEmpty(responseLine = await serverReader.ReadLineAsync()))
                        {
                            await clientWriter.WriteLineAsync(responseLine);
                        }
                        
                        await clientWriter.WriteLineAsync();

                        // используем потом для прочтения тела
                        byte[] buffer = new byte[8192];
                        int bytesRead;
                        while ((bytesRead = await serverStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await clientStream.WriteAsync(buffer, 0, bytesRead);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}