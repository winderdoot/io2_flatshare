# Uwagi i ustalenia co do backendu

## Decyzje które podjąłem za was
- Visual studio
- Kontrollery a nie minimal api
- Komentarze w kodzie po angielsku
- Dokumentacja (ta tutaj) i youtrack już po polsku chyba że bardzo chcemy bardzo mówić do siebie nawzajem po angielsku

## Foldery

- Infrastructure
    - Services (Serwisy manipulują obiektami biznesowymi, wołają ich metody, tworzą nowe i zapisują w bazie przez repozytoria)
    - Model (klasy domenowe/biznesowe)
    - Repositories (Implementują interfejs, docelowa implementacja bazodanowa/azure blob - zapisują i pobierają obiekty)
    - inne foldery na kategorie rzeczy których możemy potrzebować

## Kod

**Settery**:
- Na dotnecie robiłem public settery na wszystkim i było spoko,
ale może teraz tego nie róbmy. Będzie powód by zrobić porządne metody w klasach.

**Kiedy i jak używać zwykłych klas:**
- Do obiektów biznesowych
- Warto wtedy używać ```required``` na polach jeśli chcemy żeby c# pilnował żeby zawsze były obecne (bo inaczej mogą być nagle nullem)
- Klasy modelowe będą zapisywane w bazie danych, ale chcemy żeby były niezależne od entity framework
- Dlatego nie robimy pól typu *Id* ani kluczy obcych, chyba że obiekt potrzebuje ich z przyczyn biznesowych (zamiast kluczy obcych po prostu trzymamy referencję do innego obiektu).
- Aby przechowywać w bazie danych (W ```DbSet<ObiektBiznesowy>```) piszemy dodaktowy config kod w klasie DbSet

**Kiedy i jak używać rekordów (record class):**
- Do obiektów DTO, czyli wszystkiego co jest zwracane po HTTP przez kontrollery.
- Warto używać ```required``` kiedy tylko się da. Wyjątkiem jest tylko możliwość nulla jest pzrewidziana i obsługiwana (nullable)


**Uwaga**: Jak pole jest required to zazwyczaj używa się inicjalizacji typu
```cs
var foo = new Foo { pole1 = "cos" }
```
Bo konstruktor zaczyna się pruć że nie wolno ustawiać w nim pól required. Aby to naprawić trzeba dać na konstruktor atrybut
```[SetsRequiredMembers]```.

**Konstruktory**:  
Ta część jest w trakcie tworzenia, kombinuję czy to ma sens.

Nie lubię super długich konstruktorów w postaci:
```cs
public User(string firstName, string lastName, string password, string imięBabci, string imięDziadka, ...)
{
    /* Ustawianie wszystkich pól dokładnie tak jak w parametrach. Jak dodasz nowe pole to na bank zapomnisz dopisać */
    ...
}
```

Zamiast tego może spróbujmy korzystać z Object Initializerów:
```cs
public class User
{
    // Required jeśli się da. Init pozwala ustawić pole w object initializerze bez robienia public settera
    public required Guid Id { get; init; }

    // Lub tak jeśli chcemy żeby był default
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    // Wszystkie pola muszą być albo required albo mieć default wartość
    // Dzięki temu nie potrzebujemy w ogóle konstruktora i świat jest trochę lepszym miejscem.
    ...
}

// I potem inicjalizacja tak

User newUser = new User { Id = Guid.NewGuid(), ... /* Kompilator wymusi od nas wszystkie required pola */ }
```
Przy okazji unikniemy w ten sposób różnych śmiesznych błędów w entity frameworku, których nie lubię debugować.

Amen. Teraz do roboty.