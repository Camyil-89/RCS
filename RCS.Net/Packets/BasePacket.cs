using RCS.Net.Tcp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RCS.Net.Packets
{
	public enum PacketType : byte
	{
		None,
		Ping,
		RequestCertificates,
		RequestCertificate,
		TrustedCertificates,
		ValidatingCertificate,
		RequestSignCertificate,
		SignCertificate,
		InitialConnect,
		ConfirmConnect,
		RSTStopwatch,
		Disconnect,
		FirewallBLock,
	}
	[Serializable]
	public class RCSMagma
	{
		public byte[] KEY;
		public byte[] IV = new byte[8];
		public bool IV_Mode = true;

		public RCSMagma()
		{
			Generate();
		}
		public void Generate()
		{
			var magma = OpenGost.Security.Cryptography.Magma.Create();
			magma.GenerateIV();
			magma.GenerateKey();
			KEY = magma.Key;
			if (IV_Mode)
				IV = magma.IV;
			else
			{
				for (int i = 0; i < 8; i++)
				{
					IV[i] = 0xff;
				}
			}
		}
		public OpenGost.Security.Cryptography.Magma GetMagma()
		{
			var magma = OpenGost.Security.Cryptography.Magma.Create();
			magma.IV = IV;
			magma.Key = KEY;

			return magma;
		}
		public byte[] Encrypt(byte[] data)
		{
			var magma = GetMagma();

			using (var encryptor = magma.CreateEncryptor())
			{
				return PerformCryptography(data, encryptor);
			}
		}
		private byte[] PerformCryptography(byte[] data, ICryptoTransform cryptoTransform)
		{
			using (var ms = new MemoryStream())
			using (var cryptoStream = new CryptoStream(ms, cryptoTransform, CryptoStreamMode.Write))
			{
				cryptoStream.Write(data, 0, data.Length);
				cryptoStream.FlushFinalBlock();

				return ms.ToArray();
			}
		}
		public byte[] Decrypt(byte[] data)
		{
			var magma = GetMagma();

			using (var decryptor = magma.CreateDecryptor(magma.Key, magma.IV))
			{
				return PerformCryptography(data, decryptor);
			}
		}
	}

	[Serializable]
	public abstract class BasePacket
	{
		public BasePacket()
		{
			Guid guid = Guid.NewGuid();
			byte[] bytes = guid.ToByteArray();
			UID = BitConverter.ToInt32(bytes, 0);
		}
		public byte Version = 1;
		public int UID;
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
			if (packet.Type == PacketType.RSTStopwatch ||
				packet.Type == PacketType.FirewallBLock)
				packet.Data = null;
			CallbackAnswerEvent?.Invoke(packet);
		}

		public virtual byte[] Raw(RCSMagma magma)
		{
			using (MemoryStream memoryStream = new MemoryStream())
			{
				BinaryFormatter formatter = new BinaryFormatter();
				formatter.Serialize(memoryStream, this);
				var data = memoryStream.ToArray();
				return magma.Encrypt(data);
			}
		}

		public virtual BasePacket FromRaw(byte[] array, RCSMagma magma)
		{
			var data = magma.Decrypt(array);
			using (MemoryStream memoryStream = new MemoryStream(data))
			{
				BinaryFormatter formatter = new BinaryFormatter();
				return (BasePacket)formatter.Deserialize(memoryStream);
			}
		}
		public virtual byte[] Raw(byte[] publicKey)
		{
			using (MemoryStream memoryStream = new MemoryStream())
			{
				BinaryFormatter formatter = new BinaryFormatter();
				formatter.Serialize(memoryStream, this);
				var data = memoryStream.ToArray();
				return EncryptWithMagma(data, publicKey);
			}
		}

		public virtual BasePacket FromRaw(byte[] array, byte[] privateKey)
		{
			var data = DecryptWithMagma(array, privateKey);
			using (MemoryStream memoryStream = new MemoryStream(data))
			{
				BinaryFormatter formatter = new BinaryFormatter();
				return (BasePacket)formatter.Deserialize(memoryStream);
			}
		}
		public static bool IsKeyEmpty(byte[] key)
		{
			return key == null || key.Length == 0;
		}
		private byte[] EncryptWithMagma(byte[] data, byte[] publicKey)
		{
			using (var rsa = new RSACryptoServiceProvider())
			{
				rsa.ImportRSAPublicKey(publicKey, out _);

				RCSMagma magma = new RCSMagma();
				magma.IV_Mode = false;
				magma.Generate();
				byte[] encryptedData = magma.Encrypt(data);

				// Шифруем симметричный ключ AES с помощью RSA
				byte[] encryptedKey = rsa.Encrypt(magma.KEY, false);
				// Объединяем зашифрованный ключ и зашифрованные данные
				byte[] encryptedResult = new byte[encryptedKey.Length + encryptedData.Length];
				Buffer.BlockCopy(encryptedKey, 0, encryptedResult, 0, encryptedKey.Length);
				Buffer.BlockCopy(encryptedData, 0, encryptedResult, encryptedKey.Length, encryptedData.Length);

				return encryptedResult;
			}
		}

		private byte[] DecryptWithMagma(byte[] encryptedData, byte[] privateKey)
		{
			using (var rsa = new RSACryptoServiceProvider())
			{
				rsa.ImportRSAPrivateKey(privateKey, out _);

				byte[] encryptedKey = new byte[rsa.KeySize / 8];
				byte[] encryptedDataOnly = new byte[encryptedData.Length - encryptedKey.Length];
				Buffer.BlockCopy(encryptedData, 0, encryptedKey, 0, encryptedKey.Length);
				Buffer.BlockCopy(encryptedData, encryptedKey.Length, encryptedDataOnly, 0, encryptedDataOnly.Length);

				byte[] decryptedKey = rsa.Decrypt(encryptedKey, false);

				RCSMagma magma = new RCSMagma();
				magma.IV_Mode = false;
				magma.Generate();
				magma.KEY = decryptedKey;

				byte[] decryptedData = magma.Decrypt(encryptedDataOnly);

				return decryptedData;
			}
		}
	}
}
