using System;
using System.IO;
using System.Threading;

namespace Mordex.Windows.DataGrid.Export
{
	/// <summary>
	/// Holds the written row XML until the styles are known. The data stays in memory up to
	/// <see cref="MemoryLimit"/> bytes and then moves to a temporary file that is deleted when the buffer is disposed.
	/// </summary>
	internal sealed class ExportRowBuffer : Stream
	{
		internal const int MemoryLimit = 8 * 1024 * 1024;

		private readonly int _memoryLimit;

		private Stream _store = new MemoryStream();

		private bool _disposed;

		internal ExportRowBuffer()
			: this(MemoryLimit)
		{
		}

		internal ExportRowBuffer(int memoryLimit)
		{
			_memoryLimit = memoryLimit;
		}

		internal bool IsOnDisk => _store is FileStream;

		public override bool CanRead => false;

		public override bool CanSeek => false;

		public override bool CanWrite => !_disposed;

		public override long Length => Store.Length;

		public override long Position
		{
			get { return Store.Position; }
			set { throw new NotSupportedException(); }
		}

		private Stream Store
		{
			get
			{
				ObjectDisposedException.ThrowIf(_disposed, this);
				return _store;
			}
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			Stream store = Store;
			if (store is MemoryStream memory && memory.Length + count > _memoryLimit)
			{
				store = MoveToDisk(memory);
			}
			store.Write(buffer, offset, count);
		}

		private Stream MoveToDisk(MemoryStream memory)
		{
			string path = Path.Combine(Path.GetTempPath(), "mordex-export-" + Guid.NewGuid().ToString("N") + ".tmp");
			FileStream file = new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None, 81920, FileOptions.DeleteOnClose | FileOptions.SequentialScan);
			try
			{
				memory.Position = 0;
				memory.CopyTo(file);
			}
			catch
			{
				file.Dispose();
				throw;
			}
			memory.Dispose();
			_store = file;
			return file;
		}

		public override void Flush()
		{
			Store.Flush();
		}

		/// <summary>
		/// Copies the bytes from <paramref name="start"/> to <paramref name="end"/> to the target. The token is checked
		/// before each block, so a cancel stops the copy with only part of the range written.
		/// </summary>
		internal void CopyRange(long start, long end, Stream target, CancellationToken cancellationToken)
		{
			Stream store = Store;
			store.Flush();
			long current = store.Position;
			byte[] buffer = new byte[81920];
			try
			{
				store.Position = start;
				long left = end - start;
				while (left > 0)
				{
					cancellationToken.ThrowIfCancellationRequested();
					int read = store.Read(buffer, 0, (int)Math.Min(buffer.Length, left));
					if (read <= 0)
					{
						throw new EndOfStreamException();
					}
					target.Write(buffer, 0, read);
					left -= read;
				}
			}
			finally
			{
				store.Position = current;
			}
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				_disposed = true;
				_store.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
