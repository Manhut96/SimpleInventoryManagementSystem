namespace SimpleInventoryManagementSystem.Application.Contracts.Responses;

public record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int PageNumber, int PageSize);
