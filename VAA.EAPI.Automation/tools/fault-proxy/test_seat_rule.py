import json
import unittest
from fault_rule import FaultRule
from test_fault_rule import ENDPOINT, QUERY


class SeatRuleTests(unittest.TestCase):
    def rule(self, save=False, **overrides):
        self.now = 1000
        operation = "SearchAndBookSelectSeat" if save else "SearchAndBookRetrieveSeatMap"
        root = "selectSeat" if save else "retrieveSeatMap"
        return FaultRule({"endpoint": ENDPOINT, "operationName": operation, "rootField": root,
                          "expiresUnixSeconds": 1500,
                          "response": {"data": {root: None}, "errors": [{"message": "Controlled seat failure"}]},
                          **overrides}, lambda: self.now)

    def body(self, rule, query=None):
        kind = "mutation" if rule.mutation else "query"
        return json.dumps({"operationName": rule.operation,
                           "query": query or f"{kind} {rule.operation} {{ {rule.root} {{ __typename }} }}"}).encode()

    def test_map_fault_is_exact_one_shot_and_ignores_search(self):
        rule = self.rule()
        search = json.dumps({"operationName": "SearchAndBookOffers", "query": QUERY}).encode()
        self.assertIsNone(rule.intercept("POST", ENDPOINT, search))
        self.assertEqual(rule.response, rule.intercept("POST", ENDPOINT, self.body(rule)))
        self.assertIsNone(rule.intercept("POST", ENDPOINT, self.body(rule)))

    def test_save_retries_are_intercepted_and_expiry_blocks(self):
        rule = self.rule(save=True)
        for _ in range(3):
            self.assertEqual(rule.response, rule.intercept("POST", ENDPOINT, self.body(rule)))
            self.assertFalse(rule.guard_blocked)
        self.now = 1500
        self.assertIsNotNone(rule.intercept("POST", ENDPOINT, self.body(rule)))
        self.assertTrue(rule.guard_blocked)

    def test_save_guard_rejects_wrong_type_alias_batch_and_downstream_payment(self):
        rule = self.rule(save=True)
        bodies = [b"bad", b"[]", b"x" * 262145,
                  self.body(rule, "query SearchAndBookSelectSeat { selectSeat { id } }"),
                  self.body(rule, "mutation SearchAndBookSelectSeat { alias: selectSeat { id } }"),
                  self.body(rule, "mutation SearchAndBookSelectSeat { selectSeat { id } bookCreateOrder { id } }")]
        for operation, root in [("SearchAndBookCreateOrder", "bookCreateOrder"),
                                ("SearchAndBookUpdatePaymentStatus", "bookUpdatePaymentStatus")]:
            bodies.append(json.dumps({"operationName": operation,
                                      "query": f"mutation {operation} {{ {root} {{ id }} }}"}).encode())
        for body in bodies:
            with self.subTest(body=body[:80]):
                self.assertIsNotNone(rule.intercept("POST", ENDPOINT, body))
                self.assertTrue(rule.guard_blocked)

    def test_save_guard_allows_seat_map_setup_query(self):
        rule = self.rule(save=True)
        map_rule = self.rule()
        self.assertIsNone(rule.intercept("POST", ENDPOINT, self.body(map_rule)))
        self.assertFalse(rule.guard_blocked)

    def test_seat_timeouts_and_mismatched_roots_are_not_enabled(self):
        for save in (False, True):
            with self.assertRaises(ValueError):
                self.rule(save, mode="timeout")
            with self.assertRaises(ValueError):
                self.rule(save, rootField="bookCreateOrder")
