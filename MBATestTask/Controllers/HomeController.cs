using System.Diagnostics;
using MBATestTask.Data;
using Microsoft.AspNetCore.Mvc;
using MBATestTask.Models;
using MBATestTask.Models.WeatherAPI;
using MBATestTask.Utils;
using Microsoft.EntityFrameworkCore;

namespace MBATestTask.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly CitiesForecastContext _dbContext;
    private HttpClient httpClient;

    public HomeController(ILogger<HomeController> logger, CitiesForecastContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> WeatherDataUpdate(string? city, DateTime date)
    {
        //Запрос к апи погоды, ссылки из тестового задания не работают,
        //я использовал другой открытый апи для получения данных погоды AccuWeather 

        WeatherData? data;
        City? requestCityData;

        using (httpClient = new HttpClient())
        {
            //Find city by name
            string apiKey = $"?apikey={WebConstats.AcuWeatherApi}&details=true&metric=true";

            string apiGetCityBase = "http://dataservice.accuweather.com/locations/v1/cities/search";
            string apiGetCityQuery = $"&q={city}";

            HttpResponseMessage cityResp =
                await httpClient.GetAsync(new Uri(apiGetCityBase + apiKey + apiGetCityQuery));
            var cityDataJson = await cityResp.Content.ReadFromJsonAsync<List<CityData>>();


            string apiRespBase = $"http://dataservice.accuweather.com/forecasts/v1/daily/1day/{cityDataJson?[0].Key}";

            HttpResponseMessage responce = await httpClient.GetAsync(new Uri(apiRespBase + apiKey));
            data = await responce.Content.ReadFromJsonAsync<WeatherData>();

            Forecast forecastResp = new Forecast()
            {
                DateFrom = data.DailyForecasts[0].Date.ToString(),
                DateTo = data.DailyForecasts[0].Date.ToString(),
                Pressure = data.DailyForecasts[0].RealFeelTemperature.Maximum.Value,
                Temperature = data.DailyForecasts[0].Temperature.Maximum.Value
            };

            requestCityData = new City()
            {
                Name = cityDataJson?[0].EnglishName,
                Country = cityDataJson?[0].Country.EnglishName,
                Forecasts = { forecastResp }
            };
        }


        await SaveToDataBase(requestCityData);

        return View(requestCityData);
    }

    // Вспомогательная обобщенная функция для сохранения данных в бд. 
    private async Task SaveToDataBase(City? obj)
    {
        if (obj == null)
            return;

        var citiesFromDb = _dbContext.Cities
            .Include(c => c.Forecasts)
            .Any(c => c.Name == obj.Name);

        if (citiesFromDb)
        {
            City? updateItem = await _dbContext.Cities.Where(c => c.Name == obj.Name).FirstOrDefaultAsync();

            if (updateItem != null)
            {
                updateItem.Name = obj.Name;
                updateItem.Country = obj.Country;
                foreach (var objForecast in obj.Forecasts)
                {
                    if(!updateItem.Forecasts.Contains(objForecast))
                        updateItem.Forecasts.Add(objForecast);
                }
                _dbContext.Cities.Update(updateItem);
            }
        }
        else
        {
            await _dbContext.Cities.AddAsync(obj);
        }

        await _dbContext.SaveChangesAsync();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}