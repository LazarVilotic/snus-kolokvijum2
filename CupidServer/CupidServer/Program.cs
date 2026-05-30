using CupidServer.Services;
using CupidServer.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<CupidService>();
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.MapHub<CupidHub>("/cupidHub");

app.Run();