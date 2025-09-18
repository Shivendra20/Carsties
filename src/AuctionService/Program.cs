using AuctionService.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddDbContext<AuctionDBContext>(opt =>
{ 
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddOpenApi();
var app = builder.Build();
app.UseAuthorization();
app.MapControllers();

try
{
  DbInitializer.InitDb(app);
}
catch(Exception ex)
{
    throw;
}

app.Run();
