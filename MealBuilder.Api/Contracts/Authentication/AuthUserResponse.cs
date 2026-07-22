namespace MealBuilder.Api.Contracts.Authentication;

public sealed record AuthUserResponse(
    string Id,
    string Email);