import socket
import json
import time
import random  # demo için pozisyon rastgele

# Server bilgileri
HOST = '0.0.0.0'   # Herkesi dinle
PORT = 12345

# Socket oluştur
server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server_socket.bind((HOST, PORT))
server_socket.listen(1)

print(f"[🔌] Bekleniyor: {HOST}:{PORT}...")

client_socket, addr = server_socket.accept()
print(f"[✅] Bağlandı: {addr}")

try:
    while True:
        # DEMO: Sahte veri oluştur (gerçekte burada marker verisi olacak)
        data = {
            "timestamp": time.time(),
            "position": {
                "x": round(random.uniform(-1, 1), 3),
                "y": round(random.uniform(1, 2), 3),
                "z": round(random.uniform(-2, 0), 3)
            },
            "rotation": {
                "x": 0.0,
                "y": 0.707,
                "z": 0.0,
                "w": 0.707
            }
        }

        json_data = json.dumps(data)
        client_socket.sendall((json_data + "\n").encode("utf-8"))
        print(f"[📤] Gönderildi: {json_data}")
        time.sleep(0.1)

except Exception as e:
    print(f"[❌] Hata: {e}")
finally:
    client_socket.close()
    server_socket.close()
