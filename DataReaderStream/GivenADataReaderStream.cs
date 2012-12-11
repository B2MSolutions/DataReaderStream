namespace Presentation.Framework.Silverlight.Facts
{
    using B2M;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Text;
    using Xunit;
    using Xunit.Extensions;

    public class GivenADataReaderStream
    {
        public GivenADataReaderStream()
        {
            this.DataReader = new Mock<IDataReader>();
            this.DataReader.Setup(dr => dr.Close()).Callback(() => this.DataReader.SetupGet(dr => dr.IsClosed).Returns(true));
            this.DataReader.Setup(dr => dr.Dispose()).Callback(() => this.DataReader.SetupGet(dr => dr.IsClosed).Returns(true));
            var fieldDelimiter = "\t";
            var endOfLineDelimiter = "\r\n";
            Encoding encoding = Encoding.UTF8;
            this.Reader = new DataReaderStream(this.DataReader.Object, fieldDelimiter, endOfLineDelimiter, encoding);
        }

        public static IEnumerable<object[]> WhenCallingReadAndHaveMultipleRowsShouldReturnCorrectStreamData
        {
            get
            {
                yield return new object[]
                { 
                    new object[]
                    {
                        new object[] { "r0c0", "r0c1" },
                        new object[] { "r1c0", "r1c1" }
                    }, 
                    22,
                    22,
                    "r0c0\tr0c1\r\nr1c0\tr1c1\r\n" 
                };

                yield return new object[]
                { 
                    new object[]
                    {
                        new object[] { "r0c0", "r0c1" },
                        new object[] { "r1c0", "r1c1" }
                    }, 
                    22,
                    27,
                    "r0c0\tr0c1\r\nr1c0\tr1c1\r\n\0\0\0\0\0" 
                };

                yield return new object[]
                { 
                    new object[]
                    {
                        new object[] { "r0c0", "r0c1" },
                    }, 
                    11,
                    11,
                    "r0c0\tr0c1\r\n" 
                };

                yield return new object[]
                { 
                    new object[]
                    {
                        new object[] { "r0c0", "r0c1" },
                    }, 
                    11,
                    20,
                    "r0c0\tr0c1\r\n\0\0\0\0\0\0\0\0\0" 
                };

                yield return new object[]
                { 
                    new object[]
                    {
                        new object[] { "ฃ0", "๘1" },
                    }, 
                    11,
                    17,
                    "ฃ0\t๘1\r\n\0\0\0\0\0\0" 
                };

                yield return new object[]
                {
                    new object[]
                    {
                        new object[] { "12345", "67890" }
                    },
                    12,
                    12,
                    "12345\t67890\r"
                };
            }
        }

        public DataReaderStream Reader { get; set; }

        private Mock<IDataReader> DataReader { get; set; }

        [Fact]
        public void WhenConstructingShouldNotThrow()
        {
        }

        [Fact]
        public void CanWriteShouldReturnFalse()
        {
            Assert.False(this.Reader.CanWrite);
        }

        [Fact]
        public void CanSeekShouldReturnFalse()
        {
            Assert.False(this.Reader.CanSeek);
        }

        [Fact]
        public void WhenDataReaderIsNullShouldThrow()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new DataReaderStream(null, "\t", "\r\n", Encoding.UTF8));
            Assert.Equal("dataReader", ex.ParamName);
        }

        [Fact]
        public void WhenFieldDelimiterIsNullShouldThrow()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new DataReaderStream(this.DataReader.Object, null, "\r\n", Encoding.UTF8));
            Assert.Equal("fieldDelimiter", ex.ParamName);
        }

        [Fact]
        public void WhenFieldDelimiterIsEmptyShouldThrow()
        {
            var ex = Assert.Throws<ArgumentException>(() => new DataReaderStream(this.DataReader.Object, string.Empty, "\r\n", Encoding.UTF8));
            Assert.Equal("fieldDelimiter", ex.ParamName);
        }

        [Fact]
        public void WhenEndOfLineDelimiterIsNullShouldThrow()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new DataReaderStream(this.DataReader.Object, "\t", null, Encoding.UTF8));
            Assert.Equal("endOfLineDelimiter", ex.ParamName);
        }

        [Fact]
        public void WhenEndOfLineDelimiterIsEmptyShouldThrow()
        {
            var ex = Assert.Throws<ArgumentException>(() => new DataReaderStream(this.DataReader.Object, "\t", string.Empty, Encoding.UTF8));
            Assert.Equal("endOfLineDelimiter", ex.ParamName);
        }

        [Fact]
        public void WhenEncodingIsNullShouldThrow()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new DataReaderStream(this.DataReader.Object, "\t", "\r\n", null));
            Assert.Equal("encoding", ex.ParamName);
        }

        [Fact]
        public void WhenCallingCanReadAfterCloseHasBeenCalledShouldReturnFalse()
        {
            this.Reader.Close();
            Assert.False(this.Reader.CanRead);
        }

        [Fact]
        public void WhenCallingReadWithSumOfOffsetAndCountLongerThanBufferLengthShouldThrow()
        {
            byte[] buffer = new byte[1];
            var offset = 0;
            var count = 2;
            var ex = Assert.Throws<ArgumentException>(() => this.Reader.Read(buffer, offset, count));
            Assert.Equal("buffer", ex.ParamName);
        }

        [Fact]
        public void WhenCallingReadWithNullBufferShouldThrow()
        {
            byte[] buffer = null;
            var offset = 0;
            var count = 1;
            var ex = Assert.Throws<ArgumentNullException>(() => this.Reader.Read(buffer, offset, count));
            Assert.Equal("buffer", ex.ParamName);
        }

        [Fact]
        public void WhenCallingReadWithNegativeOffsetShouldThrow()
        {
            byte[] buffer = new byte[12];
            var offset = -1;
            var count = 1;
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => this.Reader.Read(buffer, offset, count));
            Assert.Equal("offset", ex.ParamName);
        }

        [Fact]
        public void WhenCallingReadWithNegativeCountShouldThrow()
        {
            byte[] buffer = new byte[12];
            var offset = 0;
            var count = -1;
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => this.Reader.Read(buffer, offset, count));
            Assert.Equal("count", ex.ParamName);
        }

        [Fact]
        public void WhenCallingReadAfterStreamClosedShouldThrow()
        {
            this.Reader.Close();
            byte[] buffer = new byte[12];
            var offset = 0;
            var count = 1;
            var ex = Assert.Throws<ObjectDisposedException>(() => this.Reader.Read(buffer, offset, count));
            Assert.Equal(typeof(DataReaderStream).FullName, ex.ObjectName);
        }

        [Fact]
        public void WhenCallingReadAfterStreamDisposedShouldThrow()
        {
            this.Reader.Dispose();
            byte[] buffer = new byte[12];
            var offset = 0;
            var count = 1;
            var ex = Assert.Throws<ObjectDisposedException>(() => this.Reader.Read(buffer, offset, count));
            Assert.Equal(typeof(DataReaderStream).FullName, ex.ObjectName);
        }

        [Fact]
        public void WhenDataReaderIsClosedCanReadForStreamShouldReturnFalse()
        {
            this.DataReader.SetupGet(dr => dr.IsClosed).Returns(true);
            Assert.False(this.Reader.CanRead);
        }

        [Theory]
        [PropertyData("WhenCallingReadAndHaveMultipleRowsShouldReturnCorrectStreamData")]
        public void WhenCallingReadAndHaveMultipleRowsShouldReturnCorrectStream(
            object[] rows,
            int expectedBytesRead,
            int bufferSize,
            string expectedString)
        {
            var readCount = -1;
            this.DataReader.SetupGet(dr => dr.FieldCount).Returns((rows[0] as object[]).Length);
            this.DataReader.Setup(dr => dr.Read()).Returns(() => readCount + 1 < rows.Length).Callback(() =>
            {
                ++readCount;
                this.DataReader.Setup(dr => dr.GetValues(It.IsAny<object[]>()))
                               .Returns((rows[0] as object[]).Length)
                               .Callback<object[]>((o) =>
                               {
                                   for (int i = 0; i < ((object[])rows[readCount]).Length; i++)
                                   {
                                       o[i] = ((object[])rows[readCount])[i];
                                   }
                               });
            });

            var buffer = new byte[bufferSize];
            Assert.Equal(expectedBytesRead, this.Reader.Read(buffer, 0, buffer.Length));

            var bufferString = Encoding.UTF8.GetString(buffer);

            Assert.Equal(expectedString, bufferString);
        }

        [Fact]
        public void WhenCallingReadRepeatedlyWhenBufferIsSmallerThanRowShouldReturnCorrectStream()
        {
            this.DataReader.SetupGet(dr => dr.FieldCount).Returns(2);

            var readCount = 0;
            this.DataReader.Setup(dr => dr.Read())
                                          .Returns(() =>
                                              {
                                                  return readCount < 1;
                                              })
                                          .Callback(() =>
                                              {
                                                  ++readCount;
                                              });

            this.DataReader.Setup(dr => dr.GetValues(It.IsAny<object[]>()))
                                          .Returns(2)
                                          .Callback<object[]>((o) =>
                                              {
                                                  o[0] = "123";
                                                  o[1] = "4567";
                                              });

            var buffer = new byte[4];

            Assert.Equal(4, this.Reader.Read(buffer, 0, buffer.Length));
            var buffer1String = Encoding.UTF8.GetString(buffer);
            Assert.Equal("123\t", buffer1String);

            Assert.Equal(4, this.Reader.Read(buffer, 0, buffer.Length));
            var buffer2String = Encoding.UTF8.GetString(buffer);
            Assert.Equal("4567", buffer2String);

            Assert.Equal(2, this.Reader.Read(buffer, 0, buffer.Length));
            var buffer3String = Encoding.UTF8.GetString(buffer);
            Assert.Equal("\r\n67", buffer3String);
        }

        [Fact]
        public void WhenCallingCopyAndReturnRemainderWithLargeDestinationBufferShouldReturnExpectedWithNoRemainder()
        {
            var destination = new byte[4];
            var source = new byte[] { 0x01, 0x02 };
            var expectedDestination = new byte[] { 0x01, 0x02, 0x00, 0x00 };
            var expectedRemainder = new byte[0];

            var actualRemainder = this.Reader.CopyAndReturnRemainder(destination, 0, source.Length, source);
            Assert.Equal<byte[]>(expectedDestination, destination);
            Assert.Equal<byte[]>(expectedRemainder, actualRemainder.Item1);
        }

        [Fact]
        public void WhenCallingCopyAndReturnRemainderWithLargeSourceBufferShouldReturnExpectedWithRemainder()
        {
            var destination = new byte[2];
            var source = new byte[] { 0x01, 0x02, 0x03, 0x04 };
            var expectedDestination = new byte[] { 0x01, 0x02 };
            var expectedRemainder = new byte[] { 0x03, 0x04 };

            var actualRemainder = this.Reader.CopyAndReturnRemainder(destination, 0, 4, source);
            Assert.Equal<byte[]>(expectedDestination, destination);
            Assert.Equal<byte[]>(expectedRemainder, actualRemainder.Item1);
        }

        [Fact]
        public void WhenCallingCopyAndReturnRemainderWithCountSmallerThanSourceLengthShouldReturnExpectedWithRemainder()
        {
            var destination = new byte[4];
            var source = new byte[] { 0x01, 0x02, 0x03, 0x04 };
            var expectedDestination = new byte[] { 0x00, 0x01, 0x02, 0x00 };
            var expectedRemainder = new byte[] { 0x03, 0x04 };

            var actualRemainder = this.Reader.CopyAndReturnRemainder(destination, 1, 2, source);
            Assert.Equal<byte[]>(expectedDestination, destination);
            Assert.Equal<byte[]>(expectedRemainder, actualRemainder.Item1);
        }

        [Fact]
        public void WhenCallingCopyAndReturnRemainderWithCountBiggerThanSourceLengthShouldReturnExpectedWithRemainder()
        {
            var destination = new byte[4];
            var source = new byte[] { 0x01, 0x02, 0x03, 0x04 };
            var expectedDestination = new byte[] { 0x00, 0x01, 0x02, 0x03 };
            var expectedRemainder = new byte[] { 0x04 };

            var actualRemainder = this.Reader.CopyAndReturnRemainder(destination, 1, 5, source);
            Assert.Equal<byte[]>(expectedDestination, destination);
            Assert.Equal<byte[]>(expectedRemainder, actualRemainder.Item1);
        }

        [Fact]
        public void WhenCallingReadWithOffsetAndNotOverrunningCountThenShouldNotOverwriteStartOfBuffer()
        {
            var readReturnValue = true;

            this.DataReader.SetupGet(dr => dr.FieldCount).Returns(1);
            this.DataReader.Setup(dr => dr.Read()).Returns(() => readReturnValue).Callback(() => readReturnValue = false);
            this.DataReader.Setup(dr => dr.GetValues(It.IsAny<object[]>()))
                .Returns(1)
                .Callback<object[]>((o) => o[0] = 'a');

            var buffer = new byte[2];
            var offset = 1;
            var count = 1;
            Assert.Equal(1, this.Reader.Read(buffer, offset, count));

            Assert.Equal<byte[]>(new byte[] { 0x00, 0x61 }, buffer);
        }

        [Fact]
        public void WhenCallingReadWithOffsetAndNotOverrunningCountWithLongerDataThenShouldNotOverwriteStartOfBuffer()
        {
            var readReturnValue = true;

            this.DataReader.SetupGet(dr => dr.FieldCount).Returns(2);
            this.DataReader.Setup(dr => dr.Read()).Returns(() => readReturnValue).Callback(() => readReturnValue = false);
            this.DataReader.Setup(dr => dr.GetValues(It.IsAny<object[]>()))
                .Returns(2)
                .Callback<object[]>((o) =>
                    {
                        o[0] = "12345";
                        o[1] = "67890";
                    });

            var buffer = new byte[16];
            var offset = 3;
            var count = 13;
            Assert.Equal(13, this.Reader.Read(buffer, offset, count));

            Assert.Equal("\0\0\012345\t67890\r\n", Encoding.UTF8.GetString(buffer));
        }

        [Fact]
        public void WhenCallingReadWithOffsetAndUnderrunningCountThenShouldWriteToMiddleOfBuffer()
        {
            var readReturnValue = true;

            this.DataReader.SetupGet(dr => dr.FieldCount).Returns(2);
            this.DataReader.Setup(dr => dr.Read()).Returns(() => readReturnValue).Callback(() => readReturnValue = false);
            this.DataReader.Setup(dr => dr.GetValues(It.IsAny<object[]>()))
                .Returns(2)
                .Callback<object[]>((o) =>
                {
                    o[0] = "12345";
                    o[1] = "67890";
                });

            var buffer = new byte[16];
            var offset = 3;
            var count = 10;
            Assert.Equal(10, this.Reader.Read(buffer, offset, count));

            Assert.Equal("\0\0\012345\t6789\0\0\0", Encoding.UTF8.GetString(buffer));
        }

        [Fact]
        public void WhenReadDataHasASingleColumnShouldNotAppendFieldDelimiter()
        {
            var readReturnValue = true;

            this.DataReader.SetupGet(dr => dr.FieldCount).Returns(1);
            this.DataReader.Setup(dr => dr.Read()).Returns(() => readReturnValue).Callback(() => readReturnValue = false);
            this.DataReader.Setup(dr => dr.GetValues(It.IsAny<object[]>()))
                .Returns(1)
                .Callback<object[]>((o) =>
                {
                    o[0] = "12345";
                });

            var buffer = new byte[8];
            var offset = 0;
            var count = 8;
            Assert.Equal(7, this.Reader.Read(buffer, offset, count));

            Assert.Equal("12345\r\n\0", Encoding.UTF8.GetString(buffer));
        }

        [Fact]
        public void WhenReadDataHasAZeroColumnsShouldRead()
        {
            this.DataReader.SetupGet(dr => dr.FieldCount).Returns(0);
            this.DataReader.Setup(dr => dr.Read()).Returns(false);
            this.DataReader.Setup(dr => dr.GetValues(It.IsAny<object[]>()))
                           .Returns(0);

            var buffer = new byte[8];
            var offset = 0;
            var count = 8;
            Assert.Equal(0, this.Reader.Read(buffer, offset, count));

            Assert.Equal("\0\0\0\0\0\0\0\0", Encoding.UTF8.GetString(buffer));
        }
    }
}
