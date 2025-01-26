using FluentMigrator.Runner;
using HomeworkApp.Dal.Extensions;
using HomeworkApp.Dal.Providers.Interfaces;
using HomeworkApp.Dal.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Moq;

namespace HomeworkApp.IntegrationTests.Fixtures
{
    public class TestFixture
    {
        public IUserRepository UserRepository { get; }
        
        public ITaskRepository TaskRepository { get; }
        
        public ITaskLogRepository TaskLogRepository { get; }
        
        public ITakenTaskRepository TakenTaskRepository { get; }
        
        public IUserScheduleRepository UserScheduleRepository { get; }
        
        public ITaskCommentRepository TaskCommentRepository { get; }
        
        public IRateLimitRepository RateLimitRepository { get; }
        
        public IDateTimeProvider DateTimeProvider { get; }

        public TestFixture()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            
            var dateTimeProviderMock = SetupDateTimeProviderMock();

            var host = Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    services.AddDalInfrastructure(config)
                        .AddDalRepositories();
                    
                    services.Replace(ServiceDescriptor.Singleton(_ => dateTimeProviderMock.Object));
                })
                .Build();
            
            ClearDatabase(host);
            host.MigrateUp();

            var scope = host.Services.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            UserRepository = serviceProvider.GetRequiredService<IUserRepository>();
            TaskRepository = serviceProvider.GetRequiredService<ITaskRepository>();
            TaskLogRepository = serviceProvider.GetRequiredService<ITaskLogRepository>();
            TakenTaskRepository = serviceProvider.GetRequiredService<ITakenTaskRepository>();
            UserScheduleRepository = serviceProvider.GetRequiredService<IUserScheduleRepository>();
            TaskCommentRepository = serviceProvider.GetRequiredService<ITaskCommentRepository>();
            RateLimitRepository = serviceProvider.GetRequiredService<IRateLimitRepository>();
            
            DateTimeProvider = serviceProvider.GetRequiredService<IDateTimeProvider>();
            
            FluentAssertionOptions.UseDefaultPrecision();
        }

        private static Mock<IDateTimeProvider> SetupDateTimeProviderMock()
        {
            var dateTimeProviderMock = new Mock<IDateTimeProvider>(MockBehavior.Strict);
            var fixedUtcNow = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
            dateTimeProviderMock
                .Setup(f => f.Now())
                .Returns(fixedUtcNow);
            return dateTimeProviderMock;
        }

        private static void ClearDatabase(IHost host)
        {
            using var scope = host.Services.CreateScope();
            var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
            runner.MigrateDown(0);
        }
    }
}