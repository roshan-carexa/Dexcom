using Quartz;

namespace Dexcom.Services.Jobs;

[DisallowConcurrentExecution]
public class EgvsJobs : IJob
{
    private readonly IDexcomDataService _dexcomDataService;
    private readonly ILogger<EgvsJobs> _logger;

    public EgvsJobs(IDexcomDataService dexcomDataService, ILogger<EgvsJobs> logger)
    {
        _dexcomDataService = dexcomDataService;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            var userId = "self";
            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddMinutes(-5);
            
            _logger.LogInformation("Fetching Egvs data from {Start} to {End}", startDate, endDate);

            var data = await _dexcomDataService.GetEgvsAsync(userId, startDate, endDate);
            
            _logger.LogInformation("EGVS data fetched successfully");
            Console.WriteLine(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while fetching EGVS data");
        }
    }
}