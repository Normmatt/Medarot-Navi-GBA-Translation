OUTPUT_FORMAT("elf32-littlearm", "elf32-littlearm", "elf32-littlearm")
OUTPUT_ARCH(arm)


SECTIONS {
	.text : {
		
		. = ALIGN(4);
		FILL (0x1234)
		
		. = ALIGN(4);
		__code_start = . ;
		*(.init)
		*(.text)
		*(.ctors)
		*(.dtors)
		*(.rodata)
		*(.fini)
		*(COMMON)
		__code_end  = . ;

		__bss_start = . ;
		*(.bss)
		__bss_end = . ;
	_end = __bss_end ;
	__end__ = __bss_end ;
	
	. = ALIGN(4);
	} 
}


