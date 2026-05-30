namespace AuthApi.Models;

public record LoginRequest(
    string Username,
    string Password
);

public record TokenRequest(
    string ClientId,
    string ClientSecret
);

public record RefreshRequest(
    string RefreshToken
);

public record JwtTokenResponse(
    string AccessToken,
    string TokenType,
    int ExpiresIn,
    string? RefreshToken = null
);

public record LoginResponse(
    string AccessToken,
    int ExpiresIn,
    string Username,
    string Role,
    string? RefreshToken = null
);
