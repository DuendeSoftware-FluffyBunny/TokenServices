using Microsoft.Extensions.Logging;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Common
{
    public class BinarySerializer : IBinarySerializer
    {
        private ILogger<BinarySerializer> _logger;
        private ISerializer _serializer;
        private BinaryFormatter _formatter;

        public BinarySerializer(ISerializer serializer, ILogger<BinarySerializer> logger)
        {
            _logger = logger;
            _serializer = serializer;
            _formatter = new BinaryFormatter();
        }
        public T Deserialize<T>(byte[] data)
            where T : class
        {
            try
            {
                var json = Encoding.UTF8.GetString(data);
                var obj = _serializer.Deserialize<T>(json);
                return obj;
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                throw;
            }

            throw new NotImplementedException();
        }

        public byte[] Serialize<T>(T data)
            where T : class
        {
            try
            {
                var json = _serializer.Serialize(data);
                byte[] byteData = Encoding.UTF8.GetBytes(json);
                return byteData;

            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);

                throw;
            }
        }
    }
}
