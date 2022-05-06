# Long Mode Sample: Guest Program
This folder contain assembly code for Guest Program that runs in long mode under NoirVisor CVM.

# Files
The `build.bat` file compiles all source files into binary files. \
The `paging.asm` file defines the initial paging structures to support the guest to run in Long Mode. \
The `segment.asm` file defines the initial segmentation structures to support the guest to run in Long Mode. \
The `program.asm` file defines the program to be executed in the Guest.

# Physical Memory Layout
This demo would allocate 8MiB for the guest.
- `0x000000-0x3FFFFF` is intended for program.
- `0x400000-0x5FFFFF` is intended for paging.
- `0x600000-0x7FFFFF` is intended for segmentation.

# Virtual Memory Layout
Guest memory is divided into four 2MiB large pages. 
- Address range `0xFFFF800000000000-0xFFFF800003FFFFFF` will be mapped to `0x000000-0x400000`. This range is intended for program. 
- Address range `0xFFFFF68000000000-0xFFFFF68001FFFFFF` will be mapped to `0x400000-0x5FFFFF`. This range is intended for paging. 
- Address range `0xFFFFF80000000000-0xFFFFF80001FFFFFF` will be mapped to `0x600000-0x7FFFFF`. This range is intended for segmentation and stack. 

Initial stack pointer is `0xFFFFF800001FFFF0`.

# Initial State
IDT is not initialized by hypervisor. \
GPRs, excluding `rsp`, are not initialized by hypervisor. \
`rsp` is initialized to `0xFFFFF80001FFFFF0`. \
`rip` is initialized to `0xFFFF800000000000`. \
`GDT.base` is initialized to `0xFFFFF80000000000`.

# Virtual Hardware Specification
This section defines the specification of the virtual hardware.

## Console I/O
Port #0 is standard input, Port #1 is standard output and Port #2 is standard error. \
Port #3 is an 8-bit stdio basic control register.

| Bit Position | Operation | Details |
|---|---|---|
| 0 | RO	| Data is ready in `stdin`. |
| 1 | R/W	| `stdio` operates in loop-back mode. |
| 2 | RO	| `stderr` is available to use. |
| 3 | R/W	| Interrupt if `stdin` has new data. |

Port #4 specifies the vector to which `stdio` would issue interrupts.

## Power Management
Port #5 is an 8-bit guest power command register.

| Bit Position | Operation | Details |
|---|---|---|
| 0	| WO	| Shutdown if set.	|
| 1	| WO	| Reset if set. |
| 2 | WO	| Trigger SMI. |