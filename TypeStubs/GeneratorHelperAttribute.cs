using System;

namespace Rosalina.TypeStubs;

public class GeneratorHelperAttribute : Attribute
{
	public GeneratorTarget GeneratorTarget { get; }
	public GeneratorHelperAttribute(GeneratorTarget generatorTarget) => GeneratorTarget = generatorTarget;
}