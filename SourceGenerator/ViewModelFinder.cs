using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Rosalina;

internal class ViewModelFinder : ISyntaxReceiver
{
	private readonly Type _targetInterface;

	public HashSet<ClassDeclarationSyntax> IdentifiedClasses { get; } = new();

	public ViewModelFinder(Type match) => _targetInterface = match;

	public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
	{
		if (syntaxNode is ClassDeclarationSyntax classDeclaration)
		{
			bool isPartial = false;
			foreach (SyntaxToken token in classDeclaration.Modifiers)
			{
				if (!token.IsKind(SyntaxKind.PartialKeyword))
				{
					continue;
				}

				isPartial = true;
				break;
			}

			if (!isPartial || classDeclaration.BaseList == null)
			{
				return;
			}

			foreach (SyntaxToken t in classDeclaration.BaseList.DescendantTokens())
			{
				if (t.Text != _targetInterface.Name)
				{
					continue;
				}

				IdentifiedClasses.Add(classDeclaration);
				return;
			}
		}
	}
}