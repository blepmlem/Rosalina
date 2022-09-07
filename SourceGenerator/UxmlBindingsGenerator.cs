using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Rosalina.TypeStubs;

namespace Rosalina;

[Generator]
public class UxmlBindingsGenerator : ISourceGenerator
{
	private readonly Type _targetType = typeof(ILayout);
	public void Initialize(GeneratorInitializationContext context)
	{
		context.RegisterForSyntaxNotifications(() => new ViewModelFinder(_targetType));
	}

	public void Execute(GeneratorExecutionContext context)
	{
		var syntaxReceiver = context.SyntaxReceiver as ViewModelFinder;

		if (!syntaxReceiver?.IdentifiedClasses.Any() ?? false)
		{
			return;
		}
		
		foreach (ClassDeclarationSyntax classDeclaration in syntaxReceiver!.IdentifiedClasses)
		{
			if (classDeclaration is null)
			{
				return;
			}
#if DEBUG
		if (!System.Diagnostics.Debugger.IsAttached)
		{
			System.Diagnostics.Debugger.Launch();
		}
#endif
			try
			{
				var bindings = new RosalinaBindingsGenerator().Generate(classDeclaration, new TargetTypeInfo(_targetType));
				SourceText code = SourceText.From(bindings.Code, Encoding.UTF8);
				context.AddSource($"{classDeclaration.Identifier.Text}.g", code);
			}
			catch (Exception)
			{
				// ignored
			}
		}
	}
}
