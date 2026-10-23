using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Collections.Immutable;
using System.Linq;
using System.Text;

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
            StructDeclarationSyntax @struct =
                StructDeclaration("Float128").AddModifiers(
                    Token(SyntaxKind.PublicKeyword),
                    Token(SyntaxKind.PartialKeyword)
                    );

            var rewriter = new GenericNameSyntaxRewriter(new[] { "Float128" });
            foreach (var member in members.Select(x => rewriter.Visit(x)).Cast<MemberDeclarationSyntax>())
            {
                var attributes = member.AttributeLists.SelectMany(x => x.Attributes);
                var newAttributes = attributes.Where(
                    x => !(x.Name is IdentifierNameSyntax n) ||
                    n.Identifier.Text != "MakeNonGeneric");

                MemberDeclarationSyntax newMember;
                if (newAttributes.Any())
                {
                    newMember = member.WithAttributeLists(
                        SingletonList(
                            AttributeList(
                                SeparatedList(newAttributes)
                                )
                            )
                        );
                }
                else
                {
                    newMember = member.WithAttributeLists(List<AttributeListSyntax>());
                }

                @struct = @struct.AddMembers(newMember);
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

            NamespaceDeclarationSyntax @namespace =
                NamespaceDeclaration(ParseName("QuadrupleLib"))
                .AddMembers(@struct);

            CompilationUnitSyntax unit = CompilationUnit()
                .AddUsings(
                    UsingDirective(ParseName("System.Numerics")),
                    UsingDirective(ParseName("System.Runtime.CompilerServices"))
                    )
                .AddMembers(@namespace)
                .WithLeadingTrivia(Trivia(directive));

            context.AddSource("Float128.gen.cs", unit
                .NormalizeWhitespace()
                .GetText(Encoding.UTF8));
        }
    }
}
