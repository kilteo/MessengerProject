import socket
import sys
import json
import database

HOST = 'localhost'
PORT = 10000
server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

server_socket.bind((HOST, PORT))
server_socket.listen(5)
while True:
    client_socket, client_address = server_socket.accept()
    print(f"Кто-то подключился! Адрес: {client_address}")
    try:
        while True:
            data = client_socket.recv(1024)
            if not data:
                break
            text = data.decode('utf-8')
            parts = text.split('|')
            print(parts)
            if parts[0] == 'REGISTER':
                result = database.register_user(parts[1], parts[2], parts[3])
                if result == True:
                    client_socket.sendall('SUCCESS'.encode('utf-8'))
                else:
                    client_socket.sendall('ERROR'.encode('utf-8'))

            elif parts[0] == 'LOGIN':
                result = database.verify_user(parts[1], parts[2])
                if result:
                    response = f"SUCCESS|{result}"
                    client_socket.sendall(response.encode('utf-8'))
                else:
                    client_socket.sendall('ERROR'.encode('utf-8'))

            elif parts[0] == 'GET_CHATS':
                chats = database.get_users_chats(parts[1])
                json_string = json.dumps(chats)
                client_socket.sendall(json_string.encode('utf-8'))

            elif parts[0] == 'GET_MESSAGES':
                messages = database.get_messages(parts[1])
                json_string = json.dumps(messages, default=str)
                client_socket.sendall(json_string.encode('utf-8'))

            elif parts[0] == 'SEND_MESSAGE':
                message = database.save_messages(parts[1], parts[2], parts[3])
                if message:
                    client_socket.sendall("SUCCESS".encode('utf-8'))
                else:
                    client_socket.sendall('ERROR'.encode('utf-8'))


    finally:
        client_socket.close()