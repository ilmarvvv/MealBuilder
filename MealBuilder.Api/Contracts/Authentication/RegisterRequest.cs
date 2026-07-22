using System.ComponentModel.DataAnnotations;

namespace MealBuilder.Api.Contracts.Authentication;

public sealed record RegisterRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password);