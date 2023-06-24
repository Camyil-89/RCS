// See https://aka.ms/new-console-template for more information

using RCS.Certificates;
using RCS.Service;
using System.Security.Cryptography;

//2048; 961
//True
Console.WriteLine("Hello, World!");
var cert = XmlProvider.Load<Certificate>("C:\\Users\\zhuko\\Documents\\RCS\\ServerTrustedCertificates\\1.сертификат");

Console.WriteLine(cert.Info.Raw());

//Console.WriteLine($"{cert.LengthKey};{cert.Info.RawByte().Length}");
//Console.WriteLine($"{string.Join(";", cert.Info.RawByte())}");
//Console.WriteLine($"{cert.Verify(cert.Info.RawByte(), cert.Sign)}");
using (RSA rsa = RSA.Create())
{
	rsa.ImportRSAPublicKey(cert.Info.PublicKey, out _);

	bool isSignatureValid = rsa.VerifyData(cert.Info.RawByte(), cert.Sign, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
	Console.WriteLine(isSignatureValid);
}


Console.Read();