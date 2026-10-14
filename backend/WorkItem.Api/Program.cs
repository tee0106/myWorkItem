using WorkItem.Api.Data;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddSingleton<SqliteDatabase>();
builder.Services.AddSingleton<WorkItemRepository>();

var app = builder.Build();
app.Services.GetRequiredService<SqliteDatabase>().Initialize();
app.MapControllers();
app.Run();

public partial class Program;
