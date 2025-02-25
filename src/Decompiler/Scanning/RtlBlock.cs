#region License
/* 
 * Copyright (C) 1999-2022 John Källén.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2, or (at your option)
 * any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; see the file COPYING.  If not, write to
 * the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA.
 */
#endregion

using Reko.Core;
using Reko.Core.Rtl;
using System.Collections.Generic;

namespace Reko.Scanning
{
    /// <summary>
    /// A basic block of RTL clusters.
    /// </summary>
    public class RtlBlock
    {
        public RtlBlock(
            IProcessorArchitecture arch,
            Address addr,
            string id,
            int length,
            Address addrFallThrough,
            List<RtlInstructionCluster> instructions)
        {
            this.Architecture = arch;
            this.Name = id;
            this.Address = addr;
            this.Length = length;
            this.FallThrough = addrFallThrough;
            this.Instructions = instructions;
            this.IsValid = true;
        }

        public RtlBlock(Address addr, string id) : this(
            default!,
            addr,
            id,
            0,
            default!,
            new List<RtlInstructionCluster>())
        {
        }

        /// <summary>
        /// Address at which the block starts.
        /// </summary>
        public Address Address { get; }

        /// <summary>
        /// CPU architecture used to disassemble this block.
        /// </summary>
        public IProcessorArchitecture Architecture { get; }

        /// <summary>
        /// The address after the block if control flow falls through.
        /// Note that this is not necessarily <see cref="Address"/> + <see cref="Length"/>, because
        /// control instructions with delay slots may require skipping one extra instruction.
        /// </summary>
        public Address FallThrough { get; set; }

        /// <summary>
        /// Indicates whether this block is valid or not.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// <param name="Length">The size of the basic block starting 
        /// at <see cref="Address"/> and including the length of the final
        /// instruction.
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// Invariant identifier used for this block.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The instructions this block consists of.</param>
        /// <summary>
        public List<RtlInstructionCluster> Instructions { get; init; }

        // True if this block has been identified as a shared exit block
        public bool IsSharedExitBlock { get; set; }

        public Address GetEndAddress()
        {
            int iLast = Instructions.Count - 1;
            if (iLast < 0)
                return Address;
            var instr = Instructions[iLast];
            return instr.Address + instr.Length;
        }

        public override string ToString()
        {
            return string.Format("block({0})", Address);
        }
    }
}
