import psycopg2

def verify_user(login, password):
    conn = psycopg2.connect(dbname='messenger_db', user='postgres', password='12345', host='localhost', port='5432')
    cur = conn.cursor()
    cur.execute('SELECT * FROM users WHERE login = %s AND password_hash = %s', (login, password))
    log_status = 0
    row = cur.fetchone()
    if row is not None:
        log_status = row[0]
    else:
        log_status = None
    cur.close()
    conn.close()
    return log_status


def save_messages(chat_id, sender_id, text):
    conn = psycopg2.connect(dbname='messenger_db', user='postgres', password='12345', host='localhost', port='5432')
    cur = conn.cursor()
    cur.execute('INSERT INTO messages (chat_id,sender_id, message_text) VALUES (%s, %s, %s)', (chat_id, sender_id, text))
    conn.commit()
    cur.close()
    conn.close()


def register_user(login, password, name):
    conn = psycopg2.connect(dbname='messenger_db', user='postgres', password='12345', host='localhost', port='5432')
    cur = conn.cursor()
    cur.execute('SELECT * FROM users WHERE login = %s', (login,))
    row = cur.fetchone()
    reg_status = 0
    if row is not None:
        reg_status = False
    else:
        cur.execute('INSERT INTO users (login, password_hash, display_name) VALUES (%s, %s, %s)', (login, password, name))
        reg_status = True
    conn.commit()
    cur.close()
    conn.close()
    return reg_status

