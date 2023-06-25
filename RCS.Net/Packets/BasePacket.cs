using RCS.Net.Tcp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RCS.Net.Packets
{
	public enum PacketType: byte
	{
		None = 0,
		Ping = 1,
		RequestCertificates = 2,
		TrustedCertificates = 3,
		RequestSignCertificate = 4,
		SignCertificate = 5,
	}
	[Serializable]
	public abstract class BasePacket
	{
		public float Version = 1;
		public Guid UID = Guid.NewGuid();
		public PacketType Type = PacketType.None;
		public object Data;

		[field: NonSerialized]
		public delegate void CallbackAnswer(BasePacket packet);
		[field: NonSerialized]
		public event CallbackAnswer CallbackAnswerEvent;
		public virtual byte[] Raw()
		{
			using (MemoryStream memoryStream = new MemoryStream())
			{
				BinaryFormatter formatter = new BinaryFormatter();
				formatter.Serialize(memoryStream, this);
				return memoryStream.ToArray();
			}
		}

		public virtual BasePacket FromRaw(byte[] array)
		{
			using (MemoryStream memoryStream = new MemoryStream(array))
			{
				BinaryFormatter formatter = new BinaryFormatter();
				return (BasePacket)formatter.Deserialize(memoryStream);
			}
		}

		public override string ToString()
		{
			return $"{Type};{UID};{Version};{Data}";
		}

		public virtual void Answer(BasePacket packet)
		{
			packet.UID = UID;
			CallbackAnswerEvent?.Invoke(packet);
		}
	}
}
