namespace EkgAnalysisPlatform.BuildingBlocks.EventBus
{    
    public abstract class IntegrationEvent
    {
        public Guid Id { get; private set; }
        public DateTime CreationDate { get; private set; }

        public IntegrationEvent()
        {
            Id = Guid.NewGuid();
            CreationDate = DateTime.UtcNow;
        }
    }
}