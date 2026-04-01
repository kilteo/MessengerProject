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
    return True


def register_user(login, password, name):
    conn = psycopg2.connect(dbname='messenger_db', user='postgres', password='12345', host='localhost', port='5432')
    cur = conn.cursor()
    cur.execute('SELECT * FROM users WHERE login = %s', (login,))
    row = cur.fetchone()

    if row is not None:
        cur.close()
        conn.close()
        return False, None
    else:
        cur.execute('INSERT INTO users (login, password_hash, display_name) VALUES (%s, %s, %s) RETURNING id', (login, password, name))
        new_id = cur.fetchone()[0]
        conn.commit()
        cur.close()
        conn.close()
        return True, new_id



def create_chat(name, is_group, creator_id):
    conn = psycopg2.connect(dbname='messenger_db', user='postgres', password='12345', host='localhost', port='5432')
    cur = conn.cursor()
    new_chat_id = 0
    cur.execute('INSERT INTO chats (name, is_group) VALUES (%s, %s) RETURNING id', (name, is_group))
    new_chat_id = cur.fetchone()[0]
    cur.execute('INSERT INTO chat_members (chat_id, user_id) VALUES (%s, %s)', (new_chat_id, creator_id))
    conn.commit()
    cur.close()
    conn.close()
    return new_chat_id

def get_users_chats(user_id):
    conn = psycopg2.connect(dbname='messenger_db', user='postgres', password='12345', host='localhost', port='5432')
    cur = conn.cursor()
    formated_chats = []
    cur.execute('SELECT DISTINCT ON (chats.id) chats.id,  chats.name, chats.is_group, chat_members.user_id, messages.sender_id, users.display_name, messages.message_text as last_message, messages.sent_at as last_time   FROM chats JOIN chat_members ON chats.id = chat_members.chat_id JOIN messages ON chats.id = messages.chat_id JOIN users ON messages.sender_id = users.id WHERE chat_members.user_id = %s ORDER BY chats.id, messages.sent_at DESC' , (user_id,))
    chats = cur.fetchall()
    for row in chats:
        chat = {'chat_id': row[0], 'name': row[1], 'is_group': row[2], 'user_id': row[3], 'sender_name': row[5], 'last_message': row[6], 'sent_at': row[7].strftime("%H:%M")}
        if chat['name'] is None:
            chat['name'] = row[5]
        formated_chats.append(chat)
    cur.close()
    conn.close()
    return formated_chats


def get_messages(chat_id):
    conn = psycopg2.connect(dbname='messenger_db', user='postgres', password='12345', host='localhost', port='5432')
    cur = conn.cursor()
    cur.execute('SELECT messages.sender_id, users.display_name, messages.message_text, messages.sent_at FROM messages JOIN users ON messages.sender_id = users.id WHERE messages.chat_id = %s ORDER BY messages.sent_at ASC', (chat_id,))
    messages = cur.fetchall()
    formated_messages = []
    for row in messages:
        msg = {
            'sender_id': row[0],
            'sender_name': row[1],
            'text': row[2],
            'time': row[3].strftime("%H:%M"),
        }
        formated_messages.append(msg)
    cur.close()
    conn.close()
    return formated_messages

