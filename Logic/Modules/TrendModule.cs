using DatabaseInteractions;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

using System.IO;
using System.Threading;
//using System.Web.UI.DataVisualization.Charting;
using System.Web;
using System.Drawing;
using System.Drawing.Imaging;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;

namespace Logic.Modules
{
    class TrendModule : IModule
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

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
                    LineSeries series = new LineSeries();
                    series.Title = tickers[i];
                    series.Color = colors[i];
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
            //BrowserShareChart cb = new BrowserShareChart(BrowserShareRepository.GetBrowserShares());
            //var stream = cb.GetChartImage(600, 400);
            //var test = await bot.SendPhotoAsync(update.Message.Chat.Id, new FileToSend() { Filename = "name", Content = ConvertStream(stream) }, "My Text");


            throw new NotImplementedException();
        }

        public Task GenerateAndSendWorkerAsync(TelegramBotClient bot, IList<string> parameters = null)
        {
            throw new NotImplementedException();
        }

        public async void RecordCoinPrice()
        {
            HttpClient client = new HttpClient();
            int currenciesNumber = 5;
            var response = await client.GetAsync(string.Format("https://api.coinmarketcap.com/v1/ticker?limit={0}", currenciesNumber));
            if (response.IsSuccessStatusCode)
            {
                var result = response.Content.ReadAsAsync<CoinPrice[]>().Result;
                using (AlcoDBEntities db = new AlcoDBEntities())
                {
                    foreach (var item in result)
                    {
                        decimal.TryParse(item.price_usd, out decimal price);
                        db.CoinPriceRecords.Add(new CoinPriceRecords() { dtRecorded = DateTime.UtcNow , CoinSymbol = item.symbol, Price = price });
                    }
                    db.SaveChanges();
                }

            }

        }

    }

    //internal class ChartBase
    //{
    //    protected List<Series> ChartSeriesData { get; set; }
    //    protected string ChartTitle { get; set; }

    //    // This is the method to get the chart image
    //    public MemoryStream GetChartImage(int width, int height)
    //    {
    //        var chart = InitiateChart(width, height);
    //        chart.RenderType = RenderType.BinaryStreaming;

    //        var ms = new MemoryStream();
    //        chart.SaveImage(ms, ChartImageFormat.Png);

    //        return ms;
    //    }

    //    // This is the method to get the chart image map
    //    public string GetChartImageMap(int width, int height, string mapName)
    //    {
    //        var chart = InitiateChart(width, height);
    //        chart.RenderType = RenderType.ImageMap;
    //        chart.SaveImage(Stream.Null);

    //        return chart.GetHtmlImageMap(mapName);
    //    }

    //    // Override this method to add title to the chart
    //    protected virtual void AddChartTitle()
    //    {
    //        ChartTitle = null;
    //    }

    //    // Override this method to add data to the chart
    //    protected virtual void AddChartSeries()
    //    {
    //        ChartSeriesData = new List<Series>();
    //    }

    //    // Initiate the chart to be rendered
    //    private Chart InitiateChart(int width, int height)
    //    {
    //        var chart = new Chart();
    //        chart.Width = width;
    //        chart.Height = height;
    //        chart.BorderSkin.BackColor = System.Drawing.Color.Transparent;
    //        chart.BorderSkin.PageColor = System.Drawing.Color.Transparent;
    //        chart.BackColor = System.Drawing.Color.FromArgb(211, 223, 240);
    //        chart.BorderlineDashStyle = ChartDashStyle.Solid;
    //        chart.BackSecondaryColor = System.Drawing.Color.White;
    //        chart.BackGradientStyle = GradientStyle.TopBottom;
    //        chart.BorderlineWidth = 1;
    //        chart.Palette = ChartColorPalette.BrightPastel;
    //        chart.BorderlineColor = System.Drawing.Color.FromArgb(26, 59, 105);
    //        chart.BorderSkin.SkinStyle = BorderSkinStyle.Emboss;
    //        chart.AntiAliasing = AntiAliasingStyles.All;
    //        chart.TextAntiAliasingQuality = TextAntiAliasingQuality.Normal;

    //        AddChartTitle();
    //        if (ChartTitle != null)
    //        {
    //            chart.Titles.Add(CreateTitle());
    //        }
    //        chart.Legends.Add(CreateLegend());

    //        AddChartSeries();
    //        foreach (var series in ChartSeriesData)
    //        {
    //            chart.Series.Add(series);
    //        }

    //        chart.ChartAreas.Add(CreateChartArea());
    //        return chart;
    //    }

    //    // Create chart title
    //    private Title CreateTitle()
    //    {
    //        return new Title()
    //        {
    //            Text = ChartTitle,
    //            ShadowColor = System.Drawing.Color.FromArgb(32, 0, 0, 0),
    //            Font = new System.Drawing.Font("Trebuchet MS", 10, FontStyle.Bold),
    //            ShadowOffset = 3,
    //            ForeColor = System.Drawing.Color.FromArgb(26, 59, 105)
    //        };
    //    }

    //    // configure chart Legend
    //    private Legend CreateLegend()
    //    {
    //        return new Legend()
    //        {
    //            Docking = Docking.Bottom,
    //            Alignment = StringAlignment.Center,
    //            BackColor = System.Drawing.Color.Transparent,
    //            Font = new System.Drawing.Font(new System.Drawing.FontFamily("Trebuchet MS"), 8),
    //            LegendStyle = LegendStyle.Row
    //        };
    //    }

    //    // Configure the chart area - the chart frame x/y axes
    //    private ChartArea CreateChartArea()
    //    {
    //        var area = new ChartArea()
    //        {
    //            Name = ChartTitle,
    //            BackColor = System.Drawing.Color.Transparent,
    //        };

    //        area.AxisX.IsLabelAutoFit = true;
    //        area.AxisX.LabelStyle.Font =
    //            new System.Drawing.Font("Verdana,Arial,Helvetica,sans-serif",
    //                                    8F, FontStyle.Regular);
    //        area.AxisX.LineColor = System.Drawing.Color.FromArgb(64, 64, 64, 64);
    //        area.AxisX.MajorGrid.LineColor = System.Drawing.Color.FromArgb(64, 64, 64, 64);
    //        area.AxisX.Interval = 1;


    //        area.AxisY.LabelStyle.Font =
    //            new System.Drawing.Font("Verdana,Arial,Helvetica,sans-serif",
    //                                    8F, FontStyle.Regular);
    //        area.AxisY.LineColor = System.Drawing.Color.FromArgb(64, 64, 64, 64);
    //        area.AxisY.MajorGrid.LineColor = System.Drawing.Color.FromArgb(64, 64, 64, 64);

    //        return area;
    //    }
    //}

    //public class BrowserInformation
    //{
    //    public string Name { get; set; }
    //    public double Share { get; set; }
    //    public string Url { get; set; }
    //    public string ToolTip
    //    {
    //        get
    //        {
    //            return Name + " " + Share.ToString("#0.##%");
    //        }
    //    }
    //}

    //internal class BrowserShareChartData
    //{
    //    public string Title { get; set; }
    //    public int Width { get; set; }
    //    public int Height { get; set; }
    //    public List<BrowserInformation> ShareData { get; set; }

    //    public MemoryStream ChartImageStream()
    //    {
    //        var chart = new BrowserShareChart(this);
    //        return chart.GetChartImage(Width, Height);
    //    }

    //    public string ChartImageMap(string name)
    //    {
    //        var chart = new BrowserShareChart(this);
    //        return chart.GetChartImageMap(Width, Height, name);
    //    }
    //}

    //internal class BrowserShareRepository
    //{
    //    public static BrowserShareChartData GetBrowserShares()
    //    {
    //        var chartData = new BrowserShareChartData()
    //        {
    //            Title = "Browser usage on Wikipedia October 2011",
    //            Width = 450,
    //            Height = 300,
    //            ShareData = new List<BrowserInformation>()
    //        };

    //        // The following data is the true data from Wikipedia
    //        chartData.ShareData.Add(new BrowserInformation()
    //        {
    //            Name = "IE",
    //            Share = 0.342,
    //            Url = "http://en.wikipedia.org/wiki/Internet_Explorer"
    //        });

    //        chartData.ShareData.Add(new BrowserInformation()
    //        {
    //            Name = "Firefox",
    //            Share = 0.236,
    //            Url = "http://en.wikipedia.org/wiki/Firefox"
    //        });

    //        chartData.ShareData.Add(new BrowserInformation()
    //        {
    //            Name = "Chrome",
    //            Share = 0.206,
    //            Url = "http://en.wikipedia.org/wiki/Google_Chrome"
    //        });

    //        chartData.ShareData.Add(new BrowserInformation()
    //        {
    //            Name = "Safari",
    //            Share = 0.112,
    //            Url = "http://en.wikipedia.org/wiki/Safari_(web_browser)"
    //        });

    //        chartData.ShareData.Add(new BrowserInformation()
    //        {
    //            Name = "Other",
    //            Share = 0.104,
    //            Url = null
    //        });

    //        return chartData;
    //    }
    //}
    //internal class BrowserShareChart : ChartBase
    //{
    //    private BrowserShareChartData chartData;

    //    public BrowserShareChart(BrowserShareChartData chartData)
    //    {
    //        this.chartData = chartData;
    //    }

    //    // Add Chart Title
    //    protected override void AddChartTitle()
    //    {
    //        ChartTitle = chartData.Title;
    //    }

    //    // Override the AddChartSeries method to provide the chart data
    //    protected override void AddChartSeries()
    //    {
    //        ChartSeriesData = new List<Series>();
    //        var series = new Series()
    //        {
    //            ChartType = SeriesChartType.Pie,
    //            BorderWidth = 1
    //        };

    //        var shares = chartData.ShareData;
    //        foreach (var share in shares)
    //        {
    //            var point = new DataPoint();
    //            point.IsValueShownAsLabel = true;
    //            point.AxisLabel = share.Name;
    //            point.ToolTip = share.Name + " " +
    //                  share.Share.ToString("#0.##%");
    //            if (share.Url != null)
    //            {
    //                point.MapAreaAttributes = "href=\"" +
    //                      share.Url + "\"";
    //            }
    //            point.YValues = new double[] { share.Share };
    //            point.LabelFormat = "P1";
    //            series.Points.Add(point);
    //        }

    //        ChartSeriesData.Add(series);
    //    }
    //}
}
