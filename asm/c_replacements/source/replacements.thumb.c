#include "types.h"
#include "code.h"
#include "rt.h"
#include "code.h"

#include <string.h>
#include <stdlib.h>

typedef struct struct_string
{
    u8 *string_pointer;
    u8 gap4[2];
    u16 word6;
    s16 delay;
    u8 option;
    s8 byteB;
    s32 current_character;
} struct_string;

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
				DrawCharacter((u8 *)(((current_string->current_character++ & 0x3F) * 0x40) + 0x6000B40), kanji, 0x1111u, v30018F0);
				
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
				current_string->current_character = 32; //Set this to second row of tiles.
				return 0;
			}
			case 0xF2: //Wait for key press and end current string
			{
				int temp;
				current_string->current_character = 0;
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
			case 0xF4: //??
			{
				current_string->string_pointer = 0;
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
				u8 *dest = (u8 *)(((current_string->current_character & 0x3F) * 0x40) + 0x6000B40);
				DrawCharacter(dest, curCharacter, 0x1111u, v30018F0);  // Used for F9 (Medarot name)
				++current_string->current_character;
				--current_string->string_pointer;
				return 0;
			}
			default:
			{
				current_string->word6 = 0;
				// Use upper bits of v1->current_character to store tile number for use here instead of current_character this well let us make a VWF.
				DrawCharacter((u8 *)(((current_string->current_character++ & 0x3F) * 0x40) + 0x6000B40), character, 0x1111u, v30018F0);
				
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
