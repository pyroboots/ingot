---
_layout: landing
---

![ingot Logo](https://raw.githubusercontent.com/pyroboots/ingot/master/ingot.png)

# ingot

[![C#](https://img.shields.io/badge/C%23-239120?logo=csharp&logoColor=white)](https://learn.microsoft.com/dotnet/csharp/)
[![.NET](https://img.shields.io/badge/.NET-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

**A C# framework to programmatically create Minecraft Bedrock Edition packs** - because writing mountains of JSON sucks.

## ✨ Features

- **Inheritance-Based, Type-Safe API** - Define items, blocks, entities, and more using clean, strongly-typed C# instead of hand-writing JSON
- **Automatic Manifest Generation** - `manifest.json` files for both behavior packs and resource packs are created automatically
- **Full BP + RP Support** - Create behavior packs, resource packs, and linked packs with ease
- **Script API Ready** - Built-in support for enabling the Script API in your pack
- **CompilerState** - Get clear, helpful compile-time feedback to avoid debugging in the Minecraft content log
- **Extensible & Maintainable** - Designed for large or complex addons where manual JSON becomes painful
- **Iterable & Reusable** - Because its code, you can define variable and easily change things project-wide

## 📦 Installation

**ingot** is currently distributed via source. The easiest way to get started is:

### Option 1: Add as a Project Reference (Recommended for now)
1. Clone the repository:
   ```bash
   git clone https://github.com/pyroboots/ingot.git
   ```
2. Add a reference to `ingot.Core` in your .NET project.

### Option 2: Build from Source
```bash
dotnet build ingot.sln
```

> **Future**: **ingot** will be published to NuGet once it reaches a stable API.

## 🚀 Quick Start

```csharp
using ingot.Core.Common;

// inherit traits to add behaviour
public class LasagnaItem : Item, IFood, IBlockPlacer
{
    public override Identifier Identifier => new("test:lasagna");
    public override string Texture => "lasagna";
    public override string DisplayName => "Lasagna";
    
    // override these behaviours
    int IFood.Nutrition => 5;
    float IFood.SaturationModifier => 0.9f;
    string IFood.UsingConvertsTo => "minecraft:bowl";
    
    dynamic IBlockPlacer.Block => "test:block_of_dense_lasagna";
    bool IBlockPlacer.ReplaceBlockItem => true;
}

class Program
{
    static void Main(string[] args)
    {
        BehaviourPack bp = BehaviourPack.Create(Guid.NewGuid().ToString())
            .AddItem<LasagnaItem>();
        
        ResourcePack rp = ResourcePack.Create(Guid.NewGuid().ToString())
            .AddItemTexture("lasagna", "assets/lasagna.png");
        
        Pack pack = new()
        {
            Description = "Example pack made with ingot",
            Name = "ingot example",
            BehaviourPack = bp,
            ResourcePack = rp,
            LinkPacks = true,
            ScriptsEnabled = true,
        };
        
        // and compile the whole pack (bp/ + rp/ + manifests)!
        pack.Compile("./");
    }
}
```

See the [`ingot.Example`](./ingot.Example) project for a more complete working example that includes blocks, items, textures, and the full resource pack side. The new [Resource Packs & Textures](docs/resource-packs.md) guide explains how to supply assets and generate the atlas files.

## 🛠️ Project Structure

| Folder            | Purpose                                 |
|-------------------|-----------------------------------------|
| `ingot.Core`      | Core API                                |
| `ingot.Example`   | Working example of using the library    |
| `ingot.Generators`| Automatic trait generation from MS Docs |

**Made with ❤️ for the Minecraft Bedrock addon community.**
