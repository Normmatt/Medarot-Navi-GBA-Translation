#include "types.h"
#include "code.h"
#include "rt.h"
#include "code.h"
//#include "eng_font_bin.h"
//#include "eng_font_widths_bin.h"

#include <string.h>
#include <stdlib.h>

typedef union
{
    struct {
        unsigned char tile; //Max of 0x3F
        unsigned char overflow; //Max of 8
        unsigned char drawn; //Number of characters drawn
        unsigned char english; //Toggle new font?
    } bytes;
    unsigned int current_character;
} CharInfo;

typedef struct struct_string
{
    u8 *string_pointer;
    u8 gap4[2];
    u16 word6;
    s16 delay;
    u8 option;
    s8 byteB; //abuse for new font stuff
    CharInfo info;
} struct_string;

#define VWF

#ifdef VWF
void DrawCharacterVWF(u32 *destination, u16 character, u16 fg_color, u16 bg_color, struct_string *current_string);
#endif

s32 DrawString(struct_string *current_string)
{
	sub_8006F1C();
	if(current_string->delay <= 0)
	{
		//If no string pointer then return error
		if(!current_string->string_pointer)
		{
			return 1;
		}
		
		u8 character = *current_string->string_pointer++;
		
		switch(character)
		{
			case 0x00: //??
			{
				if ( v30014C4 & 1 )
				{
					PlaySoundEffect(120);
					sub_8001EC4(-1);
					ClearTiles((void *)0x6000B40, 64, v30018F0);
					current_string->string_pointer = 0;
				}
				else
				{
					--current_string->string_pointer;
				}
				return 0;
			}
			case 0xE0: //Kanji
			case 0xE1: //Kanji
			{
				u16 kanji = (character << 8) | *current_string->string_pointer++;
				
				current_string->word6 = 0;
				// Use upper bits of v1->current_character to store tile number for use here instead of current_character this well let us make a VWF.
				DrawCharacter((u8 *)(((current_string->info.bytes.tile++ & 0x3F) * 0x40) + 0x6000B40), kanji, 0x1111u, v30018F0);
				
				//extra handling here? what is this for exactly?
				u8 curCharacter = *current_string->string_pointer;
				if ( curCharacter > 0xF2 )
				{
					if ( curCharacter > 0xFA || curCharacter < 0xF8 )
					{
						return 0;
					}
				}
				else
				{
					if ( curCharacter >= 0xF1 )
					{
						current_string->delay = 8;
						return 0;
					}
					if ( !*current_string->string_pointer )
					{
						current_string->delay = 30;
						return 0;
					}
					if ( curCharacter > 0xEB || curCharacter < 0xE8 )
					{
						return 0;
					}
				}
				current_string->word6 = 0;
				return 0;
			}
			case 0xE2:
			{
				--current_string->string_pointer; //Loop this instruction over and over until an option is chosen
				u32 v12 = current_string->option & 1;
				u32 v13 = 2 * v12;
				u8 *option_text_ptr = (u8 *)&byte_862CC3C[10 * v12]; //TODO move this into this file so its easier to modify
				for(int i=0; i<4; i++)
				{
					DrawCharacter((u8 *)((((i + 18) & 0x3F) << 6) + 0x6000B40), *option_text_ptr, 0x1111u, v30018F0);
					DrawCharacter(
						(u8 *)((((i + 50) & 0x3F) << 6) + 0x6000B40),
						*(&byte_862CC3C[5 * (v13 + 1)] + i),
						0x1111u,
						v30018F0);
					option_text_ptr++;
				}
				if ( v300192C & 0xC0 )                  // Up or Down
				{
					PlaySoundEffect(3);
					++current_string->option;
				}
				if ( v300159C & 1 )                     // Button A
				{
					PlaySoundEffect(4);
					++current_string->string_pointer;
				}
				if ( v300159C & 2 )                     // Button B
				{
					PlaySoundEffect(5);
					current_string->option = 1;
					++current_string->string_pointer;
				}
				return 0;
			}
			case 0xE3: //Long Jump
			{
				current_string->string_pointer = (u8*)(current_string->string_pointer[2]<<16 | current_string->string_pointer[1]<<8 | current_string->string_pointer[0]<<0 | 0x08000000);
				//DrawString(current_string);
				return 0;
			}
			case 0xE4: //Toggle New Font
			{
				current_string->info.bytes.english ^= 1;
				return 0;
			}
			case 0xE5: //Set Delay
			{
				u8 temp = *current_string->string_pointer++;
				u8 temp2 = *current_string->string_pointer++;
				current_string->delay = temp2 | (temp << 8);
				DrawString(current_string);
				return 0;
			}
			case 0xE6:
			{
				RestartMusic(current_music_track);
				DrawString(current_string);
				return 0;
			}
			case 0xF1: //New Line
			{
				current_string->info.bytes.tile = 32; //Set this to second row of tiles.
				current_string->info.bytes.overflow = 0;
				return 0;
			}
			case 0xF2: //Wait for key press and end current string
			{
				int temp;
				current_string->info.bytes.tile = 0;
				sub_8086598((int)&temp, (int)0x080E08F8, 2u); //reads two bytes from 0x080E08F8 (why the fuck isn't this just a direct read? why the temporary variable?)
				DrawCharacter(
					(u8 *)(((byte_862A01C[v300190C] & 0x3F) << 6) + 0x6000B40),
					*((u8 *)&temp + ((v201B366 << 16 >> 18) & 1)),
					0x1111u,
					v30018F0);
				++v201B366;
				if ( !(v30014C4 & 1) )
				{
					--current_string->string_pointer;
					return 0;
				}
				PlaySoundEffect(120);
				ClearTiles((void*)0x6000B40, 64, v30018F0);
				return 0;
			}
			case 0xF3: //Clear String Pointer (End string?)
			{
				current_string->string_pointer = 0;
				return 0;
			}
			case 0xF4: //Clear String Pointer and Clear tiles
			{
				current_string->info.bytes.tile = 0;
				ClearTiles((void *)0x6000B40, 64, v30018F0);
				return 0;
			}
			case 0xF5: //Play Sound Effect
			{
				u8 sfx = *current_string->string_pointer++;
				if(((sfx - 0x64) & 0xFFu) <= 0x12 )
				{
					sub_806B504(32);
				}
				PlaySoundEffect(sfx);
				return 0;
			}
			case 0xF6: //Set Character Portrait
			case 0xF7: //Set Character Portrait
			{
				SetCharacterPortrait(*current_string->string_pointer++,0,0,12);
				return 0;
			}
			case 0xF8: //Player Name
			case 0xF9: //Medarot1 Name
			{
				u16 curCharacterPos = current_string->word6++;
				const int maxLen = 8;
				u8 curCharacter = *(u8 *)(curCharacterPos + maxLen * (character - 0xF8) + 0x3001910);
				if ( ((s16)(curCharacterPos + 1) > maxLen) || !curCharacter )
				{
					current_string->word6 = 0;
					return 0;
				}
				u8 *dest = (u8 *)(((current_string->info.bytes.tile & 0x3F) * 0x40) + 0x6000B40);
				DrawCharacter(dest, curCharacter, 0x1111u, v30018F0);  // Used for F9 (Medarot name)
				++current_string->info.bytes.tile;
				--current_string->string_pointer;
				return 0;
			}
			default:
			{
				current_string->word6 = 0;
				// Use upper bits of v1->current_character to store tile number for use here instead of current_character this well let us make a VWF.
			#ifdef VWF
				if(current_string->info.bytes.english)
					DrawCharacterVWF((u32 *)(((current_string->info.bytes.tile & 0x3F) * 0x40) + 0x6000B40), character, 0x1111u, v30018F0, current_string);
				else
					DrawCharacter((u8 *)(((current_string->info.bytes.tile++ & 0x3F) * 0x40) + 0x6000B40), character, 0x1111u, v30018F0);
			#else
				DrawCharacter((u8 *)(((current_string->info.bytes.tile++ & 0x3F) * 0x40) + 0x6000B40), character, 0x1111u, v30018F0);
			#endif
				
				//extra handling here? what is this for exactly?
				u8 curCharacter = *current_string->string_pointer;
				if ( curCharacter > 0xF2 )
				{
					if ( curCharacter > 0xFA || curCharacter < 0xF8 )
					{
						return 0;
					}
				}
				else
				{
					if ( curCharacter >= 0xF1 )
					{
						current_string->delay = 8;
						return 0;
					}
					if ( !*current_string->string_pointer )
					{
						current_string->delay = 30;
						return 0;
					}
					if ( curCharacter > 0xEB || curCharacter < 0xE8 )
					{
						return 0;
					}
				}
				current_string->word6 = 0;
				return 0;
			}
		}
	}
	--current_string->delay;
	return 0;
}

#ifdef VWF
void DrawCharacterVWF(u32 *destination, u16 character, u16 fg_color, u16 bg_color, struct_string *current_string)
{
	//TODO: Support the other special cases like heart symbol and maybe kanji?
	u32 empty_bg = (bg_color << 16) | bg_color;
	u32 empty_fg = (fg_color << 16) | fg_color;
	const int font_padding_top = 4;
	const int font_height = 10;
	const int font_padding_bottom = 16-font_height-font_padding_top;
	u8 *src = (u8*)&eng_font_bin[font_height * character];
	u32 *dest2 = (u32*)(destination+0x10);
	
	//TODO: Make a Lookup Table for this
	//const int curWidth = 8;
	const int curWidth = eng_font_widths_bin[character];
	
	for(int j=0; j<font_padding_top; j++)
		*destination++ = empty_bg;
	
	if((current_string->info.bytes.overflow+curWidth) >= 8)
	{
		for(int i=0; i<16; i++)
		{
			*dest2++ = empty_bg;
		}
	}
	
	//special case vwf stuff
	for(int i=0; i<font_height; i++)
	{
		u32 temp1 = ConversionLUT[*src >> 4] | (ConversionLUT[*src & 0xF]<<16);
		u32 temp2 = (empty_fg & temp1) | (empty_bg & ~temp1); //final row of 4bpp
		
		//VWF START
		//NOTE: this doesn't trim any unused space in the source so it copies the entire 8 pixels so make sure any unused pixels are set as bg palette
		u32 shift_amount1 = (4*current_string->info.bytes.overflow);
		u32 shift_amount2 = 0x20 - shift_amount1;
		
		*destination <<= shift_amount2;
		*destination >>= shift_amount2;
		*destination |= temp2 << shift_amount1;
		
		u32 *spill_dest = (u32*)(destination+0x10);
		*spill_dest >>= shift_amount1;
		*spill_dest <<= shift_amount1;
		*spill_dest |= temp2 >> shift_amount2;
		//VWF END
		
		destination++;
		++src;
	}
	
	for(int j=0; j<font_padding_bottom; j++)
		*destination++ = empty_bg;
	
	current_string->info.bytes.overflow += curWidth;
	
	if(current_string->info.bytes.overflow >= 8)
	{
		current_string->info.bytes.tile++;
		current_string->info.bytes.overflow -= 8;
	}
}
#endif
