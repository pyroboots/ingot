# Setup

This guide walks you through setting up your project, ready to move onto making your first content types and more.

## Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) (the repo targets **.NET 10**)
- A Minecraft Bedrock Edition client
- A text editor or IDE (Rider, Visual Studio, or VS Code all work well)

## Installation

**ingot** is distributed as source today. The recommended approach is to add a project reference to `ingot.Core`:

```bash
git clone https://github.com/pyroboots/ingot.git
cd ingot
dotnet build ingot.sln
```

> [!NOTE]
> ingot will be published to NuGet once the API stabilizes. Until then, use a project reference to `ingot.Core`.

## Create a Project

Lets create a console application called `MyAddon`. This will act as the compiler for your pack's contents.

```bash
dotnet new console -n MyAddon
cd MyAddon
# add the ProjectReference to ingot.Core as shown below
```

In `MyAddon.csproj`, reference the core library:

```xml
<ItemGroup>
  <ProjectReference Include="path/to/ingot/ingot.Core/ingot.Core.csproj" />
</ItemGroup>
```

> [!NOTE]
> Its recommended to clone `ingot` next to your addon solution (or add it to the same solution) for easy access and simpler project references.

## Project Layout (Recommended)

A typical ingot addon solution looks like this:

```
MyAddon/
  MyAddon.csproj          # ingot.Core reference
  Program.cs              # content registration + compilation
  Content/
    Items/
    Blocks/
    Recipes/
    Entities/
  Data/                   # png textures (copy to output via .csproj)
  scripts/                # script API sources (for later)
    blocks/               # handler bodies for BlockEvents.FromFile
    items/                # handler bodies for ItemEvents.FromFile
    services/             # tick handler bodies registered with AddService
    events/               # /scriptevent handler bodies for AddScriptEvent
  output/                 # generated bp/ + rp/ (gitignored)
```

Keep identifiers, traits, and cross-references in C# - recipes can reference item classes, blocks can auto-register loot tables, and refactors stay type-safe.

To copy `Data` and `scripts` to your output directory, add this to `MyAddon.csproj`:

```xml
<ItemGroup>
    <None Include="Data\**\*" CopyToOutputDirectory="PreserveNewest" />
    <None Include="scripts\**\*" CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>
```

Your `.csproj` should look something like this:

```xml
<Project Sdk="Microsoft.NET.Sdk">
    <PropertyGroup>
        <OutputType>Exe</OutputType>
        <TargetFramework>net10.0</TargetFramework>
        <ImplicitUsings>enable</ImplicitUsings>
        <Nullable>enable</Nullable>
    </PropertyGroup>

    <ItemGroup>
      <ProjectReference Include="..\ingot\ingot.Core\ingot.Core.csproj" />
    </ItemGroup>

    <ItemGroup>
      <None Include="Data\**\*" CopyToOutputDirectory="PreserveNewest" />
      <None Include="scripts\**\*" CopyToOutputDirectory="PreserveNewest" />
    </ItemGroup>
</Project>
```

Create the folders under `Content/` and `Data/` now so you have somewhere to put files in the next steps.

> [!TIP]
> You only need to **run** the project to generate pack files. There is no long-lived game server process - `Program.cs` registers content and calls `Pack.Compile(...)`.

## What You Will Build

Across this tutorial you will create a small dirt-themed pack:

| Step | Content | Result in-game |
|------|---------|----------------|
| 1 | Item | `myaddon:dirt_soup` - edible soup that leaves a bowl |
| 2 | Block | `myaddon:compact_dirt` - placeable block with mining time and loot |
| 3 | Entity | `myaddon:dirtling` - passive mob with client visuals and spawn egg |
| 4 | Compile | Register everything, add textures, write `bp/` + `rp/` |
| 5 | Scripts | Optional block/item handlers and a tick service |
| 6 | Advanced | `.mcaddon`, `com.mojang`, UUIDs, and compile hooks |

> [!SUCCESS]
> Now you're all set up - leave `Program.cs` as-is for now. We'll come back to it in [Step 4: Compile Your Pack](compile.md).

**Next:** [1. Your First Item](your-first-item.md)
