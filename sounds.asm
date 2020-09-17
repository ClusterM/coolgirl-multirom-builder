  ; cursor moving sound
bleep:
  ; enable channel
  lda #%00000001
  sta $4015
  ; square 1
  lda #%10000111
  sta $4000
  ; sweep   
  lda #%10001001
  sta $4001
  lda #%11110000
  ; timer
  sta $4002
  ; length counter and timer
  lda #%00001000
  sta $4003
  rts

  ; beep sound
beep:
  ; enable channel
  lda #%00000100
  sta $4015
  ; triangle
  lda #%01000000
  sta $4008
  ; timer
  lda #%1000000
  sta $400A
  ; length counter and timer
  lda #%00001000
  sta $400B
  rts

  ; error sound
error_sound:
  ; enable channel
  lda #%00000100
  sta $4015
  ; triangle
  lda #%01001111
  sta $4008
  ; timer
  lda #%00000000
  sta $400A
  ; length counter and timer
  lda #%11110011
  sta $400B
  rts
  
  ; game start sound
start_sound:
  lda <KONAMI_CODE_STATE
  cmp konami_code_length
  beq start_sound_alt

  ;enable channel
  lda #%00000001
  sta $4015
  ;square 1
  lda #%00011111
  sta $4000
  ; sweep 
  lda #%10011010
  sta $4001
  ; timer
  lda #%11111111
  sta $4002
  ; length counter and timer
  lda #%10010000
  sta $4003
  rts
  
  ; Konami Code sound
start_sound_alt:
  ; enable channel
  lda #%00000001
  sta $4015
  ; square 1
  lda #%10011111  
  sta $4000
  ; sweep 
  lda #%10000011
  sta $4001
  ; timer
  lda #%00100000
  sta $4002
  ; length counter and timer
  lda #%11000000
  sta $4003
  rts

wait_sound_end:
  lda $4015
  bne wait_sound_end
  rts

reset_sound:
  lda #0
  sta $4000
  sta $4001
  sta $4002
  sta $4003
  sta $4004
  sta $4005
  sta $4006
  sta $4007
  sta $4008
  sta $4009
  sta $400A
  sta $4010
  sta $4011
  sta $4012
  sta $4013
  ldx #$40
  stx $4017 ; disable APU frame IRQ
  rts
  