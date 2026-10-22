import json
import unittest
from fault_rule import FaultRule
from test_fault_rule import ENDPOINT, QUERY


class CompletionRuleTests(unittest.TestCase):
    def setUp(self):
        self.now = 1000

    def rule(self, status=False, timeout=False):
        operation, root = (('SearchAndBookUpdatePaymentStatus', 'bookUpdatePaymentStatus') if status
                           else ('SearchAndBookCreateOrder', 'bookCreateOrder'))
        return FaultRule({'endpoint': ENDPOINT, 'operationName': operation, 'rootField': root,
                          'mode': 'timeout' if timeout else 'response', 'expiresUnixSeconds': 1100,
                          'response': {'data': {root: None}, 'errors': [{'message': 'Request failed.'}]}},
                         lambda: self.now)

    def payload(self, operation='SearchAndBookCreateOrder', root='bookCreateOrder', query=None):
        return json.dumps({'operationName': operation,
                           'query': query or f'mutation {operation} {{ {root} {{ __typename }} }}'}).encode()

    def test_every_order_attempt_is_intercepted_including_retries(self):
        for timeout in (False, True):
            rule = self.rule(timeout=timeout)
            for _ in range(3):
                self.assertEqual(rule.response, rule.intercept('POST', ENDPOINT, self.payload()))
                self.assertFalse(rule.guard_blocked)

    def test_status_failure_also_blocks_unexpected_order(self):
        rule = self.rule(status=True)
        self.assertEqual(rule.response, rule.intercept('POST', ENDPOINT,
            self.payload('SearchAndBookUpdatePaymentStatus', 'bookUpdatePaymentStatus')))
        self.assertIsNotNone(rule.intercept('POST', ENDPOINT, self.payload()))
        self.assertTrue(rule.guard_blocked)

    def test_normal_setup_and_status_can_pass_for_order_test(self):
        rule = self.rule()
        self.assertIsNone(rule.intercept('POST', ENDPOINT,
            json.dumps({'operationName': 'SearchAndBookOffers', 'query': QUERY}).encode()))
        self.assertIsNone(rule.intercept('POST', ENDPOINT,
            self.payload('SearchAndBookUpdatePaymentStatus', 'bookUpdatePaymentStatus')))
        self.assertFalse(rule.used)

    def test_unsupported_orders_fail_closed_without_counting_injection(self):
        bodies = [b'bad', b'[]', b'x' * 262145,
                  self.payload('Other', 'bookCreateOrder'),
                  self.payload(query='mutation SearchAndBookCreateOrder { alias: bookCreateOrder { id } }'),
                  self.payload(query='mutation SearchAndBookCreateOrder { bookCreateOrder { id } other { id } }'),
                  self.payload(query='mutation SearchAndBookCreateOrder { ...F } fragment F on Mutation { bookCreateOrder { id } }'),
                  self.payload(query='query SearchAndBookCreateOrder { bookCreateOrder { id } }'),
                  self.payload(query='mutation SearchAndBookCreateOrder { bookCreateOrder { id } } mutation Other { bookCreateOrder { id } }')]
        for body in bodies:
            with self.subTest(body_length=len(body)):
                rule = self.rule()
                self.assertIsNotNone(rule.intercept('POST', ENDPOINT, body))
                self.assertTrue(rule.guard_blocked)
                self.assertFalse(rule.used)

    def test_expiry_and_get_never_release_order_upstream(self):
        for method, url in [('GET', ENDPOINT), ('POST', ENDPOINT + '?x=1')]:
            rule = self.rule()
            self.assertIsNotNone(rule.intercept(method, url, self.payload()))
            self.assertTrue(rule.guard_blocked)
        rule = self.rule()
        self.now = 1101
        self.assertIsNotNone(rule.intercept('POST', ENDPOINT, self.payload()))
        self.assertTrue(rule.guard_blocked)

    def test_alternate_paths_cannot_bypass_mutation_guard(self):
        for path in ("/other", "/sit/graphql/mobile/public/v1/", "/sit/graphql/mobile/public/v1?x=1"):
            for body in (self.payload(), self.payload('Other', 'bookCreateOrder'), b'[]', b'bad', b'x' * 262145):
                rule = self.rule()
                self.assertIsNotNone(rule.intercept('POST', 'https://digital-test.virginatlantic.com' + path, body))
                self.assertTrue(rule.guard_blocked)
                self.assertFalse(rule.used)

    def test_alternate_path_setup_query_and_other_hosts_are_not_injected(self):
        rule = self.rule()
        self.assertIsNone(rule.intercept('POST', 'https://digital-test.virginatlantic.com/other',
            json.dumps({'operationName': 'SearchAndBookOffers', 'query': QUERY}).encode()))
        self.assertIsNone(rule.intercept('POST', 'https://example.invalid/other', self.payload()))
        self.assertFalse(rule.used)
