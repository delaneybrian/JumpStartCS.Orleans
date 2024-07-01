using Microsoft.Extensions.Logging;

namespace JumpStartCS.Orleans.Grains.Filters
{
    public class LoggingOutgoingGrainCallFilter : IOutgoingGrainCallFilter
    {
        private readonly ILogger<LoggingOutgoingGrainCallFilter> _logger;

        public LoggingOutgoingGrainCallFilter(ILogger<LoggingOutgoingGrainCallFilter> logger)
        {
            _logger = logger;
        }

        public async Task Invoke(IOutgoingGrainCallContext context)
        {
            _logger.LogInformation($"Outgoing Silo Grain Filter: Recived grain call on '{context.Grain}' to '{context.MethodName}' method");

            await context.Invoke();
        }
    }
}
