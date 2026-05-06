namespace AgentAssignment.Sample;

public class PasswordResetService(
    IUserRepository users,
    IEmailService email,
    ITokenStore tokens,
    IRateLimiter rateLimiter)
{
    public async Task<RequestResetResult> RequestResetAsync(string emailAddress)
    {
        if (!await rateLimiter.AllowAsync(emailAddress))
            return RequestResetResult.RateLimited;

        var user = await users.FindByEmailAsync(emailAddress);
        if (user is null || !user.IsActive)
            return RequestResetResult.NotFound;

        var token = Guid.NewGuid().ToString("N");
        var expiry = DateTime.UtcNow.AddHours(1);

        await tokens.StoreAsync(token, user.Id, expiry);

        var resetLink = $"https://app.example.com/reset?token={token}";
        await email.SendAsync(emailAddress, "Reset your password", $"Click here: {resetLink}");

        return RequestResetResult.Sent;
    }

    public async Task<ResetResult> ResetPasswordAsync(string token, string newPassword)
    {
        var entry = await tokens.GetAsync(token);
        if (entry is null || entry.ExpiresAt < DateTime.UtcNow)
            return ResetResult.InvalidOrExpired;

        if (!PasswordPolicy.IsValid(newPassword))
            return ResetResult.PolicyViolation;

        var user = await users.FindByIdAsync(entry.UserId);

        await users.UpdatePasswordAsync(entry.UserId, newPassword);
        await tokens.InvalidateAsync(token);

        if (user is not null)
            await email.SendAsync(user.Email, "Your password has been reset",
                "Your password was successfully changed. If this wasn't you, contact support.");

        return ResetResult.Success;
    }
}

public static class PasswordPolicy
{
    public static bool IsValid(string password) =>
        password.Length >= 8 &&
        password.Any(char.IsUpper) &&
        password.Any(char.IsDigit);
}

public enum RequestResetResult { Sent, NotFound, RateLimited }
public enum ResetResult { Success, InvalidOrExpired, PolicyViolation }

public interface IUserRepository
{
    Task<User?> FindByEmailAsync(string email);
    Task<User?> FindByIdAsync(string userId);
    Task UpdatePasswordAsync(string userId, string newPassword);
}

public interface IEmailService
{
    Task SendAsync(string to, string subject, string body);
}

public interface ITokenStore
{
    Task StoreAsync(string token, string userId, DateTime expiresAt);
    Task<TokenEntry?> GetAsync(string token);
    Task InvalidateAsync(string token);
}

public interface IRateLimiter
{
    Task<bool> AllowAsync(string key);
}

public record User(string Id, string Email, bool IsActive);
public record TokenEntry(string Token, string UserId, DateTime ExpiresAt);
