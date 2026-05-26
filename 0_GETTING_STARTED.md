
```markdown
# getting_started_questions

**Date:** YYYY-MM-DD  
**Tool:** Cursor.  
**Goal:** Detect issues in the assignment spec and clarify them with the issuer.

## Prompt

1) I got this spec, do not implement it, give me a list of questions that are unclear in this spec -


2) turn this into a single email-style message to the assignment issuer, and prioritize the highest-risk ambiguities (Redis “originate,” backend-only channels, and what counts as forbidden communication).

3) help me understand the assignment and the questions about it

4) i got the following answer: “IoT writes X to Redis, then it go to telemetry service ...."
Source-of-truth interpretation: Redis holds the latest value and consumers should treat Redis as authoritative for telemetry.

SQL Data service & IoT Telemetry service should communicate via RabbitMQ or gRPC to REST API service .
```
