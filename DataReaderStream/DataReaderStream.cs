namespace B2M
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.IO;
    using System.Linq;
    using System.Text;

    public class DataReaderStream : Stream
    {
        public DataReaderStream(IDataReader dataReader, string fieldDelimiter, string endOfLineDelimiter, Encoding encoding)
        {
            if (dataReader == null)
            {
                throw new ArgumentNullException("dataReader", "dataReader cannot be null");
            }

            if (fieldDelimiter == null)
            {
                throw new ArgumentNullException("fieldDelimiter", "fieldDelimiter must be a non-null and non-empty string");
            }

            if (fieldDelimiter == string.Empty)
            {
                throw new ArgumentException("fieldDelimiter must be a non-null and non-empty string", "fieldDelimiter");
            }

            if (endOfLineDelimiter == null)
            {
                throw new ArgumentNullException("endOfLineDelimiter", "endOfLineDelimiter must be a non-null and non-empty string");
            }

            if (endOfLineDelimiter == string.Empty)
            {
                throw new ArgumentException("endOfLineDelimiter must be a non-null and non-empty string", "endOfLineDelimiter");
            }

            if (encoding == null)
            {
                throw new ArgumentNullException("encoding", "encoding cannot be null");
            }

            this.DataReader = dataReader;
            this.FieldDelimiter = fieldDelimiter;
            this.EndOfLineDelimiter = endOfLineDelimiter;
            this.Encoding = encoding;
            this.Remainder = new byte[0];
        }

        public override bool CanRead
        {
            get
            {
                return !this.DataReader.IsClosed;
            }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        private IDataReader DataReader { get; set; }

        private string FieldDelimiter { get; set; }

        private string EndOfLineDelimiter { get; set; }

        private Encoding Encoding { get; set; }

        private byte[] Remainder { get; set; }

        public override void Close()
        {
            this.DataReader.Close();
            base.Close();
        }

        public override void Flush()
        {
            throw new System.NotImplementedException();
        }

        public override long Length
        {
            get { throw new System.NotImplementedException(); }
        }

        public override long Position
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
                throw new System.NotImplementedException();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer", "Buffer cannot be null");
            }

            if (buffer.Length < offset + count)
            {
                throw new ArgumentException("Buffer is too small", "buffer");
            }

            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", "Offset cannot be negative");
            }
            
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", "Count cannot be negative");
            }

            if (this.DataReader.IsClosed)
            {
                throw new ObjectDisposedException(this.GetType().FullName, "Stream is closed");
            }

            var values = new object[this.DataReader.FieldCount];
            var readBytes = 0;

            if (this.Remainder.Length > 0)
            {
                var newRemainder = this.CopyAndReturnRemainder(buffer, offset, count, this.Remainder);
                this.Remainder = newRemainder.Item1;
                readBytes += newRemainder.Item2;

                if (this.Remainder.Length > 0)
                {
                    return readBytes;
                }
            };

            while (this.DataReader.Read() && (readBytes + offset) < buffer.Length && readBytes < count)
            {
                this.DataReader.GetValues(values);
                var row = values
                    .Select(o => Convert.ToString(o))
                    .Aggregate((l, r) => l + this.FieldDelimiter + r);

                row += this.EndOfLineDelimiter;

                var rowBytes = this.Encoding.GetBytes(row);

                var remainder = this.CopyAndReturnRemainder(buffer, (readBytes + offset), count, rowBytes);
                this.Remainder = remainder.Item1;
                readBytes += remainder.Item2;
            }

            return readBytes;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new System.NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new System.NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new System.NotImplementedException();
        }

        internal Tuple<byte[], int> CopyAndReturnRemainder(byte[] destination, int destinationOffset, int count, byte[] source)
        {
            var numberOfBytesToCopy = Math.Min(count, source.Length);

            var remainderLength = Math.Max(0, source.Length - count);
            if (count > (destination.Length - destinationOffset))
            {
                numberOfBytesToCopy = Math.Min(numberOfBytesToCopy, (destination.Length - destinationOffset));
                remainderLength = source.Length - numberOfBytesToCopy;
            }

            Buffer.BlockCopy(source, 0, destination, destinationOffset, numberOfBytesToCopy);

            var remainder = new byte[remainderLength];
            Buffer.BlockCopy(source, numberOfBytesToCopy, remainder, 0, remainderLength);

            return new Tuple<byte[],int>(remainder, numberOfBytesToCopy);
        }
    }
}