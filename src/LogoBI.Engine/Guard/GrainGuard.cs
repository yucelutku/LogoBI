using LogoBI.Shared.Metadata;

namespace LogoBI.Engine.Guard;

public static class GrainGuard
{
    private const string RoleMeasure = "measure";

    public static void Validate(IEnumerable<Field> selectedFields, IEnumerable<LogicalSource> sources)
    {
        ArgumentNullException.ThrowIfNull(selectedFields);
        ArgumentNullException.ThrowIfNull(sources);

        var distinctGrains = new List<string>();

        foreach (var field in selectedFields)
        {
            if (field is null)
            {
                continue;
            }

            if (!string.Equals(field.Role, RoleMeasure, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var source = sources.FirstOrDefault(s => s.Id == field.SourceId)
                ?? throw new InvalidOperationException($"Source '{field.SourceId}' for field '{field.DisplayName}' not found.");

            if (!distinctGrains.Contains(source.Grain, StringComparer.OrdinalIgnoreCase))
            {
                distinctGrains.Add(source.Grain);
            }
        }

        if (distinctGrains.Count >= 2)
        {
            throw new GrainMixException($"Farklı seviyedeki ölçüler birlikte toplanamaz: {distinctGrains[0]} ve {distinctGrains[1]}.");
        }
    }
}
