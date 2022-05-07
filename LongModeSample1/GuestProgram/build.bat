@echo off
set path=%ProgramFiles%\NASM;%path%

nasm -v
echo Assembling...
nasm paging.asm -f bin -o .\bin\paging.bin
nasm segment.asm -f bin -o .\bin\segment.bin
nasm program.asm -f bin -o .\bin\program.bin -l .\bin\program.lst

echo Completed!
pause