// rtl8188efw.h
// Generated by decompiling rtl8188efw.bin
// using Reko decompiler version 0.11.2.0.

/*
// Equivalence classes ////////////
Eq_1: (struct "Globals")
	globals_t (in globals @ 00000000 : (ptr16 (struct "Globals")))
Eq_2: (segment "Eq_2" (1D byte b001D))
	T_2 (in __data @ 00000000 : (ptr16 Eq_2))
Eq_6: (fn void ())
	T_6 (in fn4EF3 @ 00000797 : ptr16)
Eq_10: (segment "Eq_10" (157 byte b0157) (80EA bcu8 b80EA))
	T_10 (in __data_23 @ 0000079F : selector)
Eq_21: (fn void ())
	T_21 (in fn4AC0 @ 000007A7 : ptr16)
// Type Variables ////////////
globals_t: (in globals @ 00000000 : (ptr16 (struct "Globals")))
  Class: Eq_1
  DataType: (ptr16 Eq_1)
  OrigDataType: (ptr16 (struct "Globals"))
T_2: (in __data @ 00000000 : (ptr16 Eq_2))
  Class: Eq_2
  DataType: (ptr16 Eq_2)
  OrigDataType: (ptr16 (segment (1D T_5 t001D)))
T_3: (in 0<8> @ 0000077C : byte)
  Class: Eq_3
  DataType: byte
  OrigDataType: byte
T_4: (in 0x001D<p16> @ 0000077C : mp16)
  Class: Eq_4
  DataType: (memptr (ptr16 Eq_2) byte)
  OrigDataType: (memptr T_2 (struct (0 T_5 t0000)))
T_5: (in Mem8[__data:0x001D<p16>:byte] @ 0000077C : byte)
  Class: Eq_3
  DataType: byte
  OrigDataType: byte
T_6: (in fn4EF3 @ 00000797 : ptr16)
  Class: Eq_6
  DataType: (ptr16 Eq_6)
  OrigDataType: (ptr16 (fn T_8 ()))
T_7: (in signature of fn4EF3 @ 00000000 : void)
  Class: Eq_7
  DataType: Eq_7
  OrigDataType: 
T_8: (in fn4EF3() @ 00000797 : void)
  Class: Eq_8
  DataType: void
  OrigDataType: void
T_9: (in 5<8> @ 0000079F : byte)
  Class: Eq_9
  DataType: byte
  OrigDataType: byte
T_10: (in __data_23 @ 0000079F : selector)
  Class: Eq_10
  DataType: (ptr16 Eq_10)
  OrigDataType: (ptr16 (segment (157 T_12 t0157) (80EA T_14 t80EA)))
T_11: (in 0x157<16> @ 0000079F : word16)
  Class: Eq_11
  DataType: (memptr (ptr16 Eq_10) byte)
  OrigDataType: (memptr T_10 (struct (0 T_12 t0000)))
T_12: (in Mem32[__data_23:0x157<16>:byte] @ 0000079F : byte)
  Class: Eq_9
  DataType: byte
  OrigDataType: byte
T_13: (in 0x80EA<16> @ 000007A4 : word16)
  Class: Eq_13
  DataType: (memptr (ptr16 Eq_10) bcu8)
  OrigDataType: (memptr T_10 (struct (0 T_14 t0000)))
T_14: (in Mem32[__data_23:0x80EA<16>:byte] @ 000007A4 : byte)
  Class: Eq_14
  DataType: bcu8
  OrigDataType: bcu8
T_15: (in 2<8> @ 000007A4 : byte)
  Class: Eq_15
  DataType: byte
  OrigDataType: byte
T_16: (in __data_23->b80EA >> 2<8> @ 00000000 : byte)
  Class: Eq_16
  DataType: uint8
  OrigDataType: uint8
T_17: (in 1<8> @ 000007A4 : byte)
  Class: Eq_17
  DataType: byte
  OrigDataType: byte
T_18: (in __data_23->b80EA >> 2<8> & 1<8> @ 00000000 : byte)
  Class: Eq_18
  DataType: byte
  OrigDataType: byte
T_19: (in 0<8> @ 000007A4 : byte)
  Class: Eq_18
  DataType: byte
  OrigDataType: byte
T_20: (in (__data_23->b80EA >> 2<8> & 1<8>) != 0<8> @ 00000000 : bool)
  Class: Eq_20
  DataType: bool
  OrigDataType: bool
T_21: (in fn4AC0 @ 000007A7 : ptr16)
  Class: Eq_21
  DataType: (ptr16 Eq_21)
  OrigDataType: (ptr16 (fn T_23 ()))
T_22: (in signature of fn4AC0 @ 00000000 : void)
  Class: Eq_22
  DataType: Eq_22
  OrigDataType: 
T_23: (in fn4AC0() @ 000007A7 : void)
  Class: Eq_23
  DataType: void
  OrigDataType: void
*/
typedef struct Globals {
} Eq_1;

typedef struct Eq_2 {
	byte b001D;	// 1D
} Eq_2;

typedef void (Eq_6)();

typedef struct Eq_10 {
	byte b0157;	// 157
	bcu8 b80EA;	// 80EA
} Eq_10;

typedef void (Eq_21)();

