// See https://aka.ms/new-console-template for more information


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var app = builder.Build();


Console.WriteLine("Hello, World!");
app.Run("http://+:5380");