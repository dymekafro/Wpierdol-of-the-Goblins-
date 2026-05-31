using System.Collections.Generic;
using System.Linq;

public static class CharacterClassDatabase
{
    private static readonly List<CharacterClassDefinition> Definitions = new List<CharacterClassDefinition>
    {
        new CharacterClassDefinition(
            CharacterClassType.Warrior,
            "Wojownik",
            "Wytrzymały specjalista walki bezpośredniej. Dobrze znosi obrażenia i utrzymuje linię frontu.",
            "Walka wręcz, blokowanie, przewaga w starciach frontalnych.",
            new CharacterStatsData(8, 5, 2, 7, 3, 3),
            new[] { "Stary miecz", "Prosta tarcza", "Skórzana zbroja" },
            new[] { "Ciężkie cięcie", "Blok" }),

        new CharacterClassDefinition(
            CharacterClassType.Mage,
            "Mag",
            "Postać oparta na inteligencji i kontroli dystansu. Słabszy fizycznie, ale z wysokim potencjałem magicznym.",
            "Zaklęcia dystansowe, bariery, kontrola pola walki.",
            new CharacterStatsData(2, 3, 9, 4, 5, 5),
            new[] { "Drewniana różdżka", "Szata ucznia", "Mała mikstura many" },
            new[] { "Iskra ognia", "Magiczna bariera" }),

        new CharacterClassDefinition(
            CharacterClassType.Archer,
            "Łucznik",
            "Mobilny strzelec dystansowy. Bazuje na zręczności, percepcji i unikaniu bezpośrednich starć.",
            "Dystans, celne strzały, utrzymywanie pozycji.",
            new CharacterStatsData(4, 8, 3, 4, 8, 3),
            new[] { "Prosty łuk", "20 strzał", "Lekki kaptur" },
            new[] { "Celny strzał", "Odskok" }),

        new CharacterClassDefinition(
            CharacterClassType.Rogue,
            "Łotrzyk",
            "Szybka postać do ataków z zaskoczenia. Wysoka zręczność i dobre narzędzia eksploracyjne.",
            "Uniki, ataki krytyczne, skradanie i szybkie wejścia w walkę.",
            new CharacterStatsData(4, 9, 4, 4, 6, 4),
            new[] { "Sztylet", "Wytrych", "Ciemny płaszcz" },
            new[] { "Cios w plecy", "Unik" }),

        new CharacterClassDefinition(
            CharacterClassType.Dwarf,
            "Krasnolud",
            "Twardy wojownik o bardzo wysokiej wytrzymałości. Wolniejszy, ale odporny i stabilny.",
            "Przetrwanie, mocne uderzenia, krótkodystansowa presja.",
            new CharacterStatsData(7, 3, 3, 9, 4, 3),
            new[] { "Topór górniczy", "Gruby pas", "Mały zapas jedzenia" },
            new[] { "Twarda skóra", "Mocne uderzenie" }),

        new CharacterClassDefinition(
            CharacterClassType.Wanderer,
            "Wędrowiec",
            "Wszechstronna klasa startowa bez skrajnych słabości. Dobra do eksploracji i nauki systemów gry.",
            "Eksploracja, adaptacja, umiarkowana walka wręcz i mobilność.",
            new CharacterStatsData(5, 6, 4, 6, 6, 4),
            new[] { "Kostur podróżny", "Bukłak", "Prowizoryczny płaszcz" },
            new[] { "Tropienie", "Szybki marsz" }),

        new CharacterClassDefinition(
            CharacterClassType.Barbarian,
            "Barbarzyńca",
            "Dzika, agresywna postać nastawiona na maksymalne obrażenia w zwarciu. Słaby magicznie, ale druzgocący w bezpośrednim starciu.",
            "Berserk, ciężkie uderzenia obszarowe, presja i parcie do przodu.",
            new CharacterStatsData(9, 5, 1, 8, 3, 2),
            new[] { "Wielki topór", "Skóry bojowe", "Amulet furii" },
            new[] { "Szał bojowy", "Rozłupanie" }),

        new CharacterClassDefinition(
            CharacterClassType.Ranger,
            "Łowca",
            "Leśny tropiciel łączący strzelectwo z przetrwaniem w dziczy. Mobilny, spostrzegawczy i samowystarczalny.",
            "Walka dystansowa, pułapki, tropienie i kontrola terenu.",
            new CharacterStatsData(4, 8, 2, 5, 7, 3),
            new[] { "Łuk myśliwski", "Kołczan strzał", "Płaszcz leśny" },
            new[] { "Strzał celowany", "Zastawienie pułapki" }),

        new CharacterClassDefinition(
            CharacterClassType.Knight,
            "Rycerz",
            "Ciężkozbrojny obrońca o wysokiej wytrzymałości i charyzmie. Trzon drużyny i ściana w obronie sojuszników.",
            "Tankowanie, prowokowanie wrogów, obrona i walka wręcz w pełnej zbroi.",
            new CharacterStatsData(7, 3, 3, 8, 2, 5),
            new[] { "Miecz długi", "Tarcza herbowa", "Zbroja płytowa" },
            new[] { "Prowokacja", "Mur tarczy" }),

        new CharacterClassDefinition(
            CharacterClassType.Druid,
            "Druid",
            "Opiekun natury czerpiący moc z lasu. Łączy magię żywiołów z leczeniem i wsparciem — domyślny archetyp świata gry.",
            "Magia natury, leczenie, przyzwania i kontrola pola walki.",
            new CharacterStatsData(3, 5, 8, 6, 4, 4),
            new[] { "Kostur druida", "Zielarski woreczek", "Szata z liści" },
            new[] { "Splątanie korzeni", "Odnowienie" })
    };

    public static IReadOnlyList<CharacterClassDefinition> All => Definitions;

    public static CharacterClassDefinition Get(CharacterClassType type)
    {
        return Definitions.FirstOrDefault(definition => definition.classType == type);
    }
}
