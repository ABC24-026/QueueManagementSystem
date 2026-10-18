using Microsoft.EntityFrameworkCore;
using QueueManagementSystem.Data;
using QueueManagementSystem.Ml;
using QueueManagementSystem.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=queue.db"));

builder.Services.AddSingleton<IWaitPredictor, MlWaitPredictor>();
builder.Services.AddScoped<WaitingTimeService>();
builder.Services.AddScoped<ISmsSender, LoggingSmsSender>();
builder.Services.AddScoped<QueueService>();
builder.Services.AddScoped<StaffPinFilter>();
builder.Services.AddHostedService<QueueMonitorService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();

app.Run();