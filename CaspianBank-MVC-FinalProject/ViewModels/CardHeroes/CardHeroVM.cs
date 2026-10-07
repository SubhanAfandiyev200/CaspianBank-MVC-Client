using CaspianBank_MVC_FinalProject.ViewModels.CardDesigns;

namespace CaspianBank_MVC_FinalProject.ViewModels.CardHeroes
{
    // Hero bölməsi iki mənbədən qurulur: mətnlər (CardHero) və kart dizaynları (CardDesign)
    public class CardHeroVM
    {
        // null olarsa (bazada sətir yoxdur və ya API işləmir) view statik mətni göstərir
        public CardHeroUIVM? CardHero { get; set; }
        public List<CardDesignUIVM> CardDesigns { get; set; } = new();
    }
}
