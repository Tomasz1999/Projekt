using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace PlanZajecApp
{
	public class PlanZajec
	{
		public List<Zajecia> ZajeciaLista { get; set; } = new List<Zajecia>();

		public PlanZajec()
		{
			WczytajZPliku();
		}

		public void DodajZajecia(Zajecia zajecia)
		{
			bool konflikt = ZajeciaLista.Any(z =>
				z.Data.Date == zajecia.Data.Date &&
				(z.Grupa == zajecia.Grupa || z.Sala == zajecia.Sala) &&
				(z.GodzinaRozpoczecia < zajecia.GodzinaZakonczenia && zajecia.GodzinaRozpoczecia < z.GodzinaZakonczenia)
			);
			if (konflikt)
			{
				Console.WriteLine("Błąd: Grupa lub sala jest już zajęta w tym przedziale czasowym.");
				return;
			}
			ZajeciaLista.Add(zajecia);
			ZapiszDoPliku();
			Console.WriteLine("Dodano zajęcia.");
		}

		public void UsunZajecia(string przedmiot, string prowadzacy, string data, string godzinaRozpoczecia)
		{
			if (!DateTime.TryParseExact(data, "yyyy-MM-dd", null, DateTimeStyles.None, out DateTime parsedData) ||
				!TimeSpan.TryParseExact(godzinaRozpoczecia, @"hh\:mm", null, out TimeSpan parsedStartTime))
			{
				Console.WriteLine("Nieprawidłowy format daty lub godziny.");
				return;
			}

			var zajecia = ZajeciaLista.FirstOrDefault(z =>
				z.Przedmiot.Trim().Equals(przedmiot.Trim(), StringComparison.OrdinalIgnoreCase) &&
				z.Prowadzacy.Trim().Equals(prowadzacy.Trim(), StringComparison.OrdinalIgnoreCase) &&
				z.Data.Date == parsedData.Date &&
				z.GodzinaRozpoczecia == parsedStartTime
			);

			if (zajecia != null)
			{
				ZajeciaLista.Remove(zajecia);
				ZapiszDoPliku();
				Console.WriteLine("Usunięto zajęcia.");
			}
			else
			{
				Console.WriteLine("Nie znaleziono zajęć do usunięcia.");
			}
		}

		public void EdytujZajecia(
			string przedmiot, string prowadzacy, string data, string godzinaRozpoczecia,
			string newKierunek, string newPrzedmiot, string newProwadzacy, string newSala,
			string newData, string newGodzinaRozpoczecia, string newGodzinaZakonczenia, string newGrupa)
		{
			if (!DateTime.TryParseExact(data, "yyyy-MM-dd", null, DateTimeStyles.None, out DateTime parsedData) ||
				!TimeSpan.TryParseExact(godzinaRozpoczecia, @"hh\:mm", null, out TimeSpan parsedStartTime))
			{
				Console.WriteLine("Nieprawidłowy format daty lub godziny oryginalnych zajęć.");
				return;
			}

			var zajecia = ZajeciaLista.FirstOrDefault(z =>
				z.Przedmiot.Trim().Equals(przedmiot.Trim(), StringComparison.OrdinalIgnoreCase) &&
				z.Prowadzacy.Trim().Equals(prowadzacy.Trim(), StringComparison.OrdinalIgnoreCase) &&
				z.Data.Date == parsedData.Date &&
				z.GodzinaRozpoczecia == parsedStartTime
			);

			if (zajecia == null)
			{
				Console.WriteLine("Nie znaleziono zajęć do edycji.");
				return;
			}

			if (!DateTime.TryParseExact(newData, "yyyy-MM-dd", null, DateTimeStyles.None, out DateTime newParsedData) ||
				!TimeSpan.TryParseExact(newGodzinaRozpoczecia, @"hh\:mm", null, out TimeSpan newStartTime) ||
				!TimeSpan.TryParse(newGodzinaZakonczenia, out TimeSpan newEndTime))
			{
				Console.WriteLine("Nieprawidłowy format nowych daty lub godziny.");
				return;
			}

			bool konflikt = ZajeciaLista.Any(z =>
				z != zajecia &&
				z.Data.Date == newParsedData.Date &&
				(z.Grupa == newGrupa || z.Sala == newSala) &&
				(z.GodzinaRozpoczecia < newEndTime && newStartTime < z.GodzinaZakonczenia)
			);
			if (konflikt)
			{
				Console.WriteLine("Błąd: Grupa lub sala jest już zajęta w tym przedziale czasowym.");
				return;
			}

			zajecia.Kierunek = newKierunek;
			zajecia.Przedmiot = newPrzedmiot;
			zajecia.Prowadzacy = newProwadzacy;
			zajecia.Sala = newSala;
			zajecia.Data = newParsedData;
			zajecia.GodzinaRozpoczecia = newStartTime;
			zajecia.GodzinaZakonczenia = newEndTime;
			zajecia.Grupa = newGrupa;

			ZapiszDoPliku();
			Console.WriteLine("Zajęcia zostały zaktualizowane.");
		}

		public void WypiszPlanDnia(DateTime data)
		{
			var zajeciaDnia = ZajeciaLista.Where(z => z.Data.Date == data.Date);
			foreach (var zajecia in zajeciaDnia)
			{
				Console.WriteLine($"{zajecia.Data:yyyy-MM-dd} {zajecia.GodzinaRozpoczecia}-{zajecia.GodzinaZakonczenia} ({zajecia.GetType().Name}): {zajecia.Przedmiot} - {zajecia.Prowadzacy} ({zajecia.Grupa}, {zajecia.Sala})");
			}
		}

		public void WypiszPlanGrupy(string grupa)
		{
			var zajeciaGrupy = ZajeciaLista.Where(z => z.Grupa == grupa);
			foreach (var zajecia in zajeciaGrupy)
			{
				Console.WriteLine($"{zajecia.Data:yyyy-MM-dd} {zajecia.GodzinaRozpoczecia}-{zajecia.GodzinaZakonczenia} ({zajecia.GetType().Name}): {zajecia.Przedmiot} - {zajecia.Prowadzacy} ({zajecia.Sala})");
			}
		}

		public void WypiszPlanSali(string sala)
		{
			var zajeciaSali = ZajeciaLista.Where(z => z.Sala == sala);
			foreach (var zajecia in zajeciaSali)
			{
				Console.WriteLine($"{zajecia.Data:yyyy-MM-dd} {zajecia.GodzinaRozpoczecia}-{zajecia.GodzinaZakonczenia} ({zajecia.GetType().Name}): {zajecia.Przedmiot} - {zajecia.Prowadzacy} ({zajecia.Grupa})");
			}
		}

		private void ZapiszDoPliku()
		{
			using (StreamWriter sw = new StreamWriter("plan.txt"))
			{
				foreach (var zajecia in ZajeciaLista)
				{
					sw.WriteLine($"{zajecia.GetType().Name},{zajecia.Kierunek},{zajecia.Przedmiot},{zajecia.Prowadzacy},{zajecia.Sala},{zajecia.Data:yyyy-MM-dd},{zajecia.GodzinaRozpoczecia},{zajecia.GodzinaZakonczenia},{zajecia.Grupa}");
				}
			}
		}

		private void WczytajZPliku()
		{
			if (File.Exists("plan.txt"))
			{
				foreach (var linia in File.ReadAllLines("plan.txt"))
				{
					var dane = linia.Split(',');
					if (dane.Length == 9)
					{
						Zajecia zajecia;
						switch (dane[0])
						{
							case "Wyklad":
								zajecia = new Wyklad();
								break;
							case "Laboratorium":
								zajecia = new Laboratorium();
								break;
							case "Projekt":
								zajecia = new Projekt();
								break;
							default:
								zajecia = new Wyklad();
								break;
						}
						zajecia.Kierunek = dane[1];
						zajecia.Przedmiot = dane[2];
						zajecia.Prowadzacy = dane[3];
						zajecia.Sala = dane[4];
						zajecia.Data = DateTime.ParseExact(dane[5], "yyyy-MM-dd", null);
						zajecia.GodzinaRozpoczecia = TimeSpan.Parse(dane[6]);
						zajecia.GodzinaZakonczenia = TimeSpan.Parse(dane[7]);
						zajecia.Grupa = dane[8];
						ZajeciaLista.Add(zajecia);
					}
				}
			}
		}
	}
}
