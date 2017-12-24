using ProtocolFramework;

namespace PushFramework
{
    public abstract class Serializer
    {
        public abstract bool Serialize(object message, out Buffer bytes);
        public abstract bool Deserialize(Buffer bytes,out int serviceId, out int methodId, out object message);
    }
}