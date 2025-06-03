using Contracts;
using IotPlatform.Reporting.Api.Database;
using IotPlatform.Reporting.Api.Entities;
using MassTransit;

namespace IotPlatform.Reporting.Api.Articles;

public sealed class ArticleCreatedConsumer : IConsumer<ArticleCreatedEvent>
{
    private readonly ApplicationDbContext _context;

    public ArticleCreatedConsumer(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Consume(ConsumeContext<ArticleCreatedEvent> context)
    {
        var article = new Article
        {
            Id = context.Message.Id,
            CreatedOnUtc = context.Message.CreatedOnUtc
        };

        _context.Add(article);

        await _context.SaveChangesAsync();
    }
}
