using Notism.Domain.Common;

namespace Notism.Domain.BlogEventMention;

public class BlogEventMention : Entity
{
    public Guid BlogId { get; private set; }
    public Guid EventId { get; private set; }
    public int MentionOrder { get; private set; }
    public string? Context { get; private set; }

    private BlogEventMention(Guid blogId, Guid eventId, int mentionOrder = 0, string? context = null)
    {
        BlogId = blogId;
        EventId = eventId;
        MentionOrder = mentionOrder;
        Context = context;
    }

    public static BlogEventMention Create(Guid blogId, Guid eventId, int mentionOrder = 0, string? context = null)
    {
        return new BlogEventMention(blogId, eventId, mentionOrder, context);
    }

    public void Update(int mentionOrder, string? context = null)
    {
        MentionOrder = mentionOrder;
        Context = context;
        UpdatedAt = DateTime.UtcNow;
    }
}