using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

static class MyFirstFunction
{
    [Function(nameof(RaceOrchestrator))]
    public static async Task<string> RaceOrchestrator([OrchestrationTrigger] TaskOrchestrationContext context, ILogger log)
    {
        //How to log in an Orchestrator - avoid it if necessary though
        log = context.CreateReplaySafeLogger(nameof(RaceOrchestrator));

        // Fan-out/fan-in pattern
        var tasks = new[]
        {
            context.CallActivityAsync<string>(nameof(StartRunning), "Fred the duck"),
            context.CallActivityAsync<string>(nameof(StartRunning), "Lucy the cat"),
            context.CallActivityAsync<string>(nameof(StartRunning), "Traudl the tortoise"),
            context.CallActivityAsync<string>(nameof(StartRunning), "Milo the rabbit"),
            context.CallActivityAsync<string>(nameof(StartRunning), "Gina the hamster"),
            context.CallActivityAsync<string>(nameof(StartRunning), "Benny the squirrel")
        };

        var winner = await Task.WhenAny(tasks);
        var allFinished = await Task.WhenAll(tasks);

        var result = $"All participants finished! The winner is {await winner}!";
        log.LogInformation(result);
        return result;
    }
        

    [Function(nameof(StartRunning))]
    public static async Task<string> StartRunning([ActivityTrigger] string participant, FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger(nameof(StartRunning));
        logger.LogInformation($"Participant {participant} starts running!");

        var random = new Random();
        var runningTime = random.Next(3000, 10000);
        await Task.Delay(runningTime);

        logger.LogInformation($"Participant {participant} finished in {runningTime/1000.0} ms.");
        return $"{participant}";
    }

    [Function(nameof(StartRaceContest))]
    public static async Task<HttpResponseData> StartRaceContest(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req,
        [DurableClient] DurableTaskClient client,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger(nameof(StartRaceContest));

        var instanceId = await client.ScheduleNewOrchestrationInstanceAsync(nameof(RaceOrchestrator));
        logger.LogInformation("Started race orchestration with instance ID = {instanceId}", instanceId);

        return client.CreateCheckStatusResponse(req, instanceId);
    }
}
