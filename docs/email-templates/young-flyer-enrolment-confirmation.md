# Young Flyer enrolment confirmation

## Ownership and routing

| Field | Value |
| --- | --- |
| Logical template ID | `FCGOL-YF-01` |
| Rendering owner | CRM email platform, reached through the existing CCP/Adobe integration |
| EAPI producer | Loyalty Member Management |
| Trigger | Successful child enrolment followed by managed-child household change events accepted for publication with `HoHManaged=true` |
| Recipient | The authenticated parent or guardian's preferred email address |
| Child recipient | None; the generated `@U18.virgin.com` address is never used for communication |
| Channel | Email |

The identifier is the repository contract requested from CRM. Before this change is deployed, CRM must provision that exact ID and merge-field contract in DEV, SIT, UAT, and PROD.

## Proposed customer copy

**Subject:** Your child's Flying Club account is ready

**Preheader:** The account has been created and linked to your household.

Hi `{{Customer_FirstName}}`,

`{{Child_First_Name}}`'s Flying Club account has been created and linked to your household.

Their Flying Club membership number is **`{{Child_Membership_Number}}`**.

As the parent or guardian, you can manage their account through your household. The child has not been given a separate email sign-in as part of this enrolment.

If you did not request this account, please contact the Flying Club team using the contact details on virginatlantic.com.

Thanks,

The Virgin Atlantic Flying Club team

## Merge-field contract

| Field | Required | Source | Notes |
| --- | --- | --- | --- |
| `Customer_FirstName` | Yes | Trusted head-account record | Parent or guardian salutation only. |
| `Child_First_Name` | Yes | Enrolled child link request | Do not include the child's last name. |
| `Child_Membership_Number` | Yes | Successful enrolment result | Include only in the email sent to the trusted parent address. |

Existing household fields may remain present for compatibility, but the template must not render the dummy child email, customer IDs, tokens, date of birth, address, or raw downstream data.

## Acceptance checks

- The event is emitted only after the managed-child household change events have been accepted for publication.
- The recipient is the parent's trusted email, never the child's generated dummy address.
- The Young Flyer template is selected only when the child email matches the configured dummy domain and `HoHManaged=true` is derived.
- The email identifies the child using only first name and membership number.
- Standard adult household-link templates and child notifications remain unchanged.
- CRM confirms the template ID and merge-field names in each environment before release.
