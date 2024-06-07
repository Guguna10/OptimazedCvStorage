using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Serialization;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using OptimazedCvStorage.Data;
using dotenv.net;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        //Enable Cors
        services.AddCors(c =>
        {
            c.AddPolicy("allowOrigin", options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
        });

        services.AddControllersWithViews().AddNewtonsoftJson(options =>
        {
            options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            options.SerializerSettings.ContractResolver = new DefaultContractResolver();
        });
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        
        // Configure DbContext
        services.AddDbContext<CVContext>(options =>
            options.UseMySql(GetConnectionString("OptimazedCvStorage"),
                new MySqlServerVersion(new System.Version(8, 0, 23))));
        var OptimazedCvStorage = Environment.GetEnvironmentVariable("OptimazedCvStorage");
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        //Enable Cors
        app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }

    private string GetConnectionString(string name)
    {
        return Configuration.GetConnectionString(name);
    }
}
