from flask import Flask
from flask_restful import Api

def create_app():
    app = Flask(__name__)
    api = Api(app)

    from .queries import queries as queries_blueprint
    app.register_blueprint(queries_blueprint, url_prefix='/queries')

    from .users import users as users_blueprint
    app.register_blueprint(users_blueprint, url_prefix='/users')
    return app