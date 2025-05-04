
using AlgorandAuthenticationV2;
using AVMGasStation.BusinessControllers;
using AVMGasStation.Model;
using MessagePack.AspNetCoreMvcFormatter;

namespace AVMGasStation
{
  public class Program
  {
    public static void Main(string[] args)
    {
      var builder = WebApplication.CreateBuilder(args);

      // Add services to the container.

      builder.Services.AddControllers().AddNewtonsoftJson();
      // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
      builder.Services.AddEndpointsApiExplorer();
      builder.Services.AddSwaggerGen();

      builder.Services.Configure<AlgorandAuthenticationOptionsV2>(builder.Configuration.GetSection("AlgorandAuthentication"));
      builder.Services.Configure<GasStationConfiguration>(builder.Configuration.GetSection("GasStation"));
      builder.Services.AddSingleton<GasStation>();

      var authOptions = builder.Configuration.GetSection("AlgorandAuthentication").Get<AlgorandAuthenticationOptionsV2>();
      if (authOptions == null) throw new Exception("Config for the authentication is missing");
      builder.Services.AddAuthentication(AlgorandAuthenticationHandlerV2.ID).AddAlgorand(a =>
      {
        a.Realm = authOptions.Realm;
        a.CheckExpiration = authOptions.CheckExpiration;
        a.EmptySuccessOnFailure = authOptions.EmptySuccessOnFailure;
        a.AllowedNetworks = authOptions.AllowedNetworks;
        a.Debug = authOptions.Debug;
      });

      builder.Services.AddProblemDetails();


      // Add CORS policy
      var corsConfig = builder.Configuration.GetSection("Cors").AsEnumerable().Select(k => k.Value).Where(k => !string.IsNullOrEmpty(k)).ToArray();
      if (!(corsConfig?.Length > 0)) throw new Exception("Cors not defined");

      builder.Services.AddCors(options =>
      {
        options.AddDefaultPolicy(
        builder =>
        {
          builder.WithOrigins(corsConfig)
                          .SetIsOriginAllowedToAllowWildcardSubdomains()
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials()
                          .WithExposedHeaders("rowcount", "rowstate");
        });
      });

      var app = builder.Build();

      app.UseSwagger();
      app.UseSwaggerUI();
      app.UseCors();

      app.UseHttpsRedirection();

      app.UseAuthentication();
      app.UseAuthorization();


      app.MapControllers();

      app.Services.GetService<GasStation>();

      app.Run();
    }
  }
}
