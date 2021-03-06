﻿/*
 * (c) 2008 MOSA - The Managed Operating System Alliance
 *
 * Licensed under the terms of the New BSD License.
 *
 * Authors:
 *  Michael Ruck (grover) <sharpos@michaelruck.de>
 */

namespace Mosa.Compiler.Framework.IR
{
	/// <summary>
	/// Intermediate representation of the signed division operation.
	/// </summary>
	/// <remarks>
	/// The mul instruction is a three-address instruction, where the result receives
	/// the value of the first operand (index 0) divided by the second operand (index 1).
	/// <para />
	/// Both the first and second operand must be the same integral type. If the second operand
	/// is statically or dynamically equal to or larger than the number of bits in the first
	/// operand, the result is undefined.
	/// </remarks>
	public sealed class DivSigned : ThreeOperandInstruction
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DivSigned"/>.
		/// </summary>
		public DivSigned()
		{
		}

		/// <summary>
		/// Allows visitor based dispatch for this instruction object.
		/// </summary>
		/// <param name="visitor">The visitor object.</param>
		/// <param name="context">The context.</param>
		public override void Visit(IIRVisitor visitor, Context context)
		{
			visitor.DivSigned(context);
		}

	}
}
