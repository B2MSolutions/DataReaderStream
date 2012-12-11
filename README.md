DataReaderStream
================

A Stream implementation over an IDataReader

Pair programmed by [Steve Wood](http://github.com/sjwood) and [James Bloomer](http://github.com/jamesbloomer)

### Usage

``` C#
// Create a System.Data.Common.DBCommand in the standard way to get a DataReader
var reader = command.ExecuteReader();
var dataStream = new DataReaderStream(reader, "\t", "\r\n", Encoding.UTF8);
```