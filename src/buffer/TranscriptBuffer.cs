namespace Msteambot.Buffer;

public class TranscriptBuffer
{
    private readonly Dictionary<string, List<string>> _buffers = new();

    public void Add(string meetingId, string chunk)
    {
        if (!_buffers.ContainsKey(meetingId))
        {
            _buffers[meetingId] = new List<string>();
        }

        _buffers[meetingId].Add(chunk);
    }

    public IReadOnlyList<string> Flush(string meetingId)
    {
        if (!_buffers.TryGetValue(meetingId, out var chunks))
        {
            return Array.Empty<string>();
        }

        _buffers.Remove(meetingId);
        return chunks;
    }
}
