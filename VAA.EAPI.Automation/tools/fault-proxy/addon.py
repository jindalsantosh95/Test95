"""mitmdump adapter: no flow files, credentials or request/response output."""
import json
import asyncio
import os
from datetime import datetime, timezone
from pathlib import Path
from mitmproxy import http
from fault_rule import FaultRule


class SearchFaultAddon:
    def load(self, loader):
        self.directory = Path(os.environ["EAPI_FAULT_DIRECTORY"])
        self.rule = FaultRule(json.loads((self.directory / "rule.json").read_text()))

    def running(self):
        self.write_event("ready")

    def write_event(self, state):
        destination = self.directory / (state + ".json")
        if destination.exists():
            return  # Retain the first occurrence; a retry must not reset the timeout timeline.
        temporary = destination.with_suffix(".tmp")
        temporary.write_text(json.dumps({"state": state, "operationName": self.rule.operation,
                                         "utc": datetime.now(timezone.utc).isoformat()}))
        temporary.replace(destination)

    async def request(self, flow):
        response = self.rule.intercept(flow.request.method, flow.request.pretty_url,
                                       flow.request.raw_content or b"")
        if response is not None:
            if self.rule.guard_blocked:
                flow.response = http.Response.make(200, json.dumps(response).encode(),
                    {"Content-Type": "application/json", "Cache-Control": "no-store"})
                self.write_event("guard-blocked")
                return
            if self.rule.mode == "timeout":
                # Suppress forwarding immediately, then withhold the synthetic response beyond
                # iOS Apollo's 120-second request/resource timeout. No upstream call is made.
                flow.response = http.Response.make(504, b"", {"Cache-Control": "no-store"})
                self.write_event("withheld")
                await asyncio.sleep(150)
                self.write_event("hold-completed")
                return
            # Set in request hook, before the real query reaches EAPI.
            flow.response = http.Response.make(200, json.dumps(response).encode(),
                                               {"Content-Type": "application/json",
                                                "Cache-Control": "no-store"})
            self.write_event("injected")


addons = [SearchFaultAddon()]
