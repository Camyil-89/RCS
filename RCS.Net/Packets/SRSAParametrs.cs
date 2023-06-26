using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RCS.Net.Packets
{
	[Serializable]
	public class SRSAParametrs
	{
		public byte[] D { get; set; }
		public byte[] DP { get; set; }
		public byte[] DQ { get; set; }
		public byte[] Exponent { get; set; }
		public byte[] InverseQ { get; set; }
		public byte[] Modulus { get; set; }
		public byte[] P { get; set; }
		public byte[] Q { get; set; }

		public int SizeKey = 384;

		public SRSAParametrs()
		{
		}

		public SRSAParametrs(RSAParameters rsaParams)
		{
			D = rsaParams.D;
			DP = rsaParams.DP;
			DQ = rsaParams.DQ;
			Exponent = rsaParams.Exponent;
			InverseQ = rsaParams.InverseQ;
			Modulus = rsaParams.Modulus;
			P = rsaParams.P;
			Q = rsaParams.Q;
		}

		public RSAParameters ToRSAParameters()
		{
			return new RSAParameters
			{
				D = D,
				DP = DP,
				DQ = DQ,
				Exponent = Exponent,
				InverseQ = InverseQ,
				Modulus = Modulus,
				P = P,
				Q = Q
			};
		}
	}
}
