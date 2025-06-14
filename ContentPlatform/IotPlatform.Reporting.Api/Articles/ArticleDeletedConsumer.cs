﻿using Contracts;
using IotPlatform.Reporting.Api.Database;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace IotPlatform.Reporting.Api.Articles;

public sealed class ArticleDeletedConsumer : IConsumer<ArticleDeletedEvent>
{
    private readonly ApplicationDbContext _context;

    public ArticleDeletedConsumer(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Consume(ConsumeContext<ArticleDeletedEvent> context)
    {
        var article = await _context
            .Articles
            .FirstOrDefaultAsync(article => article.Id == context.Message.Id);

        if (article is null)
        {
            return;
        }

        _context.Remove(article);

        await _context.SaveChangesAsync();
    }
}
