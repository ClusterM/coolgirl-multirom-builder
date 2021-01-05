  ; starting game!
start_game:
  ; disable PPU
  lda #%00000000
  sta PPUCTRL
  lda #%00000000
  sta PPUMASK
  ; wait for v-blank
  jsr waitblank_simple

  .if SECRETS>=3
  ; check for konami code
  lda <KONAMI_CODE_STATE
  cmp konami_code_length
  bne .no_konami_code
  lda #GAMES_COUNT & $FF
  clc
  adc #2
  sta <SELECTED_GAME
  lda #(GAMES_COUNT >> 8) & $FF
  adc #0
  sta <SELECTED_GAME+1
.no_konami_code:
  .endif

  lda SELECTED_GAME+1
  jsr select_prg_bank
  ldx SELECTED_GAME
  lda loader_data_game_flags, x
  and CONSOLE_TYPE
  beq compatible_console

  ; not compatible console!
  ; error sound
  jsr error_sound
  ; save state, without game save
  lda #0
  sta <LAST_STARTED_SAVE
  jsr save_state
  ; print error message
  jsr clear_screen
  jsr load_text_palette
  lda #$21
  sta PPUADDR
  lda #$A0
  sta PPUADDR
  lda #LOW(string_incompatible_console)
  sta <COPY_SOURCE_ADDR
  lda #HIGH(string_incompatible_console)
  sta <COPY_SOURCE_ADDR+1
  jsr print_text
  bit PPUSTATUS
  lda #0
  sta PPUSCROLL
  sta PPUSCROLL
  lda #%00001000
  sta PPUCTRL
  lda #%00001010
  sta PPUMASK
  ; wait until all buttons released
.incompatible_print_wait_no_button:
  jsr read_controller
  lda <BUTTONS
  bne .incompatible_print_wait_no_button
  ; tiny delay
  ldx #15
.incompatible_wait:
  jsr waitblank_simple
  dex
  bne .incompatible_wait
  ; wait until any button pressed
.incompatible_print_wait_button:
  jsr read_controller
  lda <BUTTONS
  beq .incompatible_print_wait_button
  jmp Start

compatible_console:
  ; clear NTRAM
  jsr enable_chr_write
  jsr enable_four_screen
  jsr clear_screen
  ; clear nametables
  jsr disable_four_screen
  jsr clear_screen
  jsr disable_chr_write
  ; clear sprite data
  jsr clear_sprites
  ; load this empty data
  jsr sprite_dma_copy
  ; load black palette
  jsr load_black
  ; loading game settings
  ldx <SELECTED_GAME
  lda loader_data_reg_0, x
  sta <LOADER_REG_0
  lda loader_data_reg_1, x
  sta <LOADER_REG_1
  lda loader_data_reg_2, x
  sta <LOADER_REG_2
  lda loader_data_reg_3, x
  sta <LOADER_REG_3
  lda loader_data_reg_4, x
  sta <LOADER_REG_4
  lda loader_data_reg_5, x
  sta <LOADER_REG_5
  lda loader_data_reg_6, x
  sta <LOADER_REG_6
  lda loader_data_reg_7, x
  sta <LOADER_REG_7
  lda loader_data_chr_start_bank_h, x
  sta <LOADER_CHR_START_H
  lda loader_data_chr_start_bank_l, x
  sta <LOADER_CHR_START_L
  lda loader_data_chr_start_bank_s, x
  sta <LOADER_CHR_START_S
  lda loader_data_chr_count, x
  sta <LOADER_CHR_LEFT
  lda loader_data_game_save, x
  sta <LOADER_GAME_SAVE
  sta <LAST_STARTED_SAVE ; save ID of save
  lda #2
  sta <LOADER_GAME_SAVE_BANK
  ; loading battery backed save if need
  jsr load_save
  ; saving state
  jsr save_state
  ; first PRG bank
  lda #0
  jsr select_prg_bank
  ; loading tiles
  jsr load_all_chr_banks
  ; wait for sound end and reset sound registers
  jsr wait_sound_end
  jsr reset_sound
  ; start loader stored into RAM
  jmp loader
