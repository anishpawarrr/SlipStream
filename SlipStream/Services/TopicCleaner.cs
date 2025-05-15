using System;
using System.Collections.Concurrent;
using Confluent.Kafka;

namespace SlipStream.Services;

public class TopicCleaner : BackgroundService, ITopicCleaner
{
    private readonly IAdminClient _adminClient;
    private static readonly ConcurrentDictionary<string, DateTime> _topicsDict = new ConcurrentDictionary<string, DateTime>();
    private readonly TimeSpan _inactiveThreshold = TimeSpan.FromHours(3), _bgInterval = TimeSpan.FromMinutes(30);

    public TopicCleaner()
    {
        var adminConfig = new AdminClientConfig{
            BootstrapServers = "localhost:9092"
        };

        _adminClient = new AdminClientBuilder(adminConfig).Build();

    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        
        while(!stoppingToken.IsCancellationRequested){
            // System.Console.WriteLine();
            // System.Console.WriteLine();
            // System.Console.WriteLine("==================== BG SERVICE =======================");
            // System.Console.WriteLine();
            // System.Console.WriteLine();
            var expiredTopics = ExpiredTopics();
            if (expiredTopics.Count > 0) await CleanAsync(expiredTopics);
            await Task.Delay(_bgInterval);
        }
    }
    public async Task CleanAsync(List<string> topics)
    {
        await _adminClient.DeleteTopicsAsync(topics);

        foreach (string expiredTopic in topics){
            // System.Console.WriteLine();
            // System.Console.WriteLine();
            // System.Console.WriteLine($"Topic {expiredTopic} expired and deleted");
            // System.Console.WriteLine();
            // System.Console.WriteLine();
            _topicsDict.TryRemove(expiredTopic, out _);
        }
    }

    public bool TopicHeartbeat(string topic)
    {
        try{
            if (_topicsDict.TryGetValue(topic, out _)){
                _topicsDict[topic] = DateTime.UtcNow;
            }else{
                _topicsDict.TryAdd(topic, DateTime.UtcNow);
            }
            return true;
        }catch{
            // System.Console.WriteLine($"Error updating topic heartbeat: {ex.Message}");
            return false;
        }
        
    }

    public List<string> ExpiredTopics()
    {
        var currentTime = DateTime.UtcNow;
        
        // Debug - show all topics and their ages
        // System.Console.WriteLine($"Current time: {currentTime}, Dictionary has {_topicsDict.Count} topics");
        // foreach (var entry in _topicsDict)
        // {
        //     var age = currentTime - entry.Value;
        //     System.Console.WriteLine($"Topic: {entry.Key}, Last active: {entry.Value}, Age: {age.TotalSeconds:F1}s, Threshold: {_inactiveThreshold.TotalSeconds}s");
        // }
        
        // Get expired topics - use proper TimeSpan comparison
        var topics = _topicsDict
                    .Where(t => (currentTime - t.Value) > _inactiveThreshold)
                    .Select(t => t.Key)
                    .ToList();

        // if (topics.Count == 0)
        // {
        //     System.Console.WriteLine("==================== No expired topics =======================");
        // }
        // else
        // {
        //     System.Console.WriteLine($"==================== Found {topics.Count} expired topics =======================");
        //     foreach (var topic in topics)
        //     {
        //         var lastActive = _topicsDict[topic];
        //         var age = currentTime - lastActive;
        //         System.Console.WriteLine($"Topic {topic} is expired (last active: {lastActive}, age: {age.TotalSeconds:F1}s)");
        //     }
        // }
        return topics;
    }

    public Dictionary<string, DateTime> GetTopics()
    {
        return _topicsDict.ToDictionary(t => t.Key, t => t.Value);
    }
}
