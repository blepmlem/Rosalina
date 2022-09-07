using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.IO;

namespace Rosalina;

internal class UIDocumentAsset
{
    private const string _fileExtension = ".uxml";
    /// <summary>
    /// Gets the UI Document name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the UI document full path.
    /// </summary>
    public string FullPath { get; }
    
    
    /// <summary>
    /// Gets the class associated with the UI Document
    /// </summary>
    public ClassDeclarationSyntax OwnerClass { get; }

    /// <summary>
    /// Creates a new <see cref="UIDocumentAsset"/> instance that represents a document to be generated.
    /// </summary>
    /// <param name="uiDocumentPath">UXML UI document file path.</param>
    /// <exception cref="ArgumentException">Thrown when the given file path is null, empty or with only white spaces.</exception>
    public UIDocumentAsset(ClassDeclarationSyntax ownerClass)
    {
        OwnerClass = ownerClass;
        var path = ownerClass.SyntaxTree.FilePath;
        
        if (!path.EndsWith(".cs"))
        {
            throw new ArgumentException($"'{nameof(path)}' must be a realized script on disk", nameof(path));
        }

        path = Path.ChangeExtension(path, _fileExtension);
        Name = Path.GetFileNameWithoutExtension(path);
        FullPath = path;
    }
}