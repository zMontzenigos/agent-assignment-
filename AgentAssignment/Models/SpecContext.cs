namespace AgentAssignment.Models;

record SpecContext(
    string RawDiff,
    string DiffSummary,
    string UserStory,
    string? EvaluationReport,
    string? ExistingFuncSpec,
    string? ExistingTechSpec
);
