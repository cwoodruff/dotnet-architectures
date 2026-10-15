# The domain

Every example in this repo implements the same domain against the same database: Chinook, a small digital music store. This document is the contract. When an example is finished, it covers every use case below, publishes the events where its architecture calls for events, and holds every invariant. If it skips something, its README says what and why.

The source of truth for the schema is `database/chinook_with_sprocs-id-pk.sql`. Everything in this document was read from that script, not from memory. Table and column names are used exactly as the script spells them, including the ones that are awkward (`SupportRepId`, `ReportsTo`). An example may give its own types friendlier names, but the mapping has to land on these columns.

## Naming

Every example uses `Chinook` as its root namespace prefix, followed by a short name for the architecture and then the project role: `Chinook.NTier.Web`, `Chinook.VerticalSlice.Api`, `Chinook.ModMono.Modules.Catalog`, and so on. Test projects are `<Prefix>.Tests`.

## Entities

Eleven tables and one view, all in the `dbo` schema. Every table except `PlaylistTrack` has an `int` primary key named `Id` declared `IDENTITY(1,1)`. `PlaylistTrack` is a pure join table whose primary key is the pair (`PlaylistId`, `TrackId`).

### Artist

| Column | Type | Null |
| --- | --- | --- |
| `Id` | int identity | no |
| `Name` | nvarchar(120) | yes |

### Album

| Column | Type | Null | References |
| --- | --- | --- | --- |
| `Id` | int identity | no | |
| `Title` | nvarchar(160) | no | |
| `ArtistId` | int | no | `Artist.Id` |

### Track

| Column | Type | Null | References |
| --- | --- | --- | --- |
| `Id` | int identity | no | |
| `Name` | nvarchar(200) | no | |
| `AlbumId` | int | yes | `Album.Id` |
| `MediaTypeId` | int | no | `MediaType.Id` |
| `GenreId` | int | yes | `Genre.Id` |
| `Composer` | nvarchar(220) | yes | |
| `Milliseconds` | int | no | |
| `Bytes` | int | yes | |
| `UnitPrice` | numeric(10,2) | no | |

### Genre

| Column | Type | Null |
| --- | --- | --- |
| `Id` | int identity | no |
| `Name` | nvarchar(120) | yes |

### MediaType

| Column | Type | Null |
| --- | --- | --- |
| `Id` | int identity | no |
| `Name` | nvarchar(120) | yes |

### Playlist

| Column | Type | Null |
| --- | --- | --- |
| `Id` | int identity | no |
| `Name` | nvarchar(120) | yes |

### PlaylistTrack

| Column | Type | Null | References |
| --- | --- | --- | --- |
| `PlaylistId` | int | no | `Playlist.Id` |
| `TrackId` | int | no | `Track.Id` |

Primary key: (`PlaylistId`, `TrackId`). The database already rejects a duplicate track on a playlist; see invariant 2 for why the examples check it anyway.

### Customer

| Column | Type | Null | References |
| --- | --- | --- | --- |
| `Id` | int identity | no | |
| `FirstName` | nvarchar(40) | no | |
| `LastName` | nvarchar(20) | no | |
| `Company` | nvarchar(80) | yes | |
| `Address` | nvarchar(70) | yes | |
| `City` | nvarchar(40) | yes | |
| `State` | nvarchar(40) | yes | |
| `Country` | nvarchar(40) | yes | |
| `PostalCode` | nvarchar(10) | yes | |
| `Phone` | nvarchar(24) | yes | |
| `Fax` | nvarchar(24) | yes | |
| `Email` | nvarchar(60) | no | |
| `SupportRepId` | int | yes | `Employee.Id` |

`Customer.SupportRepId` links a customer to the employee who supports them. It is the only link between the sales side and the staff side of the schema.

### Employee

| Column | Type | Null | References |
| --- | --- | --- | --- |
| `Id` | int identity | no | |
| `LastName` | nvarchar(20) | no | |
| `FirstName` | nvarchar(20) | no | |
| `Title` | nvarchar(30) | yes | |
| `ReportsTo` | int | yes | `Employee.Id` |
| `BirthDate` | datetime | yes | |
| `HireDate` | datetime | yes | |
| `Address` | nvarchar(70) | yes | |
| `City` | nvarchar(40) | yes | |
| `State` | nvarchar(40) | yes | |
| `Country` | nvarchar(40) | yes | |
| `PostalCode` | nvarchar(10) | yes | |
| `Phone` | nvarchar(24) | yes | |
| `Fax` | nvarchar(24) | yes | |
| `Email` | nvarchar(60) | yes | |

`Employee.ReportsTo` is a self-reference: it points at another row in `Employee`. A null `ReportsTo` marks the top of the org chart.

### Invoice

| Column | Type | Null | References |
| --- | --- | --- | --- |
| `Id` | int identity | no | |
| `CustomerId` | int | no | `Customer.Id` |
| `InvoiceDate` | datetime | no | |
| `BillingAddress` | nvarchar(70) | yes | |
| `BillingCity` | nvarchar(40) | yes | |
| `BillingState` | nvarchar(40) | yes | |
| `BillingCountry` | nvarchar(40) | yes | |
| `BillingPostalCode` | nvarchar(10) | yes | |
| `Total` | numeric(10,2) | no | |

### InvoiceLine

| Column | Type | Null | References |
| --- | --- | --- | --- |
| `Id` | int identity | no | |
| `InvoiceId` | int | no | `Invoice.Id` |
| `TrackId` | int | no | `Track.Id` |
| `UnitPrice` | numeric(10,2) | no | |
| `Quantity` | int | no | |

### AlbumWithArtistName (view)

| Column | Source |
| --- | --- |
| `AlbumId` | `Album.Id` |
| `Title` | `Album.Title` |
| `ArtistId` | `Album.ArtistId` |
| `Name` | `Artist.Name` |

The canonical script defines this view against column names that no longer exist in the id-pk variant. `database/generate-docker-sql.sh` repairs it when it builds `chinook.docker.sql`; see `database/README.md`.

### Foreign keys at a glance

```
Album.ArtistId          -> Artist.Id
Track.AlbumId           -> Album.Id         (nullable)
Track.GenreId           -> Genre.Id         (nullable)
Track.MediaTypeId       -> MediaType.Id
PlaylistTrack.PlaylistId -> Playlist.Id
PlaylistTrack.TrackId   -> Track.Id
Customer.SupportRepId   -> Employee.Id      (nullable)
Employee.ReportsTo      -> Employee.Id      (nullable, self-reference)
Invoice.CustomerId      -> Customer.Id
InvoiceLine.InvoiceId   -> Invoice.Id
InvoiceLine.TrackId     -> Track.Id
```

None of the foreign keys cascade on delete.

### What the sample data looks like

275 artists, 347 albums, 3,503 tracks, 59 customers, 8 employees, and 458 invoices dated between 2007-01-02 and 2010-12-27. Any query relative to "now" (the worker service's last-24-hours digest, for example) returns zero against the unmodified data. That is expected.

## Stored procedures

The script ships 80 stored procedures. Most follow one pattern per table:

| Family | Shape |
| --- | --- |
| `sproc_Check<T>` | Takes an id, returns a `bit` saying whether the row exists. |
| `sproc_Get<T>` | No parameters, returns every row. |
| `sproc_Get<T>Details` | Takes an id, returns that row. |
| `sproc_Insert<T>` | Takes the columns, returns the new id through an `OUTPUT` parameter. |
| `sproc_Update<T>` | Takes the id and the columns. |
| `sproc_Delete<T>` | Takes the id. |

On top of those there are relationship getters: `sproc_GetAlbumByArtist`, `sproc_GetTrackByAlbum`, `sproc_GetTrackByPlaylist`, `sproc_GetInvoiceByCustomer`, `sproc_GetCustomerBySupportRep`, `sproc_GetEmployeeDirectReports`, `sproc_GetEmployeeReportTo`, and so on. The full list is in the appendix.

Two things matter to anyone calling these by name:

1. **Every `Get` procedure returns JSON, not rows.** The body is `SELECT CAST((SELECT ... FOR JSON PATH) AS VARCHAR(MAX)) AS JSONDATA`, so the result is a single row with one string column holding a JSON array. A Dapper caller reads that string and deserializes it. A query that matches nothing returns a single `NULL`, not an empty array.
2. **The procedures cover single-table access and one-hop relationships only.** Anything that aggregates or joins further (album counts per artist, revenue by country, top tracks) needs SQL the procedures do not provide. The examples that use stored procedures say where that happens; it is one of the lessons.

`sproc_InsertInvoice` and `sproc_InsertInvoiceLine` take `money` parameters for `Total` and `UnitPrice` while the columns are `numeric(10,2)`. The procedures do not compute `Total` or look up `UnitPrice`. Invariants 1 and 4 are the caller's job.

## Use cases

Numbered so test suites can refer to them: `UC1` through `UC9`.

1. **Browse the catalog.** List artists, then the albums for one artist, then the tracks on one album.
2. **Search tracks** by genre and media type, with paging. Both filters are optional; the result is ordered and paged deterministically.
3. **Create a customer and assign a support rep.** The rep must be an existing employee.
4. **Create an invoice** for a customer with one or more lines. Each line's `UnitPrice` is copied from `Track.UnitPrice` at the time of sale, and `Invoice.Total` is computed from the lines. Billing fields default from the customer's address. Publishes `InvoiceCreated`.
5. **Create a playlist and add or remove tracks.** Adding a track already on the playlist is rejected with a clear error, not a database exception. Publishes `TrackAddedToPlaylist` and `TrackRemovedFromPlaylist`.
6. **Change a customer's support rep.** Publishes `CustomerSupportRepChanged`.
7. **Show an employee's org chart:** who they report to and their direct reports.
8. **Show a customer's purchase history:** their invoices, newest first, with each line and the track name.
9. **Sales report:** revenue by billing country, and the top ten tracks by units sold. This is the read-heavy path, and the one where the examples that separate reads from writes should look different.

Use case 4 is the one to trace when comparing two examples. It has the most invariants, crosses the most tables, and publishes an event. See `docs/comparing-examples.md`.

## Domain events

The messaging, event-driven, and event-sourcing examples publish these. Other examples may ignore them. Field lists are the minimum; an example may add to them.

| Event | Raised by | Carries |
| --- | --- | --- |
| `InvoiceCreated` | UC4 | `InvoiceId`, `CustomerId`, `InvoiceDate`, `Total`, `BillingCountry`, the lines (`TrackId`, `UnitPrice`, `Quantity`) |
| `TrackAddedToPlaylist` | UC5 | `PlaylistId`, `TrackId` |
| `TrackRemovedFromPlaylist` | UC5 | `PlaylistId`, `TrackId` |
| `CustomerSupportRepChanged` | UC6 | `CustomerId`, `PreviousSupportRepId`, `NewSupportRepId` |

The event-sourcing example also needs a `PlaylistCreated` event (`PlaylistId`, `Name`) to start a playlist stream. It is internal to that example's aggregate and is not part of the shared contract.

## Invariants

1. `Invoice.Total` equals the sum of `InvoiceLine.UnitPrice * InvoiceLine.Quantity` for that invoice.
2. A track appears at most once on a given playlist. The composite primary key on `PlaylistTrack` enforces this in the database. The examples check it in code too, so the user gets a domain error instead of a constraint violation, and so the event-sourced example (which has no such key on its event stream) holds the same rule.
3. An employee cannot report to themselves: `Employee.ReportsTo <> Employee.Id`. The database does not enforce this.
4. `InvoiceLine.UnitPrice` is a snapshot. Later changes to `Track.UnitPrice` do not alter past invoices. Nothing ever writes `InvoiceLine.UnitPrice` from a join at read time.

## Appendix: stored procedures

Generated from the canonical script by parsing every `CREATE PROCEDURE` block (`grep -c "CREATE PROCEDURE"` returns 80).

| Procedure | Parameters | Returns |
| --- | --- | --- |
| `sproc_CheckAlbum` | @AlbumId int | bit |
| `sproc_CheckArtist` | @ArtistId int | bit |
| `sproc_CheckCustomer` | @CustomerId int | bit |
| `sproc_CheckEmployee` | @EmployeeId int | bit |
| `sproc_CheckGenre` | @GenreId int | bit |
| `sproc_CheckInvoice` | @InvoiceId int | bit |
| `sproc_CheckInvoiceLine` | @InvoiceLineId int | bit |
| `sproc_CheckMediaType` | @MediaTypeId int | bit |
| `sproc_CheckPlaylist` | @PlaylistId int | bit |
| `sproc_CheckTrack` | @TrackId int | bit |
| `sproc_DeleteAlbum` | @AlbumId int | — |
| `sproc_DeleteArtist` | @ArtistId int | — |
| `sproc_DeleteCustomer` | @CustomerId int | — |
| `sproc_DeleteEmployee` | @EmployeeId int | — |
| `sproc_DeleteGenre` | @GenreId int | — |
| `sproc_DeleteInvoice` | @InvoiceId int | — |
| `sproc_DeleteInvoiceLine` | @InvoiceLineId int | — |
| `sproc_DeleteMediaType` | @MediaTypeId int | — |
| `sproc_DeletePlaylist` | @PlaylistId int | — |
| `sproc_DeletePlaylistTrack` | @PlaylistId int, @TrackId int | — |
| `sproc_DeleteTrack` | @TrackId int | — |
| `sproc_GetAlbum` | — | JSON (one `JSONDATA` row) |
| `sproc_GetAlbumByArtist` | @ArtistId int | JSON (one `JSONDATA` row) |
| `sproc_GetAlbumDetails` | @AlbumId int | JSON (one `JSONDATA` row) |
| `sproc_GetArtist` | — | JSON (one `JSONDATA` row) |
| `sproc_GetArtistDetails` | @ArtistId int | JSON (one `JSONDATA` row) |
| `sproc_GetCustomer` | — | JSON (one `JSONDATA` row) |
| `sproc_GetCustomerBySupportRep` | @SupportRepId int | JSON (one `JSONDATA` row) |
| `sproc_GetCustomerDetails` | @CustomerId int | JSON (one `JSONDATA` row) |
| `sproc_GetEmployee` | — | JSON (one `JSONDATA` row) |
| `sproc_GetEmployeeDetails` | @EmployeeId int | JSON (one `JSONDATA` row) |
| `sproc_GetEmployeeDirectReports` | @EmployeeId int | JSON (one `JSONDATA` row) |
| `sproc_GetEmployeeReportTo` | @EmployeeId int | JSON (one `JSONDATA` row) |
| `sproc_GetGenre` | — | JSON (one `JSONDATA` row) |
| `sproc_GetGenreDetails` | @GenreId int | JSON (one `JSONDATA` row) |
| `sproc_GetInvoice` | — | JSON (one `JSONDATA` row) |
| `sproc_GetInvoiceByCustomer` | @CustomerId int | JSON (one `JSONDATA` row) |
| `sproc_GetInvoiceByEmployee` | @EmployeeId int | JSON (one `JSONDATA` row) |
| `sproc_GetInvoiceDetails` | @InvoiceId int | JSON (one `JSONDATA` row) |
| `sproc_GetInvoiceLine` | — | JSON (one `JSONDATA` row) |
| `sproc_GetInvoiceLineByInvoice` | @InvoiceId int | JSON (one `JSONDATA` row) |
| `sproc_GetInvoiceLineByTrack` | @TrackId int | JSON (one `JSONDATA` row) |
| `sproc_GetInvoiceLineDetails` | @InvoiceLineId int | JSON (one `JSONDATA` row) |
| `sproc_GetMediaType` | — | JSON (one `JSONDATA` row) |
| `sproc_GetMediaTypeDetails` | @MediaTypeId int | JSON (one `JSONDATA` row) |
| `sproc_GetPlaylist` | — | JSON (one `JSONDATA` row) |
| `sproc_GetPlaylistByTrack` | @TrackId int | JSON (one `JSONDATA` row) |
| `sproc_GetPlaylistDetails` | @PlaylistId int | JSON (one `JSONDATA` row) |
| `sproc_GetPlaylistTrack` | — | JSON (one `JSONDATA` row) |
| `sproc_GetPlaylistTrackDetails` | @PlaylistId int, @TrackId int | JSON (one `JSONDATA` row) |
| `sproc_GetTrack` | — | JSON (one `JSONDATA` row) |
| `sproc_GetTrackByAlbum` | @AlbumId int | JSON (one `JSONDATA` row) |
| `sproc_GetTrackByArtist` | @ArtistId int | JSON (one `JSONDATA` row) |
| `sproc_GetTrackByGenre` | @GenreId int | JSON (one `JSONDATA` row) |
| `sproc_GetTrackByInvoice` | @InvoiceId int | JSON (one `JSONDATA` row) |
| `sproc_GetTrackByMediaType` | @MediaTypeId int | JSON (one `JSONDATA` row) |
| `sproc_GetTrackByPlaylist` | @PlaylistId int | JSON (one `JSONDATA` row) |
| `sproc_GetTrackDetails` | @TrackId int | JSON (one `JSONDATA` row) |
| `sproc_InsertAlbum` | @Title nvarchar, @ArtistId int, @AlbumId int OUT | new Id via OUTPUT |
| `sproc_InsertArtist` | @Name nvarchar, @ArtistId int OUT | new Id via OUTPUT |
| `sproc_InsertCustomer` | @FirstName nvarchar, @LastName nvarchar, @Company nvarchar, @Address nvarchar, @City nvarchar, @State nvarchar, @Country nvarchar, @PostalCode nvarchar, @Phone nvarchar, @Fax nvarchar, @Email nvarchar, @SupportRepId int, @CustomerId int OUT | new Id via OUTPUT |
| `sproc_InsertEmployee` | @LastName nvarchar, @FirstName nvarchar, @Title nvarchar, @ReportsTo int, @BirthDate datetime, @HireDate datetime, @Address nvarchar, @City nvarchar, @State nvarchar, @Country nvarchar, @PostalCode nvarchar, @Phone nvarchar, @Fax nvarchar, @Email nvarchar, @EmployeeId int OUT | new Id via OUTPUT |
| `sproc_InsertGenre` | @Name nvarchar, @GenreId int OUT | new Id via OUTPUT |
| `sproc_InsertInvoice` | @CustomerId int, @InvoiceDate datetime, @BillingAddress nvarchar, @BillingCity nvarchar, @BillingState nvarchar, @BillingCountry nvarchar, @BillingPostalCode nvarchar, @Total money, @InvoiceId int OUT | new Id via OUTPUT |
| `sproc_InsertInvoiceLine` | @InvoiceId int, @TrackId int, @UnitPrice money, @Quantity int, @InvoiceLineId int OUT | new Id via OUTPUT |
| `sproc_InsertMediaType` | @Name nvarchar, @MediaTypeId int OUT | new Id via OUTPUT |
| `sproc_InsertPlaylist` | @Name nvarchar, @PlaylistId int OUT | new Id via OUTPUT |
| `sproc_InsertPlaylistTrack` | @PlaylistId int, @TrackId int | — |
| `sproc_InsertTrack` | @Name nvarchar, @AlbumId int, @MediaTypeId int, @GenreId int, @Composer nvarchar, @Milliseconds int, @Bytes int, @UnitPrice money, @TrackId int OUT | new Id via OUTPUT |
| `sproc_UpdateAlbum` | @AlbumId int, @Title nvarchar, @ArtistId int | — |
| `sproc_UpdateArtist` | @ArtistId int, @Name nvarchar | — |
| `sproc_UpdateCustomer` | @CustomerId int, @FirstName nvarchar, @LastName nvarchar, @Company nvarchar, @Address nvarchar, @City nvarchar, @State nvarchar, @Country nvarchar, @PostalCode nvarchar, @Phone nvarchar, @Fax nvarchar, @Email nvarchar, @SupportRepId int | — |
| `sproc_UpdateEmployee` | @EmployeeId int, @LastName nvarchar, @FirstName nvarchar, @Title nvarchar, @ReportsTo int, @BirthDate datetime, @HireDate datetime, @Address nvarchar, @City nvarchar, @State nvarchar, @Country nvarchar, @PostalCode nvarchar, @Phone nvarchar, @Fax nvarchar, @Email nvarchar | — |
| `sproc_UpdateGenre` | @GenreId int, @Name nvarchar | — |
| `sproc_UpdateInvoice` | @InvoiceId int, @CustomerId int, @InvoiceDate datetime, @BillingAddress nvarchar, @BillingCity nvarchar, @BillingState nvarchar, @BillingCountry nvarchar, @BillingPostalCode nvarchar, @Total money | — |
| `sproc_UpdateInvoiceLine` | @InvoiceLineId int, @InvoiceId int, @TrackId int, @UnitPrice money, @Quantity int | — |
| `sproc_UpdateMediaType` | @MediaTypeId int, @Name nvarchar | — |
| `sproc_UpdatePlaylist` | @PlaylistId int, @Name nvarchar | — |
| `sproc_UpdatePlaylistTrack` | @PlaylistId int, @TrackId int | — |
| `sproc_UpdateTrack` | @TrackId int, @Name nvarchar, @AlbumId int, @MediaTypeId int, @GenreId int, @Composer nvarchar, @Milliseconds int, @Bytes int, @UnitPrice money | — |
