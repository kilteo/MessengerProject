import socket

client = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
client.connect(('localhost', 10000))

# Отправляем логин
message = "LOGIN|new_hacker|super_pass"
client.sendall(message.encode('utf-8'))

# Ждем ответ от сервера (вот этого не хватало!)
response = client.recv(1024).decode('utf-8')
print("Ответ от сервера:", response)

client.close()