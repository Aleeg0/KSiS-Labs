namespace Lab4;

class StorageService
{
    private readonly string _storageRoot;
    
    public StorageService(string storageRoot)
    {
        _storageRoot = storageRoot;
    }

    public IResult Get(string? filePath)
    {
        try
        {
            string? fullPath = Path.Combine(_storageRoot, filePath ?? "");
            // если был запрос на директорию
            if (Directory.Exists(fullPath))
            {
                // обработка файлов
                var files = Directory.GetFiles(fullPath) // получаем абсолютные пути
                    .Select(Path.GetFileName) // получаем только имена файлов 
                    .ToArray();
            
                // обработка директорий
                var directories = Directory.GetDirectories(fullPath) // получаем абсолютные пути
                    .Select(Path.GetFileName) // Получаем имена папок
                    .ToArray();
            
                // отправка в виде JSON
                return Results.Ok(new { directories, files });
            }
        
            // если файл не был найден 404
            if (!File.Exists(fullPath))
            {
                return Results.NotFound();
            }
        
            // обработка файла в виде буфера данных
            return Results.File(fullPath, "application/octet-stream");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return Results.InternalServerError(new {message = e.Message});
        }
    }

    public async Task<IResult> Put(HttpContext context, string filePath)
    {
        try
        {
            // создание пути
            string? fullPath = Path.Combine(_storageRoot, filePath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

            // если запрос на копирование
            if (context.Request.Headers.TryGetValue("X-Copy-From", out var origin))
            {
                string? sourcePath = Path.Combine(_storageRoot, origin!);
            
                // если файл-источник не найден 
                if (!File.Exists(sourcePath))
                {
                    return Results.NotFound(new {message = "source file not found"});
                }
            
                // если файл-источник найден, то копируем
                File.Copy(sourcePath, fullPath, true);
                return Results.Ok(new {message = "File uploaded successfully"});
            }
            
            // если запрос на запись
            
            using FileStream fileStream = new FileStream(fullPath, FileMode.Create);
            
            // переписываем из тела в хранилище сервера
            await context.Request.Body.CopyToAsync(fileStream);
            
            return Results.Created($"/{filePath}", new { message = "File created successfully" });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return Results.InternalServerError(new {message = e.Message});
        }
    }

    public IResult Delete(string filePath)
    {
        try
        {
            // создание пути
            string? fullPath = Path.Combine(_storageRoot, filePath);

            // если файл
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                return Results.Ok(new { message = "File deleted successfully"});
            }

            // если директория
            if (Directory.Exists(fullPath))
            {
                Directory.Delete(fullPath, true);
                return Results.Ok(new { message = "Directory deleted successfully"});
            }
            
            // если файл или директория не были найдены
            return Results.NotFound();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return Results.InternalServerError(new {message = e.Message});
        }
    }

    public IResult Head(HttpContext context, string path)
    {
        string fullPath = Path.Combine(_storageRoot, path);
        // если файла/директории не существует
        if (!File.Exists(fullPath))
            return Results.NotFound();

        // если существует получаем информацию о файле
        var fileInfo = new FileInfo(fullPath);
        // записываем в заголов
        context.Response.Headers["Content-Length"] = fileInfo.Length.ToString();
        context.Response.Headers["Last-Modified"] = fileInfo.LastWriteTimeUtc.ToString("R");

        return Results.Ok();
    }
}