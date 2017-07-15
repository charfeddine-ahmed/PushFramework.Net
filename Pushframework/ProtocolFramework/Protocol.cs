namespace ProtocolFramework
{
    public abstract class Protocol
    {
        public abstract bool ReadBytes(Buffer incomingBytes, object connectionToken);

        public abstract DecodeResult TryDecode(object connectionToken, out Buffer decodedBytes, out Buffer outputBytes);

        public abstract EncodeResult Encode(object connectionToken, Buffer inputBuffer, out Buffer output);

        public abstract object CreateConnectionToken();

        public abstract bool IsNegociationEnded(object connectionToken);

        public abstract void StartProtocolNegociation(object connectionToken, out Buffer outputBytes);

        public abstract string Name
        {
            get;
        }

        public virtual string Version
        {
            get
            {
                return string.Empty;
            }
        }

        public virtual string Author
        {
            get
            {
                return string.Empty;
            }
        }

    }
}