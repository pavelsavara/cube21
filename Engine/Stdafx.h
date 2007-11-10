// This file is part of project Cube21
// Whole solution including its LGPL license could be found at
// http://cube21.sf.net/
// 2007 Pavel Savara, http://zamboch.blogspot.com/

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

#define min_cube(corrBigIndex, bigIndex, corrSmallIndex, smallIndex) if (corrSmallIndex < smallIndex || (corrSmallIndex <= smallIndex && corrBigIndex < bigIndex)) { bigIndex = corrBigIndex; smallIndex = corrSmallIndex; }
