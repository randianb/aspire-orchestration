using Contracts;
using IotPlatform.Reporting.Api.Database;
using IotPlatform.Reporting.Api.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace IotPlatform.Reporting.Api.Articles;

public sealed class ArticleViewedConsumer : IConsumer<ArticleViewedEvent>
{
    private readonly ApplicationDbContext _context;

    public ArticleViewedConsumer(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Consume(ConsumeContext<ArticleViewedEvent> context)
    {
        var article = await _context
            .Articles
            .FirstOrDefaultAsync(article => article.Id == context.Message.Id);

        if (article is null)
        {
            return;
        }

        var articleEvent = new ArticleEvent
        {
            Id = Guid.NewGuid(),
            ArticleId = article.Id,
            CreatedOnUtc = context.Message.ViewedOnUtc,
            EventType = ArticleEventType.View
        };

        _context.Add(articleEvent);

        await _context.SaveChangesAsync();
    }
}
