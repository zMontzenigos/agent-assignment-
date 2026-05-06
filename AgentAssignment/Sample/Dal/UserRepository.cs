namespace AgentAssignment.Sample.Dal;

public record User(Guid Id, string Name, string Email, string Status = "Active");

public class UserRepository
{
    private readonly List<User> _store = [];
}
