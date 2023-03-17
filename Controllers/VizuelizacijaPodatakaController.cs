using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Windows.Forms.DataVisualization.Charting;
using Test_DataVisualization.Models;

namespace Test_DataVisualization.Controllers
{
    public class VizuelizacijaPodatakaController : Controller
    {

        private readonly HttpClient _httpClient = new HttpClient();

        private double IzracunajSate(DateTime pocetak,DateTime kraj)
        {
            TimeSpan razlika = kraj - pocetak;
            double razlikaSati = Math.Round(razlika.TotalHours, 1);
            return razlikaSati;
        }

        private static void GenerisiPNG(List<Radnik> listaRadnika)
        {
            Chart chart = new Chart();
            chart.Size = new System.Drawing.Size(1000, 700);
            ChartArea chartArea = new ChartArea();
            chartArea.Name = "Radnici i sati";
            chart.ChartAreas.Add(chartArea);

            Series series = new Series();
            series.ChartType = SeriesChartType.Pie;
            chart.Series.Add(series);

            // Izracunavanje procenta rada po radniku
            double ukupnoSati = listaRadnika.Sum(r => r.SatiRada);
            double delilac = ukupnoSati / 100;

            // Kreiranje dela grafika za svakog radnika
            foreach(var radmik in listaRadnika)
            {
                if (!string.IsNullOrEmpty(radmik.ImePrezime))
                {
                    series.Points.AddXY(radmik.ImePrezime + " | " + Math.Round(radmik.SatiRada / delilac, 1) + "%", radmik.SatiRada / delilac);
                }
                else
                {
                    series.Points.AddXY("Anonimus | " + Math.Round(radmik.SatiRada / delilac, 1) + "%", radmik.SatiRada / delilac);
                }
            }

            // Kreiranje legende
            var legend = new Legend();
            legend.Name = "radnici";
            legend.Title = "Radnici";
            legend.Docking = Docking.Right;
            legend.Alignment = System.Drawing.StringAlignment.Center;
            legend.Font = new System.Drawing.Font("Verdana", 14);
            chart.Legends.Add(legend);


            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var putanjaSlike = Path.Combine(baseDirectory, "img\\grafik.png");

            chart.SaveImage(putanjaSlike, ChartImageFormat.Png);
        }

        public async Task<ActionResult> Index()
        {
            // Preuzimanje podataka
            var response = await _httpClient.GetAsync("https://rc-vault-fap-live-1.azurewebsites.net/api/gettimeentries?code=vO17RnE8vuzXzPJo5eaLLjXjmRW07law99QTD90zat9FfOQJKKUcgQ==");
            var json = await response.Content.ReadAsStringAsync();
            List<Employee> employees = JsonConvert.DeserializeObject<List<Employee>>(json);

            // Prebacivanje u tip podatka koji odgovara i olaksava dalji rad i grupisanje po imenu i prezimenu - EmployeeName
            var radnici = employees.GroupBy(e => e.EmployeeName).Select(g => new
            {
                ImePrezime = g.Key,
                SatiRada = g.Sum(o => IzracunajSate(o.StarTimeUtc, o.EndTimeUtc))
            }).OrderByDescending(e => e.SatiRada);

            // Ubacivanje u listu
            List<Radnik> listaRadnika = new List<Radnik>();
            foreach(var radnik in radnici)
            {
                listaRadnika.Add(new Radnik()
                {
                    ImePrezime = radnik.ImePrezime,
                    SatiRada = radnik.SatiRada,
                });
            }
            GenerisiPNG(listaRadnika);
            return View(listaRadnika);
        }
    }
}