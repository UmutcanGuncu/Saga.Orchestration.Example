using MassTransit;
using Microsoft.EntityFrameworkCore;
using SagaStateMachine.Service.StateDbContexts;
using SagaStateMachine.Service.StateInstances;
using SagaStateMachine.Service.StateMachines;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddMassTransit(configure =>
{

    configure.AddSagaStateMachine<OrderStateMachine, OrderStateInstance>()
        .EntityFrameworkRepository(options =>
            {
                options.AddDbContext<DbContext, OrderStateDBContext>((provider, _builder) =>
                {
                    _builder.UseNpgsql(builder.Configuration.GetConnectionString("OrderStateDBContext"));
                });

            }
        );
            
    configure.UsingRabbitMq((context, _configure) =>
    {
        _configure.Host(builder.Configuration["RabbitMQ"]);
    });
});
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var host = builder.Build();
host.Run();