using System;

namespace SlipStream.Services;

public interface ITopicCleaner
{
    Task CleanAsync(List<string> topics);
    List<string> ExpiredTopics();
    bool TopicHeartbeat(string topic);
    Dictionary<string, DateTime> GetTopics();
}
