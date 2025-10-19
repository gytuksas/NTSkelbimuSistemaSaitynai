#!/usr/bin/env python3
import os
import sys
import time
import uuid
from dataclasses import dataclass
from typing import Any, Dict, Optional, Sequence

import requests

BASE_URL = os.environ.get("BASE_URL", "http://localhost:8080/api").rstrip("/")
TIMEOUT = float(os.environ.get("HTTP_TIMEOUT", "10"))

@dataclass
class TestResult:
    name: str
    ok: bool
    status: int
    details: str = ""

class ApiError(Exception):
    pass

class Client:
    def __init__(self, base_url: str):
        self.base_url = base_url.rstrip("/")
        self.sess = requests.Session()
        self.sess.headers.update({"Content-Type": "application/json"})

    def _url(self, path: str) -> str:
        path = path.lstrip("/")
        return f"{self.base_url}/{path}"

    def get(self, path: str, **kw):
        return self.sess.get(self._url(path), timeout=TIMEOUT, **kw)

    def post(self, path: str, json: Optional[dict] = None, **kw):
        return self.sess.post(self._url(path), json=json, timeout=TIMEOUT, **kw)

    def put(self, path: str, json: Optional[dict] = None, **kw):
        return self.sess.put(self._url(path), json=json, timeout=TIMEOUT, **kw)

    def delete(self, path: str, **kw):
        return self.sess.delete(self._url(path), timeout=TIMEOUT, **kw)

    def patch(self, path: str, json: Optional[dict] = None, **kw):
        return self.sess.patch(self._url(path), json=json, timeout=TIMEOUT, **kw)


def expect_status(name: str, resp: requests.Response, expected: Sequence[int]) -> TestResult:
    ok = resp.status_code in expected
    try:
        body = resp.json()
    except Exception:
        body = resp.text
    return TestResult(
        name=name,
        ok=ok,
        status=resp.status_code,
        details=f"expected {list(expected)}, got {resp.status_code}, body={body!r}" if not ok else "",
    )


def main() -> int:
    client = Client(BASE_URL)
    results: list[TestResult] = []

    # 1) Simple NOT FOUND checks (404)
    results.append(expect_status("GET /Users/999999999 (404)", client.get("Users/999999999"), [404]))
    results.append(expect_status("GET /Buildings/999999999 (404)", client.get("Buildings/999999999"), [404]))
    results.append(expect_status("GET /Listings/999999999 (404)", client.get("Listings/999999999"), [404]))

    # 2) Bad payload checks (400/422)
    # Users: invalid date format (no time) -> 422 expected by controller
    bad_user = {
        "name": "Bad",
        "surname": "Payload",
        "email": "bad@example.com",
        "phone": "000000",
        "password": "x",
        "registrationtime": "2025-01-01",  # missing time -> UnprocessableEntity
        "profilepicture": None,
    }
    results.append(expect_status("POST /Users bad date (422)", client.post("Users", json=bad_user), [422]))

    # Availabilities: invalid date parse -> 400
    bad_av = {
        "from": "not-a-date",
        "to": "also-bad",
        "fkBrokeridUser": 1234567890,
    }
    results.append(expect_status("POST /Availabilities bad date (400)", client.post("Availabilities", json=bad_av), [400]))

    # 3) Create dependency chain and exercise CRUD
    created: Dict[str, Any] = {}

    # Create 3 users (for broker, buyer, admin)
    def make_user_payload(name: str, email: str) -> Dict[str, Any]:
        return {
            "name": name,
            "surname": "Tester",
            "email": email,
            "phone": "+37060000000",
            "password": "password123",
            "registrationtime": "2025-01-01 12:00",
            "profilepicture": None,
        }

    for key, name in [("user_broker", "BrokerUser"), ("user_buyer", "BuyerUser"), ("user_admin", "AdminUser")]:
        resp = client.post("Users", json=make_user_payload(name, f"{name.lower()}-{uuid.uuid4().hex[:8]}@example.com"))
        results.append(expect_status(f"POST /Users ({name}) -> 201", resp, [201]))
        if resp.status_code == 201:
            created[key] = resp.json()

    # Broker for user_broker
    broker_payload = {"confirmed": False, "blocked": False, "idUser": created["user_broker"]["idUser"]}
    resp = client.post("Brokers", json=broker_payload)
    results.append(expect_status("POST /Brokers -> 201", resp, [201]))
    if resp.status_code == 201:
        created["broker"] = resp.json()

    # Building for broker
    building_payload = {
        "city": "Vilnius",
        "address": "Gedimino pr. 1",
        "area": 123.45,
        "year": 2020,
        "lastrenovationyear": None,
        "floors": 5,
        "energy": 1,  # assume seeded
        "fkBrokeridUser": created["broker"]["idUser"],
    }
    resp = client.post("Buildings", json=building_payload)
    results.append(expect_status("POST /Buildings -> 201", resp, [201]))
    if resp.status_code == 201:
        created["building"] = resp.json()

    # Apartment for building
    apt_payload = {
        "apartmentnumber": 1,
        "area": 52.3,
        "floor": 2,
        "rooms": 2,
        "notes": "Test apartment",
        "heating": None,
        "finish": 1,  # assume seeded
        "fkBuildingidBuilding": created["building"]["idBuilding"],
        "iswholebuilding": False,
    }
    resp = client.post("Apartments", json=apt_payload)
    results.append(expect_status("POST /Apartments -> 201", resp, [201]))
    if resp.status_code == 201:
        created["apartment"] = resp.json()

    # Picture for apartment
    pic_id = f"pic-{uuid.uuid4().hex}"
    pic_payload = {"id": pic_id, "public": False, "fkApartmentidApartment": created["apartment"]["idApartment"]}
    resp = client.post("Pictures", json=pic_payload)
    results.append(expect_status("POST /Pictures -> 201", resp, [201]))
    if resp.status_code == 201:
        created["picture"] = resp.json()

    # Listing for picture
    list_payload = {
        "description": "Cozy place",
        "askingprice": 99999.99,
        "rent": False,
        "fkPictureid": pic_id,
    }
    resp = client.post("Listings", json=list_payload)
    results.append(expect_status("POST /Listings -> 201", resp, [201]))
    if resp.status_code == 201:
        created["listing"] = resp.json()

    # Availability for broker
    av_payload = {
        "from": "2025-02-01 10:00",
        "to": "2025-02-01 11:00",
        "fkBrokeridUser": created["broker"]["idUser"],
    }
    resp = client.post("Availabilities", json=av_payload)
    results.append(expect_status("POST /Availabilities -> 201", resp, [201]))
    if resp.status_code == 201:
        created["availability"] = resp.json()

    # Viewing for listing + availability (Status 1 assumed from seed)
    view_payload = {
        "from": "2025-02-01T10:10:00Z",
        "to": "2025-02-01T10:30:00Z",
        "status": 1,
        "fkAvailabilityidAvailability": created["availability"]["idAvailability"],
        "fkListingidListing": created["listing"]["idListing"],
    }
    resp = client.post("Viewings", json=view_payload)
    results.append(expect_status("POST /Viewings -> 201", resp, [201]))
    if resp.status_code == 201:
        created["viewing"] = resp.json()

    # Buyer for user_buyer
    buyer_payload = {"confirmed": False, "blocked": False, "idUser": created["user_buyer"]["idUser"]}
    resp = client.post("Buyers", json=buyer_payload)
    results.append(expect_status("POST /Buyers -> 201", resp, [201]))
    if resp.status_code == 201:
        created["buyer"] = resp.json()

    # Administrator for user_admin
    admin_payload = {"idUser": created["user_admin"]["idUser"]}
    resp = client.post("Administrators", json=admin_payload)
    results.append(expect_status("POST /Administrators -> 201", resp, [201]))
    if resp.status_code == 201:
        created["admin"] = resp.json()

    # Session for user_broker
    session_payload = {
        "created": "2025-01-01 12:00",
        "remember": False,
        "lastactivity": "2025-01-01 12:30",
        "fkUseridUser": created["user_broker"]["idUser"],
    }
    resp = client.post("Sessions", json=session_payload)
    results.append(expect_status("POST /Sessions -> 201", resp, [201]))
    if resp.status_code == 201:
        created["session"] = resp.json()

    # Read back a few resources (GET by id)
    if "building" in created:
        bid = created["building"]["idBuilding"]
        results.append(expect_status("GET /Buildings/{id} -> 200", client.get(f"Buildings/{bid}"), [200]))
        # list apartments in building custom route
        results.append(expect_status("GET /Buildings/{id}/apartments -> 200", client.get(f"Buildings/{bid}/apartments"), [200]))
        # list pictures in building custom route
        results.append(expect_status("GET /Buildings/{id}/pictures -> 200", client.get(f"Buildings/{bid}/pictures"), [200]))
        # get a specific picture within building/apartment when available
        if "apartment" in created and "picture" in created:
            aid = created["apartment"]["idApartment"]
            pid = created["picture"]["id"]
            results.append(expect_status(
                "GET /Buildings/{id}/apartment/{apartmentId}/picture/{pictureId} -> 200",
                client.get(f"Buildings/{bid}/apartment/{aid}/picture/{pid}"),
                [200],
            ))

    if "listing" in created:
        lid = created["listing"]["idListing"]
        results.append(expect_status("GET /Listings/{id} -> 200", client.get(f"Listings/{lid}"), [200]))
        # broker-centric endpoints
        if "broker" in created:
            brid = created["broker"]["idUser"]
            results.append(expect_status("GET /Brokers/{id}/listings -> 200", client.get(f"Brokers/{brid}/listings"), [200]))
            results.append(expect_status("GET /Brokers/{id}/apartments -> 200", client.get(f"Brokers/{brid}/apartments"), [200]))
            # viewings for broker exist after viewing creation; we'll assert later if created

    # Update a few resources successfully (expect 204)
    try:
        if "building" in created:
            b = created["building"].copy()
            b["floors"] = b.get("floors", 1) + 1
            # Put requires body to include matching id
            results.append(expect_status("PUT /Buildings/{id} -> 204", client.put(f"Buildings/{b['idBuilding']}", json=b), [204]))
        if "apartment" in created:
            a = created["apartment"].copy()
            a["rooms"] = a.get("rooms", 1) + 1
            results.append(expect_status("PUT /Apartments/{id} -> 204", client.put(f"Apartments/{a['idApartment']}", json=a), [204]))
        if "picture" in created:
            p = created["picture"].copy()
            p["public"] = not p.get("public", False)
            results.append(expect_status("PUT /Pictures/{id} -> 204", client.put(f"Pictures/{p['id']}", json=p), [204]))
        if "listing" in created:
            l = created["listing"].copy()
            l["askingprice"] = l.get("askingprice", 0) + 1
            results.append(expect_status("PUT /Listings/{id} -> 204", client.put(f"Listings/{l['idListing']}", json=l), [204]))
        if "broker" in created:
            br = created["broker"].copy()
            br["blocked"] = True
            results.append(expect_status("PUT /Brokers/{id} -> 204", client.put(f"Brokers/{br['idUser']}", json=br), [204]))
        if "buyer" in created:
            by = created["buyer"].copy()
            by["blocked"] = True
            results.append(expect_status("PUT /Buyers/{id} -> 204", client.put(f"Buyers/{by['idUser']}", json=by), [204]))
        if "admin" in created:
            ad = created["admin"].copy()
            results.append(expect_status("PUT /Administrators/{id} -> 204", client.put(f"Administrators/{ad['idUser']}", json=ad), [204]))
    except Exception as e:
        results.append(TestResult(name="PUT operations block", ok=False, status=0, details=str(e)))

    # New custom GET endpoints dependent on resources created
    if "apartment" in created:
        aid = created["apartment"]["idApartment"]
        results.append(expect_status("GET /Apartments/{id}/listing -> 200", client.get(f"Apartments/{aid}/listing"), [200]))

    # 3.5) Cascade delete scenario (separate chain)
    # Build a separate resource chain under a different broker, then delete the broker and ensure cascading deletes
    cascade: Dict[str, Any] = {}
    # user for cascade broker
    resp = client.post("Users", json={
        "name": "Cascade",
        "surname": "Tester",
        "email": f"cascade-{uuid.uuid4().hex[:8]}@example.com",
        "phone": "+37060000000",
        "password": "password123",
        "registrationtime": "2025-01-01 12:00",
        "profilepicture": None,
    })
    results.append(expect_status("POST /Users (cascade) -> 201", resp, [201]))
    if resp.status_code == 201:
        cascade["user"] = resp.json()

    # broker (cascade)
    if "user" in cascade:
        resp = client.post("Brokers", json={"confirmed": False, "blocked": False, "idUser": cascade["user"]["idUser"]})
        results.append(expect_status("POST /Brokers (cascade) -> 201", resp, [201]))
        if resp.status_code == 201:
            cascade["broker"] = resp.json()

    # building (cascade)
    if "broker" in cascade:
        resp = client.post("Buildings", json={
            "city": "Vilnius",
            "address": "Cascade 1",
            "area": 10.0,
            "year": 2024,
            "lastrenovationyear": None,
            "floors": 1,
            "energy": 1,
            "fkBrokeridUser": cascade["broker"]["idUser"],
        })
        results.append(expect_status("POST /Buildings (cascade) -> 201", resp, [201]))
        if resp.status_code == 201:
            cascade["building"] = resp.json()

    # apartment (cascade)
    if "building" in cascade:
        resp = client.post("Apartments", json={
            "apartmentnumber": 1,
            "area": 20.0,
            "floor": 1,
            "rooms": 1,
            "notes": "cascade",
            "heating": None,
            "finish": 1,
            "fkBuildingidBuilding": cascade["building"]["idBuilding"],
            "iswholebuilding": False,
        })
        results.append(expect_status("POST /Apartments (cascade) -> 201", resp, [201]))
        if resp.status_code == 201:
            cascade["apartment"] = resp.json()

    # picture (cascade)
    if "apartment" in cascade:
        pic_id2 = f"pic-{uuid.uuid4().hex}"
        resp = client.post("Pictures", json={"id": pic_id2, "public": False, "fkApartmentidApartment": cascade["apartment"]["idApartment"]})
        results.append(expect_status("POST /Pictures (cascade) -> 201", resp, [201]))
        if resp.status_code == 201:
            cascade["picture"] = resp.json()

    # listing (cascade)
    if "picture" in cascade:
        resp = client.post("Listings", json={
            "description": "cascade listing",
            "askingprice": 1.0,
            "rent": False,
            "fkPictureid": cascade["picture"]["id"],
        })
        results.append(expect_status("POST /Listings (cascade) -> 201", resp, [201]))
        if resp.status_code == 201:
            cascade["listing"] = resp.json()

    # availability (cascade)
    if "broker" in cascade:
        resp = client.post("Availabilities", json={
            "from": "2025-02-01 10:00",
            "to": "2025-02-01 11:00",
            "fkBrokeridUser": cascade["broker"]["idUser"],
        })
        results.append(expect_status("POST /Availabilities (cascade) -> 201", resp, [201]))
        if resp.status_code == 201:
            cascade["availability"] = resp.json()

    # viewing (cascade)
    if "availability" in cascade and "listing" in cascade:
        resp = client.post("Viewings", json={
            "from": "2025-02-01T10:10:00Z",
            "to": "2025-02-01T10:30:00Z",
            "status": 1,
            "fkAvailabilityidAvailability": cascade["availability"]["idAvailability"],
            "fkListingidListing": cascade["listing"]["idListing"],
        })
        results.append(expect_status("POST /Viewings (cascade) -> 201", resp, [201]))
        if resp.status_code == 201:
            cascade["viewing"] = resp.json()

    # Now delete the broker and ensure cascading deletes
    if "broker" in cascade:
        results.append(expect_status("DELETE /Brokers/{id} (cascade broker) -> 204", client.delete(f"Brokers/{cascade['broker']['idUser']}"), [204]))
        # Broker itself
        results.append(expect_status("GET /Brokers/{id} (after cascade) -> 404", client.get(f"Brokers/{cascade['broker']['idUser']}"), [404]))
        # Downstream resources should be gone
        if "building" in cascade:
            results.append(expect_status("GET /Buildings/{id} (after cascade) -> 404", client.get(f"Buildings/{cascade['building']['idBuilding']}"), [404]))
        if "apartment" in cascade:
            results.append(expect_status("GET /Apartments/{id} (after cascade) -> 404", client.get(f"Apartments/{cascade['apartment']['idApartment']}"), [404]))
        if "picture" in cascade:
            results.append(expect_status("GET /Pictures/{id} (after cascade) -> 404", client.get(f"Pictures/{cascade['picture']['id']}"), [404]))
        if "listing" in cascade:
            results.append(expect_status("GET /Listings/{id} (after cascade) -> 404", client.get(f"Listings/{cascade['listing']['idListing']}"), [404]))
        if "availability" in cascade:
            results.append(expect_status("GET /Availabilities/{id} (after cascade) -> 404", client.get(f"Availabilities/{cascade['availability']['idAvailability']}"), [404]))
        if "viewing" in cascade:
            results.append(expect_status("GET /Viewings/{id} (after cascade) -> 404", client.get(f"Viewings/{cascade['viewing']['idViewing']}"), [404]))
        # User should still exist (deleting broker doesn't delete the user)
        if "user" in cascade:
            results.append(expect_status("GET /Users/{id} (user survives broker delete) -> 200", client.get(f"Users/{cascade['user']['idUser']}"), [200]))

    # PATCH operations (expect 204)
    try:
        if "picture" in created:
            pic = created["picture"]
            results.append(expect_status("PATCH /Pictures/{id} public -> 204", client.patch(f"Pictures/{pic['id']}", json={"public": True}), [204]))
        if "broker" in created:
            brid = created["broker"]["idUser"]
            results.append(expect_status("PATCH /Brokers/{id} blocked -> 204", client.patch(f"Brokers/{brid}", json={"blocked": True}), [204]))
        if "buyer" in created:
            byid = created["buyer"]["idUser"]
            results.append(expect_status("PATCH /Buyers/{id} confirmed -> 204", client.patch(f"Buyers/{byid}", json={"confirmed": True}), [204]))
        if "viewing" in created:
            vid = created["viewing"]["idViewing"]
            results.append(expect_status("PATCH /Viewings/{id} status -> 204", client.patch(f"Viewings/{vid}", json={"status": 1}), [204]))
    except Exception as e:
        results.append(TestResult(name="PATCH operations block", ok=False, status=0, details=str(e)))

    # Complete broker-centric GET after all related resources created
    if "broker" in created:
        brid = created["broker"]["idUser"]
        results.append(expect_status("GET /Brokers/{id}/apartments -> 200", client.get(f"Brokers/{brid}/apartments"), [200]))
        results.append(expect_status("GET /Brokers/{id}/listings -> 200", client.get(f"Brokers/{brid}/listings"), [200]))
        results.append(expect_status("GET /Brokers/{id}/viewings -> 200", client.get(f"Brokers/{brid}/viewings"), [200]))
        # nested broker+building endpoints
        if "building" in created:
            bid = created["building"]["idBuilding"]
            results.append(expect_status(
                "GET /Brokers/{id}/building/{buildingId}/apartments -> 200",
                client.get(f"Brokers/{brid}/building/{bid}/apartments"),
                [200],
            ))
            if "apartment" in created:
                aid = created["apartment"]["idApartment"]
                results.append(expect_status(
                    "GET /Brokers/{id}/building/{buildingId}/apartment/{apartmentId}/listings -> 200",
                    client.get(f"Brokers/{brid}/building/{bid}/apartment/{aid}/listings"),
                    [200],
                ))
                if "picture" in created:
                    pid = created["picture"]["id"]
                    results.append(expect_status(
                        "GET /Brokers/{id}/building/{buildingId}/apartment/{apartmentId}/picture/{pictureId}/listing -> 200",
                        client.get(f"Brokers/{brid}/building/{bid}/apartment/{aid}/picture/{pid}/listing"),
                        [200],
                    ))
        # nested broker+availability endpoint
        if "availability" in created:
            avid = created["availability"]["idAvailability"]
            results.append(expect_status(
                "GET /Brokers/{id}/availability/{availabilityId}/viewings -> 200",
                client.get(f"Brokers/{brid}/availability/{avid}/viewings"),
                [200],
            ))

    # DTO-based PUTs now succeed (expect 204)
    if "user_broker" in created:
        uid = created["user_broker"]["idUser"]
        bad_put_user = {
            "name": created["user_broker"]["name"],
            "surname": created["user_broker"]["surname"],
            "email": created["user_broker"]["email"],
            "phone": created["user_broker"]["phone"],
            "password": created["user_broker"]["password"],
            "registrationtime": "2025-01-02 10:00",
            "profilepicture": created["user_broker"].get("profilepicture"),
        }
        results.append(expect_status("PUT /Users/{id} DTO -> 204", client.put(f"Users/{uid}", json=bad_put_user), [204]))

    if "availability" in created:
        aid = created["availability"]["idAvailability"]
        bad_put_av = {"from": "2025-02-01 09:00", "to": "2025-02-01 10:00", "fkBrokeridUser": created["broker"]["idUser"]}
        results.append(expect_status("PUT /Availabilities/{id} DTO -> 204", client.put(f"Availabilities/{aid}", json=bad_put_av), [204]))

    if "session" in created:
        sid = created["session"]["id"]
        bad_put_session = {"created": "2025-01-01 12:00", "remember": True, "lastactivity": "2025-01-01 12:40", "fkUseridUser": created["user_broker"]["idUser"]}
        results.append(expect_status("PUT /Sessions/{id} DTO -> 204", client.put(f"Sessions/{sid}", json=bad_put_session), [204]))

    if "viewing" in created:
        vid = created["viewing"]["idViewing"]
        bad_put_view = {"from": "2025-02-01 10:20", "to": "2025-02-01 10:40", "status": 1, "fkAvailabilityidAvailability": created["availability"]["idAvailability"], "fkListingidListing": created["listing"]["idListing"]}
        results.append(expect_status("PUT /Viewings/{id} DTO -> 204", client.put(f"Viewings/{vid}", json=bad_put_view), [204]))

    # Delete chain (expect 204)
    def delete_expect_204(name: str, path: str):
        results.append(expect_status(name, client.delete(path), [204]))

    if "viewing" in created:
        delete_expect_204("DELETE /Viewings/{id} -> 204", f"Viewings/{created['viewing']['idViewing']}")
    if "listing" in created:
        delete_expect_204("DELETE /Listings/{id} -> 204", f"Listings/{created['listing']['idListing']}")
    if "picture" in created:
        delete_expect_204("DELETE /Pictures/{id} -> 204", f"Pictures/{created['picture']['id']}")
    if "apartment" in created:
        delete_expect_204("DELETE /Apartments/{id} -> 204", f"Apartments/{created['apartment']['idApartment']}")
    if "availability" in created:
        delete_expect_204("DELETE /Availabilities/{id} -> 204", f"Availabilities/{created['availability']['idAvailability']}")
    if "building" in created:
        delete_expect_204("DELETE /Buildings/{id} -> 204", f"Buildings/{created['building']['idBuilding']}")
    if "broker" in created:
        delete_expect_204("DELETE /Brokers/{id} -> 204", f"Brokers/{created['broker']['idUser']}")
    if "buyer" in created:
        delete_expect_204("DELETE /Buyers/{id} -> 204", f"Buyers/{created['buyer']['idUser']}")
    if "admin" in created:
        delete_expect_204("DELETE /Administrators/{id} -> 204", f"Administrators/{created['admin']['idUser']}")
    if "session" in created:
        delete_expect_204("DELETE /Sessions/{id} -> 204", f"Sessions/{created['session']['id']}")

    # Finally, delete the users
    for key in ("user_broker", "user_buyer", "user_admin"):
        if key in created:
            delete_expect_204(f"DELETE /Users/{{id}} ({key}) -> 204", f"Users/{created[key]['idUser']}")

    # Summary
    print("\n==== API Test Summary ====\n")
    failed = 0
    for r in results:
        status = "PASS" if r.ok else "FAIL"
        print(f"[{status}] {r.name} -> {r.status}" + (f" | {r.details}" if r.details else ""))
        if not r.ok:
            failed += 1

    # Ensure required global checks are satisfied at least once
    # - 404 on not found
    has_404 = any(r.ok and r.status == 404 for r in results)
    # - bad payload 400 or 422
    has_4xx_bad = any(r.ok and r.status in (400, 422) and ("bad" in r.name.lower() or "DTO" in r.name) for r in results)
    # - created returns 201
    has_201 = any(r.ok and r.status == 201 for r in results)
    # - delete returns 200 or 204
    has_delete_ok = any(r.ok and r.status in (200, 204) and r.name.startswith("DELETE") for r in results)

    summary_checks = [
        ("Has 404 case", has_404),
        ("Has bad payload 400/422 case", has_4xx_bad),
        ("Has 201 on create", has_201),
        ("Has 200/204 on delete", has_delete_ok),
    ]
    print("\nRequired checks:")
    for label, ok in summary_checks:
        print(f" - {label}: {'OK' if ok else 'MISSING'}")
        if not ok:
            failed += 1

    print("\nBase URL:", BASE_URL)
    return 1 if failed else 0


if __name__ == "__main__":
    sys.exit(main())
