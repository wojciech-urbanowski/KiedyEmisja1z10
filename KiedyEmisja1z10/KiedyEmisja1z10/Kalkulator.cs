using Nager.Date;
using Nager.Date.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KiedyEmisja1z10
{
    public class Kalkulator
    {
        private static readonly DateTime dataBrzegowa = new DateTime(2021, 11, 19);
        private const int seriaBrzegowa = 127;
        private const int odcinekBrzegowy = 11;

        private const int maksymalnaSeria = 150;

        public (DateTime DataEmisjiNajwczesniejsza, int LiczbaDniNiepewnych, List<(DateTime Data, (int Seria, int Odcinek)? SeriaIOdcinek, bool Niepewny, string Opis)> SzczegolyWyliczania) Oblicz(int seria, int odcinek)
        {
            if (seria > maksymalnaSeria)
                throw new ArgumentOutOfRangeException(nameof(seria), $"Maksimum serii to {maksymalnaSeria}.");
            if (seria < seriaBrzegowa)
                throw new ArgumentOutOfRangeException(nameof(seria), $"Seria nie może być wcześniejsza niż {seriaBrzegowa}.");
            if (odcinek <= 0 || odcinek > 21)
                throw new ArgumentOutOfRangeException(nameof(odcinek), "Każda edycja od 73. liczy 21 odcinków: 20 zwykłych + Wielki Finał.");
            if (seria == seriaBrzegowa && odcinek < odcinekBrzegowy)
                throw new InvalidOperationException($"Obliczanie daty odcinka działa od odcinka {odcinekBrzegowy} {seriaBrzegowa}. serii.");

            var kursorDaty = dataBrzegowa;
            var kursorSerii = seriaBrzegowa;
            var kursorOdcinka = odcinekBrzegowy;
            var liczbaDniNiepewnych = 0;

            var szczegolyWyliczania = new List<(DateTime Data, (int Seria, int Odcinek)? SeriaIOdcinek, bool Niepewny, string Opis)>();
            var publicHolidaysDictionary = new Dictionary<(int Rok, int Miesiac), IEnumerable<PublicHoliday>>();

            while (kursorSerii < seria || (kursorSerii == seria && kursorOdcinka <= odcinek))
            {
                if (!publicHolidaysDictionary.TryGetValue((kursorDaty.Year, kursorDaty.Month), out var swietaWMiesiacu))
                    swietaWMiesiacu = DateSystem.GetPublicHolidays(new DateTime(kursorDaty.Year, kursorDaty.Month, 1), new DateTime(kursorDaty.Year, kursorDaty.Month, 1).AddMonths(1).AddDays(-1), CountryCode.PL);

                var swieto = swietaWMiesiacu.FirstOrDefault(s => s.Date == kursorDaty);
                if (swieto != null)
                {
                    szczegolyWyliczania.Add((kursorDaty, null, false, swieto.LocalName));
                }
                else
                {
                    var swieto2 = JestPrawiePewnymSwietem(kursorDaty);
                    if (swieto2.JestSwietem)
                    {
                        szczegolyWyliczania.Add((kursorDaty, null, false, swieto2.Nazwa ?? string.Empty));
                    }
                    else
                    {
                        var dayOfWeek = kursorDaty.DayOfWeek;

                        switch (dayOfWeek)
                        {
                            case DayOfWeek.Saturday:
                                szczegolyWyliczania.Add((kursorDaty, null, false, "Sobota"));
                                break;
                            case DayOfWeek.Sunday:
                                szczegolyWyliczania.Add((kursorDaty, null, false, "Niedziela"));
                                break;
                            default:
                                {
                                    var jestDniemNiepewnym = JestDniemNiepewnym(kursorDaty);

                                    if (jestDniemNiepewnym)
                                        liczbaDniNiepewnych++;

                                    szczegolyWyliczania.Add((kursorDaty, (kursorSerii, kursorOdcinka), jestDniemNiepewnym, string.Empty));

                                    if (kursorSerii == seria && kursorOdcinka == odcinek)
                                        return (kursorDaty, liczbaDniNiepewnych, szczegolyWyliczania);

                                    kursorOdcinka++;
                                    if (kursorOdcinka > 21)
                                    {
                                        kursorSerii++;
                                        kursorOdcinka = 1;
                                    }
                                }
                                break;
                        }
                    }
                }

                kursorDaty = kursorDaty.AddDays(1);
            }

            throw new InvalidOperationException("Wystąpił nieznany błąd");
        }

        private (bool JestSwietem, string? Nazwa) JestPrawiePewnymSwietem(DateTime kursorDaty)
        {
            if (kursorDaty.Month == 12 && kursorDaty.Day == 24)
                    return (true, "Wigilia");

            return (false, null);
        }

        private bool JestDniemNiepewnym(DateTime kursorDaty)
        {
            if (kursorDaty.Month == 12)
            {
                if (kursorDaty.Day >= 24 && kursorDaty.Day <= 31)
                    return true;
            }

            return false;
        }
    }
}
