using System;
using System.Text;

namespace ProtocolFramework
{
    public class Buffer
    {
        public Buffer(int length)
        {
            this.Data = new byte[length];
            this.Size = 0;
            this.Offset = 0;
        }

        public Buffer(byte[] data, int size)
        {
            this.Data = data;
            this.Size = size;
            this.Offset = 0;
        }

         public byte[] Data { get; internal set; }

         public int Size{ get; set;}

         public int Offset{ get; internal set;}

         public int RemainingCapacity
         {
             get
             {
                 return this.Data.Length - this.Size - this.Offset;
             }
         }

         public void Append(Buffer buffer)
         {
             this.Append(buffer.Data, buffer.Offset, buffer.Size);
         }

         public void Append(byte[] bytes, int offset, int count)
         {
             if (this.RemainingCapacity < count){
                 throw new Exception("out of range");
             }

             Array.Copy(bytes, offset, this.Data, this.Offset + this.Size, count);

             this.Size += count;
         }

        public void Append(byte val)
         {
             if (this.RemainingCapacity == 0)
                 throw new Exception("out of range");

             this.Data[this.Offset + this.Size] = val;
             this.Size++;
         }

         public void Pop(int count)
         {
             this.Offset += count;
             this.Size -= count;
         }

        public void PopAndAdjust(int count)
        {
            this.Size -= count;

            if (this.Size == 0)
            {               
                this.Offset = 0;
                return;
            }

            System.Buffer.BlockCopy(this.Data, count + this.Offset, this.Data, 0, this.Size );
            this.Offset = 0;
        }

         public bool Empty()
         {
             return this.Size == 0;
         }

         public void Clear()
         {
             this.Offset = 0;
             this.Size = 0;
         }
    }
}