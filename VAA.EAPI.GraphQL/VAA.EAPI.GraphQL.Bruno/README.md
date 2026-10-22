# VAA EAPI Full Public GraphQL Bruno collection

This collection is generated from the live Dev schema at `https://testresources.virginatlantic.com/dev/graphql/full/public`.

Requests are grouped into `Queries` and `Mutations`. Generated variables contain safe structural placeholders; replace them with meaningful scenario data before execution. Object, interface, and union responses use `__typename` as a valid minimal selection that can be expanded as needed.

Select Dev, SIT, UAT, or Production and populate the secret `ocp_apim_subscription_key` variable where the environment requires it.

Run `generate.ps1` to refresh the collection from the live Dev schema. `manifest.json` records the exact operation inventory and signatures used for generation.
