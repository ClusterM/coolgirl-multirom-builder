 
PRG_BANK .rs 1 ; PRG_A BANK
CHR_BANK .rs 1 ; CHR_A BANK
FRAM_BANK .rs 1 ; FRAM BANK
CART_CONFIG .rs 1 ; variable to store last config

banking_init:
  ; set mirrong, disabe CHR writing, PRG-RAM and flash writing
  lda #0
  sta <PRG_BANK
  sta <CHR_BANK
  sta <FRAM_BANK
  sta $5003
  sta $5005
  lda #%00001000
  ; store config
  sta <CART_CONFIG
  sta $5007
  rts

  ; select 16KB PRG bank at $8000-$BFFF (from A)
select_prg_bank:
  sta PRG_BANK
  jsr sync_banks
  rts

  ; select 8KB CHR bank (from A)
select_chr_bank:
  sta CHR_BANK
  jsr sync_banks
  rts

  ; select 8KB FRAM bank (from A)
select_fram_bank:
  sta FRAM_BANK
  jsr sync_banks
  rts

  ; actual bank selection
sync_banks:
  lda CHR_BANK
  pha
  and #%00011111
  sta $5003
  pla
  asl A
  asl A
  sta TMP
  lda PRG_BANK
  asl A
  asl A
  and #%01111100
  ora TMP
  sta TMP
  lda FRAM_BANK  
  and #%00000011
  ora TMP
  sta $5005
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
