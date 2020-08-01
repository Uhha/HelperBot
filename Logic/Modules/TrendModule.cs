using DatabaseInteractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.IO;
using System.Threading;
using System.Drawing;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using Tracer;
using Web.Providers;

namespace Logic.Modules
{
    [Obsolete("No more /trends", true)]
    class TrendModule : IModule
    {
        public async Task GenerateAndSendAsync(TelegramBotClient bot, Update update)
        {
            //var number = update.Message.Text.Substring(update.Message.Text.IndexOf(' ') + 1);
            SetParameters(update, out int hoursBack, out string ticker, out string style);


            PlotModel pm = new PlotModel() { Title = $"{ticker} - {hoursBack}H" };
            pm.LegendPosition = LegendPosition.BottomLeft;
            pm.LegendBackground = OxyColor.FromArgb(255, 255,255,255);
            pm.LegendBorder = OxyColor.FromArgb(255, 0, 0, 0);
            var datimeAxis = new TimeSpanAxis { Position = AxisPosition.Bottom, StringFormat = style };
            datimeAxis.IntervalLength = 37;
            datimeAxis.StartPosition = 1;
            datimeAxis.EndPosition = 0;
            datimeAxis.MinorGridlineStyle = LineStyle.Dot;
            pm.Axes.Add(datimeAxis);

            bool dataOk = AddLineSeries(pm, hoursBack, ticker);
            if (!dataOk)
            {
                await bot.SendTextMessageAsync(update.Message.Chat.Id, "Wrong parameters / No data for the entered ticker");
                return;
            }

            FileStream stream = ConvertToPNG(pm);
            var test = await bot.SendPhotoAsync(update.Message.Chat.Id, new FileToSend() { Filename = "chart", Content = stream });
        }

        private bool AddLineSeries(PlotModel pm, int hoursBack, string ticker)
        {
            if (ticker.ToLower() == "all")
            {
                var tickers = new string[] { "BTC", "ETH", "XRP", "BCH", "LTC" };
                var colors = new OxyColor[] { OxyColors.Green, OxyColors.Yellow, OxyColors.Red, OxyColors.Blue, OxyColors.Violet};
                for (int i = 0; i < tickers.Length; i++)
                {
                    LineSeries series = new LineSeries
                    {
                        Title = tickers[i],
                        Color = colors[i]
                    };
                    bool dataOk = InsertData(series, hoursBack, tickers[i], true);
                    if (dataOk)
                    {
                        pm.Series.Add(series);
                    }
                }
                return true;
            }
            else
            {
                LineSeries series = new LineSeries();
                bool dataOk = InsertData(series, hoursBack, ticker);
                pm.Series.Add(series);
                return dataOk;
            }
            
        }

        private FileStream ConvertToPNG(PlotModel pm)
        {
            var stream = new MemoryStream();
            var thread = new Thread(() =>
            {
                var pngExporter = new OxyPlot.Wpf.PngExporter { Width = 700, Height = 500, Background = OxyColors.White };
                pngExporter.Export(pm, stream);
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            return ConvertStream(stream);
        }

        private void SetParameters(Update update, out int hoursBack, out string ticker, out string style)
        {
            ticker = "BTC";
            hoursBack = 5;
            var strings = update.Message.Text.Split(' ');
            if (strings.Length == 3)
            {
                int.TryParse(strings[1], out hoursBack);
                ticker = (!string.IsNullOrEmpty(strings[2])) ? strings[2] : "WrongTicker";
            }
            else if (strings.Length == 2)
            {
                var parsed = int.TryParse(strings[1], out hoursBack);
                if (!parsed)
                {
                    ticker = (!string.IsNullOrEmpty(strings[1])) ? strings[1] : "WrongTicker";
                }
            }

            if (hoursBack <= 0) hoursBack = 5;
            if (hoursBack > 500) hoursBack = 500;
            style = "h:mm";
            if (hoursBack < 2) style = "mm";
        }


        private bool InsertData(LineSeries series, int hoursBack, string ticker, bool normalize = false)
        {
            List<DataPoint> list = new List<DataPoint>();
            using (AlcoDBEntities db = new AlcoDBEntities())
            {
                var botLimit = DateTime.UtcNow.AddHours(-hoursBack);
                var prices = db.CoinPriceRecords
                    .Where(o => o.dtRecorded >= botLimit && o.dtRecorded <= DateTime.UtcNow
                    && o.CoinSymbol == ticker);
                var normalizeMax = (normalize) ? prices.Max(o => o.Price) : 1;
                foreach (var price in prices)
                {
                    list.Add(new DataPoint(TimeSpanAxis.ToDouble(DateTime.UtcNow - price.dtRecorded), Axis.ToDouble(price.Price / normalizeMax)));
                }
            }
            series.ItemsSource = list;
            return list.Count > 0;
        }

        private FileStream ConvertStream(Stream stream)
        {
            Image img = System.Drawing.Image.FromStream(stream);
            var tempFile = Path.GetTempFileName();
            //var fs = System.IO.File.Create(tempFile);
            img.Save(tempFile, System.Drawing.Imaging.ImageFormat.Png);
            FileStream fileStream = new FileStream(tempFile, FileMode.Open, FileAccess.Read);
            return fileStream;
        }

        public Task GenerateAndSendCallbackAsync(TelegramBotClient bot, Update update)
        {
            throw new NotImplementedException();
        }

        public Task GenerateAndSendWorkerAsync(TelegramBotClient bot, IList<string> parameters = null)
        {
            using (AlcoDBEntities db = new AlcoDBEntities())
            {
                db.CoinPriceRecords.RemoveRange(db.CoinPriceRecords.Where(o => o.dtRecorded < DateTime.UtcNow.AddMonths(-1)));
                db.SaveChanges();
            }
            return null;
        }

        public async void RecordCoinPrice()
        {
            HttpClient client = HttpClientProvider.GetClient();
            int currenciesNumber = 5;
            try
            {
                var response = await client.GetAsync(string.Format("https://api.coinmarketcap.com/v1/ticker?limit={0}", currenciesNumber));
                if (response.IsSuccessStatusCode)
                {
                    //var result = response.Content.ReadAsAsync<CoinPrice[]>().Result;
                    //using (AlcoDBEntities db = new AlcoDBEntities())
                    //{
                    //    foreach (var item in result)
                    //    {
                    //        decimal.TryParse(item.price_usd, out decimal price);
                    //        db.CoinPriceRecords.Add(new CoinPriceRecords() { dtRecorded = DateTime.UtcNow, CoinSymbol = item.symbol, Price = price });
                    //    }
                    //    db.SaveChanges();
                    //}

                }
            }
            catch (Exception e)
            {
                TraceError.Error(e);
            }

        }

    }
}
