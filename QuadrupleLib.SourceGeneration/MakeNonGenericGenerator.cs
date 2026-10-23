using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Collections.Immutable;
using System.Linq;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace QuadrupleLib.SourceGeneration
{
    [Generator]
    public class MakeNonGenericGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var memberDeclarations = context.SyntaxProvider.ForAttributeWithMetadataName(
                "QuadrupleLib.Annotations.MakeNonGenericAttribute",
                predicate: (node, _) => node is MemberDeclarationSyntax,
                transform: (ctx, _) => (MemberDeclarationSyntax)ctx.TargetNode
            );

            context.RegisterSourceOutput(memberDeclarations.Collect(), Execute);
        }

        private void Execute(SourceProductionContext context, ImmutableArray<MemberDeclarationSyntax> members)
        {
            StructDeclarationSyntax structDecl = StructDeclaration("Float128")
                .AddModifiers(ParseToken("public"), ParseToken("partial"));

            var rewriter = new GenericTypeSyntaxRewriter();
            foreach (var member in members.Select(x => rewriter.Visit(x)).Cast<MemberDeclarationSyntax>()) 
            {
                structDecl = structDecl.AddMembers(member.WithAttributeLists(List<AttributeListSyntax>()));
            }

            NullableDirectiveTriviaSyntax directive = 
                NullableDirectiveTrivia(
                    Token(SyntaxKind.HashToken),
                    Token(SyntaxKind.NullableKeyword),
                    Token(SyntaxKind.EnableKeyword),
                    Token(SyntaxKind.None),
                    Token(SyntaxKind.EndOfDirectiveToken),
                    isActive: true
                    );

            NamespaceDeclarationSyntax nsDecl = 
                NamespaceDeclaration(ParseName("QuadrupleLib"))
                .AddMembers(structDecl);

            CompilationUnitSyntax compUnit = CompilationUnit()
                .AddUsings(
                    UsingDirective(ParseName("System.Numerics")),
                    UsingDirective(ParseName("System.Runtime.CompilerServices"))
                    )
                .AddMembers(nsDecl)
                .WithLeadingTrivia(Trivia(directive));

            context.AddSource("Float128.gen.cs", 
                compUnit.NormalizeWhitespace()
                .GetText(System.Text.Encoding.UTF8));
        }
    }
}
