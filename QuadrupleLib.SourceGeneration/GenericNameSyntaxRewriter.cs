using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Collections.Generic;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace QuadrupleLib.SourceGeneration
{
    internal class GenericNameSyntaxRewriter : CSharpSyntaxRewriter
    {
        private readonly HashSet<string> _typeNames;

        public GenericNameSyntaxRewriter(IEnumerable<string> typeNames)
        {
            _typeNames = new HashSet<string>(typeNames);
        }

        public override SyntaxNode VisitGenericName(GenericNameSyntax node)
        {
            if (_typeNames.Contains(node.Identifier.Text))
            {
                return IdentifierName(node.Identifier);
            }
            else 
            {
                return base.VisitGenericName(node);
            }
        }
    }
}
