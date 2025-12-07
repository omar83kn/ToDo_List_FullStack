using Microsoft.EntityFrameworkCore;
using ToDo_List.Data;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Dev-only Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// serve wwwroot/index.html
app.UseDefaultFiles();   // looks for index.html in wwwroot
app.UseStaticFiles();    // enable static files

app.UseAuthorization();

app.MapControllers();

app.Run();
