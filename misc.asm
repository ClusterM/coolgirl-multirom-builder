RANDOM .rs 2 ; random numbers
RANDOM_TEMP .rs 1 ; temporary variable for random
CONSOLE_TYPE .rs 1 ; console type

random_init:
  lda #$5A
  sta <RANDOM_TEMP
  lda %10011101
  sta <RANDOM
  lda %01011011
  sta <RANDOM+1
  rts
  
random:
  lda <RANDOM+1
  sta <RANDOM_TEMP
  lda <RANDOM
  asl a
  rol <RANDOM_TEMP
  asl a
  rol <RANDOM_TEMP
  clc
  adc <RANDOM
  pha
  lda <RANDOM_TEMP
  adc <RANDOM+1
  sta <RANDOM+1
  pla
  adc #$11
  sta <RANDOM
  lda <RANDOM+1
  adc #$36
  sta <RANDOM+1
  rts

console_detect:
  jsr waitblank_simple
  ldx #0
  ldy #0
  ; Console type detect
.detect_l:
  inx
  bne .detect_s
  iny
.detect_s:  
  lda $2002
  bpl .detect_l
  lda #$00
  cpy #$08
  bne .not_ntsc1
  ora #$01
.not_ntsc1:
  cpy #$09
  bne .not_ntsc2
  ora #$01
.not_ntsc2:
  cpy #$0A
  bne .not_pal
  ora #$02
.not_pal:
  cpy #$0B
  bne .not_dendy
  ora #$04
.not_dendy:
  ldx $5000
  beq console_detect_not_new_dendy
  ora #$08
console_detect_not_new_dendy:
  sta <CONSOLE_TYPE
  rts
