## ⛔Never push sensitive information such as client id's, secrets or keys into repositories including in the README file⛔

# AAN Hub Jobs

<img src="https://avatars.githubusercontent.com/u/9841374?s=200&v=4" align="right" alt="UK Government logo">

[![Build Status](https://sfa-gov-uk.visualstudio.com/Digital%20Apprenticeship%20Service/_apis/build/status%2Fdas-aan-hub-jobs?repoName=SkillsFundingAgency%2Fdas-aan-hub-jobs&branchName=main)](https://sfa-gov-uk.visualstudio.com/Digital%20Apprenticeship%20Service/_build/latest?definitionId=3465&repoName=SkillsFundingAgency%2Fdas-aan-hub-jobs&branchName=main)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=SkillsFundingAgency_das-aan-hub-jobs&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=SkillsFundingAgency_das-aan-hub-jobs)
[![License](https://img.shields.io/badge/license-MIT-lightgrey.svg?longCache=true&style=flat-square)](https://en.wikipedia.org/wiki/MIT_License)

This azure functions solution is part of Apprentice Ambassador Network (AAN) project. Here we have background jobs in form of Azure functions that carry out periodical jobs like sending out notifications or cleaning up data.

## How It Works

The notification job uses NServiceBus protocol to send a message per notification to the notification queue. The functions connects directly with the aan-hub database to get and update data.

## 🚀 Installation

### Pre-Requisites
* A clone of this repository
* Storage emulator like Azurite for local config source
* An Azure Service Bus instance with a Topic called `bundle-1` (optional, only required when working on notification function)

### Config

You can find the latest config file in [das-employer-config repository](https://github.com/SkillsFundingAgency/das-employer-config/blob/master/das-aan-hub-jobs/SFA.DAS.AANHub.Jobs.json). 

In the `SFA.DAS.AAN.Hub.Jobs` project, if not exist already, add local.settings.json file with following content:
```
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true;",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "ConfigurationStorageConnectionString": "UseDevelopmentStorage=true;",
    "ConfigNames": "SFA.DAS.AANHub.Jobs",
    "EnvironmentName": "LOCAL"
  },
  "AzureWebJobs.SendNotificationsFunction.Disabled": "false"
}
```
When actively developing a function, it may be a good idea to disable other functions by adding `"AzureWebJobs.<function-name>.Disabled": "true"` to the local config, example seen above. 

## 🔗 External Dependencies

* The functions uses database defined in [das-aan-hub-api](https://github.com/SkillsFundingAgency/das-ann-hub-api) as primary data source.
* The notification functions depends on [das-notifications](https://github.com/SkillsFundingAgency/das-notifications) Api to listen to the queue and forward the notification requests to Gov Notify to send out emails.

### 📦 Internal Package Depedencies
* SFA.DAS.Notifications.Messages
* SFA.DAS.NServiceBus
* SFA.DAS.Configuration.AzureTableStorage

## Technologies
* .NetCore 6.0
* Azure Functions V4
* Azure Table Storage
* NServiceBus
* NUnit
* Moq
* FluentAssertions
