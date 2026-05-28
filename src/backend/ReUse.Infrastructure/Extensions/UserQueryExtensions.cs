using ReUse.Application.Enums;
using ReUse.Domain.Entities;

namespace ReUse.Infrastructure.Extensions;

public static class UserQueryExtensions
{
    public static IQueryable<User> ApplySort(
        this IQueryable<User> query,
        UserSortBy sortBy,
        SortDirection sortOrder)
    {
        var isDescending = sortOrder == SortDirection.Desc;

        query = sortBy switch
        {
            UserSortBy.FullName => isDescending
                ? query.OrderByDescending(u => u.FullName)
                : query.OrderBy(u => u.FullName),

            UserSortBy.Email => isDescending
                ? query.OrderByDescending(u => u.Email)
                : query.OrderBy(u => u.Email),

            UserSortBy.CreatedAt => isDescending
                ? query.OrderByDescending(u => u.CreatedAt)
                : query.OrderBy(u => u.CreatedAt),

            _ => query.OrderByDescending(u => u.CreatedAt)
        };

        return query;
    }

    public static IQueryable<User> Search(
        this IQueryable<User> query,
        string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return query;

        var term = searchTerm.Trim().ToLower();

        return query.Where(u =>
            u.FullName.ToLower().Contains(term) ||
            u.Email.ToLower().Contains(term)
        );
    }

    public static IQueryable<User> FilterByCity(
        this IQueryable<User> query,
        string? city)
    {
        if (string.IsNullOrWhiteSpace(city))
            return query;

        var normalized = city.Trim().ToLower();
        return query.Where(u => u.City != null && u.City.ToLower() == normalized);
    }

    public static IQueryable<User> FilterByCountry(
        this IQueryable<User> query,
        string? country)
    {
        if (string.IsNullOrWhiteSpace(country))
            return query;

        var normalized = country.Trim().ToLower();
        return query.Where(u => u.Country != null && u.Country.ToLower() == normalized);
    }

    public static IQueryable<User> FilterByStateProvince(
        this IQueryable<User> query,
        string? stateProvince)
    {
        if (string.IsNullOrWhiteSpace(stateProvince))
            return query;

        var normalized = stateProvince.Trim().ToLower();
        return query.Where(u => u.StateProvince != null && u.StateProvince.ToLower() == normalized);
    }

    public static IQueryable<User> FilterByActive(
        this IQueryable<User> query,
        bool? isActive)
    {
        if (!isActive.HasValue)
            return query;

        return query.Where(u => u.IsActive == isActive.Value);
    }

    public static IQueryable<User> FilterByCreatedDate(
        this IQueryable<User> query,
        DateTime? createdAfter,
        DateTime? createdBefore)
    {
        if (createdAfter.HasValue)
            query = query.Where(u => u.CreatedAt >= createdAfter.Value);

        if (createdBefore.HasValue)
            query = query.Where(u => u.CreatedAt <= createdBefore.Value);

        return query;
    }

    public static IQueryable<User> ExcludeUser(
        this IQueryable<User> query,
        Guid userId)
    {
        return query.Where(u => u.Id != userId);
    }
}