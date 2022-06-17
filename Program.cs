using System.Net.NetworkInformation;
using Extensions;
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
UdpListener.GetInstance.remoteHost =  builder.Configuration.GetValue<string>("host");
UdpListener.GetInstance.centerHost =  builder.Configuration.GetValue<string>("center");
UdpListener.GetInstance.localHost =  builder.Configuration.GetValue<string>("local");

UdpListener.GetInstance.serverType=  builder.Configuration.GetValue<string>("serverType");
UdpListener.GetInstance.localHost =  builder.Configuration.GetValue<string>("local");
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