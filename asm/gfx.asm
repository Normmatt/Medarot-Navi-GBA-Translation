 ; Medarot Navi - Custom Gfx

.gba				; Set the architecture to GBA
.open "rom/output.gba",0x08000000		; Open input.gba for output.
					; 0x08000000 will be used as the
					; header size

.org 0x0807CC8C
.word _007F14E8

 ; Fix the tilemap for tile in middle of save gfx
.org 0x087F2628
.incbin "asm/bin/gfx/007F14E8_eng.map.bin"

.org 0x08800000 ; should be free space to put code
_007F14E8:
.incbin "asm/bin/gfx/007F14E8_eng.cmp.bin"

.close

 ; make sure to leave an empty line at the end
