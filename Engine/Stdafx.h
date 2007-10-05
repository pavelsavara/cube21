// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently,
// but are changed infrequently

#pragma once
#include "Windows.h"


#pragma warning(disable :4793)
#define asm_ror(val, shift) __asm { ror val,shift }
#define asm_rol(val, shift) __asm { rol val,shift }

#define managed_ror(val, shift) val=((val>>shift)|(val<<(32-shift)))
#define managed_rol(val, shift) val=((val<<shift)|(val>>(32-shift)))

#define min_cube(corrBigIndex, bigIndex, corrSmallIndex, smallIndex) if (corrBigIndex<bigIndex || corrSmallIndex<smallIndex) { bigIndex = corrBigIndex; smallIndex = corrSmallIndex; }
