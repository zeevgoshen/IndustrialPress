



```markdown
# getting_started_questions

**Date:** YYYY-MM-DD  
**Tool:** Cursor.  
**Goal:** Detect issues in the assignment spec and clarify them with the issuer.

## Prompt


what is the best architecture you recommend ? would the basic folder structure change if architecture changes ?

lets do phase 0 diagram with explanation of what each component does

what happens in cases of failures ? are there re-tries in this architecture ? 

add explicit retry rules to the Phase 0 architecture doc before implementation

how well will this architecure handle more sensors ? database bottlenecks ? scaling ?

is this current architecture uses CQRS ? if not, should it ? is it async all the way ?

yes, add to md file

did you add re-tries and failures to the md file ?

scaling too ?

is there sql data that should be in nosql ?

does concurrent 20 sensors require load balancing or any multi-threading support which is not currently in the plan ?

```
