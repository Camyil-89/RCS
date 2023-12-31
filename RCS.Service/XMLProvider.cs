﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RCS.Service
{
	public static class XmlProvider
	{
		public static string PathToSave = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\RCS";
		public static string PathToTrustedCertificates = $"{PathToSave}\\TrustedCertificates";
		public static readonly string NameFileCertificateInZip = $"RCS_Certificate_metadata.xml";
		public static readonly string NameFileCertificateSignInZip = $"RCS_Certificate_metadata_sign.xml";
		public static bool CompressZip = true;

		public static T LoadInzip<T>(string path_zip, string path_in_zip)
		{
			using (var archive = ZipFile.Open(path_zip, ZipArchiveMode.Read))
			{
				var file = archive.GetEntry(path_in_zip);
				using (var stream = file.Open())
				{
					XmlSerializer xmls = new XmlSerializer(typeof(T));
					var x = (T)xmls.Deserialize(stream);
					return x;
				}
			}
		}
		public static void DeleteEntryzip(string path_zip, string path_in_zip)
		{
			using (var archive = ZipFile.Open(path_zip, ZipArchiveMode.Update))
			{
				var demoFile = archive.GetEntry(path_in_zip);
				if (demoFile != null)
					demoFile.Delete();
			}
		}
		public static void SaveInzip<T>(string path_zip, string path_in_zip, T obj)
		{
			using (var archive = ZipFile.Open(path_zip, ZipArchiveMode.Update))
			{
				var demoFile = archive.GetEntry(path_in_zip);
				if (demoFile == null)
					demoFile = archive.CreateEntry(path_in_zip);
				else
				{
					demoFile.Delete();
					demoFile = archive.CreateEntry(path_in_zip);
				}
				using (var entryStream = demoFile.Open())
				using (var sw = new StreamWriter(entryStream))
				{
					XmlSerializer xmls = new XmlSerializer(typeof(T));
					xmls.Serialize(sw, obj);
				}
			}
		}
		public static byte[] ReadInZip(string path_zip, string path_in_zip)
		{
			using (var archive = ZipFile.Open(path_zip, ZipArchiveMode.Read))
			{
				var file = archive.GetEntry(path_in_zip);
				using (var stream = file.Open())
				{
					byte[] bytes;
					using (var memoryStream = new MemoryStream())
					{
						stream.CopyTo(memoryStream);
						bytes = memoryStream.ToArray();
					}
					return bytes;
				}
			}
		}
		public static void WriteInZip(string path_zip, string path_in_zip, byte[] data)
		{
			using (var archive = ZipFile.Open(path_zip, ZipArchiveMode.Update))
			{
				var demoFile = archive.GetEntry(path_in_zip);
				if (demoFile == null)
					demoFile = archive.CreateEntry(path_in_zip);
				else
				{
					demoFile.Delete();
					demoFile = archive.CreateEntry(path_in_zip);
				}
				using (var entryStream = demoFile.Open())
					entryStream.Write(data);
			}
		}
		public static void Save<T>(string path, object obj)
		{
			if (CompressZip)
			{
				using (StreamWriter file_write = new StreamWriter(path))
				{
					using (var archive = new ZipArchive(file_write.BaseStream, ZipArchiveMode.Create, true))
					{
						var demoFile = archive.CreateEntry($"{path.Split("\\").Last()}");

						using (var entryStream = demoFile.Open())
						using (var sw = new StreamWriter(entryStream))
						{
							XmlSerializer xmls = new XmlSerializer(typeof(T));
							xmls.Serialize(sw, obj);
						}
					}
				}
				return;
			}
			using (StreamWriter sw = new StreamWriter(path))
			{
				XmlSerializer xmls = new XmlSerializer(typeof(T));
				xmls.Serialize(sw, obj);
			}
		}
		public static T Load<T>(string filename)
		{
			try
			{
				using (var archive = ZipFile.OpenRead(filename))
				{
					var file = archive.GetEntry($"{filename.Split('\\').Last()}");
					using (var stream = file.Open())
					{
						XmlSerializer xmls = new XmlSerializer(typeof(T));
						var x = (T)xmls.Deserialize(stream);
						return x;
					}
				}
			}
			catch { }

			using (StreamReader sw = new StreamReader(filename))
			{
				XmlSerializer xmls = new XmlSerializer(typeof(T));
				var x = (T)xmls.Deserialize(sw);
				return x;
			}
		}
	}
}
