
namespace Rosalina;

/// <summary>
/// Describes a generation result.
/// </summary>
internal class RosalinaGenerationResult
{
    /// <summary>
    /// Gets the generated code.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Creates a new <see cref="RosalinaGenerationResult"/> instance.
    /// </summary>
    /// <param name="code">Generated code.</param>
    /// <param name="outputFilePath">Output file path.</param>
    public RosalinaGenerationResult(string code) => Code = code;
}
