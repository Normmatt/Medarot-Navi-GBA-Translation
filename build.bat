copy rom\input.gba rom\output.gba
tools\grit\grit.exe ".\gfx\menu\007F14E8_eng.png" -gt -gB4 -m -mRtpf -mLf -ftb -fh! -o ".\gfx\menu\raw\007F14E8_eng"
tools\GfxCompressor.exe ".\gfx\menu\raw\007F14E8_eng.img.bin" ".\asm\bin\gfx\007F14E8_eng.cmp.bin"
copy .\gfx\menu\raw\007F14E8_eng.map.bin .\asm\bin\gfx\007F14E8_eng.map.bin

cd ./asm/c_replacements/
call domake.bat
cd ./../../
armips.exe asm/Cheats.asm
armips.exe asm/DrawCharacterC.asm
armips.exe asm/gfx.asm
armips.exe asm/script/script.asm
pause