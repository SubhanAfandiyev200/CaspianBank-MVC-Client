using CaspianBank_MVC_FinalProject.Helpers;
using CaspianBank_MVC_FinalProject.ViewModels.CardDesigns;
using CaspianBank_MVC_FinalProject.ViewModels.CardHeroes;
using Microsoft.AspNetCore.Mvc;

namespace CaspianBank_MVC_FinalProject.ViewComponents
{
    // Home-un yuxarı hissəsi (hero): sol mətn + kart yelpazəsi. İki API çağırışı bir modeldə (CardHeroVM) birləşir.
    public class CardHeroViewComponent : ViewComponent
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public CardHeroViewComponent(IHttpClientFactory httpClientFactory,
                                     IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = new CardHeroVM();
            var client = _httpClientFactory.CreateClient("CaspianApi");

            // Hər biri ayrıca tutulur: biri alınmasa digəri yenə də göstərilir
            try
            {
                model.CardHero = await client.GetFromJsonAsync<CardHeroUIVM>("api/home/cardHero");
            }
            catch (HttpRequestException)
            {
                // sətir yoxdur (404) və ya API işləmir: view statik mətni göstərir
            }

            try
            {
                model.CardDesigns = await client.GetFromJsonAsync<List<CardDesignUIVM>>("api/home/cards")
                                    ?? new List<CardDesignUIVM>();
            }
            catch (HttpRequestException)
            {
                // view köhnə statik kartları göstərir
            }

            var baseUrl = _configuration["ApiSettings:BaseUrl"];
            foreach (var design in model.CardDesigns)
                design.Image = ApiUrl.ToAbsolute(baseUrl, design.Image);

            return View(model);
        }
    }
}
