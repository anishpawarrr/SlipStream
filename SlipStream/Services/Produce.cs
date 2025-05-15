using System.Text.Json;
using Confluent.Kafka;
using SlipStream.Data;
using SlipStream.DTOs;
using SlipStream.DTOs.Telemetry;

namespace SlipStream.Services;

public class Produce : IProduce
{
    private readonly ProducerConfig _producerConfig;
    private readonly IProducer<string, string> _producer;
    private readonly ITopicCleaner _topicCleaner;
    public Produce(ITopicCleaner topicCleaner)
    {
        _topicCleaner = topicCleaner;
        
        _producerConfig = new ProducerConfig
        {
            BootstrapServers = "localhost:9092",
            ClientId = "SlipStreamProducer",
            Acks = Acks.All,
            AllowAutoCreateTopics = true
        };

        _producer = new ProducerBuilder<string, string>(_producerConfig).Build();
    }
    
    public async Task<ReturnDTO> InsertAsync(int SessionId, TelemetryDTO telemetryDTO, AppDbContext dbContext)
    {
        try
        {
            Entities.Telemetry telemetry;
            // Add to database
            long timeStamp = telemetryDTO.TimeStamp is not null ? telemetryDTO.TimeStamp.Value : DateTime.UtcNow.Ticks;
            foreach (var value in telemetryDTO.Values)
            {
                telemetry = new Entities.Telemetry
                {
                    SessionId = SessionId,
                    TimeStamp = timeStamp,
                    Parameter = value.Parameter,
                    State = value.State
                };
                await dbContext.Telemetries.AddAsync(telemetry);
            }
            
            await dbContext.SaveChangesAsync();
            return new ReturnDTO { Status = true, Message = "Data inserted successfully", StatusCode = 201 };
        }
        catch (Exception ex)
        {
            return new ReturnDTO { Message = $"Failed to insert data: {ex.Message}" };
        }
    }

    public async Task<ReturnDTO> InsertAsync(int SessionId, TelemetryBatchDTO telemetryBatchDTO, AppDbContext dbContext)
    {
        if (SessionId != telemetryBatchDTO.SessionId)
        {
            return new ReturnDTO { Message = "Session ID mismatch", StatusCode = 401 };
        }
        try
        {
            foreach (BatchValueDTO batchValue in telemetryBatchDTO.Values)
            {
                long timeStamp = batchValue.TimeStamp is not null ? batchValue.TimeStamp.Value : DateTime.UtcNow.Ticks;
                foreach (ValueDTO value in batchValue.BatchValues)
                {
                    var telemetry = new Entities.Telemetry
                    {
                        SessionId = SessionId,
                        TimeStamp = timeStamp,
                        Parameter = value.Parameter,
                        State = value.State
                    };
                    await dbContext.Telemetries.AddAsync(telemetry);
                }
            }
            await dbContext.SaveChangesAsync();
            return new ReturnDTO { Status = true, Message = "Batch data inserted successfully", StatusCode = 201 };
        }
        catch (Exception ex)
        {
            return new ReturnDTO { Message = $"Failed to insert batch data: {ex.Message}" };
        }
    }

    public async Task<ReturnDTO> ProduceAsync(int SessionId, TelemetryDTO telemetryDTO, AppDbContext dbContext){
        try
        {
            Task<ReturnDTO> resultPublish = PushOnQueueAsync(SessionId, telemetryDTO);
            Task<ReturnDTO> resultInsert = InsertAsync(SessionId, telemetryDTO, dbContext);

            await Task.WhenAll(resultPublish, resultInsert);
            // await Task.WhenAll(resultPublish);
            
            if (!resultPublish.Result.Status) return resultPublish.Result;
            if (!resultInsert.Result.Status) return resultInsert.Result;
            
            _topicCleaner.TopicHeartbeat(SessionId.ToString());

            return new ReturnDTO { Status = true, Message = "Message produced and inserted successfully", StatusCode = 201 };
        }
        catch (Exception ex)
        {
            return new ReturnDTO { Message = $"Failed to produce message: {ex.Message}" };
        }
    }

    public async Task<ReturnDTO> ProduceAsync(int SessionId, TelemetryBatchDTO telemetryBatchDTO, AppDbContext dbContext)
    {
        try
        {
            Task<ReturnDTO> resultPublish = PushOnQueueAsync(SessionId, telemetryBatchDTO);
            Task<ReturnDTO> resultInsert = InsertAsync(SessionId, telemetryBatchDTO, dbContext);

            await Task.WhenAll(resultPublish, resultInsert);
            if (!resultPublish.Result.Status) return resultPublish.Result;
            if (!resultInsert.Result.Status) return resultInsert.Result;

            return new ReturnDTO { Status = true, Message = "Batch message produced and inserted successfully", StatusCode = 201 };
        }
        catch (Exception ex)
        {
            return new ReturnDTO { Message = $"Failed to produce batch message: {ex.Message}" };
        }
    }

    public async Task<ReturnDTO> PushOnQueueAsync(int SessionId, TelemetryDTO telemetryDTO){
        if (SessionId != telemetryDTO.SessionId)
        {
            return new ReturnDTO { Message = "Session ID mismatch", StatusCode = 401 };
        }
        string topic = SessionId.ToString();    
        try
        {   
            telemetryDTO.TimeStamp = telemetryDTO.TimeStamp is not null ? telemetryDTO.TimeStamp.Value : DateTime.UtcNow.Ticks;  
            var message = new Message<string, string>
            {
                Key = topic,
                Value = JsonSerializer.Serialize(telemetryDTO)
            };
            
            var deliveryResult = await _producer.ProduceAsync(topic, message);
            
            return new ReturnDTO 
            { 
                Status = true,
                Message = $"Message delivered to {deliveryResult.TopicPartitionOffset}",
                StatusCode = 201
            };
        }
        catch (Exception ex)
        {
            return new ReturnDTO 
            { 
                Message = $"Failed to deliver message: {ex.Message}"
            };
        }
    }

    public async Task<ReturnDTO> PushOnQueueAsync(int SessionId, TelemetryBatchDTO telemetryBatchDTO)
    {
        if (SessionId != telemetryBatchDTO.SessionId)
        {
            return new ReturnDTO { Message = "Session ID mismatch", StatusCode = 401 };
        }

        string topic = SessionId.ToString();
        TelemetryDTO telemetryDTO;
        
        try
        {
            foreach (BatchValueDTO batchValue in telemetryBatchDTO.Values)
            {
                long timeStamp = batchValue.TimeStamp is not null ? batchValue.TimeStamp.Value : DateTime.UtcNow.Ticks;
                telemetryDTO = new TelemetryDTO
                {
                    VehicleId = telemetryBatchDTO.VehicleId,
                    SessionId = SessionId,
                    TimeStamp = timeStamp,
                    Values = batchValue.BatchValues
                };
                
                var message = new Message<string, string>
                {
                    Key = topic,
                    Value = JsonSerializer.Serialize(telemetryDTO)
                };
                
                var deliveryResult = await _producer.ProduceAsync(topic, message);
            }
            
            return new ReturnDTO 
            { 
                Status = true,
                Message = $"Batch message delivered" 
            };
        }
        catch (Exception ex)
        {
            return new ReturnDTO 
            { 
                Message = $"Failed to deliver batch message: {ex.Message}" 
            };
        }
    }

    public Dictionary<string, DateTime> GetTopics()
    {
        return _topicCleaner.GetTopics();
    }

}