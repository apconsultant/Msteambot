namespace Msteambot.DocGen;

public class DocumentGenerator
{
    public string CreateDocument(string meetingId, IEnumerable<string> content)
    {
        return $"Document for {meetingId}:\n{string.Join(Environment.NewLine, content)}";
    }
}
