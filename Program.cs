using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using myProducts.Models;
using myProducts.Services;
using System.Text;
using Serilog;
using Serilog.Sinks.MSSqlServer;
using Log = Serilog.Log;

var builder = WebApplication.CreateBuilder(args);

var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key não foi configurada");

var key = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            //Realiza a leitura do token no cookie.
            OnMessageReceived = context =>
            {
                var token = context.Request.Cookies["AuthToken"];
                if (!string.IsNullOrEmpty(token))
                {
                    context.Token = token;
                }
                return Task.CompletedTask;
            },
            //Evento disparado quando o usuário não possui um token válido (substituindo o evento padrão 401 unauthorized).
            OnChallenge = context =>
            {
                context.HandleResponse();

                context.Response.Redirect("/Account/Login");
                return Task.CompletedTask;
            },
            //Esse evento dispara quando o usuário possui token válido, porém não tem permissão para acessar determinado recurso.
            OnForbidden = context =>
            {
                context.Response.Redirect("/Account/Login");
                return Task.CompletedTask;
            }
        };
    });


builder.Services.AddAuthorization();

// Add services to the container.
builder.Services.AddRazorPages();

var connectionString = Environment.GetEnvironmentVariable("DefaultConnection");

builder.Services.AddDbContext<MyproductsContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddScoped<UserSessionService>();

//Padrão de template definido para ser exibido no console.
var outputTemplate = "{Timestamp:dd-MM-yyyy HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}";

//Configurando o Serilog.
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore.Hosting.Diagnostics", Serilog.Events.LogEventLevel.Error)
    .MinimumLevel.Override("Microsoft.AspNetCore.StaticFiles", Serilog.Events.LogEventLevel.Error)

    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "MyProducts")

    .WriteTo.Console(restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Debug, outputTemplate: outputTemplate)

    .Filter.ByExcluding(logEvent =>
    {
        if (logEvent.Properties.TryGetValue("RequestPath", out var pathValue))
        {
            var path = pathValue.ToString().Trim('"').ToLower();
            if (path.StartsWith("/css") ||
                path.StartsWith("/js") ||
                path.StartsWith("/images") ||
                path.StartsWith("/fonts") ||
                path.Contains("favicon.ico"))
            {
                return true;
            }
        }

        if (logEvent.Properties.TryGetValue("SourceContext", out var sourceValue))
        {
            var source = sourceValue.ToString();
            if (source.Contains("Microsoft.AspNetCore.StaticFiles") ||
                source.Contains("Microsoft.AspNetCore.Hosting.Diagnostics"))
            {
                return true;
            }
        }

        return false;
    })

    .WriteTo.MSSqlServer(
        connectionString: Environment.GetEnvironmentVariable("DefaultConnection"),
        sinkOptions: new MSSqlServerSinkOptions
        {
            TableName = "LOGS",
            AutoCreateSqlTable = false //Não cria a tabela automaticamente, pois a tabela já foi criada no BD.
        },
        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Warning
    )

    .WriteTo.Logger(lc => lc
    .Filter.ByIncludingOnly(evt =>
    {
        if (evt.Properties.TryGetValue("SourceContext", out var src))
        {
            var source = src is Serilog.Events.ScalarValue scalar
                ? scalar.Value?.ToString()
                : src.ToString();

            return !string.IsNullOrEmpty(source) && (source.StartsWith("myProducts.Services") || source.StartsWith("myProducts.Pages"));
        }
        return false;
    })

    .WriteTo.MSSqlServer(
        connectionString: Environment.GetEnvironmentVariable("DefaultConnection"),
        sinkOptions: new MSSqlServerSinkOptions
        {
            TableName = "LOGS",
            AutoCreateSqlTable = false
        },
        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information
        )   
    )
    .CreateLogger();

builder.Host.UseSerilog();

var app = builder.Build();

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Erro não tratado na requisição {Path}", context.Request.Path);
        throw;
    }
});

app.UseSerilogRequestLogging(options =>
{
    options.GetLevel = (httpContext, elapsed, ex) =>
    {
        var path = httpContext.Request.Path;

        if (path.StartsWithSegments("/css") ||
            path.StartsWithSegments("/js") ||
            path.StartsWithSegments("/images") ||
            path.StartsWithSegments("/fonts") ||
            path.StartsWithSegments("/favicon.ico"))
        {
            return Serilog.Events.LogEventLevel.Debug;
        }

        return ex != null ? Serilog.Events.LogEventLevel.Error : Serilog.Events.LogEventLevel.Information;
    };
});

//Redireciona o usuário para a página /Error/Index quando a aplicação apresenta alguma exceção ou status code em produção
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseStatusCodePagesWithReExecute("/Error", "?statusCode={0}");
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();