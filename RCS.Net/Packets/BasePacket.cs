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
		RSAGetKeys,
		RSAConfirm,
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

		public virtual byte[] Raw(RSAParameters publicKey)
		{
			if (IsKeyEmpty(publicKey))
			{
				return Raw();
			}
			using (MemoryStream memoryStream = new MemoryStream())
			{
				BinaryFormatter formatter = new BinaryFormatter();
				formatter.Serialize(memoryStream, this);
				var data = memoryStream.ToArray();
				return EncryptWithRSA(data, publicKey);
			}
		}

		public virtual BasePacket FromRaw(byte[] array, RSAParameters privateKey)
		{
			if (IsKeyEmpty(privateKey))
			{
				return FromRaw(array);
			}
			var data = DecryptWithRSA(array, privateKey);
			using (MemoryStream memoryStream = new MemoryStream(data))
			{
				BinaryFormatter formatter = new BinaryFormatter();
				return (BasePacket)formatter.Deserialize(memoryStream);
			}
		}
		public static bool IsKeyEmpty(RSAParameters key)
		{
			return key.Equals(new RSAParameters());
		}
		private byte[] EncryptWithRSA(byte[] data, RSAParameters publicKey)
		{
			using (var rsa = new RSACryptoServiceProvider())
			{
				rsa.ImportParameters(publicKey);
				// Генерируем симметричный ключ AES
				using (var aes = Aes.Create())
				{
					aes.GenerateKey();
					// Шифруем данные с помощью AES
					byte[] encryptedData = EncryptWithAES(data, aes.Key);

					// Шифруем симметричный ключ AES с помощью RSA
					byte[] encryptedKey = rsa.Encrypt(aes.Key, false);

					// Объединяем зашифрованный ключ и зашифрованные данные
					byte[] encryptedResult = new byte[encryptedKey.Length + encryptedData.Length];
					Buffer.BlockCopy(encryptedKey, 0, encryptedResult, 0, encryptedKey.Length);
					Buffer.BlockCopy(encryptedData, 0, encryptedResult, encryptedKey.Length, encryptedData.Length);

					return encryptedResult;
				}
			}
		}

		private byte[] DecryptWithRSA(byte[] encryptedData, RSAParameters privateKey)
		{
			using (var rsa = new RSACryptoServiceProvider())
			{
				rsa.ImportParameters(privateKey);

				// Разделяем зашифрованный ключ и зашифрованные данные
				byte[] encryptedKey = new byte[rsa.KeySize / 8];
				byte[] encryptedDataOnly = new byte[encryptedData.Length - encryptedKey.Length];
				Buffer.BlockCopy(encryptedData, 0, encryptedKey, 0, encryptedKey.Length);
				Buffer.BlockCopy(encryptedData, encryptedKey.Length, encryptedDataOnly, 0, encryptedDataOnly.Length);

				// Расшифровываем симметричный ключ AES с помощью RSA
				byte[] decryptedKey = rsa.Decrypt(encryptedKey, false);

				// Расшифровываем данные с помощью AES
				byte[] decryptedData = DecryptWithAES(encryptedDataOnly, decryptedKey);

				return decryptedData;
			}
		}

		private byte[] EncryptWithAES(byte[] data, byte[] key)
		{
			using (var aes = Aes.Create())
			{
				aes.Key = key;
				aes.Mode = CipherMode.CBC;
				aes.GenerateIV();

				using (var encryptor = aes.CreateEncryptor())
				using (var memoryStream = new MemoryStream())
				{
					memoryStream.Write(aes.IV, 0, aes.IV.Length);

					using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
					{
						cryptoStream.Write(data, 0, data.Length);
						cryptoStream.FlushFinalBlock();
					}

					return memoryStream.ToArray();
				}
			}
		}

		private byte[] DecryptWithAES(byte[] encryptedData, byte[] key)
		{
			using (var aes = Aes.Create())
			{
				aes.Key = key;
				aes.Mode = CipherMode.CBC;

				byte[] iv = new byte[aes.IV.Length];
				Buffer.BlockCopy(encryptedData, 0, iv, 0, iv.Length);
				aes.IV = iv;

				using (var decryptor = aes.CreateDecryptor())
				using (var memoryStream = new MemoryStream())
				{
					using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Write))
					{
						cryptoStream.Write(encryptedData, iv.Length, encryptedData.Length - iv.Length);
						cryptoStream.FlushFinalBlock();
					}

					return memoryStream.ToArray();
				}
			}
		}
	}
}
