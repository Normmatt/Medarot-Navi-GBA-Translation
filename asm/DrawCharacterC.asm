 ; Medarot Navi 8x16 font hack by Normmatt and Spikeman

.gba				; Set the architecture to GBA
.open "rom/output.gba",0x08000000		; Open input.gba for output.
					; 0x08000000 will be used as the
					; header size

.thumb

.org 0x08006F9C
DrawString_hook:
    ldr r1, =DrawString+1    ; r1-r3 are safe to use for jump
    bx r1

.align 4
.pool

.org 0x087F8000 ; should be free space to put code
.definelabel v20183E8, 0x020183E8
.definelabel current_music_track, 0x020183E8

.definelabel v201B366, 0x0201B366
.definelabel v30014C4, 0x030014C4
.definelabel v300159C, 0x0300159C
.definelabel v30018F0, 0x030018F0
.definelabel v300190C, 0x0300190C
.definelabel v300192C, 0x0300192C
.definelabel byte_862A01C, 0x0862A01C
.definelabel byte_862CC3C, 0x0862CC3C

.definelabel SetCharacterPortrait, 0x080023D5
.definelabel sub_8006F1C, 0x08006F1D
.definelabel RestartMusic, 0x080005B1
.definelabel PlaySoundEffect, 0x0806AC4D
.definelabel DrawCharacter, 0x0804BE0D
.definelabel ClearTiles, 0x0804C5E1
.definelabel sub_8001EC4, 0x08001EC5
.definelabel sub_8086598, 0x08086599
.definelabel sub_806B504, 0x0806B505

.align 4
.importlib "asm/c_replacements/lib/libc_replacements.a"

.close

 ; make sure to leave an empty line at the end
