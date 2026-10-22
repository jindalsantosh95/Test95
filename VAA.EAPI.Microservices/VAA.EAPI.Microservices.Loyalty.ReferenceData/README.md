# Loyalty Reference Data

Azure Functions isolated-worker microservice for Loyalty reference data.

## Title endpoint

`GET /api/Titles` reads the SQL `TitleMaster` table and returns all titles, ordered by database `Priority` and then `Title`.

Each response item contains only `title`.

The endpoint uses the existing `SCV:CommonRefDbConnString` SQL configuration and managed-identity database token configuration (`DatabaseID`). Title data follows the existing cache-aside behavior and cache key used by Loyalty enrolment's `GetAllTitlesAsync()` implementation. Authentication is enforced by the established EAPI middleware and OAuth2 API contract.
