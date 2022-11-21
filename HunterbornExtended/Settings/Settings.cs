using System;

namespace HunterbornExtender.Settings
{
    public class Settings
    {
        public General general = new();
        public Weapons weapons = new();
        public Armors armors = new();

        public class General
        {
            public string unclassifeidStr { get; set; } = "UNCLASSIFIED";
            public string unknownSlotStr { get; set; } = "UNKNOWN_SLOT";
        }

        public class Weapons
        {
            public bool enable { get; set; } = false;
            public bool handedness { get; set; } = false;
            public string oneHandText { get; set; } = "1H";
            public string twoHandText { get; set; } = "2H";
            public bool handednessAfter { get; set; } = true;

            public string oneHSwordText { get; set; } = "Sword";
            public string twoHSwordText { get; set; } = "Greatsword";
            public string oneHAxeText { get; set; } = "Axe";
            public string twoHAxeText { get; set; } = "Battleaxe";
            public string oneHHammerText { get; set; } = "Mace";
            public string twoHHammerText { get; set; } = "Warhammer";
            public string DaggerText { get; set; } = "Dagger";
            public string BowText { get; set; } = "Bow";
            public string CrossbowText { get; set; } = "Crossbow";
            public string StaffText { get; set; } = "Staff";
        }

        public class Armors
        {
            public bool enable { get; set; } = true;
            public bool jewelryEnable { get; set; } = false;
            public string jewelryClassText { get; set; } = "Jewelry";
            public bool accessoryEnable { get; set; } = false;
            public string accessorySlotName { get; set; } = "Accessory";
            public bool packEnable { get; set; } = false;
            public string packSlotName { get; set; } = "Pack";
            public bool cloakEnable { get; set; } = true;
            public string cloakSlotName { get; set; } = "Cloak";
            public string feetSlotName { get; set; } = "Feet";
            public string handsSlotName { get; set; } = "Hands";
            public bool deviousEnable { get; set; } = true;
            public string deviousName { get; set; } = "DD";
        }

        public class Books
        {
            public bool recipes { get; set; } = true;
            public string recipeText { get; set; } = "Recipe";
        }
        public Books books = new();

        public class Ammo
        {
            public bool enable { get; set; } = true;
            public string arrowText { get; set; } = "Arrow";
            public string boltText { get; set; } = "Bolt";

        }
        public Ammo ammo = new();

        public class Ingestibles
        {
            public bool potionsEnable { get; set; } = true;
            public string potionsText { get; set; } = "Potion";
            public bool poisonsEnable { get; set; } = true;
            public string poisonsText { get; set; } = "Poison";
            public bool cookedFoodEnable { get; set; } = true;
            public string cookedFoodText { get; set; } = "Food";
            public bool rawFoodEnable { get; set; } = true;
            public string rawFoodText { get; set; } = "Raw";
            public bool identifyDrinks { get; set; } = true;
            public bool identifyDrugs { get; set; } = true;
            public bool identifyAlcohol { get; set; } = true;
            public string stewText { get; set; } = "Stew";

            public string drinkText { get; set; } = "Drink";
            public string drugText { get; set; } = "Drug";
            public string alcoholText { get; set; } = "Alcohol";

        }
        public Ingestibles ingestibles = new();

        public class Soulgems
        {
            public bool enable { get; set; } = true;
            public string prefixText { get; set; } = "Soul Gem";
            public string filledText { get; set; } = " [Filled]";
            public string pettyText { get; set; } = " I - Petty";
            public string lesserText { get; set; } = " II - Lesser";
            public string commonText { get; set; } = " III - Common";
            public string greaterText { get; set; } = " IV - Greater";
            public string grandText { get; set; } = " V - Grand";
            public string blackText { get; set; } = " VI - Black";
            public string artifactText { get; set; } = " X - ";
        }
        public Soulgems soulgems = new();


        public class Misc
        {
            public bool enable { get; set; } = true;
            public bool ore { get; set; } = true;
            public bool ingots { get; set; } = true;
            public bool hide { get; set; } = true;
            public bool leather { get; set; } = true;
            public bool gems { get; set; } = true;
            public bool clutter { get; set; } = true;

        }
        public Misc misc = new();

    }


}
