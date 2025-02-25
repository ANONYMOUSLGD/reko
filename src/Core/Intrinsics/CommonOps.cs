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

using Reko.Core.Serialization;
using Reko.Core.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Reko.Core.Intrinsics
{
    /// <summary>
    /// Common intrinsics.
    /// </summary>
    public static class CommonOps
    {
        // Bit manipulation
        public static readonly IntrinsicProcedure Bit = new IntrinsicBuilder("__bit", false)
            .GenericTypes("TData", "TBit")
            .Param("TData").Param("TBit")
            .Returns(PrimitiveType.Bool);
        public static readonly IntrinsicProcedure ClearBit = new IntrinsicBuilder("__clear_bit", false)
            .GenericTypes("TData", "TBit")
            .Param("TData").Param("TBit")
            .Returns("TData");
        public static readonly IntrinsicProcedure InvertBit = new IntrinsicBuilder("__invert_bit", false)
            .GenericTypes("TData", "TBit")
            .Param("TData").Param("TBit")
            .Returns("TData");
        public static readonly IntrinsicProcedure SetBit = new IntrinsicBuilder("__set_bit", false)
            .GenericTypes("TData", "TBit")
            .Param("TData").Param("TBit")
            .Returns("TData");
        public static readonly IntrinsicProcedure WriteBit = new IntrinsicBuilder("__write_bit", false)
            .GenericTypes("TData", "TBit")
            .Param("TData").Param("TBit").Param(PrimitiveType.Bool)
            .Returns("TData");

        // Byte manipulations
        public static readonly IntrinsicProcedure ReverseBytes = IntrinsicBuilder.GenericUnary("__reverse_bytes");

        // Integer math
        public static readonly IntrinsicProcedure Abs = IntrinsicBuilder.GenericUnary("abs"); //$REVIEW: math.h
        public static readonly IntrinsicProcedure Max = IntrinsicBuilder.GenericBinary("max"); //$REVIEW: math.h
        public static readonly IntrinsicProcedure Min = IntrinsicBuilder.GenericBinary("min"); //$REVIEW: math.h

        // Rotations
        public static readonly IntrinsicProcedure Rol = new IntrinsicBuilder("__rol", false)
            .GenericTypes("TData", "TCount")
            .Param("TData").Param("TCount")
            .Returns("TData");
        public static readonly IntrinsicProcedure RolC = new IntrinsicBuilder("__rcl", false)
            .GenericTypes("TData", "TCount")
            .Param("TData").Param("TCount").Param(PrimitiveType.Bool)
            .Returns("TData");
        public static readonly IntrinsicProcedure Ror = new IntrinsicBuilder("__ror", false)
            .GenericTypes("TData", "TCount")
            .Param("TData").Param("TCount")
            .Returns("TData");
        public static readonly IntrinsicProcedure RorC = new IntrinsicBuilder("__rcr", false)
            .GenericTypes("TData", "TCount")
            .Param("TData").Param("TCount").Param(PrimitiveType.Bool)
            .Returns("TData");

        // Overflow
        public static readonly IntrinsicProcedure Overflow = new IntrinsicBuilder("OVERFLOW", false)
            .GenericTypes("T")
            .Param("T")
            .Returns(PrimitiveType.Bool);

        // Halt the processor
        public static readonly IntrinsicProcedure Halt = new IntrinsicBuilder(
            "__halt", true, new ProcedureCharacteristics
            {
                Terminates = true,
            }).Void();
        public static readonly IntrinsicProcedure Halt_1 = new IntrinsicBuilder(
            "__halt", true, new ProcedureCharacteristics
            {
                Terminates = true,
            })
            .GenericTypes("T")
            .Param("T")
            .Void();

        // System calls.
        public static readonly IntrinsicProcedure Syscall = new IntrinsicBuilder("__syscall", true)
            .Void();

        // __syscall overloads
        // For system calls with no arguments.
        public static readonly IntrinsicProcedure Syscall_0 = new IntrinsicBuilder("__syscall", true)
            .Void();
        // For system calls with one argument.
        public static readonly IntrinsicProcedure Syscall_1 = new IntrinsicBuilder("__syscall", true)
            .GenericTypes("T")
            .Param("T")
            .Void();
        // For system calls with two arguments.
        public static readonly IntrinsicProcedure Syscall_2 = new IntrinsicBuilder("__syscall", true)
            .GenericTypes("T1","T2")
            .Param("T1")
            .Param("T2")
            .Void();
    }
}
