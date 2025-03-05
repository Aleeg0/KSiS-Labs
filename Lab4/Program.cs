using Lab4;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions()
{
    WebRootPath = "storage"
});

var app = builder.Build();
Directory.CreateDirectory("storage");
string storageRoot = Path.Combine(Directory.GetCurrentDirectory(), "storage");
StorageService storageService = new StorageService(storageRoot);

app.MapGet("/{**path}", (string? path) => storageService.Get(path));
app.MapPut("/{**path}", async (HttpContext context, string path) => await storageService.Put(context, path));
app.MapDelete("/{**path}", (string path) => storageService.Delete(path));
app.MapMethods("/{**path}", new []{"HEAD"}, (HttpContext context, string path) => storageService.Head(context, path));

app.Run();