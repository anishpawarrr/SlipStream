using Confluent.Kafka;
using SlipStream.DTOs;
using SlipStream.DTOs.Telemetry;
using SlipStream.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Runtime.CompilerServices;
namespace SlipStream.Services;

public class Consumer : IConsume
{
    private ConsumerConfig _consumerConfig;
    private IConsumer<string, string> _consumer;
    public Consumer()
    {
        _consumerConfig = new ConsumerConfig
        {
            BootstrapServers = "localhost:9092",
            ClientId = "SlipStreamConsumer",
            GroupId = Guid.NewGuid().ToString(),
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };
        _consumer = new ConsumerBuilder<string, string>(_consumerConfig).Build();
    }

    ~Consumer(){
        _consumer.Close();
        _consumer.Dispose();
    }
    public async Task<ReturnDTO> ConsumeFromDBAsync(int vehicleId, int SessionId, AppDbContext dbContext){

        Entities.Session? session = await dbContext.Sessions.FirstOrDefaultAsync(s => s.Id == SessionId && s.VehicleId == vehicleId);
        if (session == null)
        {
            return new ReturnDTO
            {
                Message = "Session not found",
                StatusCode = 404
            };
        }


        try{
            var telemetryData = await dbContext.Telemetries
                .Where(t => t.SessionId == SessionId)
                .GroupBy(t => new { t.SessionId, t.TimeStamp })
                .Select(g => new SendTelemetryDTO
                {
                    TimeStamp = g.Key.TimeStamp,
                    Values = g.Select(t => new ValueDTO
                    {
                        Parameter = t.Parameter,
                        State = t.State
                    }).ToList()
                })
                .ToListAsync();
                
            return new ReturnDTO
            {
                Message = "Successfully consumed data",
                Data = telemetryData,
                StatusCode = 200,
                Status = true
            };
        }catch (Exception ex){
            return new ReturnDTO{
                Message = $"Failed to consume data: {ex.Message}"
            };
        }
    }

    public async IAsyncEnumerable<SendTelemetryDTO> ConsumeFromKafkaAsync(int SessionId, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _consumer.Unsubscribe();
        _consumer.Subscribe(SessionId.ToString());
        TelemetryDTO? telemetryDTO = null;
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = await Task.Run(() => _consumer.Consume(TimeSpan.FromSeconds(5)), cancellationToken);
                if (consumeResult != null)
                {
                    telemetryDTO = JsonSerializer.Deserialize<TelemetryDTO>(consumeResult.Message.Value);   
                    _consumer.Commit(consumeResult);
                }
            }
            catch
            {
                // System.Console.WriteLine("Error consuming message");
                // System.Console.WriteLine("Error consuming message");
                // System.Console.WriteLine("Error consuming message");
                // System.Console.WriteLine("Error consuming message");
                break;
            }

            if (telemetryDTO != null)
            {
                // System.Console.WriteLine();
                // System.Console.WriteLine();
                // System.Console.WriteLine(telemetryDTO);
                // System.Console.WriteLine();
                // System.Console.WriteLine();
                yield return new SendTelemetryDTO
                {
                    TimeStamp = telemetryDTO.TimeStamp,
                    Values = telemetryDTO.Values
                };
                telemetryDTO = null;
            }
            await Task.Delay(100, cancellationToken);
        }
        yield return new SendTelemetryDTO{
                    TimeStamp = DateTime.UtcNow.Ticks,
                    Values = new List<ValueDTO>()
                };
    }
}
