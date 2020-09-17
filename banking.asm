select_bank:
  tax
  sta unrom_bank_data, x
  asl A
  asl A
  sta $5005
  rts
