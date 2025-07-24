using System;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using Newtonsoft.Json.Linq;
/*  1. Start() → Python TCP server’a bağlan
    2. Update() → gelen byte’ları oku
    3. \n ile biten JSON mesajlarını ayır
    4. JSON verisinden pozisyon ve rotasyon al
    5. Hedef objeye uygula    */




//Küçük verilerle çalışır
public class TCPClientReceiver : MonoBehaviour
{
    public string serverIP = "10.10.50.22"; // Python'un çalıştığı IP
    public int port = 12345;
    public Transform targetObject;//Güncellencek obje

    private TcpClient client; //Client nesnesi
    private NetworkStream stream; //Veri akışı
    private byte[] buffer = new byte[1024]; //Geçici olarak verileri tut

    private Vector3 latestPosition;
    private Quaternion latestRotation;
    private bool dataReceived = false;

    void Start()
    {
        try
        {
            client = new TcpClient(serverIP, port);
            stream = client.GetStream();              //Bağlantıyı kur
            Debug.Log("[TCP] Bağlantı başarılı.");
        }
        catch (Exception ex)
        {
            Debug.LogError("[TCP] Bağlantı hatası: " + ex.Message);
        }
    }

    private StringBuilder dataBuffer = new StringBuilder();  //Ayrı ayrı gelen verileri bekler
    
    
    //Json dosyasını okuma
    void Update()
    {
        if (client != null && stream != null && stream.DataAvailable)
        {
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string chunk = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            dataBuffer.Append(chunk);

            string content = dataBuffer.ToString();
            int newlineIndex;
            while ((newlineIndex = content.IndexOf('\n')) >= 0)
            {
                string line = content.Substring(0, newlineIndex).Trim();
                content = content.Substring(newlineIndex + 1);

                if (!string.IsNullOrEmpty(line))
                {
                    try
                    {
                        JObject data = JObject.Parse(line);

                        float x = data["position"]["x"].Value<float>();
                        float y = data["position"]["y"].Value<float>();
                        float z = data["position"]["z"].Value<float>();

                        float rx = data["rotation"]["x"].Value<float>();
                        float ry = data["rotation"]["y"].Value<float>();
                        float rz = data["rotation"]["z"].Value<float>();
                        float rw = data["rotation"]["w"].Value<float>();

                        latestPosition = new Vector3(x, y, z);
                        latestRotation = new Quaternion(rx, ry, rz, rw);
                        dataReceived = true;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("JSON Hatası: " + e.Message);
                    }
                }
            }

            // kalan veriyi tekrar buffer'a al
            dataBuffer.Clear();
            dataBuffer.Append(content);
        }

        //Data alındıysa pozisyonu güncelle latest'a kopyalamıştık gelen rotasyon ve konum bilgilerini
        if (dataReceived && targetObject != null)
        {
            targetObject.position = latestPosition;
            targetObject.rotation = latestRotation;
        }
    }

    void OnApplicationQuit()
    {
        stream?.Close();
        client?.Close();
    }
}
