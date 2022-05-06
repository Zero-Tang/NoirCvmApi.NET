bits 64
org 0xFFFF800000000000

%define stdin_port		0
%define stdout_port		1
%define stderr_port		2
%define stdio_ctrl		3
%define stdio_intv		4
%define power_mgmt		5

section .code
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
	; Shutdown the guest.
	mov al,1
	out power_mgmt,al
	; The code won't go further.

; The pointer to the string should be put into rsi register.
; Limit of the string length should be put into rcx register.
; Return value is on rax
strlen:
	push rsi
	push rdx
	push rcx
	xor edx,edx
@1:
	lodsb
	inc rdx
	test al,al
	loopnz @1
	mov rax,rdx
	pop rcx
	pop rdx
	pop rsi
	ret

section .data
hello_world_text:
db "Hello World from 64-bit Long Mode for NoirVisor CVM!\n\0"