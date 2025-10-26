namespace OrderService.Shared.Dtos.Jwt;

public record TokenDto(string AccessToken, string RefreshToken);