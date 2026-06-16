using System;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using GraZaDuzoZaMalo.Model;

namespace AppGraZaDuzoZaMaloCLI
{
	public static class RejestratorStanuXml
	{
		private static readonly string SciezkaPliku = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "stan_gry.xml");

		// Klucz AES i wektor IV do szyfrowania pliku
		private static readonly byte[] Klucz = Encoding.UTF8.GetBytes("A6tg90FkdE321mNqZpWxYcvBbnM12345");
		private static readonly byte[] IV = Encoding.UTF8.GetBytes("SecuredIV1234567");

		public static bool CzyIstniejeStan() => File.Exists(SciezkaPliku);

		public static bool ZapiszStan(StanGryData stan)
		{
			try
			{
				DataContractSerializer serializer = new DataContractSerializer(typeof(StanGryData));
				using (FileStream fs = new FileStream(SciezkaPliku, FileMode.Create, FileAccess.Write))
				using (Aes aes = Aes.Create())
				{
					aes.Key = Klucz;
					aes.IV = IV;
					using (ICryptoTransform encryptor = aes.CreateEncryptor())
					using (CryptoStream cs = new CryptoStream(fs, encryptor, CryptoStreamMode.Write))
					{
						serializer.WriteObject(cs, stan);
					}
				}
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"\n[Błąd zapisu stanu]: {ex.Message}");
				return false;
			}
		}

		public static StanGryData OdczytajStan()
		{
			if (!CzyIstniejeStan()) return null;
			try
			{
				DataContractSerializer serializer = new DataContractSerializer(typeof(StanGryData));
				using (FileStream fs = new FileStream(SciezkaPliku, FileMode.Open, FileAccess.Read))
				using (Aes aes = Aes.Create())
				{
					aes.Key = Klucz;
					aes.IV = IV;
					using (ICryptoTransform decryptor = aes.CreateDecryptor())
					using (CryptoStream cs = new CryptoStream(fs, decryptor, CryptoStreamMode.Read))
					{
						return (StanGryData)serializer.ReadObject(cs);
					}
				}
			}
			catch (Exception)
			{
				return null;
			}
		}

		public static void UsunPlikStanu()
		{
			try { if (CzyIstniejeStan()) File.Delete(SciezkaPliku); } catch { }
		}
	}
}