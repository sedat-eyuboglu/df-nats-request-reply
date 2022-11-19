var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddSingleton<NatsService>();

var app = builder.Build();
app.MapControllers();
app.Run();