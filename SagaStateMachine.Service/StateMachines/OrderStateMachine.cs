using MassTransit;
using SagaStateMachine.Service.StateInstances;
using Shared.Messages;
using Shared.OrderEvents;
using Shared.PaymentEvents;
using Shared.Settings;
using Shared.StockEvents;

namespace SagaStateMachine.Service.StateMachines;

public class OrderStateMachine : MassTransitStateMachine<OrderStateInstance>
{
    public Event<OrderStartedEvent> OrderStartedEvent { get; set; } // gelen eventleri burda property olarak tanımlarız
    public Event<StockReservedEvent> StockReservedEvent { get; set; }
    public Event<StockNotReservedEvent> StockNotReservedEvent { get; set; }
    public Event<PaymentCompletedEvent> PaymentCompletedEvent { get; set; }
    public Event<PaymentFailedEvent> PaymentFailedEvent { get; set; }
    
    public State OrderCreated { get; set; }
    public State StockReserved { get; set; } // Bir siparişe dair State Machine tarafınan kullanılacak durumları prop olrk tanımlarız
    public State StockNotReserved { get; set; }
    public State PaymentCompleted { get; set; }
    public State PaymentFailed { get; set; }
    public OrderStateMachine()
    {
        InstanceState(instance => instance.CurrentState); // Güncel durum bilgisinin tutulacağı attribute bilgisi verilmektedir
        Event(() => OrderStartedEvent, 
            orderStateInstance => orderStateInstance.CorrelateBy<int>(database 
                => database.OrderId, @event  => @event.Message.OrderId)
                .SelectId(e=> Guid.NewGuid()));
        // Event() fonksiyonu gelen eventlere göre aksiyon alabilmemizi sağlar
        // Burda orderStarted event olayı gerçekleince database'deki order Id ile gelen eventteki order ıd eşleşmezse correlationId oluşturacak
        //Tetikleyici event OrderStartedEvent'dir
        // Tetikleyici event'in oluşturacağı correlationId değerini kullanarak sürecin takibi sağlanır
        Event(() => StockReservedEvent,
            orderStateInstance => orderStateInstance.CorrelateById(@event =>
                @event.Message.CorrelationId));
        Event(() => StockNotReservedEvent,
            orderStateInstance => orderStateInstance.CorrelateById(@event =>
                @event.Message.CorrelationId));
        Event(() => PaymentCompletedEvent,
            orderStateInstance => orderStateInstance.CorrelateById(@event =>
                @event.Message.CorrelationId));
        Event(() => PaymentFailedEvent,
            orderStateInstance => orderStateInstance.CorrelateById(@event =>
                @event.Message.CorrelationId));
        // İlgili event'ler fırlatıldığında veritabanındaki hangi CorrelationId Değerine sahip state instance'in state'inin değiştirileceğini belirtiriz

        Initially(When(OrderStartedEvent) // tetikleyici event kullanılacağı zaman Inıtally ile kontrok edilmektedir
            .Then(context =>
            {
                context.Saga.OrderId = context.Message.OrderId;
                context.Saga.BuyerId = context.Message.BuyerId;
                context.Saga.TotalPrice = context.Message.TotalPrice;
                context.Saga.CreatedDate = DateTime.UtcNow;
            })
            .TransitionTo(OrderCreated) // state'i değitiriyoruz
            .Send(new Uri($"queue:{RabbitMQSetting.Stock_OrderCreatedEventQueue}"),
                context =>
                {
                    var message = new OrderCreatedEvent(context.Saga.CorrelationId)
                    {
                        OrderItems = context.Message.OrderItems
                    };
                    return message;
                }));
        During(OrderCreated, 
            When(StockReservedEvent)
                .TransitionTo(StockReserved)
                .Send(new Uri($"queue:{RabbitMQSetting.Payment_StartedEventQueue}"),
                    context => new PaymentStartedEvent(context.Saga.CorrelationId)
                    {
                        TotalPrice = context.Saga.TotalPrice,
                        OrderItems = context.Message.OrderItems
                    }),
            When(StockNotReservedEvent)
                .TransitionTo(StockNotReserved)
                .Send(new Uri($"queue:{RabbitMQSetting.Order_OrderFailedEventQueue}"),
                    context => new OrderFailedEvent
                    {
                        OrderId = context.Saga.OrderId,
                        Message = context.Message.Message
                    }));
        During(StockReserved,
            When(PaymentCompletedEvent)
                .TransitionTo(PaymentCompleted)
                .Send(new Uri($"queue:{RabbitMQSetting.Order_OrderCompletedEventQueue}"),
                    context => new OrderCompletedEvent
                    {
                        OrderId = context.Saga.OrderId
                    })
                .Finalize(),
            When(PaymentFailedEvent)
                .TransitionTo(PaymentFailed)
                .Send(new Uri($"queue:{RabbitMQSetting.Order_OrderFailedEventQueue}"),
                    context => new OrderFailedEvent
                    {
                        OrderId = context.Saga.OrderId,
                        Message = context.Message.Message
                    })
                .Send(new Uri($"queue:{RabbitMQSetting.Stock_RollbackMessageQueue}"),
                    context => new StockRollbackMessage
                    {
                        OrderItems = context.Message.OrderItems
                    }));
        SetCompletedWhenFinalized();  // finalize 'a çektiklerimizi veritabanından sildirmek istersek
        //context.Data veritabanındaki ilgili siparişe karşılık gelen instance satırını temsil eder
        //context.Message ise o anki ilgili event'ten gelen datayı temsil eder

    }
}