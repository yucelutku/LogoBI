using LogoBI.Shared.Metadata;

namespace LogoBI.Engine.Join;

public record ResolvedJoin(LogicalSource ToSource, Relationship Relationship);
