from flask import Blueprint, jsonify, request
import mysql.connector
import random
import string
import hashlib

users = Blueprint('users', __name__)

# Route to verify api connection
@users.route('/test', methods=['GET'])
def test():
    return jsonify({'response': 'Users Blueprint is working'}), 200

# Route to create a new user
@users.route('/createAccount', methods=['POST'])
def createAccount():
    data = request.get_json()
    print(data)
    db_name = data['db_name']
    userDB = data['user']
    passwordDB = data['password']
    appPassword = data['appPassword']
    appUser = data['appUser']

    if db_name is None:
        return jsonify({'error': 'No database name provided',
                        'response': 'error'}), 400
    else:
        userHash = createUserHash()
        try:
            mydb = mysql.connector.connect(
                host="localhost",
                user=userDB,
                password=passwordDB,
            )
            if mydb.is_connected():
                mycursor = mydb.cursor()
                mycursor.execute("CREATE DATABASE IF NOT EXISTS " + db_name)
                mycursor.execute("USE " + db_name)
        except:
            print("Error en las credenciales de la base de datos")
            return jsonify({'error': 'Connection failed',
                            'response': 'error'}), 400
        
        if mydb.is_connected():
            mycursor = mydb.cursor()
            try:
                passwordHash = getPasswordHash(appPassword)
                mycursor.execute("CREATE TABLE IF NOT EXISTS Usuarios (UserHash VARCHAR(25) PRIMARY KEY, Usuario VARCHAR(25), Contraseña VARCHAR(100))")
                # validar que no exista el usuario
                mycursor.execute("SELECT * FROM Usuarios WHERE Usuario = %s", (appUser,))
                result = mycursor.fetchone()
                if result is not None:
                    
                    return jsonify({'error': 'User already exists',
                                    'response': 'error'}), 400
                mycursor.execute("INSERT INTO Usuarios (UserHash, Usuario, Contraseña) VALUES (%s, %s, %s)", (userHash, appUser, passwordHash))
                # de la tabla configuración, insertar valores por defecto para el usuario, el id de la configuración es automático, solo insertar los otros
                mycursor.execute("INSERT INTO Configuracion (ID_Sensor, valorMin, valorMax, UserHash) VALUES (%s, %s, %s, %s)", ("1", "150", "300", userHash))
                mycursor.execute("INSERT INTO Configuracion (ID_Sensor, valorMin, valorMax, UserHash) VALUES (%s, %s, %s, %s)", ("2", "30", "60", userHash))
                mycursor.execute("INSERT INTO Configuracion (ID_Sensor, valorMin, valorMax, UserHash) VALUES (%s, %s, %s, %s)", ("3", "30", "50", userHash))
                mycursor.execute("INSERT INTO Configuracion (ID_Sensor, valorMin, valorMax, UserHash) VALUES (%s, %s, %s, %s)", ("4", "0", "150", userHash))
                mycursor.execute("INSERT INTO AsistenteVirtual (nombreAsistente, UserHash) VALUES (%s, %s)", ("N/A", userHash))
                mydb.commit()
                return jsonify({'response': 'Account created succesfully'}), 200
            except Exception as e:
                return jsonify({'error': 'Error: ' + str(e),
                                'response': 'error'}), 400
        else:
            return jsonify({'error': 'Connection failed',
                            'response': 'error'}), 400


@users.route('/login', methods=['GET', 'POST'])
def login():
    data = request.get_json()
    db_name = data['db_name']
    userDB = data['user']
    passwordDB = data['password']
    appPassword = data['appPassword']
    appUser = data['appUser']

    if db_name is None:
        return jsonify({'error': 'No database name provided',
                        'response': 'error'}), 400
    else:
        try:
            mydb = mysql.connector.connect(
                host="localhost",
                user=userDB,
                password=passwordDB,
            )
            if mydb.is_connected():
                mycursor = mydb.cursor()
                mycursor.execute("CREATE DATABASE IF NOT EXISTS " + db_name)
                mycursor.execute("USE " + db_name)
        except:
            print("Error en las credenciales de la base de datos")
            return jsonify({'error': 'Connection failed',
                'response': 'error'}), 400
        if mydb.is_connected():
            mycursor = mydb.cursor()
            try:
                passwordHash = getPasswordHash(appPassword)
                print(passwordHash)
                mycursor.execute("CREATE TABLE IF NOT EXISTS Usuarios (UserHash VARCHAR(25) PRIMARY KEY, Usuario VARCHAR(25), Contraseña VARCHAR(100))")
                mycursor.execute("SELECT * FROM Usuarios WHERE Usuario = %s AND Contraseña = %s", (appUser, passwordHash))
                result = mycursor.fetchone()
                if result is None:
                    return jsonify({'error': 'User or password incorrect',
                                    'response': 'error'}), 400
                return jsonify({'response': 'Login succesful',
                                'userHash': result[0]}), 200
            except Exception as e:
                return jsonify({'error': 'Error: ' + str(e),
                                'response': 'error'}), 400
        else:
            return jsonify({'error': 'Connection failed',
                            'response': 'error'}), 400

# Get the userñs configuration from the database
@users.route('/getVariablesFromActiveUser', methods=['POST'])
def getVariablesFromActiveUser():
    print(request)
    data = request.get_json()
    print(data)
    db_name = data['db_name']
    passwordReq = data['password']
    userReq = data['user']
    
    if db_name is None:
        return jsonify({'error': 'No database name provided',
                        'response': 'error'}), 400
    else:
        mydb = mysql.connector.connect(
            host="localhost",
            user=userReq,
            password=passwordReq,
            database=db_name
        )
        if mydb.is_connected():
            mycursor = mydb.cursor()
            try: 
                mycursor.execute("SELECT C.ID_Sensor, C.valorMin, C.valorMax FROM CONFIGURACION as C INNER JOIN USUARIOACTIVO as U ON C.UserHash = U.UserHash WHERE U.ID_Activo = 1")
                myresult = mycursor.fetchall()
                columns = [column[0] for column in mycursor.description]
                print(myresult)
                mycursor.close()
                # Formats the results into a JSON format with the column names
                data = []
                for row in myresult:
                    row_data = {}
                    for i, value in enumerate(row):
                        row_data[columns[i]] = value
                    data.append(row_data)
                # If data is empty, send a table with one element with the name of the columns and empty variables in each one
                if len(data) == 0:
                    data = [dict(zip(columns, ['' for i in range(len(columns))]))]
                
                return jsonify({'response': data}), 200

            except Exception as e:
                return jsonify({'error': 'Error: ' + str(e),
                                'response': 'error'}), 400
        else:
            return jsonify({'error': 'Connection failed',
                            'response': 'error'}), 400
        

@users.route('/getActiveUser', methods=['POST'])
def getActiveUser():
    data = request.get_json()
    db_name = data['db_name']
    userDB = data['user']
    passwordDB = data['password']

    if db_name is None:
        return jsonify({'error': 'No database name provided',
                        'response': 'error'}), 400
    else:
        try:
            mydb = mysql.connector.connect(
                host="localhost",
                user=userDB,
                password=passwordDB,
            )
            if mydb.is_connected():
                mycursor = mydb.cursor()
                mycursor.execute("CREATE DATABASE IF NOT EXISTS " + db_name)
                mycursor.execute("USE " + db_name)
        except:
            print("Error en las credenciales de la base de datos")
            return jsonify({'error': 'Connection failed',
                'response': 'error'}), 400
        if mydb.is_connected():
            mycursor = mydb.cursor()
            try:
                mycursor.execute("CREATE table IF NOT EXISTS usuarioActivo(ID_Activo int auto_increment primary key, UserHash varchar(25), FOREIGN KEY (UserHash) REFERENCES Usuarios(UserHash));")
                mycursor.execute("SELECT UserHash FROM usuarioactivo WHERE ID_Activo = 1;")
                result = mycursor.fetchone()
                if result is None:
                    return jsonify({'response': 'No user active'}), 200
                return jsonify({'response': 'User active found succesfully',
                                'userHash': result[0]}), 200
            except Exception as e:
                return jsonify({'error': 'Error: ' + str(e),
                                'response': 'error'}), 400
        else:
            return jsonify({'error': 'Connection failed',
                            'response': 'error'}), 400

@users.route('/setActiveUser', methods=['POST'])
def setActiveUser():
    data = request.get_json()
    db_name = data['db_name']
    userDB = data['user']
    passwordDB = data['password']
    userHash = data['userHash']

    if db_name is None:
        return jsonify({'error': 'No database name provided',
                        'response': 'error'}), 400
    else:
        try:
            mydb = mysql.connector.connect(
                host="localhost",
                user=userDB,
                password=passwordDB,
            )
            if mydb.is_connected():
                mycursor = mydb.cursor()
                mycursor.execute("CREATE DATABASE IF NOT EXISTS " + db_name)
                mycursor.execute("USE " + db_name)
        except:
            print("Error en las credenciales de la base de datos")
            return jsonify({'error': 'Connection failed',
                'response': 'error'}), 400
        if mydb.is_connected():
            mycursor = mydb.cursor()
            try:
                mycursor.execute("CREATE table IF NOT EXISTS usuarioActivo(ID_Activo int auto_increment primary key, UserHash varchar(25), FOREIGN KEY (UserHash) REFERENCES Usuarios(UserHash));")
                mycursor.execute("SELECT UserHash FROM usuarioactivo WHERE ID_Activo = 1;")
                result = mycursor.fetchone()
                if result is None:
                    mycursor.execute("INSERT INTO usuarioActivo (ID_Activo, UserHash) VALUES (1, %s)", (userHash,))

                    mydb.commit()
                else:
                    mycursor.execute("UPDATE usuarioActivo SET UserHash = %s WHERE ID_Activo = 1", (userHash,))
                    mydb.commit()
                return jsonify({'response': 'Active user set succesfully',
                                'userHash': userHash}), 200
            except Exception as e:
                return jsonify({'error': 'Error: ' + str(e),
                                'response': 'error'}), 400
        else:
            return jsonify({'error': 'Connection failed',
                            'response': 'error'}), 400

def createUserHash():
    # returns a random string of 10 characters
    return ''.join(random.choice(string.ascii_letters + string.digits) for _ in range(10))

def getPasswordHash(contraseña):
    # Convert the password to bytes before generating the hash
    contraseña_bytes = contraseña.encode('utf-8')
    # Create a hash object using SHA-256
    hash_obj = hashlib.sha256()
    # Update the hash object with the password
    hash_obj.update(contraseña_bytes)
    # Obtain the hash in hexadecimal format
    hash_resultado = hash_obj.hexdigest()
    return hash_resultado