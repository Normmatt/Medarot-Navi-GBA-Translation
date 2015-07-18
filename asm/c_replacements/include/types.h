#ifndef _TYPES_H_
#define _TYPES_H_

// define libnds types in terms of stdint
#include <stdint.h>
#include <wchar.h>

typedef struct {
 unsigned char R,G,B;
} Color;


typedef Color* Image;


typedef char* String;


typedef enum {
	TOP_LEFT,
	TOP_RIGHT,
	BOTTOM
} Display;


#define INIT_CODE	__attribute__((section(".init")))
#define TEXT_CODE	__attribute__((section(".text")))

//! aligns a struct (and other types?) to m, making sure that the size of the struct is a multiple of m.
#define ALIGN(m)	__attribute__((aligned (m)))

#define _INLINE		static inline

//! packs a struct (and other types?) so it won't include padding bytes.
#define PACKED __attribute__ ((packed))
#define packed_struct struct PACKED

typedef enum {
	FALSE,
	TRUE
} BOOL;

/*!
\brief returns a number with the nth bit set.
*/

#define BIT(n) (1 << (n))


//! 8 bit unsigned integer.
typedef uint8_t	uint8;
//! 16 bit unsigned integer.
typedef uint16_t	uint16;
//! 32 bit unsigned integer.
typedef uint32_t	uint32;
//! 64 bit unsigned integer.
typedef uint64_t	uint64;
//! 8 bit signed integer.
typedef int8_t	int8;
//! 16 bit signed integer.
typedef int16_t	int16;
//! 32 bit signed integer.
typedef int32_t	int32;
//! 64 bit signed integer.
typedef int64_t	int64;
//! 32 bit signed floating point number.
typedef float	float32;
//! 64 bit signed floating point number.
typedef double	float64;
//! 8 bit volatile unsigned integer.
typedef volatile uint8_t	vuint8;
//! 16 bit volatile unsigned integer.
typedef volatile uint16_t	vuint16;
//! 32 bit volatile unsigned integer.
typedef volatile uint32_t	vuint32;
//! 64 bit volatile unsigned integer.
typedef volatile uint64_t	vuint64;
//! 8 bit volatile signed integer.
typedef volatile int8_t	vint8;
//! 16 bit volatile signed integer.
typedef volatile int16_t	vint16;
//! 32 bit volatile signed integer.
typedef volatile int32_t	vint32;
//! 64 bit volatile signed integer.
typedef volatile int64_t	vint64;
//! 32 bit volatile signed floating point number.
typedef volatile float32 vfloat32;
//! 64 bit volatile signed floating point number.
typedef volatile float64 vfloat64;
//! 8 bit unsigned integer.
typedef uint8_t	byte;
//! 8 bit unsigned integer.
typedef uint8_t	u8;
//! 16 bit unsigned integer.
typedef uint16_t	u16;
//! 32 bit unsigned integer.
typedef uint32_t	u32;
//! 64 bit unsigned integer.
typedef uint64_t	u64;
//! 8 bit signed integer.
typedef int8_t	s8;
//! 16 bit signed integer.
typedef int16_t	s16;
//! 32 bit signed integer.
typedef int32_t	s32;
//! 64 bit signed integer.
typedef int64_t	s64;
//! 8 bit volatile unsigned integer.
typedef volatile u8 vu8;
//! 16 bit volatile unsigned integer.
typedef volatile u16 vu16;
//! 32 bit volatile unsigned integer.
typedef volatile u32 vu32;
//! 64 bit volatile unsigned integer.
typedef volatile u64 vu64;
//! 8 bit volatile signed integer.
typedef volatile s8 vs8;
//! 16 bit volatile signed integer.
typedef volatile s16 vs16;
//! 32 bit volatile signed integer.
typedef volatile s32 vs32;
//! 64 bit volatile signed integer.
typedef volatile s64 vs64;

typedef float 			f32;
typedef volatile f32 	vf32;

typedef double 			f64;
typedef volatile f64 	vf64;

typedef u32 FHANDLE;

typedef u32 Handle;

typedef s32	Result;

typedef u16	char16;

typedef u8	bit8;	// for bitflags
typedef u16 bit16;
typedef u32	bit32;
typedef u64	bit64;

typedef void (*VoidFn)();
typedef void (*IntrFn)();
typedef void (*fp)();
typedef void (*KernelFn)();
typedef void (*ThreadFn)();

#endif