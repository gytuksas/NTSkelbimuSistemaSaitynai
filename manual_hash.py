import binascii, hashlib
password=b'Skutubulves123!'
salt=bytes.fromhex('8f1e6d02c3a1b4e5f6a7b8c9d0e1f2a3')
dk=hashlib.pbkdf2_hmac('sha256', password, salt, 100000, dklen=32)
print('PBKDF2$100000$'+salt.hex()+'$'+dk.hex())