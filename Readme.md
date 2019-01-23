# ASP.Net Core RealWorld example API

> ### ASP.NET Core 2 API using a proper SQL database, basic but solid auth, and no stupid patterns.

### [RealWorld](https://github.com/gothinkster/realworld)

This codebase was created because [the other ASP.Net Core example API](https://github.com/gothinkster/aspnetcore-realworld-example-app) uses hard and verbose patterns (looking at you CQRS). And I couldn't get it working on a proper SQL database.

I haven't gone to great lengths to adhere to any kind of guideline other than not having grey text or squiggly lines in Visual Studio.

For more info head over to the [RealWorld](https://github.com/gothinkster/realworld) repo.

# Basic info

This is a basic ASP.Net core Api created by selecting File -> New Project and selecting all the obvious choices.
- For database we use Entity Framework Core with SQL Server.
  - If you want it to work with something else it will probably take some work, but in the "real world" EF and SQL Server go together like PB and Jam.
  - The models used are plain POCO models with code first and migrations. The models are only used for the database (not for API models) in order to avoid serialization gymnastics.
- For Authentication we rolled our own very simple yet probably solid authentication middleware. It doesn't use JWT because it's one more moving part and we're not using [OAuth](https://oauth.net/2/) and [OpenID Conenct](https://openid.net/connect/) anyway.
- The API uses the built in dependency injection instead of using [something from the olden days](https://github.com/AutoMapper/AutoMapper/releases/tag/v1.0) when dinosaurs still roamed the earth.
- For serialization we made some custom converters because the RealWorld API spec wraps everything in an envelope.

# Getting it running

- If you're using Windows, [install Visual Studio](https://visualstudio.microsoft.com/).
- If you're using Linux or Mac, do Linux or Mac things until it works.
- Install the [.NET Core *SDK* version at least 2.2](https://dotnet.microsoft.com/download).
- Load the file into VS and click run. Everything should go smoothly. It uses SQL LocalDb which is installed with VS by default.

## Docker

No docker atm. Log a PR if you're feeling frisky.

# License
[See LICENSE](LICENSE)
