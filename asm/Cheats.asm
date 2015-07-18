 ; Medarot Navi Cheats

.gba				; Set the architecture to GBA
.open "rom/output.gba",0x08000000		; Open input.gba for output.
					; 0x08000000 will be used as the
					; header size

.thumb
.org 0x08039510
	ldr r2, =HandleDamage+1	; r2 is best variable to use for jump
	bx r2
.align 4
.pool

.org 0x087F7000
HandleDamage:
	CMP		R0, #2 ; Part number (right arm)
	BEQ		HandleDamage_Max

HandleDamage_Original:
	MOV		R2, SP
	LDRH	R3, [R2,#0x14] ; Load damage amount
	B		HandleDamage_End

HandleDamage_Max:
	MOV		R3, #0xFF

HandleDamage_End:
	MOV		R2, R10
	STRH	R3, [R2,#6] ; Store damage amount
	ADD		R0, R6, #0
	ADD		SP, #0x28
	POP		{R3-R5}
	MOV		R8, R3
	MOV		R9, R4
	MOV		R10, R5
	POP		{R4-R7}
	POP		{R1}
	BX		R1 

.pool
.close

 ; make sure to leave an empty line at the end
