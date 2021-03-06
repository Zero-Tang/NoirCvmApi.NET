bits 64
org 0xFFFFF80000000000

gdt_base:
; 0x00 is null selector
dq 0,0
; 0x10 is kernel code selector
dq 0x00209b0000000000
; 0x18 is kernel data selector
dq 0x00cf93000000ffff
; 0x20 is task register selector
dw 0x0067,0x0200
db 0x00,0x8b,0x00,0x00
dd 0xfffff800,0x00000000
times 512-($-$$) db 0
tss_base:
; Reserved
dd 0
; Rsp0-Rsp2
dq 0,0,0
; Reserved
dq 0
; IST1-IST7
dq 0,0,0,0,0,0,0
; Reserved
dq 0
dw 0
; I/O Map Base Address
dw 0

times 1024-($-$$) db 0