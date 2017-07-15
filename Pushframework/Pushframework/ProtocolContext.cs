using System.Collections.Generic;
using ProtocolFramework;

namespace PushFramework
{
    internal class ProtocolContext
    {
        public ProtocolContext(Protocol protocol)
        {
            this.protocol = protocol;
            this.ConnectionToken = this.protocol.CreateConnectionToken();
        }

        public object ConnectionToken
        {
            get;
            internal set;
        }

        private Protocol protocol;

        public Protocol Protocol
        {
            get
            {
                return this.protocol;
            }
        }

        public ProtocolContext UpperProtocol
        {
            get;
            set;
        }

        public ProtocolContext LowerProtocol
        {
            get;
            set;
        }

        public bool IsNegociationEnded
        {
            get;
            set;
        }

        public PhysicalConnection PhysicalConnection
        {
            get;
            internal set;
        } 

        public void AdvanceNegociation()
        {
            Buffer outputBytes;
            this.Protocol.StartProtocolNegociation(this.ConnectionToken, out outputBytes);

            if (outputBytes != null)
            {
                this.PhysicalConnection.SendProtocolBytes(outputBytes, this);
            }

            this.IsNegociationEnded = this.Protocol.IsNegociationEnded(this.ConnectionToken);

            if (this.IsNegociationEnded && this.UpperProtocol != null)
            {
                this.UpperProtocol.AdvanceNegociation();
            }
        }

        void CheckAdvanceNegociation()
        {
            if (!this.IsNegociationEnded && this.Protocol.IsNegociationEnded(this.ConnectionToken))
            {
                this.IsNegociationEnded = true;

                if(this.UpperProtocol != null)
                {
                    this.UpperProtocol.AdvanceNegociation();
                }
            }
        }

        public DecodeState TryDecode(out Buffer decodedBytes)
        {
            DecodeState state;
            state.protocol = this;

            decodedBytes = null;

            bool bytesWentToHigherProtocol = false;

            while (true)
            {
                Buffer outputBytes;
                state.decodeResult = this.Protocol.TryDecode(this.ConnectionToken, out decodedBytes, out outputBytes);
                
                if (state.decodeResult == DecodeResult.Failure)
                {
                    if (outputBytes != null)
                    {
                        this.PhysicalConnection.SendProtocolBytes(outputBytes, this);
                    }

                    return state;
                }
                else if (state.decodeResult == DecodeResult.WantMoreData)
                {
                    break;
                }
                else if (state.decodeResult == DecodeResult.Success)
                {
                    if (outputBytes != null)
                    {
                        this.PhysicalConnection.SendProtocolBytes(outputBytes, this);
                    }

                    this.CheckAdvanceNegociation();

                    if (this.UpperProtocol == null)
                    {
                        return state; // We are at the top. this must be an application message.
                    }
                    else
                    {
                        if (!this.UpperProtocol.Protocol.ReadBytes(decodedBytes, this.ConnectionToken))
                        {
                            state.decodeResult = DecodeResult.Failure;
                            return state;
                        }
                        
                        bytesWentToHigherProtocol = true;
                    }
                }
            }

            if (bytesWentToHigherProtocol)
            {
                return this.UpperProtocol.TryDecode(out decodedBytes);
            } 

            return state;
        }

        public EncodeResult Encode(Buffer bytes, out Buffer encodedBytes)
        {
            Buffer encoded;
            EncodeResult result = this.Protocol.Encode(this.ConnectionToken, bytes, out encoded);

            if (result != EncodeResult.Success)
            {
                encodedBytes = null;
                return result;
            }

            if (this.LowerProtocol == null)
            {
                encodedBytes = encoded;
                return EncodeResult.Success;
            }

            return this.LowerProtocol.Encode(encoded, out encodedBytes);            
        }
    }
}