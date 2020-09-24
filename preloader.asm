  ; starting game!
start_game:
  ; disable PPU
  lda #%00000000
  sta $2000
  lda #%00000000
  sta $2001
  ; wait for v-blank
  jsr waitblank_simple
  
  .if SECRETS>=3
  ; check for konami code
  lda <KONAMI_CODE_STATE
  cmp konami_code_length
  bne .no_konami_code
  lda games_count
  clc
  adc #2
  sta <SELECTED_GAME
  lda games_count+1
  adc #0
  sta <SELECTED_GAME+1
.no_konami_code:
  .endif

  lda SELECTED_GAME+1
  jsr select_prg_bank
  ldx SELECTED_GAME
  lda loader_data_game_type, x
  and CONSOLE_TYPE
  beq compatible_console

  ; not compatible console!
  ; save state, without game save
  lda #0
  sta <LAST_STARTED_SAVE
  jsr save_state
  jsr load_text_palette
  lda #$21
  sta $2006
  lda #$A0
  sta $2006
  lda #LOW(string_incompatible_console)
  sta <COPY_SOURCE_ADDR
  lda #HIGH(string_incompatible_console)
  sta <COPY_SOURCE_ADDR+1
  jsr print_text
  bit $2002
  lda #0
  sta $2005
  sta $2005
  lda #%00001000
  sta $2000
  lda #%00001010
  sta $2001
  jsr waitblank_simple
.incompatible_print_wait_no_button:
  jsr read_controller
  lda <BUTTONS
  bne .incompatible_print_wait_no_button
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
