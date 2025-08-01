public interface IReceiver
{
    public void CreateClient();

    public ReceivedData GetData();
    
    public void Close();
}
