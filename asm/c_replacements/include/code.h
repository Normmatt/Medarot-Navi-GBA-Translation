#ifndef CODE_H
#define CODE_H

extern void SetCharacterPortrait(u16 id, u8 a2, u8 x, u8 y);
extern void sub_8006F1C(void);
extern void RestartMusic(u16 track);
extern void PlaySoundEffect(u32 id);
extern void DrawCharacter(u8 *destination, u16 character, u16 fg_color, u16 bg_color);
extern void ClearTiles(void *dst, int len_divide16, u32 src);
extern void sub_8001EC4(int a1);
extern int sub_8086598(int a1, int a2, u32 a3);
extern void sub_806B504(int a1);
extern u8 eng_font_bin[];
extern u8 eng_font_widths_bin[];

#define v20183E8 (*(u16*)0x020183E8)
#define current_music_track (*(u16*)0x020183E8)
#define v201B366 (*(u16*)0x0201B366)
#define v30014C4 (*(u16*)0x030014C4)
#define v300159C (*(u16*)0x0300159C)
#define v30018F0 (*(u32*)0x030018F0)
#define v300190C (*(u16*)0x0300190C)
#define v300192C (*(u16*)0x0300192C)
#define byte_862A01C ((u8*)0x0862A01C)
#define byte_862CC3C ((u8*)0x0862CC3C)
#define Font ((u8*)0x08657D60)
#define ConversionLUT ((u16*)0x084C7608)

#endif