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
    cur.execute('INSERT INTO messages (chat_id,sender_id, message_text, sent_at) VALUES (%s, %s, %s, NOW())', (chat_id, sender_id, text))
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


def create_personal_chat(current_user_id, target_login):
    conn = psycopg2.connect(dbname='messenger_db', user='postgres', password='12345', host='localhost', port='5432')
    cur = conn.cursor()
    try:
        cur.execute("SELECT id FROM users WHERE login = %s", (target_login,))
        target_user = cur.fetchone()

        if not target_user:
            return "USER_NOT_FOUND"
        target_user_id = target_user[0]

        if int(current_user_id) == target_user_id:
            return "CANNOT_CHAT_WITH_SELF"

        check_query = """
            SELECT c.id 
            FROM chats c
            JOIN chat_members m1 ON c.id = m1.chat_id
            JOIN chat_members m2 ON c.id = m2.chat_id
            WHERE c.is_group = FALSE 
              AND m1.user_id = %s 
              AND m2.user_id = %s
        """
        cur.execute(check_query, (current_user_id, target_user_id))
        existing_chat = cur.fetchone()
        if existing_chat:
            return f"CHAT_ALREADY_EXISTS|{existing_chat[0]}"

        cur.execute("INSERT INTO chats (is_group) VALUES (FALSE) RETURNING id")
        new_chat_id = cur.fetchone()[0]

        cur.execute("INSERT INTO chat_members (chat_id, user_id) VALUES (%s, %s), (%s, %s)",
                    (new_chat_id, current_user_id, new_chat_id, target_user_id))
        conn.commit()

        return f"SUCCESS|{new_chat_id}"

    except Exception as e:
        conn.rollback()
        print("Ошибка при создании чата:", e)
        return "ERROR"
    finally:
        cur.close()
        conn.close()


def get_users_chats(user_id):
    conn = psycopg2.connect(dbname='messenger_db', user='postgres', password='12345', host='localhost', port='5432')
    cur = conn.cursor()
    formated_chats = []
    cur.execute('SELECT DISTINCT ON (chats.id) chats.id, COALESCE(chats.name, (SELECT u.display_name FROM chat_members other_member JOIN users u ON other_member.user_id = u.id WHERE other_member.chat_id = chats.id AND other_member.user_id != %s LIMIT 1)) as chat_name, chats.is_group, messages.message_text as last_message, messages.sent_at as last_time FROM chats JOIN chat_members ON chats.id = chat_members.chat_id LEFT JOIN messages ON chats.id = messages.chat_id WHERE chat_members.user_id = %s ORDER BY chats.id, messages.sent_at DESC',(user_id, user_id))
    chats = cur.fetchall()

    for row in chats:
        chat = {
            'chat_id': row[0],
            'name': row[1],
            'is_group': row[2],
            'last_message': row[3] if row[3] else "Нет сообщений",
            'sent_at': row[4].strftime("%H:%M") if row[4] else ""
        }
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
            'time': row[3].isoformat(),
        }
        formated_messages.append(msg)
    cur.close()
    conn.close()
    return formated_messages

