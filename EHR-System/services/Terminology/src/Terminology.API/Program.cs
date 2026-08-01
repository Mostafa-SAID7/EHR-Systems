using EHRPlatform.Services.Terminology.Application;

var builder = WebApplicationBuilder.CreateBuilder(args);

// Add services to the container
builder.Services
    .AddTerminologyApplicationServices()
    .AddSwaggerGen();

builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();
