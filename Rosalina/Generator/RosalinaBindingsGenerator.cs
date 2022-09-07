using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using Rosalina.TypeStubs;

namespace Rosalina;

internal class RosalinaBindingsGenerator
{
    private TargetTypeInfo _targetTypeInfo;
    private const string RootVisualElementQueryMethodName = "Q";
    private const string PreserveAttributeName = "UnityEngine.Scripting.Preserve";
    
    private static readonly UsingDirectiveSyntax[] DefaultUsings = {
        SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("UnityEngine")),
        SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("UnityEngine.UIElements"))
    };

    public RosalinaGenerationResult Generate(ClassDeclarationSyntax classDeclaration, TargetTypeInfo targetTypeInfo)
    {
        _targetTypeInfo = targetTypeInfo;
        
        var document = new UIDocumentAsset(classDeclaration);
        if (document is null)
        {
            throw new ArgumentNullException(nameof(document), "Cannot generate binding with an empty UI document definition.");
        }

        UxmlDocument uxmlDocument = RosalinaUXMLParser.ParseUIDocument(document.FullPath);

        MemberDeclarationSyntax visualElementProperty = CreateVisualElementRootProperty();
        InitializationStatement[] statements = GenerateInitializeStatements(uxmlDocument);
        PropertyDeclarationSyntax[] propertyStatements = statements.Select(x => x.Property).ToArray();
        StatementSyntax[] initializationStatements = statements.Select(x => x.Statement).ToArray();

        MethodDeclarationSyntax initializeMethod = RosalinaSyntaxFactory.CreateMethod("void", _targetTypeInfo.InitializeBindingsMethod, SyntaxKind.PublicKeyword)
            .WithBody(SyntaxFactory.Block(initializationStatements));

        MemberDeclarationSyntax[] classMembers = propertyStatements
            .Append(visualElementProperty)
            .Append(initializeMethod)
            .ToArray();


        MemberDeclarationSyntax output = SyntaxFactory.ClassDeclaration(document.OwnerClass.Identifier)
                                                      .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                                                      .AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword))
                                                      .AddMembers(classMembers) .AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                                                          SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(PreserveAttributeName))
                                                      )));
        
        var nameSpaceIdentifier = GetNamespace(document.OwnerClass);

        if (!string.IsNullOrEmpty(nameSpaceIdentifier))
        {
            output = SyntaxFactory.NamespaceDeclaration(
                SyntaxFactory.ParseName(nameSpaceIdentifier)).AddMembers(output);
        }
 
        
        CompilationUnitSyntax compilationUnit = SyntaxFactory.CompilationUnit()
                                                             .AddUsings(DefaultUsings)
                                                             .AddMembers(output);

        string code = compilationUnit
            .NormalizeWhitespace()
            .ToFullString();

        return new RosalinaGenerationResult(code);
    }
    

    private MemberDeclarationSyntax CreateVisualElementRootProperty()
    {
        string propertyTypeName = "VisualElement";

        return RosalinaSyntaxFactory.CreateProperty(propertyTypeName, _targetTypeInfo.RootVisualElement, SyntaxKind.PublicKeyword)
	        .AddAccessorListAccessors(
		        SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
			        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
	        )
	        .AddAccessorListAccessors(
		        SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
			        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
	        );
    }

    private MemberAccessExpressionSyntax CreateRootQueryMethodAccessor()
    {
        return SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            SyntaxFactory.IdentifierName($"{_targetTypeInfo.RootVisualElement}?"),
            SyntaxFactory.Token(SyntaxKind.DotToken),
            SyntaxFactory.IdentifierName(RootVisualElementQueryMethodName)
        );
    }

    private InitializationStatement[] GenerateInitializeStatements(UxmlDocument uxmlDocument)
    {
        var statements = new List<InitializationStatement>();
        MemberAccessExpressionSyntax documentQueryMethodAccess = CreateRootQueryMethodAccessor();
        IEnumerable<UIProperty> properties = uxmlDocument.GetChildren().Select(x => new UIProperty(x.Type, x.Name)).ToList();

        if (CheckForDuplicateProperties(properties))
        {
            throw new InvalidProgramException($"Failed to generate bindings for document: {uxmlDocument.Name} because of duplicate properties.");
        }

        foreach (UIProperty uiProperty in properties)
        {
            if (uiProperty.Type is null)
            {
                // Debug.LogWarning($"[Rosalina]: Failed to get property type: '{uiProperty.TypeName}', field: '{uiProperty.Name}' for document '{uxmlDocument.Path}'. Property will be ignored.");
                continue;
            }

            PropertyDeclarationSyntax @property = RosalinaSyntaxFactory.CreateProperty(uiProperty.Type, uiProperty.Name, SyntaxKind.PublicKeyword)
                .AddAccessorListAccessors(
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                )
                .AddAccessorListAccessors(
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword))
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                );

            var argumentList = SyntaxFactory.SeparatedList(new[]
            {
                SyntaxFactory.Argument(
                    SyntaxFactory.LiteralExpression(
                        SyntaxKind.StringLiteralExpression,
                        SyntaxFactory.Literal(uiProperty.OriginalName)
                    )
                )
            });
            
            var cast = SyntaxFactory.CastExpression(
                SyntaxFactory.ParseTypeName(uiProperty.Type),
                SyntaxFactory.InvocationExpression(documentQueryMethodAccess, SyntaxFactory.ArgumentList(argumentList))
            );
            var statement = SyntaxFactory.ExpressionStatement(
                SyntaxFactory.AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    SyntaxFactory.IdentifierName(uiProperty.Name),
                    cast
                )
            );

            statements.Add(new InitializationStatement(statement, property));
        }

        return statements.ToArray();
    }

    private static bool CheckForDuplicateProperties(IEnumerable<UIProperty> properties)
    {
        var duplicatePropertyGroups = properties.GroupBy(x => x.Name).Where(g => g.Count() > 1).ToArray();
        bool containsDuplicateProperties = duplicatePropertyGroups.Any();

        if (containsDuplicateProperties)
        {
            foreach (var property in duplicatePropertyGroups)
            {
                string duplicateProperties = string.Join(", ", property.Select(x => $"{x.OriginalName}"));

                // Debug.LogError($"Conflict detected between {duplicateProperties}.");
            }
        }

        return containsDuplicateProperties;
    }
    
    static string GetNamespace(BaseTypeDeclarationSyntax syntax)
    {
        // If we don't have a namespace at all we'll return an empty string
        // This accounts for the "default namespace" case
        string nameSpace = string.Empty;

        // Get the containing syntax node for the type declaration
        // (could be a nested type, for example)
        SyntaxNode potentialNamespaceParent = syntax.Parent;
    
        // Keep moving "out" of nested classes etc until we get to a namespace
        // or until we run out of parents
        while (potentialNamespaceParent != null &&
               potentialNamespaceParent is not NamespaceDeclarationSyntax)
        {
            potentialNamespaceParent = potentialNamespaceParent.Parent;
        }

        // Build up the final namespace by looping until we no longer have a namespace declaration
        if (potentialNamespaceParent is NamespaceDeclarationSyntax namespaceParent)
        {
            // We have a namespace. Use that as the type
            nameSpace = namespaceParent.Name.ToString();
        
            // Keep moving "out" of the namespace declarations until we 
            // run out of nested namespace declarations
            while (true)
            {
                if (namespaceParent.Parent is not NamespaceDeclarationSyntax parent)
                {
                    break;
                }

                // Add the outer namespace as a prefix to the final namespace
                nameSpace = $"{namespaceParent.Name}.{nameSpace}";
                namespaceParent = parent;
            }
        }

        // return the final namespace
        return nameSpace;
    }

    private struct InitializationStatement
    {
        public StatementSyntax Statement { get; }

        public PropertyDeclarationSyntax Property { get; }

        public InitializationStatement(StatementSyntax statement, PropertyDeclarationSyntax property)
        {
            Statement = statement;
            Property = property;
        }
    }
}

