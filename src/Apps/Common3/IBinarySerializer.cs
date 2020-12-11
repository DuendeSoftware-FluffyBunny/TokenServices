namespace Common
{
    public interface IBinarySerializer
    {
        byte[] Serialize<T>(T data) where T : class;
        T Deserialize<T>(byte[] data) where T : class;


    }
}
