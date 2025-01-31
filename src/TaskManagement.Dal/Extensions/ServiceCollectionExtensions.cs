using HomeworkApp.Dal.Infrastructure;
using HomeworkApp.Dal.Providers;
using HomeworkApp.Dal.Providers.Interfaces;
using HomeworkApp.Dal.Repositories;
using HomeworkApp.Dal.Repositories.Interfaces;
using HomeworkApp.Dal.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HomeworkApp.Dal.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDalRepositories(
        this IServiceCollection services)
    {
        AddPostgresRepositories(services);
        AddRedisRepositories(services);
        
        return services;
    }

    private static void AddRedisRepositories(IServiceCollection services)
    {
        services.AddScoped<ITakenTaskRepository, TakenTaskRepository>();
        services.AddScoped<IUserScheduleRepository, UserScheduleRepository>();
        services.AddScoped<IRateLimitRepository, RateLimitRepository>();
    }

    private static void AddPostgresRepositories(IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<ITaskLogRepository, TaskLogRepository>();
        services.AddScoped<ITaskCommentRepository, TaskCommentRepository>();
    }
    
    private static void AddProviders(IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
    }


    public static IServiceCollection AddDalInfrastructure(
        this IServiceCollection services, 
        IConfigurationRoot config)
    {
        //read config
        services.Configure<DalOptions>(config.GetSection(nameof(DalOptions)));
        
        //add providers
        AddProviders(services);

        //configure postrges types
        Postgres.MapCompositeTypes();
        
        //add migrations
        Postgres.AddMigrations(services);
        
        return services;

    }
}