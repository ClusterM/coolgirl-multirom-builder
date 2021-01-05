  ; cursor moving sound
bleep:
  ; enable channel
  lda #%00000001
  sta APUSTATUS
  ; square 1
  lda #%10000111
  sta SQ1VOL
  ; sweep
  lda #%10001001
  sta SQ1SWEEP
  lda #%11110000
  ; timer
  sta SQ1LO
  ; length counter and timer
  lda #%00001000
  sta SQ1HI
  rts

  ; beep sound
beep:
  ; enable channel
  lda #%00000100
  sta APUSTATUS
  ; triangle
  lda #%01000000
  sta TRILINEAR
  ; timer
  lda #%1000000
  sta TRILO
  ; length counter and timer
  lda #%00001000
  sta TRIHI
  rts

  ; error sound
error_sound:
  ; enable channel
  lda #%00000100
  sta APUSTATUS
  ; triangle
  lda #%01001111
  sta TRILINEAR
  ; timer
  lda #%00000000
  sta TRILO
  ; length counter and timer
  lda #%11110011
  sta TRIHI
  rts

  ; game start sound
start_sound:
  lda <KONAMI_CODE_STATE
  cmp konami_code_length
  beq start_sound_alt

  ;enable channel
  lda #%00000001
  sta APUSTATUS
  ;square 1
  lda #%00011111
  sta SQ1VOL
  ; sweep
  lda #%10011010
  sta SQ1SWEEP
  ; timer
  lda #%11111111
  sta SQ1LO
  ; length counter and timer
  lda #%10010000
  sta SQ1HI
  rts

  ; Konami Code sound
start_sound_alt:
  ; enable channel
  lda #%00000001
  sta APUSTATUS
  ; square 1
  lda #%10011111
  sta SQ1VOL
  ; sweep
  lda #%10000011
  sta SQ1SWEEP
  ; timer
  lda #%00100000
  sta SQ1LO
  ; length counter and timer
  lda #%11000000
  sta SQ1HI
  rts

wait_sound_end:
  lda APUSTATUS
  bne wait_sound_end
  rts

reset_sound:
  lda #0
  sta SQ1_VOL
  sta SQ1_SWEEP
  sta SQ1_LO
  sta SQ1_HI
  sta SQ2_VOL
  sta SQ2_SWEEP
  sta SQ2_LO
  sta SQ2_HI
  sta TRILINEAR
  sta TRI_LO
  sta TRI_HI
  sta NOISE_VOL
  sta NOISE_LO
  sta NOISE_HI
  sta DMC_FREQ
  sta DMC_RAW
  sta DMC_START
  sta DMC_LEN
  ldx #$40
  stx JOY2_FRAME ; disable APU frame IRQ
  rts
