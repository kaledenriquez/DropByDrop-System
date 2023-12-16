import hashlib

def getPasswordHash(contraseña):
    contraseña_bytes = contraseña.encode('utf-8')
    hash_obj = hashlib.sha256()
    hash_obj.update(contraseña_bytes)
    hash_resultado = hash_obj.hexdigest()
    return hash_resultado

contraseña = "imaxinfjakdlffjñkalsjfkñalwefñaewjlñfkjwñip"
hash_generado = getPasswordHash(contraseña)

print("Contraseña original:", contraseña)
print("Hash generado:", hash_generado)
