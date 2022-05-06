bits 64
org 0x400000
; This file will be compiled into a binary file that includes initial paging structure state.

; The first page will start with the PML4E.
pml4e_base:
times 256 dq 0
dq program_pdpte_base+3
times 236 dq 0
dq page_structure_pdpte_base+3
times 2 dq 0
dq segmentation_pdpte_base+3
times 15 dq 0

; Next, PDPTEs
program_pdpte_base:
dq program_pde_base+3
times 511 dq 0

page_structure_pdpte_base:
dq page_structure_pde_base+3
times 511 dq 0

segmentation_pdpte_base:
dq segmentation_pde_base+3
times 511 dq 0

; Finally, PDEs that maps 2MiB large pages.
program_pde_base:
dq 0x00000083,0x00200083
times 510 dq 0

page_structure_pde_base:
dq 0x00400083
times 511 dq 0

segmentation_pde_base:
dq 0x00600083
times 511 dq 0