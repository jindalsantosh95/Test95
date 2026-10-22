"""Real local TLS proxy qualification. Upstream is a local sentinel, never EAPI."""
import http.client
import json
import os
from pathlib import Path
import socket
import ssl
import subprocess
import sys
import tempfile
import time
import unittest
from test_fault_rule import ENDPOINT, QUERY


class ProxyProcessTests(unittest.TestCase):
    def test_tls_injection_without_contacting_upstream(self):
        self.exercise_proxy("response")

    def test_tls_timeout_withholds_response_without_contacting_upstream(self):
        self.exercise_proxy("timeout")

    def test_tls_order_error_never_contacts_upstream(self):
        self.exercise_proxy("response", completion=True)

    def test_tls_order_timeout_never_contacts_upstream(self):
        self.exercise_proxy("timeout", completion=True)

    def test_tls_seat_map_error_never_contacts_upstream(self):
        self.exercise_proxy("response", seat="map")

    def test_tls_seat_save_error_never_contacts_upstream(self):
        self.exercise_proxy("response", seat="save")

    def test_tls_alternate_order_path_never_contacts_upstream(self):
        self.exercise_proxy("response", completion=True, alternate_path=True)

    def test_tls_alternate_seat_path_never_contacts_upstream(self):
        self.exercise_proxy("response", seat="save", alternate_path=True)

    def exercise_proxy(self, mode, completion=False, seat=None, alternate_path=False):
        with tempfile.TemporaryDirectory() as directory, socket.socket() as sentinel:
            sentinel.bind(("127.0.0.1", 0))
            sentinel.listen()
            sentinel.settimeout(0.2)
            upstream_port = sentinel.getsockname()[1]
            with socket.socket() as reservation:
                reservation.bind(("127.0.0.1", 0))
                port = reservation.getsockname()[1]
            operation = "SearchAndBookCreateOrder" if completion else "SearchAndBookOffers"
            root = "bookCreateOrder" if completion else "searchOffers"
            query = "mutation SearchAndBookCreateOrder { bookCreateOrder { __typename } }" if completion else QUERY
            if seat is not None:
                operation = "SearchAndBookSelectSeat" if seat == "save" else "SearchAndBookRetrieveSeatMap"
                root = "selectSeat" if seat == "save" else "retrieveSeatMap"
                kind = "mutation" if seat == "save" else "query"
                query = f"{kind} {operation} {{ {root} {{ __typename }} }}"
            rule = {"endpoint": ENDPOINT, "operationName": operation, "rootField": root,
                    "mode": mode, "expiresUnixSeconds": time.time() + 300,
                    "response": {"data": {root: None}, "errors": [{"message": "Controlled Error"}]}}
            Path(directory, "rule.json").write_text(json.dumps(rule))
            addon = Path(__file__).resolve().with_name("addon.py")
            executable = Path(sys.executable).with_name("mitmdump.exe" if os.name == "nt" else "mitmdump")
            environment = {**os.environ, "EAPI_FAULT_DIRECTORY": directory, "PYTHONPATH": str(addon.parent)}
            process = subprocess.Popen([str(executable), "--quiet", "--listen-host", "127.0.0.1",
                "--listen-port", str(port), "--mode", f"upstream:http://127.0.0.1:{upstream_port}",
                "--set", "confdir=" + directory, "--set", "connection_strategy=lazy",
                "--set", "upstream_cert=false", "--set", "flow_detail=0", "-s", str(addon)],
                env=environment, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
            try:
                deadline = time.monotonic() + 20
                while not Path(directory, "ready.json").exists():
                    self.assertIsNone(process.poll(), "Proxy exited before readiness")
                    self.assertLess(time.monotonic(), deadline, "Proxy readiness timeout")
                    time.sleep(0.05)
                context = ssl.create_default_context(cafile=str(Path(directory, "mitmproxy-ca-cert.pem")))
                connection = http.client.HTTPSConnection("127.0.0.1", port, context=context, timeout=5)
                connection.set_tunnel("digital-test.virginatlantic.com", 443)
                try:
                    connection.request("POST", "/alternate" if alternate_path else "/sit/graphql/mobile/public/v1",
                        json.dumps({"operationName": operation, "query": query}),
                        {"Content-Type": "application/json"})
                    if mode == "timeout":
                        with self.assertRaises(TimeoutError):
                            connection.getresponse()
                        self.assertTrue(Path(directory, "withheld.json").exists())
                        self.assertFalse(Path(directory, "injected.json").exists())
                    else:
                        response = connection.getresponse()
                        self.assertEqual(200, response.status)
                        actual = json.loads(response.read())
                        if alternate_path:
                            self.assertEqual("AUTOMATION_GUARD", actual["errors"][0]["extensions"]["code"])
                            self.assertTrue(Path(directory, "guard-blocked.json").exists())
                            self.assertFalse(Path(directory, "injected.json").exists())
                        else:
                            self.assertEqual(rule["response"], actual)
                            self.assertTrue(Path(directory, "injected.json").exists())
                    with self.assertRaises(TimeoutError):
                        sentinel.accept()
                finally:
                    connection.close()
            finally:
                process.terminate()
                try:
                    process.wait(timeout=5)
                except subprocess.TimeoutExpired:
                    process.kill()
                    process.wait(timeout=5)


if __name__ == "__main__":
    unittest.main()
