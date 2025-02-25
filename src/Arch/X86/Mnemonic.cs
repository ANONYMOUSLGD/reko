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

namespace Reko.Arch.X86
{
    public enum Mnemonic : ushort
    {
        illegal,

        @in,
        @int,
        @lock,
        @out,
        aaa,
        aad,
        aam,
        aas,
        adc,
        adcx,
        add,
        addpd,
        addps,
        addsd,
        addss,
        addsubpd,
        addsubps,
        adox,
        aesdec,
        aesdeclast,
        aesenc,
        aesenclast,
        aesimc,
        aeskeygen,
        and,
        andn,
        andnpd,
        andnps,
        andpd,
        andps,
        arpl,
        bextr,
        blendpd,
        blendps,
        blendvpd,
        blendvps,
        blendw,
        blsi,
        blsmsk,
        blsr,
        bound,
        bsf,
        bsr,
        bswap,
        bt,
        btc,
        btr,
        bts,
        bzhi,
        call,
        cbw,
        cdq,
        cdqe,
        clac,
        clc,
        cld,
        cldemote,
        clflush,
        cli,
        clts,
        cmc,
        cmova,
        cmovbe,
        cmovc,
        cmovg,
        cmovge,
        cmovl,
        cmovle,
        cmovnc,
        cmovno,
        cmovns,
        cmovnz,
        cmovo,
        cmovpe,
        cmovpo,
        cmovs,
        cmovz,
        cmp,
        cmppd,
        cmpps,
        cmps,
        cmpsb,
        cmpsd,
        cmpss,
        cmpxchg,
        cmpxchg16b,
        cmpxchg8b,
        comisd,
        comiss,
        cpuid,
        cqo,
        cvtdq2pd,
        cvtdq2ps,
        cvtpd2dq,
        cvtpd2ps,
        cvtpd2si,
        cvtph2ps,
        cvtpi2pd,
        cvtpi2ps,
        cvtps2dq,
        cvtps2pd,
        cvtps2pi,
        cvtsd2si,
        cvtsd2ss,
        cvtsi2sd,
        cvtsi2ss,
        cvtss2sd,
        cvtss2si,
        cvttpd2dq,
        cvttpd2pi,
        cvttpd2si,
        cvttps2dq,
        cvttps2pi,
        cvttsd2si,
        cvttss2si,
        cwd,
        cwde,
        daa,
        das,
        dec,
        div,
        divpd,
        divps,
        divsd,
        divss,
        dppd,
        dpps,
        emms,
        endbr32,
        endbr64,
        enter,
        extractps,
        f2xm1,
        fabs,
        fadd,
        faddp,
        fbld,
        fbstp,
        fchs,
        fclex,
        fcmovb,
        fcmovbe,
        fcmove,
        fcmovnb,
        fcmovnbe,
        fcmovne,
        fcmovnu,
        fcmovu,
        fcom,
        fcomi,
        fcomip,
        fcomp,
        fcompp,
        fcos,
        fdecstp,
        fdiv,
        fdivp,
        fdivr,
        fdivrp,
        femms,
        ffree,
        ffreep,
        fiadd,
        ficom,
        ficomp,
        fidiv,
        fidivr,
        fild,
        fildcw,
        fildenv,
        fimul,
        fincstp,
        fist,
        fistcw,
        fistenv,
        fistp,
        fisttp,
        fisub,
        fisubr,
        fld,
        fld1,
        fldcw,
        fldenv,
        fldl2e,
        fldl2t,
        fldlg2,
        fldln2,
        fldpi,
        fldz,
        fmul,
        fmulp,
        fndisi,
        fneni,
        fninit,
        fnop,
        fnsetpm,
        fpatan,
        fprem,
        fprem1,
        fptan,
        frndint,
        frstor,
        frstpm,
        fsave,
        fscale,
        fsin,
        fsincos,
        fsqrt,
        fst,
        fstcw,
        fstenv,
        fstp,
        fstsw,
        fsub,
        fsubp,
        fsubr,
        fsubrp,
        ftst,
        fucom,
        fucomi,
        fucomip,
        fucomp,
        fucompp,
        fxam,
        fxch,
        fxrstor,
        fxsave,
        fxtract,
        fyl2x,
        fyl2xp1,
        getsec,
        haddpd,
        haddps,
        hlt,
        hsubpd,
        hsubps,
        icebp,
        idiv,
        imul,
        inc,
        ins,
        insb,
        inserps,
        into,
        invd,
        invept,
        invlpg,
        invpcid,
        invvpid,
        iret,
        ja,
        jbe,
        jc,
        jcxz,
        jecxz,
        jg,
        jge,
        jl,
        jle,
        jmp,
        jmpe,
        jnc,
        jno,
        jns,
        jnz,
        jo,
        jpe,
        jpo,
        jrcxz,
        js,
        jz,
        lahf,
        lar,
        lddqu,
        ldmxcsr,
        lds,
        lea,
        leave,
        les,
        lfence,
        lfs,
        lgdt,
        lgs,
        lidt,
        lldt,
        lmsw,
        lods,
        lodsb,
        loop,
        loope,
        loopne,
        lsl,
        lss,
        ltr,
        lzcnt,
        maskmovdqu,
        maskmovq,
        maxpd,
        maxps,
        maxsd,
        maxss,
        mfence,
        minpd,
        minps,
        minsd,
        minss,
        monitor,
        monitorx,
        mov,
        movapd,
        movaps,
        movbe,
        movd,
        movddup,
        movdqa,
        movdqu,
        movhpd,
        movhps,
        movlhps,
        movlpd,
        movlps,
        movmskpd,
        movmskps,
        movnti,
        movntpd,
        movntps,
        movntq,
        movq,
        movs,
        movsb,
        movsd,
        movshdup,
        movsldup,
        movss,
        movsx,
        movsxd,
        movupd,
        movups,
        movzx,
        mpsadbw,
        mul,
        mulpd,
        mulps,
        mulsd,
        mulss,
        mulx,
        mwait,
        mwaitx,
        neg,
        nop,
        not,
        or,
        orpd,
        orps,
        outs,
        outsb,
        pabsb,
        pabsd,
        pabsw,
        packssdw,
        packsswb,
        packuswb,
        paddb,
        paddd,
        paddq,
        paddsb,
        paddsw,
        paddusb,
        paddusw,
        paddw,
        palignr,
        pand,
        pandn,
        pause,
        pavgb,
        pavgw,
        pblendvb,
        pblendvdb,
        pclmulqdq,
        pcmpeqb,
        pcmpeqd,
        pcmpeqw,
        pcmpestri,
        pcmpestrm,
        pcmpgtb,
        pcmpgtd,
        pcmpgtw,
        pcmpistri,
        pcmpistrm,
        pdep,
        pext,
        pextrb,
        pextrd,
        pextrq,
        pextrw,
        phaddd,
        phaddsw,
        phaddw,
        phsubd,
        phsubsw,
        phsubw,
        pinsrb,
        pinsrd,
        pinsrq,
        pinsrw,
        pmaddubsw,
        pmaddwd,
        pmaxsw,
        pmaxub,
        pminsw,
        pminub,
        pmovmskb,
        pmovsxbd,
        pmovsxbq,
        pmovsxbw,
        pmovsxwd,
        pmovzxbd,
        pmovzxbq,
        pmovzxwd,
        pmulhrsw,
        pmulhuw,
        pmulhw,
        pmullw,
        pmuludq,
        pop,
        popa,
        popcnt,
        popf,
        por,
        prefetchnta,
        prefetcht0,
        prefetcht1,
        prefetcht2,
        prefetchw,
        psadbw,
        pshufb,
        pshufd,
        pshufhw,
        pshuflw,
        pshufw,
        psignb,
        psignd,
        psignw,
        pslld,
        pslldq,
        psllq,
        psllw,
        psrad,
        psraw,
        psrld,
        psrldq,
        psrlq,
        psrlw,
        psubb,
        psubd,
        psubq,
        psubsb,
        psubsw,
        psubusb,
        psubusw,
        psubw,
        punpckhbw,
        punpckhdq,
        punpckhqdq,
        punpckhwd,
        punpcklbw,
        punpckldq,
        punpcklqdq,
        punpcklwd,
        push,
        pusha,
        pushf,
        pxor,
        rcl,
        rcpps,
        rcpss,
        rcr,
        rdmsr,
        rdpkru,
        rdpmc,
        rdrand,
        rdseed,
        rdtsc,
        rdtscp,
        ret,
        retf,
        rol,
        ror,
        rorx,
        roundpd,
        roundps,
        roundsd,
        roundss,
        rsm,
        rsqrtps,
        rsqrtss,
        sahf,
        sar,
        sarx,
        sbb,
        scas,
        scasb,
        seta,
        setbe,
        setc,
        setg,
        setge,
        setl,
        setle,
        setnc,
        setno,
        setns,
        setnz,
        seto,
        setpe,
        setpo,
        sets,
        setz,
        sfence,
        sgdt,
        sha1msg1,
        sha1msg2,
        sha1nexte,
        sha1rnds4,
        sha256mds2,
        sha256msg1,
        sha256msg2,
        shl,
        shld,
        shlx,
        shr,
        shrd,
        shrx,
        shufpd,
        shufps,
        sidt,
        sldt,
        smsw,
        sqrtpd,
        sqrtps,
        sqrtsd,
        sqrtss,
        stac,
        stc,
        std,
        sti,
        stmxcsr,
        stos,
        stosb,
        str,
        sub,
        subpd,
        subps,
        subsd,
        subss,
        swapgs,
        syscall,
        sysenter,
        sysexit,
        sysret,
        test,
        tzcnt,
        ucomisd,
        ucomiss,
        ud0,
        ud1,
        ud2,
        unpckhpd,
        unpckhps,
        unpcklpd,
        unpcklps,
        vaddpd,
        vaddps,
        vaddsd,
        vaddss,
        vaddsubpd,
        vaddsubps,
        vaesdec,
        vaesdeclast,
        vaesenc,
        vaesenclast,
        vaeskeygen,
        vandnpd,
        vandnps,
        vandpd,
        vandps,
        vblendpd,
        vblendps,
        vblendvb,
        vblendvpd,
        vblendvps,
        vblendw,
        vbroadcastb,
        vbroadcastf128,
        vbroadcastsd,
        vbroadcastss,
        vbroadcastw,
        vcmppd,
        vcmpps,
        vcmpsd,
        vcmpss,
        vcomisd,
        vcomiss,
        vcvtdq2pd,
        vcvtdq2ps,
        vcvtpd2dq,
        vcvtpd2ps,
        vcvtpd2si,
        vcvtph2ps,
        vcvtpi2pd,
        vcvtpi2ps,
        vcvtps2dq,
        vcvtps2pd,
        vcvtps2ph,
        vcvtps2pi,
        vcvtsd2si,
        vcvtsd2ss,
        vcvtsi2sd,
        vcvtsi2ss,
        vcvtss2sd,
        vcvtss2si,
        vcvttpd2dq,
        vcvttpd2pi,
        vcvttps2dq,
        vcvttps2pi,
        vcvttsd2si,
        vcvttss2si,
        vdivpd,
        vdivps,
        vdivsd,
        vdivss,
        vdppd,
        vdpps,
        verr,
        verw,
        vextractf128,
        vextracti128,
        vextractps,
        vextrw,
        vfmadd213pd,
        vfmadd213ps,
        vfmaddsub132pd,
        vfmaddsub132ps,
        vfmsubadd132pd,
        vfmsubadd132ps,
        vfnmsub231ps,
        vfnmsub231ss,
        vgatherdd,
        vgatherdpd,
        vgatherdps,
        vgatherdq,
        vgatherqd,
        vgatherqpd,
        vgatherqps,
        vgatherqq,
        vhaddpd,
        vhaddps,
        vhsubpd,
        vhsubps,
        vinserps,
        vinsertf128,
        vinserti128,
        vlddqu,
        vmaskmovdqu,
        vmaskmovpd,
        vmaskmovps,
        vmaxpd,
        vmaxps,
        vmaxsd,
        vmaxss,
        vmcall,
        vmclear,
        vmfunc,
        vminpd,
        vminps,
        vminsd,
        vminss,
        vmlaunch,
        vmovapd,
        vmovaps,
        vmovd,
        vmovddup,
        vmovdqa,
        vmovdqu,
        vmovhpd,
        vmovhps,
        vmovlhps,
        vmovlpd,
        vmovlps,
        vmovmskpd,
        vmovmskps,
        vmovntdqa,
        vmovntpd,
        vmovntps,
        vmovntq,
        vmovq,
        vmovsd,
        vmovshdup,
        vmovsldup,
        vmovss,
        vmovupd,
        vmovups,
        vmpsadbw,
        vmptrld,
        vmptrst,
        vmread,
        vmresume,
        vmulpd,
        vmulps,
        vmulsd,
        vmulss,
        vmwrite,
        vmxoff,
        vmxon,
        vorpd,
        vorps,
        vpabsb,
        vpabsd,
        vpabsw,
        vpackssdw,
        vpacksswb,
        vpackusdw,
        vpaddb,
        vpaddd,
        vpaddq,
        vpaddsb,
        vpaddsw,
        vpaddusb,
        vpaddusw,
        vpaddw,
        vpand,
        vpandn,
        vpavgb,
        vpavgw,
        vpblendd,
        vpbroadcastb,
        vpbroadcastd,
        vpbroadcasti128,
        vpbroadcastq,
        vpbroadcastw,
        vpclmulqdq,
        vpcmpeqb,
        vpcmpeqd,
        vpcmpeqq,
        vpcmpeqw,
        vpcmpestri,
        vpcmpestrm,
        vpcmpgtb,
        vpcmpgtd,
        vpcmpgtq,
        vpcmpgtw,
        vpcmpistri,
        vpcmpistrm,
        vperm2f128,
        vperm2i128,
        vpermd,
        vpermilpd,
        vpermilps,
        vpermpd,
        vpermps,
        vpermq,
        vpextrb,
        vpextrd,
        vpextrq,
        vpextrw,
        vphaddd,
        vphaddsw,
        vphaddw,
        vphminposuw,
        vphsubd,
        vphsubsw,
        vphsubw,
        vpinsrb,
        vpinsrd,
        vpinsrq,
        vpinsrw,
        vpmaddubsw,
        vpmaddwd,
        vpmaxsb,
        vpmaxsd,
        vpmaxsw,
        vpmaxub,
        vpmaxud,
        vpmaxuw,
        vpminsb,
        vpminsd,
        vpminsw,
        vpminub,
        vpminud,
        vpminuw,
        vpmovmskb,
        vpmovsxbd,
        vpmovsxbq,
        vpmovsxbw,
        vpmovsxdq,
        vpmovsxwd,
        vpmovsxwq,
        vpmovzxbd,
        vpmovzxbq,
        vpmovzxbw,
        vpmovzxdq,
        vpmovzxwd,
        vpmovzxwq,
        vpmuldq,
        vpmulhrsw,
        vpmulhuw,
        vpmulhw,
        vpmulld,
        vpmullw,
        vpmuludq,
        vpor,
        vpsadbw,
        vpshufb,
        vpshufd,
        vpshufhw,
        vpshuflw,
        vpsignb,
        vpsignd,
        vpsignw,
        vpslld,
        vpslldq,
        vpsllq,
        vpsllvd,
        vpsllvq,
        vpsllw,
        vpsrad,
        vpsravd,
        vpsraw,
        vpsrld,
        vpsrldq,
        vpsrlq,
        vpsrlvd,
        vpsrlvq,
        vpsrlw,
        vpsubb,
        vpsubd,
        vpsubq,
        vpsubsb,
        vpsubsw,
        vpsubusb,
        vpsubusw,
        vpsubw,
        vptest,
        vpunpckhbw,
        vpunpckhdq,
        vpunpckhqdq,
        vpunpckhwd,
        vpunpcklbw,
        vpunpckldq,
        vpunpcklqdq,
        vpunpcklwd,
        vpxor,
        vrcpps,
        vrcpss,
        vroundpd,
        vroundps,
        vroundsd,
        vroundss,
        vrsqrtps,
        vrsqrtss,
        vshufpd,
        vshufps,
        vsqrtpd,
        vsqrtps,
        vsqrtsd,
        vsqrtss,
        vsubpd,
        vsubps,
        vsubsd,
        vsubss,
        vtestpd,
        vtestps,
        vucomisd,
        vucomiss,
        vunpckhpd,
        vunpckhps,
        vunpcklpd,
        vunpcklps,
        vxorpd,
        vxorps,
        vzeroall,
        vzeroupper,
        wait,
        wbinvd,
        wrmsr,
        wrpkru,
        xabort,
        xadd,
        xchg,
        xend,
        xgetbv,
        xlat,
        xor,
        xorpd,
        xorps,
        xrstor,
        xsave,
        xsave64,
        xsaveopt,
        xsetbv,
        xtest,

        // Borland implemented an x87 emulator, and introduced these
        // pseudo-instructions.
        BOR_exp,
        BOR_ln,
    }
}
