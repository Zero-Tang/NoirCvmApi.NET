bits 64
org 0xFFFF800000000000

%define stdin_port		0
%define stdout_port		1
%define stderr_port		2
%define stdio_ctrl		3
%define stdio_intv		4
%define power_mgmt		5
%define stdio_color		6

%define stdio_intvector	233

guest_start:
	; Load the string to rsi.
	mov rsi,hello_world_text
	mov ecx,1000	; Let the limit be 1000 bytes.
	; Get the length of the string.
	call strlen
	; Length of string
	mov rcx,rax
	; Port Number
	mov dx,stdout_port
	; Perform output.
	rep outsb
	; Set-up interrupt handler
	mov al,8
	out stdio_ctrl,al
	mov al,stdio_intvector
	out stdio_intv,al
	call setup_idt
	call halting_loop
	; Output what's obtained from the stdin
	; But first, use another color
	mov al,0xE		; Background is black (0x0). Foreground is yellow (0x7).
	out stdio_color,al
	mov rsi,stdin_buffer+8
	mov rcx,[rsi-8]
	mov dx,stdout_port
	rep outsb
	; Change a text color
	mov al,0xF4		; Background is white (0xF). Foreground is red (0x4).
	out stdio_color,al
	; Output to console
	mov rsi,shutdown_text
	mov ecx,1000
	call strlen
	mov rcx,rax
	mov dx,stdout_port
	rep outsb
	; Shutdown the guest.
	mov al,1
	out power_mgmt,al
	; The code won't go further.

setup_idt:
	; Push stack and save registers...
	sub rsp,0x30
	mov [rsp+0x28],rbx
	mov [rsp+0x20],rdx
	mov [rsp+0x18],rcx
	mov [rsp+0x10],rax
	; Locate IDT Base
	sidt [rsp+6]
	mov rbx,[rsp+8]
	; Load Interrupt Handler info
	mov rcx,stdio_intvector*16
	mov rdx,stdin_int_handler
	mov word [rbx+rcx],dx			; Target Offset [15:0]
	mov word [rbx+rcx+2],cs			; Target Code Selector
	mov word [rbx+rcx+4],0x8E00		; Present=1, DPL=0, Type=64-bit Interrupt Gate, IST=0
	shr rdx,16
	mov word [rbx+rcx+6],dx			; Target Offset [31:16]
	shr rdx,16
	mov dword [rbx+rcx+8],edx		; Target Offset [63:32]
	mov dword [rbx+rcx+12],0		; Reserved
	; Pop stack and restore registers...
	mov rax,[rsp+0x10]
	mov rcx,[rsp+0x18]
	mov rdx,[rsp+0x20]
	mov rbx,[rsp+0x28]
	add rsp,0x30
	ret

halting_loop:
	xor eax,eax
	sti
.1:
	hlt
	mov rbx,stdin_buffer
	mov rax,qword [rbx+248]
	; If rax is non-zero then leave loop.
	test rax,rax
	jz .1
	ret

stdin_int_handler:
	; Receive the byte from stdin.
	push rax
	push rbx
	in al,stdin_port
	cmp al,13
	je .1		; If stdin has a CR byte, leave the halt-loop.
	push rcx
	; Load the index
	mov rbx,stdin_buffer
	mov rcx,[rbx]
	mov [rbx+rcx+8],al
	; Increment the index
	inc qword [rbx]
	pop rcx
	jmp .2
.1:
	mov rbx,stdin_buffer
	mov qword [rbx+248],2
.2:
	pop rbx
	pop rax
	iretq

; The pointer to the string should be put into rsi register.
; Limit of the string length should be put into rcx register.
; Return value is on rax
strlen:
	push rsi
	push rdx
	push rcx
	xor edx,edx
.1:
	lodsb
	inc rdx
	test al,al
	loopnz .1
	mov rax,rdx
	pop rcx
	pop rdx
	pop rsi
	ret

hello_world_text:
db "Hello World from 64-bit Long Mode for NoirVisor CVM!",0
shutdown_text:
db "Guest is shutting down...",0
stdin_buffer:
times 256 db 0