using SC4PropTextureCatalogAPI.Controllers;
using SC4PropTextureCatalogAPI.Models;
using SQLite;


var builder = WebApplication.CreateBuilder(args);


string dbPath = Path.Combine(builder.Environment.ContentRootPath, "Data", "Catalog.db");

// Add services to the container.
builder.Services.AddSingleton(sp => {
    var conn = new SQLiteAsyncConnection(dbPath);
    //conn.CreateTableAsync<Item>().Wait();
    return conn;
});
builder.Services.AddScoped<IItemRepository, SqliteItemRepository>();
builder.Services.AddControllers();
builder.Services.AddCors(o => o.AddPolicy("AllowAll", pb => pb.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDefaultFiles();
app.UseStaticFiles();


app.UseHttpsRedirection();
app.UseCors("AllowAll");

//app.UseAuthorization();

app.MapControllers();

app.Run();
