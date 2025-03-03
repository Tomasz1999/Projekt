using System;
using System.Globalization;

namespace PlanZajecApp
{
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
						if (DateTime.TryParseExact(Console.ReadLine(), "yyyy-MM-dd", null, DateTimeStyles.None, out DateTime data))
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
						string typZajec = Console.ReadLine();

						Console.WriteLine("Podaj dane w formacie: Kierunek,Przedmiot,Prowadzacy,Sala,Data(yyyy-MM-dd),GodzinaRozpoczecia(HH:mm),GodzinaZakonczenia(HH:mm),Grupa");
						var dane = Console.ReadLine().Split(',');

						if (dane.Length == 8)
						{
							Zajecia zajecia;
							switch (typZajec)
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

							if (!DateTime.TryParseExact(dane[4], "yyyy-MM-dd", null, DateTimeStyles.None, out DateTime parsedData))
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
							DateTime.TryParseExact(usunDane[2], "yyyy-MM-dd", null, DateTimeStyles.None, out DateTime dataUsuniecia) &&
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
}
