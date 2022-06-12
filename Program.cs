using UdpWebApi;
using UdpWebApi.Registrations;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "*",
        policy  =>
        {
            policy.WithOrigins("*",
                    "http://www.contoso.com")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowAnyOrigin();
        });
});
builder.Services.AddSwaggerGen();
//builder.Services.AddSignalR();
builder.Services.AddSingleton<UdpListener>(new UdpListener());
builder.Services.AddScoped<IManager, Manager>();
    
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("*");
//app.MapGet("/", () => "Hello World!");
// app.MapHub<ServerHub>("/service");
app.UseStaticFiles();
app.MapControllers();
app.Run();