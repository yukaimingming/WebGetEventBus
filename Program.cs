using Serilog;
using Serilog.Events;
using WebGetEventBus.common;

//serilog初始化设置
Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().
    MinimumLevel.Override("Default", LogEventLevel.Information).
    MinimumLevel.Override("Microsoft", LogEventLevel.Error).MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information).MinimumLevel.Override("Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware", LogEventLevel.Information)
.MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Information)
.WriteTo.Console()
.WriteTo.File("").CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//首先初始化 Serilog 的缺点是，来自 ASP.NET Core 主机的服务（包括配置和依赖项注入）尚不可用。appsettings.json
//为了解决这个问题，Serilog 支持两阶段初始化。初始“引导”记录器在程序启动时立即配置，一旦主机加载，该记录器将被完全配置的记录器取代。
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File($"Logs/.log", rollingInterval: RollingInterval.Day, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} || {Level} || {SourceContext:l} || {Message} || {Exception} ||end {NewLine}"));

//绑定真实IP和443端口
//builder.WebHost.ConfigureKestrel((context, options) =>
//{
//    options.ListenAnyIP(443, listenOptions =>
//    {
//        listenOptions.Protocols = HttpProtocols.Http1AndHttp2AndHttp3;
//        listenOptions.UseHttps();
//        listenOptions.UseConnectionLogging();
//    });

//    options.ListenAnyIP(5001, listenOptions =>
//    {
//        listenOptions.Protocols = HttpProtocols.Http1;
//        listenOptions.UseHttps();
//        listenOptions.UseConnectionLogging();
//    });
//});

Log.Information("Starting web application!!!!");

//服务依赖注册
builder.Services.AddScoped<IGetxml, strBuder>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSwagger();
app.UseSwaggerUI();

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
