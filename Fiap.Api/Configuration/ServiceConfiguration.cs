using Fiap.Api.Interfaces;
using Fiap.Domain.Repositories;
using Fiap.Infra.Context;
using Microsoft.EntityFrameworkCore;

namespace Fiap.Api.Configuration
{
    public static class ServiceConfiguration
    {
        public static void ConfigureServices(WebApplicationBuilder builder)
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<FiapDataContext>(options => options.UseNpgsql(connectionString));
            builder.Services.AddScoped<IContatoRepository, ContatoRepository>();
        }

    }
}
