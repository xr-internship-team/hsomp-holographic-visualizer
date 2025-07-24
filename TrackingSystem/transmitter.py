
import socket
import json
import time
# UDP hedef bilgileri
UDP_IP = "127.0.0.1"  # HoloLens IP adresi
UDP_PORT = 12345

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

while True:
    # Örnek pozisyon ve yönelim (x, y, z, qx, qy, qz, qw)
    pose_data = {
        "position": [0.5, 1.2, 0.3],
        "rotation": [0.0, 0.0, 0.0, 1.0]
    }

    message = json.dumps(pose_data)
    sock.sendto(message.encode(), (UDP_IP, UDP_PORT))
    print("Gönderildi:", message)
    time.sleep(1)  # 1 saniyede bir gönder