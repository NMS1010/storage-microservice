using MassTransit;

namespace BuildingBlocks.MassTransit
{
    public class ConsumeFilter<T> : IFilter<ConsumeContext<T>> where T : class
    {
        public void Probe(ProbeContext context)
        {
            throw new NotImplementedException();
        }

        public Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
        {
            throw new NotImplementedException();
        }
    }
}
