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
using Reko.Core.Machine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reko.Core.Types;
using System.Diagnostics;
using Reko.Core.Lib;
using Reko.Core.Services;
using Reko.Core.Memory;

namespace Reko.Arch.SuperH
{
    using Decoder = Decoder<SuperHDisassembler, Mnemonic, SuperHInstruction>;
#pragma warning disable IDE1006

    // http://www.shared-ptr.com/sh_insns.html

    public class SuperHDisassembler : DisassemblerBase<SuperHInstruction, Mnemonic>
    {
        private readonly SuperHArchitecture arch;
        private readonly EndianImageReader rdr;
        private readonly DasmState state;
        private Address addr;

        public SuperHDisassembler(SuperHArchitecture arch, EndianImageReader rdr)
        {
            this.arch = arch;
            this.rdr = rdr;
            this.state = new DasmState();
            this.addr = null!;
        }

        public override SuperHInstruction? DisassembleInstruction()
        {
            this.addr = rdr.Address;
            if (!rdr.TryReadUInt16(out ushort uInstr))
                return null;
            var instr = decoders[uInstr >> 12].Decode(uInstr, this);
            state.Clear();
            instr.Address = addr;
            instr.Length = 2;
            return instr;
        }

        public override SuperHInstruction MakeInstruction(InstrClass iclass, Mnemonic mnemonic)
        {
            var instr = new SuperHInstruction
            {
                Mnemonic = mnemonic,
                InstructionClass = iclass,
                Operands = state.ops.ToArray()
            };
            return instr;
        }

        public override SuperHInstruction CreateInvalidInstruction()
        {
            return new SuperHInstruction
            {
                InstructionClass = InstrClass.Invalid,
                Mnemonic = Mnemonic.invalid,
                Operands = MachineInstruction.NoOperands
            };
        }

        public class DasmState
        {
            public List<MachineOperand> ops = new List<MachineOperand>();
            public Mnemonic mnemonic;
            public InstrClass iclass;
            public RegisterStorage? reg;

            public void Clear()
            {
                ops.Clear();
            }
        }

        public override SuperHInstruction NotYetImplemented(string message)
        {
            var testGenSvc = arch.Services.GetService<ITestGenerationService>();
            testGenSvc?.ReportMissingDecoder("SuperH", this.addr, this.rdr, message);
            return CreateInvalidInstruction();
        }

        // Mutators

        private static bool r1(uint uInstr, SuperHDisassembler dasm)
        {
            var reg = (uInstr >> 8) & 0xF;
            dasm.state.ops.Add(Registers.gpregs[reg]);
            return true;
        }

        private static bool r2(uint uInstr, SuperHDisassembler dasm)
        {
            var reg = (uInstr >> 4) & 0xF;
            dasm.state.ops.Add(Registers.gpregs[reg]);
            return true;
        }

        private static bool r3(uint uInstr, SuperHDisassembler dasm)
        {
            var reg = uInstr & 0xF;
            dasm.state.ops.Add(Registers.gpregs[reg]);
            return true;
        }

        private static bool R0(uint uInstr, SuperHDisassembler dasm)
        {
            dasm.state.ops.Add(Registers.r0);
            return true;
        }
        private static bool RBank2_3bit(uint uInstr, SuperHDisassembler dasm)
        {
            var reg = (uInstr >> 4) & 0b0111;
            dasm.state.ops.Add(Registers.rbank[reg]);
            return true;
        }


        private static bool f1(uint uInstr, SuperHDisassembler dasm)
        {
            var reg = (uInstr >> 8) & 0xF;
            dasm.state.ops.Add(Registers.fpregs[reg]);
            return true;
        }

        private static bool f2(uint uInstr, SuperHDisassembler dasm)
        {
            var reg = (uInstr >> 4) & 0xF;
            dasm.state.ops.Add(Registers.fpregs[reg]);
            return true;
        }

        private static bool F0(uint uInstr, SuperHDisassembler dasm)
        {
            dasm.state.ops.Add(Registers.fr0);
            return true;
        }

        private static bool d1(uint uInstr, SuperHDisassembler dasm)
        {
            var reg = (uInstr >> (1 + 8)) & 0x7;
            dasm.state.ops.Add(Registers.dfpregs[reg]);
            return true;
        }

        private static bool d2(uint uInstr, SuperHDisassembler dasm)
        {
            var reg = (uInstr >> (1 + 4)) & 0x7;
            dasm.state.ops.Add(Registers.dfpregs[reg]);
            return true;
        }

        private static bool xd2(uint uInstr, SuperHDisassembler dasm)
        {
            var reg = (uInstr >> (1 + 4)) & 0x7;
            dasm.state.ops.Add(Registers.XdRegisters[reg]);
            return true;
        }

        private static bool v1(uint uInstr, SuperHDisassembler dasm)
        {
            var reg = (uInstr >> 10) & 0x3;
            dasm.state.ops.Add(Registers.vfpregs[reg]);
            return true;
        }

        private static bool v2(uint uInstr, SuperHDisassembler dasm)
        {
            var reg = (uInstr >> 8) & 0x3;
            dasm.state.ops.Add(Registers.vfpregs[reg]);
            return true;
        }

        private static bool pr(uint uInstr, SuperHDisassembler dasm)
        {
            dasm.state.ops.Add(Registers.pr);
            return true;
        }

        private static bool sr(uint uInstr, SuperHDisassembler dasm)
        {
            dasm.state.ops.Add(Registers.sr);
            return true;
        }

        private static bool gbr(uint uInstr, SuperHDisassembler dasm)
        {
            dasm.state.ops.Add(Registers.gbr);
            return true;
        }

        private static bool RK(uint uInstr, SuperHDisassembler dasm)
        {
            dasm.state.ops.Add(Registers.spc);
            return true;
        }

        private static bool tbr(uint uInstr, SuperHDisassembler dasm)
        {
            dasm.state.ops.Add(Registers.tbr);
            return true;
        }

        private static bool RV(uint uInstr, SuperHDisassembler dasm)
        {
            dasm.state.ops.Add(Registers.vbr);
            return true;
        }

        private static bool mod(uint uInstr, SuperHDisassembler dasm)
        {
            dasm.state.ops.Add(Registers.mod);
            return true;
        }

        private static bool ssr(uint uInstr, SuperHDisassembler dasm)
        {
            dasm.state.ops.Add(Registers.ssr);
            return true;
        }

        private static bool rs(uint uInstr, SuperHDisassembler dasm)
        {
            dasm.state.ops.Add(Registers.rs);
            return true;
        }

        private static bool spc(uint uInstr, SuperHDisassembler dasm)
        {
            dasm.state.ops.Add(Registers.spc);
            return true;
        }

        private static bool mh(uint uInstr, SuperHDisassembler dasm)
        {
            dasm.state.ops.Add(Registers.mach);
            return true;
        }

        private static bool ml(uint uInstr, SuperHDisassembler dasm)
        {
            dasm.state.ops.Add(Registers.macl);
            return true;
        }

        private static bool fpul(uint uInstr, SuperHDisassembler dasm)
        {
            dasm.state.ops.Add(Registers.fpul);
            return true;
        }

        private static bool xmtrx(uint uInstr, SuperHDisassembler dasm)
        {
            dasm.state.ops.Add(Registers.xmtrx);
            return true;
        }

        private static bool dsr(uint uInstr, SuperHDisassembler dasm)
        {
            dasm.state.ops.Add(Registers.dsr);
            return true;
        }

        private static bool dbr(uint uInstr, SuperHDisassembler dasm)
        {
            dasm.state.ops.Add(Registers.dbr);
            return true;
        }

        private static bool sgr(uint uInstr, SuperHDisassembler dasm)
        {
            dasm.state.ops.Add(Registers.sgr);
            return true;
        }


        private static bool I(uint uInstr, SuperHDisassembler dasm)
        {
            dasm.state.ops.Add(ImmediateOperand.Byte((byte)uInstr));
            return true;
        }

        private static bool j(uint uInstr, SuperHDisassembler dasm)
        {
            dasm.state.ops.Add(AddressOperand.Create(dasm.rdr.Address + (2 + 2 * (sbyte)uInstr)));
            return true;
        }

        private static bool J(uint uInstr, SuperHDisassembler dasm)
        {
            int offset = ((int)uInstr << 20) >> 19;
            dasm.state.ops.Add(AddressOperand.Create(dasm.rdr.Address + (2 + offset)));
            return true;
        }

        private static bool Ind1b(uint uInstr, SuperHDisassembler dasm)
        {
            var reg = Registers.gpregs[(uInstr >> 8) & 0x0F];
            dasm.state.ops.Add(MemoryOperand.Indirect(PrimitiveType.Byte, reg));
            return true;
        }

        private static bool Ind1w(uint uInstr, SuperHDisassembler dasm)
        {
            var reg = Registers.gpregs[(uInstr >> 8) & 0x0F];
            dasm.state.ops.Add(MemoryOperand.Indirect(PrimitiveType.Word16, reg));
            return true;
        }

        private static bool Ind1l(uint uInstr, SuperHDisassembler dasm)
        {
            var reg = Registers.gpregs[(uInstr >> 8) & 0x0F];
            dasm.state.ops.Add(MemoryOperand.Indirect(PrimitiveType.Word32, reg));
            return true;
        }

        private static bool Ind1d(uint uInstr, SuperHDisassembler dasm)
        {
            var reg = Registers.gpregs[(uInstr >> 8) & 0x0F];
            dasm.state.ops.Add(MemoryOperand.Indirect(PrimitiveType.Word64, reg));
            return true;
        }

        private static bool Ind2b(uint uInstr, SuperHDisassembler dasm)
        {
            var reg = Registers.gpregs[(uInstr >> 4) & 0x0F];
            dasm.state.ops.Add(MemoryOperand.Indirect(PrimitiveType.Byte, reg));
            return true;
        }

        private static bool Ind2w(uint uInstr, SuperHDisassembler dasm)
        {
            var reg = Registers.gpregs[(uInstr >> 4) & 0x0F];
            dasm.state.ops.Add(MemoryOperand.Indirect(PrimitiveType.Word16, reg));
            return true;
        }

        private static bool Ind2l(uint uInstr, SuperHDisassembler dasm)
        {
            var reg = Registers.gpregs[(uInstr >> 4) & 0x0F];
            dasm.state.ops.Add(MemoryOperand.Indirect(PrimitiveType.Word32, reg));
            return true;
        }

        private static bool Pre1b(uint uInstr, SuperHDisassembler dasm)
        {
            var reg = Registers.gpregs[(uInstr >> 8) & 0x0F];
            dasm.state.ops.Add(MemoryOperand.IndirectPreDecr(PrimitiveType.Byte, reg));
            return true;
        }

        private static bool Pre1w(uint uInstr, SuperHDisassembler dasm)
        {
            var reg = Registers.gpregs[(uInstr >> 8) & 0x0F];
            dasm.state.ops.Add(MemoryOperand.IndirectPreDecr(PrimitiveType.Word16, reg));
            return true;
        }

        private static bool Pre1l(uint uInstr, SuperHDisassembler dasm)
        {
            var reg = Registers.gpregs[(uInstr >> 8) & 0x0F];
            dasm.state.ops.Add(MemoryOperand.IndirectPreDecr(PrimitiveType.Word32, reg));
            return true;
        }

        private static bool Pre1d(uint uInstr, SuperHDisassembler dasm)
        {
            var reg = Registers.gpregs[(uInstr >> 8) & 0x0F];
            dasm.state.ops.Add(MemoryOperand.IndirectPreDecr(PrimitiveType.Word64, reg));
            return true;
        }

        private static bool Pre15l(uint uInstr, SuperHDisassembler dasm)
        {
            var reg = Registers.r15;
            dasm.state.ops.Add(MemoryOperand.IndirectPreDecr(PrimitiveType.Word32, reg));
            return true;
        }

        private static bool Post1w(uint uInstr, SuperHDisassembler dasm)
        {
            var reg = Registers.gpregs[(uInstr >> 8) & 0x0F];
            dasm.state.ops.Add(MemoryOperand.IndirectPostIncr(PrimitiveType.Word16, reg));
            return true;
        }

        private static bool Post1l(uint uInstr, SuperHDisassembler dasm)
        {
            var reg = Registers.gpregs[(uInstr >> 8) & 0x0F];
            dasm.state.ops.Add(MemoryOperand.IndirectPostIncr(PrimitiveType.Word32, reg));
            return true;
        }

        private static bool Post1d(uint uInstr, SuperHDisassembler dasm)
        {
            var reg = Registers.gpregs[(uInstr >> 8) & 0x0F];
            dasm.state.ops.Add(MemoryOperand.IndirectPostIncr(PrimitiveType.Word64, reg));
            return true;
        }

        private static bool Post2b(uint uInstr, SuperHDisassembler dasm)
        {
            var reg = Registers.gpregs[(uInstr >> 4) & 0x0F];
            dasm.state.ops.Add(MemoryOperand.IndirectPostIncr(PrimitiveType.Byte, reg));
            return true;
        }

        private static bool Post2w(uint uInstr, SuperHDisassembler dasm)
        {
            var reg = Registers.gpregs[(uInstr >> 4) & 0x0F];
            dasm.state.ops.Add(MemoryOperand.IndirectPostIncr(PrimitiveType.Word16, reg));
            return true;
        }

        private static bool Post2l(uint uInstr, SuperHDisassembler dasm)
        {
            var reg = Registers.gpregs[(uInstr >> 4) & 0x0F];
            dasm.state.ops.Add(MemoryOperand.IndirectPostIncr(PrimitiveType.Word32, reg));
            return true;
        }

        private static bool Post2d(uint uInstr, SuperHDisassembler dasm)
        {
            var reg = Registers.gpregs[(uInstr >> 4) & 0x0F];
            dasm.state.ops.Add(MemoryOperand.IndirectPostIncr(PrimitiveType.Word64, reg));
            return true;
        }

        private static bool Post15l(uint uInstr, SuperHDisassembler dasm)
        {
            var reg = Registers.r15;
            dasm.state.ops.Add(MemoryOperand.IndirectPostIncr(PrimitiveType.Word32, reg));
            return true;
        }

        // Indirect indexed
        private static bool X1b(uint uInstr, SuperHDisassembler dasm)
        {
            var iReg = (uInstr >> 8) & 0xF;
            dasm.state.ops.Add(MemoryOperand.IndexedIndirect(PrimitiveType.Byte, Registers.gpregs[iReg]));
            return true;
        }

        private static bool X1w(uint uInstr, SuperHDisassembler dasm)
        {
            var iReg = (uInstr >> 8) & 0xF;
            dasm.state.ops.Add(MemoryOperand.IndexedIndirect(PrimitiveType.Word16, Registers.gpregs[iReg]));
            return true;
        }

        private static bool X1l(uint uInstr, SuperHDisassembler dasm)
        {
            var iReg = (uInstr >> 8) & 0xF;
            dasm.state.ops.Add(MemoryOperand.IndexedIndirect(PrimitiveType.Word32, Registers.gpregs[iReg]));
            return true;
        }

        private static bool X1d(uint uInstr, SuperHDisassembler dasm)
        {
            var iReg = (uInstr >> 8) & 0xF;
            dasm.state.ops.Add(MemoryOperand.IndexedIndirect(PrimitiveType.Word64, Registers.gpregs[iReg]));
            return true;
        }

        private static bool X2b(uint uInstr, SuperHDisassembler dasm)
        {
            var iReg = (uInstr >> 4) & 0xF;
            dasm.state.ops.Add(MemoryOperand.IndexedIndirect(PrimitiveType.Byte, Registers.gpregs[iReg]));
            return true;
        }

        private static bool X2w(uint uInstr, SuperHDisassembler dasm)
        {
            var iReg = (uInstr >> 4) & 0xF;
            dasm.state.ops.Add(MemoryOperand.IndexedIndirect(PrimitiveType.Word16, Registers.gpregs[iReg]));
            return true;
        }

        private static bool X2l(uint uInstr, SuperHDisassembler dasm)
        {
            var iReg = (uInstr >> 4) & 0xF;
            dasm.state.ops.Add(MemoryOperand.IndexedIndirect(PrimitiveType.Word32, Registers.gpregs[iReg]));
            return true;
        }


        // indirect with displacement
        private static bool D1l(uint uInstr, SuperHDisassembler dasm)
        {
            var reg = Registers.gpregs[(uInstr >> 8) & 0xF];
            var width = PrimitiveType.Word32;
            dasm.state.ops.Add(MemoryOperand.IndirectDisplacement(width, reg, (int)(uInstr & 0xF) * width.Size));
            return true;
        }

        private static bool D2b(uint uInstr, SuperHDisassembler dasm)
        {
            var reg = Registers.gpregs[(uInstr >> 4) & 0xF];
            var width = PrimitiveType.Byte;
            dasm.state.ops.Add(MemoryOperand.IndirectDisplacement(width, reg, (int)(uInstr & 0xF) * width.Size));
            return true;
        }

        private static bool D2w(uint uInstr, SuperHDisassembler dasm)
        {
            var reg = Registers.gpregs[(uInstr >> 4) & 0xF];
            var width = PrimitiveType.Word16;
            dasm.state.ops.Add(MemoryOperand.IndirectDisplacement(width, reg, (int)(uInstr & 0xF) * width.Size));
            return true;
        }

        private static bool D2l(uint uInstr, SuperHDisassembler dasm)
        {
            var reg = Registers.gpregs[(uInstr >> 4) & 0xF];
            var width = PrimitiveType.Word32;
            dasm.state.ops.Add(MemoryOperand.IndirectDisplacement(width, reg, (int)(uInstr & 0xF) * width.Size));
            return true;
        }



        // PC-relative with displacement
        private static bool Pw(uint uInstr, SuperHDisassembler dasm)
        {
            var width = PrimitiveType.Word16;
            dasm.state.ops.Add(MemoryOperand.PcRelativeDisplacement(width, width.Size * (byte)uInstr));
            return true;
        }

        private static bool Pl(uint uInstr, SuperHDisassembler dasm)
        {
            var width = PrimitiveType.Word32;
            dasm.state.ops.Add(MemoryOperand.PcRelativeDisplacement(width, width.Size * (byte)uInstr));
            return true;
        }

        private static bool Gb(uint uInstr, SuperHDisassembler dasm)
        {
            dasm.state.ops.Add(MemoryOperand.GbrIndexedIndirect(PrimitiveType.Byte));
            return true;
        }

        // Factory methods

        private static Decoder Instr(Mnemonic mnemonic, params Mutator<SuperHDisassembler>[] mutators)
        {
            return new InstrDecoder<SuperHDisassembler, Mnemonic, SuperHInstruction>(InstrClass.Linear, mnemonic, mutators);
        }

        private static Decoder Instr(Mnemonic mnemonic, InstrClass iclass, params Mutator<SuperHDisassembler>[] mutators)
        {
            return new InstrDecoder<SuperHDisassembler, Mnemonic, SuperHInstruction>(iclass, mnemonic, mutators);
        }

        // Predicates

        private static bool Ne0(uint n)
        {
            return n != 0;
        }

        // Decoders

        private static readonly Decoder invalid = Instr(Mnemonic.invalid);

        /*
1111101111111101 ~ FRCHG ~FPSCR.FR → SPFCR.FR 
1111001111111101 ~ FSCHG ~FPSCR.SZ → SPFCR.SZ 
1111nn0111111101 ~ FTRV XMTRX,FVn transform_vector [XMTRX, FVn] → FVn 
*/

        private static readonly Decoder decode_FxFD = Mask(8, 4,
            Instr(Mnemonic.fsca, fpul, f1),
            Instr(Mnemonic.ftrv, xmtrx, v2),
            Instr(Mnemonic.fsca, fpul, f1),
            Instr(Mnemonic.fschg),

            Instr(Mnemonic.fsca, fpul, f1),
            Instr(Mnemonic.ftrv, xmtrx, v2),
            Instr(Mnemonic.fsca, fpul, f1),
            invalid,

            Instr(Mnemonic.fsca, fpul, f1),
            Instr(Mnemonic.ftrv, xmtrx, v2),
            Instr(Mnemonic.fsca, fpul, f1),
            Instr(Mnemonic.frchg),

            Instr(Mnemonic.fsca, fpul, f1),
            Instr(Mnemonic.ftrv, xmtrx, v2),
            Instr(Mnemonic.fsca, fpul, f1),
            invalid);

        /*
1111nnnn 0000 1101 ~ FSTS FPUL,FRn FPUL → FRn 
1111mmmm 0001 1101 ~ FLDS FRm,FPUL FRm → FPUL 
1111nnn0 0010 1101 ~ FLOAT FPUL,DRn (float)FPUL → DRn 
1111nnnn 0100 1101 ~ FNEG FRn FRn ∧ 0x80000000 → FRn 
1111nnn0 0100 1101 ~ FNEG DRn DRn ^ 0x8000 0000 0000 0000 → DRn 
1111mmmm 0011 1101 ~ FTRC FRm,FPUL (long) FRm → FPUL 
1111mmm0 0011 1101 ~ FTRC DRm,FPUL (long) DRm → FPUL 
1111nnnn 0101 1101 ~ FABS FRn FRn & 0x7FFF FFFF → FRn 
1111nnn0 0101 1101 ~ FABS DRn DRn & 0x7FFF FFFF FFFF FFFF → DRn 
1111nnnn 0010 1101 ~ FLOAT FPUL,FRn (float) FPUL → FRn 
1111nnnn 0110 1101 ~ FSQRT FRn √FRn → FRn 
1111nnn0 0110 1101 ~ FSQRT DRn √DRn → DRn 
1111nnnn 1000 1101 ~ FLDI0 FRn 0x00000000 → FRn 
1111nnnn 1001 1101 ~ FLDI1 FRn 0x3F800000 → FRn 
1111nnn0 1010 1101 ~ FCNVSD FPUL,DRn float_to_ double [FPUL] → DRn 
1111mmm0 1011 1101 ~ FCNVDS DRm,FPUL double_to_ float[DRm] → FPUL 
1111nnmm 1110 1101 ~ FIPR FVm,FVn inner_product [FVm, FVn] → FR[n+3] 
*/

        private static readonly Decoder decode_FxxD = Sparse(4, 4, "  FxxD", invalid,
            (0x00, Mask(8, 1, 
                Instr(Mnemonic.fsts, fpul, d1),
                Instr(Mnemonic.fsts, fpul, f1))),
            (0x01, Mask(8, 1,
                Instr(Mnemonic.flds, d1, fpul),
                Instr(Mnemonic.flds, f1, fpul))),
            (0x02, Mask(8, 1, 
                Instr(Mnemonic.@float, fpul, d1),
                Instr(Mnemonic.@float, fpul, f1))),
            (0x03, Mask(8, 1,
                Instr(Mnemonic.ftrc, d1, fpul),
                Instr(Mnemonic.ftrc, f1, fpul))),
            (0x04, Mask(8, 1,
                Instr(Mnemonic.fneg, d1),
                Instr(Mnemonic.fneg, f1))),
            (0x05, Mask(8, 1,
                Instr(Mnemonic.fabs, d1),
                Instr(Mnemonic.fabs, f1))),
            (0x06, Mask(8, 1,
                Instr(Mnemonic.fsqrt, d1),
                Instr(Mnemonic.fsqrt, f1))),
            (0x08, Instr(Mnemonic.fldi0, f1)),
            (0x09, Instr(Mnemonic.fldi1, f1)),
            (0x0A, Instr(Mnemonic.fcnvsd, fpul, d1)),
            (0x0B, Instr(Mnemonic.fcnvds, d1, fpul)),
            (0x0E, Instr(Mnemonic.fipr, v2, v1)),
            (0x0F, decode_FxFD));

        private static readonly Bitfield[] bfFpuBinop = new[]
        {
            new Bitfield(8, 1),
            new Bitfield(4, 1)
        };

        private static readonly Decoder[] decoders = new Decoder[]
        {
            // 0...
            Sparse(0, 4, "  0...", invalid,
                (0x0, Instr(Mnemonic.invalid, InstrClass.Invalid|InstrClass.Zero) ),
                (0x1, invalid ),
                (0x02, Mask(7, 1,
                    Mask(4, 3,
                        Instr(Mnemonic.stc, sr,r1),
                        Instr(Mnemonic.stc, gbr,r1),
                        Instr(Mnemonic.stc, gbr,r1),
                        Instr(Mnemonic.stc, ssr,r1),
                        Instr(Mnemonic.stc, RK,r1),
                        Instr(Mnemonic.stc, mod,r1),
                        Instr(Mnemonic.stc, rs,r1),
                        invalid),
                    Instr(Mnemonic.stc, RBank2_3bit,r1))
                ),
                (0x03, Sparse(4, 4,  "  0..3", invalid,
                        ( 0x0, Instr(Mnemonic.bsrf, r1)),
                        ( 0x1, invalid),
                        ( 0x2, Instr(Mnemonic.braf, r1)),
                        ( 0x3, invalid),
                        ( 0x7, Instr(Mnemonic.movco_l, R0,Ind1l)),
                        ( 0x8, Instr(Mnemonic.pref, Ind1l)),
                        ( 0x9, Instr(Mnemonic.ocbi, Ind1b)),
                        ( 0xA, Instr(Mnemonic.ocbp, Ind1b)),
                        ( 0xC, Instr(Mnemonic.movca_l, R0,Ind1l))
                        )
                ),
                (0x4, Instr(Mnemonic.mov_b, r2,X1b) ),
                (0x5, Instr(Mnemonic.mov_w, r2,X1w) ),
                (0x6, Instr(Mnemonic.mov_l, r2,X1l) ),
                (0x7, Instr(Mnemonic.mul_l, r2,r1) ),
                (0x8, Select((8, 4), Ne0,
                    invalid,
                    Mask(4, 4,
                        Instr(Mnemonic.clrt),
                        Instr(Mnemonic.sett),
                        Instr(Mnemonic.clrmac),
                        Instr(Mnemonic.ldtlb),

                        Instr(Mnemonic.clrs),
                        invalid,
                        invalid,
                        invalid,

                        invalid,
                        invalid,
                        invalid,
                        invalid,

                        invalid,
                        invalid,
                        invalid,
                        invalid))
                ),
                (0x9, Sparse(4, 4, "  9", invalid,
                        ( 0x0, Select((8,4),Ne0,invalid, Instr(Mnemonic.nop))),
                        ( 0x1, Select((8,4),Ne0,invalid, Instr(Mnemonic.div0u))),
                        ( 0x2, Instr(Mnemonic.movt, r1)),
                        ( 0x5, invalid),
                        ( 0xD, invalid),
                        ( 0xF, invalid))
                ),
                (0xA, Sparse(4, 4, "  A", invalid,
                        ( 0x0, Instr(Mnemonic.sts, mh,r1) ),
                        ( 0x1, Instr(Mnemonic.sts, ml,r1) ),
                        ( 0x2, Instr(Mnemonic.sts, pr,r1) ),
                        ( 0x4, Instr(Mnemonic.sts, tbr,r1) ),
                        ( 0x5, Instr(Mnemonic.sts, fpul,r1) ),
                        ( 0x6, Instr(Mnemonic.sts, dsr,r1) ),
                        ( 0x8, invalid ),   // DSP:sts X0,r1
                        ( 0x9, invalid ),   // DSP:sts X1,r1
                        ( 0xA, invalid ),   // DSP:lds	Rm,Y1
                        ( 0xF, Instr(Mnemonic.stc, dbr,r1))
                    )
                ),
                (0xB, Select((8, 4), Ne0,
                    invalid,
                    Sparse(4, 4, "  B", invalid,
                        (0x0, Instr(Mnemonic.rts, InstrClass.Transfer|InstrClass.Return|InstrClass.Delay)),
                        (0x1, Instr(Mnemonic.sleep)),
                        (0x2, Instr(Mnemonic.rte, InstrClass.Transfer|InstrClass.Return|InstrClass.Delay)),
                        (0x3, Instr(Mnemonic.brk))))
                ),
                (0xC, Instr(Mnemonic.mov_b, X2b,r1) ),
                (0xD, Instr(Mnemonic.mov_w, X2w,r1) ),
                (0xE, Instr(Mnemonic.mov_l, X2l,r1) ),
                (0xF, Instr(Mnemonic.mac_l, Post2l,Post1l))
            ),
            Instr(Mnemonic.mov_l, r2,D1l),
            // 2...
            Mask(0, 4, new Decoder[]
            {
                Instr(Mnemonic.mov_b, r2,Ind1b),
                Instr(Mnemonic.mov_w, r2,Ind1w),
                Instr(Mnemonic.mov_l, r2,Ind1l),
                invalid,

                Instr(Mnemonic.mov_b, r2,Pre1b),
                Instr(Mnemonic.mov_w, r2,Pre1w),
                Instr(Mnemonic.mov_l, r2,Pre1l),
                Instr(Mnemonic.div0s, r2,r1),

                Instr(Mnemonic.tst, r2,r1),
                Instr(Mnemonic.and, r2,r1),
                Instr(Mnemonic.xor, r2,r1),
                Instr(Mnemonic.or, r2,r1),

                Instr(Mnemonic.cmp_str, r2,r1),
                Instr(Mnemonic.xtrct, r2,r1),
                Instr(Mnemonic.mulu_w, r2,r1),
                Instr(Mnemonic.muls_w, r2,r1),
            }),
            // 3...
            Mask(0, 4, new Decoder[]
            {
                Instr(Mnemonic.cmp_eq, r2,r1),
                invalid,
                Instr(Mnemonic.cmp_hs, r2,r1),
                Instr(Mnemonic.cmp_ge, r2,r1),

                Instr(Mnemonic.div1, r2,r1),
                Instr(Mnemonic.dmulu_l, r2,r1),
                Instr(Mnemonic.cmp_hi, r2,r1),
                Instr(Mnemonic.cmp_gt, r2,r1),

                Instr(Mnemonic.sub, r2,r1),
                invalid,
                Instr(Mnemonic.subc, r2,r1),
                Instr(Mnemonic.subv, r2,r1),

                Instr(Mnemonic.add, r2,r1),
                Instr(Mnemonic.dmuls_l, r2,r1),
                Instr(Mnemonic.addc, r2,r1),
                Instr(Mnemonic.addv, r2,r1),
            }),

            // 4...
            Sparse(0, 8, "  4", invalid,
                ( 0x00, Instr(Mnemonic.shll, r1)),
                ( 0x01, Instr(Mnemonic.shlr, r1)),
                ( 0x04, Instr(Mnemonic.rotl, r1)),
                ( 0x05, Instr(Mnemonic.rotr, r1)),
                ( 0x06, Instr(Mnemonic.lds_l, Post1l,mh)),
                ( 0x08, Instr(Mnemonic.shll2, r1)),
                ( 0x09, Instr(Mnemonic.shlr, r1)),
                ( 0x0B, Instr(Mnemonic.jsr, Ind1l)),
                ( 0x0C, Instr(Mnemonic.shad, r2,r1)),
                ( 0x0E, Instr(Mnemonic.ldc, r1,sr)),
                ( 0x13, Instr(Mnemonic.stc_l, gbr,Pre1l)),
                ( 0x14, invalid),  // DSP setrc r1
                ( 0x1B, Instr(Mnemonic.tas_b, Ind1b)),
                ( 0x1C, Instr(Mnemonic.shad, r2,r1)),
                ( 0x20, Instr(Mnemonic.shal, r1)),
                ( 0x2A, Instr(Mnemonic.lds, r1,pr)),
                ( 0x2C, Instr(Mnemonic.shad, r2,r1)),
                ( 0x2E, Instr(Mnemonic.ldc, r1,RV)),
                ( 0x30, invalid),
                ( 0x34, invalid),
                ( 0x38, invalid),
                ( 0x36, Instr(Mnemonic.ldc_l, Post1l,sgr)),
                ( 0x3C, Instr(Mnemonic.shad, r2,r1)),
                ( 0x40, invalid),
                ( 0x41, invalid),
                ( 0x44, invalid),
                ( 0x48, invalid),
                ( 0x4A, Instr(Mnemonic.ldc, r1,tbr)),
                ( 0x4C, Instr(Mnemonic.shad, r2,r1)),
                ( 0x52, invalid),
                ( 0x59, invalid),
                ( 0x5A, Instr(Mnemonic.lds, r1,fpul)),
                ( 0x5C, Instr(Mnemonic.shad, r2,r1)),
                ( 0x64, invalid),
                ( 0x66, Instr(Mnemonic.lds_l, Post1l,dsr)),
                ( 0x68, invalid),
                ( 0x6A, Instr(Mnemonic.lds, r1,dsr)),
                ( 0x6C, Instr(Mnemonic.shad, r2,r1)),
                ( 0x70, invalid),
                ( 0x74, invalid),
                ( 0x7C, Instr(Mnemonic.shad, r2,r1)),
                ( 0x80, Instr(Mnemonic.mulr, R0,r1)),
                ( 0x88, invalid),
                ( 0x8C, Instr(Mnemonic.shad, r2,r1)),
                ( 0x90, invalid),
                ( 0x94, Instr(Mnemonic.divs, R0,r1)),
                ( 0x98, invalid),
                ( 0x9C, Instr(Mnemonic.shad, r2,r1)),
                ( 0xA0, invalid),
                ( 0xA4, invalid),
                ( 0xA8, invalid),
                ( 0xAC, Instr(Mnemonic.shad, r2,r1)),
                ( 0xB4, invalid),
                ( 0xB8, invalid),
                ( 0xBC, Instr(Mnemonic.shad, r2,r1)),
                ( 0xC4, invalid),
                ( 0xC8, invalid),
                ( 0xCC, Instr(Mnemonic.shad, r2,r1)),
                ( 0xD0, invalid),
                ( 0xD2, invalid),
                ( 0xD3, Instr(Mnemonic.stc_l, RBank2_3bit, r1)),
                ( 0xD8, invalid),
                ( 0xDC, Instr(Mnemonic.shad, r2,r1)),
                ( 0xE0, invalid),
                ( 0xE4, invalid),
                ( 0xE8, invalid),
                ( 0xEC, Instr(Mnemonic.shad, r2,r1)),
                ( 0xF0, Instr(Mnemonic.movmu_l, r1,Pre15l)),
                ( 0xF4, Instr(Mnemonic.movmu_l, Post15l,r1)),
                ( 0xF8, invalid),
                ( 0xFC, Instr(Mnemonic.shad, r2,r1)),


                ( 0x87, Instr(Mnemonic.ldc_l, Post1l,RBank2_3bit)),
                ( 0x97, Instr(Mnemonic.ldc_l, Post1l,RBank2_3bit)),
                ( 0xA7, Instr(Mnemonic.ldc_l, Post1l,RBank2_3bit)),
                ( 0xB7, Instr(Mnemonic.ldc_l, Post1l,RBank2_3bit)),
                ( 0xC7, Instr(Mnemonic.ldc_l, Post1l,RBank2_3bit)),
                ( 0xD7, Instr(Mnemonic.ldc_l, Post1l,RBank2_3bit)),
                ( 0xE7, Instr(Mnemonic.ldc_l, Post1l,RBank2_3bit)),
                ( 0xF7, Instr(Mnemonic.ldc_l, Post1l,RBank2_3bit)),

                ( 0x0D, Instr(Mnemonic.shld, r2,r1)),
                ( 0x1D, Instr(Mnemonic.shld, r2,r1)),
                ( 0x2D, Instr(Mnemonic.shld, r2,r1)),
                ( 0x3D, Instr(Mnemonic.shld, r2,r1)),
                ( 0x4D, Instr(Mnemonic.shld, r2,r1)),
                ( 0x5D, Instr(Mnemonic.shld, r2,r1)),
                ( 0x6D, Instr(Mnemonic.shld, r2,r1)),
                ( 0x7D, Instr(Mnemonic.shld, r2,r1)),
                ( 0x8D, Instr(Mnemonic.shld, r2,r1)),
                ( 0x9D, Instr(Mnemonic.shld, r2,r1)),
                ( 0xAD, Instr(Mnemonic.shld, r2,r1)),
                ( 0xBD, Instr(Mnemonic.shld, r2,r1)),
                ( 0xCD, Instr(Mnemonic.shld, r2,r1)),
                ( 0xDD, Instr(Mnemonic.shld, r2,r1)),
                ( 0xED, Instr(Mnemonic.shld, r2,r1)),
                ( 0xFD, Instr(Mnemonic.shld, r2,r1)),

                ( 0x0F, Instr(Mnemonic.mac_w, Post2w,Post1w)),
                ( 0x1F, Instr(Mnemonic.mac_w, Post2w,Post1w)),
                ( 0x2F, Instr(Mnemonic.mac_w, Post2w,Post1w)),
                ( 0x3F, Instr(Mnemonic.mac_w, Post2w,Post1w)),
                ( 0x4F, Instr(Mnemonic.mac_w, Post2w,Post1w)),
                ( 0x5F, Instr(Mnemonic.mac_w, Post2w,Post1w)),
                ( 0x6F, Instr(Mnemonic.mac_w, Post2w,Post1w)),
                ( 0x7F, Instr(Mnemonic.mac_w, Post2w,Post1w)),
                ( 0x8F, Instr(Mnemonic.mac_w, Post2w,Post1w)),
                ( 0x9F, Instr(Mnemonic.mac_w, Post2w,Post1w)),
                ( 0xAF, Instr(Mnemonic.mac_w, Post2w,Post1w)),
                ( 0xBF, Instr(Mnemonic.mac_w, Post2w,Post1w)),
                ( 0xCF, Instr(Mnemonic.mac_w, Post2w,Post1w)),
                ( 0xDF, Instr(Mnemonic.mac_w, Post2w,Post1w)),
                ( 0xEF, Instr(Mnemonic.mac_w, Post2w,Post1w)),
                ( 0xFF, Instr(Mnemonic.mac_w, Post2w,Post1w)),


                ( 0x10, Instr(Mnemonic.dt, r1)),
                ( 0x11, Instr(Mnemonic.cmp_pz, r1)),
                ( 0x15, Instr(Mnemonic.cmp_pl, r1)),
                ( 0x18, Instr(Mnemonic.shll8, r1)),
                ( 0x19, Instr(Mnemonic.shlr8, r1)),
                ( 0x21, Instr(Mnemonic.shar, r1)),
                ( 0x22, Instr(Mnemonic.sts_l, pr,Pre1l)),
                ( 0x24, Instr(Mnemonic.rotcl, r1)),
                ( 0x25, Instr(Mnemonic.rotcr, r1)),
                ( 0x26, Instr(Mnemonic.lds_l, Post1l,pr)),
                ( 0x28, Instr(Mnemonic.shll16, r1)),
                ( 0x29, Instr(Mnemonic.shlr16, r1)),
                ( 0x2B, Instr(Mnemonic.jmp, Ind1l)),
                ( 0x43, Instr(Mnemonic.stc_l, spc,r1))
            ),
            Instr(Mnemonic.mov_l, D2l,r1),
            // 6...
            Mask(0, 4, new Decoder[]
            {
                Instr(Mnemonic.mov_b, Ind2b,r1),
                Instr(Mnemonic.mov_w, Ind2w,r1),
                Instr(Mnemonic.mov_l, Ind2l,r1),
                Instr(Mnemonic.mov, r2,r1),

                Instr(Mnemonic.mov_b, Post2b,r1),
                Instr(Mnemonic.mov_w, Post2w,r1),
                Instr(Mnemonic.mov_l, Post2l,r1),
                Instr(Mnemonic.not, r2,r1),

                invalid,
                Instr(Mnemonic.swap_w, r2,r1),
                Instr(Mnemonic.negc, r2,r1),
                Instr(Mnemonic.neg, r2,r1),

                Instr(Mnemonic.extu_b, r2,r1),
                Instr(Mnemonic.extu_w, r2,r1),
                Instr(Mnemonic.exts_b, r2,r1),
                Instr(Mnemonic.exts_w, r2,r1),
            }),
            Instr(Mnemonic.add, I,r1),

            // 8...
            Mask(8, 4, new Decoder[] {
                Instr(Mnemonic.mov_b, R0,D2b),
                Instr(Mnemonic.mov_w, R0,D2w),
                invalid,
                invalid,

                Instr(Mnemonic.mov_b, D2b,R0),
                Instr(Mnemonic.mov_w, D2w,R0),
                invalid,
                invalid,

                Instr(Mnemonic.cmp_eq, I,R0),
                Instr(Mnemonic.bt, j),
                invalid,
                Instr(Mnemonic.bf, j),

                invalid,
                Instr(Mnemonic.bt_s, j),
                invalid,
                Instr(Mnemonic.bf_s, j),
            }),
            Instr(Mnemonic.mov_w, Pw,r1),
            Instr(Mnemonic.bra, J),
            Instr(Mnemonic.bsr, J),

            // C...
            Mask(8, 4, new Decoder[]
            {
                invalid,
                invalid,
                invalid,
                invalid,

                invalid,
                invalid,
                invalid,
                Instr(Mnemonic.mova, Pl,R0),

                Instr(Mnemonic.tst, I,R0),
                Instr(Mnemonic.and, I,R0),
                Instr(Mnemonic.xor, I,R0),
                Instr(Mnemonic.or, I,R0),

                invalid,
                Instr(Mnemonic.and_b, I,Gb),
                invalid,
                invalid,
            }),
            Instr(Mnemonic.mov_l, Pl,r1),
            Instr(Mnemonic.mov, I,r1),
            // F...
            Mask(0, 4, "  F...", 
                Select(bfFpuBinop, u => u == 0, "F..0",
                        Instr(Mnemonic.fadd, d2,d1),
                        Instr(Mnemonic.fadd, f2,f1)),
                Select(bfFpuBinop, u => u == 0, "F..1",
                        Instr(Mnemonic.fsub, d2,d1),
                        Instr(Mnemonic.fsub, f2,f1)),
                Select(bfFpuBinop, u => u == 0, "F..2",
                        Instr(Mnemonic.fmul, d2,d1),
                        Instr(Mnemonic.fmul, f2,f1)),
                Select(bfFpuBinop, u => u == 0, "F..3",
                        Instr(Mnemonic.fdiv, d2,d1),
                        Instr(Mnemonic.fdiv, f2,f1)),

                Select(bfFpuBinop, u => u == 0, "F..4",
                        Instr(Mnemonic.fcmp_eq, d2,d1),
                        Instr(Mnemonic.fcmp_eq, f2,f1)),
                Select(bfFpuBinop, u => u == 0, "F..5",
                        Instr(Mnemonic.fcmp_gt, d2,d1),
                        Instr(Mnemonic.fcmp_gt, f2,f1)),
/*
1111nnnnmmmm0110 ~ FMOV.S @(R0,Rm),FRn (R0 + Rm) → FRn 
1111nnn0mmmm0110 ~ FMOV @(R0,Rm),DRn (R0 + Rm) → DRn 
1111nnnnmmmm0111 ~ FMOV.S FRm,@(R0,Rn) FRm → (R0 + Rn) 
1111nnnnmmm00111 ~ FMOV DRm,@(R0,Rn) DRm → (R0 + Rn) 
*/
                Mask(8, 1, "F..6",
                    Instr(Mnemonic.fmov_d, X1d,d2),
                    Instr(Mnemonic.fmov_s, X1l,f2)),

                Mask(4, 1, "F..7",
                    Instr(Mnemonic.fmov_d, d2,X1d),
                    Instr(Mnemonic.fmov_s, f2,X1l)),

/*
1111nnnnmmmm1000 ~ FMOV.S @Rm,FRn (Rm) → FRn 
1111nnn0mmmm1000 ~ FMOV @Rm,DRn (Rm) → DRn 
1111nnnnmmmm1001 ~ FMOV.S @Rm+,FRn (Rm) → FRn, Rm + 4 → Rm 
1111nnn0mmmm1001 ~ FMOV @Rm+,DRn (Rm) → DRn, Rm + 8 → Rm 
1111nnnnmmmm1010 ~ FMOV.S FRm,@Rn FRm → (Rn) 
1111nnnnmmm01010 ~ FMOV DRm,@Rn DRm → (Rn) 
1111nnnnmmmm1011 ~ FMOV.S FRm,@-Rn Rn-4 → Rn, FRm → (Rn) 
1111nnnnmmm01011 ~ FMOV DRm,@-Rn Rn-8 → Rn, DRm → (Rn) 
*/
                Mask(4, 1, "F..8",
                    Instr(Mnemonic.fmov_d, Ind1d,d2),
                    Instr(Mnemonic.fmov_s, Ind1l,f2)),

                Mask(8, 1, "F..9",
                    Instr(Mnemonic.fmov_d, Post2d,d1),
                    Instr(Mnemonic.fmov_s, Post2l,f1)),

                Mask(4, 1, "F..A",
                    Instr(Mnemonic.fmov_d, d2, Ind1d),
                    Instr(Mnemonic.fmov_s, f2, Ind1l)),

                Mask(4, 1, "F..B",
                    Instr(Mnemonic.fmov_d, d2, Pre1d),
                    Instr(Mnemonic.fmov_s, f2, Pre1l)),

/*
1111nnnnmmmm1100 ~ FMOV FRm,FRn FRm → FRn 
1111nnn0mmm01100 ~ FMOV DRm,DRn DRm → DRn 
*/
                // 
                Select(bfFpuBinop, u => u == 0, "F..C",
                    Instr(Mnemonic.fmov, d2,d1),
                    Instr(Mnemonic.fmov, f2,f1)),
                decode_FxxD,
                Instr(Mnemonic.fmac, F0,f2,f1),
                invalid
            )
        };

        /*
        0100mmmm01101010 ~ LDS Rm,FPSCR Rm → FPSCR 
        0100mmmm01011010 ~ LDS Rm,FPUL Rm → FPUL 
        0100mmmm01100110 ~ LDS.L @Rm+,FPSCR (Rm) → FPSCR, Rm+4 → Rm 
        0100mmmm01010110 ~ LDS.L @Rm+,FPUL (Rm) → FPUL, Rm+4 → Rm 
        0000nnnn01101010 ~ STS FPSCR,Rn FPSCR → Rn 
        0000nnnn01011010 ~ STS FPUL,Rn FPUL → Rn 
        0100nnnn01100010 ~ STS.L FPSCR,@-Rn Rn – 4 → Rn, FPSCR → (Rn) 
        0100nnnn01010010 ~ STS.L FPUL,@-Rn Rn – 4 → Rn, FPUL → (Rn) 

        1111nnn1mmmm0110 ~ FMOV @(R0,Rm),XDn (R0 + Rm) → XDn 
        1111nnnnmmm10111 ~ FMOV XDm,@(R0,Rn) XDm → (R0+Rn) 
        1111nnn1mmmm1000 ~ FMOV @Rm,XDn (Rm) → XDn 
        1111nnn1mmmm1001 ~ FMOV @Rm+,XDn (Rm) → XDn, Rm + 8 → Rm 
        1111nnnnmmm11010 ~ FMOV XDm,@Rn XDm → (Rn) 
        1111nnnnmmm11011 ~ FMOV XDm,@-Rn Rn – 8 → Rn, XDm → (Rn) 
        1111nnn1mmm01100 ~ FMOV DRm,XDn DRm → XDn 
        1111nnn0mmm11100 ~ FMOV XDm,DRn XDm → DRn 
        1111nnn1mmm11100 ~ FMOV XDm,XDn XDm → XDn 

                 */

        /*
        1111nnnnmmmm1110 ~ FMAC FR0,FRm,FRn FR0*FRm + FRn → FRn 
        */
    }
}
