using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Permissions;
using System.Text;

[assembly: AssemblyCopyright("Copyright ©  2013")]
[assembly: AssemblyTitle("Lz4Net")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("Lz4Net")]
[assembly: TargetFramework(".NETFramework,Version=v4.5", FrameworkDisplayName = ".NET Framework 4.5")]
[assembly: AssemblyTrademark("")]
[assembly: ComVisible(false)]
[assembly: Guid("9cdd9009-ae01-44ff-8b18-b3a8f39ec838")]
[assembly: AssemblyFileVersion("1.0.94")]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[assembly: AssemblyVersion("1.0.2.0")]
namespace Lz4Net
{
	/// <summary>
	/// Stream to compress and write back data
	/// </summary>
	public sealed class Lz4DecompressionStream : Stream
	{
		/// <summary>
		/// Room for header
		/// </summary>
		private const int HeaderSize = 8;

		/// <summary>
		/// Stream we're reading from
		/// </summary>
		private Stream m_targetStream;

		/// <summary>
		/// Temporary buffer where raw data is stored.
		/// Kept to be reused from one buffer fill to another
		/// </summary>
		private byte[] m_readBuffer;

		/// <summary>
		/// Unpacked buffer
		/// </summary>
		private byte[] m_unpackedBuffer;

		/// <summary>
		/// Read position un unpacked buffer
		/// </summary>
		private int m_unpackedOffset;

		/// <summary>
		/// Length for unpacked data
		/// </summary>
		private int m_unpackedLength;

		private readonly byte[] m_header = new byte[8];

		private readonly bool m_closeStream;

		/// <summary>
		/// Gets a value indicating whether the current stream supports reading.
		/// </summary>
		/// <value></value>
		/// <returns>true if the stream supports reading; otherwise, false.
		/// </returns>
		public override bool CanRead => true;

		/// <summary>
		/// Gets a value indicating whether the current stream supports seeking.
		/// </summary>
		/// <value></value>
		/// <returns>true if the stream supports seeking; otherwise, false.
		/// </returns>
		public override bool CanSeek => false;

		/// <summary>
		/// Gets a value indicating whether the current stream supports writing.
		/// </summary>
		/// <value></value>
		/// <returns>true if the stream supports writing; otherwise, false.
		/// </returns>
		public override bool CanWrite => false;

		/// <summary>[NotSupported]
		/// Gets the length in bytes of the stream.
		/// </summary>
		/// <value></value>
		/// <returns>
		/// A long value representing the length of the stream in bytes.
		/// </returns>
		/// <exception cref="T:System.NotSupportedException">
		/// A class derived from Stream does not support seeking.
		/// </exception>
		/// <exception cref="T:System.ObjectDisposedException">
		/// Methods were called after the stream was closed.
		/// </exception>
		public override long Length
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>[NotSupported]
		/// Gets or sets the position within the current stream.
		/// </summary>
		/// <value></value>
		/// <returns>
		/// The current position within the stream.
		/// </returns>
		/// <exception cref="T:System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		/// <exception cref="T:System.NotSupportedException">
		/// The stream does not support seeking.
		/// </exception>
		/// <exception cref="T:System.ObjectDisposedException">
		/// Methods were called after the stream was closed.
		/// </exception>
		public override long Position
		{
			get
			{
				throw new NotSupportedException();
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Clears all buffers for this stream and causes any buffered data to be written to the underlying device.
		/// </summary>
		/// <exception cref="T:System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		public override void Flush()
		{
		}

		/// <summary>[NotSupported]
		/// Sets the position within the current stream.
		/// </summary>
		/// <param name="offset">A byte offset relative to the <paramref name="origin" /> parameter.</param>
		/// <param name="origin">A value of type <see cref="T:System.IO.SeekOrigin" /> indicating the reference point used to obtain the new position.</param>
		/// <returns>
		/// The new position within the current stream.
		/// </returns>
		/// <exception cref="T:System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		/// <exception cref="T:System.NotSupportedException">
		/// The stream does not support seeking, such as if the stream is constructed from a pipe or console output.
		/// </exception>
		/// <exception cref="T:System.ObjectDisposedException">
		/// Methods were called after the stream was closed.
		/// </exception>
		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Sets the length of the current stream.
		/// </summary>
		/// <param name="value">The desired length of the current stream in bytes.</param>
		/// <exception cref="T:System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		/// <exception cref="T:System.NotSupportedException">
		/// The stream does not support both writing and seeking, such as if the stream is constructed from a pipe or console output.
		/// </exception>
		/// <exception cref="T:System.ObjectDisposedException">
		/// Methods were called after the stream was closed.
		/// </exception>
		public override void SetLength(long value)
		{
			m_targetStream.SetLength(value);
		}

		/// <summary>[NotSupported]
		/// Writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
		/// </summary>
		/// <param name="buffer">An array of bytes. This method copies <paramref name="count" /> bytes from <paramref name="buffer" /> to the current stream.</param>
		/// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin copying bytes to the current stream.</param>
		/// <param name="count">The number of bytes to be written to the current stream.</param>
		/// <exception cref="T:System.ArgumentException">
		/// The sum of <paramref name="offset" /> and <paramref name="count" /> is greater than the buffer length.
		/// </exception>
		/// <exception cref="T:System.ArgumentNullException">
		/// 	<paramref name="buffer" /> is null.
		/// </exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		/// 	<paramref name="offset" /> or <paramref name="count" /> is negative.
		/// </exception>
		/// <exception cref="T:System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		/// <exception cref="T:System.NotSupportedException">
		/// The stream does not support writing.
		/// </exception>
		/// <exception cref="T:System.ObjectDisposedException">
		/// Methods were called after the stream was closed.
		/// </exception>
		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Fills or refills the read buffer.
		/// </summary>
		private void Fill()
		{
			switch (m_targetStream.Read(m_header, 0, 8))
			{
			case 0:
				m_unpackedBuffer = null;
				break;
			default:
				throw new InvalidDataException("input buffer corrupted (header)");
			case 8:
			{
				int compressedSize = Lz4.GetCompressedSize(m_header);
				if (m_readBuffer == null || m_readBuffer.Length < compressedSize + 8)
				{
					m_readBuffer = new byte[compressedSize + 8];
				}
				Buffer.BlockCopy(m_header, 0, m_readBuffer, 0, 8);
				int num = m_targetStream.Read(m_readBuffer, 8, compressedSize);
				if (num != compressedSize)
				{
					throw new InvalidDataException("input buffer corrupted (body)");
				}
				m_unpackedLength = Lz4.Decompress(m_readBuffer, 0, ref m_unpackedBuffer);
				m_unpackedOffset = 0;
				break;
			}
			}
		}

		/// <summary>
		/// Reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
		/// </summary>
		/// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset" /> and (<paramref name="offset" /> + <paramref name="count" /> - 1) replaced by the bytes read from the current source.</param>
		/// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin storing the data read from the current stream.</param>
		/// <param name="count">The maximum number of bytes to be read from the current stream.</param>
		/// <returns>
		/// The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.
		/// </returns>
		/// <exception cref="T:System.ArgumentException">
		/// The sum of <paramref name="offset" /> and <paramref name="count" /> is larger than the buffer length.
		/// </exception>
		/// <exception cref="T:System.ArgumentNullException">
		/// 	<paramref name="buffer" /> is null.
		/// </exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		/// 	<paramref name="offset" /> or <paramref name="count" /> is negative.
		/// </exception>
		/// <exception cref="T:System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		/// <exception cref="T:System.NotSupportedException">
		/// The stream does not support reading.
		/// </exception>
		/// <exception cref="T:System.ObjectDisposedException">
		/// Methods were called after the stream was closed.
		/// </exception>
		public override int Read(byte[] buffer, int offset, int count)
		{
			if (m_unpackedBuffer == null || m_unpackedOffset == m_unpackedLength)
			{
				Fill();
			}
			if (m_unpackedBuffer == null)
			{
				return 0;
			}
			if (m_unpackedOffset + count > m_unpackedLength)
			{
				int num = m_unpackedLength - m_unpackedOffset;
				int num2 = Read(buffer, offset, num);
				int num3 = Read(buffer, offset + num, count - num);
				return num2 + num3;
			}
			Buffer.BlockCopy(m_unpackedBuffer, m_unpackedOffset, buffer, offset, count);
			m_unpackedOffset += count;
			return count;
		}

		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="T:System.IO.Stream" /> and optionally releases the managed resources.
		/// </summary>
		/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Flush();
			}
			base.Dispose(disposing);
			if (m_closeStream && m_targetStream != null)
			{
				m_targetStream.Dispose();
			}
			m_targetStream = null;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="!:ZlibDecompressionStream" /> class.
		/// </summary>
		/// <param name="sourceStream">The source stream.</param>
		/// <param name="compression">The compression.</param>
		/// <param name="closeStream">The close stream.</param>
		public Lz4DecompressionStream(Stream sourceStream, bool closeStream = false)
		{
			m_closeStream = closeStream;
			m_targetStream = sourceStream;
			Fill();
		}
	}
	/// <summary>
	/// Stream to compress and write back data
	/// </summary>
	public sealed class Lz4CompressionStream : Stream
	{
		/// <summary>
		/// The Stream we're writing to
		/// </summary>
		private Stream m_targetStream;

		/// <summary>
		/// Write buffer
		/// </summary>
		private readonly byte[] m_writeBuffer;

		/// <summary>
		/// Current position in write buffer
		/// </summary>
		private int m_writeBufferOffset;

		/// <summary>
		/// Buffer where compressed data is stored
		/// </summary>
		private byte[] m_compressedBuffer;

		/// <summary>
		/// If the target stream should be close on Dispose
		/// </summary>
		private readonly bool m_closeStream;

		/// <summary>
		/// The selected compression Move
		/// </summary>
		private Lz4Mode m_compressionMode;

		/// <summary>
		/// When overridden in a derived class, gets a value indicating whether the current stream supports reading.
		/// </summary>
		/// <returns>true if the stream supports reading; otherwise, false.
		/// </returns>
		/// <value></value>
		public override bool CanRead => false;

		/// <summary>
		/// Gets a value indicating whether the current stream supports seeking.
		/// </summary>
		/// <returns>true if the stream supports seeking; otherwise, false.
		/// </returns>
		/// <value></value>
		public override bool CanSeek => false;

		/// <summary>
		/// Gets a value indicating whether the current stream supports writing.
		/// </summary>
		/// <returns>true if the stream supports writing; otherwise, false.
		/// </returns>
		/// <value></value>
		public override bool CanWrite => true;

		/// <summary>[NotSupported]
		/// Gets the length in bytes of the stream.
		/// </summary>
		/// <returns>
		/// A long value representing the length of the stream in bytes.
		/// </returns>
		/// <exception cref="T:System.NotSupportedException">
		/// A class derived from Stream does not support seeking.
		/// </exception>
		/// <exception cref="T:System.ObjectDisposedException">
		/// Methods were called after the stream was closed.
		/// </exception>
		/// <value></value>
		public override long Length
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>[NotSupported]
		/// Gets or sets the position within the current stream.
		/// </summary>
		/// <returns>
		/// The current position within the stream.
		/// </returns>
		/// <exception cref="T:System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		/// <exception cref="T:System.NotSupportedException">
		/// The stream does not support seeking.
		/// </exception>
		/// <exception cref="T:System.ObjectDisposedException">
		/// Methods were called after the stream was closed.
		/// </exception>
		/// <value></value>
		public override long Position
		{
			get
			{
				throw new NotSupportedException();
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>[NotSupported]
		/// Sets the position within the current stream.
		/// </summary>
		/// <param name="offset">A byte offset relative to the <paramref name="origin" /> parameter.</param>
		/// <param name="origin">A value of type <see cref="T:System.IO.SeekOrigin" /> indicating the reference point used to obtain the new position.</param>
		/// <exception cref="T:System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		/// <exception cref="T:System.NotSupportedException">
		/// The stream does not support seeking, such as if the stream is constructed from a pipe or console output.
		/// </exception>
		/// <exception cref="T:System.ObjectDisposedException">
		/// Methods were called after the stream was closed.
		/// </exception>
		/// <returns>The new position within the current stream.</returns>
		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Sets the length of the current stream.
		/// </summary>
		/// <param name="value">The desired length of the current stream in bytes.</param>
		/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
		/// <exception cref="T:System.NotSupportedException">The stream does not support
		/// both writing and seeking, such as if the stream is constructed from a pipe or
		/// console output. </exception>
		/// <exception cref="T:System.ObjectDisposedException">Methods were called after
		/// the stream was closed. </exception>
		public override void SetLength(long value)
		{
			m_targetStream.SetLength(value);
		}

		/// <summary>[NotSupported]
		/// Reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
		/// </summary>
		/// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset" /> and (<paramref name="offset" /> + <paramref name="count" /> - 1) replaced by the bytes read from the current source.</param>
		/// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin storing the data read from the current stream.</param>
		/// <param name="count">The maximum number of bytes to be read from the current stream.</param>
		/// <exception cref="T:System.ArgumentException">
		/// The sum of <paramref name="offset" /> and <paramref name="count" /> is larger than the buffer length.
		/// </exception>
		/// <exception cref="T:System.ArgumentNullException">
		/// <paramref name="buffer" /> is null.
		/// </exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		/// <paramref name="offset" /> or <paramref name="count" /> is negative.
		/// </exception>
		/// <exception cref="T:System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		/// <exception cref="T:System.NotSupportedException">
		/// The stream does not support reading.
		/// </exception>
		/// <exception cref="T:System.ObjectDisposedException">
		/// Methods were called after the stream was closed.
		/// </exception>
		/// <returns>
		/// The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.
		/// </returns>
		public override int Read(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Clears all buffers for this stream and causes any buffered data to be written to the underlying device.
		/// </summary>
		/// <exception cref="T:System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		public override void Flush()
		{
			if (m_writeBufferOffset > 0)
			{
				int count = Lz4.Compress(m_writeBuffer, 0, m_writeBufferOffset, ref m_compressedBuffer, m_compressionMode);
				m_targetStream.Write(m_compressedBuffer, 0, count);
				m_writeBufferOffset = 0;
			}
		}

		/// <summary>
		/// Writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
		/// </summary>
		/// <param name="buffer">An array of bytes. This method copies <paramref name="count" /> bytes from <paramref name="buffer" /> to the current stream.</param>
		/// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin copying bytes to the current stream.</param>
		/// <param name="count">The number of bytes to be written to the current stream.</param>
		/// <exception cref="T:System.ArgumentException">
		/// The sum of <paramref name="offset" /> and <paramref name="count" /> is greater than the buffer length.
		/// </exception>
		/// <exception cref="T:System.ArgumentNullException">
		/// <paramref name="buffer" /> is null.
		/// </exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		/// <paramref name="offset" /> or <paramref name="count" /> is negative.
		/// </exception>
		/// <exception cref="T:System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		/// <exception cref="T:System.NotSupportedException">
		/// The stream does not support writing.
		/// </exception>
		/// <exception cref="T:System.ObjectDisposedException">
		/// Methods were called after the stream was closed.
		/// </exception>
		public override void Write(byte[] buffer, int offset, int count)
		{
			int num = m_writeBuffer.Length - m_writeBufferOffset;
			if (count <= num)
			{
				Buffer.BlockCopy(buffer, offset, m_writeBuffer, m_writeBufferOffset, count);
				m_writeBufferOffset += count;
				if (num == 0)
				{
					Flush();
				}
			}
			else
			{
				Write(buffer, offset, num);
				Write(buffer, offset + num, count - num);
			}
		}

		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="T:System.IO.Stream" /> and optionally releases the managed resources.
		/// </summary>
		/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Flush();
			}
			base.Dispose(disposing);
			if (m_closeStream && m_targetStream != null)
			{
				m_targetStream.Dispose();
			}
			m_targetStream = null;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="!:ZlibCompressionStream" /> class.
		/// </summary>
		/// <param name="targetStream">The target stream.</param>
		/// <param name="writeBuffer">The write buffer.</param>
		/// <param name="compressionBuffer">The compression buffer.</param>
		/// <param name="closeStream">The close stream.</param>
		public Lz4CompressionStream(Stream targetStream, byte[] writeBuffer, byte[] compressionBuffer, Lz4Mode mode = Lz4Mode.Fast, bool closeStream = false)
		{
			m_closeStream = closeStream;
			m_targetStream = targetStream;
			m_writeBuffer = writeBuffer;
			m_compressedBuffer = compressionBuffer;
			m_compressionMode = mode;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="!:ZlibCompressionStream" /> class.
		/// </summary>
		/// <param name="targetStream">The target.</param>
		/// <param name="bufferSize">Size of the buffer.</param>
		/// <param name="closeStream">The close stream.</param>
		public Lz4CompressionStream(Stream targetStream, int bufferSize, Lz4Mode mode = Lz4Mode.Fast, bool closeStream = false)
			: this(targetStream, new byte[bufferSize], new byte[Lz4.LZ4_compressBound(bufferSize)], mode, closeStream)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="!:ZlibCompressionStream" /> class.
		/// </summary>
		/// <param name="targetStream">The target.</param>
		/// <param name="closeStream">The close stream.</param>
		public Lz4CompressionStream(Stream targetStream, Lz4Mode mode = Lz4Mode.Fast, bool closeStream = false)
			: this(targetStream, 262144, mode, closeStream)
		{
		}
	}
	public static class Lz4
	{
		[DllImport("x86\\lz4X86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "dll_LZ4_compress")]
		private unsafe static extern int LZ4_compress_x86(byte* source, byte* destination, int size);

		[DllImport("x86\\lz4X86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "dll_LZ4_compressHC")]
		private unsafe static extern int LZ4_compressHC_x86(byte* source, byte* destination, int size);

		[DllImport("x86\\lz4X86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "dll_LZ4_uncompress")]
		private unsafe static extern int LZ4_uncompress_x86(byte* source, byte* destination, int size);

		[DllImport("x64\\lz4X64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "dll_LZ4_compress")]
		private unsafe static extern int LZ4_compress_x64(byte* source, byte* destination, int size);

		[DllImport("x64\\lz4X64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "dll_LZ4_compressHC")]
		private unsafe static extern int LZ4_compressHC_x64(byte* source, byte* destination, int size);

		[DllImport("x64\\lz4X64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "dll_LZ4_uncompress")]
		private unsafe static extern int LZ4_uncompress_x64(byte* source, byte* destination, int size);

		/// <summary>
		/// Native method call (PInvoke) to the LZ4_compress method.
		/// The platform target (X86 or X64) is chosen at runtime. Description:
		/// Compresses 'isize' bytes from 'source' into 'dest'.
		/// Destination buffer must be already allocated,
		/// and must be sized to handle worst cases situations (input data not compressible)
		/// Worst case size evaluation is provided by function LZ4_compressBound().
		/// note : destination buffer must be already allocated. 
		/// To avoid any problem, size it to handle worst cases situations (input data not compressible)
		/// Worst case size evaluation is provided by function LZ4_compressBound() (see "lz4.h")
		/// </summary>
		/// <param name="source">The source (input).</param>
		/// <param name="destination">The destination (output). Its memory must alredy be allocated, use LZ4_compressBound for the size hint.</param>
		/// <param name="size">The source size. Max supported value is ~1.9GB.</param>
		/// <returns>The number of bytes written in buffer destination.</returns>
		public unsafe static int LZ4_compress(byte* source, byte* destination, int size)
		{
			if (IntPtr.Size != 4)
			{
				return LZ4_compress_x64(source, destination, size);
			}
			return LZ4_compress_x86(source, destination, size);
		}

		/// <summary>
		/// Native method call (PInvoke) to the LZ4_compressHC method. 
		/// It provide a High Compression mode that is slower but with a better compression rate.
		/// The platform target (X86 or X64) is chosen at runtime. Description:
		/// Compresses 'isize' bytes from 'source' into 'dest'.
		/// Destination buffer must be already allocated,
		/// and must be sized to handle worst cases situations (input data not compressible)
		/// Worst case size evaluation is provided by function LZ4_compressBound().
		/// note : destination buffer must be already allocated. 
		/// To avoid any problem, size it to handle worst cases situations (input data not compressible)
		/// Worst case size evaluation is provided by function LZ4_compressBound() (see "lz4.h")
		/// </summary>
		/// <param name="source">The source (input).</param>
		/// <param name="destination">The destination (output). Its memory must alredy be allocated, use LZ4_compressBound for the size hint.</param>
		/// <param name="size">The source size. Max supported value is ~1.9GB.</param>
		/// <returns>The number of bytes written in buffer destination.</returns>
		public unsafe static int LZ4_compressHC(byte* source, byte* destination, int size)
		{
			if (IntPtr.Size != 4)
			{
				return LZ4_compressHC_x64(source, destination, size);
			}
			return LZ4_compressHC_x86(source, destination, size);
		}

		/// <summary>
		/// Native method call (PInvoke) to the LZ4_uncompress method.
		/// The platform target (X86 or X64) is chosen at runtime. Description:
		/// note : destination buffer must be already allocated.
		/// its size must be a minimum of 'osize' bytes.
		/// </summary>
		/// <param name="source">The source (input).</param>
		/// <param name="destination">The destination (output). Its memory must alredy be allocated, use LZ4_compressBound for the size hint.</param>
		/// <param name="originalSize">Size of the original buffer. Is the output size, therefore the original size.</param>
		/// <returns>the number of bytes read in the source buffer (in other words, the compressed size)
		/// If the source stream is malformed, the function will stop decoding and return a negative result, indicating the byte position of the faulty instruction
		/// This function never writes outside of provided buffers, and never modifies input buffer.</returns>
		public unsafe static int LZ4_uncompress(byte* source, byte* destination, int originalSize)
		{
			if (IntPtr.Size != 4)
			{
				return LZ4_uncompress_x64(source, destination, originalSize);
			}
			return LZ4_uncompress_x86(source, destination, originalSize);
		}

		/// <summary>
		/// Provides the maximum size that LZ4 may output in a "worst case" scenario (input data not compressible)
		/// primarily useful for memory allocation of output buffer.
		/// </summary>
		/// <param name="isize">is the input size. Max supported value is ~1.9GB.</param>
		/// <returns>maximum output size in a "worst case" scenario.</returns>
		public static int LZ4_compressBound(int isize)
		{
			return isize + isize / 255 + 16;
		}

		/// <summary>
		/// Compresses the byte buffer.
		/// This method stores a 8-byte header to store the original and compressed buffer size.
		/// </summary>
		/// <param name="data">The data to be compressed.</param>
		/// <param name="mode">The compression mode [Fast, HighCompression].</param>
		/// <returns>The compressed byte array</returns>
		public static byte[] CompressBytes(byte[] data, Lz4Mode mode = Lz4Mode.Fast)
		{
			return CompressBytes(data, 0, data.Length, mode);
		}

		/// <summary>
		/// Compresses the byte buffer.
		/// This method stores a 8-byte header to store the original and compressed buffer size.
		/// </summary>
		/// <param name="data">The data to be compressed.</param>
		/// <param name="offset">The offset.</param>
		/// <param name="length">The length.</param>
		/// <param name="mode">The compression mode [Fast, HighCompression].</param>
		/// <returns>The compressed byte array</returns>
		/// <exception cref="T:System.ArgumentOutOfRangeException">If length if outside data array bounds</exception>
		public static byte[] CompressBytes(byte[] data, int offset, int length, Lz4Mode mode)
		{
			if (length > data.Length)
			{
				throw new ArgumentOutOfRangeException("length");
			}
			byte[] buffer = null;
			int num = Compress(data, 0, length, ref buffer, mode);
			byte[] array = new byte[num];
			Buffer.BlockCopy(buffer, 0, array, 0, num);
			return array;
		}

		/// <summary>
		/// Compresses the byte buffer.
		/// This method stores a 8-byte header to store the original and compressed buffer size.
		/// </summary>
		/// <param name="data">The data to be compressed.</param>
		/// <param name="offset">The offset.</param>
		/// <param name="length">The length.</param>
		/// <param name="buffer">The compression buffer. If the buffer is null or the size is insuficient, a new array will be created.</param>
		/// <param name="mode">The compression mode [Fast, HighCompression].</param>
		/// <returns>The compressed byte array</returns>
		public unsafe static int Compress(byte[] data, int offset, int length, ref byte[] buffer, Lz4Mode mode)
		{
			int num = LZ4_compressBound(length) + 8;
			if (buffer == null || buffer.Length < num)
			{
				buffer = new byte[num];
			}
			int num2;
			fixed (byte* source = &data[offset])
			{
				fixed (byte* destination = &buffer[8])
				{
					num2 = ((mode == Lz4Mode.Fast) ? LZ4_compress(source, destination, length) : LZ4_compressHC(source, destination, length));
					byte* ptr = (byte*)(&length);
					buffer[0] = *ptr;
					buffer[1] = ptr[1];
					buffer[2] = ptr[2];
					buffer[3] = ptr[3];
					ptr = (byte*)(&num2);
					buffer[4] = *ptr;
					buffer[5] = ptr[1];
					buffer[6] = ptr[2];
					buffer[7] = ptr[3];
				}
			}
			return num2 + 8;
		}

		/// <summary>
		/// Decompresses the byte buffer compressed by a Lz4.CompressBytes or Lz4.Compress method.
		/// This method uses the byte array header info to correctly prepare the output buffer.
		/// </summary>
		/// <param name="data">The compressed data returned by a Lz4.CompressBytes or Lz4.Compress method.</param>
		/// <returns>The uncompressed buffer</returns>
		public static byte[] DecompressBytes(byte[] data)
		{
			byte[] buffer = null;
			Decompress(data, 0, ref buffer);
			return buffer;
		}

		/// <summary>
		/// Decompresses the byte buffer compressed by a Lz4.CompressBytes or Lz4.Compress method.
		/// This method uses the byte array header info to correctly prepare the output buffer.
		/// </summary>
		/// <param name="data">The compressed data returned by a Lz4.CompressBytes or Lz4.Compress method.</param>
		/// <param name="offset">The data buffer offset.</param>
		/// <param name="buffer">The decompression buffer. If the buffer is null or the size is insuficient, a new array will be created.</param>
		/// <returns>Uncompressed data size</returns>
		/// <exception cref="T:System.Exception">Input data is incomplete. Data header info size is lesser than total array size</exception>
		public unsafe static int Decompress(byte[] data, int offset, ref byte[] buffer)
		{
			int num;
			fixed (byte* ptr = &data[offset])
			{
				if (data.Length < *(int*)(ptr + 4))
				{
					throw new Exception("Input data is incomplete. Total data array size is lesser than header info size. Data array could be incomplete or was not generated by 'CompressBytes' or 'Compress'.");
				}
				num = *(int*)ptr;
				if (buffer == null || buffer.Length < num)
				{
					buffer = new byte[num];
				}
				fixed (byte* destination = &buffer[0])
				{
					LZ4_uncompress(ptr + 8, destination, num);
				}
			}
			return num;
		}

		/// <summary>
		/// Compresses the specified text and return a Base64 enconded string.
		/// </summary>
		/// <param name="text">The text.</param>
		/// <param name="mode">The compression mode [Fast, HighCompression].</param>
		/// <returns>The compressed text as a Base64 enconded string</returns>
		public static string CompressString(string text, Lz4Mode mode = Lz4Mode.Fast)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(text);
			byte[] inArray = CompressBytes(bytes, 0, bytes.Length, mode);
			return Convert.ToBase64String(inArray);
		}

		/// <summary>
		/// Decompresses the specified compressed text by the Lz4.CompressString method.
		/// </summary>
		/// <param name="compressedText">The compressed text.</param>
		/// <returns>The decompressed string</returns>
		/// <exception cref="T:System.Exception">Input data is incomplete or was not generated by 'CompressString'.</exception>
		public static string DecompressString(string compressedText)
		{
			byte[] data = Convert.FromBase64String(compressedText);
			byte[] bytes = DecompressBytes(data);
			return Encoding.UTF8.GetString(bytes);
		}

		/// <summary>
		/// Size of the compressed buffer returned by a Lz4.CompressBytes or Lz4.CompressBytesHC method.
		/// </summary>
		/// <param name="data">The buffer returned by a Lz4.CompressBytes or Lz4.CompressBytesHC method.</param>
		/// <returns></returns>
		public unsafe static int GetCompressedSize(byte[] data)
		{
			if (data != null && data.Length >= 8)
			{
				fixed (byte* ptr = &data[4])
				{
					return *(int*)ptr;
				}
			}
			return 0;
		}

		/// <summary>
		/// Size of the original (uncompressed) buffer returned by a Lz4.CompressBytes or Lz4.CompressBytesHC method.
		/// </summary>
		/// <param name="data">The buffer returned by a Lz4.CompressBytes or Lz4.CompressBytesHC method.</param>
		/// <returns></returns>
		public unsafe static int GetUncompressedSize(byte[] data)
		{
			if (data != null && data.Length >= 8)
			{
				fixed (byte* ptr = &data[0])
				{
					return *(int*)ptr;
				}
			}
			return 0;
		}
	}
	public enum Lz4Mode
	{
		/// <summary>
		/// The very fast Lz4 algorithm implemtation.
		/// </summary>
		Fast,
		/// <summary>
		/// A High Compression mode that is slower but with a better compression rate.
		/// </summary>
		HighCompression
	}
}
