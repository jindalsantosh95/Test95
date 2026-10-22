"""Closed operation allowlist with bounded faults. No network, logs or request storage."""
import json
import time
import re
from urllib.parse import urlsplit
from graphql import parse, OperationType
from graphql.language.ast import OperationDefinitionNode, FieldNode


class FaultRule:
    def __init__(self, rule, clock=time.time):
        self.clock = clock
        self.used = False
        # Deliberately closed POC allowlist, independent of caller-supplied operation names.
        allowed = {"SearchAndBookOffers": "searchOffers", "SearchAndBookRetrieveBasket": "retrieveBasket",
                   "SearchAndBookUpdatePaymentStatus": "bookUpdatePaymentStatus",
                   "SearchAndBookCreateOrder": "bookCreateOrder",
                   "SearchAndBookRetrieveSeatMap": "retrieveSeatMap",
                   "SearchAndBookSelectSeat": "selectSeat"}
        self.operation = rule.get("operationName")
        self.root = rule.get("rootField")
        if (rule.get("endpoint") != "https://digital-test.virginatlantic.com/sit/graphql/mobile/public/v1"
                or self.operation not in allowed or allowed[self.operation] != self.root):
            raise ValueError("Only reviewed SIT operations are supported")
        self.completion = self.operation in ("SearchAndBookUpdatePaymentStatus", "SearchAndBookCreateOrder")
        self.mutation = self.completion or self.operation == "SearchAndBookSelectSeat"
        self.guard_blocked = False
        self.expires = rule["expiresUnixSeconds"]
        if not 0 < self.expires - clock() <= 600:
            raise ValueError("Fault expiry must be within ten minutes")
        self.response = rule["response"]
        self.mode = rule.get("mode", "response")
        if self.mode not in ("response", "timeout") or (self.mode == "timeout" and self.operation not in ("SearchAndBookOffers", "SearchAndBookCreateOrder")):
            raise ValueError("Only bounded search/order timeouts are supported")
        if (not isinstance(self.response, dict) or not self.response.get("errors")
                or self.response.get("data") not in (None, {self.root: None})):
            raise ValueError("A GraphQL error fixture is required")
        if len(json.dumps(self.response)) > 16384:
            raise ValueError("Fixture exceeds the bounded response size")
        self.endpoint = rule["endpoint"]

    def intercept(self, method, url, body):
        self.guard_blocked = False
        # Mutation faults remain intercepted on retries; unsupported protected calls fail closed.
        # Query-mode POC behaviour is deliberately unchanged.
        guarded = self.mutation and urlsplit(url).hostname == urlsplit(self.endpoint).hostname
        if guarded and (method != "POST" or self.clock() >= self.expires):
            return self.block_completion()
        if not guarded and ((self.used and self.mode != "timeout") or self.clock() >= self.expires or method != "POST" or url != self.endpoint):
            return None
        if len(body) > 262144:
            return self.block_completion() if guarded else None
        try:
            payload = json.loads(body)
            if guarded:
                if not isinstance(payload, dict) or not isinstance(payload.get("query"), str):
                    return self.block_completion()
                protected = ["bookCreateOrder", "SearchAndBookCreateOrder"]
                if self.operation == "SearchAndBookUpdatePaymentStatus":
                    protected += ["bookUpdatePaymentStatus", "SearchAndBookUpdatePaymentStatus"]
                if self.operation == "SearchAndBookSelectSeat":
                    protected += ["selectSeat", "SearchAndBookSelectSeat", "bookUpdatePaymentStatus", "SearchAndBookUpdatePaymentStatus"]
                sensitive = any(re.search(r"\b" + name + r"\b", payload["query"]) for name in protected)
                # Parse every guarded request before allowing it through. This also rejects
                # unsupported persisted documents rather than accidentally submitting orders.
                parse(payload["query"], max_tokens=20000)
                if sensitive and (url != self.endpoint or payload.get("operationName") != self.operation):
                    return self.block_completion()
                if url != self.endpoint:
                    # Never inject a fixture at an unreviewed path. Only parsed, unprotected
                    # setup operations can pass; malformed/batched documents fail closed.
                    return self.block_completion() if payload.get("operationName") == self.operation else None
            if not isinstance(payload, dict) or payload.get("operationName") != self.operation:
                return None
            document = parse(payload["query"], max_tokens=20000)
            operations = [d for d in document.definitions if isinstance(d, OperationDefinitionNode)]
            if len(operations) != 1:
                return self.block_completion() if guarded else None
            operation = operations[0]
            expected_type = OperationType.MUTATION if self.mutation else OperationType.QUERY
            if (operation.operation != expected_type or operation.name is None
                    or operation.name.value != self.operation):
                return self.block_completion() if guarded else None
            fields = operation.selection_set.selections
            if (len(fields) != 1 or not isinstance(fields[0], FieldNode)
                    or fields[0].name.value != self.root or fields[0].alias is not None):
                return self.block_completion() if guarded else None
        except Exception:
            # Completion guards reject malformed requests; query-only faults pass them through.
            return self.block_completion() if guarded else None
        self.used = True
        return self.response

    def block_completion(self):
        self.guard_blocked = True
        return {"errors": [{"message": "Automation completion guard blocked an unsupported request.",
                            "extensions": {"code": "AUTOMATION_GUARD"}}]}
