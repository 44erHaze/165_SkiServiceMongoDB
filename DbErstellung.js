// Verbinden Sie sich mit Ihrer MongoDB-Instanz (ersetzen Sie 'your_connection_string' und 'SkiServiceDB' durch die entsprechenden Werte)
var connection = new Mongo("your_connection_string");
var db = connection.getDB("SkiServiceDB");


db.createCollection("Auftraege");

db.Aufträge.insert({
    Kundenname: "Erster Kunde",
    EMail: "erster.kunde@gmail.com",
    Telefon: "079 987 65 43",
    Prioritaet: "hoch",
    Dienstleistung: "Komplett Service"
});

db.Aufträge.insert({
    Kundenname: "Hans Peter",
    EMail: "hans.peter@gmail.com",
    Telefon: "079 123 45 67",
    Prioritaet: "standart",
    Dienstleistung: "Heisswaschen"
});


db.createCollection("Benutzerrollen");


db.Benutzerrollen.insert({
    "Rolle": "Lesen"
});

db.Benutzerrollen.insert({
    "Rolle": "Schreiben"
});

db.Aufträge.createIndex({ Kundenname: 1 });
db.Aufträge.createIndex({ EMail: 1 });
db.Aufträge.createIndex({ Telefon: 1 });