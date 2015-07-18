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

extern u16 v20183E8;
extern u16 current_music_track;

extern u16 v201B366;
extern u16 v30014C4;
extern u16 v300159C; //input?
extern u32 v30018F0;
extern u16 v300190C;
extern u16 v300192C; //input?
extern u8 byte_862A01C[8];
extern u8 byte_862CC3C[20];

#endif