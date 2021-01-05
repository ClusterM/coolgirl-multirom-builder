PRG_SUPERBANK .rs 2 ; PRG superbank
PRG_BANK .rs 1 ; PRG_A BANK
CHR_BANK .rs 1 ; CHR_A BANK
PRG_RAM_BANK .rs 1 ; PRG RAM BANK
CART_CONFIG .rs 1 ; variable to store last config
PRG_RAM_BANKS .equ 4 ; number of PRG RAM banks
BANKS_TMP .rs 1

banking_init:
  ; set mirrong, disabe CHR writing, PRG-RAM and flash writing
  lda #0
  sta <PRG_SUPERBANK
  sta <PRG_SUPERBANK+1
  sta <PRG_BANK
  sta <CHR_BANK
  sta <PRG_RAM_BANK
  jsr sync_banks
  ; mirroring
  lda #%00001000
  ; store config
  sta <CART_CONFIG
  sta $5007
  rts

  ; select 16KB PRG bank at $8000-$BFFF (from A)
select_prg_bank:
  sta <PRG_BANK
  jsr sync_banks
  rts

  ; select 8KB CHR bank (from A)
select_chr_bank:
  sta <CHR_BANK
  jsr sync_banks
  rts

  ; select 8KB FRAM bank (from A)
select_prg_ram_bank:
  sta <PRG_RAM_BANK
  jsr sync_banks
  rts

  ; actual bank selection
sync_banks:
  pha
  lda <PRG_SUPERBANK
  sta $5001
  lda <PRG_SUPERBANK+1
  sta $5000
  lda CHR_BANK
  pha
  and #%00011111
  sta $5003
  pla
  asl A
  asl A
  and #%10000000
  sta <BANKS_TMP
  lda <PRG_BANK
  asl A
  asl A
  and #%01111100
  ora <BANKS_TMP
  sta <BANKS_TMP
  lda <PRG_RAM_BANK
  and #%00000011
  ora <BANKS_TMP
  sta $5005
  ; for UNROM compatibility
  txa
  pha
  lda <PRG_BANK
  tax
  sta unrom_bank_data, x
  pla
  tax
  pla
  rts

enable_prg_ram:
  lda <CART_CONFIG
  ora #%00000001
  sta <CART_CONFIG
  sta $5007
  rts

disable_prg_ram:
  lda <CART_CONFIG
  and #%11111110
  sta <CART_CONFIG
  sta $5007
  rts

enable_chr_write:
  lda <CART_CONFIG
  ora #%00000010
  sta <CART_CONFIG
  sta $5007
  rts

disable_chr_write:
  lda <CART_CONFIG
  and #%11111101
  sta <CART_CONFIG
  sta $5007
  rts

enable_flash_write:
  lda <CART_CONFIG
  ora #%00000100
  sta <CART_CONFIG
  sta $5007
  rts

disable_flash_write:
  lda <CART_CONFIG
  and #%11111011
  sta <CART_CONFIG
  sta $5007
  rts

enable_four_screen:
  lda <CART_CONFIG
  ora #%00100000
  sta <CART_CONFIG
  sta $5007
  rts

disable_four_screen:
  lda <CART_CONFIG
  and #%11011111
  sta <CART_CONFIG
  sta $5007
  rts
