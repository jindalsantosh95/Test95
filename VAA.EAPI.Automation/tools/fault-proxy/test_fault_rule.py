import json
import unittest
from fault_rule import FaultRule

ENDPOINT = "https://digital-test.virginatlantic.com/sit/graphql/mobile/public/v1"
QUERY = "query SearchAndBookOffers { searchOffers { result { __typename } } }"


class FaultRuleTests(unittest.TestCase):
    def setUp(self):
        self.now = 1000
        self.fixture = {"data": {"searchOffers": None}, "errors": [{"message": "Search Error"}]}
        self.config = {"endpoint": ENDPOINT, "operationName": "SearchAndBookOffers",
                       "rootField": "searchOffers", "expiresUnixSeconds": 1100, "response": self.fixture}
        self.rule = FaultRule(self.config, lambda: self.now)

    def request(self, query=QUERY, **overrides):
        payload = {"operationName": "SearchAndBookOffers", "query": query}
        payload.update(overrides)
        return json.dumps(payload).encode()

    def test_injects_once_only(self):
        self.assertEqual(self.fixture, self.rule.intercept("POST", ENDPOINT, self.request()))
        self.assertIsNone(self.rule.intercept("POST", ENDPOINT, self.request()))

    def test_expiry(self):
        self.now = 1100
        self.assertIsNone(self.rule.intercept("POST", ENDPOINT, self.request()))

    def test_wrong_endpoint_or_method_does_not_consume(self):
        for method, endpoint in [("GET", ENDPOINT), ("POST", ENDPOINT + "?x=1"),
                                 ("POST", ENDPOINT.replace("sit", "prod"))]:
            self.assertIsNone(self.rule.intercept(method, endpoint, self.request()))
        self.assertFalse(self.rule.used)

    def test_unsafe_queries_do_not_consume(self):
        for query in ["mutation SearchAndBookOffers { searchOffers { id } }",
                      "query SearchAndBookOffers { searchOffers { id } createOrder { id } }",
                      "query SearchAndBookOffers { alias: searchOffers { id } }",
                      "query SearchAndBookOffers { ...Root } fragment Root on Query { searchOffers { id } }",
                      "query Other { searchOffers { id } }", "not graphql"]:
            with self.subTest(query=query):
                self.assertIsNone(self.rule.intercept("POST", ENDPOINT, self.request(query)))
                self.assertFalse(self.rule.used)

    def test_wrong_operation_and_bad_payloads(self):
        for body in [b"{}", b"bad", b"[]", b"x" * 262145, self.request(operationName="CreateOrder")]:
            self.assertIsNone(self.rule.intercept("POST", ENDPOINT, body))
        self.assertFalse(self.rule.used)

    def test_configuration_rejects_scope_expansion(self):
        for key, value in [("endpoint", ENDPOINT.replace("sit", "prod")), ("operationName", "CreateOrder"),
                           ("rootField", "createOrder"), ("expiresUnixSeconds", 1000),
                           ("expiresUnixSeconds", 1601), ("response", {"data": {"searchOffers": {}}})]:
            with self.subTest(key=key):
                with self.assertRaises(ValueError):
                    FaultRule({**self.config, key: value}, lambda: self.now)

    def test_basket_query_is_exact_one_shot_and_does_not_consume_search(self):
        response = {"data": {"retrieveBasket": None}, "errors": [{"message": "Request failed."}]}
        rule = FaultRule({**self.config, "operationName": "SearchAndBookRetrieveBasket",
                          "rootField": "retrieveBasket", "response": response}, lambda: self.now)
        self.assertIsNone(rule.intercept("POST", ENDPOINT, self.request()))
        for query in ["mutation SearchAndBookRetrieveBasket { retrieveBasket { id } }",
                      "query SearchAndBookRetrieveBasket { retrieveBasket { id } createOrder { id } }",
                      "query SearchAndBookRetrieveBasket { alias: retrieveBasket { id } }"]:
            self.assertIsNone(rule.intercept("POST", ENDPOINT, self.request(query, operationName="SearchAndBookRetrieveBasket")))
        payload = self.request("query SearchAndBookRetrieveBasket { retrieveBasket { id } }", operationName="SearchAndBookRetrieveBasket")
        self.assertEqual(response, rule.intercept("POST", ENDPOINT, payload))
        self.assertIsNone(rule.intercept("POST", ENDPOINT, payload))

    def test_basket_rejects_mismatched_root_and_fixture(self):
        for override in [{"operationName": "SearchAndBookRetrieveBasket"},
                         {"operationName": "SearchAndBookRetrieveBasket", "rootField": "retrieveBasket"}]:
            with self.assertRaises(ValueError):
                FaultRule({**self.config, **override}, lambda: self.now)

    def test_timeout_mode_cannot_target_basket_or_unknown_mode(self):
        with self.assertRaises(ValueError):
            FaultRule({**self.config, "mode": "arbitrary"}, lambda: self.now)
        with self.assertRaises(ValueError):
            FaultRule({**self.config, "operationName": "SearchAndBookRetrieveBasket", "rootField": "retrieveBasket",
                       "mode": "timeout", "response": {"errors": [{"message": "unused"}]}}, lambda: self.now)


if __name__ == "__main__":
    unittest.main()
