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

using NUnit.Framework;
using Reko.Arch.X86;
using Reko.Arch.X86.Assembler;
using Reko.Core;
using Reko.Core.Assemblers;
using Reko.Core.Machine;
using Reko.Core.Memory;
using Reko.Core.Services;
using Reko.Core.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Text;

namespace Reko.UnitTests.Arch.X86
{
    [TestFixture]
    public class X86DisassemblerTests
    {
        private readonly ServiceContainer sc;
        private X86Disassembler dasm;
        private Dictionary<string, object> options;

        public X86DisassemblerTests()
        {
            sc = new ServiceContainer();
            sc.AddService<IFileSystemService>(new FileSystemServiceImpl());
        }

        private X86Instruction Disassemble16(params byte[] bytes)
        {
            ByteMemoryArea mem = new ByteMemoryArea(Address.SegPtr(0xC00, 0), bytes);
            EndianImageReader rdr = mem.CreateLeReader(mem.BaseAddress);
            var decoders = ProcessorMode.Real.CreateRootDecoders(options);
            var dasm = ProcessorMode.Real.CreateDisassembler(sc, decoders, rdr, options);
            if (options.ContainsKey("Emulate8087"))
            {
                dasm.Emulate8087 = true;
            }
            return dasm.First();
        }

        private X86Instruction Disassemble64(params byte[] bytes)
        {
            var img = new ByteMemoryArea(Address.Ptr64(0x10000), bytes);
            var rdr = img.CreateLeReader(img.BaseAddress);
            var decoders = ProcessorMode.Protected64.CreateRootDecoders(options);
            var dasm = new X86Disassembler(
                sc,
                decoders,
                ProcessorMode.Protected64,
                rdr,
                PrimitiveType.Word32,
                PrimitiveType.Word64,
                true);
            return dasm.First();
        }

        private void CreateDisassembler16(params byte[] bytes)
        {
            var mem = new ByteMemoryArea(Address.SegPtr(0x0C00, 0), bytes);
            CreateDisassembler16(mem);
        }

        private void CreateDisassembler16(MemoryArea mem)
        {
            var decoders = ProcessorMode.Real.CreateRootDecoders(options);
            dasm = new X86Disassembler(
                sc,
                decoders,
                ProcessorMode.Real,
                mem.CreateLeReader(mem.BaseAddress),
                PrimitiveType.Word16,
                PrimitiveType.Word16,
                false);
            if (options != null && options.ContainsKey("Emulate8087"))
            {
                dasm.Emulate8087 = true;
            }
        }

        private void CreateDisassembler32(MemoryArea mem)
        {
            var decoders = ProcessorMode.Protected32.CreateRootDecoders(options);
            dasm = new X86Disassembler(
                sc,
                decoders,
                ProcessorMode.Protected32,
                mem.CreateLeReader(mem.BaseAddress),
                PrimitiveType.Word32,
                PrimitiveType.Word32,
                false);
        }

        private void CreateDisassembler16(EndianImageReader rdr)
        {
            var decoders = ProcessorMode.Real.CreateRootDecoders(options);
            dasm = new X86Disassembler(
                sc,
                decoders,
                ProcessorMode.Real,
                rdr,
                PrimitiveType.Word16,
                PrimitiveType.Word16,
                false);
        }

        private void AssertCode16(string sExp, params byte[] bytes)
        {
            var instr = Disassemble16(bytes);
            Assert.AreEqual(sExp, instr.ToString());
        }

        private void AssertCode16(string sExp, string hexBytes)
        {
            var instr = Disassemble16(BytePattern.FromHexBytes(hexBytes));
            Assert.AreEqual(sExp, instr.ToString());
        }

        private X86Instruction AssertCode64(string sExp, params byte[] bytes)
        {
            var instr = Disassemble64(bytes);
            Assert.AreEqual(sExp, instr.ToString());
            return instr;
        }

        private void AssertCode64(string sExp, string hexBytes)
        {
            var instr = Disassemble64(BytePattern.FromHexBytes(hexBytes));
            Assert.AreEqual(sExp, instr.ToString());
        }

        [SetUp]
        public void Setup()
        {
            options = new Dictionary<string, object>();
        }

        [Test]
        public void X86dis_Sequence()
        {
            var arch = new X86ArchitectureReal(sc, "x86-real-16", new Dictionary<string, object>());
            X86TextAssembler asm = new X86TextAssembler(arch);
            var program = asm.AssembleFragment(
                Address.SegPtr(0xB96, 0),
                @"	mov	ax,0
	cwd
foo:
	lodsb	
	dec		cx
	jnz		foo
");

            CreateDisassembler16(program.SegmentMap.Segments.Values.First().MemoryArea);
            StringBuilder sb = new StringBuilder();
            foreach (var instr in dasm.Take(5))
            {
                Address addr = instr.Address;
                sb.AppendFormat("{0}\t{1}\r\n", addr, instr.ToString());
            }

            string s = sb.ToString();
            Assert.AreEqual(
                "0B96:0000\tmov\tax,0h\r\n" +
                "0B96:0003\tcwd\r\n" +
                "0B96:0004\tlodsb\r\n" +
                "0B96:0005\tdec\tcx\r\n" +
                "0B96:0006\tjnz\t0004h\r\n",
                s);
        }

        [Test]
        public void SegmentOverrides()
        {
            var arch = new X86ArchitectureReal(sc, "x86-real-16", new Dictionary<string, object>());
            X86TextAssembler asm = new X86TextAssembler(arch);
            var program = asm.AssembleFragment(
                Address.SegPtr(0xB96, 0),
                "foo	proc\r\n" +
                "		mov	bx,[bp+4]\r\n" +
                "		mov	ax,[bx+4]\r\n" +
                "		mov cx,cs:[si+4]\r\n");

            CreateDisassembler16(program.SegmentMap.Segments.Values.First().MemoryArea);
            X86Instruction[] instrs = dasm.Take(3).ToArray();
            Assert.AreEqual(Registers.ss, ((MemoryOperand) instrs[0].Operands[1]).DefaultSegment);
            Assert.AreEqual(Registers.ds, ((MemoryOperand) instrs[1].Operands[1]).DefaultSegment);
            Assert.AreEqual(Registers.cs, ((MemoryOperand) instrs[2].Operands[1]).DefaultSegment);
        }

        [Test]
        public void X86Dis_Rotations()
        {
            var arch = new X86ArchitectureReal(sc, "x86-real-16", new Dictionary<string, object>());
            X86TextAssembler asm = new X86TextAssembler(arch);
            var lr = asm.AssembleFragment(
                Address.SegPtr(0xB96, 0),
                "foo	proc\r\n" +
                "rol	ax,cl\r\n" +
                "ror	word ptr [bx+2],cl\r\n" +
                "rcr	word ptr [bp+4],4\r\n" +
                "rcl	ax,1\r\n");

            var bmem = (ByteMemoryArea) lr.SegmentMap.Segments.Values.First().MemoryArea;
            CreateDisassembler16(bmem.CreateLeReader(bmem.BaseAddress));
            StringBuilder sb = new StringBuilder();
            foreach (var instr in dasm.Take(4))
            {
                sb.AppendFormat("{0}\r\n", instr.ToString());
            }
            string s = sb.ToString();
            Assert.AreEqual(
                "rol\tax,cl\r\n" +
                "ror\tword ptr [bx+2h],cl\r\n" +
                "rcr\tword ptr [bp+4h],4h\r\n" +
                "rcl\tax,1h\r\n", s);
        }

        [Test]
        public void X86Dis_rorx()
        {
            AssertCode64("rorx\trax,r10,2h", "C4C3FBF0C202");
        }

        [Test]
        public void X86Dis_Extensions()
        {
            var arch = new X86ArchitectureReal(sc, "x86-real-16", new Dictionary<string, object>());
            IAssembler asm = arch.CreateAssembler(null);
            var program = asm.AssembleFragment(
                Address.SegPtr(0xA14, 0),
@"		.i86
foo		proc
		movsx	ecx,word ptr [bp+8]
		movzx   edx,cl
		movsx	ebx,bx
		movzx	ax,byte ptr [bp+04]
");
            program.Platform = new DefaultPlatform(sc, arch);
            CreateDisassembler16(program.SegmentMap.Segments.Values.First().MemoryArea);
            StringBuilder sb = new StringBuilder();
            foreach (var ii in dasm.Take(4))
            {
                sb.AppendLine(string.Format("{0}", ii.ToString()));
            }
            string s = sb.ToString();
            Assert.AreEqual(
@"movsx	ecx,word ptr [bp+8h]
movzx	edx,cl
movsx	ebx,bx
movzx	ax,byte ptr [bp+4h]
", s);
        }

        [Test]
        public void X86Dis_FpuInstructions()
        {
            using (FileUnitTester fut = new FileUnitTester("Arch/X86/DisFpuInstructions.txt"))
            {
                var arch = new X86ArchitectureReal(sc, "x86-real-16", new Dictionary<string, object>());
                X86TextAssembler asm = new X86TextAssembler(arch);
                Program lr;
                using (var rdr = new StreamReader(FileUnitTester.MapTestPath("Fragments/fpuops.asm")))
                {
                    lr = asm.Assemble(Address.SegPtr(0xC32, 0), rdr);
                }
                CreateDisassembler16(lr.SegmentMap.Segments.Values.First().MemoryArea);
                foreach (X86Instruction instr in dasm)
                {
                    fut.TextWriter.WriteLine("{0}", instr.ToString());
                }
                fut.AssertFilesEqual();
            }
        }

        [Test]
        public void X86Dis_SignedByte()
        {
            var instr = Disassemble16(0x83, 0xC6, 0x1);  // add si,+01
            Assert.AreEqual("add\tsi,1h", instr.ToString());
            Assert.AreEqual(PrimitiveType.Word16, instr.Operands[0].Width);
            Assert.AreEqual(PrimitiveType.Byte, instr.Operands[1].Width);
            Assert.AreEqual(PrimitiveType.Word16, instr.dataWidth);
        }

        [Test]
        public void X86Dis_LesBxStackArg()
        {
            var instr = Disassemble16(0xC4, 0x5E, 0x6);		// les bx,[bp+06]
            Assert.AreEqual("les\tbx,[bp+6h]", instr.ToString());
            Assert.AreSame(PrimitiveType.SegPtr32, instr.Operands[1].Width);
        }

        [Test]
        public void SegFromBits()
        {
            Assert.AreSame(Registers.es, X86Disassembler.SegFromBits(0));
            Assert.AreSame(Registers.cs, X86Disassembler.SegFromBits(1));
            Assert.AreSame(Registers.ss, X86Disassembler.SegFromBits(2));
            Assert.AreSame(Registers.ds, X86Disassembler.SegFromBits(3));
            Assert.AreSame(Registers.fs, X86Disassembler.SegFromBits(4));
            Assert.AreSame(Registers.gs, X86Disassembler.SegFromBits(5));
        }

        [Test]
        public void X86dis_bsf_w16()
        {
            AssertCode64("bsf\tr11w,r15w", "66450FBCDF");
        }

        [Test]
        public void X86dis_RelocatedOperand()
        {
            byte[] image = new byte[] { 0xB8, 0x78, 0x56, 0x34, 0x12 };	// mov eax,0x12345678<32>
            ByteMemoryArea img = new ByteMemoryArea(Address.Ptr32(0x00100000), image);
            img.Relocations.AddPointerReference(0x00100001ul, 0x12345678);
            EndianImageReader rdr = img.CreateLeReader(img.BaseAddress);
            var decoders = ProcessorMode.Protected32.CreateRootDecoders(new Dictionary<string, object>());
            X86Disassembler dasm = new X86Disassembler(
                sc,
                decoders,
                ProcessorMode.Protected32,
                rdr,
                PrimitiveType.Word32,
                PrimitiveType.Word32,
                false);
            X86Instruction instr = dasm.First();
            Assert.AreEqual("mov\teax,12345678h", instr.ToString());
            Assert.AreEqual("ptr32", instr.Operands[1].Width.ToString());
        }

        [Test]
        public void X86Dis_RelocatedSegment()
        {
            byte[] image = new byte[] { 0x2E, 0xC7, 0x06, 0x01, 0x00, 0x00, 0x08 }; // mov cs:[0001],0800
            ByteMemoryArea img = new ByteMemoryArea(Address.SegPtr(0x900, 0), image);
            var relAddr = Address.SegPtr(0x900, 5);
            img.Relocations.AddSegmentReference(relAddr.ToLinear(), 0x0800);
            EndianImageReader rdr = img.CreateLeReader(img.BaseAddress);
            CreateDisassembler16(rdr);
            X86Instruction instr = dasm.First();
            Assert.AreEqual("mov\tword ptr cs:[0001h],800h", instr.ToString());
            Assert.AreEqual("selector", instr.Operands[1].Width.ToString());
        }

        [Test]
        public void X86dis_TestWithImmediateOperands()
        {
            var instr = Disassemble16(0xF6, 0x06, 0x26, 0x54, 0x01);     // test byte ptr [5426],01
            Assert.AreEqual("test\tbyte ptr [5426h],1h", instr.ToString());
            Assert.AreSame(PrimitiveType.Byte, instr.Operands[0].Width);
            Assert.AreSame(PrimitiveType.Byte, instr.Operands[1].Width);
            Assert.AreSame(PrimitiveType.Byte, instr.dataWidth, "Instruction data width should be byte");
        }

        [Test]
        public void X86dis_RelativeCallTest()
        {
            var instr = Disassemble16(0xE8, 0x00, 0xF0);
            Assert.AreEqual("call\t0F003h", instr.ToString());
            Assert.AreSame(PrimitiveType.Offset16, instr.Operands[0].Width);
        }

        [Test]
        public void X86dis_farCall()
        {
            var instr = Disassemble16(0x9A, 0x78, 0x56, 0x34, 0x01, 0x90, 0x90);
            Assert.AreEqual("call\tfar 0134h:5678h", instr.ToString());
        }

        [Test]
        public void X86dis_Xlat16()
        {
            var instr = Disassemble16(0xD7);
            Assert.AreEqual("xlat", instr.ToString());
            Assert.AreEqual(PrimitiveType.Byte, instr.dataWidth);
            Assert.AreEqual(PrimitiveType.Word16, instr.addrWidth);
        }

        [Test]
        public void X86dis_hlt()
        {
            var instr = Disassemble16(0xF4);
            Assert.AreEqual("hlt", instr.ToString());
        }

        [Test]
        public void X86Dis_64_rexW()
        {
            AssertCode64("lea\trdx,[rsp+20h]", 0x48, 0x8D, 0x54, 0x24, 0x20);
        }

        [Test]
        public void X86Dis_64_rexWR()
        {
            AssertCode64("lea\tr10,[rsp+20h]", 0x4C, 0x8D, 0x54, 0x24, 0x20);
        }

        [Test]
        public void X86Dis_64_rexWB()
        {
            AssertCode64("lea\trdx,[r12+20h]", 0x49, 0x8D, 0x54, 0x24, 0x20);
        }

        [Test]
        public void X86Dis_64_rexR()
        {
            AssertCode64("xor\tr8d,eax", 0x44, 0x33, 0xC0);
        }

        [Test]
        public void X86Dis_64_rexRB()
        {
            AssertCode64("xor\tr8d,r8d", 0x45, 0x33, 0xC0);
        }

        [Test]
        public void X86Dis_64_rexB_bytesize()
        {
            AssertCode64("xor\tr8b,al", 0x41, 0x30, 0xC0);
        }

        [Test]
        public void X86Dis_64_rexR_bytesize_sil()
        {
            AssertCode64("xor\tsil,r8b", 0x44, 0x30, 0xC6);
        }

        [Test]
        public void X86Dis_64_rexB_wordsize()
        {
            AssertCode64("xor\tax,r14w", 0x66, 0x41, 0x33, 0xC6);
        }

        [Test]
        public void X86Dis_64_movaps()
        {
            AssertCode64("movaps\txmm0,[rsp+20h]", 0x0f, 0x28, 0x44, 0x24, 0x20);
        }

        [Test]
        public void X86Dis_64_movdqa()
        {
            AssertCode64("movdqa\t[rsp+20h],xmm0", 0x66, 0x0f, 0x7f, 0x44, 0x24, 0x20);
        }

        [Test]
        public void X86Dis_Call16()
        {
            var instr = Disassemble16(0xE9, 0x78, 0x56);
            var addrOp = (ImmediateOperand) instr.Operands[0];
            Assert.AreEqual("567B", addrOp.ToString());
        }

        [Test]
        public void X86Dis_DirectOperand16()
        {
            var instr = Disassemble16(0x8B, 0x16, 0x22, 0x33, 0x44);
            Assert.AreEqual("mov\tdx,[3322h]", instr.ToString());
            var memOp = (MemoryOperand) instr.Operands[1];
            Assert.AreEqual("word16", memOp.Offset.DataType.ToString());
        }

        [Test]
        public void X86Dis_bextr()
        {
            AssertCode64("bextr\teax,r11d,r10d", "C4C228F7C3");
        }

        [Test]
        public void X86Dis_bzhi()
        {
            AssertCode64("bzhi\trax,rdx,r9", "C4E2B0F5C2");
        }

        [Test]
        public void X86Dis_movd_64()
        {
            AssertCode64("movd\tdword ptr [rsi],mm1", 0x0F, 0x7E, 0x0E);
            AssertCode64("movd\tdword ptr [rsi],xmm1", 0x66, 0x0F, 0x7E, 0x0E);
            AssertCode64("movq\txmm1,qword ptr [rsi]", 0xF3, 0x0F, 0x7E, 0x0E);
            AssertCode64("movd\tesi,mm1", 0x0F, 0x7E, 0xCE);
            AssertCode64("movd\tesi,xmm1", 0x66, 0x0F, 0x7E, 0xCE);
            AssertCode64("movq\txmm1,xmm6", 0xF3, 0x0F, 0x7E, 0xCE);
        }

        [Test]
        public void X86Dis_movd_64_rex()
        {
            AssertCode64("movq\t[rsi],mm1", 0x48, 0x0F, 0x7E, 0x0E);
            AssertCode64("movq\tqword ptr [rsi],xmm1", 0x66, 0x48, 0x0F, 0x7E, 0x0E);
            AssertCode64("movq\trsi,mm1", 0x48, 0x0F, 0x7E, 0xCE);
            AssertCode64("movq\trsi,xmm1", 0x66, 0x48, 0x0F, 0x7E, 0xCE);
        }

        [Test]
        public void X86Dis_64_movsxd()
        {
            AssertCode64("movsxd\trcx,dword ptr [rax+3Ch]", 0x48, 0x63, 0x48, 0x3c);
        }

        [Test]
        public void X86Dis_64_rip_relative()
        {
            AssertCode64("mov\trax,[rip+100000h]", 0x49, 0x8b, 0x05, 0x00, 0x00, 0x10, 0x00);
            AssertCode64("mov\trax,[rip+100000h]", 0x48, 0x8b, 0x05, 0x00, 0x00, 0x10, 0x00);
        }

        [Test]
        public void X86Dis_64_sub_immediate_dword()
        {
            AssertCode64("sub\trsp,+508h", 0x48, 0x81, 0xEC, 0x08, 0x05, 0x00, 0x00);
        }

        [Test]
        public void X86Dis_nops()
        {
            AssertCode64("nop", 0x90);
            AssertCode64("nop", 0x66, 0x90);
            AssertCode64("nop\tdword ptr [rax]", 0x0F, 0x1F, 0x00);
            AssertCode64("nop\tdword ptr [rax+0h]", 0x0F, 0x1F, 0x40, 0x00);
            AssertCode64("nop\tdword ptr [rax+rax+0h]", 0x0F, 0x1F, 0x44, 0x00, 0x00);
            AssertCode64("nop\tword ptr [rax+rax+0h]", 0x66, 0x0F, 0x1F, 0x44, 0x00, 0x00);
            AssertCode64("nop\tdword ptr [rax+0h]", 0x0F, 0x1F, 0x80, 0x00, 0x00, 0x00, 0x00);
            AssertCode64("nop\tdword ptr [rax+rax+0h]", 0x0F, 0x1F, 0x84, 0x00, 0x00, 0x00, 0x00, 0x00);
            AssertCode64("nop\tword ptr [rax+rax+0h]", 0x66, 0x0F, 0x1F, 0x84, 0x00, 0x00, 0x00, 0x00, 0x00);
        }

        [Test]
        public void X86Dis_rex_prefix_40()
        {
            AssertCode64("push\trbp", 0x40, 0x55);
        }

        [Test]
        public void X86Dis_repz_ret()
        {
            AssertCode64("ret", 0xF3, 0xC3);
        }

        [Test]
        public void X86Dis_emulate_x87_int_39()
        {
            options = new Dictionary<string, object> { { "Emulate8087", "true" } };
            CreateDisassembler16(0xCD, 0x39, 0x5E, 0xEA);
            var instrs = dasm.Take(2)
                .Select(i => i.ToString())
                .ToArray();
            Assert.AreEqual("nop", instrs[0]);
            Assert.AreEqual("fstp\tdouble ptr [bp-16h]", instrs[1]);
        }

        [Test]
        public void X86Dis_emulate_x87_int_3C()
        {
            options = new Dictionary<string, object> { { "Emulate8087", "true" } };
            CreateDisassembler16(0xCD, 0x3C, 0xDD, 0x06, 0x8B, 0x04);
            var instrs = dasm.Take(2)
                .Select(i => i.ToString())
                .ToArray();
            Assert.AreEqual("nop", instrs[0]);
            Assert.AreEqual("fld\tdouble ptr es:[048Bh]", instrs[1]);
        }

        [Test]
        public void X86dis_regression()
        {
            AssertCode64("movups\t[rsp+20h],xmm0", 0x0F, 0x11, 0x44, 0x24, 0x20);
        }

        [Test]
        public void X86dis_movups()
        {
            AssertCode64("movups\txmm0,[rbp-20h]", 0x0F, 0x10, 0x45, 0xE0);
            AssertCode64("movupd\txmm0,[rbp-20h]", 0x66, 0x0F, 0x10, 0x45, 0xE0);
            AssertCode64("movss\txmm0,dword ptr [rbp-20h]", 0xF3, 0x0F, 0x10, 0x45, 0xE0);
            AssertCode64("movsd\txmm0,double ptr [rbp-20h]", 0xF2, 0x0F, 0x10, 0x45, 0xE0);
        }

        [Test]
        public void X86dis_movups_16bit()
        {
            AssertCode16("movups\txmm7,xmm3", 0x0F, 0x10, 0xFB);
        }

        [Test]
        public void X86dis_ucomiss()
        {
            AssertCode64("ucomiss\txmm0,dword ptr [rip+0B12Dh]", 0x0F, 0x2E, 0x05, 0x2D, 0xB1, 0x00, 0x00);
        }

        [Test]
        public void X86dis_ucomisd()
        {
            AssertCode64("ucomisd\txmm0,double ptr [rip+0B12Dh]", 0x66, 0x0F, 0x2E, 0x05, 0x2D, 0xB1, 0x00, 0x00);
        }

        [Test]
        public void X86dis_addss()
        {
            AssertCode64("addss\txmm1,dword ptr [rip+0B0FBh]", 0xF3, 0x0F, 0x58, 0x0D, 0xFB, 0xB0, 0x00, 0x00);
        }

        [Test]
        public void X86dis_cvtsi2ss()
        {
            AssertCode64("cvtsi2ss\txmm0,rax", 0xF3, 0x48, 0x0F, 0x2A, 0xC0);
        }

        [Test]
        public void X86dis_out_dx()
        {
            AssertCode16("out\tdx,al", 0xEE);
            AssertCode16("out\tdx,ax", 0xEF);
        }

        [Test]
        public void X86dis_x64_push()
        {
            AssertCode64("push\tr15", 0x41, 0x57);
        }

        [Test]
        public void X86dis_x64_push_immediate()
        {
            AssertCode64("push\t42h", 0x6A, 0x42);
        }

        [Test]
        public void X86dis_mfence()
        {
            AssertCode64("mfence", 0x0F, 0xAE, 0xF0);
        }

        [Test]
        public void X86dis_lfence()
        {
            AssertCode64("lfence", 0x0F, 0xAE, 0xE8);
        }

        [Test]
        public void X86dis_aesimc()
        {
            AssertCode64("aesimc\txmm0,xmm0", 0x66, 0x0F, 0x38, 0xDB, 0xC0);
        }

        [Test]
        public void X86dis_vmovss()
        {
            AssertCode64("vmovss\txmm0,dword ptr [rip+351h]", 0xC5, 0xFA, 0x10, 0x05, 0x51, 0x03, 0x00, 0x00);
        }

        [Test]
        public void X86dis_vcvtsi2ss()
        {
            AssertCode64("vcvtsi2ss\txmm0,xmm0,rax", 0xC4, 0xE1, 0xFA, 0x2A, 0xC0);
        }

        [Test]
        public void X86dis_vcvtsi2sd()
        {
            AssertCode64("vcvtsi2sd\txmm0,xmm0,rdx", 0xC4, 0xE1, 0xFB, 0x2A, 0xC2);
        }

        [Test]
        public void X86dis_call_Ev()
        {
            AssertCode64("call\trax", 0xFF, 0xD0);
        }

        [Test]
        public void X86dis_vaddsd()
        {
            AssertCode64("vaddsd\txmm0,xmm0,xmm0", 0xC5, 0xFB, 0x58, 0xC0);
        }

        [Test]
        public void X86dis_vmovapd()
        {
            AssertCode64("vmovapd\tymm1,[rax]", 0xC5, 0xFD, 0x28, 0x08);
        }

        [Test]
        public void X86dis_vmovaps()
        {
            AssertCode64("vmovaps\t[rcx+4Dh],xmm4", 0xC5, 0xF8, 0x29, 0x61, 0x4D);
        }

        [Test]
        public void X86dis_vmovsd()
        {
            AssertCode64("vmovsd\tdouble ptr [rcx],xmm0", 0xC5, 0xFB, 0x11, 0x01);
        }

        [Test]
        public void X86dis_vxorps()
        {
            AssertCode64("vxorpd\txmm0,xmm0,xmm0", 0xC5, 0xF9, 0x57, 0xC0);
        }

        [Test]
        public void X86dis_vaddpd()
        {
            AssertCode64("vaddpd\tymm0,ymm0,[rbp-90h]", 0xC5, 0xFD, 0x58, 0x85, 0x70, 0xFF, 0xFF, 0xFF);
        }

        [Test]
        public void X86dis_64_lea()
        {
            AssertCode64("lea\trdi,[rip+0DAh]", "488D3DDA000000");
        }

        [Test]
        public void X86dis_vxorpd_256()
        {
            AssertCode64("vxorpd\tymm0,ymm0,ymm0", 0xC5, 0xFD, 0x57, 0xC0);
        }

        [Test]
        public void X86dis_vxorpd_mem_256()
        {
            AssertCode64("vxorpd\tymm1,ymm0,[rcx]", 0xC5, 0xFD, 0x57, 0x09);
        }

        [Test]
        public void X86dis_syscall()
        {
            AssertCode64("syscall", 0x0F, 0x05, 0x42, 0x42);
        }

        [Test]
        public void X86dis_sysret()
        {
            AssertCode64("sysret", 0x0F, 0x07);
        }

        [Test]
        public void X86Dis_64_vzeroall()
        {
            AssertCode64("vzeroall", "C5FC77");
        }

        [Test]
        public void X86Dis_64_vzeroupper()
        {
            AssertCode64("vzeroupper", "C5F877");
        }

        [Test]
        public void X86Dis_andn()
        {
            AssertCode64("andn\teax,eax,ebx", "C4E278F2C3");
        }

        [Test]
        public void X86Dis_pdep()
        {
            AssertCode64("pdep\trax,r10,r10", "C4C2ABF5C2");
        }

        [Test]
        public void X86dis_permq()
        {
            AssertCode64("vpermq\tymm0,[rdx+42h],6h", "C4 E3 01 00 42 42 06");
        }

        [Test]
        public void X86Dis_pext()
        {
            AssertCode64("pext\tr8,r10,r10", "C442AAF5C2");
        }




        [Test]
        public void X86Dis_vmcall()
        {
            AssertCode64("vmcall", 0x0f, 0x01, 0xC1);
        }

        [Test]
        public void X86Dis_sidt()
        {
            AssertCode64("sidt\t[rip+10AD7D6h]", 0x0f, 0x01, 0x0d, 0xd6, 0xd7, 0x0a, 0x01);
        }

        [Test]
        public void X86Dis_lgdt()
        {
            AssertCode64("lgdt\t[rdi+rcx]", 0x0f, 0x01, 0x14, 0x0f);
        }

        [Test]
        public void X86Dis_mwait()
        {
            AssertCode64("mwait", 0x0f, 0x01, 0xC9);
        }

        [Test]
        public void X86Dis_mulx()
        {
            AssertCode64("mulx\tr12,rax,rdx,r10", "C442FBF6E2");
        }

        [Test]
        public void X86Dis_invalid_0f6c()
        {
            AssertCode64("illegal", 0x0f, 0x6c);
        }

        [Test]
        public void X86Dis_vpunpcklqdq()
        {
            AssertCode64("punpcklqdq\txmm0,xmm3", 0x66, 0x0f, 0x6c, 0xC3);
        }

        [Test]
        public void X86Dis_invalid_0f6d()
        {
            AssertCode64("illegal", 0x0f, 0x6d);
        }

        [Test]
        public void X86Dis_ud0_0fffff()
        {
            AssertCode64("ud0\tedi,edi", 0x0f, 0xff, 0xff);
        }

        [Test]
        public void X86Dis_bt_imm()
        {
            AssertCode64("bt\tebx,4h", 0x0F, 0xBA, 0xE3, 0x04);
        }

        [Test]
        public void X86Dis_rdrand()
        {
            AssertCode64("rdrand\tebx", 0x0f, 0xc7, 0xF3);
        }

        [Test]
        public void X86Dis_fcomi()
        {
            AssertCode64("fcomi\tst(0),st(1)", 0xdb, 0xf1);
        }

        [Test]
        public void X86Dis_vblendvpd()
        {
            AssertCode64("vblendvpd\txmm0,xmm7,xmm2,xmm4", "C4E341 4BC242");
            AssertCode64("illegal", "0F3A4BC242");
        }

        [Test]
        public void X86Dis_vcvttpd2dq()
        {
            AssertCode64("vcvttpd2dq\txmm6,xmm5", 0xc5, 0xe9, 0xe6, 0xf5);
        }

        [Test]
        public void X86Dis_nop_0f19c0()
        {
            AssertCode64("nop\teax", 0x0f, 0x19, 0xc0);
        }

        [Test]
        public void X86Dis_nop_0f19c1()
        {
            AssertCode64("nop\teax", 0x0f, 0x19, 0xc0);
        }

        [Test]
        public void X86Dis_lidt()
        {
            AssertCode64("lidt\t[rdi]", 0x0f, 0x01, 0x1f);
        }

        [Test]
        public void X86Dis_smsw()
        {
            AssertCode64("smsw\tword ptr [rcx+40h]", 0x0f, 0x01, 0x61, 0x40);
        }

        [Test]
        public void X86Dis_lmsw()
        {
            AssertCode64("lmsw\tword ptr [rbp+rax*2+48h]", 0x0f, 0x01, 0x74, 0x45, 0x48);
        }

        [Test]
        public void X86Dis_invlpg()
        {
            AssertCode64("invlpg\tbyte ptr [rax+16h]", 0x0f, 0x01, 0x78, 0x16);
        }

        [Test]
        public void X86Dis_vmresume_0f01c3()
        {
            AssertCode64("vmresume", 0x0f, 0x01, 0xc3);
        }

        [Test]
        public void X86Dis_vmxoff_0f01c4()
        {
            AssertCode64("vmxoff", 0x0f, 0x01, 0xc4);
        }

        [Test]
        public void X86Dis_monitor()
        {
            AssertCode64("monitor", 0x0f, 0x01, 0xc8);
        }

        [Test]
        public void X86Dis_smsw_0f01e6()
        {
            AssertCode64("smsw\tesi", 0x0f, 0x01, 0xe6);
        }

        [Test]
        public void X86Dis_rdpkru_0f01ee()
        {
            AssertCode64("rdpkru", 0x0f, 0x01, 0xee);
        }

        [Test]
        public void X86Dis_wrpkru()
        {
            AssertCode64("wrpkru", 0x0f, 0x01, 0xef);
        }

        [Test]
        public void X86Dis_lmsw_0f01f0()
        {
            AssertCode64("lmsw\tax", 0x0f, 0x01, 0xf0);
        }

        [Test]
        public void X86Dis_lmsw_0f01f3()
        {
            AssertCode64("lmsw\tbx", 0x0f, 0x01, 0xf3);
        }

        [Test]
        public void X86Dis_lmsw_0f01f6()
        {
            AssertCode64("lmsw\tsi", 0x0f, 0x01, 0xf6);
        }

        [Test]
        public void X86Dis_lmsw_0f01f7()
        {
            AssertCode64("lmsw\tdi", 0x0f, 0x01, 0xf7);
        }

        [Test]
        public void X86Dis_swapgs()
        {
            AssertCode64("swapgs", 0x0f, 0x01, 0xf8);
        }

        [Test]
        public void X86Dis_monitorx()
        {
            AssertCode64("monitorx", 0x0f, 0x01, 0xfa);
        }

        [Test]
        public void X86Dis_mwaitx()
        {
            AssertCode64("mwaitx", 0x0f, 0x01, 0xfb);
        }

        [Test]
        public void X86Dis_unpckhps_0f1500()
        {
            AssertCode64("unpckhps\txmm0,[rax]", 0x0f, 0x15, 0x00);
        }

        [Test]
        public void X86Dis_cldemote_0f1c00()
        {
            AssertCode64("cldemote\tbyte ptr [rax]", 0x0f, 0x1c, 0x00);
        }

        [Test]
        public void X86Dis_cldemote_invalid()
        {
            AssertCode64("illegal", 0x0f, 0x1c, 0xC3);
        }

        [Test]
        public void X86Dis_pshufb_0f38004154()
        {
            AssertCode64("pshufb\tmm0,[rcx+54h]", 0x0f, 0x38, 0x00, 0x41, 0x54);
        }

        [Test]
        public void X86Dis_sha1msg2()
        {
            AssertCode64("sha1msg2\txmm6,[rbx+rcx+48h]", 0x0f, 0x38, 0xca, 0x74, 0x0b, 0x48);
        }

        [Test]
        public void X86Dis_shrx()
        {
            AssertCode64("shrx\teax,r11d,r8d", "C4C23BF7C3");
        }

        [Test]
        public void X86Dis_packsswb_0f63dd()
        {
            AssertCode64("packsswb\tmm3,mm5", 0x0f, 0x63, 0xdd);
        }

        [Test]
        public void X86Dis_psrlq()
        {
            AssertCode64("psrlq\tmm3,2h", 0x0f, 0x73, 0xD3, 0x02);
        }

        [Test]
        public void X86Dis_psrld()
        {
            AssertCode64("psrld\tmm3,0Bh", "0F72D30B");
            AssertCode64("psrld\tmm4,[rdi+8E42A09h]", "0FD2A7092AE408");
            AssertCode64("vpsrld\txmm12,xmm6,[rdi+8E42A09h]", "C549D2A7092AE408");
        }

        [Test]
        public void X86Dis_haddpd()
        {
            AssertCode64("haddpd\txmm0,xmm3", 0x66, 0x0f, 0x7c, 0xC3);
        }

        [Test]
        public void X86Dis_haddps()
        {
            AssertCode64("haddps\txmm0,xmm3", 0xF2, 0x0f, 0x7c, 0xC3);
        }

        [Test]
        public void X86Dis_hsubpd()
        {
            AssertCode64("hsubpd\txmm2,xmm4", 0x66, 0x0f, 0x7d, 0xD4);
        }

        [Test]
        public void X86Dis_rsm()
        {
            AssertCode64("rsm", 0x0f, 0xaa);
        }

        [Test]
        public void X86Dis_btr()
        {
            AssertCode64("btr\t[rsp+30h],eax", 0x0f, 0xb3, 0x44, 0x24, 0x30);
        }

        [Test]
        public void X86Dis_jmpe()
        {
            AssertCode64("jmpe", 0x0f, 0xb8);
        }

        [Test]
        public void X86Dis_ud1()
        {
            AssertCode64("ud1\tesp,[rax+0F000001h]", 0x0f, 0xb9, 0xa0, 0x01, 0x00, 0x00, 0x0f);
        }

        [Test]
        public void X86Dis_cvtdq2pd()
        {
            AssertCode64("cvtdq2pd\txmm0,xmm4", 0xF3, 0x0F, 0xE6, 0xC4);
        }

        [Test]
        public void X86Dis_sgdt()
        {
            AssertCode64("sgdt\t[rax]", 0x0f, 0x01, 0x00);
        }

        [Test]
        public void X86Dis_sldt()
        {
            AssertCode64("sldt\tes:[rcx-7Dh]", 0x26, 0x0f, 0x00, 0x41, 0x83);
        }

        [Test]
        public void X86Dis_sldt_0f00c1()
        {
            AssertCode64("sldt\tecx", 0x0f, 0x00, 0xc1);
        }

        [Test]
        public void X86Dis_str()
        {
            AssertCode64("str\tword ptr cs:[rax-7Bh]", 0x2e, 0x0f, 0x00, 0x48, 0x85);
        }

        [Test]
        public void X86Dis_xsave64_480fae27()
        {
            AssertCode64("xsave64\tbyte ptr [rdi]", 0x48, 0x0f, 0xae, 0x27);
        }

        [Test]
        public void X86Dis_btr_rax()
        {
            AssertCode64("btr\t[rsp+30h],rax", 0x48, 0x0f, 0xb3, 0x44, 0x24, 0x30);
        }

        [Test]
        public void X86Dis_cmpxchg16b()
        {
            AssertCode64("cmpxchg16b\txmmword ptr [rsp+20h]", 0x0f, 0xc7, 0x4c, 0x24, 0x20);
        }

        [Test]
        public void X86Dis_invpcid()
        {
            AssertCode64("invpcid\teax,xmmword ptr [rcx]", 0x66, 0x0f, 0x38, 0x82, 0x01);
        }

        [Test]
        public void X86Dis_vpacksswb_c4014963cc()
        {
            AssertCode64("vpacksswb\txmm9,xmm6,xmm12", 0xc4, 0x01, 0x49, 0x63, 0xcc);
        }

        [Test]
        public void X86Dis_vpacksswb_c40175634183()
        {
            AssertCode64("vpacksswb\tymm8,ymm1,[r9-7Dh]", 0xc4, 0x01, 0x75, 0x63, 0x41, 0x83);
        }

        [Test]
        public void X86Dis_vpunpckhqdq()
        {
            AssertCode64("vpunpckhqdq\tymm9,ymm1,[r8-77h]", 0xc4, 0x01, 0x75, 0x6d, 0x48, 0x89);
        }

        [Test]
        public void X86Dis_vpshufb()
        {
            AssertCode64("vpshufb\txmm9,xmm7,[r8-75h]", 0xc4, 0x02, 0x41, 0x00, 0x48, 0x8b);
        }

        [Test]
        public void X86Dis_vfnmsub231ps_c40241beff()
        {
            AssertCode64("vfnmsub231ps\txmm15,xmm7,xmm15", 0xc4, 0x02, 0x41, 0xbe, 0xff);
        }

        [Test]
        public void X86Dis_vtestpd_c402450fb63424()
        {
            AssertCode64("vtestpd\tymm14,[r14+2434h]", 0xc4, 0x02, 0x45, 0x0f, 0xb6, 0x34, 0x24, 0x00, 0x00);
        }

        [Test]
        public void X86Dis_phaddsw()
        {
            AssertCode64("phaddsw\tmm0,mm3", 0xc4, 0x02, 0x74, 0x03, 0xC3);
        }

        [Test]
        public void X86Dis_phsubw()
        {
            AssertCode64("phsubw\tmm0,mm3", 0xc4, 0x02, 0x74, 0x05, 0xC3);
        }

        [Test]
        public void X86Dis_psignb()
        {
            AssertCode64("psignb\tmm0,mm3", 0xc4, 0x02, 0x74, 0x08, 0xC3);
        }

        [Test]
        public void X86Dis_vphaddd_c4027502f3()
        {
            AssertCode64("vphaddd\tymm14,ymm1,ymm11", 0xc4, 0x02, 0x75, 0x02, 0xf3);
        }
        [Test]
        public void X86Dis_vphaddsw()
        {
            AssertCode64("vphaddsw\tymm14,ymm1,[r9]", 0xc4, 0x02, 0x75, 0x03, 0x31);
        }
        [Test]
        public void X86Dis_vphsubw()
        {
            AssertCode64("vphsubw\tymm11,ymm1,[r11+5Dh]", 0xc4, 0x02, 0x75, 0x05, 0x5b, 0x5d);
        }

        [Test]
        public void X86Dis_vphsubsw_c4027507f0()
        {
            AssertCode64("vphsubsw\tymm14,ymm1,ymm8", 0xc4, 0x02, 0x75, 0x07, 0xf0);
        }

        [Test]
        public void X86Dis_vpsignb()
        {
            AssertCode64("vpsignb\tymm8,ymm1,[r9-0Ah]", 0xc4, 0x02, 0x75, 0x08, 0x41, 0xf6);
        }

        [Test]
        public void X86Dis_vpmulhrsw_c402750b4883()
        {
            AssertCode64("vpmulhrsw\tymm9,ymm1,[r8-7Dh]", 0xc4, 0x02, 0x75, 0x0b, 0x48, 0x83);
        }

        [Test]
        public void X86Dis_vpermps()
        {
            AssertCode64("vpermps\tymm9,ymm1,[r8-77h]", 0xc4, 0x02, 0x75, 0x16, 0x48, 0x89);
        }

        [Test]
        public void X86Dis_vptest()
        {
            AssertCode64("vptest\tymm14,[r9]", 0xc4, 0x02, 0x75, 0x17, 0x31);
        }

        [Test]
        public void X86Dis_vpabsd()
        {
            AssertCode64("vpabsd\tymm9,[r8-75h]", 0xc4, 0x02, 0x75, 0x1e, 0x48, 0x8b);
        }

        [Test]
        public void X86Dis_vpmovsxbw()
        {
            AssertCode64("vpmovsxbw\tymm9,qword ptr [r9-7Dh]", "C4 02 75 20 49 83");
        }

        [Test]
        public void X86Dis_vpmovsxbd()
        {
            AssertCode64("vpmovsxbd\tymm12,dword ptr [r13-75h]", "C4027521658B");
        }

        [Test]
        public void X86Dis_pmovsxbq()
        {
            AssertCode64("illegal", "0F3822418B");
            AssertCode64("pmovsxbq\txmm0,word ptr [rcx-75h]", "660F3822418B");
            AssertCode64("vpmovsxbq\tymm8,word ptr [r9-75h]", "C4027522418B");
        }

        [Test]
        public void X86Dis_vpmovsxwq()
        {
            AssertCode64("vpmovsxwq\tymm9,qword ptr [r8+39h]", 0xc4, 0x02, 0x75, 0x24, 0x48, 0x39);
        }

        [Test]
        public void X86Dis_vpmuldq()
        {
            AssertCode64("vpmuldq\tymm14,ymm1,[r9]", 0xc4, 0x02, 0x75, 0x28, 0x31);
        }

        [Test]
        public void X86Dis_vpcmpeqq()
        {
            AssertCode64("vpcmpeqq\tymm9,ymm1,[r11+15h]", 0xc4, 0x02, 0x75, 0x29, 0x4b, 0x15);
        }

        [Test]
        public void X86Dis_vmaskmovpd_toreg()
        {
            AssertCode64("vmaskmovpd\tymm11,ymm1,[r11+48h]", 0xc4, 0x02, 0x75, 0x2d, 0x5b, 0x48);
        }

        [Test]
        public void X86Dis_vmaskmovps_tomem()
        {
            AssertCode64("vmaskmovps\t[r8+123400h],ymm1,ymm8", 0xc4, 0x02, 0x75, 0x2e, 0x80, 0x00, 0x34, 0x12, 0x00);
        }

        [Test]
        public void X86Dis_vmaskmovps_2()
        {
            AssertCode64("vmaskmovps\t[r8-0F734h],ymm1,ymm8", "C402752E80CC08FFFF");
        }

        [Test]
        public void X86Dis_vmaskmovpd_tomem()
        {
            AssertCode64("vmaskmovpd\t[r11+43h],ymm1,ymm9", 0xc4, 0x02, 0x75, 0x2f, 0x4b, 0x43);
        }

        [Test]
        public void X86Dis_vpmovzxbd()
        {
            AssertCode64("vpmovzxbd\tymm8,dword ptr [r9-7Dh]", "C4 02 75 31 41 83");
        }

        [Test]
        public void X86Dis_vpmovzxbq()
        {
            AssertCode64("vpmovzxbq\tymm9,word ptr [r11+7Bh]", "C4 02 75 32 4B 7B");
        }

        [Test]
        [Ignore("Intel opcode map _appears_ to imply that 0x66 prefix is required, but none seen.")]
        public void X86Dis_vpmovzxbq_2()
        {
            AssertCode64("vpmovzxbq\tymm9,DWORD PTR [r11-0x12ceef85<32>]", 0xc4, 0x02, 0x75, 0x32, 0x8b, 0x7b, 0x10);
        }

        [Test]
        public void X86Dis_vpminuw()
        {
            AssertCode64("vpminuw\tymm14,ymm1,ymm14", 0xc4, 0x02, 0x75, 0x3a, 0xf6);
        }

        [Test]
        public void X86Dis_vpmaxuw()
        {
            AssertCode64("vpmaxuw\tymm8,ymm1,[r8+70h]", 0xc4, 0x02, 0x75, 0x3e, 0x40, 0x70);
        }
        [Test]
        public void X86Dis_vpmaxud()
        {
            AssertCode64("vpmaxud\tymm9,ymm1,[r9-75h]", 0xc4, 0x02, 0x75, 0x3f, 0x49, 0x8b);
        }
        [Test]
        public void X86Dis_vpsllvd()
        {
            AssertCode64("vpsllvd\tymm9,ymm1,[r15]", 0xc4, 0x02, 0x75, 0x47, 0x0f);
        }

        [Test]
        public void X86Dis_vpbroadcastb()
        {
            AssertCode64("vpbroadcastb\tymm9,byte ptr [r9+r9*4-19h]", 0xc4, 0x02, 0x75, 0x78, 0x4c, 0x89, 0xe7);
        }

        [Test]
        public void X86Dis_verr_sp()
        {
            AssertCode64("verr\tsp", 0xc4, 0x61, 0x29, 0x00, 0xE4);
        }

        [Test]
        public void X86Dis_vfmadd213ps()
        {
            AssertCode64("vfmadd213ps\tymm9,ymm1,[r8+63h]", 0xc4, 0x02, 0x75, 0xa8, 0x48, 0x63);
        }

        [Test]
        public void X86Dis_vaesenc()
        {
            AssertCode64("vaesenc\txmm8,xmm1,[r11-20h]", 0xc4, 0x02, 0x75, 0xdc, 0x43, 0xe0, 0xf7, 0x00);
        }

        [Test]
        public void X86Dis_vpgatherqq()
        {
            AssertCode64("vgatherqq\txmm8,xmm2,[r8]", "C402E99100");
        }

        [Test]
        public void X86Dis_vpshufb_ymm()
        {
            AssertCode64("vpshufb\tymm9,ymm10,[rax-75h]", 0xc4, 0x22, 0x2d, 0x00, 0x48, 0x8b);
        }

        [Test]
        public void X86Dis_vphaddw()
        {
            AssertCode64("vphaddw\tymm9,ymm15,[rdi]", 0xc4, 0x62, 0x05, 0x01, 0x0f);
        }

        [Test]
        public void X86Dis_vpunpckhqdq_sib()
        {
            AssertCode64("vpunpckhqdq\tymm6,ymm8,[r13+r9+1010101h]", 0xc4, 0x81, 0x3d, 0x6d, 0xb4, 0x0d, 0x01, 0x1, 0x01, 0x01);
        }

        [Test]
        public void X86Dis_vpermpd()
        {
            AssertCode64("vpermpd\tymm5,ymm11,0AEh", 0xc4, 0x83, 0xc5, 0x01, 0xeb, 0xae);
        }

        [Test]
        public void X86Dis_femms()
        {
            AssertCode64("femms", 0xc4, 0xc1, 0xc0, 0x0e);
        }

        [Test]
        public void X86Dis_addsubpd()
        {
            AssertCode64("vaddsubpd\txmm7,xmm2,xmm6", 0xc5, 0xe9, 0xd0, 0xfe);
        }

        [Test]
        public void X86Dis_vaddsubpd()
        {
            AssertCode64("vaddsubpd\txmm13,xmm14,xmm0", 0xc5, 0x09, 0xd0, 0xe8);
        }

        [Test]
        public void X86Dis_vaddsubps_c537d0ff()
        {
            AssertCode64("vaddsubps\tymm15,ymm9,ymm7", 0xc5, 0x37, 0xd0, 0xff);
        }

        [Test]
        public void X86Dis_vcvtdq2pd()
        {
            AssertCode64("vcvtdq2pd\txmm8,[rcx+5C415D5Bh]", 0xc5, 0x3a, 0xe6, 0x81, 0x5b, 0x5d, 0x41, 0x5C);
        }

        [Test]
        public void X86Dis_vpacksswb()
        {
            AssertCode64("vpacksswb\txmm8,xmm6,[rsp+4h]", 0xc5, 0x49, 0x63, 0x44, 0x24, 0x04);
        }

        [Test]
        public void X86Dis_vpacksswb_c54d63f5()
        {
            AssertCode64("vpacksswb\tymm14,ymm6,ymm5", 0xc5, 0x4d, 0x63, 0xf5);
        }

        [Test]
        public void X86Dis_vmovlps()
        {
            AssertCode64("vmovlps\tqword ptr [rax],xmm8", 0xc5, 0x50, 0x13, 0x00);
        }

        [Test]
        public void X86Dis_vcvttpd2dq_c551e6ff()
        {
            AssertCode64("vcvttpd2dq\txmm15,xmm7", 0xc5, 0x51, 0xe6, 0xff);
        }

        [Test]
        public void X86Dis_vunpckhps_c56015ca()
        {
            AssertCode64("vunpckhps\txmm9,xmm3,xmm2", 0xc5, 0x60, 0x15, 0xca);
        }

        [Test]
        public void X86Dis_vunpckhps()
        {
            AssertCode64("vunpckhps\tymm9,ymm1,[rbx+50h]", 0xc5, 0x74, 0x15, 0x4b, 0x50);
        }

        [Test]
        public void X86Dis_vpunpckhqdq_ymm()
        {
            AssertCode64("vpunpckhqdq\tymm14,ymm1,[rcx]", 0xc5, 0x75, 0x6d, 0x31);
        }

        [Test]
        public void X86Dis_vhaddpd_ymm()
        {
            AssertCode64("vhaddpd\tymm9,ymm1,[rax-75h]", 0xc5, 0x75, 0x7c, 0x48, 0x8b);
        }

        [Test]
        public void X86Dis_vhsubpd_ymm()
        {
            AssertCode64("vhsubpd\tymm9,ymm1,[rbp+rcx*4-5Dh]", 0xc5, 0x75, 0x7d, 0x4c, 0x8d, 0xa3);
        }

        [Test]
        public void X86Dis_vcvttpd2dq_mem()
        {
            AssertCode64("vcvttpd2dq\txmm14,ymmword ptr [rcx]", 0xc5, 0x75, 0xe6, 0x31);
        }

        [Test]
        public void X86Dis_vhaddps_c5777cf7()
        {
            AssertCode64("vhaddps\tymm14,ymm1,ymm7", 0xc5, 0x77, 0x7c, 0xf7);
        }

        [Test]
        public void X86Dis_vaddsubps()
        {
            AssertCode64("vaddsubps\tymm9,ymm1,[rax-77h]", 0xc5, 0x77, 0xd0, 0x48, 0x89);
        }

        [Test]
        public void X86Dis_vpunpcklqdq_ymm()
        {
            AssertCode64("vpunpcklqdq\tymm8,ymm0,[rbp+31h]", 0xc5, 0x7d, 0x6c, 0x45, 0x31);
        }

        [Test]
        public void X86Dis_vmovlps_memdst()
        {
            AssertCode64("vmovlps\tqword ptr [rdx],xmm0", 0xc5, 0x80, 0x13, 0x02);
        }

        [Test]
        public void X86Dis_vcvtpd2dq()
        {
            AssertCode64("vcvtpd2dq\txmm0,[rsp+rsi*2]", 0xc5, 0x83, 0xe6, 0x04, 0x74);
        }

        [Test]
        public void X86Dis_vlddqu_c583f001()
        {
            AssertCode64("vlddqu\txmm0,[rcx]", 0xc5, 0x83, 0xf0, 0x01);
        }

        [Test]
        public void X86Dis_vunpckhps_ymm()
        {
            AssertCode64("vunpckhps\tymm0,ymm14,[rax]", 0xc5, 0x8c, 0x15, 0x00);
        }

        [Test]
        public void X86Dis_vpunpcklqdq_rip()
        {
            AssertCode64("vpunpcklqdq\tymm5,ymm14,[rip+4CE889FFh]", 0xc5, 0x8d, 0x6c, 0x2d, 0xff, 0x89, 0xe8, 0x4C);
        }

        [Test]
        public void X86Dis_vcvtpd2dq_c59fe6ff()
        {
            AssertCode64("vcvtpd2dq\txmm7,ymm7", 0xc5, 0x9f, 0xe6, 0xff);
        }

        [Test]
        public void X86Dis_vpunpckhqdq_c5b96deb()
        {
            AssertCode64("vpunpckhqdq\txmm5,xmm8,xmm3", 0xc5, 0xb9, 0x6d, 0xeb);
        }

        [Test]
        public void X86Dis_vunpckhps_c5c015c0()
        {
            AssertCode64("vunpckhps\txmm0,xmm7,xmm0", 0xc5, 0xc0, 0x15, 0xc0);
        }

        [Test]
        public void X86Dis_vunpckhps_c5e015c4()
        {
            AssertCode64("vunpckhps\txmm0,xmm3,xmm4", 0xc5, 0xe0, 0x15, 0xc4);
        }

        [Test]
        public void X86Dis_vpacksswb_xmm()
        {
            AssertCode64("vpacksswb\txmm0,xmm3,[rax]", 0xc5, 0xe1, 0x63, 0x00);
        }

        [Test]
        public void X86Dis_vunpckhps_xmm()
        {
            AssertCode64("vunpckhps\txmm2,xmm2,[rdi]", 0xc5, 0xe8, 0x15, 0x17);
        }

        [Test]
        public void X86Dis_vunpckhpd_c5e915ff()
        {
            AssertCode64("vunpckhpd\txmm7,xmm2,xmm7", 0xc5, 0xe9, 0x15, 0xff);
        }

        [Test]
        public void X86Dis_vpacksswb_c5e963ff()
        {
            AssertCode64("vpacksswb\txmm7,xmm2,xmm7", 0xc5, 0xe9, 0x63, 0xff);
        }

        [Test]
        public void X86Dis_vpunpcklqdq_c5e96cfb()
        {
            AssertCode64("vpunpcklqdq\txmm7,xmm2,xmm3", 0xc5, 0xe9, 0x6c, 0xfb);
        }

        [Test]
        public void X86Dis_vpunpckhqdq_c5e96dfe()
        {
            AssertCode64("vpunpckhqdq\txmm7,xmm2,xmm6", 0xc5, 0xe9, 0x6d, 0xfe);
        }

        [Test]
        public void X86Dis_vpslld_c5e972f2ff()
        {
            AssertCode64("vpslld\txmm2,xmm2,0FFh", 0xc5, 0xe9, 0x72, 0xf2, 0xff);
        }

        [Test]
        public void X86Dis_vhaddpd_c5e97cfd()
        {
            AssertCode64("vhaddpd\txmm7,xmm2,xmm5", 0xc5, 0xe9, 0x7c, 0xfd);
        }

        [Test]
        public void X86Dis_vaddsubps_c5efd0ff()
        {
            AssertCode64("vaddsubps\tymm7,ymm2,ymm7", 0xc5, 0xef, 0xd0, 0xff);
        }

        [Test]
        public void X86Dis_fneni()
        {
            AssertCode64("fneni", 0xdb, 0xe0);
        }

        [Test]
        public void X86Dis_fndisi()
        {
            AssertCode64("fndisi", 0xdb, 0xe1);
        }

        [Test]
        public void X86Dis_fnsetpm_287_dbe4()
        {
            AssertCode64("fnsetpm", 0xdb, 0xe4);
        }

        [Test]
        public void X86Dis_frstpm_287_dbe5()
        {
            AssertCode64("frstpm", 0xdb, 0xe5);
        }

        [Test]
        public void X86Dis_ffreep_dfc1()
        {
            AssertCode64("ffreep\tst(1)", 0xdf, 0xc1);
        }

        [Test]
        public void X86Dis_vmovdqa()
        {
            AssertCode64("vmovdqa\txmm1,[rbp-40h]", 0xC5, 0xF9, 0x6F, 0x4D, 0xC0);
        }

        [Test]
        public void X86Dis_vmaskmovpd()
        {
            AssertCode64("vmaskmovpd\tymm11,ymm1,[r11+48h]", 0xc4, 0x02, 0x75, 0x2d, 0x5b, 0x48);
        }

        [Test]
        public void X86Dis_vpminuw_c402753af6()
        {
            AssertCode64("vpminuw\tymm14,ymm1,ymm14", 0xc4, 0x02, 0x75, 0x3a, 0xf6);
        }

        [Test]
        public void X86Dis_vunpckhpd()
        {
            AssertCode64("vunpckhpd\tymm8,ymm1,[rcx-7Dh]", 0xc5, 0x75, 0x15, 0x41, 0x83);
        }

        [Test]
        public void X86Dis_ffreep()
        {
            AssertCode64("ffreep\tst(0)", 0xdf, 0xc0);
        }

        [Test]
        public void X86Dis_expected_bad_group11_instruction()
        {
            AssertCode64("illegal", 0xC6, 0xAE, 0x53, 0x87, 0x7A, 0x9B, 0xC3);
        }

        [Test]
        public void X86Dis_mov_Ea_imm_group11()
        {
            AssertCode64("mov\tbyte ptr [rsi+9B7A8753h],0C3h", 0xC6, 0x86, 0x53, 0x87, 0x7A, 0x9B, 0xC3);
        }

        [Test]
        public void X86Dis_move_ea_group11_16bit()
        {
            var instr = Disassemble16(0xC6, 0xAE, 0x53, 0x87, 0x7A);
            Assert.AreEqual("mov\tbyte ptr [bp+8753h],7Ah", instr.ToString());
        }

        [Test]
        public void X86Dis_pop_group_1A_16bit()
        {
            AssertCode16("pop\tbx", "8FC3");
            AssertCode16("pop\tbx", "8FCB");
        }

        [Test]
        public void X86Dis_pop_group_1A_64bit()
        {
            AssertCode64("pop\tebx", "8FC3");
            AssertCode64("illegal", "8FCF");
        }

        // X86-64
        [Test]
        public void X86Dis_out_64()
        {
            AssertCode64("out\tdx,eax", "EF");
            AssertCode64("out\t50h,eax", "E7 50");
        }

        [Test]
        public void X86Dis_cwd16()
        {
            AssertCode16("cwd", "99");
            AssertCode16("cdq", "66 99");
        }

        [Test]
        public void X86Dis_cdq64()
        {
            AssertCode64("cdq", "99");
            AssertCode64("cqo", "48 99");
        }

        [Test]
        public void X86Dis_cbw16()
        {
            AssertCode16("cbw", "98");
            AssertCode16("cwde", "66 98");
        }

        [Test]
        public void X86Dis_cdqe64()
        {
            AssertCode64("cwde", "98");
            AssertCode64("cdqe", "48 98");
        }

        [Test]
        public void X86Dis_mov_ds_64()
        {
            AssertCode64("mov\tds,di", "8E DF");
        }

        [Test]
        public void X86Dis_movsxd_64()
        {
            AssertCode64("movsxd\tesp,dword ptr [rcx]", "63 21");
            AssertCode64("movsxd\trsp,dword ptr [rcx]", "48 63 21");
        }

        [Test]
        public void X86Dis_jcxz()
        {
            AssertCode16("jcxz\t0006h", "E3 04");
            AssertCode64("jrcxz\t10006h", "E3 04");
        }

        [Test]
        public void X86Dis_adc_64()
        {
            AssertCode64("adc\tdword ptr [edx-27h],6E13A5B4h", "67 81 52 D9 B4 A5 13 6E");
        }

        [Test]
        public void X86Dis_fisub()
        {
            AssertCode64("fisub\tword ptr [ebx]", "67 DE 23");
        }

        [Test]
        public void X86Dis_addps()
        {
            AssertCode64("addps\txmm1,[rdi-12h]", "0F584FEE");
            AssertCode64("vaddps\txmm1,xmm10,[rdi-12h]", "C5A8584FEE7F");
        }

        [Test]
        public void X86Dis_xabort()
        {
            AssertCode64("xabort\t42h", "C6F842");
        }

        [Test]
        public void X86Dis_shufps()
        {
            AssertCode64("shufps\txmm5,xmm6,0DEh", "0FC6EEDE");
        }

        [Test]
        public void X86Dis_vshufps_VEX()
        {
            AssertCode64("vshufps\txmm13,xmm5,xmm6,0DEh", "C550C6EEDE");
            AssertCode64("vshufps\tymm13,ymm5,ymm6,0DEh", "C554C6EEDE");
        }

        [Test]
        public void X86Dis_pmulhw()
        {
            AssertCode64("pmulhw\tmm5,[rbx]", "0FE52B");
            AssertCode64("vpmulhw\tymm5,ymm4,[rbx]", "C5DDE52B");
        }

        [Test]
        public void X86Dis_andps()
        {
            AssertCode64("andps\txmm6,[rdi-1Dh]", "0F5477E3");
        }

        [Test]
        public void X86Dis_vandps()
        {
            AssertCode64("vandps\txmm6,xmm9,[rdi-1Dh]", "C5B05477E3");
        }

        [Test]
        public void X86Dis_punpckhwd()
        {
            AssertCode64("punpckhwd\tmm6,dword ptr [rcx+7h]", "0F697107");
            AssertCode64("vpunpckhwd\tymm14,ymm12,[rcx+7h]", "C51D697107");
        }

        [Test]
        public void X86Dis_cmpss()
        {
            AssertCode64("cmpss\txmm2,dword ptr [rdi+27h],69h", "F30FC2572769");
            AssertCode64("vcmpss\txmm10,xmm10,[rdi+27h],69h", "C52EC2572769");
        }

        [Test]
        public void X86Dis_pandn()
        {
            AssertCode64("pandn\tmm7,[rcx+0C148E7D4h]", "0FDFB9D4E748C1");
            AssertCode64("vpandn\txmm15,xmm6,[rcx+0C148E7D4h]", "C549DFB9D4E748C1");
        }

        [Test]
        public void X86Dis_psubd()
        {
            AssertCode64("psubd\tmm6,[rdi]", "0FFA37");
            AssertCode64("psubd\txmm6,[rdi]", "660FFA37");
            AssertCode64("vpsubd\tymm6,ymm1,[rdi]", "C5F5FA37"); //  833563B6A4BC9ADAF9C455");
        }

        [Test]
        public void X86Dis_pxor()
        {
            AssertCode64("pxor\tmm1,mm2", "0FEFCA"); //  BFCFF487E40067119259D0");
            AssertCode64("vpxor\txmm9,xmm10,xmm2", "C529EFCA"); // BFCFF487E40067119259D0");
        }

        [Test]
        public void X86Dis_andnps()
        {
            AssertCode64("andnps\txmm3,[rbx]", "0F551B");
        }

        [Test]
        public void X86Dis_andnpd()
        {
            AssertCode64("andnpd\txmm3,[rbx]", "660F551B");
            AssertCode64("vandnpd\tymm3,ymm4,[rbx]", "C5DD551B");
        }

        [Test]
        public void X86Dis_vcmpneqpd()
        {
            AssertCode64("cmpps\txmm4,[rsi],4h", "0FC22604");
            AssertCode64("vcmppd\txmm12,xmm3,[rsi],4h", "C561C22604");
        }

        [Test]
        [Ignore("EVEX prefix is horrendously complex.")]
        public void X86Dis_vpermt2pd()
        {
            AssertCode64("vpermt2pd\tymm12{k7},ymm25,QWORD PTR [r11]{1to4}", "6252B5377F2312423CAC2746CA518E");
        }

        [Test]
        public void X86Dis_and_64_rex()
        {
            AssertCode64("and\trax,-3BDB14BCh", "48 25 44 EB 24 C4");
        }

        [Test]
        [Ignore("Discovered by RekoSifter tool")]
        public void X86Dis_pushw_64()
        {
            AssertCode64("pushw\t0x33f1", "66 68 F1 33");
        }

        [Test]
        public void X86Dis_vpaddsb()
        {
            AssertCode64("vpaddsb\txmm8,xmm10,[rdi]", "C529EC07");
        }

        [Test]
        public void X86Dis_ReservedNop_0F18()
        {
            AssertCode64("nop", "0F1826");
        }

        [Test]
        public void X86Dis_xchg()
        {
            AssertCode64("xchg\tr13,rax", "49 95");
        }

        [Test]
        public void X86Dis_cvtpi2ps()
        {
            AssertCode64("cvtpi2ps\txmm7,[rdx+0B24DEEE0h]", "0F 2A BA E0 EE 4D B2");
        }

        [Test]
        [Ignore("Think about this, some more -- Intel manual allows it")]
        public void X86Dis_enterw()
        {
            AssertCode64("enterw\t0x854,0xD5", "66 C8 54 08 D5");
        }

        [Test]
        public void X86Dis_Redundant_SIB()
        {
            AssertCode64("add\t[rax],ebp", "012c20");
        }

        [Test]
        public void X86Dis_mov_dr0_rax()
        {
            AssertCode64("mov\tdr0,rax", "0F23C0");
        }

        [Test]
        public void X86Dis_out_with_rex_prefix()
        {
            // REX prefix should have no effect.
            AssertCode64("out\tdx,al", "4F EE");
        }

        [Test]
        public void X86Dis_rw_mov_seg()
        {
            AssertCode64("mov\t[rax+76h],cs", "8C 48 76    ");
            AssertCode64("mov\t[rax+76h],cs", "48 8C 48 76 ");
            AssertCode64("mov\teax,cs", "8c c8       ");
            AssertCode64("mov\tax,cs", "66 8c c8    ");
            AssertCode64("mov\trax,cs", "66 48 8c c8 ");       // REX prefix wins
            AssertCode64("mov\tax,cs", "48 66 8c c8 ");        // data size override prefix wins
        }

        [Test]
        public void X86Dis_push_64_size_override()
        {
            AssertCode64("push\tcx", "66 51");
        }

        public void X86Dis_adcx()
        {
            AssertCode64("adcx\teax,ebx", "660F38F6C3");
        }

        [Test]
        public void X86Dis_adox()
        {
            AssertCode64("adox\teax,esp", "F30F38F6C4");
        }

        [Test(Description = "Tests customizing operator separator.")]
        public void X86Dis_OperandRendering()
        {
            var options = new MachineInstructionRendererOptions(
                operandSeparator: ", ");
            var instr = Disassemble16(0x33, 0xc0);
            Assert.AreEqual("xor\tax, ax", instr.ToString(options));
        }

        [Test]
        public void X86Dis_pextrX()
        {
            AssertCode64("pextrb\tebx,xmm0,4h",             "66 0f 3a 14 C3 04");
            AssertCode64("pextrb\tbyte ptr [rbx],xmm0,4h",  "66 0F 3A 14 03 04");
            AssertCode64("pextrd\tebx,xmm0,4h",             "66 0F 3A 16 C3 04");
            AssertCode64("pextrd\tdword ptr [rbx],xmm0,4h", "66 0F 3A 16 03 04");
            AssertCode64("pextrq\trbx,xmm0,4h",             "66 48 0f 3a 16 c3 04");
            AssertCode64("pextrq\tqword ptr [rbx],xmm0,4h", "66 48 0f 3a 16 03 04");
        }
    }
}