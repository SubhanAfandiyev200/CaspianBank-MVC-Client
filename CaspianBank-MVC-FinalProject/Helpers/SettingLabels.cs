namespace CaspianBank_MVC_FinalProject.Helpers
{
    // Settings cədvəlindəki açarların admin panelində göstərilən adı və izahı. Açarlar API-dəki SettingKeys ilə eynidir
    public static class SettingLabels
    {
        public const string Logo = "Logo";

        public static bool IsLogo(string key)
        {
            return key == Logo;
        }

        // Saytın mətnidir, dilə görə tərcümə olunur (Edit səhifəsində EN | AZ | RU seçimi çıxır). URL, email, telefon və s. tərcümə olunmur
        public static bool IsTranslatable(string key)
        {
            return key == "FooterTitle" || key == "FooterDescription";
        }

        // Uzun mətn olduğu üçün çox sətirli sahədə redaktə olunur
        public static bool IsLong(string key)
        {
            return key == "FooterDescription";
        }

        public static string For(string key)
        {
            switch (key)
            {
                case "Logo":
                    return "Logo";
                case "CompanyName":
                    return "Company name";
                case "Address":
                    return "Address";
                case "Email":
                    return "Contact email";
                case "PhoneNumber":
                    return "Phone number";
                case "FooterTitle":
                    return "Footer title";
                case "FooterDescription":
                    return "Footer text";
                case "Copyright":
                    return "Copyright line";
                case "InstagramUrl":
                    return "Instagram link";
                case "FacebookUrl":
                    return "Facebook link";
                case "TwitterUrl":
                    return "X (Twitter) link";
                case "LinkedInUrl":
                    return "LinkedIn link";
                case "MapLatitude":
                    return "Map latitude";
                case "MapLongitude":
                    return "Map longitude";
                default:
                    return key;
            }
        }

        public static string Hint(string key)
        {
            switch (key)
            {
                case "Logo":
                    return "The logo in the header and the footer.";
                case "CompanyName":
                    return "The name next to the logo, up to 100 characters.";
                case "Address":
                    return "The office address in the footer, up to 200 characters.";
                case "Email":
                    return "The contact email in the footer.";
                case "PhoneNumber":
                    return "The contact phone in the footer, for example +994 12 345 67 89.";
                case "FooterTitle":
                    return "The big heading at the top of the footer, up to 200 characters.";
                case "FooterDescription":
                    return "The text under the footer heading, up to 500 characters.";
                case "Copyright":
                    return "The copyright line at the very bottom, up to 100 characters.";
                case "InstagramUrl":
                case "FacebookUrl":
                case "TwitterUrl":
                case "LinkedInUrl":
                    return "Where the icon in the footer opens. Write the full link, for example https://www.instagram.com/caspianbank.";
                case "MapLatitude":
                    return "The map on the Contact us page. The latitude of the office, with a dot, for example 40.3730. In Google Maps right-click the place and copy the first number.";
                case "MapLongitude":
                    return "The map on the Contact us page. The longitude of the office, with a dot, for example 49.8370. In Google Maps right-click the place and copy the second number. Keep the Address setting in step with the map.";
                default:
                    return string.Empty;
            }
        }
    }
}
