using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace QuadrupleLib.SourceGeneration
{
    internal class GenericTypeSyntaxRewriter : CSharpSyntaxRewriter
    {
        public override SyntaxNode VisitGenericName(GenericNameSyntax node)
        {
            if (node.Identifier.Value as string == "Float128")
            {
                return SyntaxFactory.ParseTypeName("Float128");
            }
            else 
            {
                return base.VisitGenericName(node);
            }
        }
    }
}
