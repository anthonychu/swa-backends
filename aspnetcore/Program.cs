using SwaBackend.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddAuthentication("AzureStaticWebApps")
    .AddScheme<StaticWebAppsAuthenticationSchemeOptions, StaticWebAppsAuthenticationHandler>(
        "AzureStaticWebApps", options => { });
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UsePathBase("/api");
app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();
