# Changelog for Relativity-Smoke-Test-App

- This file is used to list changes made in the Relativity-Smoke-Test-App.

-------------------------

## [Unreleased]

### Added

- Added NuGet packages for running tests via Visual Studio 2022/dotnet (#16)
- Created separate `SqlInstance` test constant to account for differences in server instance name across DevVM versions (#16)

### Changed

- Expanded README &amp; updated formatting of CHANGELOG to follow [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) (#16)

### Security

- Newtonsoft.Json version bumped to 13.0.3 (#16)

## [2.2.2.2] - 2021-01-22

### Removed

- Removed check for Admin Audits for the Data Grid Test (#9)