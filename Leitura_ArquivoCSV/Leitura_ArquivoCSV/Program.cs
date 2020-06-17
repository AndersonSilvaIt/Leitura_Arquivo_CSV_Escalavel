using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace Leitura_ArquivoCSV {
	class Program {
		/*
		 Estrutura do arquivo

				userId,movieId,rating,timestamp
				1,2,3.5,1112486027
				1,29,3.5,1112484676
				1,32,3.5,1112484819
				1,47,3.5,1112484727
				1,50,3.5,1112484580
				1,112,3.5,1094785740
				1,151,4.0,1094785734
				1,223,4.0,1112485573
				1,253,4.0,1112484940
		*/

		static void Main(string[] args) {

			var before0 = GC.CollectionCount(0);
			var before1 = GC.CollectionCount(1);
			var before2 = GC.CollectionCount(2);

			var sw = new Stopwatch();
			sw.Start();

			//Run3(@"C:\Users\Prodwin\Desktop\LendoArquivoCSV\Leitura_Arquivo_CSV_Escalavel\ratings.csv");
			
			//Validacao01();  // ambos ficaram iguais
			//Validacao02();

			sw.Stop();
			Console.WriteLine($"\nTime .: {sw.ElapsedMilliseconds} ms");
			Console.WriteLine($"# Gen0: {GC.CollectionCount(0) - before0}");
			Console.WriteLine($"# Gen1: {GC.CollectionCount(1) - before1}");
			Console.WriteLine($"# Gen2: {GC.CollectionCount(2) - before2}");
			Console.WriteLine($"Memory: {Process.GetCurrentProcess().WorkingSet64 / 1024 / 1024}/ mb");

			Console.ReadKey();
		}

		public static void Run1(string filePath) {
			var lines = File.ReadAllLines(filePath);
			var sum = 0d;
			var count = 0;

			foreach(var line in lines) {
				var parts = line.Split(',');

				if(parts[1] == "110") {
					sum += double.Parse(parts[2], CultureInfo.InvariantCulture);
					count++;
				}
			}

			Console.WriteLine($"Average rate for Braveheart is {sum / count} ({count} votes).");
		}

		public static void Run2(string filePath) {
			var sum = 0d;
			var count = 0;
			string line;

			using(var fs = File.OpenRead(filePath))
			using(var reader = new StreamReader(fs))
				while((line = reader.ReadLine()) != null) {
					var parts = line.Split(',');

					if(parts[1] == "110") {
						sum += double.Parse(parts[2], CultureInfo.InvariantCulture);
						count++;
					}
				}

			Console.WriteLine($"Average rate for Braveheart is {sum / count} ({count} votes).");
		}

		public static void Run3(string filePath) {
			var sum = 0d;
			var count = 0;
			string line;

			// Braveheart id movie id as span;
			var lookingFor = "110".AsSpan();

			using(var fs = File.OpenRead(filePath))
			using(var reader = new StreamReader(fs))

				while((line = reader.ReadLine()) != null) {
					// ignoring the voter id
					var span = line.AsSpan(line.IndexOf(',') + 1);

					// movieId
					var firstCommaPos = span.IndexOf(',');
					var movieId = span.Slice(0, firstCommaPos);
					if(!movieId.SequenceEqual(lookingFor)) continue;

					// rating
					span = span.Slice(firstCommaPos + 1);
					firstCommaPos = span.IndexOf(',');
					var rating = double.Parse(span.Slice(0, firstCommaPos), provider: CultureInfo.InvariantCulture);

					sum += rating;
					count++;
				}
		}

		public static void Run4(string filePath) 
		{
			var sum = 0d;
			var count = 0;

			var lookingFor = Encoding.UTF8.GetBytes("110").AsSpan();
			var rawBuffer = new byte[1024 * 1024];
			using(var fs = File.OpenRead(filePath)) {
				var bytesBuffered = 0;
				var bytesConsumed = 0;

				while(true) {
					var bytesRead = fs.Read(rawBuffer, bytesBuffered, rawBuffer.Length - bytesBuffered);

					if(bytesRead == 0) break;
					bytesBuffered += bytesRead;

					int linePosition;

					do {
						linePosition = Array.IndexOf(rawBuffer, (byte)'\n', bytesConsumed,
							bytesBuffered - bytesConsumed);

						if(linePosition >= 0) {
							var lineLength = linePosition - bytesConsumed;
							var line = new Span<byte>(rawBuffer, bytesConsumed, lineLength);
							bytesConsumed += lineLength + 1;


							// ignoring the voter id
							var span = line.Slice(line.IndexOf((byte)',') + 1);

							// movieId
							var firstCommaPos = span.IndexOf((byte)',');
							var movieId = span.Slice(0, firstCommaPos);
							if(!movieId.SequenceEqual(lookingFor)) continue;

							// rating
							span = span.Slice(firstCommaPos + 1);
							firstCommaPos = span.IndexOf((byte)',');
							var rating = double.Parse(Encoding.UTF8.GetString(span.Slice(0, firstCommaPos)), provider: CultureInfo.InvariantCulture);

							sum += rating;
							count++;
						}

					} while(linePosition >= 0);

					Array.Copy(rawBuffer, bytesConsumed, rawBuffer, 0, (bytesBuffered - bytesConsumed));
					bytesBuffered -= bytesConsumed;
					bytesConsumed = 0;
				}
			}

			Console.WriteLine($"Average rate for Braveheart is {sum / count} ({count} votes).");
		}

		/*Testes de comparação*/

		public static List<byte> lista;

		public static void Preencher() {
			lista = new List<byte>();

			int count = 999999999;

			for(int i = 0; i < count; i++) {
				lista.Add(0x41);
			}
		}

		public static void Validacao01() 
		{
			Preencher();

			int numero = 0;
			for(int i = 0; i < lista.Count; i++) {

				if((char)lista[i] == 'A')
					numero = 1;
			}
			Console.WriteLine("Passou: " + numero);
		}

		public static void Validacao02() 
		{
			Preencher();

			int numero = 0;
			for(int i = 0; i < lista.Count; i++) {

				if(lista[i] == 0x41)
					numero = 1;
			}

			Console.WriteLine("Passou: " + numero);
		}

	}
}
