namespace ingot.Core.Scripting;

/// <summary>
/// A Script API service registered on a <see cref="Pack"/>.
/// </summary>
/// <param name="SourceFile">Path to the JavaScript source file.</param>
/// <param name="RelativePath">Behaviour-pack relative output path.</param>
internal readonly record struct ScriptServiceRegistration(string SourceFile, string RelativePath);