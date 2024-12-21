using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace StarboardSocial.UserService.Domain.Services;

public class DataDeletionListener(IServiceProvider services)
    : BackgroundService
{
    private readonly IServiceProvider _services = services;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    { 
        IServiceScope scope = _services.CreateScope();
        

        DataDeletionConsumer dataDeletionConsumer = 
            scope.ServiceProvider
                .GetRequiredService<DataDeletionConsumer>();
            
        await dataDeletionConsumer.StartListening(async userId =>
        {
            IServiceScope scope2 = _services.CreateScope();
            DataDeletionHandler dataDeletionHandler = 
                scope2.ServiceProvider
                    .GetRequiredService<DataDeletionHandler>();
            await dataDeletionHandler.Handle(userId);
            scope2.Dispose();
        });
        
        scope.Dispose();
    }
}