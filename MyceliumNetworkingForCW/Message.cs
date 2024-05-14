using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MyceliumNetworking
{
	// https://github.com/tom-weiland/tcp-udp-networking/blob/master/UnityGameServer/Assets/Scripts/message.cs
	public class Message : IDisposable
	{
		// Max size of a message, in bytes
		public static int MaxSize { get; private set; } = Steamworks.Constants.k_cbMaxSteamNetworkingSocketsMessageSizeSend;

		public string GetDestination() => $"{ModID}: {MethodName}";

		const int MYCELIUM_VERSION = 1;

		public byte MyceliumVersion;
		public int Mask;
		public uint ModID;
		public string MethodName;

		private List<byte> buffer;
		private byte[] readableBuffer;
		private int readPos;

		/// <summary>Creates a new message with a given ID. Used for sending.</summary>
		public Message(uint modID, string methodName, int mask)
		{
			buffer = new List<byte>(); // Initialize buffer
			readPos = 0; // Set readPos to 0

			this.MyceliumVersion = MYCELIUM_VERSION;
			this.ModID = modID;
			this.MethodName = methodName;
			this.Mask = mask;

			WriteByte(MyceliumVersion);
			WriteUInt(ModID);
			WriteString(MethodName);
			WriteInt(Mask);
		}

		/// <summary>Creates a message from which data can be read. Used for receiving.</summary>
		/// <param name="data">The bytes to add to the message.</param>
		public Message(byte[] data)
		{
			buffer = new List<byte>(); // Initialize buffer
			readPos = 0; // Set readPos to 0

			SetBytes(data);

			MyceliumVersion = ReadByte();
			ModID = ReadUInt();
			MethodName = ReadString();
			Mask = ReadInt();
		}

		#region Functions
		/// <summary>Sets the message's content and prepares it to be read.</summary>
		/// <param name="data">The bytes to add to the message.</param>
		public void SetBytes(byte[] data)
		{
			buffer.AddRange(data);
			readableBuffer = buffer.ToArray();
		}

		/// <summary>Inserts the length of the message's content at the start of the buffer.</summary>
		public void WriteLength()
		{
			buffer.InsertRange(0, BitConverter.GetBytes(buffer.Count)); // Insert the byte length of the message at the very beginning
		}

		/// <summary>Gets the message's content in array form.</summary>
		public byte[] ToArray()
		{
			readableBuffer = buffer.ToArray();
			return readableBuffer;
		}

		/// <summary>Gets the length of the message's content.</summary>
		public int Length()
		{
			return buffer.Count; // Return the length of buffer
		}

		/// <summary>Gets the length of the unread data contained in the message.</summary>
		public int UnreadLength()
		{
			return Length() - readPos; // Return the remaining length (unread)
		}

		/// <summary>Resets the message instance to allow it to be reused.</summary>
		/// <param name="shouldReset">Whether or not to reset the message.</param>
		public void Reset(bool shouldReset = true)
		{
			if(shouldReset)
			{
				buffer.Clear(); // Clear buffer
				readableBuffer = null;
				readPos = 0; // Reset readPos
			}
			else
			{
				readPos -= 4; // "Unread" the last read int
			}
		}
		#endregion

		#region Write Data
		/// <summary>Adds a byte to the message.</summary>
		/// <param name="value">The byte to add.</param>
		public Message WriteByte(byte value)
		{
			buffer.Add(value);
			return this;
		}
		/// <summary>Adds an array of bytes to the message.</summary>
		/// <param name="value">The byte array to add.</param>
		public Message WriteBytes(byte[] value)
		{
			WriteInt(value.Length);
			buffer.AddRange(value);
			return this;
		}
		/// <summary>Adds an array of bytes to the message.</summary>
		/// <param name="value">The byte array to add.</param>
		public Message WriteBools(bool[] value)
		{
			WriteInt(value.Length);
			byte[] bytes = new byte[value.Length];
			for(int i = 0; i < bytes.Length; i++)
			{
				bytes[i] = value[i] ? (byte)1 : (byte)0;
			}
			buffer.AddRange(bytes);
			return this;
		}
		/// <summary>Adds a short to the message.</summary>
		/// <param name="value">The short to add.</param>
		public Message WriteShort(short value)
		{
			buffer.AddRange(BitConverter.GetBytes(value));
			return this;
		}
		/// <summary>Adds a ushort to the message.</summary>
		/// <param name="value">The ushort to add.</param>
		public Message WriteUShort(ushort value)
		{
			buffer.AddRange(BitConverter.GetBytes(value));
			return this;
		}
		/// <summary>Adds an int to the message.</summary>
		/// <param name="value">The int to add.</param>
		public Message WriteInt(int value)
		{
			buffer.AddRange(BitConverter.GetBytes(value));
			return this;
		}
		/// <summary>Adds a uint to the message.</summary>
		/// <param name="value">The uint to add.</param>
		public Message WriteUInt(uint value)
		{
			buffer.AddRange(BitConverter.GetBytes(value));
			return this;
		}
		/// <summary>Inserts the given int at the start of the message (after the header).</summary>
		/// <param name="value">The int to insert.</param>
		public Message InsertInt(int value)
		{
			buffer.InsertRange(1, BitConverter.GetBytes(value));
			return this;
		}
		/// <summary>Adds a long to the message.</summary>
		/// <param name="value">The long to add.</param>
		public Message WriteLong(long value)
		{
			buffer.AddRange(BitConverter.GetBytes(value));
			return this;
		}
		/// <summary>Adds a ulong to the message.</summary>
		/// <param name="value">The ulong to add.</param>
		public Message WriteULong(ulong value)
		{
			buffer.AddRange(BitConverter.GetBytes(value));
			return this;
		}
		/// <summary>Adds a float to the message.</summary>
		/// <param name="value">The float to add.</param>
		public Message WriteFloat(float value)
		{
			buffer.AddRange(BitConverter.GetBytes(value));
			return this;
		}
		/// <summary>Adds a bool to the message.</summary>
		/// <param name="value">The bool to add.</param>
		public Message WriteBool(bool value)
		{
			buffer.AddRange(BitConverter.GetBytes(value));
			return this;
		}
		/// <summary>Adds a string to the message.</summary>
		/// <param name="value">The string to add.</param>
		public Message WriteString(string value)
		{
			var bytes = Encoding.UTF8.GetBytes(value);
			WriteInt(bytes.Length); // Add the length of the byte array to the message
			buffer.AddRange(bytes); // Add the string itself
			return this;
		}
		/// <summary>Adds a Vector3 to the message.</summary>
		/// <param name="value">The Vector3 to add.</param>
		public Message WriteVector3(Vector3 value)
		{
			WriteFloat(value.x);
			WriteFloat(value.y);
			WriteFloat(value.z);
			return this;
		}
		/// <summary>Adds a Quaternion to the message.</summary>
		/// <param name="value">The Quaternion to add.</param>
		public Message WriteQuaternion(Quaternion value)
		{
			WriteFloat(value.x);
			WriteFloat(value.y);
			WriteFloat(value.z);
			WriteFloat(value.w);
			return this;
		}

		/// <summary>Reads an object from the message.</summary>
		public void WriteObject(Type type, object value)
		{
			if(WriteCasters.TryGetValue(type, out var write))
			{
				write(this, value);
			}
			else
			{
				throw new Exception($"Could not write value of type '{type}' as it is unsupported!");
			}
		}

		Dictionary<Type, Action<Message, object>> WriteCasters = new Dictionary<Type, Action<Message, object>>
		{
			{ typeof(byte), (Message msg, object value) => { msg.WriteByte((byte)value); } },
			{ typeof(byte[]), (Message msg, object value) => { msg.WriteBytes((byte[])value); } },
			{ typeof(bool), (Message msg, object value) => { msg.WriteBool((bool)value); } },
			{ typeof(bool[]), (Message msg, object value) => { msg.WriteBools((bool[])value); } },
			{ typeof(int), (Message msg, object value) => { msg.WriteInt((int)value); } },
			{ typeof(uint), (Message msg, object value) => { msg.WriteUInt((uint)value); } },
			{ typeof(short), (Message msg, object value) => { msg.WriteShort((short)value); } },
			{ typeof(ushort), (Message msg, object value) => { msg.WriteUShort((ushort)value); } },
			{ typeof(long), (Message msg, object value) => { msg.WriteLong((long)value); } },
			{ typeof(ulong), (Message msg, object value) => { msg.WriteULong((ulong)value); } },
			{ typeof(float), (Message msg, object value) => { msg.WriteFloat((float)value); } },
			{ typeof(string), (Message msg, object value) => { msg.WriteString((string)value); } },
			{ typeof(Vector3), (Message msg, object value) => { msg.WriteVector3((Vector3)value); } },
			{ typeof(Quaternion), (Message msg, object value) => { msg.WriteQuaternion((Quaternion)value); } },
			{ typeof(CSteamID), (Message msg, object value) => { msg.WriteULong(((CSteamID)value).m_SteamID); } },
		};
		#endregion

		#region Read Data
		/// <summary>Reads a byte from the message.</summary>
		public byte ReadByte()
		{
			if(buffer.Count > readPos)
			{
				// If there are unread bytes
				byte value = readableBuffer[readPos]; // Get the byte at readPos' position
													  // If moveReadPos is true
				readPos += 1; // Increase readPos by 1
				return value; // Return the byte
			}
			else
			{
				throw new Exception("Could not read value of type 'byte'!");
			}
		}

		/// <summary>Reads an array of bytes from the message.</summary>
		public byte[] ReadBytes()
		{
			int length = ReadInt();

			if(length == 0)
				return new byte[0];

			if(buffer.Count > readPos)
			{
				// If there are unread bytes
				byte[] value = buffer.GetRange(readPos, length).ToArray(); // Get the bytes at readPos' position with a range of length
																		   // If moveReadPos is true
				readPos += length; // Increase readPos by length
				return value; // Return the bytes
			}
			else
			{
				throw new Exception("Could not read value of type 'byte[]'!");
			}
		}

		public bool[] ReadBools()
		{
			int length = ReadInt();
			if(buffer.Count > readPos)
			{
				// If there are unread bytes
				bool[] value = new bool[length];
				for(int i = 0; i < length; i++)
				{
					byte current = buffer[readPos + i];
					value[i] = current == 1;
				}

				readPos += length; // Increase readPos by length
				return value; // Return the bytes
			}
			else
			{
				throw new Exception("Could not read value of type 'bool[]'!");
			}
		}


		/// <summary>Reads a short from the message.</summary>
		public short ReadShort()
		{
			if(buffer.Count > readPos)
			{
				// If there are unread bytes
				short value = BitConverter.ToInt16(readableBuffer, readPos); // Convert the bytes to a short
																			 // If moveReadPos is true and there are unread bytes
				readPos += 2; // Increase readPos by 2
				return value; // Return the short
			}
			else
			{
				throw new Exception("Could not read value of type 'short'!");
			}
		}

		/// <summary>Reads a ushort from the message.</summary>
		public ushort ReadUShort()
		{
			if(buffer.Count > readPos)
			{
				// If there are unread bytes
				ushort value = BitConverter.ToUInt16(readableBuffer, readPos); // Convert the bytes to a short
																			   // If moveReadPos is true and there are unread bytes
				readPos += 2; // Increase readPos by 2
				return value; // Return the short
			}
			else
			{
				throw new Exception("Could not read value of type 'short'!");
			}
		}

		/// <summary>Reads an int from the message.</summary>
		public int ReadInt()
		{
			if(buffer.Count > readPos)
			{
				// If there are unread bytes
				int value = BitConverter.ToInt32(readableBuffer, readPos); // Convert the bytes to an int
																		   // If moveReadPos is true
				readPos += 4; // Increase readPos by 4
				return value; // Return the int
			}
			else
			{
				throw new Exception("Could not read value of type 'int'!");
			}
		}

		/// <summary>Reads a uint from the message.</summary>
		public uint ReadUInt()
		{
			if(buffer.Count > readPos)
			{
				// If there are unread bytes
				uint value = BitConverter.ToUInt32(readableBuffer, readPos); // Convert the bytes to an uint
																			 // If moveReadPos is true
				readPos += 4; // Increase readPos by 4
				return value; // Return the uint
			}
			else
			{
				throw new Exception("Could not read value of type 'uint'!");
			}
		}

		/// <summary>Reads a long from the message.</summary>
		public long ReadLong()
		{
			if(buffer.Count > readPos)
			{
				// If there are unread bytes
				long value = BitConverter.ToInt64(readableBuffer, readPos); // Convert the bytes to a long
				readPos += 8; // Increase readPos by 8
				return value; // Return the long
			}
			else
			{
				throw new Exception("Could not read value of type 'long'!");
			}
		}

		/// <summary>Reads a ulong from the message.</summary>
		public ulong ReadULong()
		{
			if(buffer.Count > readPos)
			{
				// If there are unread bytes
				ulong value = BitConverter.ToUInt64(readableBuffer, readPos); // Convert the bytes to a long
				readPos += 8; // Increase readPos by 8
				return value; // Return the ulong
			}
			else
			{
				throw new Exception("Could not read value of type 'ulong'!");
			}
		}

		/// <summary>Reads a float from the message.</summary>
		public float ReadFloat()
		{
			if(buffer.Count > readPos)
			{
				// If there are unread bytes
				float value = BitConverter.ToSingle(readableBuffer, readPos); // Convert the bytes to a float
				readPos += 4; // Increase readPos by 4
				return value; // Return the float
			}
			else
			{
				throw new Exception("Could not read value of type 'float'!");
			}
		}

		/// <summary>Reads a bool from the message.</summary>
		public bool ReadBool()
		{
			if(buffer.Count > readPos)
			{
				// If there are unread bytes
				bool value = BitConverter.ToBoolean(readableBuffer, readPos); // Convert the bytes to a bool
				readPos += 1; // Increase readPos by 1
				return value; // Return the bool
			}
			else
			{
				throw new Exception("Could not read value of type 'bool'!");
			}
		}

		/// <summary>Reads a string from the message.</summary>
		public string ReadString()
		{
			try
			{
				int length = ReadInt(); // Get the length of the byte array (NOT the length of the string)
				string value = Encoding.UTF8.GetString(readableBuffer, readPos, length); // Convert the bytes to a string
				if(value.Length > 0)
				{
					// string is not empty
					readPos += length; // Increase readPos by the length of the string
				}
				return value; // Return the string
			}
			catch
			{
				throw new Exception("Could not read value of type 'string'!");
			}
		}

		/// <summary>Reads a Vector3 from the message.</summary>
		public Vector3 ReadVector3()
		{
			return new Vector3(ReadFloat(), ReadFloat(), ReadFloat());
		}

		/// <summary>Reads a Quaternion from the message.</summary>
		public Quaternion ReadQuaternion()
		{
			return new Quaternion(ReadFloat(), ReadFloat(), ReadFloat(), ReadFloat());
		}

		/// <summary>Reads an object from the message.</summary>
		public object ReadObject(Type type)
		{
			if(ReadCasters.TryGetValue(type, out var read))
			{
				return read(this);
			}
			else
			{
				throw new Exception($"Could not read value of type '{type}' as it is unsupported!");
			}
		}

		Dictionary<Type, Func<Message, object>> ReadCasters = new Dictionary<Type, Func<Message, object>>
		{
			{ typeof(byte), (Message msg) => { return msg.ReadByte(); } },
			{ typeof(byte[]), (Message msg) => { return msg.ReadBytes(); } },
			{ typeof(bool), (Message msg) => { return msg.ReadBool(); } },
			{ typeof(bool[]), (Message msg) => { return msg.ReadBools(); } },
			{ typeof(int), (Message msg) => { return msg.ReadInt(); } },
			{ typeof(uint), (Message msg) => { return msg.ReadUInt(); } },
			{ typeof(short), (Message msg) => { return msg.ReadShort(); } },
			{ typeof(ushort), (Message msg) => { return msg.ReadUShort(); } },
			{ typeof(long), (Message msg) => { return msg.ReadLong(); } },
			{ typeof(ulong), (Message msg) => { return msg.ReadULong(); } },
			{ typeof(float), (Message msg) => { return msg.ReadFloat(); } },
			{ typeof(string), (Message msg) => { return msg.ReadString(); } },
			{ typeof(Vector3), (Message msg) => { return msg.ReadVector3(); } },
			{ typeof(Quaternion), (Message msg) => { return msg.ReadQuaternion(); } },
			{ typeof(CSteamID), (Message msg) => { return (CSteamID)msg.ReadULong(); } },
		};
		#endregion

		public int GetSize()
		{
			return readableBuffer.Length;
		}

		private bool disposed = false;

		protected virtual void Dispose(bool disposing)
		{
			if(!disposed)
			{
				if(disposing)
				{
					buffer = null;
					readableBuffer = null;
					readPos = 0;
				}

				disposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
