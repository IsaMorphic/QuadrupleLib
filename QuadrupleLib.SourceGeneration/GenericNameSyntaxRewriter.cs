/*
 *  Copyright 2026 Chosen Few Software
 *  This file is part of QuadrupleLib.
 *
 *  QuadrupleLib is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Lesser General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  QuadrupleLib is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public License
 *  along with QuadrupleLib.  If not, see <https://www.gnu.org/licenses/>.
 */

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
