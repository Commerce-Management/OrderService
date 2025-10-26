namespace OrderService.Shared.Dtos.Jwt;

public record RefreshDto(string AccessToken, string RefreshToken);