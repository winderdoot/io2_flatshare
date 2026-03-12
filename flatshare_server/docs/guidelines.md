# Uwagi i ustalenia co do backendu

## Decyzje które podjąłem
- Visual studio
- Kontrollery a nie minimal api

## Foldery

- Infrastructure
    - Services
    - Model (klasy domenowe/biznesowe)
    - Repositories
    - inne foldery których możemy potrzebować

## Kod

Kiedy i jak używać zwykłych klas:
- Do obiektów biznesowych
- Warto wtedy używać ```required``` na polach jeśli chcemy żeby c# pilnował żeby zawsze były obecne (bo inaczej mogą być nagle nullem)
- Klasy modelowe będą zapisywane w bazie danych, ale chcemy żeby były niezależne od entity framework
- Dlatego nie robimy pól typu *Id* ani kluczy obcych (zamiast kluczy obcych po prostu trzymamy referencję do innego obiektu)
- Aby przechowywać w bazie danych (W ```DbSet<ObiektBiznesowy>```) piszemy dodaktowy config kod w klasie DbSet

Kiedy i jak używać rekordów (record class):
- Do obiektów DTO, czyli wszystkiego co jest zwracane po HTTP przez kontrollery.
- Warto używać ```required``` kiedy tylko się da. Wyjątkiem jest tylko możliwość nulla jest pzrewidziana i obsługiwana (nullable)


**Uwaga**: Jak pole jest required to zazwyczaj używa się inicjalizacji typu
```cs
var foo = new Foo { pole1 = "cos" }
```
Bo konstruktor zaczyna się pruć że nie wolno ustawiać w nim pól required. Aby to naprawić trzeba dać na konstruktor atrybut
```[SetsRequiredMembers]```.

Amen. Teraz do roboty.