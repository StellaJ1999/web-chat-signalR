# WebChatSignalR

## Beskrivning

Detta är en chattapplikation byggd med ASP.NET Core och SignalR. Applikationen låter flera användare ansluta sig till och skicka meddelanden i en chatt i realtid.
För att applikationen ska kunna användas måste användaren skriva in sitt användarnamn innan denne får åtkomst till chattfunktionaliteten, login är implementerat med cookie authentication med claims.

Meddelanden skickas till alla anslutna klienter och systemmeddelanden visas när andra användare ansluter och lämnar chatten.
SignalR använder websockets där det är möjligt men har även fallback-protokoll ifall att WebSockets inte fungerar. Kommunikation sker över HTTPS med TLS.

Användarnamn och och meddelanden valideras på servern och en enkel rate limiter är implementerad för att minska spam. 

## Tekniker

- ASP.NET Core
- SignalR
- JavaScript
- HTML
- CSS
- Cookie Authentication
- TLS via HTTPS

## Körinstruktioner

### Förkrav

- .NET SDK 10.0 eller senare
- Git
- En modern webbläsare, till exempel Chrome, Edge eller Firefox

### Klona projektet

```bash
git clone <repository-url>
cd WebChatSignalR
```
### Starta applikationen

```bash
dotnet run --project WebChatSignalR
```

Applikation startar normalt på: 

```
https://localhost:7271
http://localhost:5033
```

Öppna sedan webbläsaren och gå till:

```
https://localhost:7271
```

Om webbläsaren varnar för utvecklingscertifikatet kan du behöva lita på .NET:s lokala utvecklingscertifikat:

```bash
dotnet dev-certs https --trust
```

### Testa chatten

1. Öppna applikationen i en webbläsare.
2. Skriv ett användarnamn och logga in.
3. Öppna gärna samma adress i en till flik eller annan webbläsare.
4. Logga in med ett annat användarnamn.
5. Skicka meddelanden och kontrollera att de visas i realtid.

