// See https://aka.ms/new-console-template for more information

using RCS.Certificates;
using RCS.Net.Tcp;
using RCS.Service;
using System.Security.Cryptography;

var cert = CertificateManager.RCSLoadCertificate($@"C:\Users\zhuko\Documents\RCS\RCSSERVER.сертификат");

for (int i = 0; i < 5; i++)
{
	Task.Run(() =>
	{
		RCSTCPClient client = new RCSTCPClient();
		client.PublicKey = cert.Info.PublicKey;
		client.TimeoutUpdateKeys = 1;
		client.Connect($"127.0.0.1", 1991);
		while (true)
		{
			Thread.Sleep(1000);
		}

	});
}


Console.Read();