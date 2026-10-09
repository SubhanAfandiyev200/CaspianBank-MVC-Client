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
                default:
                    return string.Empty;
            }
        }
    }
}
