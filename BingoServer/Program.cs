var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();
var app = builder.Build();

app.MapGet("/", () => "Bingo Server Working");
app.MapHub<BingoHub>("/bingo");

app.Run();