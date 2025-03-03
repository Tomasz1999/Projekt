using System;

namespace PlanZajecApp
{
	public abstract class Zajecia
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

	public class Laboratorium : Zajecia { }

	public class Wyklad : Zajecia { }

	public class Projekt : Zajecia { }
}
