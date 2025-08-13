/// Interface for all receiver types.
/// Defines the basic operations for receiving data.
public interface IReceiver
{
    /// Creates and initializes the receiving client.
    public void CreateClient();

    /// Retrieves data from the source.
    /// Returns ReceivedData object or null if failed.
    public ReceivedData GetData();

    /// Closes the client and cleans up resources.
    public void Close();
}
