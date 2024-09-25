

using System.Text.Json;
using PuppeteerSharp;

//下載瀏覽器，若已下載過則不會再次下載
await new BrowserFetcher().DownloadAsync();
//使用 headless 模式 (不顯示瀏覽器) 啟動瀏覽器
var browser = await Puppeteer.LaunchAsync(new LaunchOptions 
{ 
    Headless = true
}); 
var page = await browser.NewPageAsync(); 
await page.GoToAsync("https://tw.stock.yahoo.com/class-quote?sectorId=26&exchange=TAI"); 
//使用 css 選擇器取得所有股票眕內容，再透過 XPath 取得股票詳細資訊
var jsSelectAllStocks = @"Array.from(document.querySelectorAll('li[class=""List(n)""]')).map(a => {
    return {
        Name: document.evaluate('./div/div[1]/div[2]/div/div[1]', a, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue.innerText,
        Symbol: document.evaluate('./div/div[1]/div[2]/div/div[2]', a, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue.innerText,
        Price: document.evaluate('./div/div[2]', a, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue.innerText,
        PriceChange: document.evaluate('./div/div[3]', a, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue.innerText,
        Change: document.evaluate('./div/div[4]', a, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue.innerText,
        Open: document.evaluate('./div/div[5]', a, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue.innerText,
        LastClose: document.evaluate('./div/div[6]', a, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue.innerText,
        High: document.evaluate('./div/div[7]', a, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue.innerText,
        Low: document.evaluate('./div/div[8]', a, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue.innerText,
        Turnover: document.evaluate('./div/div[9]', a, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue.innerText,
        UpDown: document.evaluate('./div/div[3]/span', a, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue.className
    };
});";

var matchHtml = await page.EvaluateExpressionAsync<JsonDocument[]>(jsSelectAllStocks);
var stocks = matchHtml.Select(a=>JsonSerializer.Deserialize<Stock>(a));

foreach(var stock in stocks) 
{
    Console.WriteLine($"股票名稱: {stock.Name.PadRight(12)}\t 股票代號: {stock.Symbol}\t 股價: {stock.Price.PadRight(5)}\t 漲跌: {stock.UpDown} {stock.PriceChange.PadRight(8)}\t 漲跌幅: {stock.UpDown} {stock.Change.PadRight(8)}\t 開盤: {stock.Open}\t 昨收: {stock.LastClose}\t 最高: {stock.High}\t 最低: {stock.Low}\t 成交量(張): {stock.Turnover}");
} 

class Stock
{
    public string Name { get; set; }
    public string Symbol { get; set; }
    public string Price { get; set; }
    public string Change { get; set; }
    public string PriceChange { get; set; }
    public string Open { get; set; }
    public string LastClose { get; set; }
    public string High { get; set; }
    public string Low { get; set; }
    public string Turnover { get; set; }
    private string _upDown;

    public string UpDown
    {
        get => _upDown;
        set => _upDown = UpDownCheck(value);
    }
    string UpDownCheck(string value)
    {
        if (value.Contains("up"))
        {
            return "上漲";
        }
        if (value.Contains("down"))
        {
            return "下跌";
        }
        return string.Empty;
    }
} 