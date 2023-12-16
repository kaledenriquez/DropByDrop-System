from flask import Blueprint, jsonify, request
import mysql.connector

queries = Blueprint('queries', __name__)

# Route to verify connection to api
@queries.route('/test', methods=['GET'])
def test():
    return jsonify({'response': 'The API is working'}), 200

# Route too create a database with the name provided
@queries.route('/createDatabase', methods=['POST'])
def createDatabase():
    data = request.get_json()
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
        )
        if mydb.is_connected():
            mycursor = mydb.cursor()
            try:
                mycursor.execute("CREATE DATABASE IF NOT EXISTS " + db_name)
                return jsonify({'response': 'database created successfully'}), 200
            except Exception as e:
                return jsonify({'error': 'Error: ' + str(e),
                                'response': 'error'}), 400
        else:
            return jsonify({'error': 'Connection failed',
                            'response': 'error'}), 400
        
# Route to create a table with the name provided
@queries.route('/createTable', methods=['POST'])
def createTable():
    data = request.get_json()
    db_name = data['db_name']
    table_name = data['table_name']
    passwordReq = data['password']
    userReq = data['user']
    columns = data['columns']

    if db_name is None:
        return jsonify({'error': 'No database name provided',
                        'response': 'error'}), 400
    elif table_name is None:
        return jsonify({'error': 'No table name provided',
                        'response': 'error'}), 400
    elif columns is None:
        return jsonify({'error': 'No columns provided',
                        'response': 'error'}), 400
    else:
        mydb = mysql.connector.connect(
            host="localhost",
            user=userReq,
            password=passwordReq,
        )
        if mydb.is_connected():
            mycursor = mydb.cursor()
            try:
                mycursor.execute("CREATE DATABASE IF NOT EXISTS " + db_name)
                mycursor.execute("USE " + db_name)
                mycursor.execute("CREATE TABLE IF NOT EXISTS " + table_name + " (" + columns + ")")
                return jsonify({'response': 'Table created successfully'}), 200
            except Exception as e:
                return jsonify({'error': 'Error: ' + str(e),
                                'response': 'error'}), 400
        else:
            return jsonify({'error': 'Connection failed',
                            'response': 'error'}), 400
        
# Route to insert data into a table
@queries.route('/insertData', methods=['POST'])
def insertData():
    data = request.get_json()
    db_name = data['db_name']
    table_name = data['table_name']
    passwordReq = data['password']
    userReq = data['user']
    columns = data['columns']
    values = data['values']

    if db_name is None or db_name == "":
        return jsonify({'error': 'No database name provided',
                        'response': 'error'}), 400
    elif table_name is None:
        return jsonify({'error': 'No table name provided',
                        'response': 'error'}), 400
    elif values is None:
        return jsonify({'error': 'No values provided',
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
                if columns is None or columns == "":
                    mycursor.execute("INSERT INTO " + table_name + " VALUES (" + values + ")")
                    mydb.commit()
                    return jsonify({'response': 'Data inserted with all columns affected successfully'}), 200
                else:
                    mycursor.execute("INSERT INTO " + table_name + " (" + columns + ") VALUES (" + values + ")")
                    mydb.commit()
                    return jsonify({'response': 'Data inserted in the columns specified successfully'}), 200
            except Exception as e:
                return jsonify({'error': 'Error: ' + str(e),
                                'response': 'error'}), 400
        else:
            return jsonify({'error': 'Connection failed',
                            'response': 'error'}), 400

# Route to drop a table
@queries.route('/dropTable', methods=['POST'])
def dropTable():
    data = request.get_json()
    db_name = data['db_name']
    table_name = data['table_name']
    passwordReq = data['password']
    userReq = data['user']

    if db_name is None:
        return jsonify({'error': 'No database name provided',
                        'response': 'error'}), 400
    elif table_name is None:
        return jsonify({'error': 'No table name provided',
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
                mycursor.execute("DROP TABLE IF EXISTS " + table_name)
                return jsonify({'response': 'Table dropped successfully'}), 200
            except Exception as e:
                return jsonify({'error': 'Error: ' + str(e),
                                'response': 'error'}), 400
        else:
            return jsonify({'error': 'Connection failed',
                            'response': 'error'}), 400

# Route to drop a database
@queries.route('/dropDatabase', methods=['POST'])
def dropDatabase():
    data = request.get_json()
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
        )
        if mydb.is_connected():
            mycursor = mydb.cursor()
            try: 
                mycursor.execute("DROP DATABASE IF EXISTS " + db_name)
                return jsonify({'response': 'Database dropped successfully'}), 200
            except Exception as e:
                return jsonify({'error': 'Error: ' + str(e),
                                'response': 'error'}), 400
        else:
            return jsonify({'error': 'Connection failed',
                            'response': 'error'}), 400
        
# Route to request data from a table
@queries.route('/requestData', methods=['GET', 'POST'])
def requestData():
    print(request)
    data = request.get_json()
    print(data)
    db_name = data['db_name']
    query = data['query']
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
                mycursor.execute(query)
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
                # if data is empty, send a table that has an element with the name of the columns and empty variables in each one
                if len(data) == 0:
                    data = [dict(zip(columns, ['' for i in range(len(columns))]))]
                print(data)
                return jsonify({'response': data}), 200

            except Exception as e:
                return jsonify({'error': 'Error: ' + str(e),
                                'response': 'error'}), 400
        else:
            return jsonify({'error': 'Connection failed',
                            'response': 'error'}), 400

# Route to update data in a table
@queries.route('/updateData', methods=['POST'])
def updateData():
    data = request.get_json()
    db_name = data['db_name']
    query = data['query']
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
                mycursor.execute(query)
                mydb.commit()
                return jsonify({'response': 'Data updated successfully'}), 200
            except Exception as e:
                return jsonify({'error': 'Error: ' + str(e),
                                'response': 'error'}), 400
        else:
            return jsonify({'error': 'Connection failed',
                            'response': 'error'}), 400

# Route to create the default tables for the project
@queries.route("/createTables", methods=['POST'])
def createTables():
    data = request.get_json()
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
        )
        if mydb.is_connected():
            mycursor = mydb.cursor()
            try:
                mycursor.execute("CREATE DATABASE IF NOT EXISTS " + db_name)
                mycursor.execute("USE " + db_name)
                mycursor.execute("CREATE TABLE IF NOT EXISTS Sensor (ID_Sensor INT AUTO_INCREMENT PRIMARY KEY, tipo VARCHAR(255), valor DECIMAL(10,2),b1 int,b0 int);")
                mycursor.execute("CREATE TABLE IF NOT EXISTS Usuarios (UserHash varchar(25) PRIMARY KEY, Usuario VARCHAR(25), Contraseña VARCHAR(100));")
                mycursor.execute("CREATE TABLE IF NOT EXISTS Alerta (ID_Alerta INT AUTO_INCREMENT PRIMARY KEY, hora DATETIME, tipoAlerta VARCHAR(255), ID_Sensor INT, UserHash varchar(25), FOREIGN KEY (ID_Sensor) REFERENCES Sensor(ID_Sensor), FOREIGN KEY (UserHash) REFERENCES Usuarios(UserHash));")
                mycursor.execute("CREATE TABLE IF NOT EXISTS AsistenteVirtual (ID_Asistente INT AUTO_INCREMENT PRIMARY KEY, nombreAsistente VARCHAR(255), UserHash varchar(25), FOREIGN KEY (UserHash) REFERENCES Usuarios(UserHash));")
                mycursor.execute("CREATE TABLE IF NOT EXISTS ConsumoAgua (ID_Consumo INT AUTO_INCREMENT PRIMARY KEY, consumoAgua DECIMAL(10,2), hora DATETIME, UserHash varchar(25), FOREIGN KEY (UserHash) REFERENCES Usuarios(UserHash));")
                mycursor.execute("CREATE TABLE IF NOT EXISTS bitcoraRegistros(bitRegID int primary key auto_increment, marcaTiempo datetime not null, nodeID int not null, numLED int not null, estadoLED int not null, UserHash varchar(25), FOREIGN KEY (UserHash) REFERENCES Usuarios(UserHash));")
                mycursor.execute("CREATE TABLE IF NOT EXISTS configuracion(ID_Config int auto_increment primary key, ID_Sensor int, valorMin decimal(10,2) not null, valorMax decimal(10,2) not null, UserHash varchar(25), FOREIGN KEY (ID_Sensor) REFERENCES Sensor(ID_Sensor), FOREIGN KEY (UserHash) REFERENCES Usuarios(UserHash));")
                mycursor.execute("CREATE TABLE IF NOT EXISTS usuarioActivo(ID_Activo int auto_increment primary key, UserHash varchar(25), FOREIGN KEY (UserHash) REFERENCES Usuarios(UserHash));")
                mycursor.execute("INSERT IGNORE INTO Sensor (ID_Sensor, tipo, valor, b1, b0) VALUES (1, 'Presion', 200, 0, 0);")
                mycursor.execute("INSERT IGNORE INTO Sensor (ID_Sensor, tipo, valor, b1, b0) VALUES (2, 'Humedad', 50, 0, 0);")
                mycursor.execute("INSERT IGNORE INTO Sensor (ID_Sensor, tipo, valor, b1, b0) VALUES (3, 'Temperatura', 40, 0, 0);")
                mycursor.execute("INSERT IGNORE INTO Sensor (ID_Sensor, tipo, valor, b1, b0) VALUES (4, 'ConsumoAgua', 100, 0, 0);")
                mydb.commit()
                return jsonify({'response': 'Tables created successfully'}), 200
            except Exception as e:
                return jsonify({'error': 'Error: ' + str(e),
                                'response': 'error'}), 400
        else:
            return jsonify({'error': 'Connection failed',
                            'response': 'error'}), 400

# Route to create an alert in the database
@queries.route("/crearAlerta", methods=['POST'])
def crearAlerta():
    data = request.get_json()
    db_name = data['db_name']
    passwordReq = data['password']
    userReq = data['user']
    tipoAlerta = data['tipoAlerta']
    ID_Sensor = data['ID_Sensor']

    if db_name is None:
        return jsonify({'error': 'No database name provided',
                        'response': 'error'}), 400
    elif tipoAlerta is None:
        return jsonify({'error': 'No alert type provided',
                        'response': 'error'}), 400
    elif ID_Sensor is None:
        return jsonify({'error': 'No sensor ID provided',
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
                mycursor.execute("SELECT UserHash FROM usuarioactivo WHERE ID_Activo = 1;")
                result = mycursor.fetchone()
                UserHash = result[0]
                mycursor.execute("INSERT INTO Alerta (hora, tipoAlerta, ID_Sensor, UserHash) VALUES (NOW(), '" + tipoAlerta + "', " + str(ID_Sensor) + ", '" + UserHash + "')")
                mydb.commit()
                return jsonify({'response': 'Alert created successfully'}), 200
            except Exception as e:
                return jsonify({'error': 'Error: ' + str(e),
                                'response': 'error'}), 400
        else:
            return jsonify({'error': 'Connection failed',
                            'response': 'error'}), 400

# Path to insert water consumption data
@queries.route("/insertarConsumo", methods=['POST'])
def insertarConsumo():
    data = request.get_json()
    db_name = data['db_name']
    passwordReq = data['password']
    userReq = data['user']
    consumoAgua = data['consumoAgua']

    if db_name is None:
        return jsonify({'error': 'No database name provided',
                        'response': 'error'}), 400
    elif consumoAgua is None:
        return jsonify({'error': 'No water consumption provided',
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
                mycursor.execute("SELECT UserHash FROM usuarioactivo WHERE ID_Activo = 1;")
                result = mycursor.fetchone()
                UserHash = result[0]
                mycursor.execute("INSERT INTO ConsumoAgua (consumoAgua, hora, UserHash) VALUES (" + str(consumoAgua) + ", NOW(), '" + UserHash + "')")
                mydb.commit()
                return jsonify({'response': 'Water consumption inserted successfully'}), 200
            except Exception as e:
                return jsonify({'error': 'Error: ' + str(e),
                                'response': 'error'}), 400
        else:
            return jsonify({'error': 'Connection failed',
                            'response': 'error'}), 400

# Route to verify if the user has alerts
@queries.route("/activeUserHasAlerts", methods=['POST'])
def activeUserHasAlerts():
    data = request.get_json()
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
                mycursor.execute("SELECT UserHash FROM usuarioactivo WHERE ID_Activo = 1;")
                result = mycursor.fetchone()
                UserHash = result[0]
                mycursor.execute("SELECT * FROM Alerta WHERE UserHash = '" + UserHash + "';")
                result = mycursor.fetchone()
                if result is None:
                    return jsonify({'response': False}), 200
                else:
                    return jsonify({'response': True}), 200
            except Exception as e:
                return jsonify({'error': 'Error: ' + str(e),
                                'response': 'error'}), 400

        else:
            return jsonify({'error': 'Connection failed',
                            'response': 'error'}), 400