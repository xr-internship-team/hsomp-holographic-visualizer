import cv2
import numpy as np
from pupil_apriltags import Detector
import csv
from datetime import datetime
# Kalibrasyon parametrelerini yükle
with np.load("calib_params.npz") as data:
    camera_matrix = data["camera_matrix"]
    dist_coeffs = data["dist_coeffs"]




# --- AprilTag ayarları ---
tag_size = 0.0375 # Metre cinsinden (örneğin: 5 cm)

# CSV dosyasını hazırla
csv_file = open('apriltag_log.csv', mode='w', newline='')
csv_writer = csv.writer(csv_file)
csv_writer.writerow(['Time', 'ID', 'Tx', 'Ty', 'Tz',
                     'R00', 'R01', 'R02', 'R10', 'R11', 'R12', 'R20', 'R21', 'R22','BetaX','BetaY','BetaZ','AlphaX','AlphaY','AlphaZ'])
# AprilTag detector oluştur
detector = Detector(families="tag36h11",
                    nthreads=1,
                    quad_decimate=1,
                    quad_sigma=0.0,
                    refine_edges=1,
                    decode_sharpening=0.25,
                    debug=0)

cap = cv2.VideoCapture(0)

while True:
    ret, frame = cap.read()
    if not ret:
        break

    gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
    #AprilTag detection
    tags = detector.detect(gray, estimate_tag_pose=True,
                           camera_params=[camera_matrix[0, 0], camera_matrix[1, 1], camera_matrix[0, 2], camera_matrix[1, 2]],
                           tag_size=tag_size)

    for tag in tags:
        # Çerçeve çiz
        for idx in range(4):
            pt1 = tuple(map(int, tag.corners[idx]))
            pt2 = tuple(map(int, tag.corners[(idx + 1) % 4]))
            cv2.line(frame, pt1, pt2, (0, 255, 0), 2)

        # Poz tahmini (OpenCV uyumlu hale getir)
        rmat = tag.pose_R
        tvec = tag.pose_t.reshape(3)

        # Örnek veri/ Finds the beta point as getting the alpha point 
        P_local = np.array([0.0, 1, 1])  # Marker'a göre 1cm sağ, 2cm yukarı bir nokta
        R = rmat                        # 3x3 rotasyon matrisi
        T = tvec              # 3x1 translation vektörü

        # Küresel koordinatta noktanın yeri
        P_global = R @ P_local + T

        print("Global (kamera) sistemine göre nokta:", P_global)
        cv2.putText(frame, f"Beta Point: {P_global}", (10, 60), cv2.FONT_HERSHEY_SIMPLEX, 0.6, (0, 0, 255), 2)
        cv2.putText(frame, f"Alpha Point: {P_local}", (10, 90), cv2.FONT_HERSHEY_SIMPLEX, 0.6, (0, 255, 0), 2)

        # X, Y, Z eksenlerini çiz
        rvec, _ = cv2.Rodrigues(rmat)
        tvec_cv = tvec.reshape(3, 1)
        cv2.drawFrameAxes(frame, camera_matrix, dist_coeffs, rvec, tvec_cv, 0.03)

        print(f"Tag ID: {tag.tag_id}, tag.pose_t: {tag.pose_t}, tag.pose_R: {tag.pose_R}")
        # OpenCV için dönüştür (Rodrigues formatı gerekiyor)

        rvec_cv, _ = cv2.Rodrigues(rvec)
        tvec_cv = tvec.reshape(3, 1)
        # Print the Tz to window on cv2
        cv2.putText(frame, f"Tz: {tvec[2]}", (10, 30), cv2.FONT_HERSHEY_SIMPLEX, 0.6, (0, 0, 255), 2)   


        # Koordinat sistemini çiz
        cv2.drawFrameAxes(frame, camera_matrix, dist_coeffs, rvec_cv, tvec_cv, 0.03)
        # Tarih + Zaman
        timestamp = datetime.now().strftime("%Y-%m-%d %H:%M:%S.%f")[:-3]

        # CSV'ye yaz
        row = [timestamp, tag.tag_id] + list(tvec) + list(rmat.flatten()) + list(P_global) + list(P_local)
        csv_writer.writerow(row)

        # ID yaz
        center = tuple(map(int, tag.center))
        cv2.putText(frame, f'ID: {tag.tag_id}', center, cv2.FONT_HERSHEY_SIMPLEX, 0.6, (0, 0, 255), 2)

    cv2.imshow("AprilTag Tracker", frame)

    if cv2.waitKey(1) == 27:
        break

cap.release()
csv_file.close()
cv2.destroyAllWindows()

