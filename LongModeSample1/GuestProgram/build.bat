@echo off
set path=%ProgramFiles%\NASM;%path%

echo Assembling...
nasm paging.asm -f bin -o .\bin\paging.bin
nasm segment.asm -f bin -o .\bin\segment.bin
nasm program.asm -f bin -o .\bin\program.bin

echo Completed!
pause