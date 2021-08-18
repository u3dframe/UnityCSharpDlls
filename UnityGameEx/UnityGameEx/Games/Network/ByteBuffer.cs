using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
#if ToLua
using LuaInterface;
#endif

namespace TNet {
    public class ByteBuffer {
        MemoryStream stream = null;
        BinaryWriter writer = null;
        BinaryReader reader = null;

        public bool m_isCurWrite = false;

        private ByteBuffer() { }

        private void InitMS(){
            if(stream == null) {
                stream = new MemoryStream();
            }
        }

        public ByteBuffer InitWriter(){
            this.InitMS();
            this.Clear();
            if(writer == null) {
                writer = new BinaryWriter(stream);
            }
            m_isCurWrite = true;
            return this;
        }


        public ByteBuffer InitReader(byte[] data){
            this.InitMS();
            this.Clear();
            if(reader == null) {
                reader = new BinaryReader(stream);
            }
            if(data != null && data.Length > 0) {
                int _lens = data.Length;
                stream.Write(data,0,_lens);
            }
            return this;
        }

        public ByteBuffer InitReader(){
            return InitReader(null);
        }

        public void Close() {
            if (writer != null) writer.Close();
            if (reader != null) reader.Close();

            stream.Close();
            writer = null;
            reader = null;
            stream = null;
        }

        public void WriteByte(byte v) {
            writer.Write(v);
        }

        public void WriteInt(int v) {
            writer.Write((int)v);
        }

        public void WriteShort(ushort v) {
            writer.Write((ushort)v);
        }

        public void WriteLong(long v) {
            writer.Write((long)v);
        }

        public void WriteFloat(float v) {
            byte[] temp = BitConverter.GetBytes(v);
            Array.Reverse(temp);
            writer.Write(BitConverter.ToSingle(temp, 0));
        }

        public void WriteDouble(double v) {
            byte[] temp = BitConverter.GetBytes(v);
            Array.Reverse(temp);
            writer.Write(BitConverter.ToDouble(temp, 0));
        }

        public void WriteBytes(byte[] v) {
            writer.Write((int)v.Length);
            writer.Write(v);
        }

        public void WriteBytes(byte[] v, int length) {
            writer.Write(length);
            writer.Write(v, 0, length);
        }

        public void WriteString(string v) {
            byte[] bytes = Encoding.UTF8.GetBytes(v);
            WriteBytes(bytes);
        }

#if ToLua
        public void WriteBuffer(LuaByteBuffer strBuffer) {
           WriteBytes(strBuffer.buffer);
        }
#endif
        public byte ReadByte() {
            return reader.ReadByte();
        }

        public int ReadInt() {
            return (int)reader.ReadInt32();
        }

        public ushort ReadShort() {
            return (ushort)reader.ReadInt16();
        }

        public long ReadLong() {
            return (long)reader.ReadInt64();
        }

        public float ReadFloat() {
            byte[] temp = BitConverter.GetBytes(reader.ReadSingle());
            Array.Reverse(temp);
            return BitConverter.ToSingle(temp, 0);
        }

        public double ReadDouble() {
            byte[] temp = BitConverter.GetBytes(reader.ReadDouble());
            Array.Reverse(temp);
            return BitConverter.ToDouble(temp, 0);
        }

        public byte[] ReadBytes(bool isShort) {
            int len = isShort ? ReadShort() : ReadInt();
            return reader.ReadBytes(len);
        }

        public byte[] ReadBytes() {
            return ReadBytes(false);
        }

        public string ReadString() {
            byte[] buffer = ReadBytes();
            return Encoding.UTF8.GetString(buffer);
        }

#if ToLua
        public LuaByteBuffer ReadBuffer() {
            byte[] bytes = ReadBytes();
            return new LuaByteBuffer(bytes);
        }
#endif

        public byte[] ToBytes() {
            writer.Flush();
            return stream.ToArray();
        }

        public void Flush() {
            writer.Flush();
        }

        public ByteBuffer ToReader(){
            if(m_isCurWrite){
                if(reader == null) {
                    reader = new BinaryReader(stream);
                }
                this.Clear(false);
            }
            return this;
        }

        protected void Clear(bool isAll = true){
            m_isCurWrite = false;
            if (writer != null) writer.Seek(0,SeekOrigin.Begin);
            if (reader != null) stream.Seek(0,SeekOrigin.Begin);
            if (isAll && stream != null) {
                // stream.Position = 0;
                // stream.Length = 0;
                stream.Seek(0, SeekOrigin.Begin);
                stream.SetLength(0);
            }
        }

#if ToLua
		[NoToLua]
		public LuaByteBuffer ReadBufferLua() {
			int lens = (int)(stream.Length - stream.Position);
			byte[] bytes = reader.ReadBytes(lens);
			return new LuaByteBuffer(bytes);
		}
#endif

        static readonly private Queue<ByteBuffer> _pools = new Queue<ByteBuffer>();

        static private ByteBuffer Borrow(){
            lock(_pools){
                if(_pools.Count > 0){
                    return _pools.Dequeue();
                }
            }
            return new ByteBuffer();
        }

        static public void ReBack(ByteBuffer btBuffer){
            if(btBuffer == null)
                return;
            btBuffer.Clear();
            lock(_pools){
                _pools.Enqueue(btBuffer);
            }
        }

        static public ByteBuffer BuildWriter(){
            ByteBuffer ret = Borrow();
            return ret.InitWriter();
        }

        static public ByteBuffer BuildReader(byte[] data){
            ByteBuffer ret = Borrow();
            return ret.InitReader(data);
        }
    }
}