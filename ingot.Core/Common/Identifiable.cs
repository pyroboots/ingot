namespace ingot.Core.Common;

public class Identifiable
{
    public Identifier Identifier { get; }
    public Identifiable(string identifier) => Identifier = new(identifier);
    public Identifiable(Identifier identifier) => Identifier = identifier;
}