using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;
using OneOf;
using OneOf.Types;
using CardExplorer.Client.Core;
using CardExplorer.Client.Features.Cards;
using CardExplorer.Client.Features.Collections;
using CardExplorer.Client.Features.Explorations;
using CardExplorer.Data;
using CardExplorer.Data.Cards;
using CardExplorer.Data.Explorations;

namespace CardExplorer.Endpoints.Explorations;

internal sealed class ExplorationService
{
    private readonly ApplicationDbContext _db;

    public ExplorationService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<ExplorationSummaryDto>> GetAsync(UserId userId, GetExplorationsRequest request,
        CancellationToken cancellationToken)
    {
        var query = _db.Users
            .Where(u => u.Id == userId)
            .SelectMany(u => u.Explorations);

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            query = query.Where(c => c.Name.Contains(request.Name));
        }

        return await query
            .ProjectToSummaryDto()
            .ToListAsync(cancellationToken: cancellationToken);
    }

    public async Task<OneOf<ExplorationDto, NotFound>> GetByIdAsync(UserId userId, ExplorationId explorationId,
        CancellationToken cancellationToken)
    {
        var exploration = await _db.Users
            .Where(u => u.Id == userId)
            .SelectMany(u => u.Explorations)
            .Include(e => e.SeenCards)
            .Include(e => e.CardCollections)
            .ThenInclude(c => c.Cards)
            .FirstOrDefaultAsync(e => e.Id == explorationId, cancellationToken);

        if (exploration == null)
        {
            return new NotFound();
        }

        return exploration.MapToDto();
    }

    public async Task<OneOf<Success, NotFound>> DeleteAsync(UserId userId, ExplorationId explorationId,
        CancellationToken cancellationToken)
    {
        var exploration = await _db.Users
            .Where(u => u.Id == userId)
            .SelectMany(u => u.Explorations)
            .Where(x => x.Id == explorationId)
            .ExecuteDeleteAsync(cancellationToken);
        
        return exploration > 0 ? new Success() : new NotFound();
    }

    public async Task<OneOf<ExplorationDto, NotFound>> CreateAsync(UserId userId, CreateExplorationRequest request,
        CancellationToken cancellationToken)
    {
        var exploration = new Exploration
        {
            Name = request.Name,
            SearchString = request.SearchString,
            UserId = userId
        };

        var userEntity = await _db.Users.Include(u => u.Explorations)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (userEntity == null)
        {
            return new NotFound();
        }

        userEntity.Explorations.Add(exploration);
        await _db.SaveChangesAsync(cancellationToken);

        return exploration.MapToDto();
    }

    public async Task<OneOf<Success, NotFound>> UpdateAsync(UserId userId, PatchExplorationRequest request,
        CancellationToken cancellationToken)
    {
        var entry = await _db.Users
            .Where(x => x.Id == userId)
            .SelectMany(x => x.Explorations)
            .Where(x => x.Id == request.Id)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (entry is null)
        {
            return new NotFound();
        }

        entry.Name = request.Name ?? entry.Name;
        entry.SearchString = request.SearchString ?? entry.SearchString;
        entry.CurrentPage = request.CurrentPage ?? entry.CurrentPage;

        await _db.SaveChangesAsync(cancellationToken);
        return new Success();
    }

    public async Task<OneOf<Success, NotFound>> AddCollectionAsync(UserId userId,
        AddCollectionToExplorationRequest request, CancellationToken cancellationToken)
    {
        var exploration = await _db.Users
            .Where(u => u.Id == userId)
            .SelectMany(u => u.Explorations)
            .Include(e => e.CardCollections)
            .FirstOrDefaultAsync(e => e.Id == request.ExplorationId, cancellationToken);

        if (exploration == null)
        {
            return new NotFound();
        }

        var collection = await _db.Users
            .Where(u => u.Id == userId)
            .SelectMany(u => u.CardCollections)
            .FirstOrDefaultAsync(c => c.Id == request.CollectionId, cancellationToken);

        if (collection == null)
        {
            return new NotFound();
        }

        if (exploration.CardCollections.Any(c => c.Id == request.CollectionId))
        {
            return new Success();
        }

        exploration.CardCollections.Add(collection);
        await _db.SaveChangesAsync(cancellationToken);

        return new Success();
    }

    public async Task<OneOf<Success, NotFound>> RemoveCollectionAsync(UserId userId,
        RemoveCollectionFromExplorationRequest request, CancellationToken cancellationToken)
    {
        var exploration = await _db.Users
            .Where(u => u.Id == userId)
            .SelectMany(u => u.Explorations)
            .Include(e => e.CardCollections)
            .FirstOrDefaultAsync(e => e.Id == request.ExplorationId, cancellationToken);

        if (exploration == null)
        {
            return new NotFound();
        }

        var collection = exploration.CardCollections.FirstOrDefault(c => c.Id == request.CollectionId);
        if (collection != null)
        {
            exploration.CardCollections.Remove(collection);
            await _db.SaveChangesAsync(cancellationToken);
        }

        return new Success();
    }

    public async Task<OneOf<Success, NotFound>> AddSeenCardAsync(UserId userId, AddSeenCardToExplorationRequest request,
        CancellationToken cancellationToken)
    {
        var alreadySeen = await _db.Users
            .Where(u => u.Id == userId)
            .SelectMany(u => u.Explorations)
            .Where(x => x.Id == request.ExplorationId)
            .SelectMany(x => x.SeenCards)
            .AnyAsync(x => x.Id == request.CardId, cancellationToken);
        
        if (alreadySeen)
        {
            return new Success();
        }
        
        var exploration = await _db
            .Users
            .SelectMany(x => x.Explorations)
            .Where(x => x.Id == request.ExplorationId)
            .FirstOrDefaultAsync(cancellationToken);

        if (exploration is null)
        {
            return new NotFound();
        }
        
        var card = await _db.Cards.FirstOrDefaultAsync(x => x.Id == request.CardId, cancellationToken) 
                   ?? new Card { Id = request.CardId };
        
        exploration.SeenCards.Add(card);
        await _db.SaveChangesAsync(cancellationToken);
        return new Success();
    }

    public async Task<OneOf<Success, NotFound>> RemoveSeenCardAsync(UserId userId,
        RemoveSeenCardFromExplorationRequest request, CancellationToken cancellationToken)
    {
        var exploration = await _db.Users
            .Where(u => u.Id == userId)
            .SelectMany(u => u.Explorations)
            .Where(x => x.Id == request.ExplorationId)
            .SelectMany(x => x.SeenCards)
            .Where(x => x.Id == request.CardId)
            .ExecuteDeleteAsync(cancellationToken);

        if (exploration == 0)
        {
            return new NotFound();
        }

        return new Success();
    }
}

public static class ExplorationExtensions
{
    public static ExplorationDto MapToDto(this Exploration exploration)
    {
        return new ExplorationDto
        {
            Id = exploration.Id,
            Name = exploration.Name,
            SearchString = exploration.SearchString,
            UserId = exploration.UserId,
            SeenCards = new ObservableCollection<ScryfallCardId>(exploration.SeenCards.Select(c => c.Id)),
            CardCollections = new ObservableCollection<CardCollectionDto>(exploration.CardCollections.Select(c =>
                new CardCollectionDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    UserId = c.UserId,
                    Cards = new ObservableCollection<ScryfallCardId>(c.Cards.Select(card => card.Id))
                })),
            CurrentPage = exploration.CurrentPage
        };
    }

    public static IQueryable<ExplorationSummaryDto> ProjectToSummaryDto(this IQueryable<Exploration> queryable) =>
        queryable.Select(exploration => new ExplorationSummaryDto
        {
            Id = exploration.Id,
            Name = exploration.Name,
            SearchString = exploration.SearchString,
            UserId = exploration.UserId,
        });

    public static IQueryable<ExplorationDto> ProjectToDto(this IQueryable<Exploration> queryable) => queryable
        .Select(exploration => new ExplorationDto
        {
            Id = exploration.Id,
            Name = exploration.Name,
            SearchString = exploration.SearchString,
            SeenCards = new ObservableCollection<ScryfallCardId>(exploration.SeenCards.Select(seenCard => seenCard.Id)
                .ToList()),
            UserId = exploration.UserId,
            CardCollections = new ObservableCollection<CardCollectionDto>(exploration.CardCollections.Select(c =>
                new CardCollectionDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    UserId = c.UserId,
                    Cards = new ObservableCollection<ScryfallCardId>(c.Cards.Select(card => card.Id))
                })),
            CurrentPage = exploration.CurrentPage
        });
}