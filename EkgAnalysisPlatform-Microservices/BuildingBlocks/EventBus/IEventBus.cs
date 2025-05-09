namespace EkgAnalysisPlatform.BuildingBlocks.EventBus
{
    public interface IEventBus
    {
        void Publish<T>(T @event) where T : IntegrationEvent;
        void Subscribe<T, TH>() 
            where T : IntegrationEvent 
            where TH : IIntegrationEventHandler<T>;
        void Unsubscribe<T, TH>() 
            where T : IntegrationEvent 
            where TH : IIntegrationEventHandler<T>;
    }

}