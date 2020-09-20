; Games database

  .bank 0
  .org $8000
loader_data_reg_0:
  .db $00, $00, $00, $00, $00, $00

  .bank 0
  .org $8100
loader_data_reg_1:
  .db $08, $0A, $0C, $0E, $0F, $10

  .bank 0
  .org $8200
loader_data_reg_2:
  .db $7E, $7E, $7E, $7F, $7F, $7F

  .bank 0
  .org $8300
loader_data_reg_3:
  .db $00, $00, $00, $80, $00, $00

  .bank 0
  .org $8400
loader_data_reg_4:
  .db $1F, $1F, $1F, $5F, $1F, $1F

  .bank 0
  .org $8500
loader_data_reg_5:
  .db $01, $01, $01, $01, $01, $01

  .bank 0
  .org $8600
loader_data_reg_6:
  .db $00, $00, $00, $14, $00, $00

  .bank 0
  .org $8700
loader_data_reg_7:
  .db $80, $80, $88, $89, $88, $A0

  .bank 0
  .org $8800
loader_data_chr_start_bank_h:
  .db $00, $00, $00, $00, $00, $00

  .bank 0
  .org $8900
loader_data_chr_start_bank_l:
  .db $10, $10, $12, $12, $12, $12

  .bank 0
  .org $8A00
loader_data_chr_start_bank_s:
  .db $C0, $E0, $80, $A0, $C0, $E0

  .bank 0
  .org $8B00
loader_data_chr_count:
  .db $01, $01, $01, $01, $01, $01

  .bank 0
  .org $8C00
loader_data_game_save:
  .db $00, $00, $00, $00, $00, $00

  .bank 0
  .org $8D00
loader_data_game_type:
  .db $00, $00, $00, $00, $00, $08

  .bank 0
  .org $8E00
loader_data_cursor_pos:
  .db $11, $09, $11, $01, $01, $01



  .bank 0
  .org $9000
game_names_list:
  .dw game_names
game_names:
  .dw game_name_0
  .dw game_name_1
  .dw game_name_2
  .dw game_name_3
  .dw game_name_4
  .dw game_name_5


; Game names

  .bank 1
  .org $A000
; AFTER DARK (DEMO)
game_name_0:
  .db $81, $86, $94, $85, $92, $B0, $84, $81, $92, $8B, $B0, $AB, $84, $85, $8D
  .db $8F, $AC, $00

; Alter Ego
game_name_1:
  .db $81, $8C, $94, $85, $92, $B0, $85, $87, $8F, $00

; Zooming Secretary
game_name_2:
  .db $9A, $8F, $8F, $8D, $89, $8E, $87, $B0, $93, $85, $83, $92, $85, $94, $81
  .db $92, $99, $00

; ?
game_name_3:
  .db $A6, $00

; ?
game_name_4:
  .db $A6, $00

; ?
game_name_5:
  .db $A6, $00

  .bank 14
  .org $C800

games_count:
  .dw 3


games_offset:
  .db 4


maximum_scroll:
  .dw 0


string_file:
  .db $86, $89, $8C, $85, $A7, $B0, $87, $81, $8D, $85, $93, $A4, $8C, $89, $93
  .db $94, $00
string_build_date:
  .db $82, $95, $89, $8C, $84, $B0, $84, $81, $94, $85, $A7, $B0, $9C, $8F, $9C
  .db $8F, $A8, $8F, $A3, $A8, $9C, $9B, $00
string_build_time:
  .db $82, $95, $89, $8C, $84, $B0, $94, $89, $8D, $85, $A7, $B0, $8F, $8F, $A7
  .db $9B, $9C, $A7, $8F, $9E, $00
string_console_type:
  .db $83, $8F, $8E, $93, $8F, $8C, $85, $B0, $94, $99, $90, $85, $A7, $00
string_ntsc:
  .db $8E, $94, $93, $83, $00
string_pal:
  .db $90, $81, $8C, $00
string_dendy:
  .db $84, $85, $8E, $84, $99, $00
string_new:
  .db $8E, $85, $97, $00
string_flash:
  .db $86, $8C, $81, $93, $88, $A7, $00
string_read_only:
  .db $92, $85, $81, $84, $B0, $8F, $8E, $8C, $99, $00
string_writable:
  .db $97, $92, $89, $94, $81, $82, $8C, $85, $00
flash_sizes:
  .dw string_1mb
  .dw string_2mb
  .dw string_4mb
  .dw string_8mb
  .dw string_16mb
  .dw string_32mb
  .dw string_64mb
  .dw string_128mb
  .dw string_256mb
string_1mb:
  .db $9B, $8D, $82, $00
string_2mb:
  .db $9C, $8D, $82, $00
string_4mb:
  .db $9E, $8D, $82, $00
string_8mb:
  .db $A2, $8D, $82, $00
string_16mb:
  .db $9B, $A0, $8D, $82, $00
string_32mb:
  .db $9D, $9C, $8D, $82, $00
string_64mb:
  .db $A0, $9E, $8D, $82, $00
string_128mb:
  .db $9B, $9C, $A2, $8D, $82, $00
string_256mb:
  .db $9C, $9F, $A0, $8D, $82, $00
string_chr_ram:
  .db $83, $88, $92, $B0, $92, $81, $8D, $A7, $00
chr_ram_sizes:
  .dw string_8kb
  .dw string_16kb
  .dw string_32kb
  .dw string_64kb
  .dw string_128kb
  .dw string_256kb
  .dw string_512kb
  .dw string_1024kb
  .dw string_2048kb
string_8kb:
  .db $A2, $8B, $82, $00
string_16kb:
  .db $9B, $A0, $8B, $82, $00
string_32kb:
  .db $9D, $9C, $8B, $82, $00
string_64kb:
  .db $A0, $9E, $8B, $82, $00
string_128kb:
  .db $9B, $9C, $A2, $8B, $82, $00
string_256kb:
  .db $9C, $9F, $A0, $8B, $82, $00
string_512kb:
  .db $9F, $9B, $9C, $8B, $82, $00
string_1024kb:
  .db $9B, $8F, $9C, $9E, $8B, $82, $00
string_2048kb:
  .db $9C, $8F, $9E, $A2, $8B, $82, $00
string_prg_ram:
  .db $90, $92, $87, $B0, $92, $81, $8D, $A7, $00
string_present:
  .db $90, $92, $85, $93, $85, $8E, $94, $00
string_not_available:
  .db $8E, $8F, $94, $B0, $81, $96, $81, $89, $8C, $81, $82, $8C, $85, $00
string_saving:
  .db $B0, $B0, $C2, $BF, $C6, $C1, $B1, $BE, $D0, $B6, $BD, $C2, $D0, $A4, $A4
  .db $A4, $B0, $BE, $B6, $B0, $B3, $CC, $BB, $BC, $CF, $C8, $B1, $BA, $AA, $B0
  .db $B0, $B0, $00
string_incompatible_console:
  .db $B0, $B0, $B0, $B0, $B0, $B9, $B8, $B3, $B9, $BE, $B9, $C3, $B6, $A5, $B0
  .db $B0, $B5, $B1, $BE, $BE, $B1, $D0, $B0, $B9, $B4, $C1, $B1, $B0, $B0, $B0
  .db $B0, $B0, $B0, $B0, $BE, $B6, $C2, $BF, $B3, $BD, $B6, $C2, $C3, $B9, $BD
  .db $B1, $B0, $C2, $B0, $CE, $C3, $BF, $BA, $B0, $BB, $BF, $BE, $C2, $BF, $BC
  .db $CD, $CF, $B0, $B0, $B0, $B0, $B0, $B0, $B0, $B0, $B0, $B0, $B0, $B0, $B0
  .db $B0, $B0, $B0, $B0, $B0, $B0, $B0, $B0, $B0, $B0, $B0, $B0, $B0, $B0, $B0
  .db $B0, $B0, $B0, $B0, $B0, $B0, $B0, $B0, $B0, $B0, $B0, $B0, $BE, $B1, $B7
  .db $BD, $B9, $C3, $B6, $B0, $BC, $CF, $B2, $C4, $CF, $B0, $BB, $BE, $BF, $C0
  .db $BB, $C4, $B0, $B0, $B0, $B0, $B0, $B0, $00
string_prg_ram_test:
  .db $90, $92, $87, $B0, $92, $81, $8D, $B0, $94, $85, $93, $94, $A7, $00
string_chr_ram_test:
  .db $83, $88, $92, $B0, $92, $81, $8D, $B0, $94, $85, $93, $94, $A7, $00
string_passed:
  .db $90, $81, $93, $93, $85, $84, $00
string_failed:
  .db $86, $81, $89, $8C, $85, $84, $00
string_ok:
  .db $8F, $8B, $00
string_error:
  .db $85, $92, $92, $8F, $92, $00


SECRETS .equ 3
