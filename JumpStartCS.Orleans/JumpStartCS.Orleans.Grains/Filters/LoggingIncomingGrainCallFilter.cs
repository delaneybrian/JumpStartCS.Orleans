using Microsoft.Extensions.Logging;

namespace JumpStartCS.Orleans.Grains.Filters
{
    public class LoggingIncomingGrainCallFilter : IIncomingGrainCallFilter
    {
        private readonly ILogger<LoggingIncomingGrainCallFilter> _logger;

        public LoggingIncomingGrainCallFilter(ILogger<LoggingIncomingGrainCallFilter> logger)
        {
            _logger = logger;
        }

        public async Task Invoke(IIncomingGrainCallContext context)
        {
            _logger.LogInformation($"Incoming Silo Grain Filter: Recived grain call on '{context.Grain}' to '{context.MethodName}' method");

            await context.Invoke();
        }
    }
}
