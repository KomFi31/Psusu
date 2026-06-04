# FitnessCentar

WPF aplikacija za evidenciju trenera i klijenata u fitness centru.

Projekat je rađen kao kolokvijumski zadatak iz predmeta **PSUSU**. Aplikacija omogućava rad sa dve povezane tabele u bazi podataka: `Trainers` i `Clients`.

## Tehnologije

- C#
- WPF
- .NET 8
- Entity Framework Core
- SQL Server LocalDB

## Opis domena

Aplikacija modeluje jednostavan sistem fitness centra.

Relacija između tabela je:

```text
Trainer  1 --- N Client
