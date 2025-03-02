using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Globalization;

abstract class Zajecia
{
	public string Kierunek { get; set; }
	public string Przedmiot { get; set; }
	public string Prowadzacy { get; set; }
	public string Sala { get; set; }
	public DateTime Data { get; set; }  // Data zajęć, np. 2025-02-24
	public TimeSpan GodzinaRozpoczecia { get; set; }  // np. 08:30
	public TimeSpan GodzinaZakonczenia { get; set; }    // np. 10:00
	public string Grupa { get; set; }

	public bool CzyDlaGrupy(string numerGrupy)
	{
		return Grupa == numerGrupy;
	}
}

class Laboratorium : Zajecia { }
class Wyklad : Zajecia { }
class Projekt : Zajecia { }

class PlanZajec
{
	public List<Zajecia> ZajeciaLista { get; set; } = new List<Zajecia>();

	public PlanZajec()
	{
		WczytajZPliku();
	}

	public void DodajZajecia(Zajecia zajecia)
	{
		// Sprawdzenie konfliktu czasowego: zajęcia na ten sam dzień, dla tej samej grupy lub w tej samej sali
		// nie mogą mieć nakładających się przedziałów czasowych.
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
		if (!DateTime.TryParseExact(data, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime parsedData) ||
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
		// Parsowanie oryginalnych danych, aby zlokalizować zajęcia do edycji
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

		// Parsowanie nowych danych
		if (!DateTime.TryParseExact(newData, "yyyy-MM-dd", null, DateTimeStyles.None, out DateTime newParsedData) ||
			!TimeSpan.TryParseExact(newGodzinaRozpoczecia, @"hh\:mm", null, out TimeSpan newStartTime) ||
			!TimeSpan.TryParse(newGodzinaZakonczenia, out TimeSpan newEndTime))
		{
			Console.WriteLine("Nieprawidłowy format nowych daty lub godziny.");
			return;
		}

		// Sprawdzenie konfliktu czasowego dla nowych danych, pomijając edytowane zajęcia
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

		// Aktualizacja danych
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
				// Zapisujemy typ zajęć jako pierwszy element, potem pozostałe dane
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
					// Pierwszy element określa typ zajęć
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
							// Jeśli typ jest nieznany, domyślnie tworzymy wykład
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

class Program
{
	static void Main()
	{
		PlanZajec plan = new PlanZajec();
		while (true)
		{
			Console.WriteLine("\nSystem rezerwacji sal");
			Console.WriteLine("1. Wyświetl plan dla dnia");
			Console.WriteLine("2. Wyświetl plan dla grupy");
			Console.WriteLine("3. Wyświetl plan dla sali");
			Console.WriteLine("4. Dodaj zajęcia");
			Console.WriteLine("5. Usuń zajęcia");
			Console.WriteLine("6. Edytuj zajęcia");
			Console.WriteLine("7. Wyjście");
			Console.Write("Wybierz opcję: ");


			switch (Console.ReadLine())
			{
				case "1":
					Console.Write("Podaj datę (yyyy-MM-dd): ");
					if (DateTime.TryParseExact(Console.ReadLine(), "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime data))
					{
						plan.WypiszPlanDnia(data);
					}
					else
					{
						Console.WriteLine("Nieprawidłowy format daty.");
					}
					break;
				case "2":
					Console.Write("Podaj grupę: ");
					plan.WypiszPlanGrupy(Console.ReadLine());
					break;
				case "3":
					Console.Write("Podaj numer sali: ");
					plan.WypiszPlanSali(Console.ReadLine());
					break;
				case "4":
					Console.WriteLine("Wybierz typ zajęć:");
					Console.WriteLine("1. Wykład");
					Console.WriteLine("2. Laboratorium");
					Console.WriteLine("3. Projekt");
					Console.Write("Twój wybór: ");
					string typ = Console.ReadLine();

					Console.WriteLine("Dodawanie zajęć:");
					Console.WriteLine("Wybierz typ zajęć: 1 - Wykład, 2 - Laboratorium, 3 - Projekt");
					string typZajec = Console.ReadLine();  // Zmieniona nazwa zmiennej

					Console.WriteLine("Podaj dane w formacie: Kierunek,Przedmiot,Prowadzacy,Sala,Data(yyyy-MM-dd),GodzinaRozpoczecia(HH:mm),GodzinaZakonczenia(HH:mm),Grupa");
					var dane = Console.ReadLine().Split(',');

					if (dane.Length == 8)
					{
						Zajecia zajecia;
						switch (typZajec)  // Używamy nowej nazwy zmiennej
						{
							case "1":
								zajecia = new Wyklad();
								break;
							case "2":
								zajecia = new Laboratorium();
								break;
							case "3":
								zajecia = new Projekt();
								break;
							default:
								Console.WriteLine("Nieprawidłowy typ zajęć.");
								return;
						}

						zajecia.Kierunek = dane[0];
						zajecia.Przedmiot = dane[1];
						zajecia.Prowadzacy = dane[2];
						zajecia.Sala = dane[3];

						if (!DateTime.TryParseExact(dane[4], "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime parsedData))
						{
							Console.WriteLine("Nieprawidłowy format daty.");
							return;
						}
						zajecia.Data = parsedData;

						if (!TimeSpan.TryParse(dane[5], out TimeSpan startTime))
						{
							Console.WriteLine("Nieprawidłowy format godziny rozpoczęcia.");
							return;
						}
						zajecia.GodzinaRozpoczecia = startTime;

						if (!TimeSpan.TryParse(dane[6], out TimeSpan endTime))
						{
							Console.WriteLine("Nieprawidłowy format godziny zakończenia.");
							return;
						}
						zajecia.GodzinaZakonczenia = endTime;

						zajecia.Grupa = dane[7];
						plan.DodajZajecia(zajecia);
					}
					break;
				case "5":
					Console.Write("Podaj dane do usunięcia w formacie: Przedmiot,Prowadzacy,Data(yyyy-MM-dd),GodzinaRozpoczecia(HH:mm): ");
					var usunDane = Console.ReadLine().Split(',');
					if (usunDane.Length == 4 &&
					   DateTime.TryParseExact(usunDane[2], "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime dataUsuniecia) &&
					   TimeSpan.TryParse(usunDane[3], out TimeSpan godzUsuniecia))
					{
						plan.UsunZajecia(usunDane[0], usunDane[1], usunDane[2], usunDane[3]);
					}
					else
					{
						Console.WriteLine("Błędne dane do usunięcia.");
					}
					break;
				case "6":
					// Edycja zajęć
					Console.WriteLine("Podaj dane identyfikujące zajęcia do edycji w formacie: Przedmiot,Prowadzacy,Data(yyyy-MM-dd),GodzinaRozpoczecia(HH:mm)");
					var edytujDane = Console.ReadLine().Split(',');
					if (edytujDane.Length != 4 ||
						!DateTime.TryParseExact(edytujDane[2], "yyyy-MM-dd", null, DateTimeStyles.None, out DateTime orygData) ||
						!TimeSpan.TryParseExact(edytujDane[3], @"hh\:mm", null, out TimeSpan orygStart))
					{
						Console.WriteLine("Błędne dane identyfikujące zajęcia.");
						break;
					}
					Console.WriteLine("Podaj nowe dane zajęć w formacie: Kierunek,Przedmiot,Prowadzacy,Sala,Data(yyyy-MM-dd),GodzinaRozpoczecia(HH:mm),GodzinaZakonczenia(HH:mm),Grupa");
					var noweDane = Console.ReadLine().Split(',');
					if (noweDane.Length != 8)
					{
						Console.WriteLine("Błędny format nowych danych.");
						break;
					}
					plan.EdytujZajecia(
						edytujDane[0], edytujDane[1], edytujDane[2], edytujDane[3],
						noweDane[0], noweDane[1], noweDane[2], noweDane[3],
						noweDane[4], noweDane[5], noweDane[6], noweDane[7]
					);
					break;
				case "7":
					return;
				default:
					Console.WriteLine("Nieprawidłowy wybór.");
					break;
			}
		}
	}
}
