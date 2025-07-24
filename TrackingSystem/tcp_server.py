import cv2
import numpy as np
import socket
import json
import time
from pupil_apriltags import Detector

# === Kamera ve TCP Ayarları ===
HOST = '0.0.0.0'
PORT = 12345
marker_length = 0.0375 # metre
#Kalibrasyon Değerlerini yükleme
with np.load("calib_params.npz") as data:
    camera_matrix = data["camera_matrix"]
    dist_coeffs = data["dist_coeffs"]

# TCP server kur
server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server_socket.bind((HOST, PORT))
server_socket.listen(1)
print(f"[🔌] Bekleniyor: {HOST}:{PORT}")
client_socket, addr = server_socket.accept() #Unitynin bağlanmasını bekliyor
print(f"[✅] Bağlandı: {addr}")

# AprilTag dedektörü
at_detector = Detector(families='tag36h11',
                       nthreads=1,
                       quad_decimate=1.0,
                       quad_sigma=0.0,
                       refine_edges=1,
                       decode_sharpening=0.25,
                       debug=0)
#Kamera açma
cap = cv2.VideoCapture(0)
if not cap.isOpened():
    print("[❌] Kamera açılamadı!")
    exit()

#Quaternion = (x, y, z, w) 
# Not: Bu x, y, z pozisyon değil, dönme eksenidir.
#(x, y, z) → Dönme ekseninin yönü (normalize edilmiş vektör)
#w → Dönme miktarının (açının) kosinüsüdür (yarıya bölünmüş açı üzerinden)

#"rotation": {
#  "x": 0.0,
#  "y": 0.707,
#  "z": 0.0,
#  "w": 0.707
#}

def rotation_matrix_to_quaternion(R):
    # OpenCV 3x3 rotation matrix → Quaternion (Unity uyumlu)
    qw = np.sqrt(1 + R[0,0] + R[1,1] + R[2,2]) / 2
    qx = (R[2,1] - R[1,2]) / (4 * qw)
    qy = (R[0,2] - R[2,0]) / (4 * qw)
    qz = (R[1,0] - R[0,1]) / (4 * qw)
    return qx, qy, qz, qw

try:
    while True:
        ret, frame = cap.read()
        if not ret:
            continue

        gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)

        results = at_detector.detect(
            gray,
            estimate_tag_pose=True,
            camera_params=(camera_matrix[0,0], camera_matrix[1,1], camera_matrix[0,2], camera_matrix[1,2]),
            tag_size=marker_length
        )

        for tag in results:
            if tag.pose_t is None or tag.pose_R is None:
                continue
            tvec = tag.pose_t.flatten()
            rvec = tag.pose_R

            qx, qy, qz, qw = rotation_matrix_to_quaternion(rvec)

            data = {
                "timestamp": time.time(),
                "position": {
                    "x": round(float(tvec[0]), 4),
                    "y": round(float(tvec[1]), 4),
                    "z": round(float(tvec[2]), 4)
                },
                "rotation": {
                    "x": round(float(qx), 4),
                    "y": round(float(qy), 4),
                    "z": round(float(qz), 4),
                    "w": round(float(qw), 4)
                }
            }

            json_data = json.dumps(data)
            client_socket.sendall((json_data + "\n").encode("utf-8"))
            print(f"[📤] Tag {tag.tag_id} gönderildi: {json_data}")

        time.sleep(0.05)

except Exception as e:
    print("[❌] Hata:", e)

finally:
    cap.release()
    client_socket.close()
    server_socket.close()
