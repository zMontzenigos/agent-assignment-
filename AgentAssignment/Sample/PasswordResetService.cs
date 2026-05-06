namespace AgentAssignment.Sample;

/// <summary>
/// Initial implementation of password reset — v1 (partial).
/// </summary>
public class PasswordResetService(IUserRepository users, IEmailService email, ITokenStore tokens)
{
    public async Task<bool> RequestResetAsync(string emailAddress)
    {
        var user = await users.FindByEmailAsync(emailAddress);
        if (user is null || !user.IsActive)
            return false;

        var token = Guid.NewGuid().ToString("N");
        var expiry = DateTime.UtcNow.AddHours(1);

        await tokens.StoreAsync(token, user.Id, expiry);

        var resetLink = $"https://app.example.com/reset?token={token}";
        await email.SendAsync(emailAddress, "Reset your password", $"Click here: {resetLink}");

        return true;
    }

    public async Task<ResetResult> ResetPasswordAsync(string token, string newPassword)
    {
        var entry = await tokens.GetAsync(token);
        if (entry is null || entry.ExpiresAt < DateTime.UtcNow)
            return ResetResult.InvalidOrExpired;

        // TODO: validate password policy
        await users.UpdatePasswordAsync(entry.UserId, newPassword);
        await tokens.InvalidateAsync(token);

        return ResetResult.Success;
    }
}

public enum ResetResult { Success, InvalidOrExpired, PolicyViolation }

// Dependency interfaces (contracts only — implementations live elsewhere)
public interface IUserRepository
{
    Task<User?> FindByEmailAsync(string email);
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

public record User(string Id, string Email, bool IsActive);
public record TokenEntry(string Token, string UserId, DateTime ExpiresAt);
