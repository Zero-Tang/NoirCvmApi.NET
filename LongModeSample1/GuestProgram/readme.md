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
`rsp` is initialized to `0xFFFFF800001FFFF0`. \
`rip` is initialized to `0xFFFF800000000000`. \
`GDT.base` is initialized to `0xFFFFF80000000000`.

# Virtual Hardware Specification
This section defines the specification of the virtual hardware.

## Console I/O
Port #0 is standard input, Port #1 is standard output and Port #2 is standard error. \
A repeating string output instruction to the console will automatically generate a line feed. \
Port #3 is an 8-bit stdio basic control register.

| Bit Position | Operation | Details |
|---|---|---|
| 0 | R/W	| Data is ready in `stdin`. |
| 1 | R/W	| `stdio` operates in loop-back mode. |
| 2 | RO	| `stderr` is available to use. |
| 3 | R/W	| If set, issue an interrupt if `stdin` has new data. |
| 4 | WO    | Clear the console. |

Port #4 specifies the vector to which `stdio` would issue interrupts. \
Port #6 specifies the color of the `stdout`.

| Bit Position	| Operation	| Details	|
|---|---|---|
| 0-3	| R/W	| Foreground Color. Default Value: 7	|
| 4-7	| R/W	| Background Color. Default Value: 0	|

Color Specification of Text Console:

| Decimal	| Hexadecimal	| Definition	|
|---|---|---|
| 00	| 0x0	| <span style="color:black">Black</span> |
| 01	| 0x1	| <span style="color:blue">Blue</span>	|
| 02	| 0x2	| <span style="color:green">Green</span>	|
| 03	| 0x3	| <span style="color:cyan">Cyan</span>	|
| 04	| 0x4	| <span style="color:red">Red</span>	|
| 05	| 0x5	| <span style="color:magenta">Magenta</span>	|
| 06	| 0x6	| <span style="color:brown">Brown</span>	|
| 07	| 0x7	| <span style="color:lightgray">Light Gray</span>	|
| 08	| 0x8	| <span style="color:darkgray">Dark Gray</span>	|
| 09	| 0x9	| <span style="color:lightblue">Light Blue</span>	|
| 10	| 0xA	| <span style="color:lightgreen">Light Green</span>	|
| 11	| 0xB	| <span style="color:lightcyan">Light Cyan</span>	|
| 12	| 0xC	| <span style="color:lightcoral">Light Red</span>	|
| 13	| 0xD	| <span style="color:violet">Light Magenta</span>	|
| 14	| 0xE	| <span style="color:yellow">Yellow</span>	|
| 15	| 0xF	| <span style="color:white">White</span>	|

## Power Management
Port #5 is an 8-bit guest power command register.

| Bit Position | Operation | Details |
|---|---|---|
| 0	| WO	| Shutdown if set.	|
| 1	| WO	| Reset if set. |
| 2 | WO	| Trigger SMI. |

# Build
In order to build the guest programs, you should install [NASM](https://www.nasm.us/pub/nasm/releasebuilds/2.15.05/win64/). \
Run the `build.bat` script file to build the files.
