# Compile Hooks

To run custom logic before or after a content type is written during pack compile, implement `ICompileHooks` and attach it with `[CompileHooks]`:

```csharp
using ingot.Core.TraitSystem;

public class MyBlockHooks : ICompileHooks
{
    public void PreCompile(object inst) { /* e.g. log or mutate */ }
    public string? PostCompile(string json) => json; // return null to keep original
}

[CompileHooks(typeof(MyBlockHooks))]
public class MyBlock : Block { /* ... */ }
```

Hooks run for types registered on the behaviour pack (blocks, items, entities, recipes). 

`PostCompile` may return modified JSON. This can be used for post-processing, formatting of the outputted JSON or to facilitate something that **ingot** does not yet support.

```csharp
public class DenseLasagnaBlockHooks : ICompileHooks
{
    public void PreCompile(object inst) => CompilerState.Warn("pre compile hooks!");

    public string? PostCompile(string json)
    {
        // pair with JsonTextWriter and/or JsonTextReader for easier manipulation
        JsonTextWriter writer = new(new StringWriter(new StringBuilder(json)));
        writer.WriteComment("extra comment added with post compile hooks!");
        
        return writer.ToString();
    }
}

[CompileHooks(typeof(DenseLasagnaBlockHooks))]
public class DenseLasagnaBlock : Block { ... }
```

See also: [Trait System](trait-system.md), [Trait Attributes](trait-attributes.md).