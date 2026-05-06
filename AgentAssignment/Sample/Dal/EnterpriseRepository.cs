namespace AgentAssignment.Sample.Dal;

public record Enterprise(Guid Id, string Name, string Domain);

public class EnterpriseRepository
{
    private readonly List<Enterprise> _store = [];
}
