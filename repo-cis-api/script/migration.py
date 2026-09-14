import mysql.connector
from pymongo import MongoClient

MYSQL_CONFIG = {
    'host': 'localhost',
    'user': 'root',
    'password': 'sd5',
    'database': 'sd3',
    'port': 3307
}

MONGO_URI = "mongodb://localhost:27017/"
DATABASE_NAME = "sd3"

def transform_row(row):
    if 'id' in row:
        row['_id'] = row['id']
        del row['id']
    return row

def big_bang_migration():
    try:
        mysql_conn = mysql.connector.connect(**MYSQL_CONFIG)
        cursor = mysql_conn.cursor(dictionary=True)

        mongo_client = MongoClient(MONGO_URI)
        mongo_db = mongo_client[DATABASE_NAME]

        tabelas = ['users', 'topics', 'ideas']

        print(f"Iniciando Migração Big Bang para o banco: {DATABASE_NAME}")

        for tabela in tabelas:
            try:
                print(f"Migrando tabela: {tabela}...")

                cursor.execute(f"SELECT * FROM {tabela}")
                rows = cursor.fetchall()

                if rows:
                    docs = [transform_row(row) for row in rows]

                    mongo_db.drop_collection(tabela)

                    mongo_db[tabela].insert_many(docs)
                    print(f"{len(docs)} registros migrados.")
                else:
                    print(f"Tabela '{tabela}' está vazia.")

            except Exception as table_error:
                print(f"Erro na tabela {tabela}: {table_error}")

        print("\nMigração concluída!")

    except Exception as e:
        print(f"Erro geral: {e}")

    finally:
        if 'mysql_conn' in locals() and mysql_conn.is_connected():
            cursor.close()
            mysql_conn.close()
            print("Conexões encerradas.")

if __name__ == "__main__":
    big_bang_migration()