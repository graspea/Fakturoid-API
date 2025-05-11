# Fakturoid API C#/.NET client
*This is a fork of https://github.com/ridercz/Fakturoid-API*

> Toto je knihovna určená pro další vývojáře. 
> Máte zájem o vývoj na zakázku? Contact: [Michal A. Valášek](http://www.rider.cz) and the [Altairis](http://www.altairis.cz) corporation.
>
> This is library intended for other developers. 
> Are you interested in custom development? Contact: [Michal A. Valášek](http://www.rider.cz) and the [Altairis](http://www.altairis.cz) corporation.

This projects aims to create C#/.NET client for API of Czech online accounting service [Fakturoid](http://www.fakturoid.cz) and implements the API v3.

The library is written in C# and targets .NET Standard 2.0, so it can be used both in current .NET ("Core") and the legacy .NET Framework.

## How to use in your project

Use this as a library in your project.

## What is supported and what is not

The library currently supports the following features of the Fakturoid API:

* Client Credentials Flow
* The following entities:
  * BankAccounts
  * Events
  * Invoices
  * NumberFormats
  * Subjects
  * Todos

Partial support:
* Authorization Code Flow. It supports use of a refresh token (Authorization Code Flow), but you have to provide valid refresh token.

The following features are not supported yet:

* Proper handling of the rate limiting. If you hit a rate limit, the library will throw an exception, but currently does not provide any way get information on how many requests are remaining in current period and when the period will reset.
* Other entities than the mentioned above. There are models prepared for them, but the proxies are not implemented yet.

## Upgrade from version 2.x

If you used version 2.x of this library, you will need to make some changes in your code. Most changes are related to the new API version 3, which has a new logic in many places and completely new authentication system. In addition, there are mainly the following breaking changes:

* The interface is now fully asynchronous, and the synchronous methods are not available anymore.
* The models, originally called `JsonSomething` , are now called just `Something` and were moved to `Altairis.Fakturoid.Client.Models` namespace.
* The model properties are now in `PascalCase` (as is common in C#) instead of `snake_case` (as in the original API).

## Documentation

* Look into the `Altairis.Fakturoid.Client.DemoApp` project for usage.
* All public members have XML documentation that will show up in IntelliSense.
* The [API Reference](API-Reference.md) is available.

## Contributor Code of Conduct

This project adheres to No Code of Conduct. We are all adults. We accept anyone's contributions. Nothing else matters.

For more information please visit the [No Code of Conduct](https://github.com/domgetter/NCoC) homepage.

> This project was originally developed and maintained by [Michal A. Valášek](http://www.rider.cz) and the [Altairis](http://www.altairis.cz) corporation. This project has no official relation to the Fakturoid service or its owner.