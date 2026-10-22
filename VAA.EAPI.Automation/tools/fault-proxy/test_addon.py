import json
import asyncio
import os
import tempfile
import time
import unittest
from pathlib import Path
from unittest.mock import patch, AsyncMock
from mitmproxy import http
from mitmproxy.test import tflow
from addon import SearchFaultAddon
from test_fault_rule import ENDPOINT, QUERY


class AddonTests(unittest.TestCase):
    def test_timeout_blocks_forwarding_and_records_withheld_not_delivered(self):
        with tempfile.TemporaryDirectory() as directory:
            rule = {"endpoint": ENDPOINT, "operationName": "SearchAndBookOffers", "rootField": "searchOffers",
                    "mode": "timeout", "expiresUnixSeconds": time.time() + 300,
                    "response": {"errors": [{"message": "unused"}]}}
            Path(directory, "rule.json").write_text(json.dumps(rule))
            with patch.dict(os.environ, {"EAPI_FAULT_DIRECTORY": directory}), patch('addon.asyncio.sleep', new_callable=AsyncMock) as delay:
                addon = SearchFaultAddon()
                addon.load(None)
                first_event = None
                for _ in range(2):
                    flow = tflow.tflow()
                    flow.request = http.Request.make("POST", ENDPOINT,
                        json.dumps({"operationName": "SearchAndBookOffers", "query": QUERY}).encode())
                    asyncio.run(addon.request(flow))
                    self.assertEqual(504, flow.response.status_code)
                    current_event = Path(directory, "withheld.json").read_text()
                    if first_event is not None:
                        self.assertEqual(first_event, current_event)
                    first_event = current_event
                self.assertEqual(2, delay.await_count)
                delay.assert_awaited_with(150)
                self.assertTrue(Path(directory, "withheld.json").exists())
                self.assertTrue(Path(directory, "hold-completed.json").exists())
                self.assertFalse(Path(directory, "injected.json").exists())

    def test_returns_http_200_graphql_error_and_only_safe_evidence(self):
        with tempfile.TemporaryDirectory() as directory:
            rule = {"endpoint": ENDPOINT, "operationName": "SearchAndBookOffers", "rootField": "searchOffers",
                    "expiresUnixSeconds": time.time() + 60,
                    "response": {"data": {"searchOffers": None}, "errors": [{"message": "Search Error"}]}}
            Path(directory, "rule.json").write_text(json.dumps(rule))
            with patch.dict(os.environ, {"EAPI_FAULT_DIRECTORY": directory}):
                addon = SearchFaultAddon()
                addon.load(None)
                addon.running()
                flow = tflow.tflow()
                flow.request = http.Request.make("POST", ENDPOINT,
                    json.dumps({"operationName": "SearchAndBookOffers", "query": QUERY}).encode(),
                    {"Authorization": "synthetic-secret-not-for-evidence", "Content-Type": "application/json"})
                asyncio.run(addon.request(flow))
                self.assertEqual(200, flow.response.status_code)
                self.assertEqual(rule["response"], json.loads(flow.response.content))
                evidence = json.loads(Path(directory, "injected.json").read_text())
                self.assertEqual({"state", "operationName", "utc"}, set(evidence))
                self.assertEqual("injected", evidence["state"])
                self.assertEqual("SearchAndBookOffers", evidence["operationName"])
                self.assertNotIn("synthetic-secret", Path(directory, "injected.json").read_text())
                second = tflow.tflow()
                second.request = flow.request.copy()
                asyncio.run(addon.request(second))
                self.assertIsNone(second.response)


if __name__ == "__main__":
    unittest.main()
