public interface IReceiver
{
    public void CreateClient(string ipAddress, int portNumber);
    public void InitializeReceiver();

    public void StopReceiving();
}
