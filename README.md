# file-service-sample

## User story

(revise as real user story?)
As a service providers, you require or use files uploaded by your customers. You are aiming for a secured, easy to maintain micro-service, that would be used either as an API or as a UI service.

## Main Desgin Considerations

- Secured, the code must hosted in vnet enabled compute.
- Youd storage must not be publicly exposed.
- All uploads are considered unsafe unless verified.
- Customer uploads must land on DMZ storage, with minimal, automatic clean up.

