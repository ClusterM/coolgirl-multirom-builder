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
	
	; cursor moving sound
bleep:
	;rts ; выключить звук
	lda #%00000001
	sta $4015
	;square 1
	;lda #%10011111  ; 50% duty, max volume
	lda #$87
	sta $4000
	;lda #%10011010  ; sweep 
	lda #$89
	sta $4001
	;lda #40
	lda #$F0
	sta $4002
	lda #%00000000
	sta $4003
	rts

  ; short bleep sound
bleep_short:
	lda #%00000100
	sta $4015
	lda #$40
	sta $4008
	lda #$80
	sta $400A
	lda #$00
	sta $400B
	rts

  ; error sound
error_sound:
	lda #%00000100
	sta $4015
	lda #$4F
	sta $4008
	lda #$00
	sta $400A
	lda #$F3
	sta $400B
	rts
	
	; game start sound
start_sound:
	lda <KONAMI_CODE_STATE
	cmp konami_code_length
	beq start_sound_alt

	lda #%00000001
	sta $4015 ;enable channel(s)	
	;square 1
	lda #%00111111
	sta $4000
	;lda #%10100010  ; sweep 
	lda #$9A
	sta $4001
	;lda #20
	lda #$FF
	sta $4002
	;lda #%11111000
	lda #$00
	sta $4003
	rts
	
	; Konami Code sound
start_sound_alt:
	lda #%00000001
	sta $4015 ;enable channel(s)	
	;square 1
	lda #%10011111  ; 50% duty, max volume
	sta $4000
	lda #%10000011  ; sweep 
	sta $4001
	lda #20
	sta $4002
	lda #%11000000
	sta $4003
	rts
